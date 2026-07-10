using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Ciclo día/noche continuo (24h con wrap en medianoche). Dormir salta a las 7:00.</summary>
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle I { get; private set; }

        [Tooltip("Segundos reales por hora de juego")]
        public float SecondsPerHour = 45f;
        public float Hour = 7f;
        public Light Sun;

        public GamePhase Phase => Hour >= 7f && Hour < 13f ? GamePhase.Manana :
            (Hour >= 13f && Hour < 19f ? GamePhase.Tarde : GamePhase.Noche);
        public bool IsNight => Hour >= 19.5f || Hour < 6f;

        int lastWholeHour = -1;

        static readonly Color DayAmbient = new Color(0.52f, 0.53f, 0.58f);
        static readonly Color DuskAmbient = new Color(0.5f, 0.32f, 0.24f);
        static readonly Color NightAmbient = new Color(0.10f, 0.11f, 0.19f);

        void Awake() { I = this; }

        void Start()
        {
            if (Sun == null)
            {
                var go = GameObject.Find("Sun");
                if (go != null) Sun = go.GetComponent<Light>();
            }
            UpdateLighting();
        }

        void Update()
        {
            if (GameManager.I == null || GameManager.I.GamePaused) return;
            Hour += Time.deltaTime / SecondsPerHour;
            if (Hour >= 24f)
            {
                // medianoche: el día cambia sin pausas ni resúmenes
                Hour -= 24f;
                lastWholeHour = -1;
                if (!Net.IsClientOnly) GameManager.I.AdvanceDay(); // en red, el día lo manda el host
            }
            int wh = Mathf.FloorToInt(Hour);
            if (wh != lastWholeHour)
            {
                lastWholeHour = wh;
                GameEvents.OnHourTick?.Invoke(Hour);
            }
            UpdateLighting();
        }

        public void BeginNewDay()
        {
            Hour = 7f;
            lastWholeHour = -1;
            UpdateLighting();
        }

        /// <summary>Dormir: salta a las 7:00. Antes de medianoche cuenta como día siguiente.</summary>
        public void SleepUntilMorning()
        {
            bool beforeMidnight = Hour >= 7f;
            Hour = 7f;
            lastWholeHour = -1;
            if (beforeMidnight) GameManager.I.AdvanceDay();
            UpdateLighting();
        }

        void UpdateLighting()
        {
            float t = Mathf.InverseLerp(6f, 20f, Hour);
            float sunAngle = Mathf.Lerp(-8f, 188f, t);
            if (Sun != null)
            {
                Sun.transform.rotation = Quaternion.Euler(sunAngle, -35f, 0f);
                float arc = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
                Sun.intensity = arc * 1.3f + 0.02f;
                Sun.color = Color.Lerp(new Color(1f, 0.5f, 0.25f), new Color(1f, 0.96f, 0.88f), Mathf.Clamp01(arc * 1.6f));
            }

            Color amb;
            if (Hour < 5f) amb = NightAmbient;
            else if (Hour < 8f) amb = Color.Lerp(NightAmbient, DayAmbient, (Hour - 5f) / 3f);
            else if (Hour < 17f) amb = DayAmbient;
            else if (Hour < 20f) amb = Color.Lerp(DayAmbient, DuskAmbient, (Hour - 17f) / 3f);
            else amb = Color.Lerp(DuskAmbient, NightAmbient, Mathf.Clamp01((Hour - 20f) / 1.5f));
            RenderSettings.ambientLight = amb;
            if (RenderSettings.fog) RenderSettings.fogColor = amb * 0.85f;
        }

        public string ClockText()
        {
            int h = Mathf.FloorToInt(Hour);
            if (h >= 24) h = 23;
            int m = Mathf.FloorToInt((Hour - Mathf.FloorToInt(Hour)) * 60f);
            return h.ToString("00") + ":" + m.ToString("00");
        }
    }

}
