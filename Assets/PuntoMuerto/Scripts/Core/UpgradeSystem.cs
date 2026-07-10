using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    [System.Serializable]
    public class UpgradeDef
    {
        public string Id;
        public string Nombre;
        public int Costo;
        public int Tier;              // 1 básico, 2 medio, 3 avanzado
        public float RepMinima;       // algunas requieren Meta B (GDD 2.4)
        public string Descripcion;

        public UpgradeDef(string id, string nombre, int costo, int tier, float repMin, string desc)
        { Id = id; Nombre = nombre; Costo = costo; Tier = tier; RepMinima = repMin; Descripcion = desc; }
    }

    /// <summary>Meta D — árbol de mejoras del taller (GDD 2.4).</summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem I { get; private set; }

        public HashSet<string> Compradas = new HashSet<string>();

        public static readonly UpgradeDef[] Catalogo = new UpgradeDef[]
        {
            new UpgradeDef("herramientas", "Herramientas mejoradas", 1200, 1, -100f, "Reparaciones 25% más rápidas."),
            new UpgradeDef("cerca1", "Reparar la cerca", 800, 1, -100f, "Menos rendijas: -30% riesgo de espionaje."),
            new UpgradeDef("almacen1", "Estantería de almacén", 1500, 1, -100f, "+30 de capacidad de almacenamiento."),
            new UpgradeDef("bahia4", "Cuarta bahía", 6000, 2, -100f, "Un slot más para carros de clientes."),
            new UpgradeDef("elevador", "Elevador hidráulico", 5000, 2, -100f, "Habilita frenos/suspensión y transmisión."),
            new UpgradeDef("cerca2", "Cerca sólida", 8000, 2, -100f, "-60% riesgo de espionaje."),
            new UpgradeDef("almacen2", "Bodega trasera", 4500, 2, -100f, "+60 de capacidad de almacenamiento."),
            new UpgradeDef("bahia5", "Quinta bahía", 12000, 3, -100f, "Otro slot más: taller a máxima capacidad."),
            new UpgradeDef("pintura", "Equipo de pintura profesional", 15000, 3, -100f, "Habilita repintados (legales e ilegales)."),
            new UpgradeDef("vehiculo", "Vehículo de recolección", 18000, 3, -100f, "Misiones de recolección más rápidas y seguras."),
            new UpgradeDef("caja", "Caja fuerte grande", 12000, 3, 30f, "Guarda más efectivo sucio sin riesgo en allanamientos."),
        };

        void Awake() { I = this; }

        public int TallerTier
        {
            get
            {
                if (Catalogo.Where(u => u.Tier == 3).Any(u => Compradas.Contains(u.Id))) return 3;
                if (Catalogo.Where(u => u.Tier == 2).Any(u => Compradas.Contains(u.Id))) return 2;
                if (Compradas.Count > 0) return 1;
                return 0;
            }
        }

        public bool Tiene(string id) => Compradas.Contains(id);

        public float FenceRiskMultiplier => Tiene("cerca2") ? 0.4f : (Tiene("cerca1") ? 0.7f : 1f);
        public float RepairSpeedMultiplier => Tiene("herramientas") ? 1.25f : 1f;

        public bool Comprar(string id)
        {
            var def = Catalogo.FirstOrDefault(u => u.Id == id);
            if (def == null || Compradas.Contains(id)) return false;
            if (Net.IsClientOnly)
            {
                // el host valida, cobra y replica a todos
                if (GameManager.I.TotalMoney < def.Costo)
                { GameEvents.Notify("No te alcanza: cuesta $" + def.Costo.ToString("N0")); return false; }
                GameSync.RequestUpgrade(id);
                return true;
            }
            if (MetasManager.I.Reputacion < def.RepMinima)
            { GameEvents.Notify("Necesitas +"+ def.RepMinima + " de reputación (proveedor de confianza)."); return false; }
            if (!GameManager.I.Spend(def.Costo))
            { GameEvents.Notify("No te alcanza: cuesta $" + def.Costo.ToString("N0")); return false; }

            InstallLocal(id);
            GameSync.MirrorUpgrade(id);
            return true;
        }

        /// <summary>Instala la mejora sin cobrar (compra local o espejo de red).</summary>
        public void InstallLocal(string id)
        {
            var def = Catalogo.FirstOrDefault(u => u.Id == id);
            if (def == null || Compradas.Contains(id)) return;
            Compradas.Add(id);
            GameEvents.Notify("Mejora instalada: " + def.Nombre + PistaVisual(id));
            ReapplySceneEffects(id);
            GameEvents.OnUpgradesChanged?.Invoke();
            GameEvents.OnMetasChanged?.Invoke();
        }

        static string PistaVisual(string id)
        {
            switch (id)
            {
                case "elevador": return " — míralo en la bahía 1.";
                case "herramientas": return " — gabinete nuevo junto a la pared trasera.";
                case "almacen1": return " — estantería nueva en el garaje.";
                case "almacen2": return " — bodega nueva en el patio.";
                case "pintura": return " — cabina de pintura en el patio.";
                case "vehiculo": return " — el furgón está en el patio.";
                case "caja": return " — caja fuerte en la oficina.";
                case "cerca1": return " — tablones nuevos en la cerca.";
                case "cerca2": return " — la cerca quedó sellada.";
                case "bahia4": return " — el slot 4 quedó despejado.";
                case "bahia5": return " — el slot 5 quedó despejado.";
                default: return "";
            }
        }

        // kits visuales inactivos que el MapBuilder deja bajo Taller/Mejoras
        static readonly Dictionary<string, string> Kits = new Dictionary<string, string>
        {
            { "elevador", "ElevadorKit" }, { "herramientas", "KitHerramientas" },
            { "almacen1", "Almacen1" }, { "almacen2", "Almacen2" },
            { "pintura", "CabinaPintura" }, { "vehiculo", "Furgon" },
            { "caja", "CajaFuerte" }, { "cerca1", "CercaParches" },
        };

        /// <summary>Efectos en escena (también usado al cargar partida).</summary>
        public void ReapplySceneEffects(string id)
        {
            if (id == "bahia4" || id == "bahia5")
            {
                var block = GameObject.Find("Taller/SlotLock" + (id == "bahia4" ? "4" : "5"));
                if (block != null) block.SetActive(false);
            }
            else if (id == "cerca2")
            {
                var holes = GameObject.Find("Taller/CercaRendijas");
                if (holes != null) holes.SetActive(false);
            }
            if (Kits.TryGetValue(id, out var kitName))
            {
                // GameObject.Find no ve objetos inactivos: buscar vía el padre activo
                var root = GameObject.Find("Taller/Mejoras");
                var kit = root != null ? root.transform.Find(kitName) : null;
                if (kit != null) kit.gameObject.SetActive(true);
            }
        }
    }
}
