using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Libro Real vs Libro Oficial (GDD 4.5): lavar dinero sucio con riesgo de auditoría.</summary>
    public class LedgerSystem : MonoBehaviour
    {
        public static LedgerSystem I { get; private set; }

        [Range(0f, 100f)] public float RiesgoAuditoria;
        public int TotalLavado;

        void Awake() { I = this; }
        void OnEnable() { GameEvents.OnDayStart += OnDayStart; }
        void OnDisable() { GameEvents.OnDayStart -= OnDayStart; }

        /// <summary>Convierte dinero sucio en limpio. Sobre-declarar sin clientela real sube el riesgo.</summary>
        public void Lavar(int monto)
        {
            monto = Mathf.Min(monto, GameManager.I.DirtyMoney);
            if (monto <= 0) return;
            GameManager.I.DirtyMoney -= monto;
            GameManager.I.CleanMoney += monto;
            TotalLavado += monto;

            int respaldo = Mathf.Max(200, GameManager.I.DayCleanEarned);
            float exceso = Mathf.Max(0f, (float)monto / (respaldo * 2f) - 1f);
            RiesgoAuditoria = Mathf.Clamp(RiesgoAuditoria + exceso * 12f + monto / 1000f * 0.5f, 0f, 100f);
            GameEvents.OnMetasChanged?.Invoke();
        }

        void OnDayStart(int day)
        {
            // el riesgo decae despacio; auditoría aleatoria si está alto
            RiesgoAuditoria = Mathf.Max(0f, RiesgoAuditoria - 2f);
            if (RiesgoAuditoria > 35f && Random.value < RiesgoAuditoria / 250f)
            {
                int multa = Mathf.RoundToInt(TotalLavado * 0.25f + 500);
                GameManager.I.Spend(multa, true);
                MetasManager.I.CambiarReputacion(-10f, "Auditoría sorpresa: encontraron inconsistencias. Multa $" + multa.ToString("N0"));
                RiesgoAuditoria = 5f;
            }
        }
    }
}
