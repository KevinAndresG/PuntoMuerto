using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Pizarra de mejoras del taller (Meta D, GDD 2.4).</summary>
    public class UpgradesUI : MonoBehaviour
    {
        RectTransform panel;

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "UpgOverlay",
                new Color(0f, 0f, 0f, 0.5f));
            panel = UIRoot.CreatePanel(overlay, "Upgrades", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(820f, 640f), new Color(0.09f, 0.1f, 0.13f, 0.98f));
            UIRoot.CreateText(panel, "MEJORAS DEL TALLER — Tier actual: " + UpgradeSystem.I.TallerTier,
                26, UIRoot.Accent, new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(780f, 36f),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            float y = 70f;
            foreach (var u in UpgradeSystem.Catalogo)
            {
                bool owned = UpgradeSystem.I.Tiene(u.Id);
                string label = u.Nombre + "  [$" + u.Costo.ToString("N0") + " · Tier " + u.Tier + "]";
                UIRoot.CreateText(panel, label + "\n" + u.Descripcion +
                    (u.RepMinima > -100f ? " (Requiere reputación +" + u.RepMinima + ")" : ""),
                    17, owned ? new Color(0.5f, 0.8f, 0.5f) : UIRoot.TextColor,
                    new Vector2(0f, 1f), new Vector2(24f, -y), new Vector2(560f, 54f));
                if (owned)
                {
                    UIRoot.CreateText(panel, "INSTALADA", 18, new Color(0.5f, 0.8f, 0.5f),
                        new Vector2(1f, 1f), new Vector2(-60f, -y - 10f), new Vector2(150f, 30f), TextAnchor.MiddleCenter);
                }
                else
                {
                    string id = u.Id;
                    UIRoot.CreateButton(panel, "Comprar", new Vector2(1f, 1f), new Vector2(-24f, -y),
                        new Vector2(150f, 44f), () => { UpgradeSystem.I.Comprar(id); Reopen(); }, null, 18);
                }
                y += 62f;
            }

            UIRoot.CreateButton(panel, "Cerrar", new Vector2(0.5f, 0f), new Vector2(0f, 16f),
                new Vector2(220f, 46f), Close);
        }

        void Reopen() { Close(); Open(); }

        void Close()
        {
            if (panel == null) return;
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
        }
    }
}
