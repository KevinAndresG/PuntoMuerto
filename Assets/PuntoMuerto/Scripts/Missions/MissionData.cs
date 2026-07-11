using UnityEngine;

namespace PuntoMuerto
{
    public enum MissionType
    {
        ClienteHonesto,   // reparaciones legales
        Auto,             // familia A: placas, VIN, repintado, desarme
        Piezas,           // familia B: alterar series, empacar, vender
        Recoleccion,      // familia C: recoger/entregar en un punto
        Especial          // familia D
    }

    public enum MissionState { Pendiente, EnRecepcion, EnProgreso, Completada, Fallida }

    // Los índices EXISTENTES no se reordenan: el guardado y la red serializan por índice.
    // Los ítems nuevos se agregan al final. 0..5 = almacén normal + la vieja pieza ilegal;
    // 6..8 = repuestos legales nuevos; 9..13 = piezas del almacén turbio (patio).
    public enum ItemType
    {
        Aceite = 0, Llanta = 1, Repuesto = 2, Pintura = 3, Gasolina = 4, PiezaIlegal = 5,
        Filtro = 6, Bateria = 7, Pastillas = 8,
        PlacasBlanco = 9, KitVIN = 10, Desbloqueo = 11, BujiasPot = 12, ECUTrucada = 13
    }

    [System.Serializable]
    public struct PartReq
    {
        public ItemType Type;
        public int Count;
        public PartReq(ItemType t, int c) { Type = t; Count = c; }
    }

    // NO serializable a propósito: Unity serializaría por valor en los MonoBehaviours,
    // creando copias en blanco y rompiendo la identidad por referencia de las misiones.
    public class Mission
    {
        static int nextId = 1;

        public int Id;
        public MissionType Type;
        public string Title;
        public string Description;
        public string ClientName;
        public int Pay;
        public bool PayIsDirty;
        public int DeadlineDay;        // último día para entregar (encargos de Fabio)
        public float WorkRequired;     // segundos de trabajo en estación
        public float WorkDone;
        public MissionState State = MissionState.Pendiente;
        public bool EsNocturna;        // solo se trabaja de noche en el patio
        public float Noise;            // 0..1 cuánto ruido/luz genera (riesgo cerca)
        public bool OfreceLeverage;    // encargo de alto nivel: puedes quedarte una copia
        public bool ByCar;             // el cliente llega en carro
        public int OwnerNpcIndex = -1; // NPC dueño (para sospecha por retraso)
        public float RepBonus;
        public float FabioBonus;
        public System.Collections.Generic.List<PartReq> Parts = new System.Collections.Generic.List<PartReq>();
        public bool PartsConsumed;
        public int Slot = -1;          // slot de bahía asignado

        // venta ambulante: el "cliente" te VENDE un lote (Pay = lo que pagas tú)
        public bool EsVenta;
        // pedido: un comprador te COMPRA un lote de tu almacén (Pay = lo que te pagan a ti)
        public bool EsPedido;
        public ItemType VentaItem;
        public int VentaCount;

        public Mission() { Id = nextId++; }

        // venta (te venden) y pedido (te compran) son transacciones de mostrador, no encargos rastreados
        public bool EsTransaccion => EsVenta || EsPedido;
        public bool EsIlegal => Type != MissionType.ClienteHonesto;
        public float Progress01 => WorkRequired <= 0f ? 1f : Mathf.Clamp01(WorkDone / WorkRequired);
    }
}
