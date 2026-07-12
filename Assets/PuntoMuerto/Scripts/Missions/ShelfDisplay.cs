using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>
    /// Estantería/bodega del taller. Arma en runtime, sobre el marco horneado por MapBuilder, un
    /// almacén ORDENADO: una bandeja por ítem, con una pieza representativa (llantas, latas, cajas,
    /// placas...) y su conteo en vivo. Muestra el stock de su ZONA (Normal = garaje, Turbio = patio).
    /// Todo se cuelga como hijo (localPosition) para heredar la rotación del estante y mirar al frente.
    /// Solo display — la compra se centraliza en el PC de la oficina.
    /// </summary>
    public class ShelfDisplay : MonoBehaviour
    {
        public Zona Zona = Zona.Normal;

        ItemType[] items;
        TextMesh[] labels;
        GameObject[] props;
        float refreshAt;

        void Start()
        {
            items = InventorySystem.ItemsOf(Zona).ToArray();
            labels = new TextMesh[items.Length];
            props = new GameObject[items.Length];
            bool turbio = Zona == Zona.Turbio;

            // encabezado
            var header = MakeText(new Vector3(0f, 2.34f, -0.63f), 0.052f);
            header.text = turbio ? "BODEGA" : "ALMACÉN";
            header.fontStyle = FontStyle.Bold;
            header.color = turbio ? new Color(0.95f, 0.6f, 0.42f) : new Color(0.96f, 0.9f, 0.72f);

            // grilla de bandejas (3 columnas, filas según cantidad)
            int cols = 3;
            int rows = Mathf.CeilToInt(items.Length / (float)cols);
            for (int i = 0; i < items.Length; i++)
            {
                int c = i % cols, r = i / cols;
                float x = (c - 1) * 1.2f;
                float y = rows > 1 ? Mathf.Lerp(1.9f, 0.74f, r / (float)(rows - 1)) : 1.3f;

                // bandeja/compartimento oscuro que agrupa la pieza
                Solid(PrimitiveType.Cube, "Bin_" + items[i], new Vector3(x, y - 0.02f, -0.34f),
                    new Vector3(0.9f, 0.3f, 0.34f), Quaternion.identity,
                    turbio ? new Color(0.12f, 0.12f, 0.13f) : new Color(0.27f, 0.19f, 0.11f));

                // la pieza va DENTRO de la bandeja y la etiqueta pegada a su borde inferior, para que
                // cada rótulo pertenezca claramente a su bandeja (antes quedaba a medio camino entre dos).
                props[i] = BuildProp(items[i], new Vector3(x, y + 0.05f, -0.5f));
                labels[i] = MakeText(new Vector3(x, y - 0.14f, -0.63f), 0.016f);
            }
        }

        void Update()
        {
            if (Time.time < refreshAt || InventorySystem.I == null || labels == null) return;
            refreshAt = Time.time + 0.5f;
            bool turbio = Zona == Zona.Turbio;
            for (int i = 0; i < items.Length; i++)
            {
                int n = InventorySystem.I.Count(items[i]);
                labels[i].text = InventorySystem.ShortLabel(items[i]) + "\nx" + n;
                labels[i].color = n > 0
                    ? (turbio ? new Color(0.95f, 0.62f, 0.45f) : new Color(0.96f, 0.93f, 0.8f))
                    : new Color(0.5f, 0.5f, 0.52f);
                if (props[i] != null) props[i].SetActive(n > 0); // bandeja vacía = sin pieza
            }
        }

        // ---------- piezas representativas ----------

        GameObject BuildProp(ItemType t, Vector3 localPos)
        {
            var root = new GameObject("Prop_" + t);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = Quaternion.identity;
            var p = root.transform;

            switch (t)
            {
                case ItemType.Llanta:
                    Tire(p, new Vector3(-0.05f, -0.02f, 0f));
                    Tire(p, new Vector3(0.13f, -0.02f, 0.03f));
                    break;
                case ItemType.Aceite:
                    Can(p, new Color(0.85f, 0.7f, 0.2f), -0.09f);
                    Can(p, new Color(0.8f, 0.62f, 0.18f), 0.09f);
                    break;
                case ItemType.Gasolina:
                    Can(p, new Color(0.8f, 0.22f, 0.16f), 0f);
                    break;
                case ItemType.Pintura:
                    Can(p, new Color(0.3f, 0.6f, 0.85f), -0.09f);
                    Can(p, new Color(0.75f, 0.35f, 0.65f), 0.09f);
                    break;
                case ItemType.PlacasBlanco:
                    Plate(p, new Color(0.86f, 0.83f, 0.6f));
                    break;
                default:
                    Crate(p, ColorFor(t), Vector3.zero);
                    if (t == ItemType.Repuesto || t == ItemType.PiezaIlegal)
                        Crate(p, ColorFor(t) * 0.82f, new Vector3(0.03f, 0.15f, 0.04f));
                    break;
            }
            return root;
        }

        void Tire(Transform parent, Vector3 off) =>
            Solid(PrimitiveType.Cylinder, "Llanta", off, new Vector3(0.24f, 0.055f, 0.24f),
                Quaternion.Euler(90f, 0f, 0f), new Color(0.07f, 0.07f, 0.08f)).transform.SetParent(parent, false);

        void Can(Transform parent, Color col, float dx) =>
            Solid(PrimitiveType.Cylinder, "Lata", new Vector3(dx, 0f, 0f), new Vector3(0.14f, 0.15f, 0.14f),
                Quaternion.identity, col).transform.SetParent(parent, false);

        void Crate(Transform parent, Color col, Vector3 off) =>
            Solid(PrimitiveType.Cube, "Caja", off, new Vector3(0.32f, 0.24f, 0.28f),
                Quaternion.identity, col).transform.SetParent(parent, false);

        void Plate(Transform parent, Color col) =>
            Solid(PrimitiveType.Cube, "Placa", Vector3.zero, new Vector3(0.05f, 0.24f, 0.42f),
                Quaternion.identity, col).transform.SetParent(parent, false);

        // ---------- helpers ----------

        TextMesh MakeText(Vector3 localPos, float scale)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * scale;
            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 56;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            TextStyle.Apply(tm);
            return tm;
        }

        GameObject Solid(PrimitiveType type, string name, Vector3 localPos, Vector3 scale,
            Quaternion localRot, Color color)
        {
            var g = GameObject.CreatePrimitive(type);
            g.name = name;
            Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(transform, false);
            g.transform.localPosition = localPos;
            g.transform.localRotation = localRot;
            g.transform.localScale = scale;
            var r = g.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.SetColor("_BaseColor", color);
            return g;
        }

        static Color ColorFor(ItemType t)
        {
            switch (t)
            {
                case ItemType.Repuesto: return new Color(0.55f, 0.55f, 0.58f);
                case ItemType.Filtro: return new Color(0.72f, 0.56f, 0.26f);
                case ItemType.Bateria: return new Color(0.16f, 0.36f, 0.6f);
                case ItemType.Pastillas: return new Color(0.62f, 0.26f, 0.2f);
                case ItemType.PiezaIlegal: return new Color(0.42f, 0.26f, 0.14f);
                case ItemType.KitVIN: return new Color(0.5f, 0.5f, 0.55f);
                case ItemType.Desbloqueo: return new Color(0.22f, 0.22f, 0.24f);
                case ItemType.BujiasPot: return new Color(0.72f, 0.62f, 0.22f);
                case ItemType.ECUTrucada: return new Color(0.26f, 0.32f, 0.38f);
                default: return new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }
}
