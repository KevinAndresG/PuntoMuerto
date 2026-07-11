using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Genera clientes honestos diarios y ofertas nocturnas de Fabio (GDD 4.1/4.2).</summary>
    public class MissionGenerator : MonoBehaviour
    {
        public static MissionGenerator I { get; private set; }

        public Mission PendingFabioOffer;   // esperando en el teléfono
        public bool PhoneRinging;

        class ScheduledClient { public Mission Mission; public float Hour; public bool Spawned; }
        readonly List<ScheduledClient> schedule = new List<ScheduledClient>();

        // pedido que aún no puedes cumplir (GDD 4.1)
        public string FocusUpgradeId;
        public string FocusClientName;

        class JobDef
        {
            public string Title; public int PayMin, PayMax; public float Work;
            public string RequiereUpgrade; public PartReq[] Parts;
            public JobDef(string t, int pMin, int pMax, float w, string upg, params PartReq[] parts)
            { Title = t; PayMin = pMin; PayMax = pMax; Work = w; RequiereUpgrade = upg; Parts = parts; }
        }

        static readonly JobDef[] Jobs =
        {
            new JobDef("Cambio de aceite", 150, 300, 12f, null, new PartReq(ItemType.Aceite, 1)),
            new JobDef("Cambio de llantas", 420, 620, 15f, null, new PartReq(ItemType.Llanta, 4)),
            new JobDef("Diagnóstico de motor", 200, 380, 18f, null, new PartReq(ItemType.Repuesto, 1)),
            new JobDef("Tanquear y revisar", 80, 150, 6f, null, new PartReq(ItemType.Gasolina, 1)),
            new JobDef("Frenos y suspensión", 300, 600, 25f, "elevador", new PartReq(ItemType.Repuesto, 2)),
            new JobDef("Revisión de transmisión", 350, 600, 25f, "elevador", new PartReq(ItemType.Repuesto, 2)),
            new JobDef("Pintura y carrocería", 400, 700, 0f, "pintura", new PartReq(ItemType.Pintura, 1)),
        };

        void Awake() { I = this; }
        bool fabioRolledToday;
        float nextWalkInHour;   // clientes espontáneos durante el día
        float dirtyTimer = 30f; // clientes turbios en la ventanilla trasera (segundos reales)

        static readonly string[] DirtyNames =
        {
            "Un tipo con gorra", "Mujer de lentes oscuros", "El flaco del bar",
            "Un desconocido", "El primo de alguien", "Voz ronca"
        };

        void OnEnable()
        {
            GameEvents.OnDayStart += OnDayStart;
            GameEvents.OnUpgradesChanged += OnUpgradesChanged;
        }
        void OnDisable()
        {
            GameEvents.OnDayStart -= OnDayStart;
            GameEvents.OnUpgradesChanged -= OnUpgradesChanged;
        }

        /// <summary>Chequeo por frame (robusto: no depende de ticks de hora exacta).</summary>
        void Update()
        {
            if (DayNightCycle.I == null || GameManager.I == null || GameManager.I.GamePaused) return;
            if (!Net.IsAuthority) return; // en multijugador solo el host genera clientes
            float hour = DayNightCycle.I.Hour;

            foreach (var s in schedule)
            {
                if (!s.Spawned && hour >= s.Hour)
                {
                    s.Spawned = true;
                    SpawnClientArrival(s.Mission);
                }
            }

            // clientes espontáneos: cada 0.8–2.2h de juego puede llegar alguien más
            if (hour >= nextWalkInHour && hour >= 8f && hour < 17.5f)
            {
                nextWalkInHour = hour + Random.Range(0.8f, 2.2f);
                if (ReceptionSystem.I != null && ReceptionSystem.I.Queue.Count < ReceptionSystem.I.MaxQueue &&
                    Random.value < 0.75f)
                {
                    // a veces en vez de un cliente llega un vendedor (te vende repuestos) o
                    // un comprador (te compra repuestos de tu almacén)
                    float r = Random.value;
                    if (r < 0.18f) ScheduleVendor(hour);
                    else if (r < 0.38f) ScheduleBuyer(hour);
                    else ScheduleHonestClient(MetasManager.I.Reputacion >= 30f, hour);
                }
            }

            // clientes turbios en la ventanilla trasera: día y noche (de noche llegan más).
            // Timer en segundos reales: inmune al wrap de medianoche.
            dirtyTimer -= Time.deltaTime;
            if (dirtyTimer <= 0f)
            {
                bool night = DayNightCycle.I.IsNight;
                dirtyTimer = (night ? Random.Range(0.7f, 1.8f) : Random.Range(2f, 3.5f)) * DayNightCycle.I.SecondsPerHour;
                if (DirtyReceptionSystem.I != null && DirtyReceptionSystem.I.CanReceive &&
                    Random.value < (night ? 0.85f : 0.5f))
                {
                    // a pie puede llegar quien te VENDE piezas o quien te las COMPRA; en carro, trabajo sucio
                    float r = Random.value;
                    Mission dm = r < 0.25f ? GenerateDirtyVendor()
                        : r < 0.5f ? GenerateDirtyBuyer()
                        : GenerateDirtyClient();
                    DirtyReceptionSystem.I.BeginArrival(dm,
                        new Color(Random.Range(0.15f, 0.4f), Random.Range(0.15f, 0.4f), Random.Range(0.15f, 0.4f)));
                }
            }

            // Fabio llama una vez al caer la noche, según Meta C
            if (!fabioRolledToday && hour >= 19f && hour < 21f)
            {
                fabioRolledToday = true;
                float chance = 0.55f + MetasManager.I.Fabio / 200f;
                if (GameManager.I.IsJefe) chance = 0f; // el Jefe no recibe órdenes
                if (Random.value < chance)
                {
                    PendingFabioOffer = GenerateFabioOffer();
                    PhoneRinging = true;
                    GameEvents.Notify("El teléfono de la oficina está sonando. Es Fabio.");
                    GameSync.MirrorOffer(PendingFabioOffer); // que también suene donde el socio
                }
            }
        }

        void OnDayStart(int day)
        {
            schedule.Clear();
            PhoneRinging = false;
            fabioRolledToday = false;
            nextWalkInHour = Random.Range(8.5f, 10f);

            bool premium = MetasManager.I.Reputacion >= 30f;
            int count = Random.Range(4, 7) + (premium ? 2 : 0);
            for (int i = 0; i < count; i++) ScheduleHonestClient(premium);

            // evento raro: pedido que no puedes cumplir todavía
            if (FocusUpgradeId == null && Random.value < 0.08f)
            {
                var faltantes = UpgradeSystem.Catalogo
                    .Where(u => !UpgradeSystem.I.Tiene(u.Id) && (u.Id == "pintura" || u.Id == "elevador"))
                    .ToList();
                if (faltantes.Count > 0)
                {
                    var u = faltantes[Random.Range(0, faltantes.Count)];
                    var npc = NPCManager.I.RandomClient();
                    FocusUpgradeId = u.Id;
                    FocusClientName = npc != null ? npc.NpcName : "Un cliente";
                    GameEvents.Notify("OPORTUNIDAD: " + FocusClientName + " necesita algo que tu taller aún no puede hacer (" +
                        u.Nombre + "). Si consigues esa mejora, pagará el triple.");
                }
            }
        }

        void OnUpgradesChanged()
        {
            if (FocusUpgradeId != null && UpgradeSystem.I.Tiene(FocusUpgradeId))
            {
                var m = new Mission
                {
                    Type = MissionType.ClienteHonesto,
                    Title = "Pedido especial de " + FocusClientName,
                    Description = "El trabajo que estuvo esperando. Paga triple.",
                    ClientName = FocusClientName,
                    Pay = Random.Range(450, 600) * 3,
                    WorkRequired = 30f,
                    RepBonus = 5f,
                    ByCar = true
                };
                m.Parts.Add(new PartReq(ItemType.Repuesto, 2));
                SpawnClientArrival(m);
                GameEvents.Notify(FocusClientName + " volvió con su pedido especial. ¡Atiéndelo en recepción!");
                FocusUpgradeId = null;
                FocusClientName = null;
            }
        }

        void ScheduleHonestClient(bool premium, float atHour = -1f)
        {
            var npc = NPCManager.I != null ? NPCManager.I.RandomClient() : null;
            string cliente = npc != null ? npc.NpcName : "Cliente de paso";

            var disponibles = Jobs.Where(j => j.RequiereUpgrade == null || UpgradeSystem.I.Tiene(j.RequiereUpgrade)).ToList();
            var job = disponibles[Random.Range(0, disponibles.Count)];

            int pay = Random.Range(job.PayMin, job.PayMax + 1);
            if (premium) pay = Mathf.RoundToInt(pay * 1.5f);

            var m = new Mission
            {
                Type = MissionType.ClienteHonesto,
                Title = job.Title,
                Description = "Trabajo honesto.",
                ClientName = cliente,
                Pay = pay,
                WorkRequired = job.Work,
                ByCar = Random.value > 0.35f,
                OwnerNpcIndex = npc != null ? NPCManager.I.NPCs.IndexOf(npc) : -1
            };
            m.Parts.AddRange(job.Parts);

            schedule.Add(new ScheduledClient { Mission = m, Hour = atHour >= 0f ? atHour : Random.Range(8.2f, 17f) });
        }

        static readonly (ItemType t, int n, int price)[] VendorLots =
        {
            (ItemType.Aceite, 3, 70), (ItemType.Llanta, 4, 170), (ItemType.Repuesto, 2, 120),
            (ItemType.Gasolina, 3, 55), (ItemType.Pintura, 1, 60)
        };

        static readonly string[] VendorNames =
        {
            "Vendedor ambulante", "El chatarrero", "Doña de los repuestos", "El rebuscador"
        };

        /// <summary>Vendedor a pie: te ofrece un lote de repuestos con descuento.</summary>
        void ScheduleVendor(float atHour)
        {
            var lot = VendorLots[Random.Range(0, VendorLots.Length)];
            var m = new Mission
            {
                Type = MissionType.ClienteHonesto,
                EsVenta = true,
                VentaItem = lot.t,
                VentaCount = lot.n,
                Pay = lot.price,
                Title = "Vende " + InventorySystem.Label(lot.t) + " x" + lot.n,
                Description = "Mercancía a buen precio, sin recibo.",
                ClientName = VendorNames[Random.Range(0, VendorNames.Length)],
                ByCar = false
            };
            schedule.Add(new ScheduledClient { Mission = m, Hour = atHour });
        }

        // lo que un comprador legal te puede pedir (de tu almacén normal)
        static readonly (ItemType t, int nMin, int nMax)[] BuyerWants =
        {
            (ItemType.Llanta, 4, 4), (ItemType.Repuesto, 1, 3), (ItemType.Aceite, 2, 4),
            (ItemType.Filtro, 1, 2), (ItemType.Pastillas, 1, 2), (ItemType.Bateria, 1, 1)
        };

        static readonly string[] BuyerNames =
        {
            "Un cliente apurado", "El de la ferretería", "Doña Marta", "Un taxista",
            "El vecino del camión", "Una señora"
        };

        /// <summary>Comprador legal: te COMPRA un lote de tu almacén (pagas con lo que ya tienes en stock).</summary>
        void ScheduleBuyer(float atHour)
        {
            var want = BuyerWants[Random.Range(0, BuyerWants.Length)];
            int n = Random.Range(want.nMin, want.nMax + 1);
            int market = InventorySystem.PriceOf(want.t) * n;
            int pay = Mathf.RoundToInt(market * Random.Range(1.35f, 1.7f)); // margen para ti
            var m = new Mission
            {
                Type = MissionType.ClienteHonesto,
                EsPedido = true,
                VentaItem = want.t,
                VentaCount = n,
                Pay = pay,
                Title = "Compra " + InventorySystem.Label(want.t) + " x" + n,
                Description = "Quiere llevarse repuestos de tu almacén. Buen dinero si los tienes.",
                ClientName = BuyerNames[Random.Range(0, BuyerNames.Length)],
                ByCar = false
            };
            schedule.Add(new ScheduledClient { Mission = m, Hour = atHour });
        }

        void SpawnClientArrival(Mission m)
        {
            if (ReceptionSystem.I == null || !ReceptionSystem.I.CanReceive) return; // recepción llena
            if (!ShopSign.Abierto) return; // taller cerrado: el cliente ni viene

            var carColor = new Color(Random.value * 0.6f + 0.2f, Random.value * 0.5f + 0.2f, Random.value * 0.5f + 0.2f);
            // secuencia completa la maneja ReceptionSystem: carro llega → cliente baja → fila → tarea
            ReceptionSystem.I.BeginArrival(m, carColor);
        }

        /// <summary>Cliente turbio de la ventanilla trasera: trabajo sucio por carro, se hace en el patio.</summary>
        Mission GenerateDirtyClient()
        {
            int tier = UpgradeSystem.I != null ? UpgradeSystem.I.TallerTier : 0;
            float roll = Random.value;

            var m = new Mission
            {
                Type = MissionType.Auto,
                PayIsDirty = true,
                ByCar = true,
                ClientName = DirtyNames[Random.Range(0, DirtyNames.Length)],
                FabioBonus = 4f
            };

            if (roll < 0.4f)
            {
                m.Title = "Cambio de placas y VIN";
                m.Description = "Que este carro parezca otro antes de que alguien pregunte.";
                m.Pay = Random.Range(700, 1401);
                m.WorkRequired = 22f;
                m.Noise = 0.45f;
            }
            else if (roll < 0.7f && tier >= 1)
            {
                m.Title = "Desarme exprés";
                m.Description = "Este carro sobra. Piezas sueltas valen más que el carro entero.";
                m.Pay = Random.Range(1800, 4001);
                m.WorkRequired = 40f;
                m.Noise = 0.85f;
            }
            else
            {
                m.Title = "Pintura sin preguntas";
                m.Description = "Otro color, hoy mismo. No preguntes por las manchas.";
                m.Pay = Random.Range(900, 1801);
                m.WorkRequired = 18f;
                m.Noise = 0.35f;
            }
            return m;
        }

        /// <summary>Tipo a pie que te VENDE un lote de piezas turbias (baratas para revender/usar).</summary>
        Mission GenerateDirtyVendor()
        {
            var turbios = InventorySystem.ItemsOf(Zona.Turbio, soloComprables: true).ToArray();
            var t = turbios[Random.Range(0, turbios.Length)];
            int n = Random.Range(1, 4);
            // te lo dejan por debajo del precio de mercado turbio (margen para ti al revender)
            int pay = Mathf.RoundToInt(InventorySystem.PriceOf(t) * n * Random.Range(0.55f, 0.8f));
            return new Mission
            {
                Type = MissionType.Piezas,
                EsVenta = true,
                VentaItem = t,
                VentaCount = n,
                Pay = pay,
                PayIsDirty = true,
                Title = "Ofrece " + InventorySystem.Label(t) + " x" + n,
                Description = "Sin preguntas. Barato para lo que valen.",
                ClientName = DirtyNames[Random.Range(0, DirtyNames.Length)],
                ByCar = false
            };
        }

        /// <summary>Comprador turbio: te COMPRA piezas/dispositivos de la bodega del patio (paga sucio).</summary>
        Mission GenerateDirtyBuyer()
        {
            var turbios = InventorySystem.ItemsOf(Zona.Turbio, soloComprables: true).ToArray();
            var t = turbios[Random.Range(0, turbios.Length)];
            int n = Random.Range(1, 4);
            int pay = Mathf.RoundToInt(InventorySystem.PriceOf(t) * n * Random.Range(1.6f, 2.3f));
            return new Mission
            {
                Type = MissionType.Piezas,
                EsPedido = true,
                VentaItem = t,
                VentaCount = n,
                Pay = pay,
                PayIsDirty = true,
                Title = "Busca " + InventorySystem.Label(t) + " x" + n,
                Description = "Paga bien por lo que tengas en la bodega. Sin recibo.",
                ClientName = DirtyNames[Random.Range(0, DirtyNames.Length)],
                ByCar = false
            };
        }

        Mission GenerateFabioOffer()
        {
            float c = MetasManager.I.Fabio;
            int tier = UpgradeSystem.I.TallerTier;
            var opciones = new List<MissionType> { MissionType.Recoleccion, MissionType.Auto };
            if (c >= 20f) opciones.Add(MissionType.Piezas);
            if (c >= 60f && tier >= 2) opciones.Add(MissionType.Especial);
            var tipo = opciones[Random.Range(0, opciones.Count)];

            var m = new Mission { Type = tipo, PayIsDirty = true, EsNocturna = tipo != MissionType.Recoleccion };
            m.DeadlineDay = GameManager.I.Day + Random.Range(1, 3);
            var npcDueno = NPCManager.I.RandomNPC();
            m.OwnerNpcIndex = npcDueno != null ? NPCManager.I.NPCs.IndexOf(npcDueno) : -1;

            switch (tipo)
            {
                case MissionType.Auto:
                    bool desarme = Random.value > 0.5f && tier >= 1;
                    m.Title = desarme ? "Desarme completo" : "Cambio de placas y VIN";
                    m.Description = desarme
                        ? "Un auto tiene que dejar de existir esta noche. Pieza por pieza."
                        : "Auto caliente. Placas nuevas, VIN limado, y nadie lo vio.";
                    m.Pay = desarme ? Random.Range(2500, 6001) : Random.Range(800, 1501);
                    m.WorkRequired = desarme ? 50f : 25f;
                    m.Noise = desarme ? 0.9f : 0.5f;
                    m.OfreceLeverage = desarme && c >= 50f && Random.value < 0.4f;
                    break;
                case MissionType.Piezas:
                    m.Title = "Lote de piezas sin papeles";
                    m.Description = "Clasificar, limar números de serie y empacar para el comprador.";
                    m.Pay = Random.Range(500, 3001);
                    m.WorkRequired = 35f;
                    m.Noise = 0.6f;
                    m.OfreceLeverage = c >= 60f && Random.value < 0.35f;
                    break;
                case MissionType.Recoleccion:
                    m.Title = "Recolección discreta";
                    m.Description = "Recoge un paquete en el pueblo y tráelo al patio. Sin preguntas.";
                    m.Pay = Random.Range(600, 1401);
                    m.WorkRequired = 0f;
                    m.Noise = 0.1f;
                    break;
                default:
                    m.Title = "Encargo especial: prueba de confianza";
                    m.Description = "Auto dañado con piezas calientes adentro. Todo junto, esta noche.";
                    m.Pay = Random.Range(3000, 7001);
                    m.WorkRequired = 60f;
                    m.Noise = 1f;
                    m.FabioBonus = 25f; // +15 extra por riesgo alto (GDD 2.3)
                    m.OfreceLeverage = Random.value < 0.5f;
                    break;
            }
            return m;
        }

        /// <summary>Aceptar oferta desde el teléfono (solo autoridad; el cliente pide por GameSync).</summary>
        public void AcceptOffer()
        {
            if (PendingFabioOffer == null) return;
            var m = PendingFabioOffer;
            PendingFabioOffer = null;
            PhoneRinging = false;
            GameSync.MirrorOfferCleared();
            MissionSystem.I.Add(m);

            if (m.Type == MissionType.Recoleccion)
            {
                PickupPoint.SpawnForMission(m);
            }
            else
            {
                var patio = RepairStation.All.FirstOrDefault(r => r.Kind == StationKind.Patio && r.CurrentMission == null);
                if (patio != null) patio.CurrentMission = m;
                GameSync.MirrorPatioJob(m);
                GameEvents.Notify("El encargo espera en el patio trasero. Trabájalo de noche.");
            }
        }

        public void RejectOffer()
        {
            if (PendingFabioOffer == null) return;
            PendingFabioOffer = null;
            PhoneRinging = false;
            GameSync.MirrorOfferCleared();
            MetasManager.I.CambiarFabio(Random.Range(-20f, -10f), "Rechazaste el encargo");
        }

        /// <summary>Operación propia del Jefe (GDD 6.3). Solo autoridad; el cliente pide por GameSync.</summary>
        public void JefeOrder(bool delegar)
        {
            if (GameManager.I == null || !GameManager.I.IsJefe) return;
            var job = new Mission
            {
                Type = MissionType.Piezas,
                Title = "Operación propia: lote de piezas",
                Description = "Tu red consigue el material. Tú decides quién lo trabaja.",
                Pay = Random.Range(1500, 4501),
                PayIsDirty = true,
                DeadlineDay = GameManager.I.Day + 2,
                WorkRequired = 30f,
                Noise = 0.5f,
                EsNocturna = true
            };
            if (delegar)
            {
                MissionSystem.I.Delegate(job);
                return;
            }
            MissionSystem.I.Add(job);
            foreach (var st in RepairStation.All)
                if (st.Kind == StationKind.Patio && st.CurrentMission == null)
                { st.CurrentMission = job; break; }
            GameSync.MirrorPatioJob(job);
            GameEvents.Notify("El encargo espera en el patio trasero. Trabájalo de noche.");
        }
    }
}
