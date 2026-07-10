using UnityEngine;
using UnityEngine.InputSystem;

namespace PuntoMuerto
{
    /// <summary>Slots del taller: bahías del frente (3 base + mejoras) y slots del patio trasero (negocio sucio).
    /// Convención: Mission.Slot >= 100 = slot de patio (índice - 100).</summary>
    public class BayManager : MonoBehaviour
    {
        public static BayManager I { get; private set; }

        Transform[] slots;
        Mission[] occupants;
        GameObject[] cars;

        Transform[] patioSlots;
        Mission[] patioOccupants;
        GameObject[] patioCars;

        void Awake() { I = this; }

        void Start()
        {
            var root = GameObject.Find("Taller/Slots");
            int n = root != null ? root.transform.childCount : 0;
            slots = new Transform[n];
            for (int i = 0; i < n; i++) slots[i] = root.transform.GetChild(i);
            occupants = new Mission[n];
            cars = new GameObject[n];

            var patioRoot = GameObject.Find("Taller/PatioSlots");
            int pn = patioRoot != null ? patioRoot.transform.childCount : 0;
            patioSlots = new Transform[pn];
            for (int i = 0; i < pn; i++) patioSlots[i] = patioRoot.transform.GetChild(i);
            patioOccupants = new Mission[pn];
            patioCars = new GameObject[pn];
        }

        public int TotalSlots => slots != null ? slots.Length : 0;
        public int TotalPatioSlots => patioSlots != null ? patioSlots.Length : 0;

        public int UnlockedSlots
        {
            get
            {
                int u = 3;
                if (UpgradeSystem.I != null && UpgradeSystem.I.Tiene("bahia4")) u++;
                if (UpgradeSystem.I != null && UpgradeSystem.I.Tiene("bahia5")) u++;
                return Mathf.Min(u, TotalSlots);
            }
        }

        public int FreeSlotIndex()
        {
            for (int i = 0; i < UnlockedSlots; i++)
                if (occupants[i] == null) return i;
            return -1;
        }

        public int FreePatioSlotIndex()
        {
            for (int i = 0; i < TotalPatioSlots; i++)
                if (patioOccupants[i] == null) return i;
            return -1;
        }

        public bool Occupy(Mission m, Color carColor)
        {
            int i = FreeSlotIndex();
            if (i < 0) return false;
            occupants[i] = m;
            m.Slot = i;
            m.State = MissionState.Pendiente;
            cars[i] = SpawnJobCar(m, carColor, slots[i]);
            return true;
        }

        public bool OccupyPatio(Mission m, Color carColor)
        {
            int i = FreePatioSlotIndex();
            if (i < 0) return false;
            patioOccupants[i] = m;
            m.Slot = 100 + i;
            m.State = MissionState.Pendiente;
            patioCars[i] = SpawnJobCar(m, carColor, patioSlots[i]);
            return true;
        }

        static GameObject SpawnJobCar(Mission m, Color color, Transform slot)
        {
            var car = TrafficManager.BuildCar(color, false);
            car.name = "CarroCliente_" + m.Id;
            car.transform.position = slot.position;
            car.transform.rotation = slot.rotation;
            var job = car.AddComponent<CarJob>();
            job.Mission = m;
            return car;
        }

        /// <summary>Entrega: el carro sale del slot, el cliente que esperaba sube y se van juntos.</summary>
        public void Deliver(Mission m)
        {
            if (m == null || m.Slot < 0) return;

            GameObject car;
            ClientDummy dummy;
            Vector3 curb;
            Vector3[] exitPath;
            if (m.Slot >= 100)
            {
                int i = m.Slot - 100;
                if (i >= TotalPatioSlots) return;
                patioOccupants[i] = null;
                car = patioCars[i];
                patioCars[i] = null;
                dummy = DirtyReceptionSystem.I != null ? DirtyReceptionSystem.I.TakeWaiting(m) : null;
                curb = new Vector3(39f, 0.08f, -41.5f); // frente a la rendija sur
                exitPath = new[] { new Vector3(39f, 0.08f, -50f), new Vector3(20f, 0.08f, -62f) };
            }
            else
            {
                int i = m.Slot;
                if (i >= TotalSlots) return;
                occupants[i] = null;
                car = cars[i];
                cars[i] = null;
                dummy = ReceptionSystem.I != null ? ReceptionSystem.I.TakeWaiting(m) : null;
                curb = new Vector3(35f, 0.08f, -4.5f); // parqueadero del frente
                exitPath = new[] { new Vector3(48f, 0.08f, 2f), new Vector3(75f, 0.08f, 2f) };
            }
            m.Slot = -1;
            if (car == null)
            {
                if (dummy != null) dummy.Leave();
                return;
            }

            Destroy(car.GetComponent<CarJob>());
            var del = car.AddComponent<CarDelivery>();
            del.Begin(curb, exitPath, dummy);
            if (dummy != null)
                GameEvents.Notify(m.ClientName + " va por su carro.");
        }
    }

    /// <summary>Secuencia de entrega (solo runtime): carro sale al punto de recogida,
    /// el cliente camina hasta él, "sube" y el carro se va.</summary>
    public class CarDelivery : MonoBehaviour
    {
        ClientDummy dummy;
        Vector3[] exitPath;
        VehicleAI ai;
        int phase;        // 0 = saliendo al punto, 1 = esperando al cliente, 2 = yéndose
        float timeoutAt;

        public void Begin(Vector3 curb, Vector3[] exit, ClientDummy d)
        {
            dummy = d;
            exitPath = exit;
            ai = GetComponent<VehicleAI>();
            if (ai == null) ai = gameObject.AddComponent<VehicleAI>();
            ai.Speed = 4.5f;
            ai.SetPath(MakePoints(transform.position, new[] { curb }), false, false);
            timeoutAt = Time.time + 50f;
        }

        void Update()
        {
            if (phase == 0 && (ai == null || ai.Finished))
            {
                phase = 1;
                if (dummy != null)
                    dummy.MoveTo(transform.position - transform.right * 1.6f);
            }
            else if (phase == 1)
            {
                bool aboard = dummy == null ||
                    Vector3.Distance(dummy.transform.position, transform.position) < 2.4f;
                if (aboard || Time.time > timeoutAt)
                {
                    if (dummy != null) Destroy(dummy.gameObject);
                    dummy = null;
                    phase = 2;
                    ai.Speed = 7f;
                    ai.SetPath(MakePoints(transform.position, exitPath), false, true);
                }
            }
        }

        static Transform[] MakePoints(Vector3 start, Vector3[] rest)
        {
            var pts = new Transform[rest.Length + 1];
            var p0 = new GameObject("del0").transform;
            p0.position = start;
            pts[0] = p0;
            Destroy(p0.gameObject, 60f);
            for (int k = 0; k < rest.Length; k++)
            {
                var t = new GameObject("del" + (k + 1)).transform;
                t.position = rest[k];
                pts[k + 1] = t;
                Destroy(t.gameObject, 60f);
            }
            return pts;
        }
    }

    /// <summary>Carro de cliente en un slot: mantén E para trabajar. Consume repuestos. El trabajo ilegal hace ruido.
    /// Al terminar queda "listo" y se despacha con E (el cliente sube y se va).</summary>
    public class CarJob : MonoBehaviour, IInteractable
    {
        public Mission Mission;
        Transform player;
        bool working;
        bool dispatched;
        GameObject workLight;

        bool EsPintura => Mission != null && Mission.Title.Contains("Pintura");
        bool Listo => Mission != null && Mission.State == MissionState.Completada;

        public string Prompt
        {
            get
            {
                if (Mission == null) return null;
                if (Listo) return "Entregar el carro de " + Mission.ClientName;
                if (!Mission.PartsConsumed && Mission.Parts.Count > 0 && !InventorySystem.I.Has(Mission.Parts))
                    return "Faltan repuestos: " + InventorySystem.I.MissingText(Mission.Parts) + " (cómpralos en el PC de la oficina)";
                if (EsPintura) return "Lijar y pintar: " + Mission.Title;
                return "Trabajar: " + Mission.Title + " " + Mathf.RoundToInt(Mission.Progress01 * 100f) + "%";
            }
        }

        public bool CanInteract => Mission != null && !dispatched;

        public void Interact(PlayerInteraction p)
        {
            if (Listo)
            {
                dispatched = true;
                if (Net.IsClientOnly) GameSync.RequestDeliver(Mission.Id);
                else
                {
                    GameSync.MirrorDeliver(Mission.Id);
                    BayManager.I.Deliver(Mission);
                }
                return;
            }
            if (!Mission.PartsConsumed && Mission.Parts.Count > 0)
            {
                if (!InventorySystem.I.Consume(Mission.Parts))
                {
                    GameEvents.Notify("No tienes los repuestos: " + InventorySystem.I.MissingText(Mission.Parts));
                    return;
                }
                Mission.PartsConsumed = true;
            }
            if (EsPintura)
            {
                var mg = Object.FindFirstObjectByType<SandingMinigameUI>();
                if (mg != null) { mg.Open(Mission, null); return; }
            }
            player = p.transform;
            working = true;
        }

        void Update()
        {
            if (!working || Mission == null) { SetWorkLight(false); return; }
            var kb = Keyboard.current;
            bool holding = kb != null && kb.eKey.isPressed;
            bool near = player != null && Vector3.Distance(player.position, transform.position) < 3.5f;
            if (!holding || !near || UIRoot.ModalOpen) { working = false; SetWorkLight(false); return; }

            SetWorkLight(true);
            bool done = MissionSystem.I.DoWork(Mission, Time.deltaTime);
            HUDController.SetWorkProgress(Mission.Progress01, Mission.Title,
                Mission.EsIlegal ? Mission.Noise : 0f);
            if (done || Listo)
            {
                working = false;
                SetWorkLight(false);
                HUDController.SetWorkProgress(-1f, null, 0f);
                GameEvents.Notify("\"" + Mission.Title + "\" terminado. Despacha el carro con E.");
            }
        }

        /// <summary>Luz de trabajo: soplete naranja si es ilegal (alerta vecinos), azulada si es legal.</summary>
        void SetWorkLight(bool on)
        {
            if (on && workLight == null)
            {
                bool ilegal = Mission != null && Mission.EsIlegal;
                workLight = new GameObject("LuzTrabajo");
                workLight.transform.SetParent(transform, false);
                workLight.transform.localPosition = Vector3.up * 1.3f;
                var l = workLight.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = ilegal ? new Color(1f, 0.6f, 0.25f) : new Color(0.65f, 0.8f, 1f);
                l.range = ilegal ? 8f : 5f;
                l.intensity = ilegal ? 3.5f : 2.2f;
                var flick = workLight.AddComponent<LightFlicker>();
                flick.BaseIntensity = l.intensity;
            }
            else if (!on && workLight != null)
            {
                Destroy(workLight);
                workLight = null;
            }
        }
    }
}
