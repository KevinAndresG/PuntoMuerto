using UnityEngine;
using UnityEngine.Events;

namespace PuntoMuerto
{
    /// <summary>Modal genérico de 1-2 botones.</summary>
    public static class ModalUI
    {
        public static void Show(string title, string body, string opt1, UnityAction act1,
            string opt2 = null, UnityAction act2 = null)
        {
            if (UIRoot.I == null) return;
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "ModalOverlay",
                new Color(0f, 0f, 0f, 0.55f));
            var panel = UIRoot.CreatePanel(overlay, "Modal", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(620f, 360f), new Color(0.1f, 0.11f, 0.14f, 0.98f));
            UIRoot.CreateText(panel, title, 28, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -22f), new Vector2(580f, 40f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIRoot.CreateText(panel, body, 20, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -80f), new Vector2(560f, 180f), TextAnchor.UpperCenter);

            void Close() { UIRoot.PopModal(); Object.Destroy(overlay.gameObject); }

            if (string.IsNullOrEmpty(opt2))
            {
                UIRoot.CreateButton(panel, opt1, new Vector2(0.5f, 0f), new Vector2(0f, 24f),
                    new Vector2(280f, 52f), () => { Close(); act1?.Invoke(); });
            }
            else
            {
                UIRoot.CreateButton(panel, opt1, new Vector2(0.5f, 0f), new Vector2(-150f, 24f),
                    new Vector2(270f, 52f), () => { Close(); act1?.Invoke(); });
                UIRoot.CreateButton(panel, opt2, new Vector2(0.5f, 0f), new Vector2(150f, 24f),
                    new Vector2(270f, 52f), () => { Close(); act2?.Invoke(); });
            }
        }
    }
}
