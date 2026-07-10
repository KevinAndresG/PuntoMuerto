using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace PuntoMuerto
{
    /// <summary>
    /// Recepción del frente. Secuencia: el carro llega y estaciona → el cliente se baja →
    /// camina a la fila → recién ahí aparece la tarea. Al aceptar, el cliente espera en la sala
    /// de espera interior hasta que le entregues su carro (sube y se van juntos).
    /// En multijugador el host es la autoridad; los clientes ven un espejo sincronizado.
    /// </summary>
    public class ReceptionSystem : MonoBehaviour
    {
        public static ReceptionSystem I { get; private set; }

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
        public int MaxQueue = 4;
        public float TimeoutHours = 1.75f;

        readonly List<PendingArrival> pending = new List<PendingArrival>();
        // clientes aceptados esperando su carro en la sala de espera
        readonly List<(Mission m, ClientDummy d)> waitingForJob = new List<(Mission, ClientDummy)>();

        static readonly Vector3 WalkInPoint = new Vector3(30f, 0f, -6f);
        // sala de espera interior (ala oeste del garaje)
        static readonly Vector3[] WaitSpots =
        {
            new Vector3(18.6f, 0f, -13.2f), new Vector3(18.6f, 0f, -17.8f), new Vector3(19.8f, 0f, -15.5f)
        };

        public bool CanReceive => Queue.Count + pending.Count < MaxQueue;

        void Awake() { I = this; }

        void OnEnable()
        {
            GameEvents.OnMissionCompleted += OnMissionCompleted;
            GameEvents.OnMissionFailed += OnMissionFailed;
            GameEvents.OnDayStart += OnDayStart;
        }
        void OnDisable()
        {
            GameEvents.OnMissionCompleted -= OnMissionCompleted;
            GameEvents.OnMissionFailed -= OnMissionFailed;
            GameEvents.OnDayStart -= OnDayStart;
        }

        Vector3 QueueSpot(int i) => new Vector3(44f - i * 1.1f, 0f, -8.6f + i * 1.7f);

        /// <summary>El cliente parte hacia el taller (en carro o a pie). La tarea aparece cuando llega a la fila.</summary>
        public void BeginArrival(Mission m, Color carColor, bool fromNet = false)
        {
            if (!CanReceive) return;
            if (!fromNet) GameSync.MirrorArrival(0, m, carColor);
            var pa = new PendingArrival { M = m, CarColor = carColor };
            if (m.ByCar && TrafficManager.I != null)
            {
                int idx = Queue.Count + pending.Count;
                pa.Car = TrafficManager.I.SpawnClientCar(carColor, new Vector3(28f + idx * 6f, 0f, -5f));
            }
            else
            {
                pa.Dummy = ClientDummy.Spawn(m.ClientName, carColor, WalkInPoint,
                    QueueSpot(Queue.Count + pending.Count));
            }
            pending.Add(pa);
        }

        void Update()
        {
            if (DayNightCycle.I == null) return;
            float hour = DayNightCycle.I.Hour;

            // llegadas en curso
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var pa = pending[i];
                if (pa.Dummy == null)
                {
                    if (pa.Car == null) { pending.RemoveAt(i); continue; } // carro destruido, cancelar
                    var ai = pa.Car.GetComponent<VehicleAI>();
                    if (ai == null || ai.Finished)
                    {
                        // el carro estacionó: el cliente se baja por la puerta
                        var doorPos = pa.Car.transform.position - pa.Car.transform.right * 1.4f;
                        pa.Dummy = ClientDummy.Spawn(pa.M.ClientName, pa.CarColor, doorPos,
                            QueueSpot(Queue.Count + pending.Count - 1));
                    }
                }
                else if (pa.Dummy.AtDestination)
                {
                    // llegó a la fila: ahora sí existe la tarea
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
                    if (!pa.M.EsVenta) MissionSystem.I.Add(pa.M);
                    GameEvents.Notify(pa.M.EsVenta
                        ? "Llegó " + pa.M.ClientName + " a recepción: " + pa.M.Title + " (pide $" + pa.M.Pay.ToString("N0") + ")"
                        : "Llegó " + pa.M.ClientName + " a recepción: " + pa.M.Title + " ($" + pa.M.Pay.ToString("N0") + ")");
                    Reflow();
                }
            }

            // timeouts: solo la autoridad decide; los clientes reciben el espejo
            if (Net.IsAuthority)
            {
                for (int i = Queue.Count - 1; i >= 0; i--)
                {
                    if (hour - Queue[i].ArriveHour > TimeoutHours && hour >= Queue[i].ArriveHour)
                    {
                        var w = Queue[i];
                        GameSync.MirrorDismiss(0, w.M.Id);
                        Dismiss(w);
                        MetasManager.I.CambiarReputacion(-1f, w.M.ClientName + " se cansó de esperar y se fue de mal humor");
                    }
                }
            }
        }

        public void Accept(WaitingClient w)
        {
            if (Net.IsClientOnly)
            {
                // prechequeo local con el estado sincronizado, luego el host ejecuta
                if (!w.M.EsVenta && BayManager.I.FreeSlotIndex() < 0)
                {
                    GameEvents.Notify("No hay slots libres en el taller. Termina un carro o mejora las bahías.");
                    return;
                }
                GameSync.RequestAccept(0, w.M.Id);
                return;
            }
            if (DoAccept(w)) GameSync.MirrorAccept(0, w.M.Id);
        }

        public bool AcceptById(int id)
        {
            var w = Queue.FirstOrDefault(x => x.M.Id == id);
            if (w == null) return false;
            bool ok = DoAccept(w);
            if (ok && Net.IsAuthority) GameSync.MirrorAccept(0, id);
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
                    if (!GameManager.I.Spend(w.M.Pay))
                    { GameEvents.Notify("No te alcanza: $" + w.M.Pay.ToString("N0")); return false; }
                    InventorySystem.I.Add(w.M.VentaItem, w.M.VentaCount);
                }
                Queue.Remove(w);
                if (w.Dummy != null) w.Dummy.Leave();
                GameEvents.Notify("Compraste " + InventorySystem.Label(w.M.VentaItem) + " x" + w.M.VentaCount +
                    " por $" + w.M.Pay.ToString("N0"));
                Reflow();
                return true;
            }

            if (BayManager.I.FreeSlotIndex() < 0)
            {
                GameEvents.Notify("No hay slots libres en el taller. Termina un carro o mejora las bahías.");
                return false;
            }
            Queue.Remove(w);
            if (w.WaitCar != null) Destroy(w.WaitCar); // el carro "entra" a la bahía (BayManager crea el suyo)
            BayManager.I.Occupy(w.M, w.CarColor);
            GameEvents.Notify("Aceptado: " + w.M.Title + ". El carro está en la bahía " + (w.M.Slot + 1) + ".");

            // el cliente espera en la sala de espera a que su carro esté listo
            if (w.Dummy != null)
            {
                w.Dummy.MoveTo(WaitSpots[waitingForJob.Count % WaitSpots.Length]);
                waitingForJob.Add((w.M, w.Dummy));
            }
            Reflow();
            return true;
        }

        public void Reject(WaitingClient w)
        {
            if (Net.IsClientOnly) { GameSync.RequestReject(0, w.M.Id); return; }
            DoReject(w);
            GameSync.MirrorDismiss(0, w.M.Id);
        }

        public void RejectById(int id)
        {
            var w = Queue.FirstOrDefault(x => x.M.Id == id);
            if (w == null) return;
            DoReject(w);
            if (Net.IsAuthority) GameSync.MirrorDismiss(0, id);
        }

        void DoReject(WaitingClient w)
        {
            Dismiss(w);
            if (!w.M.EsVenta)
            {
                MetasManager.I.CambiarReputacion(-0.5f);
                GameEvents.Notify(w.M.ClientName + ": \"¿Y ahora quién me arregla el carro?\" — se fue molesto.");
            }
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
            if (w.Dummy != null) w.Dummy.Leave();
            if (w.WaitCar != null)
            {
                if (TrafficManager.I != null) TrafficManager.I.DriveOff(w.WaitCar);
                else Destroy(w.WaitCar);
            }
            if (!w.M.EsVenta) MissionSystem.I.Reject(w.M);
            Reflow();
        }

        void Reflow()
        {
            for (int i = 0; i < Queue.Count; i++)
                if (Queue[i].Dummy != null) Queue[i].Dummy.MoveTo(QueueSpot(i));
        }

        /// <summary>Saca al cliente que esperaba esta misión (para que camine a su carro en la entrega).</summary>
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

        /// <summary>Su carro quedó listo: el cliente sigue esperando hasta que lo despaches.</summary>
        void OnMissionCompleted(Mission m)
        {
            foreach (var (mm, d) in waitingForJob)
                if (mm == m && d != null)
                    GameEvents.Notify(m.ClientName + " espera en la sala a que le entregues el carro.");
        }

        void OnMissionFailed(Mission m)
        {
            for (int i = waitingForJob.Count - 1; i >= 0; i--)
            {
                if (waitingForJob[i].m != m) continue;
                var d = waitingForJob[i].d;
                waitingForJob.RemoveAt(i);
                if (d != null) d.Leave();
            }
        }

        void OnDayStart(int day)
        {
            // nadie duerme en la sala de espera
            foreach (var (_, d) in waitingForJob)
                if (d != null) d.Leave();
            waitingForJob.Clear();
        }
    }

    /// <summary>Cliente visible (fila o sala de espera). Solo runtime, nunca serializado en escena.</summary>
    public class ClientDummy : MonoBehaviour
    {
        NavMeshAgent agent;
        Transform label;
        bool leaving;
        float killAt;
        Vector3 exitPoint = new Vector3(20f, 0f, -6f);

        public bool AtDestination => agent != null && agent.isOnNavMesh &&
            !agent.pathPending && agent.remainingDistance < 0.7f;

        public static ClientDummy Spawn(string clientName, Color shirt, Vector3 from, Vector3 to)
        {
            var go = new GameObject("Cliente_" + clientName.Replace(" ", ""));
            go.transform.position = from;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Cuerpo";
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Object.Destroy(body.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", shirt);
            body.GetComponent<Renderer>().material = mat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Cabeza";
            head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            head.transform.localScale = Vector3.one * 0.45f;
            Object.Destroy(head.GetComponent<Collider>());
            var skin = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            skin.SetColor("_BaseColor", new Color(0.85f, 0.65f, 0.5f));
            head.GetComponent<Renderer>().material = skin;

            var labelGo = new GameObject("Nombre");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            labelGo.transform.localScale = Vector3.one * 0.045f;
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = clientName;
            tm.fontSize = 48;
            tm.color = new Color(1f, 0.9f, 0.65f);
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            TextStyle.Apply(tm);

            var col = go.AddComponent<CapsuleCollider>();
            col.height = 1.9f;
            col.center = new Vector3(0f, 0.95f, 0f);
            col.radius = 0.4f;

            var d = go.AddComponent<ClientDummy>();
            d.label = labelGo.transform;
            d.agent = go.AddComponent<NavMeshAgent>();
            d.agent.speed = 1.9f;
            d.agent.radius = 0.3f;
            d.agent.height = 1.9f;
            d.agent.angularSpeed = 300f;
            if (!d.agent.isOnNavMesh && NavMesh.SamplePosition(from, out var hit, 6f, NavMesh.AllAreas))
                d.agent.Warp(hit.position);
            d.MoveTo(to);
            return d;
        }

        public void MoveTo(Vector3 p)
        {
            if (!leaving && agent != null && agent.isOnNavMesh) agent.SetDestination(p);
        }

        /// <summary>Camina a la salida y desaparece. exit opcional (los sucios salen por atrás).</summary>
        public void Leave(Vector3? exit = null)
        {
            leaving = true;
            killAt = Time.time + 30f;
            if (exit.HasValue) exitPoint = exit.Value;
            if (agent != null && agent.isOnNavMesh) agent.SetDestination(exitPoint);
        }

        void Update()
        {
            if (label != null && Camera.main != null)
                label.rotation = Quaternion.LookRotation(label.position - Camera.main.transform.position);

            if (leaving && agent != null &&
                ((!agent.pathPending && agent.remainingDistance < 1f) || Time.time > killAt))
                Destroy(gameObject);
        }
    }
}
