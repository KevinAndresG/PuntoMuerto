using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace PuntoMuerto
{
    /// <summary>Canvas raíz + fábrica de controles uGUI construidos por código.</summary>
    public class UIRoot : MonoBehaviour
    {
        public static UIRoot I;
        public Canvas Canvas;

        static int modalCount;
        public static bool ModalOpen => modalCount > 0;
        public static void PushModal() { modalCount++; }
        public static void PopModal() { modalCount = Mathf.Max(0, modalCount - 1); LastModalClosedFrame = Time.frameCount; }

        /// <summary>Frame en que se cerró el último modal: evita que un mismo Esc que cierra
        /// un modal abra el menú de pausa en el mismo frame.</summary>
        public static int LastModalClosedFrame = -1;

        public static Font DefaultFont;

        public static readonly Color PanelBg = new Color(0.07f, 0.08f, 0.1f, 0.88f);
        public static readonly Color Accent = new Color(1f, 0.55f, 0.15f);
        public static readonly Color TextColor = new Color(0.92f, 0.9f, 0.85f);

        void Awake()
        {
            I = this;
            modalCount = 0;
            DefaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Canvas = GetComponent<Canvas>();
            if (Canvas == null)
            {
                Canvas = gameObject.AddComponent<Canvas>();
                Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                gameObject.AddComponent<GraphicRaycaster>();
            }

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }
        }

        // ---------- fábrica ----------

        public static RectTransform CreatePanel(Transform parent, string name, Vector2 anchor,
            Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        public static RectTransform CreateFullscreenPanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        public static Text CreateText(Transform parent, string content, int size, Color color,
            Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta,
            TextAnchor alignment = TextAnchor.UpperLeft, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = DefaultFont;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = alignment;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Button CreateButton(Transform parent, string label, Vector2 anchor,
            Vector2 anchoredPos, Vector2 size, UnityAction onClick, Color? bg = null, int fontSize = 22)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = bg ?? new Color(0.18f, 0.2f, 0.25f, 0.95f);
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 0.7f, 0.3f);
            colors.pressedColor = new Color(0.8f, 0.45f, 0.1f);
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);
            var txt = CreateText(go.transform, label, fontSize, TextColor,
                new Vector2(0.5f, 0.5f), Vector2.zero, size, TextAnchor.MiddleCenter);
            txt.raycastTarget = false;
            return btn;
        }

        /// <summary>Barra: fondo + relleno. Devuelve el RectTransform del relleno (escala X = valor).</summary>
        public static RectTransform CreateBar(Transform parent, Vector2 anchor, Vector2 pos,
            Vector2 size, Color bgColor, Color fillColor, out Image fillImage)
        {
            var bg = CreatePanel(parent, "BarBg", anchor, pos, size, bgColor);
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var frt = fillGo.GetComponent<RectTransform>();
            frt.SetParent(bg, false);
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(0f, 1f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.anchoredPosition = Vector2.zero;
            frt.sizeDelta = new Vector2(size.x, 0f);
            fillImage = fillGo.GetComponent<Image>();
            fillImage.color = fillColor;
            frt.localScale = new Vector3(0.5f, 1f, 1f);
            return frt;
        }

        public static void SetBar(RectTransform fill, float value01)
        {
            if (fill != null) fill.localScale = new Vector3(Mathf.Clamp01(value01), 1f, 1f);
        }
    }
}
