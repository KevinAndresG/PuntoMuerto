using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>
    /// Estantería/bodega: muestra en vivo el stock de su ZONA (Normal = repuestos del garaje,
    /// Turbio = piezas del patio). Solo display — la compra está centralizada en el PC de la oficina.
    /// </summary>
    public class ShelfDisplay : MonoBehaviour
    {
        public Zona Zona = Zona.Normal;

        ItemType[] items;
        TextMesh[] labels;
        float refreshAt;

        void Start()
        {
            items = InventorySystem.ItemsOf(Zona).ToArray();
            labels = new TextMesh[items.Length];
            int rows = Mathf.CeilToInt(items.Length / 2f);
            for (int i = 0; i < items.Length; i++)
            {
                var go = new GameObject("Label_" + items[i]);
                go.transform.SetParent(transform, false);
                // dos columnas; filas de arriba hacia abajo, repartidas en el alto de la estantería
                float col = i % 2 == 0 ? -0.75f : 0.75f;
                float row = 2.05f - (i / 2) * (2.4f / Mathf.Max(1, rows));
                go.transform.localPosition = new Vector3(col, row, -0.62f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * 0.033f;
                var tm = go.AddComponent<TextMesh>();
                tm.fontSize = 56;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                TextStyle.Apply(tm);
                labels[i] = tm;
            }
        }

        void Update()
        {
            if (Time.time < refreshAt || InventorySystem.I == null || labels == null) return;
            refreshAt = Time.time + 0.5f;
            bool turbio = Zona == Zona.Turbio;
            for (int i = 0; i < items.Length; i++)
            {
                labels[i].text = InventorySystem.ShortLabel(items[i]) + "\n" + InventorySystem.I.Count(items[i]);
                labels[i].color = turbio ? new Color(0.92f, 0.55f, 0.42f) : new Color(0.95f, 0.92f, 0.8f);
            }
        }
    }
}
