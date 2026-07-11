using UnityEngine;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>Libro Real vs Oficial: lavar dinero, pagar deuda, pedir préstamos (GDD 4.5/4.6).</summary>
    public class LedgerUI : MonoBehaviour
    {
        RectTransform panel;
        Text estado;

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "LedgerOverlay",
                new Color(0f, 0f, 0f, 0.5f));
            panel = UIRoot.CreatePanel(overlay, "Ledger", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(760f, 600f), new Color(0.09f, 0.1f, 0.13f, 0.98f));
            UIRoot.CreateText(panel, "LIBRO REAL / LIBRO OFICIAL", 28, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(700f, 40f), TextAnchor.MiddleCenter, FontStyle.Bold);

            estado = UIRoot.CreateText(panel, "", 19, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -70f), new Vector2(680f, 130f), TextAnchor.UpperCenter);

            float y = 210f;
            UIRoot.CreateText(panel, "— Disfrazar dinero sucio como ingresos legítimos —", 18, UIRoot.TextColor,
                new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(680f, 24f), TextAnchor.MiddleCenter);
            y += 32f;
            float yy1 = y;
            UIRoot.CreateButton(panel, "Lavar $500", new Vector2(0.5f, 1f), new Vector2(-230f, -yy1), new Vector2(200f, 44f),
                () => { Lavar(500); });
            UIRoot.CreateButton(panel, "Lavar $2.000", new Vector2(0.5f, 1f), new Vector2(0f, -yy1), new Vector2(200f, 44f),
                () => { Lavar(2000); });
            UIRoot.CreateButton(panel, "Lavar todo", new Vector2(0.5f, 1f), new Vector2(230f, -yy1), new Vector2(200f, 44f),
                () => { Lavar(GameManager.I.DirtyMoney); });
            y += 60f;

            UIRoot.CreateText(panel, "— Pagar deuda del banco (solo dinero limpio a la vista) —", 18, UIRoot.TextColor,
                new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(680f, 24f), TextAnchor.MiddleCenter);
            y += 32f;
            float yy2 = y;
            UIRoot.CreateButton(panel, "Pagar $1.000", new Vector2(0.5f, 1f), new Vector2(-230f, -yy2), new Vector2(200f, 44f),
                () => { Pagar(1000); });
            UIRoot.CreateButton(panel, "Pagar $5.000", new Vector2(0.5f, 1f), new Vector2(0f, -yy2), new Vector2(200f, 44f),
                () => { Pagar(5000); });
            UIRoot.CreateButton(panel, "Pagar $20.000", new Vector2(0.5f, 1f), new Vector2(230f, -yy2), new Vector2(200f, 44f),
                () => { Pagar(20000); });
            y += 60f;

            UIRoot.CreateText(panel, "— Préstamos (riesgo real: cuota semanal) —", 18, UIRoot.TextColor,
                new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(680f, 24f), TextAnchor.MiddleCenter);
            y += 32f;
            float yy3 = y;
            UIRoot.CreateButton(panel, "Pequeño: $8.000 / 10 sem", new Vector2(0.5f, 1f), new Vector2(-160f, -yy3), new Vector2(300f, 44f),
                () => { Prestamo(false); });
            UIRoot.CreateButton(panel, "Mediano: $20.000 / 14 sem", new Vector2(0.5f, 1f), new Vector2(160f, -yy3), new Vector2(300f, 44f),
                () => { Prestamo(true); });

            UIRoot.CreateButton(panel, "Cerrar el libro", new Vector2(0.5f, 0f), new Vector2(0f, 20f),
                new Vector2(240f, 48f), Close);

            // el estado puede cambiar por red (host ejecuta y sincroniza): refrescar en vivo
            GameEvents.OnMetasChanged += Refresh;
            Refresh();
        }

        // en multijugador el cliente pide y el host ejecuta; el estado vuelve por el sync
        void Lavar(int monto)
        {
            if (Net.IsClientOnly) GameSync.RequestLavar(monto);
            else LedgerSystem.I.Lavar(monto);
            Refresh();
        }

        void Pagar(int monto)
        {
            if (Net.IsClientOnly) GameSync.RequestPagarDeuda(monto);
            else MetasManager.I.PagarDeuda(monto);
            Refresh();
        }

        void Prestamo(bool mediano)
        {
            if (Net.IsClientOnly) GameSync.RequestPrestamo(mediano);
            else BankSystem.I.PedirPrestamo(mediano);
            Refresh();
        }

        void Refresh()
        {
            if (estado == null) return;
            var g = GameManager.I; var m = MetasManager.I; var l = LedgerSystem.I;
            string prestamos = "";
            foreach (var p in BankSystem.I.Prestamos)
                prestamos += "\n  · Préstamo: cuota $" + p.Cuota.ToString("N0") + ", " + p.SemanasRestantes + " semanas";
            estado.text =
                "Dinero limpio: $" + g.CleanMoney.ToString("N0") + "    Dinero sucio: $" + g.DirtyMoney.ToString("N0") +
                "\nDeuda restante: $" + (MetasManager.DeudaTotal - m.DeudaPagada).ToString("N0") +
                " (" + Mathf.RoundToInt(m.DeudaPct * 100f) + "% pagado)" +
                "\nRiesgo de auditoría: " + Mathf.RoundToInt(l.RiesgoAuditoria) + "%" +
                (prestamos.Length > 0 ? "\nPréstamos activos:" + prestamos : "\nSin préstamos activos.");
        }

        void Close()
        {
            if (panel == null) return;
            GameEvents.OnMetasChanged -= Refresh;
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
        }
    }
}
