using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>UI de la ventanilla trasera: aceptar/rechazar encargos sucios.</summary>
    public class DirtyReceptionUI : MonoBehaviour
    {
        RectTransform panel;

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "DirtyRecOverlay",
                new Color(0f, 0f, 0f, 0.55f));
            panel = UIRoot.CreatePanel(overlay, "Ventanilla", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(860f, 560f), new Color(0.11f, 0.09f, 0.09f, 0.98f));

            int libres = BayManager.I != null
                ? Enumerable.Range(0, BayManager.I.TotalPatioSlots).Count(_ => true) - OccupiedPatio()
                : 0;
            UIRoot.CreateText(panel, "VENTANILLA TRASERA — patio: " + BayManager.I.TotalPatioSlots +
                " espacios (libres: " + libres + ")", 26, new Color(0.9f, 0.55f, 0.3f),
                new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(820f, 36f),
                TextAnchor.MiddleCenter, FontStyle.Bold);

            var q = DirtyReceptionSystem.I.Queue;
            if (q.Count == 0)
            {
                UIRoot.CreateText(panel, "Nadie espera en este momento.\nEste negocio se mueve solo, sobre todo de noche.",
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
                        "   (se revenden por teléfono a $250-450 c/u)",
                        18, new Color(0.85f, 0.75f, 0.5f),
                        new Vector2(0f, 1f), new Vector2(26f, -y), new Vector2(560f, 54f));
                    UIRoot.CreateButton(panel, "Comprar", new Vector2(1f, 1f), new Vector2(-160f, -y),
                        new Vector2(130f, 46f), () => { Close(); DirtyReceptionSystem.I.Accept(wc); },
                        new Color(0.35f, 0.3f, 0.15f), 18);
                }
                else
                {
                    UIRoot.CreateText(panel,
                        w.M.ClientName + " — " + w.M.Title + "\nPaga: $" + w.M.Pay.ToString("N0") +
                        " (sucio)   Ruido: " + Mathf.RoundToInt(w.M.Noise * 100f) + "%",
                        18, UIRoot.TextColor,
                        new Vector2(0f, 1f), new Vector2(26f, -y), new Vector2(560f, 54f));
                    UIRoot.CreateButton(panel, "Aceptar", new Vector2(1f, 1f), new Vector2(-160f, -y),
                        new Vector2(130f, 46f), () => { Close(); DirtyReceptionSystem.I.Accept(wc); },
                        new Color(0.35f, 0.25f, 0.15f), 18);
                }
                UIRoot.CreateButton(panel, "Rechazar", new Vector2(1f, 1f), new Vector2(-24f, -y),
                    new Vector2(130f, 46f), () => { Close(); DirtyReceptionSystem.I.Reject(wc); },
                    new Color(0.4f, 0.2f, 0.2f), 18);
                y += 70f;
            }

            UIRoot.CreateText(panel,
                "El trabajo del patio hace ruido: los vecinos pueden asomarse por la cerca.",
                16, new Color(0.7f, 0.6f, 0.55f), new Vector2(0.5f, 0f), new Vector2(0f, 80f),
                new Vector2(820f, 24f), TextAnchor.MiddleCenter);

            UIRoot.CreateButton(panel, "Cerrar", new Vector2(0.5f, 0f), new Vector2(0f, 20f),
                new Vector2(220f, 48f), Close);
        }

        int OccupiedPatio()
        {
            return MissionSystem.I != null ? MissionSystem.I.Active.Count(m => m.Slot >= 100) : 0;
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
