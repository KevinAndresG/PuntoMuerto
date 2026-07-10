using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Pantalla de final (GDD 7.1): resumen + "Continuar en Los Alisos".</summary>
    public class EndingUI : MonoBehaviour
    {
        RectTransform panel;

        public void Show(string titulo, string texto, string stats, EndingType tipo)
        {
            if (panel != null) return;
            GameManager.I.SetPaused(true);
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "EndingOverlay",
                new Color(0f, 0f, 0f, 0.92f));
            panel = UIRoot.CreatePanel(overlay, "Ending", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(760f, 640f), new Color(0.07f, 0.08f, 0.1f, 1f));

            UIRoot.CreateText(panel, titulo, 40, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -30f), new Vector2(700f, 54f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIRoot.CreateText(panel, texto, 22, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -110f), new Vector2(660f, 200f), TextAnchor.UpperCenter, FontStyle.Italic);
            UIRoot.CreateText(panel, stats, 19, new Color(0.7f, 0.7f, 0.7f), new Vector2(0.5f, 1f),
                new Vector2(0f, -320f), new Vector2(660f, 220f), TextAnchor.UpperCenter);

            UIRoot.CreateButton(panel, "Continuar en Los Alisos", new Vector2(0.5f, 0f),
                new Vector2(0f, 24f), new Vector2(340f, 56f), () =>
                {
                    UIRoot.PopModal();
                    Destroy(panel.parent.gameObject);
                    panel = null;
                    EndingSystem.I.ContinuarTemporada(tipo);
                });
        }
    }
}
