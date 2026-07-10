using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Gestión de misiones activas: progreso, entregas, retrasos, recompensas.</summary>
    public class MissionSystem : MonoBehaviour
    {
        public static MissionSystem I { get; private set; }

        public List<Mission> Active = new List<Mission>();
        public Mission CarryingPickup;   // recolección en curso
        readonly List<Mission> delegated = new List<Mission>(); // mandos medios (GDD 6.3)

        void Awake() { I = this; }
        void OnEnable() { GameEvents.OnDayStart += OnDayStart; }
        void OnDisable() { GameEvents.OnDayStart -= OnDayStart; }

        public void Add(Mission m)
        {
            Active.Add(m);
            GameEvents.OnMissionAdded?.Invoke(m);
        }

        /// <summary>Delegar a mandos medios: se completa al día siguiente con 60% de la paga, sin Calor.</summary>
        public void Delegate(Mission m)
        {
            m.Pay = Mathf.RoundToInt(m.Pay * 0.6f);
            delegated.Add(m);
            GameEvents.Notify("Encargo delegado. Mañana recibirás tu corte: $" + m.Pay.ToString("N0"));
        }

        void OnDayStart(int day)
        {
            if (!Net.IsAuthority) return; // el host decide pagos y vencimientos
            foreach (var d in delegated)
            {
                GameManager.I.AddMoney(d.Pay, true);
                GameManager.I.MissionsCompleted++;
                GameEvents.Notify("Tu gente entregó \"" + d.Title + "\": +$" + d.Pay.ToString("N0") + " (sucio)");
            }
            delegated.Clear();

            foreach (var m in Active.ToList())
            {
                if (m.Type == MissionType.ClienteHonesto || m.DeadlineDay <= 0) continue;
                if (day > m.DeadlineDay && m.State != MissionState.Completada)
                {
                    int diasTarde = day - m.DeadlineDay;
                    // dueño original sospecha por el retraso (GDD 4.4.1)
                    if (m.OwnerNpcIndex >= 0 && NPCManager.I != null && m.OwnerNpcIndex < NPCManager.I.NPCs.Count)
                        NPCManager.I.NPCs[m.OwnerNpcIndex].Suspicion.Add(Random.Range(8f, 15f));

                    if (diasTarde == 1)
                        MetasManager.I.CambiarFabio(-15f, "Encargo atrasado: \"" + m.Title + "\"");
                    else if (diasTarde >= 3)
                    {
                        m.State = MissionState.Fallida;
                        Active.Remove(m);
                        MetasManager.I.CambiarFabio(-20f, "Encargo perdido: \"" + m.Title + "\"");
                        GameEvents.OnMissionFailed?.Invoke(m);
                    }
                }
            }
        }

        /// <summary>Trabajo desde una estación. Devuelve true si la misión terminó.
        /// En multijugador, el cliente reporta su avance al host y la completa el host.</summary>
        public bool DoWork(Mission m, float dt)
        {
            if (m == null || m.State == MissionState.Completada) return false;
            m.State = MissionState.EnProgreso;
            float mult = UpgradeSystem.I != null ? UpgradeSystem.I.RepairSpeedMultiplier : 1f;
            if (Net.IsClientOnly)
            {
                // avance visual local; la barra real y la completada llegan del host
                m.WorkDone = Mathf.Min(m.WorkDone + dt * mult, m.WorkRequired * 0.999f);
                GameSync.SendWork(m.Id, dt * mult);
                return false;
            }
            m.WorkDone += dt * mult;
            if (m.EsIlegal && FenceSpySystem.I != null)
                FenceSpySystem.I.RegisterNoise(m.Noise);
            if (m.WorkDone >= m.WorkRequired)
            {
                Complete(m);
                return true;
            }
            return false;
        }

        /// <summary>Avance reportado por un cliente remoto (solo host).</summary>
        public void HostWork(int id, float amount)
        {
            var m = FindById(id);
            if (m == null || m.State == MissionState.Completada) return;
            m.State = MissionState.EnProgreso;
            m.WorkDone += amount;
            if (m.EsIlegal && FenceSpySystem.I != null)
                FenceSpySystem.I.RegisterNoise(m.Noise);
            if (m.WorkDone >= m.WorkRequired) Complete(m);
        }

        public Mission FindById(int id)
        {
            foreach (var m in Active) if (m.Id == id) return m;
            if (ReceptionSystem.I != null)
                foreach (var w in ReceptionSystem.I.Queue) if (w.M.Id == id) return w.M;
            if (DirtyReceptionSystem.I != null)
                foreach (var w in DirtyReceptionSystem.I.Queue) if (w.M.Id == id) return w.M;
            return null;
        }

        /// <summary>Espejo de red en el cliente: la completó el host (dinero/metas llegan por el sync).</summary>
        public void MirrorComplete(int id)
        {
            var m = FindById(id);
            if (m == null || m.State == MissionState.Completada) return;
            m.State = MissionState.Completada;
            m.WorkDone = m.WorkRequired;
            Active.Remove(m);
            GameEvents.Notify("Completado: \"" + m.Title + "\" +$" + m.Pay.ToString("N0") +
                (m.PayIsDirty ? " (sucio)" : ""));
            GameEvents.OnMissionCompleted?.Invoke(m);
        }

        public void Complete(Mission m)
        {
            if (m.State == MissionState.Completada) return;
            m.State = MissionState.Completada;
            Active.Remove(m);
            GameManager.I.MissionsCompleted++;
            GameManager.I.AddMoney(m.Pay, m.PayIsDirty);

            switch (m.Type)
            {
                case MissionType.ClienteHonesto:
                    float rep = m.RepBonus > 0 ? m.RepBonus : Random.Range(1f, 3f);
                    MetasManager.I.CambiarReputacion(rep, m.ClientName + " quedó satisfecho");
                    break;
                default:
                    float fb = m.FabioBonus > 0 ? m.FabioBonus : Random.Range(5f, 10f);
                    MetasManager.I.CambiarFabio(fb, "Encargo entregado a tiempo");
                    // el desarme deja piezas ilegales en el almacén (se venden por teléfono)
                    if (m.Title.Contains("Desarme") || m.Type == MissionType.Especial)
                    {
                        int piezas = Random.Range(2, 5);
                        if (InventorySystem.I != null && InventorySystem.I.Add(ItemType.PiezaIlegal, piezas))
                            GameEvents.Notify("Guardaste " + piezas + " piezas ilegales. Véndelas llamando por teléfono.");
                    }
                    if (m.OfreceLeverage)
                    {
                        ModalUI.Show("Encargo de alto nivel",
                            "Entre los papeles del encargo hay documentos comprometedores.\n" +
                            "¿Te quedas una copia? Fabio no tiene por qué enterarse... (Leverage " +
                            MetasManager.I.Leverage + "/5)",
                            "Quedarme una copia", () => MetasManager.I.AgregarLeverage(),
                            "Entregar todo", null);
                    }
                    // a veces el encargo deja material para fabricar evidencia falsa
                    else if (Random.value < 0.25f)
                    {
                        MetasManager.I.EvidenciaFalsa++;
                        GameEvents.Notify("Guardaste material que serviría como evidencia falsa (" +
                            MetasManager.I.EvidenciaFalsa + ").");
                    }
                    break;
            }

            GameEvents.Notify("Completado: \"" + m.Title + "\" +$" + m.Pay.ToString("N0") +
                (m.PayIsDirty ? " (sucio)" : ""));
            GameEvents.OnMissionCompleted?.Invoke(m);
            GameSync.MirrorMissionComplete(m.Id);
        }

        public void Reject(Mission m)
        {
            Active.Remove(m);
            m.State = MissionState.Fallida;
            GameEvents.OnMissionFailed?.Invoke(m);
        }

        public Mission FirstFor(MissionType type) =>
            Active.FirstOrDefault(m => m.Type == type && m.State != MissionState.Completada);
    }
}
