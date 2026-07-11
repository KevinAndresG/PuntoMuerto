using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Zona de almacén: el taller legal (garaje) y el turbio (bodega del patio).</summary>
    public enum Zona { Normal, Turbio }

    /// <summary>
    /// Almacén del taller. Un solo diccionario de conteos, pero cada ItemType pertenece a una ZONA
    /// (Normal = repuestos/consumibles legales del garaje; Turbio = piezas y dispositivos del patio)
    /// con capacidad independiente. La tabla estática Meta centraliza nombre, categoría, zona y precio
    /// de cada ítem para que UIs y catálogos no dupliquen esa información.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem I { get; private set; }

        readonly Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

        // ---------- catálogo de ítems ----------

        public struct ItemMeta
        {
            public Zona Zona;
            public string Categoria;   // agrupador dentro de la zona
            public string Nombre;      // etiqueta larga
            public string Corto;       // etiqueta de estantería
            public int Precio;         // costo de compra (limpio si Normal, sucio si Turbio)
            public bool Comprable;     // aparece en el catálogo del PC
            public ItemMeta(Zona z, string cat, string nom, string corto, int precio, bool comprable = true)
            { Zona = z; Categoria = cat; Nombre = nom; Corto = corto; Precio = precio; Comprable = comprable; }
        }

        static readonly Dictionary<ItemType, ItemMeta> Meta = new Dictionary<ItemType, ItemMeta>
        {
            // ---- almacén normal ----
            { ItemType.Aceite,    new ItemMeta(Zona.Normal, "Consumibles", "Aceite",              "ACEITE",     30) },
            { ItemType.Gasolina,  new ItemMeta(Zona.Normal, "Consumibles", "Bidón de gasolina",   "GASOLINA",   25) },
            { ItemType.Pintura,   new ItemMeta(Zona.Normal, "Consumibles", "Lata de pintura",     "PINTURA",    90) },
            { ItemType.Llanta,    new ItemMeta(Zona.Normal, "Repuestos",   "Llanta",              "LLANTAS",    60) },
            { ItemType.Repuesto,  new ItemMeta(Zona.Normal, "Repuestos",   "Repuesto genérico",   "REPUESTOS",  80) },
            { ItemType.Filtro,    new ItemMeta(Zona.Normal, "Repuestos",   "Filtro",              "FILTRO",     40) },
            { ItemType.Bateria,   new ItemMeta(Zona.Normal, "Repuestos",   "Batería",             "BATERÍA",   120) },
            { ItemType.Pastillas, new ItemMeta(Zona.Normal, "Repuestos",   "Pastillas de freno",  "PASTILLAS",  70) },
            // ---- almacén turbio (compra con dinero sucio) ----
            { ItemType.PiezaIlegal,  new ItemMeta(Zona.Turbio, "Piezas calientes", "Pieza caliente",           "P. CALIENTE", 150) },
            { ItemType.PlacasBlanco, new ItemMeta(Zona.Turbio, "Piezas calientes", "Juego de placas en blanco","PLACAS",      200) },
            { ItemType.KitVIN,       new ItemMeta(Zona.Turbio, "Piezas calientes", "Kit de VIN",               "KIT VIN",     180) },
            { ItemType.Desbloqueo,   new ItemMeta(Zona.Turbio, "Dispositivos",     "Desbloqueo de puertas",    "DESBLOQUEO",  260) },
            { ItemType.BujiasPot,    new ItemMeta(Zona.Turbio, "Dispositivos",     "Bujías potenciadas",       "BUJÍAS+",     140) },
            { ItemType.ECUTrucada,   new ItemMeta(Zona.Turbio, "Dispositivos",     "ECU trucada",              "ECU",         320) },
        };

        public static ItemMeta MetaOf(ItemType t) =>
            Meta.TryGetValue(t, out var m) ? m : new ItemMeta(Zona.Normal, "Otros", t.ToString(), t.ToString(), 0, false);

        public static Zona ZonaOf(ItemType t) => MetaOf(t).Zona;
        public static string Label(ItemType t) => MetaOf(t).Nombre;
        public static string ShortLabel(ItemType t) => MetaOf(t).Corto;
        public static string Categoria(ItemType t) => MetaOf(t).Categoria;
        public static int PriceOf(ItemType t) => MetaOf(t).Precio;

        /// <summary>Ítems de una zona (opcionalmente solo los comprables), agrupados por categoría
        /// y luego por índice del enum: así los catálogos y estanterías salen ordenados de forma estable.</summary>
        public static IEnumerable<ItemType> ItemsOf(Zona z, bool soloComprables = false) =>
            Meta.Where(kv => kv.Value.Zona == z && (!soloComprables || kv.Value.Comprable))
                .OrderBy(kv => kv.Value.Categoria).ThenBy(kv => (int)kv.Key)
                .Select(kv => kv.Key);

        // ---------- estado ----------

        void Awake()
        {
            I = this;
            // stock inicial para poder trabajar el día 1
            items[ItemType.Aceite] = 4;
            items[ItemType.Llanta] = 8;
            items[ItemType.Repuesto] = 4;
            items[ItemType.Gasolina] = 3;
        }

        // capacidad por zona: la estantería del garaje (almacen1) y la bodega del patio (almacen2)
        public int CapacityNormal => 30 + (UpgradeSystem.I != null && UpgradeSystem.I.Tiene("almacen1") ? 30 : 0);
        public int CapacityTurbio => 15 + (UpgradeSystem.I != null && UpgradeSystem.I.Tiene("almacen2") ? 60 : 0);
        public int CapacityFor(Zona z) => z == Zona.Turbio ? CapacityTurbio : CapacityNormal;

        public int UsedNormal => items.Where(kv => ZonaOf(kv.Key) == Zona.Normal).Sum(kv => kv.Value);
        public int UsedTurbio => items.Where(kv => ZonaOf(kv.Key) == Zona.Turbio).Sum(kv => kv.Value);
        public int UsedFor(Zona z) => z == Zona.Turbio ? UsedTurbio : UsedNormal;

        // compatibilidad: totales combinados (algunos textos los usan)
        public int Capacity => CapacityNormal + CapacityTurbio;
        public int Used => items.Values.Sum();

        public int Count(ItemType t) => items.TryGetValue(t, out var c) ? c : 0;

        /// <summary>¿Caben n unidades de t en su zona?</summary>
        public bool CanFit(ItemType t, int n) => UsedFor(ZonaOf(t)) + n <= CapacityFor(ZonaOf(t));

        public bool Add(ItemType t, int n)
        {
            if (!CanFit(t, n))
            {
                var z = ZonaOf(t);
                GameEvents.Notify((z == Zona.Turbio ? "Bodega del patio" : "Almacén") + " llena (" +
                    UsedFor(z) + "/" + CapacityFor(z) + "). Mejora el almacenamiento.");
                return false;
            }
            items[t] = Count(t) + n;
            GameEvents.OnMetasChanged?.Invoke();
            return true;
        }

        public bool Remove(ItemType t, int n)
        {
            if (Count(t) < n) return false;
            items[t] -= n;
            GameEvents.OnMetasChanged?.Invoke();
            return true;
        }

        public bool Has(List<PartReq> parts) => parts == null || parts.All(p => Count(p.Type) >= p.Count);
        public bool Has(ItemType t, int n) => Count(t) >= n;

        public bool Consume(List<PartReq> parts)
        {
            if (!Has(parts)) return false;
            if (parts != null) foreach (var p in parts) items[p.Type] = Count(p.Type) - p.Count;
            GameEvents.OnMetasChanged?.Invoke();
            return true;
        }

        public string MissingText(List<PartReq> parts)
        {
            if (parts == null) return "";
            var missing = parts.Where(p => Count(p.Type) < p.Count)
                .Select(p => Label(p.Type) + " x" + (p.Count - Count(p.Type)));
            return string.Join(", ", missing);
        }

        /// <summary>Espejo de red: el host manda los conteos completos (índice = ItemType).</summary>
        public void SetAll(int[] counts)
        {
            bool changed = false;
            for (int i = 0; i < counts.Length; i++)
            {
                var t = (ItemType)i;
                if (Count(t) != counts[i]) { items[t] = counts[i]; changed = true; }
            }
            if (changed) GameEvents.OnMetasChanged?.Invoke();
        }

        // ---- guardado ----
        public List<int> SaveTypes() => items.Keys.Select(k => (int)k).ToList();
        public List<int> SaveCounts() => items.Values.ToList();
        public void LoadFrom(List<int> types, List<int> counts)
        {
            items.Clear();
            for (int i = 0; i < types.Count && i < counts.Count; i++)
                items[(ItemType)types[i]] = counts[i];
        }
    }
}
