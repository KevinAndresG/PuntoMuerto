using UnityEngine;

namespace PuntoMuerto
{
    public enum SuspicionState { Normal, Alerta, SospechaAlta, Investigando }

    /// <summary>Máquina de estados de sospecha individual por NPC (GDD 4.4).</summary>
    public class NPCSuspicion : MonoBehaviour
    {
        [Range(0f, 100f)] public float Sospecha;
        public bool FueSobornado;          // piso 15-20, nunca vuelve a NORMAL limpio
        public bool EscaloAAutoridad;
        public int DiasInvestigando;

        NPCController npc;

        public SuspicionState CurrentState
        {
            get
            {
                if (Sospecha < 25f) return SuspicionState.Normal;
                if (Sospecha < 50f) return SuspicionState.Alerta;
                if (Sospecha < 75f) return SuspicionState.SospechaAlta;
                return SuspicionState.Investigando;
            }
        }

        public string StateLabel
        {
            get
            {
                switch (CurrentState)
                {
                    case SuspicionState.Alerta: return "Alerta";
                    case SuspicionState.SospechaAlta: return "Sospecha alta";
                    case SuspicionState.Investigando: return "¡Investigando!";
                    default: return "Normal";
                }
            }
        }

        void Awake() { npc = GetComponent<NPCController>(); }
        void OnEnable() { GameEvents.OnDayStart += OnDayStart; }
        void OnDisable() { GameEvents.OnDayStart -= OnDayStart; }

        void OnDayStart(int day)
        {
            if (npc != null && npc.Activity == NPCActivity.Desaparecido) return;

            var st = CurrentState;
            // decaimiento pasivo solo en NORMAL/ALERTA (GDD 4.4.5)
            if (st == SuspicionState.Normal || st == SuspicionState.Alerta)
            {
                float floor = NPCManager.I != null ? NPCManager.I.GlobalSuspicionFloor : 0f;
                Sospecha = Mathf.Max(floor, Sospecha - Random.Range(1f, 2f));
            }
            else if (st == SuspicionState.Investigando && !EscaloAAutoridad)
            {
                DiasInvestigando++;
                float chance = 0.10f * DiasInvestigando; // +10% acumulativo diario (4.4.4)
                if (Random.value < chance)
                {
                    EscaloAAutoridad = true;
                    MetasManager.I.CambiarReputacion(Random.Range(-25f, -15f),
                        npc.NpcName + " fue a la policía con lo que sospecha. Un inspector vendrá con causa concreta");
                    Sospecha = 60f; // ya soltó lo que sabía; baja pero queda marcado
                    EscaloAAutoridad = false;
                    DiasInvestigando = 0;
                }
            }
        }

        public void Add(float amount)
        {
            Sospecha = Mathf.Clamp(Sospecha + amount, 0f, 100f);
            if (CurrentState == SuspicionState.Investigando)
                GameEvents.Notify("¡" + npc.NpcName + " está investigando activamente al taller!");
        }

        float FloorValue => FueSobornado ? Random.Range(15f, 20f) :
            (NPCManager.I != null ? NPCManager.I.GlobalSuspicionFloor : 0f);

        /// <summary>Despistar (4.4.2/4.4.3). Devuelve true si funcionó.</summary>
        public bool TryDespistar()
        {
            var st = CurrentState;
            float rate = st == SuspicionState.Alerta ? 0.82f :
                         st == SuspicionState.SospechaAlta ? 0.45f : 0f;
            if (st == SuspicionState.Normal) { return true; }
            if (st == SuspicionState.Investigando) rate = 0.1f;
            if (Random.value < rate)
            {
                Sospecha = FloorValue + Random.Range(0f, 5f);
                DiasInvestigando = 0;
                return true;
            }
            Sospecha = Mathf.Max(Sospecha, 52f); // quedaste mal parado con la excusa
            return false;
        }

        public int CostoSoborno
        {
            get
            {
                float baseCost = Random.Range(500, 2000);
                if (MetasManager.I.Reputacion < -20f) baseCost *= 1.5f;
                if (CurrentState == SuspicionState.Investigando) baseCost *= 2.5f;
                return Mathf.RoundToInt(baseCost / 50f) * 50;
            }
        }

        /// <summary>Sobornar. Devuelve true si aceptó callarse.</summary>
        public bool TrySobornar(int costoPagado)
        {
            float rate = CurrentState == SuspicionState.Investigando ? 0.5f : 0.85f;
            MetasManager.I.CambiarReputacion(-1f);
            LedgerSystem.I.RiesgoAuditoria += 3f; // rastro en el libro
            if (Random.value < rate)
            {
                FueSobornado = true;
                Sospecha = Random.Range(15f, 20f);
                DiasInvestigando = 0;
                return true;
            }
            Add(8f);
            return false;
        }

        /// <summary>Incriminar con evidencia falsa: este NPC a Normal, otro random +20.</summary>
        public bool TryIncriminar()
        {
            if (MetasManager.I.EvidenciaFalsa <= 0) return false;
            MetasManager.I.EvidenciaFalsa--;
            Sospecha = 0f;
            DiasInvestigando = 0;
            var otro = NPCManager.I.RandomNPC(npc);
            if (otro != null)
            {
                otro.Suspicion.Add(20f);
                GameEvents.Notify("La sospecha ahora apunta hacia " + otro.NpcName + "...");
            }
            return true;
        }

        public void ReportarAFabio()
        {
            NPCManager.I.ScheduleDisappearance(npc);
            MetasManager.I.RegistrarReporteAFabio();
        }
    }
}
