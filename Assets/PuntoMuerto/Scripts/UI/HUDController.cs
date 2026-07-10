using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>HUD: 4 metas, reloj, dinero, agenda, notificaciones, prompt, ojo de espía.</summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController I;

        Text clockText, moneyText, agendaText, promptText, workText;
        RectTransform deudaFill, repPosFill, repNegFill, fabioFill, tallerFill, workFill, noiseFill;
        GameObject workGroup;
        readonly List<Text> notifications = new List<Text>();
        RectTransform notifRoot;

        Image crosshair;
        RectTransform crosshairRt;

        static string pendingPrompt;
        static bool crosshairActive;
        static float workProgress = -1f; static string workTitle; static float workNoise;

        void Awake() { I = this; }

        void Start()
        {
            var c = UIRoot.I.Canvas.transform;

            // reloj / día (arriba izquierda)
            var clockPanel = UIRoot.CreatePanel(c, "Clock", new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(230f, 84f), UIRoot.PanelBg);
            clockText = UIRoot.CreateText(clockPanel, "Día 1\n07:00", 26, UIRoot.TextColor,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 80f), TextAnchor.MiddleCenter, FontStyle.Bold);

            // dinero (arriba derecha)
            var moneyPanel = UIRoot.CreatePanel(c, "Money", new Vector2(1f, 1f), new Vector2(-18f, -18f), new Vector2(300f, 84f), UIRoot.PanelBg);
            moneyText = UIRoot.CreateText(moneyPanel, "$0", 22, UIRoot.TextColor,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(290f, 80f), TextAnchor.MiddleCenter);

            // metas (abajo izquierda)
            var metas = UIRoot.CreatePanel(c, "Metas", new Vector2(0f, 0f), new Vector2(18f, 18f), new Vector2(400f, 220f), UIRoot.PanelBg);
            UIRoot.CreateText(metas, "DEUDA BANCO", 16, UIRoot.TextColor, new Vector2(0f, 1f), new Vector2(12f, -10f), new Vector2(370f, 20f));
            UIRoot.CreateBar(metas, new Vector2(0f, 1f), new Vector2(12f, -34f), new Vector2(376f, 16f),
                new Color(0.15f, 0.15f, 0.18f), new Color(0.35f, 0.75f, 0.3f), out _).TryGetComponent(out RectTransform _);
            deudaFill = LastFill(metas);
            UIRoot.CreateText(metas, "REPUTACIÓN / CALOR", 16, UIRoot.TextColor, new Vector2(0f, 1f), new Vector2(12f, -58f), new Vector2(370f, 20f));
            // barra bidireccional: mitad izquierda calor (rojo), mitad derecha reputación (azul)
            var repBg = UIRoot.CreatePanel(metas, "RepBg", new Vector2(0f, 1f), new Vector2(12f, -82f), new Vector2(376f, 16f), new Color(0.15f, 0.15f, 0.18f));
            var negGo = new GameObject("Neg", typeof(RectTransform), typeof(Image));
            var nrt = negGo.GetComponent<RectTransform>();
            nrt.SetParent(repBg, false);
            nrt.anchorMin = new Vector2(0.5f, 0f); nrt.anchorMax = new Vector2(0.5f, 1f); nrt.pivot = new Vector2(1f, 0.5f);
            nrt.sizeDelta = new Vector2(188f, 0f);
            negGo.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.15f);
            repNegFill = nrt;
            var posGo = new GameObject("Pos", typeof(RectTransform), typeof(Image));
            var prt = posGo.GetComponent<RectTransform>();
            prt.SetParent(repBg, false);
            prt.anchorMin = new Vector2(0.5f, 0f); prt.anchorMax = new Vector2(0.5f, 1f); prt.pivot = new Vector2(0f, 0.5f);
            prt.sizeDelta = new Vector2(188f, 0f);
            posGo.GetComponent<Image>().color = new Color(0.3f, 0.55f, 0.9f);
            repPosFill = prt;

            UIRoot.CreateText(metas, "FABIO REYES", 16, UIRoot.TextColor, new Vector2(0f, 1f), new Vector2(12f, -106f), new Vector2(370f, 20f));
            UIRoot.CreateBar(metas, new Vector2(0f, 1f), new Vector2(12f, -130f), new Vector2(376f, 16f),
                new Color(0.15f, 0.15f, 0.18f), new Color(0.9f, 0.65f, 0.2f), out _);
            fabioFill = LastFill(metas);
            UIRoot.CreateText(metas, "MEJORA TALLER", 16, UIRoot.TextColor, new Vector2(0f, 1f), new Vector2(12f, -154f), new Vector2(370f, 20f));
            UIRoot.CreateBar(metas, new Vector2(0f, 1f), new Vector2(12f, -178f), new Vector2(376f, 16f),
                new Color(0.15f, 0.15f, 0.18f), new Color(0.4f, 0.75f, 0.8f), out _);
            tallerFill = LastFill(metas);

            // agenda (abajo derecha)
            var agenda = UIRoot.CreatePanel(c, "Agenda", new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(360f, 240f), UIRoot.PanelBg);
            UIRoot.CreateText(agenda, "AGENDA", 18, UIRoot.Accent, new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(340f, 24f), TextAnchor.MiddleCenter, FontStyle.Bold);
            agendaText = UIRoot.CreateText(agenda, "", 16, UIRoot.TextColor, new Vector2(0f, 1f), new Vector2(14f, -38f), new Vector2(330f, 190f));

            // crosshair (centro exacto)
            var chGo = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
            crosshairRt = chGo.GetComponent<RectTransform>();
            crosshairRt.SetParent(c, false);
            crosshairRt.anchorMin = crosshairRt.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRt.sizeDelta = new Vector2(6f, 6f);
            crosshair = chGo.GetComponent<Image>();
            crosshair.color = new Color(1f, 1f, 1f, 0.75f);
            crosshair.raycastTarget = false;

            // prompt de interacción (bajo el crosshair)
            promptText = UIRoot.CreateText(c, "", 24, UIRoot.Accent, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(800f, 34f), TextAnchor.MiddleCenter, FontStyle.Bold);

            // progreso de trabajo + ruido
            workGroup = UIRoot.CreatePanel(c, "Work", new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(460f, 74f), UIRoot.PanelBg).gameObject;
            workText = UIRoot.CreateText(workGroup.transform, "", 17, UIRoot.TextColor, new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(440f, 22f), TextAnchor.MiddleCenter);
            UIRoot.CreateBar(workGroup.transform, new Vector2(0f, 1f), new Vector2(12f, -32f), new Vector2(436f, 14f),
                new Color(0.15f, 0.15f, 0.18f), new Color(0.4f, 0.8f, 0.4f), out _);
            workFill = LastFill(workGroup.transform);
            UIRoot.CreateText(workGroup.transform, "Ruido", 13, new Color(1f, 0.6f, 0.5f), new Vector2(0f, 1f), new Vector2(12f, -50f), new Vector2(60f, 16f));
            UIRoot.CreateBar(workGroup.transform, new Vector2(0f, 1f), new Vector2(64f, -52f), new Vector2(384f, 10f),
                new Color(0.15f, 0.15f, 0.18f), new Color(0.95f, 0.4f, 0.25f), out _);
            noiseFill = LastFill(workGroup.transform);
            workGroup.SetActive(false);

            // (el aviso de espía se quitó: la vigilancia es silenciosa, hay que mirar la cerca)

            // notificaciones (arriba centro)
            var notifGo = new GameObject("Notifs", typeof(RectTransform));
            notifRoot = notifGo.GetComponent<RectTransform>();
            notifRoot.SetParent(c, false);
            notifRoot.anchorMin = new Vector2(0.5f, 1f); notifRoot.anchorMax = new Vector2(0.5f, 1f);
            notifRoot.pivot = new Vector2(0.5f, 1f);
            notifRoot.anchoredPosition = new Vector2(0f, -30f);
            notifRoot.sizeDelta = new Vector2(760f, 300f);

            GameEvents.OnNotification += AddNotification;
        }

        void OnDestroy() { GameEvents.OnNotification -= AddNotification; }

        static RectTransform LastFill(Transform panel)
        {
            // el último BarBg creado, su hijo Fill
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                var ch = panel.GetChild(i);
                if (ch.name == "BarBg" && ch.childCount > 0)
                    return ch.GetChild(0) as RectTransform;
            }
            return null;
        }

        void Update()
        {
            var g = GameManager.I; var m = MetasManager.I;
            if (g == null || m == null) return;

            if (DayNightCycle.I != null)
            {
                string fase = DayNightCycle.I.Phase == GamePhase.Manana ? "Mañana" :
                    DayNightCycle.I.Phase == GamePhase.Tarde ? "Tarde" : "Noche";
                clockText.text = "Día " + g.Day + " — " + fase + "\n" + DayNightCycle.I.ClockText();
            }

            moneyText.text = "Limpio: $" + g.CleanMoney.ToString("N0") + "\nSucio: $" + g.DirtyMoney.ToString("N0");

            UIRoot.SetBar(deudaFill, m.DeudaPct);
            float rep = m.Reputacion;
            repPosFill.localScale = new Vector3(Mathf.Clamp01(rep / 100f), 1f, 1f);
            repNegFill.localScale = new Vector3(Mathf.Clamp01(-rep / 100f), 1f, 1f);
            UIRoot.SetBar(fabioFill, m.Fabio / 100f);
            UIRoot.SetBar(tallerFill, UpgradeSystem.I != null ? UpgradeSystem.I.TallerTier / 3f : 0f);

            // agenda
            if (MissionSystem.I != null)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var mi in MissionSystem.I.Active)
                {
                    string tag = mi.Type == MissionType.ClienteHonesto ? "•" : "◆";
                    sb.Append(tag).Append(' ').Append(mi.Title);
                    if (mi.WorkRequired > 0) sb.Append(" [").Append(Mathf.RoundToInt(mi.Progress01 * 100f)).Append("%]");
                    if (mi.DeadlineDay > 0 && mi.EsIlegal) sb.Append(" (día ").Append(mi.DeadlineDay).Append(')');
                    sb.AppendLine();
                }
                if (ReceptionSystem.I != null && ReceptionSystem.I.Queue.Count > 0)
                    sb.AppendLine("★ " + ReceptionSystem.I.Queue.Count + " cliente(s) esperando en recepción");
                if (DirtyReceptionSystem.I != null && DirtyReceptionSystem.I.Queue.Count > 0)
                    sb.AppendLine("◆ " + DirtyReceptionSystem.I.Queue.Count + " esperando en la ventanilla trasera");
                if (MissionGenerator.I != null && MissionGenerator.I.PhoneRinging)
                    sb.AppendLine("☎ El teléfono está sonando...");
                if (MissionSystem.I.CarryingPickup != null)
                    sb.AppendLine("▣ Llevas un paquete. Al patio del taller.");
                agendaText.text = sb.Length == 0 ? "(sin pendientes)" : sb.ToString();
            }

            promptText.text = pendingPrompt ?? "";

            // crosshair: crece y toma acento cuando apuntas a algo interactuable; oculto con modal
            if (crosshair != null)
            {
                bool visible = !UIRoot.ModalOpen;
                crosshair.enabled = visible;
                if (visible)
                {
                    crosshair.color = crosshairActive ? UIRoot.Accent : new Color(1f, 1f, 1f, 0.75f);
                    crosshairRt.sizeDelta = crosshairActive ? new Vector2(11f, 11f) : new Vector2(6f, 6f);
                }
            }

            // trabajo
            if (workProgress >= 0f)
            {
                workGroup.SetActive(true);
                workText.text = workTitle;
                UIRoot.SetBar(workFill, workProgress);
                UIRoot.SetBar(noiseFill, workNoise);
            }
            else workGroup.SetActive(false);
            workProgress = -1f;

            // limpiar notificaciones viejas
            for (int i = notifications.Count - 1; i >= 0; i--)
                if (notifications[i] == null) notifications.RemoveAt(i);
        }

        public static void SetPrompt(string prompt) { pendingPrompt = prompt; }

        public static void SetCrosshair(bool active) { crosshairActive = active; }

        public static void SetWorkProgress(float progress, string title, float noise)
        { workProgress = progress; workTitle = title; workNoise = noise; }

        void AddNotification(string msg)
        {
            var t = UIRoot.CreateText(notifRoot, msg, 19, Color.white,
                new Vector2(0.5f, 1f), Vector2.zero, new Vector2(740f, 26f), TextAnchor.MiddleCenter, FontStyle.Bold);
            var bg = t.gameObject.AddComponent<Outline>();
            bg.effectColor = Color.black;
            notifications.Add(t);
            // reacomodar
            for (int i = 0; i < notifications.Count; i++)
            {
                var rt = notifications[notifications.Count - 1 - i].rectTransform;
                rt.anchoredPosition = new Vector2(0f, -i * 28f);
            }
            Destroy(t.gameObject, 6f);
        }
    }
}
