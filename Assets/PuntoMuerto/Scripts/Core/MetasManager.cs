using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Las 4 metas paralelas del GDD (sección 2) + leverage (6.1).</summary>
    public class MetasManager : MonoBehaviour
    {
        public static MetasManager I { get; private set; }

        public const int DeudaTotal = 100000;

        [Header("Meta A — Deuda")]
        public int DeudaPagada;

        [Header("Meta B — Reputación/Calor (-100..100)")]
        public float Reputacion;

        [Header("Meta C — Fabio (0..100)")]
        public float Fabio = 30f;

        [Header("Vía del Poder")]
        public int Leverage;              // piezas 0..5
        public int ReportesAFabio;
        public int DiasEnCritico;         // rep <= -90 sostenida
        public int EvidenciaFalsa;        // ítems para "incriminar"

        public float DeudaPct => (float)DeudaPagada / DeudaTotal;
        public bool PrestamosPequenos => DeudaPagada >= 25000;
        public bool PrestamosMedianos => DeudaPagada >= 50000;
        public bool LibreDeBanco => DeudaPagada >= DeudaTotal;

        void Awake() { I = this; }
        void OnEnable() { GameEvents.OnDayStart += OnDayStart; }
        void OnDisable() { GameEvents.OnDayStart -= OnDayStart; }

        void OnDayStart(int day)
        {
            if (!Net.IsAuthority) return; // el host decide el final; llega por espejo de red
            // Umbral crítico -90 sostenido 3 días => final "Caída" (GDD 2.2 / 7.2)
            if (Reputacion <= -90f)
            {
                DiasEnCritico++;
                if (DiasEnCritico >= 3 && EndingSystem.I != null)
                    EndingSystem.I.Trigger(EndingType.Caida);
                else
                    GameEvents.Notify("El pueblo está al límite. Baja el Calor o todo se derrumba (" + DiasEnCritico + "/3).");
            }
            else DiasEnCritico = 0;
        }

        public void PagarDeuda(int monto)
        {
            monto = Mathf.Min(monto, DeudaTotal - DeudaPagada);
            if (monto <= 0) { GameEvents.Notify("La deuda ya está saldada."); return; }
            if (!GameManager.I.Spend(monto)) { GameEvents.Notify("No tienes suficiente dinero."); return; }
            int prev = DeudaPagada;
            DeudaPagada += monto;
            GameEvents.Notify("Pagaste $" + monto.ToString("N0") + " al banco. Restante: $" + (DeudaTotal - DeudaPagada).ToString("N0"));
            if (prev < 25000 && DeudaPagada >= 25000) GameEvents.Notify("Banco: préstamos pequeños disponibles ($8.000).");
            if (prev < 50000 && DeudaPagada >= 50000) GameEvents.Notify("Banco: préstamos medianos disponibles ($20.000).");
            if (prev < DeudaTotal && DeudaPagada >= DeudaTotal)
                GameEvents.Notify("¡DEUDA SALDADA! Insignia 'Libre de Banco'. Puedes retirarte cuando quieras (teléfono).");
            GameEvents.OnMetasChanged?.Invoke();
        }

        public void CambiarReputacion(float delta, string razon = null)
        {
            float prev = Reputacion;
            Reputacion = Mathf.Clamp(Reputacion + delta, -100f, 100f);
            if (!string.IsNullOrEmpty(razon) && Mathf.Abs(delta) >= 3f)
                GameEvents.Notify(razon + " (" + (delta > 0 ? "+" : "") + delta.ToString("0") + " reputación)");
            CheckRepThreshold(prev, 30f, "El pueblo confía: llegan clientes de mayor valor.");
            CheckRepThreshold(prev, 60f, "Proveedor de confianza desbloqueado. El pueblo te respalda.");
            CheckRepThresholdDown(prev, -20f, "El patrullero pasa más seguido frente al taller...");
            CheckRepThresholdDown(prev, -40f, "Riesgo de inspecciones aleatorias activo.");
            CheckRepThresholdDown(prev, -70f, "Peligro: riesgo real de allanamiento nocturno.");
            CheckRepThresholdDown(prev, -90f, "UMBRAL CRÍTICO. Tres días así y todo termina.");
            GameEvents.OnMetasChanged?.Invoke();
        }

        void CheckRepThreshold(float prev, float t, string msg)
        { if (prev < t && Reputacion >= t) GameEvents.Notify(msg); }

        void CheckRepThresholdDown(float prev, float t, string msg)
        { if (prev > t && Reputacion <= t) GameEvents.Notify(msg); }

        public void CambiarFabio(float delta, string razon = null)
        {
            float prev = Fabio;
            Fabio = Mathf.Clamp(Fabio + delta, 0f, 100f);
            if (!string.IsNullOrEmpty(razon))
                GameEvents.Notify(razon + " (Fabio " + (delta > 0 ? "+" : "") + delta.ToString("0") + ")");
            if (prev < 70f && Fabio >= 70f)
                GameEvents.Notify("Fabio confía plenamente en ti. Se abren posibilidades...");
            if (Fabio < 20f && prev >= 20f)
                GameEvents.Notify("Fabio está perdiendo la paciencia contigo. Cuidado.");
            GameEvents.OnMetasChanged?.Invoke();
        }

        public void AgregarLeverage()
        {
            if (Leverage >= 5) return;
            Leverage++;
            GameEvents.Notify("Te quedaste con una copia... Leverage: " + Leverage + "/5. Estás construyendo algo.");
            GameEvents.OnMetasChanged?.Invoke();
        }

        /// <summary>Consecuencia de "reportar a Fabio" (GDD 2.2 y 4.4.4).</summary>
        public void RegistrarReporteAFabio()
        {
            ReportesAFabio++;
            float delta = -10f - 3f * (ReportesAFabio - 1);
            CambiarReputacion(delta, "El pueblo empieza a atar cabos");
            if (NPCManager.I != null)
                NPCManager.I.GlobalSuspicionFloor = ReportesAFabio * 3f;
        }

        public bool PuedeConfrontar => Fabio >= 70f && Leverage >= 3;
        public bool PuedeTraicion => Leverage >= 4;
    }
}
