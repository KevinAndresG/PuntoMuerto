using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Estantería del taller: muestra el stock de cada ítem y abre el catálogo para comprar rápido.</summary>
    public class ShelfDisplay : MonoBehaviour, IInteractable
    {
        static readonly ItemType[] Items =
        {
            ItemType.Aceite, ItemType.Llanta, ItemType.Repuesto,
            ItemType.Pintura, ItemType.Gasolina, ItemType.PiezaIlegal
        };

        TextMesh[] labels;
        float refreshAt;

        public string Prompt => "Estantería: ver stock / comprar repuestos";
        public bool CanInteract => true;

        void Start()
        {
            // etiquetas generadas en runtime (nada que serializar en escena)
            labels = new TextMesh[Items.Length];
            for (int i = 0; i < Items.Length; i++)
            {
                var go = new GameObject("Label_" + Items[i]);
                go.transform.SetParent(transform, false);
                // dos columnas x tres filas sobre el frente de la estantería
                float col = i % 2 == 0 ? -0.75f : 0.75f;
                float row = 1.9f - (i / 2) * 0.62f;
                go.transform.localPosition = new Vector3(col, row, -0.62f);
                // hereda el -90° del padre (frente local -z → +x): sin voltear, el texto se lee derecho.
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * 0.035f;
                var tm = go.AddComponent<TextMesh>();
                tm.fontSize = 56;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = new Color(0.95f, 0.92f, 0.8f);
                TextStyle.Apply(tm);
                labels[i] = tm;
            }
        }

        void Update()
        {
            if (Time.time < refreshAt || InventorySystem.I == null) return;
            refreshAt = Time.time + 0.5f;
            for (int i = 0; i < Items.Length; i++)
            {
                string name = ShortLabel(Items[i]);
                labels[i].text = name + "\n" + InventorySystem.I.Count(Items[i]);
                labels[i].color = Items[i] == ItemType.PiezaIlegal
                    ? new Color(0.9f, 0.5f, 0.4f) : new Color(0.95f, 0.92f, 0.8f);
            }
        }

        static string ShortLabel(ItemType t)
        {
            switch (t)
            {
                case ItemType.Aceite: return "ACEITE";
                case ItemType.Llanta: return "LLANTAS";
                case ItemType.Repuesto: return "REPUESTOS";
                case ItemType.Pintura: return "PINTURA";
                case ItemType.Gasolina: return "GASOLINA";
                default: return "P. ILEGAL";
            }
        }

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<SupplierUI>();
            if (ui != null) ui.Open();
        }
    }
}
