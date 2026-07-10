using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>
    /// Negocio sucio por la ventanilla trasera: clientes turbios entran (en carro o a pie) por la
    /// rendija sur del patio, hacen fila en el cuarto de la ventanilla y, si aceptas, su carro pasa
    /// a un slot del patio y el cliente espera en la banca hasta que le entregues.
    /// En multijugador el host es la autoridad; los clientes ven un espejo sincronizado.
    /// </summary>
    public class DirtyReceptionSystem : MonoBehaviour
    {
        public static DirtyReceptionSystem I { get; private set; }

        public class WaitingClient
        {
            public Mission M;
            public float ArriveHour;
            public GameObject WaitCar;
            public Color CarColor;
            public ClientDummy Dummy;
        }

        class PendingArrival
        {
            public Mission M;
            public GameObject Car;
            public Color CarColor;
            public ClientDummy Dummy;
        }

        public List<WaitingClient> Queue = new List<WaitingClient>();
        public int MaxQueue = 3;
        public float TimeoutHours = 2.5f;

        readonly List<PendingArrival> pending = new List<PendingArrival>();
        readonly List<(Mission m, ClientDummy d)> waitingForJob = new List<(Mission, ClientDummy)>();

        // salida por la rendija sur de la cerca
        static readonly Vector3 BackExit = new Vector3(39f, 0f, -47f);
        static readonly Vector3 FootGate = new Vector3(39f, 0f, -45.5f);
        // banca de espera junto al cuarto de la ventanilla
        static readonly Vector3[] WaitSpots =
        {
            new Vector3(34.8f, 0f, -22.6f), new Vector3(36.2f, 0f, -22.6f), new Vector3(37.6f, 0f, -22.6f)
        };

        public bool CanReceive => Queue.Count + pending.Count < MaxQueue;

        void Awake() { I = this; }

        void OnEnable() { GameEvents.OnMissionFailed += OnMissionFailed; GameEvents.OnDayStart += OnDayStart; }
        void OnDisable() { GameEvents.OnMissionFailed -= OnMissionFailed; GameEvents.OnDayStart -= OnDayStart; }

        // fila dentro del cuarto: desde la ventanilla hacia la puerta sur
        Vector3 QueueSpot(int i) => new Vector3(30f, 0f, -22.3f - i * 1.15f);
        Vector3 ParkSpot(int i) => new Vector3(36f + i * 4f, 0f, -40.5f);

        public void BeginArrival(Mission m, Color carColor, bool fromNet = false)
        {
            if (!CanReceive) return;
            if (!fromNet) GameSync.MirrorArrival(1, m, carColor);
            var pa = new PendingArrival { M = m, CarColor = carColor };
            if (m.ByCar && TrafficManager.I != null)
            {
                pa.Car = TrafficManager.I.SpawnDirtyClientCar(carColor, ParkSpot(Queue.Count + pending.Count));
            }
            else
            {
                // vendedor a pie: entra caminando por la rendija sur
                pa.Dummy = ClientDummy.Spawn(m.ClientName, carColor, FootGate,
                    QueueSpot(Queue.Count + pending.Count));
            }
            pending.Add(pa);
        }

        void Update()
        {
            if (DayNightCycle.I == null) return;
            float hour = DayNightCycle.I.Hour;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var pa = pending[i];
                if (pa.Dummy == null)
                {
                    if (pa.Car == null) { pending.RemoveAt(i); continue; }
                    var ai = pa.Car.GetComponent<VehicleAI>();
                    if (ai == null || ai.Finished)
                    {
                        var doorPos = pa.Car.transform.position - pa.Car.transform.right * 1.4f;
                        pa.Dummy = ClientDummy.Spawn(pa.M.ClientName, pa.CarColor, doorPos,
                            QueueSpot(Queue.Count + pending.Count - 1));
                    }
                }
                else if (pa.Dummy.AtDestination)
                {
                    pending.RemoveAt(i);
                    Queue.Add(new WaitingClient
                    {
                        M = pa.M,
                        ArriveHour = hour,
                        WaitCar = pa.Car,
                        CarColor = pa.CarColor,
                        Dummy = pa.Dummy
                    });
                    pa.M.State = MissionState.EnRecepcion;
                    GameEvents.Notify(pa.M.EsVenta
                        ? "Alguien ofrece mercancía en la ventanilla trasera: " + pa.M.Title
                        : "Alguien espera en la ventanilla trasera: " + pa.M.Title +
                          " ($" + pa.M.Pay.ToString("N0") + " sucio)");
                    Reflow();
                }
            }

            // timeouts: solo la autoridad
            if (Net.IsAuthority)
            {
                for (int i = Queue.Count - 1; i >= 0; i--)
                {
                    if (hour - Queue[i].ArriveHour > TimeoutHours && hour >= Queue[i].ArriveHour)
                    {
                        var w = Queue[i];
                        GameSync.MirrorDismiss(1, w.M.Id);
                        Dismiss(w);
                        if (!w.M.EsVenta)
                            MetasManager.I.CambiarFabio(-3f, "Dejaste plantado un encargo en la ventanilla");
                    }
                }
            }
        }

        public void Accept(WaitingClient w)
        {
            if (Net.IsClientOnly)
            {
                if (!w.M.EsVenta && BayManager.I.FreePatioSlotIndex() < 0)
                {
                    GameEvents.Notify("No hay espacio en el patio. Termina un encargo primero.");
                    return;
                }
                GameSync.RequestAccept(1, w.M.Id);
                return;
            }
            if (DoAccept(w)) GameSync.MirrorAccept(1, w.M.Id);
        }

        public bool AcceptById(int id)
        {
            var w = Queue.FirstOrDefault(x => x.M.Id == id);
            if (w == null) return false;
            bool ok = DoAccept(w);
            if (ok && Net.IsAuthority) GameSync.MirrorAccept(1, id);
            return ok;
        }

        bool DoAccept(WaitingClient w)
        {
            if (w.M.EsVenta)
            {
                if (Net.IsAuthority)
                {
                    if (InventorySystem.I.Used + w.M.VentaCount > InventorySystem.I.Capacity)
                    { GameEvents.Notify("No cabe en el almacén."); return false; }
                    if (!GameManager.I.Spend(w.M.Pay, preferDirty: true))
                    { GameEvents.Notify("No te alcanza: $" + w.M.Pay.ToString("N0")); return false; }
                    InventorySystem.I.Add(w.M.VentaItem, w.M.VentaCount);
                }
                Queue.Remove(w);
                if (w.Dummy != null) w.Dummy.Leave(BackExit);
                GameEvents.Notify("Compraste el lote: " + InventorySystem.Label(w.M.VentaItem) + " x" +
                    w.M.VentaCount + " por $" + w.M.Pay.ToString("N0") + ". Véndelas por teléfono.");
                Reflow();
                return true;
            }

            if (BayManager.I.FreePatioSlotIndex() < 0)
            {
                GameEvents.Notify("No hay espacio en el patio. Termina un encargo primero.");
                return false;
            }
            Queue.Remove(w);
            if (w.WaitCar != null) Destroy(w.WaitCar);
            MissionSystem.I.Add(w.M);
            BayManager.I.OccupyPatio(w.M, w.CarColor);
            // el cliente turbio espera en la banca junto a la ventanilla
            if (w.Dummy != null)
            {
                w.Dummy.MoveTo(WaitSpots[waitingForJob.Count % WaitSpots.Length]);
                waitingForJob.Add((w.M, w.Dummy));
            }
            GameEvents.Notify("Trato hecho. El carro está en el patio: \"" + w.M.Title + "\". El tipo espera en la banca.");
            Reflow();
            return true;
        }

        public void Reject(WaitingClient w)
        {
            if (Net.IsClientOnly) { GameSync.RequestReject(1, w.M.Id); return; }
            Dismiss(w);
            GameSync.MirrorDismiss(1, w.M.Id);
            GameEvents.Notify("El tipo se fue sin decir palabra. Mejor no hacer enemigos.");
        }

        public void RejectById(int id)
        {
            var w = Queue.FirstOrDefault(x => x.M.Id == id);
            if (w == null) return;
            Dismiss(w);
            if (Net.IsAuthority) GameSync.MirrorDismiss(1, id);
            GameEvents.Notify("El tipo se fue sin decir palabra. Mejor no hacer enemigos.");
        }

        /// <summary>Espejo de red: quitar de la fila sin penalizaciones locales.</summary>
        public void DismissById(int id)
        {
            var w = Queue.FirstOrDefault(x => x.M.Id == id);
            if (w != null) Dismiss(w);
        }

        void Dismiss(WaitingClient w)
        {
            Queue.Remove(w);
            if (w.Dummy != null) w.Dummy.Leave(BackExit);
            if (w.WaitCar != null)
            {
                if (TrafficManager.I != null) TrafficManager.I.DriveOffBack(w.WaitCar);
                else Destroy(w.WaitCar);
            }
            Reflow();
        }

        void Reflow()
        {
            for (int i = 0; i < Queue.Count; i++)
                if (Queue[i].Dummy != null) Queue[i].Dummy.MoveTo(QueueSpot(i));
        }

        /// <summary>Saca al cliente que esperaba esta misión (camina a su carro en la entrega).</summary>
        public ClientDummy TakeWaiting(Mission m)
        {
            for (int i = 0; i < waitingForJob.Count; i++)
            {
                if (waitingForJob[i].m != m) continue;
                var d = waitingForJob[i].d;
                waitingForJob.RemoveAt(i);
                return d;
            }
            return null;
        }

        void OnMissionFailed(Mission m)
        {
            for (int i = waitingForJob.Count - 1; i >= 0; i--)
            {
                if (waitingForJob[i].m != m) continue;
                var d = waitingForJob[i].d;
                waitingForJob.RemoveAt(i);
                if (d != null) d.Leave(BackExit);
            }
        }

        void OnDayStart(int day)
        {
            foreach (var (_, d) in waitingForJob)
                if (d != null) d.Leave(BackExit);
            waitingForJob.Clear();
        }
    }
}
