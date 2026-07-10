using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>UI de recepción: aceptar/rechazar clientes viendo trabajo, pago y repuestos.</summary>
    public class ReceptionUI : MonoBehaviour
    {
        RectTransform panel;

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "RecOverlay",
                new Color(0f, 0f, 0f, 0.5f));
            panel = UIRoot.CreatePanel(overlay, "Recepcion", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(860f, 620f), new Color(0.09f, 0.1f, 0.13f, 0.98f));

            int libres = BayManager.I.UnlockedSlots - Enumerable.Range(0, BayManager.I.UnlockedSlots)
                .Count(i => BayManager.I.FreeSlotIndex() != i && false); // solo informativo abajo
            UIRoot.CreateText(panel, "RECEPCIÓN — bahías: " + BayManager.I.UnlockedSlots + " (libres: " +
                FreeCount() + ")", 26, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -18f), new Vector2(820f, 36f), TextAnchor.MiddleCenter, FontStyle.Bold);

            var q = ReceptionSystem.I.Queue;
            if (q.Count == 0)
            {
                UIRoot.CreateText(panel, "Nadie espera en este momento.\nLos clientes llegan durante la mañana y la tarde.",
                    20, UIRoot.TextColor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 60f), TextAnchor.MiddleCenter);
            }

            float y = 70f;
            foreach (var w in q.ToList())
            {
                var wc = w;
                if (w.M.EsVenta)
                {
                    UIRoot.CreateText(panel,
                        w.M.ClientName + " — " + w.M.Title + "\nPide: $" + w.M.Pay.ToString("N0") +
                        "   (proveedor: $" + (InventorySystem.PriceOf(w.M.VentaItem) * w.M.VentaCount).ToString("N0") + ")",
                        18, new Color(0.7f, 0.9f, 0.7f),
                        new Vector2(0f, 1f), new Vector2(26f, -y), new Vector2(560f, 54f));
                    UIRoot.CreateButton(panel, "Comprar", new Vector2(1f, 1f), new Vector2(-160f, -y),
                        new Vector2(130f, 46f), () => { Close(); ReceptionSystem.I.Accept(wc); },
                        new Color(0.2f, 0.35f, 0.4f), 18);
                }
                else
                {
                    string parts = w.M.Parts.Count == 0 ? "ninguno"
                        : string.Join(", ", w.M.Parts.Select(p => InventorySystem.Label(p.Type) + " x" + p.Count));
                    bool tienes = InventorySystem.I.Has(w.M.Parts);
                    UIRoot.CreateText(panel,
                        w.M.ClientName + " — " + w.M.Title + "\nPaga: $" + w.M.Pay.ToString("N0") +
                        "   Repuestos: " + parts + (tienes ? "" : "  (¡NO los tienes!)"),
                        18, tienes ? UIRoot.TextColor : new Color(1f, 0.75f, 0.5f),
                        new Vector2(0f, 1f), new Vector2(26f, -y), new Vector2(560f, 54f));
                    UIRoot.CreateButton(panel, "Aceptar", new Vector2(1f, 1f), new Vector2(-160f, -y),
                        new Vector2(130f, 46f), () => { Close(); ReceptionSystem.I.Accept(wc); },
                        new Color(0.2f, 0.4f, 0.2f), 18);
                }
                UIRoot.CreateButton(panel, "Rechazar", new Vector2(1f, 1f), new Vector2(-24f, -y),
                    new Vector2(130f, 46f), () => { Close(); ReceptionSystem.I.Reject(wc); },
                    new Color(0.4f, 0.2f, 0.2f), 18);
                y += 70f;
            }

            UIRoot.CreateText(panel,
                "Inventario: " + ResumenInventario() + "   (almacén " + InventorySystem.I.Used + "/" + InventorySystem.I.Capacity + ")",
                16, new Color(0.7f, 0.7f, 0.72f), new Vector2(0.5f, 0f), new Vector2(0f, 80f),
                new Vector2(820f, 24f), TextAnchor.MiddleCenter);

            UIRoot.CreateButton(panel, "Cerrar", new Vector2(0.5f, 0f), new Vector2(0f, 20f),
                new Vector2(220f, 48f), Close);
        }

        int FreeCount()
        {
            int free = 0;
            for (int i = 0; i < BayManager.I.UnlockedSlots; i++)
            {
                // FreeSlotIndex devuelve el primero libre; contamos probando ocupación indirecta
            }
            // conteo simple: slots desbloqueados menos misiones con Slot asignado
            int occupied = MissionSystem.I.Active.Count(m => m.Slot >= 0);
            free = Mathf.Max(0, BayManager.I.UnlockedSlots - occupied);
            return free;
        }

        string ResumenInventario()
        {
            var inv = InventorySystem.I;
            return "Aceite " + inv.Count(ItemType.Aceite) +
                " · Llantas " + inv.Count(ItemType.Llanta) +
                " · Repuestos " + inv.Count(ItemType.Repuesto) +
                " · Pintura " + inv.Count(ItemType.Pintura) +
                " · Gasolina " + inv.Count(ItemType.Gasolina) +
                " · P.ilegales " + inv.Count(ItemType.PiezaIlegal);
        }

        void Close()
        {
            if (panel == null) return;
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
        }
    }
}
