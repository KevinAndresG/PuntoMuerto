using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>Minijuego de lijado (GDD 4.1): pasa el mouse sosteniendo clic sobre las manchas.</summary>
    public class SandingMinigameUI : MonoBehaviour
    {
        RectTransform panel;
        readonly List<Image> spots = new List<Image>();
        Mission mission;
        RepairStation station;
        Text progressText;

        public void Open(Mission m, RepairStation st)
        {
            if (panel != null) return;
            mission = m;
            station = st;
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "SandOverlay",
                new Color(0f, 0f, 0f, 0.7f));
            panel = UIRoot.CreatePanel(overlay, "Sanding", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(900f, 620f), new Color(0.12f, 0.13f, 0.16f, 1f));
            UIRoot.CreateText(panel, "LIJADO — mantén clic y frota las manchas de óxido", 24, UIRoot.Accent,
                new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(860f, 34f), TextAnchor.MiddleCenter, FontStyle.Bold);
            progressText = UIRoot.CreateText(panel, "", 20, UIRoot.TextColor, new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(400f, 28f), TextAnchor.MiddleCenter);

            // panel de "carrocería"
            var body = UIRoot.CreatePanel(panel, "Carroceria", new Vector2(0.5f, 0.5f), new Vector2(0f, -10f),
                new Vector2(820f, 480f), new Color(0.45f, 0.15f, 0.12f));

            spots.Clear();
            int n = Random.Range(6, 10);
            for (int i = 0; i < n; i++)
            {
                var spot = UIRoot.CreatePanel(body, "Oxido", new Vector2(0.5f, 0.5f),
                    new Vector2(Random.Range(-360f, 360f), Random.Range(-200f, 200f)),
                    new Vector2(Random.Range(60f, 120f), Random.Range(60f, 120f)),
                    new Color(0.3f, 0.22f, 0.12f, 1f));
                spots.Add(spot.GetComponent<Image>());
            }
        }

        void Update()
        {
            if (panel == null) return;
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector2 screenPos = mouse.position.ReadValue();
                foreach (var s in spots)
                {
                    if (s == null || s.color.a <= 0f) continue;
                    if (RectTransformUtility.RectangleContainsScreenPoint(s.rectTransform, screenPos, null))
                    {
                        var c = s.color;
                        c.a -= Time.unscaledDeltaTime * 1.1f;
                        s.color = c;
                    }
                }
            }

            int clean = 0;
            foreach (var s in spots) if (s == null || s.color.a <= 0.05f) clean++;
            progressText.text = clean + " / " + spots.Count + " zonas lijadas";

            if (clean >= spots.Count) Finish();
        }

        void Finish()
        {
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
            if (mission != null)
            {
                if (Net.IsClientOnly)
                {
                    // el host completa; la señal llega por el espejo
                    GameSync.SendWork(mission.Id, Mathf.Max(mission.WorkRequired, 1f));
                }
                else
                {
                    mission.WorkDone = Mathf.Max(mission.WorkRequired, 1f);
                    MissionSystem.I.Complete(mission);
                }
                // el carro queda "listo" en su slot: se despacha con E (el cliente sube y se va)
            }
            if (station != null && station.CurrentMission == mission) station.CurrentMission = null;
            mission = null;
            station = null;
        }
    }
}
