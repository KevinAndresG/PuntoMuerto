using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Teléfono/agenda de la oficina: ofertas, Vía del Poder, finales (GDD 6/7).</summary>
    public class PhoneUI : MonoBehaviour
    {
        RectTransform panel;

        public void Open()
        {
            if (panel != null) Close();
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "PhoneOverlay",
                new Color(0f, 0f, 0f, 0.5f));
            panel = UIRoot.CreatePanel(overlay, "Phone", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(700f, 560f), new Color(0.09f, 0.1f, 0.13f, 0.98f));
            UIRoot.CreateText(panel, "TELÉFONO / AGENDA", 28, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(660f, 40f), TextAnchor.MiddleCenter, FontStyle.Bold);

            float y = 80f;
            var gen = MissionGenerator.I;
            var m = MetasManager.I;
            var g = GameManager.I;

            if (gen != null && gen.PendingFabioOffer != null)
            {
                var offer = gen.PendingFabioOffer;
                UIRoot.CreateText(panel, "Fabio: \"" + offer.Title + "\"\n" + offer.Description +
                    "\nPaga: $" + offer.Pay.ToString("N0") + " — Entrega: día " + offer.DeadlineDay,
                    19, UIRoot.TextColor, new Vector2(0.5f, 1f), new Vector2(0f, -y),
                    new Vector2(620f, 100f), TextAnchor.UpperCenter);
                y += 108f;
                float yy = y;
                UIRoot.CreateButton(panel, "Aceptar el encargo", new Vector2(0.5f, 1f), new Vector2(-160f, -yy),
                    new Vector2(290f, 48f), () =>
                    {
                        Close();
                        if (Net.IsClientOnly) GameSync.RequestPhoneOffer(true); // el host ejecuta
                        else gen.AcceptOffer();
                    });
                UIRoot.CreateButton(panel, "Rechazar (Fabio -10/-20)", new Vector2(0.5f, 1f), new Vector2(160f, -yy),
                    new Vector2(290f, 48f), () =>
                    {
                        Close();
                        if (Net.IsClientOnly) GameSync.RequestPhoneOffer(false);
                        else gen.RejectOffer();
                    });
                y += 62f;
            }
            else
            {
                UIRoot.CreateText(panel, g.IsJefe ? "Los Alisos espera tus órdenes." : "Nadie llama ahora mismo.",
                    19, UIRoot.TextColor, new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(620f, 30f), TextAnchor.MiddleCenter);
                y += 44f;
            }

            if (g.IsJefe)
            {
                AddBtn("Ordenar un encargo a la red", ref y, () =>
                {
                    ModalUI.Show("Operación propia",
                        "¿Lo trabajas tú en el patio o lo delegas a un mando medio?\n(Delegar: 60% de la paga, sin Calor para ti)",
                        "Trabajarlo yo", () =>
                        {
                            if (Net.IsClientOnly) GameSync.RequestJefeJob(false);
                            else MissionGenerator.I.JefeOrder(false);
                        },
                        "Delegarlo", () =>
                        {
                            if (Net.IsClientOnly) GameSync.RequestJefeJob(true);
                            else MissionGenerator.I.JefeOrder(true);
                        });
                });
                AddBtn("Consolidar tu posición como Jefe (FINAL)", ref y, () =>
                {
                    ModalUI.Show("Consolidar posición",
                        "Cierras esta etapa como el nuevo nombre de Los Alisos. ¿Continuar?",
                        "Sí, consolidar", () => EndingSystem.I.Trigger(EndingType.NuevoNombre),
                        "Todavía no", null);
                });
            }
            else
            {
                if (m.Fabio >= 70f)
                {
                    AddBtn("Aceptar el ascenso de Fabio (FINAL: su imperio)", ref y, () =>
                    {
                        ModalUI.Show("El ascenso",
                            "Fabio te ofrece subir de nivel dentro de SU organización. Para siempre.",
                            "Aceptar el ascenso", () => EndingSystem.I.Trigger(EndingType.ImperioDeFabio),
                            "Rechazar por ahora", null);
                    });
                }
                if (m.Leverage >= 1)
                {
                    string aviso = m.PuedeConfrontar
                        ? "Tienes las cartas: Fabio confía en ti y tu leverage es sólido."
                        : "ADVERTENCIA: no cumples las condiciones (Fabio ≥70 y leverage ≥3). Si fallas, no hay vuelta atrás.";
                    AddBtn("Confrontar a Fabio (leverage " + m.Leverage + "/5)", ref y, () =>
                    {
                        ModalUI.Show("Confrontar a Fabio",
                            aviso + "\n\n¿Presentas tu posición a Fabio? Esto no se puede deshacer.",
                            "Hacer la jugada", () =>
                            {
                                if (Net.IsClientOnly) { GameSync.RequestConfrontar(); return; } // el host resuelve
                                if (m.PuedeConfrontar)
                                {
                                    GameManager.I.IsJefe = true;
                                    ModalUI.Show("Los Alisos es tuyo",
                                        "Fabio escucha, mira los documentos, y por primera vez no tiene una línea preparada.\n\n\"...Está bien. A partir de hoy, el pueblo responde ante ti.\"",
                                        "Continuar", null);
                                    GameEvents.Notify("Ahora tú das las órdenes. El teléfono ya no suena igual.");
                                    GameEvents.OnMetasChanged?.Invoke();
                                }
                                else
                                {
                                    EndingSystem.I.Trigger(EndingType.GolpeFallido);
                                }
                            },
                            "Retirarte", null);
                    });
                }
                if (m.PuedeTraicion)
                {
                    AddBtn("Entregar evidencia a las autoridades (FINAL secreto)", ref y, () =>
                    {
                        ModalUI.Show("Traición calculada",
                            "Todo lo que acumulaste, directo a la fiscalía. Fabio cae. Tú desapareces.",
                            "Entregar todo", () => EndingSystem.I.Trigger(EndingType.TraicionCalculada),
                            "Guardar las cartas", null);
                    });
                }
            }

            int piezas = InventorySystem.I != null ? InventorySystem.I.Count(ItemType.PiezaIlegal) : 0;
            if (piezas > 0)
            {
                int precio = piezas * Random.Range(250, 451);
                AddBtn("Vender lote de piezas ilegales (" + piezas + ") — ~$" + precio.ToString("N0"), ref y, () =>
                {
                    if (Net.IsClientOnly)
                    {
                        // el host ejecuta la venta; dinero e inventario llegan por el sync
                        GameSync.RequestSellPieces();
                        GameEvents.Notify("Vendiendo el lote...");
                        return;
                    }
                    InventorySystem.I.Remove(ItemType.PiezaIlegal, piezas);
                    GameManager.I.AddMoney(precio, true);
                    MetasManager.I.CambiarFabio(2f);
                    GameEvents.Notify("Lote vendido: +$" + precio.ToString("N0") + " (sucio).");
                });
            }

            if (m.LibreDeBanco)
            {
                AddBtn("Cerrar cuentas con el banco y retirarte (FINAL)", ref y, () =>
                {
                    ModalUI.Show("Taller limpio",
                        "La deuda está saldada. Puedes cerrar esta etapa con la frente en alto.",
                        "Retirarte", () => EndingSystem.I.Trigger(EndingType.TallerLimpio),
                        "Seguir un tiempo más", null);
                });
            }

            AddBtn("Colgar", ref y, null);
        }

        void AddBtn(string label, ref float y, UnityEngine.Events.UnityAction action)
        {
            float yy = y;
            UIRoot.CreateButton(panel, label, new Vector2(0.5f, 1f), new Vector2(0f, -yy),
                new Vector2(600f, 48f), () => { Close(); action?.Invoke(); }, null, 19);
            y += 56f;
        }

        void Close()
        {
            if (panel == null) return;
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
        }
    }
}
