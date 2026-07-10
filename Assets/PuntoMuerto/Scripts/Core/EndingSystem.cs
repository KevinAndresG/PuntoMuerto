using UnityEngine;

namespace PuntoMuerto
{
    public enum EndingType { TallerLimpio, ImperioDeFabio, Caida, TraicionCalculada, NuevoNombre, GolpeFallido }

    /// <summary>Finales (GDD sección 7): pantalla de resumen + Temporada Continua.</summary>
    public class EndingSystem : MonoBehaviour
    {
        public static EndingSystem I { get; private set; }
        public bool EndingShown { get; private set; }

        void Awake() { I = this; }

        public void Trigger(EndingType tipo)
        {
            if (EndingShown) return;
            EndingShown = true;
            var m = MetasManager.I;
            var g = GameManager.I;

            string titulo; string texto;
            switch (tipo)
            {
                case EndingType.TallerLimpio:
                    titulo = "TALLER LIMPIO";
                    texto = m.Reputacion >= 30f
                        ? "Pagaste hasta el último peso. El pueblo entero se acerca a despedirte con respeto: eres de los suyos. El taller queda limpio, y tú también."
                        : "Pagaste la deuda. Nadie hace fiesta — el cierre es silencioso, casi solitario. Pero es tuyo, y es limpio.";
                    break;
                case EndingType.ImperioDeFabio:
                    titulo = "EL IMPERIO DE FABIO";
                    texto = "Aceptaste el ascenso. Ahora eres parte de la estructura: más dinero, más protección, y una correa invisible que jamás podrás cortar.";
                    break;
                case EndingType.Caida:
                    titulo = "LA CAÍDA";
                    texto = "El pueblo dejó de mirar hacia otro lado. Una madrugada, las luces azules rodearon el taller. No hubo a quién llamar: Fabio no contesta números quemados.";
                    break;
                case EndingType.TraicionCalculada:
                    titulo = "TRAICIÓN CALCULADA";
                    texto = "Entregaste todo a las autoridades: documentos, nombres, rutas. Fabio cayó sin saber de dónde vino el golpe. Tú desapareces del mapa con un nombre nuevo.";
                    break;
                case EndingType.NuevoNombre:
                    titulo = "EL NUEVO NOMBRE";
                    texto = "Los Alisos tiene un nuevo dueño silencioso. El teléfono ya no suena para darte órdenes: suena para pedirte permiso.";
                    break;
                default:
                    titulo = "GOLPE FALLIDO";
                    texto = "Confrontaste a Fabio sin las cartas suficientes. Sonrió, colgó el teléfono, y esa fue la última noche tranquila del taller. Nadie volvió a verte por el pueblo.";
                    break;
            }

            string stats =
                "Días jugados: " + g.Day +
                "\nDinero total ganado: $" + g.TotalEarned.ToString("N0") +
                "\nEncargos completados: " + g.MissionsCompleted +
                "\nReputación final: " + m.Reputacion.ToString("0") +
                "\nRelación con Fabio: " + m.Fabio.ToString("0") +
                "\nVeces que 'reportaste' a alguien: " + m.ReportesAFabio;

            if (tipo == EndingType.NuevoNombre) { g.IsJefe = true; }

            var ui = Object.FindFirstObjectByType<EndingUI>();
            if (ui != null) ui.Show(titulo, texto, stats, tipo);
        }

        /// <summary>Botón "Continuar en Los Alisos" (GDD 7.1 → sección 8).</summary>
        public void ContinuarTemporada(EndingType tipo)
        {
            EndingShown = false;
            GameManager.I.TemporadaContinua = true;
            if (tipo == EndingType.Caida)
            {
                // arrancas de nuevo con calor alto pero vivo: el pueblo perdona lento
                MetasManager.I.Reputacion = -50f;
            }
            if (tipo == EndingType.GolpeFallido)
            {
                MetasManager.I.Fabio = 10f;
                MetasManager.I.Leverage = 0;
            }
            GameManager.I.SetPaused(false);
            GameEvents.Notify("Temporada Continua: Los Alisos sigue su curso.");
        }
    }
}
