using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>
    /// PC de la oficina: único punto de compra del taller. Dos catálogos en pestañas —
    /// CATÁLOGO (repuestos legales, dinero limpio, al almacén del garaje) y MERCADO NEGRO
    /// (piezas turbias, dinero sucio, a la bodega del patio; se desbloquea con confianza de Fabio).
    /// </summary>
    public class SupplierUI : MonoBehaviour
    {
        const float FabioParaMercadoNegro = 15f;

        RectTransform overlay;
        RectTransform panel;
        Text estado;
        Zona activa = Zona.Normal;

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "SupOverlay",
                new Color(0f, 0f, 0f, 0.5f));
            Build();
        }

        bool MercadoNegroDisponible =>
            MetasManager.I != null && MetasManager.I.Fabio >= FabioParaMercadoNegro;

        void Build()
        {
            if (overlay == null) return;
            foreach (Transform c in overlay) Destroy(c.gameObject);

            panel = UIRoot.CreatePanel(overlay, "Proveedor", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(780f, 660f), new Color(0.09f, 0.1f, 0.13f, 0.98f));

            bool turbio = activa == Zona.Turbio;
            bool descuento = !turbio && MetasManager.I != null && MetasManager.I.Reputacion >= 60f;

            UIRoot.CreateText(panel, turbio ? "MERCADO NEGRO" : "CATÁLOGO DEL PROVEEDOR" +
                    (descuento ? " — precio de amigo (-20%)" : ""),
                24, turbio ? new Color(0.95f, 0.55f, 0.35f) : UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -16f), new Vector2(740f, 32f), TextAnchor.MiddleCenter, FontStyle.Bold);

            // pestañas
            UIRoot.CreateButton(panel, "CATÁLOGO", new Vector2(0f, 1f), new Vector2(150f, -58f),
                new Vector2(200f, 40f), () => { activa = Zona.Normal; Build(); },
                turbio ? new Color(0.18f, 0.2f, 0.24f) : new Color(0.2f, 0.4f, 0.5f), 18);
            UIRoot.CreateButton(panel, "MERCADO NEGRO", new Vector2(0f, 1f), new Vector2(370f, -58f),
                new Vector2(230f, 40f), () => { activa = Zona.Turbio; Build(); },
                turbio ? new Color(0.45f, 0.28f, 0.15f) : new Color(0.22f, 0.18f, 0.16f), 18);

            estado = UIRoot.CreateText(panel, "", 16, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -104f), new Vector2(740f, 24f), TextAnchor.MiddleCenter);

            if (turbio && !MercadoNegroDisponible)
            {
                UIRoot.CreateText(panel,
                    "Todavía no tienes contactos para esto.\nGánate la confianza de Fabio (≥" +
                    Mathf.RoundToInt(FabioParaMercadoNegro) + ") y este catálogo se abrirá.",
                    20, new Color(0.85f, 0.6f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(640f, 80f), TextAnchor.MiddleCenter);
            }
            else
            {
                BuildLista(turbio);
            }

            UIRoot.CreateButton(panel, "Cerrar", new Vector2(0.5f, 0f), new Vector2(0f, 20f),
                new Vector2(260f, 48f), Close);
            Refresh();
        }

        void BuildLista(bool turbio)
        {
            var zona = turbio ? Zona.Turbio : Zona.Normal;
            float y = 140f;
            string catActual = null;
            foreach (var item in InventorySystem.ItemsOf(zona, soloComprables: true))
            {
                var it = item;
                var meta = InventorySystem.MetaOf(it);
                if (meta.Categoria != catActual)
                {
                    catActual = meta.Categoria;
                    UIRoot.CreateText(panel, "— " + catActual + " —", 17, new Color(0.7f, 0.72f, 0.78f),
                        new Vector2(0f, 1f), new Vector2(30f, -y), new Vector2(400f, 24f));
                    y += 30f;
                }

                int precio = Precio(it);
                bool juego = it == ItemType.Llanta;
                int n1 = juego ? 4 : 1, n2 = juego ? 8 : 5, n3 = juego ? 12 : 10;
                UIRoot.CreateText(panel, meta.Nombre + " — $" + precio + " c/u" +
                    (juego ? " (juego 4: $" + (precio * 4) + ")" : "") + "   (tienes " +
                    InventorySystem.I.Count(it) + ")", 18, UIRoot.TextColor,
                    new Vector2(0f, 1f), new Vector2(30f, -y - 6f), new Vector2(440f, 28f));
                UIRoot.CreateButton(panel, "x" + n1, new Vector2(1f, 1f), new Vector2(-250f, -y),
                    new Vector2(88f, 42f), () => Comprar(it, n1), null, 18);
                UIRoot.CreateButton(panel, "x" + n2, new Vector2(1f, 1f), new Vector2(-152f, -y),
                    new Vector2(88f, 42f), () => Comprar(it, n2), null, 18);
                UIRoot.CreateButton(panel, "x" + n3, new Vector2(1f, 1f), new Vector2(-54f, -y),
                    new Vector2(88f, 42f), () => Comprar(it, n3), null, 18);
                y += 54f;
            }
        }

        int Precio(ItemType t)
        {
            int p = InventorySystem.PriceOf(t);
            // el descuento de reputación solo aplica al proveedor legal
            if (InventorySystem.ZonaOf(t) == Zona.Normal && MetasManager.I != null && MetasManager.I.Reputacion >= 60f)
                p = Mathf.RoundToInt(p * 0.8f);
            return p;
        }

        void Comprar(ItemType t, int n)
        {
            bool turbio = InventorySystem.ZonaOf(t) == Zona.Turbio;
            int costo = Precio(t) * n;

            if (!InventorySystem.I.CanFit(t, n))
            {
                var z = InventorySystem.ZonaOf(t);
                GameEvents.Notify((turbio ? "Bodega del patio" : "Almacén") + " sin espacio (" +
                    InventorySystem.I.UsedFor(z) + "/" + InventorySystem.I.CapacityFor(z) + ").");
                Refresh(); return;
            }
            if (turbio && GameManager.I.DirtyMoney < costo)
            {
                GameEvents.Notify("Esto se paga con dinero sucio y no te alcanza: $" + costo.ToString("N0"));
                Refresh(); return;
            }
            if (!turbio && GameManager.I.TotalMoney < costo)
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
            GameManager.I.Spend(costo, preferDirty: turbio);
            InventorySystem.I.Add(t, n);
            GameEvents.Notify("Compraste " + InventorySystem.Label(t) + " x" + n + " por $" + costo.ToString("N0") +
                (turbio ? " (sucio)" : ""));
            Build(); // reconstruye para actualizar los contadores "tienes X"
        }

        void Refresh()
        {
            if (estado == null) return;
            var inv = InventorySystem.I;
            estado.text = "Limpio: $" + GameManager.I.CleanMoney.ToString("N0") +
                "   ·   Sucio: $" + GameManager.I.DirtyMoney.ToString("N0") +
                "      Almacén " + inv.UsedNormal + "/" + inv.CapacityNormal +
                "   ·   Bodega " + inv.UsedTurbio + "/" + inv.CapacityTurbio;
        }

        void Close()
        {
            if (overlay == null) return;
            UIRoot.PopModal();
            Destroy(overlay.gameObject);
            overlay = null;
            panel = null;
        }
    }
}
