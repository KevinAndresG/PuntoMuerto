using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Diálogo con NPC: manejo del sistema de sospecha (GDD 4.4).</summary>
    public class DialogueUI : MonoBehaviour
    {
        RectTransform panel;
        NPCController npc;

        public void Open(NPCController target)
        {
            if (panel != null) Close();
            npc = target;

            // si es el espía activo, confrontarlo directamente resuelve el evento
            if (FenceSpySystem.I != null && FenceSpySystem.I.ActiveSpy == npc)
            {
                npc.ResumeAfterDialogue(); // ConfrontSpy necesita el agente activo
                FenceSpySystem.I.ConfrontSpy();
                return;
            }

            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "DialogOverlay",
                new Color(0f, 0f, 0f, 0.45f));
            panel = UIRoot.CreatePanel(overlay, "Dialog", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(680f, 460f), new Color(0.1f, 0.11f, 0.14f, 0.98f));

            var s = npc.Suspicion;
            UIRoot.CreateText(panel, npc.NpcName, 30, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(640f, 40f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIRoot.CreateText(panel, "Estado: " + s.StateLabel + "  (sospecha " + s.Sospecha.ToString("0") + "/100)",
                18, UIRoot.TextColor, new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(640f, 26f), TextAnchor.MiddleCenter);

            string linea;
            switch (s.CurrentState)
            {
                case SuspicionState.Alerta:
                    linea = "\"Oye... ¿ese auto de anoche no me suena de algún lado?\""; break;
                case SuspicionState.SospechaAlta:
                    linea = "\"¿Qué es exactamente lo que hacen en las noches ahí atrás?\""; break;
                case SuspicionState.Investigando:
                    linea = "\"Yo sé lo que vi. Y no soy el único que se está dando cuenta.\""; break;
                default:
                    linea = "\"¡Buenas! ¿Cómo va el taller? Mi carro suena raro, cualquier día te lo llevo.\""; break;
            }
            UIRoot.CreateText(panel, linea, 21, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -100f), new Vector2(600f, 70f), TextAnchor.UpperCenter, FontStyle.Italic);

            float y = 190f;
            var st = s.CurrentState;

            if (st == SuspicionState.Normal)
            {
                AddOption("Charlar un rato", ref y, () =>
                {
                    GameEvents.Notify(npc.NpcName + " parece tranquilo.");
                });
            }
            if (st == SuspicionState.Alerta || st == SuspicionState.SospechaAlta)
            {
                string pct = st == SuspicionState.Alerta ? "80%" : "45%";
                AddOption("Despistar / dar una explicación (" + pct + ")", ref y, () =>
                {
                    bool ok = s.TryDespistar();
                    GameEvents.Notify(ok ? npc.NpcName + " se tragó la explicación. Todo en orden."
                                         : npc.NpcName + " no te creyó nada. Quedaste peor.");
                });
            }
            if (st == SuspicionState.SospechaAlta || st == SuspicionState.Investigando)
            {
                int costo = s.CostoSoborno;
                AddOption("Sobornar ($" + costo.ToString("N0") + ")", ref y, () =>
                {
                    if (!GameManager.I.Spend(costo, true)) { GameEvents.Notify("No tienes ese dinero."); return; }
                    bool ok = s.TrySobornar(costo);
                    GameEvents.Notify(ok ? npc.NpcName + " tomó el dinero. \"Yo no vi nada.\" Pero ahora sabe que ocultas algo."
                                         : npc.NpcName + " rechazó el dinero, ofendido. Esto se ve mal.");
                });
                if (MetasManager.I.EvidenciaFalsa > 0)
                {
                    AddOption("Incriminar a otro (evidencia falsa: " + MetasManager.I.EvidenciaFalsa + ")", ref y, () =>
                    {
                        s.TryIncriminar();
                        GameEvents.Notify("Plantaste la duda. " + npc.NpcName + " ahora mira hacia otro lado.");
                    });
                }
            }
            if (st == SuspicionState.Investigando)
            {
                AddOption("Reportar a Fabio...", ref y, () =>
                {
                    ModalUI.Show("¿Estás seguro?",
                        "Le dirás a Fabio que " + npc.NpcName + " está haciendo demasiadas preguntas.\n\nNunca preguntas qué pasa después. Nunca te lo dicen.",
                        "Hacer la llamada", () => s.ReportarAFabio(),
                        "No. Todavía no.", null);
                });
            }
            AddOption("Despedirte", ref y, null);
        }

        void AddOption(string label, ref float y, UnityEngine.Events.UnityAction action)
        {
            float yy = y;
            UIRoot.CreateButton(panel, label, new Vector2(0.5f, 1f), new Vector2(0f, -yy),
                new Vector2(560f, 46f), () => { Close(); action?.Invoke(); }, null, 19);
            y += 54f;
        }

        void Close()
        {
            if (panel == null) return;
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
            if (npc != null) npc.ResumeAfterDialogue();
        }
    }
}
