using Unity.Netcode;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Ayudas de red: en single player no hay NetworkManager y todo es "autoridad".</summary>
    public static class Net
    {
        public static bool Active => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        /// <summary>true si soy quien simula el juego (host, o partida local sin red).</summary>
        public static bool IsAuthority => !Active || NetworkManager.Singleton.IsServer;
        /// <summary>true si soy un cliente conectado a un host remoto.</summary>
        public static bool IsClientOnly => Active && !NetworkManager.Singleton.IsServer;
    }

    /// <summary>Sincroniza el estado compartido del host a los clientes (GDD sección 9: metas de equipo).
    /// El host es la autoridad de TODO el estado de juego: dinero, metas, inventario, letrero,
    /// llegadas de clientes, aceptar/rechazar, progreso y entrega de misiones.
    /// Los clientes ven un espejo y le piden acciones al host por ServerRpc.</summary>
    public class GameSync : NetworkBehaviour
    {
        public static GameSync I { get; private set; }

        float timer;
        float progressTimer;

        // acumulador de trabajo del cliente (para no mandar un RPC por frame)
        static int pendingWorkId = -1;
        static float pendingWorkAmt;
        static float workFlushAt;

        public override void OnNetworkSpawn() { I = this; }
        public override void OnNetworkDespawn() { if (I == this) I = null; }

        void Update()
        {
            // cliente: enviar trabajo acumulado
            if (Net.IsClientOnly && pendingWorkAmt > 0f && Time.time >= workFlushAt)
                FlushWork();

            if (!IsServer || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 2f;
                var g = GameManager.I; var m = MetasManager.I;
                if (g == null || m == null) return;
                var inv = new int[6];
                if (InventorySystem.I != null)
                    for (int i = 0; i < 6; i++) inv[i] = InventorySystem.I.Count((ItemType)i);
                SyncStateClientRpc(g.Day, g.CleanMoney, g.DirtyMoney,
                    m.DeudaPagada, m.Reputacion, m.Fabio, m.Leverage,
                    DayNightCycle.I != null ? DayNightCycle.I.Hour : 12f,
                    ShopSign.Abierto, inv);
            }

            // progreso de misiones activas (para que el otro vea la barra avanzar)
            progressTimer -= Time.deltaTime;
            if (progressTimer <= 0f && MissionSystem.I != null)
            {
                progressTimer = 1f;
                foreach (var mi in MissionSystem.I.Active)
                    if (mi.State == MissionState.EnProgreso)
                        ProgressClientRpc(mi.Id, mi.WorkDone);
            }
        }

        [ClientRpc]
        void SyncStateClientRpc(int day, int clean, int dirty, int deuda, float rep, float fabio,
            int leverage, float hour, bool signOpen, int[] inv)
        {
            if (IsServer) return; // el host ya tiene el estado
            var g = GameManager.I; var m = MetasManager.I;
            if (g == null || m == null) return;
            if (g.Day != day)
            {
                g.Day = day;
                GameEvents.OnDayStart?.Invoke(day);
            }
            g.CleanMoney = clean;
            g.DirtyMoney = dirty;
            m.DeudaPagada = deuda;
            m.Reputacion = rep;
            m.Fabio = fabio;
            m.Leverage = leverage;
            if (DayNightCycle.I != null) DayNightCycle.I.Hour = hour;
            ShopSign.Abierto = signOpen;
            if (InventorySystem.I != null && inv != null) InventorySystem.I.SetAll(inv);
            GameEvents.OnMetasChanged?.Invoke();
        }

        // ---------- llegadas y fila (host → clientes) ----------

        // DTO serializable para viajar por red (Mission NO puede ser [Serializable]: ver MissionData.cs)
        [System.Serializable]
        struct MissionWire
        {
            public int Id, Type, Pay, DeadlineDay, OwnerNpcIndex, VentaItem, VentaCount;
            public string Title, Description, ClientName;
            public bool PayIsDirty, EsNocturna, OfreceLeverage, ByCar, EsVenta;
            public float WorkRequired, WorkDone, Noise, RepBonus, FabioBonus;
            public int[] PartTypes, PartCounts;
        }

        static string Pack(Mission m)
        {
            var w = new MissionWire
            {
                Id = m.Id, Type = (int)m.Type, Pay = m.Pay, DeadlineDay = m.DeadlineDay,
                OwnerNpcIndex = m.OwnerNpcIndex, VentaItem = (int)m.VentaItem, VentaCount = m.VentaCount,
                Title = m.Title, Description = m.Description, ClientName = m.ClientName,
                PayIsDirty = m.PayIsDirty, EsNocturna = m.EsNocturna, OfreceLeverage = m.OfreceLeverage,
                ByCar = m.ByCar, EsVenta = m.EsVenta,
                WorkRequired = m.WorkRequired, WorkDone = m.WorkDone, Noise = m.Noise,
                RepBonus = m.RepBonus, FabioBonus = m.FabioBonus,
                PartTypes = new int[m.Parts.Count], PartCounts = new int[m.Parts.Count]
            };
            for (int i = 0; i < m.Parts.Count; i++)
            { w.PartTypes[i] = (int)m.Parts[i].Type; w.PartCounts[i] = m.Parts[i].Count; }
            return JsonUtility.ToJson(w);
        }

        static Mission Unpack(string json)
        {
            var w = JsonUtility.FromJson<MissionWire>(json);
            var m = new Mission
            {
                Id = w.Id, Type = (MissionType)w.Type, Pay = w.Pay, DeadlineDay = w.DeadlineDay,
                OwnerNpcIndex = w.OwnerNpcIndex, VentaItem = (ItemType)w.VentaItem, VentaCount = w.VentaCount,
                Title = w.Title, Description = w.Description, ClientName = w.ClientName,
                PayIsDirty = w.PayIsDirty, EsNocturna = w.EsNocturna, OfreceLeverage = w.OfreceLeverage,
                ByCar = w.ByCar, EsVenta = w.EsVenta,
                WorkRequired = w.WorkRequired, WorkDone = w.WorkDone, Noise = w.Noise,
                RepBonus = w.RepBonus, FabioBonus = w.FabioBonus
            };
            if (w.PartTypes != null)
                for (int i = 0; i < w.PartTypes.Length; i++)
                    m.Parts.Add(new PartReq((ItemType)w.PartTypes[i], w.PartCounts[i]));
            return m;
        }

        public static void MirrorArrival(int kind, Mission m, Color c)
        {
            if (I != null && I.IsServer)
                I.ArrivalClientRpc(kind, Pack(m), c.r, c.g, c.b);
        }

        [ClientRpc]
        void ArrivalClientRpc(int kind, string json, float r, float g, float b)
        {
            if (IsServer) return;
            var m = Unpack(json);
            var c = new Color(r, g, b);
            if (kind == 0) { if (ReceptionSystem.I != null) ReceptionSystem.I.BeginArrival(m, c, true); }
            else { if (DirtyReceptionSystem.I != null) DirtyReceptionSystem.I.BeginArrival(m, c, true); }
        }

        public static void MirrorAccept(int kind, int id)
        { if (I != null && I.IsServer) I.AcceptClientRpc(kind, id); }

        [ClientRpc]
        void AcceptClientRpc(int kind, int id)
        {
            if (IsServer) return;
            if (kind == 0) { if (ReceptionSystem.I != null) ReceptionSystem.I.AcceptById(id); }
            else { if (DirtyReceptionSystem.I != null) DirtyReceptionSystem.I.AcceptById(id); }
        }

        public static void MirrorDismiss(int kind, int id)
        { if (I != null && I.IsServer) I.DismissClientRpc(kind, id); }

        [ClientRpc]
        void DismissClientRpc(int kind, int id)
        {
            if (IsServer) return;
            if (kind == 0) { if (ReceptionSystem.I != null) ReceptionSystem.I.DismissById(id); }
            else { if (DirtyReceptionSystem.I != null) DirtyReceptionSystem.I.DismissById(id); }
        }

        // ---------- acciones del cliente (cliente → host) ----------

        public static void RequestAccept(int kind, int id)
        { if (I != null) I.AcceptServerRpc(kind, id); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void AcceptServerRpc(int kind, int id)
        {
            if (kind == 0) { if (ReceptionSystem.I != null) ReceptionSystem.I.AcceptById(id); }
            else { if (DirtyReceptionSystem.I != null) DirtyReceptionSystem.I.AcceptById(id); }
        }

        public static void RequestReject(int kind, int id)
        { if (I != null) I.RejectServerRpc(kind, id); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void RejectServerRpc(int kind, int id)
        {
            if (kind == 0) { if (ReceptionSystem.I != null) ReceptionSystem.I.RejectById(id); }
            else { if (DirtyReceptionSystem.I != null) DirtyReceptionSystem.I.RejectById(id); }
        }

        // ---------- trabajo y entrega ----------

        public static void SendWork(int id, float amount)
        {
            if (I == null) return;
            if (pendingWorkId != id && pendingWorkAmt > 0f) FlushWork();
            pendingWorkId = id;
            pendingWorkAmt += amount;
        }

        static void FlushWork()
        {
            if (I == null || pendingWorkAmt <= 0f) return;
            I.WorkServerRpc(pendingWorkId, pendingWorkAmt);
            pendingWorkAmt = 0f;
            workFlushAt = Time.time + 0.3f;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void WorkServerRpc(int id, float amount)
        {
            if (MissionSystem.I != null) MissionSystem.I.HostWork(id, amount);
        }

        [ClientRpc]
        void ProgressClientRpc(int id, float workDone)
        {
            if (IsServer) return;
            var m = MissionSystem.I != null ? MissionSystem.I.FindById(id) : null;
            if (m != null && m.State != MissionState.Completada)
                m.WorkDone = Mathf.Max(m.WorkDone, workDone);
        }

        public static void MirrorMissionComplete(int id)
        { if (I != null && I.IsServer) I.CompleteClientRpc(id); }

        [ClientRpc]
        void CompleteClientRpc(int id)
        {
            if (IsServer) return;
            if (MissionSystem.I != null) MissionSystem.I.MirrorComplete(id);
        }

        public static void RequestDeliver(int id)
        { if (I != null) I.DeliverServerRpc(id); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void DeliverServerRpc(int id)
        {
            var m = MissionSystem.I != null ? MissionSystem.I.FindById(id) : null;
            // FindById no ve misiones completadas (salen de Active): buscar también por slots vía CarJob
            if (m == null) m = FindDeliverable(id);
            if (m != null && BayManager.I != null)
            {
                MirrorDeliver(id);
                BayManager.I.Deliver(m);
            }
        }

        public static void MirrorDeliver(int id)
        { if (I != null && I.IsServer) I.DeliverClientRpc(id); }

        [ClientRpc]
        void DeliverClientRpc(int id)
        {
            if (IsServer) return;
            var m = FindDeliverable(id);
            if (m != null && BayManager.I != null) BayManager.I.Deliver(m);
        }

        static Mission FindDeliverable(int id)
        {
            foreach (var job in Object.FindObjectsByType<CarJob>(FindObjectsSortMode.None))
                if (job.Mission != null && job.Mission.Id == id) return job.Mission;
            return null;
        }

        // ---------- letrero, compras, mejoras, teléfono ----------

        public static void RequestSign(bool open)
        { if (I != null) I.SignServerRpc(open); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void SignServerRpc(bool open)
        {
            ShopSign.Abierto = open; // el sync periódico lo reparte
        }

        public static void RequestBuy(ItemType t, int n)
        { if (I != null) I.BuyServerRpc((int)t, n); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void BuyServerRpc(int item, int n)
        {
            var t = (ItemType)item;
            int p = InventorySystem.PriceOf(t);
            if (MetasManager.I != null && MetasManager.I.Reputacion >= 60f) p = Mathf.RoundToInt(p * 0.8f);
            int costo = p * n;
            if (InventorySystem.I == null || GameManager.I == null) return;
            if (InventorySystem.I.Used + n > InventorySystem.I.Capacity) return;
            if (!GameManager.I.Spend(costo)) return;
            InventorySystem.I.Add(t, n);
            GameEvents.Notify("Tu socio compró " + InventorySystem.Label(t) + " x" + n + " por $" + costo.ToString("N0"));
        }

        public static void RequestUpgrade(string id)
        { if (I != null) I.UpgradeServerRpc(id); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void UpgradeServerRpc(string id)
        {
            if (UpgradeSystem.I != null) UpgradeSystem.I.Comprar(id);
        }

        public static void MirrorUpgrade(string id)
        { if (I != null && I.IsServer) I.UpgradeClientRpc(id); }

        [ClientRpc]
        void UpgradeClientRpc(string id)
        {
            if (IsServer) return;
            if (UpgradeSystem.I != null) UpgradeSystem.I.InstallLocal(id);
        }

        public static void RequestSellPieces()
        { if (I != null) I.SellPiecesServerRpc(); }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void SellPiecesServerRpc()
        {
            int piezas = InventorySystem.I != null ? InventorySystem.I.Count(ItemType.PiezaIlegal) : 0;
            if (piezas <= 0) return;
            int precio = piezas * Random.Range(250, 451);
            InventorySystem.I.Remove(ItemType.PiezaIlegal, piezas);
            GameManager.I.AddMoney(precio, true);
            MetasManager.I.CambiarFabio(2f);
            GameEvents.Notify("Lote vendido: +$" + precio.ToString("N0") + " (sucio).");
        }
    }
}
