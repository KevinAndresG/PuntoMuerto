using UnityEngine;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>Catálogo del proveedor: compra de repuestos y consumibles.</summary>
    public class SupplierUI : MonoBehaviour
    {
        RectTransform panel;
        Text estado;

        static readonly ItemType[] Catalog =
        {
            ItemType.Aceite, ItemType.Llanta, ItemType.Repuesto, ItemType.Pintura, ItemType.Gasolina
        };

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "SupOverlay",
                new Color(0f, 0f, 0f, 0.5f));
            panel = UIRoot.CreatePanel(overlay, "Proveedor", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(760f, 620f), new Color(0.09f, 0.1f, 0.13f, 0.98f));

            bool descuento = MetasManager.I.Reputacion >= 60f;
            UIRoot.CreateText(panel, "CATÁLOGO DEL PROVEEDOR" + (descuento ? " — precio de amigo (-20%)" : ""),
                24, UIRoot.Accent, new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(720f, 34f),
                TextAnchor.MiddleCenter, FontStyle.Bold);
            estado = UIRoot.CreateText(panel, "", 18, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -56f), new Vector2(700f, 30f), TextAnchor.MiddleCenter);

            float y = 110f;
            foreach (var item in Catalog)
            {
                var it = item;
                int precio = Precio(it);
                // las llantas se venden por juegos de 4 (un carro completo)
                bool juego = it == ItemType.Llanta;
                int n1 = juego ? 4 : 1, n2 = juego ? 8 : 5, n3 = juego ? 12 : 10;
                UIRoot.CreateText(panel, InventorySystem.Label(it) + " — $" + precio + " c/u" +
                    (juego ? " (juego de 4: $" + (precio * 4) + ")" : "") + "  (tienes " +
                    InventorySystem.I.Count(it) + ")", 19, UIRoot.TextColor,
                    new Vector2(0f, 1f), new Vector2(30f, -y - 8f), new Vector2(430f, 30f));
                UIRoot.CreateButton(panel, "x" + n1, new Vector2(1f, 1f), new Vector2(-250f, -y),
                    new Vector2(90f, 44f), () => Comprar(it, n1), null, 18);
                UIRoot.CreateButton(panel, "x" + n2, new Vector2(1f, 1f), new Vector2(-150f, -y),
                    new Vector2(90f, 44f), () => Comprar(it, n2), null, 18);
                UIRoot.CreateButton(panel, "x" + n3, new Vector2(1f, 1f), new Vector2(-50f, -y),
                    new Vector2(90f, 44f), () => Comprar(it, n3), null, 18);
                y += 58f;
            }

            UIRoot.CreateButton(panel, "Cerrar catálogo", new Vector2(0.5f, 0f), new Vector2(0f, 20f),
                new Vector2(260f, 48f), Close);
            Refresh();
        }

        int Precio(ItemType t)
        {
            int p = InventorySystem.PriceOf(t);
            if (MetasManager.I.Reputacion >= 60f) p = Mathf.RoundToInt(p * 0.8f);
            return p;
        }

        void Comprar(ItemType t, int n)
        {
            int costo = Precio(t) * n;
            if (InventorySystem.I.Used + n > InventorySystem.I.Capacity)
            {
                GameEvents.Notify("No cabe en el almacén (" + InventorySystem.I.Used + "/" + InventorySystem.I.Capacity + ").");
                Refresh(); return;
            }
            if (GameManager.I.TotalMoney < costo)
            {
                GameEvents.Notify("No te alcanza: $" + costo.ToString("N0"));
                Refresh(); return;
            }
            if (Net.IsClientOnly)
            {
                // el host cobra y agrega; el inventario llega por el sync
                GameSync.RequestBuy(t, n);
                GameEvents.Notify("Pedido enviado: " + InventorySystem.Label(t) + " x" + n);
                Refresh(); return;
            }
            GameManager.I.Spend(costo);
            InventorySystem.I.Add(t, n);
            GameEvents.Notify("Compraste " + InventorySystem.Label(t) + " x" + n + " por $" + costo.ToString("N0"));
            Refresh();
        }

        void Refresh()
        {
            if (estado == null) return;
            estado.text = "Dinero: $" + GameManager.I.CleanMoney.ToString("N0") +
                " (+$" + GameManager.I.DirtyMoney.ToString("N0") + " sucio)   Almacén: " +
                InventorySystem.I.Used + "/" + InventorySystem.I.Capacity;
            // refrescar los contadores re-abriendo sería pesado; el texto principal basta
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
