using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Almacén del taller: repuestos, consumibles y piezas ilegales.</summary>
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem I { get; private set; }

        readonly Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

        void Awake()
        {
            I = this;
            // stock inicial para poder trabajar el día 1
            items[ItemType.Aceite] = 4;
            items[ItemType.Llanta] = 8;
            items[ItemType.Repuesto] = 4;
            items[ItemType.Gasolina] = 3;
        }

        public int Capacity => 30
            + (UpgradeSystem.I != null && UpgradeSystem.I.Tiene("almacen1") ? 30 : 0)
            + (UpgradeSystem.I != null && UpgradeSystem.I.Tiene("almacen2") ? 60 : 0);

        public int Used => items.Values.Sum();
        public int Count(ItemType t) => items.TryGetValue(t, out var c) ? c : 0;

        public bool Add(ItemType t, int n)
        {
            if (Used + n > Capacity)
            {
                GameEvents.Notify("Almacén lleno (" + Used + "/" + Capacity + "). Mejora el almacenamiento.");
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

        public static string Label(ItemType t)
        {
            switch (t)
            {
                case ItemType.Aceite: return "Aceite";
                case ItemType.Llanta: return "Llanta";
                case ItemType.Repuesto: return "Repuesto";
                case ItemType.Pintura: return "Lata de pintura";
                case ItemType.Gasolina: return "Bidón de gasolina";
                default: return "Pieza ilegal";
            }
        }

        public static int PriceOf(ItemType t)
        {
            switch (t)
            {
                case ItemType.Aceite: return 30;
                case ItemType.Llanta: return 60;
                case ItemType.Repuesto: return 80;
                case ItemType.Pintura: return 90;
                case ItemType.Gasolina: return 25;
                default: return 0; // las piezas ilegales no se compran
            }
        }

        /// <summary>Espejo de red: el host manda los conteos completos.</summary>
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
