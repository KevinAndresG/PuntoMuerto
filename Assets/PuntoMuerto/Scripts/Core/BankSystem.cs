using System.Collections.Generic;
using UnityEngine;

namespace PuntoMuerto
{
    [System.Serializable]
    public class Loan
    {
        public int Principal;
        public int Cuota;
        public int SemanasRestantes;
        public int CuotasSeguidasAtrasadas;
    }

    /// <summary>Préstamos del banco (GDD 4.6) y cuotas semanales.</summary>
    public class BankSystem : MonoBehaviour
    {
        public static BankSystem I { get; private set; }

        public List<Loan> Prestamos = new List<Loan>();

        void Awake() { I = this; }
        void OnEnable() { GameEvents.OnDayStart += OnDayStart; }
        void OnDisable() { GameEvents.OnDayStart -= OnDayStart; }

        void OnDayStart(int day)
        {
            if (!Net.IsAuthority) return; // el host cobra; el cliente recibe el espejo
            // cuotas cada 7 días
            if (day % 7 != 1 || day == 1) return;
            for (int i = Prestamos.Count - 1; i >= 0; i--)
            {
                var p = Prestamos[i];
                if (GameManager.I.CleanMoney >= p.Cuota)
                {
                    GameManager.I.CleanMoney -= p.Cuota;
                    p.SemanasRestantes--;
                    p.CuotasSeguidasAtrasadas = 0;
                    GameEvents.Notify("Cuota del préstamo pagada: $" + p.Cuota.ToString("N0") +
                        " (" + p.SemanasRestantes + " semanas restantes)");
                    if (p.SemanasRestantes <= 0)
                    {
                        Prestamos.RemoveAt(i);
                        GameEvents.Notify("Préstamo saldado con el banco.");
                    }
                }
                else
                {
                    p.CuotasSeguidasAtrasadas++;
                    if (p.CuotasSeguidasAtrasadas == 1)
                    {
                        MetasManager.I.CambiarReputacion(-5f, "Carta del banco: cuota atrasada");
                    }
                    else
                    {
                        MetasManager.I.CambiarReputacion(-15f, "El banco envía un tasador al taller");
                    }
                }
            }
            GameEvents.OnMetasChanged?.Invoke();
        }

        public bool PedirPrestamo(bool mediano)
        {
            var m = MetasManager.I;
            if (mediano && !m.PrestamosMedianos) { GameEvents.Notify("Necesitas 50% de la deuda pagada."); return false; }
            if (!mediano && !m.PrestamosPequenos) { GameEvents.Notify("Necesitas 25% de la deuda pagada."); return false; }

            float interes = mediano ? 0.12f : 0.18f;
            if (m.LibreDeBanco) interes *= 0.8f; // insignia Libre de Banco
            int monto = mediano ? 20000 : 8000;
            int semanas = mediano ? 14 : 10;
            int cuota = Mathf.CeilToInt(monto * (1f + interes) / semanas);

            Prestamos.Add(new Loan { Principal = monto, Cuota = cuota, SemanasRestantes = semanas });
            GameManager.I.AddMoney(monto, false);
            GameEvents.Notify("Préstamo aprobado: $" + monto.ToString("N0") + ". Cuota semanal: $" + cuota.ToString("N0"));
            return true;
        }
    }
}
