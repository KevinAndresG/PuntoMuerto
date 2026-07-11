using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PuntoMuerto
{
    /// <summary>Menú de pausa (Esc).</summary>
    public class PauseMenu : MonoBehaviour
    {
        RectTransform panel;

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
            if (panel == null && UIRoot.ModalOpen) return; // otro modal abierto: Esc no pausa
            if (panel == null && UIRoot.LastModalClosedFrame == Time.frameCount) return; // Esc acaba de cerrar un modal
            if (panel == null) Open(); else Close();
        }

        void Open()
        {
            UIRoot.PushModal();
            GameManager.I.SetPaused(true);
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "PauseOverlay",
                new Color(0f, 0f, 0f, 0.65f));
            panel = UIRoot.CreatePanel(overlay, "Pause", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(440f, 420f), new Color(0.09f, 0.1f, 0.13f, 0.98f));
            UIRoot.CreateText(panel, "PAUSA", 34, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -26f), new Vector2(400f, 44f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIRoot.CreateText(panel, "PUNTO MUERTO — Los Alisos", 17, UIRoot.TextColor, new Vector2(0.5f, 1f),
                new Vector2(0f, -70f), new Vector2(400f, 24f), TextAnchor.MiddleCenter);

            UIRoot.CreateButton(panel, "Reanudar", new Vector2(0.5f, 1f), new Vector2(0f, -110f),
                new Vector2(320f, 48f), Close);
            UIRoot.CreateButton(panel, "Guardar partida", new Vector2(0.5f, 1f), new Vector2(0f, -166f),
                new Vector2(320f, 48f), () => SaveSystem.Save());
            UIRoot.CreateButton(panel, "Controles", new Vector2(0.5f, 1f), new Vector2(0f, -222f),
                new Vector2(320f, 48f), () =>
                {
                    ModalUI.Show("Controles",
                        "WASD — moverte · Shift — correr · Espacio — saltar\nE — interactuar / mantener para trabajar\n" +
                        "Mouse — cámara · Rueda — zoom · Esc — pausa\n\n" +
                        "Recepción: atiende clientes y acepta carros.\nOficina: teléfono, PC (repuestos), libro, pizarra y catre.",
                        "Entendido", null);
                });
            UIRoot.CreateButton(panel, "Guardar y salir al menú", new Vector2(0.5f, 1f), new Vector2(0f, -278f),
                new Vector2(320f, 48f), () =>
                {
                    SaveSystem.Save();
                    Time.timeScale = 1f;
                    if (Unity.Netcode.NetworkManager.Singleton != null)
                        Unity.Netcode.NetworkManager.Singleton.Shutdown();
                    SceneManager.LoadScene("MainMenu");
                });
            UIRoot.CreateButton(panel, "Salir del juego", new Vector2(0.5f, 1f), new Vector2(0f, -334f),
                new Vector2(320f, 48f), () => { SaveSystem.Save(); Application.Quit(); });
        }

        void Close()
        {
            if (panel == null) return;
            UIRoot.PopModal();
            Destroy(panel.parent.gameObject);
            panel = null;
            GameManager.I.SetPaused(false);
        }
    }
}
