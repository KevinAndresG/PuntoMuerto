using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>Menú principal: jugar solo, multijugador (host/unirse), opciones, salir.</summary>
    public class MainMenu : MonoBehaviour
    {
        RectTransform root;
        RectTransform mpPanel;
        Text ipText;
        string ip = "127.0.0.1";

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;

            var c = UIRoot.I.Canvas.transform;
            root = UIRoot.CreateFullscreenPanel(c, "Menu", new Color(0.05f, 0.06f, 0.09f, 1f));

            // título estilo neón (como el letrero del taller)
            var title = UIRoot.CreateText(root, "PUNTO MUERTO", 96, new Color(1f, 0.45f, 0.1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(1200f, 120f),
                TextAnchor.MiddleCenter, FontStyle.Bold);
            var glow = title.gameObject.AddComponent<Outline>();
            glow.effectColor = new Color(1f, 0.3f, 0f, 0.6f);
            glow.effectDistance = new Vector2(3f, -3f);
            UIRoot.CreateText(root, "TALLER — LOS ALISOS", 28, new Color(0.9f, 0.85f, 0.7f),
                new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(800f, 40f), TextAnchor.MiddleCenter);
            UIRoot.CreateText(root, "Heredaste un taller en quiebra. Un cartel te ofrece salvarlo.\nDe día reparas. De noche, desapareces autos.",
                20, new Color(0.6f, 0.6f, 0.62f), new Vector2(0.5f, 1f), new Vector2(0f, -290f),
                new Vector2(900f, 60f), TextAnchor.MiddleCenter, FontStyle.Italic);

            float y = 420f;
            if (SaveSystem.HasSave)
            {
                MenuBtn("CONTINUAR", ref y, () =>
                {
                    MPMode.Mode = MPModeKind.None;
                    GameManager.LoadRequested = true;
                    SceneManager.LoadScene("LosAlisos");
                });
            }
            MenuBtn("JUGAR (partida nueva)", ref y, () =>
            {
                MPMode.Mode = MPModeKind.None;
                SaveSystem.Delete();
                SceneManager.LoadScene("LosAlisos");
            });
            MenuBtn("MULTIJUGADOR", ref y, ToggleMP);
            MenuBtn("OPCIONES", ref y, () =>
            {
                ModalUI.Show("Opciones",
                    "Pantalla completa: clic abajo para alternar.\nCalidad gráfica: automática (URP).",
                    Screen.fullScreen ? "Cambiar a ventana" : "Cambiar a pantalla completa",
                    () => Screen.fullScreen = !Screen.fullScreen,
                    "Cerrar", null);
            });
            MenuBtn("SALIR", ref y, () => Application.Quit());

            UIRoot.CreateText(root, "v0.1 — prototipo construido desde el GDD v3.0", 15,
                new Color(0.4f, 0.4f, 0.42f), new Vector2(0.5f, 0f), new Vector2(0f, 16f),
                new Vector2(600f, 22f), TextAnchor.MiddleCenter);
        }

        void MenuBtn(string label, ref float y, UnityEngine.Events.UnityAction action)
        {
            float yy = y;
            UIRoot.CreateButton(root, label, new Vector2(0.5f, 1f), new Vector2(0f, -yy),
                new Vector2(420f, 60f), action, new Color(0.13f, 0.15f, 0.19f, 0.95f), 24);
            y += 74f;
        }

        void ToggleMP()
        {
            if (mpPanel != null) { Destroy(mpPanel.gameObject); mpPanel = null; return; }
            mpPanel = UIRoot.CreatePanel(root, "MPPanel", new Vector2(0.5f, 0f), new Vector2(400f, 260f),
                new Vector2(460f, 500f), new Color(0.1f, 0.12f, 0.16f, 0.98f));
            UIRoot.CreateText(mpPanel, "MULTIJUGADOR COOPERATIVO", 22, UIRoot.Accent, new Vector2(0.5f, 1f),
                new Vector2(0f, -16f), new Vector2(430f, 30f), TextAnchor.MiddleCenter, FontStyle.Bold);

            // ---- ONLINE (Unity Relay: gratis, sin abrir puertos) ----
            UIRoot.CreateText(mpPanel, "— ONLINE (recomendado) —", 17, new Color(0.6f, 0.85f, 0.6f),
                new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(430f, 24f), TextAnchor.MiddleCenter);
            UIRoot.CreateButton(mpPanel, "CREAR PARTIDA ONLINE", new Vector2(0.5f, 1f), new Vector2(0f, -82f),
                new Vector2(400f, 48f), () =>
                {
                    MPMode.Mode = MPModeKind.RelayHost;
                    SceneManager.LoadScene("LosAlisos");
                }, new Color(0.2f, 0.35f, 0.2f));
            UIRoot.CreateText(mpPanel, "Al crear, el CÓDIGO se copia solo: envíaselo a tu amigo.",
                14, UIRoot.TextColor, new Vector2(0.5f, 1f), new Vector2(0f, -134f), new Vector2(430f, 22f), TextAnchor.MiddleCenter);
            UIRoot.CreateButton(mpPanel, "UNIRSE ONLINE (pega el código y pulsa)", new Vector2(0.5f, 1f), new Vector2(0f, -162f),
                new Vector2(400f, 48f), () =>
                {
                    string code = GUIUtility.systemCopyBuffer;
                    if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 12)
                    {
                        ModalUI.Show("Falta el código",
                            "Copia primero el código de 6 letras que te pasó el anfitrión (Ctrl+C) y vuelve a pulsar el botón.",
                            "Entendido", null);
                        return;
                    }
                    MPMode.Mode = MPModeKind.RelayClient;
                    MPMode.JoinCode = code.Trim();
                    SceneManager.LoadScene("LosAlisos");
                });
            UIRoot.CreateText(mpPanel, "Requiere vincular el proyecto a Unity Gaming Services (una sola vez).",
                13, new Color(0.6f, 0.6f, 0.62f), new Vector2(0.5f, 1f), new Vector2(0f, -214f), new Vector2(430f, 22f), TextAnchor.MiddleCenter);

            // ---- LAN / IP directa ----
            UIRoot.CreateText(mpPanel, "— RED LOCAL (misma casa/red) —", 17, new Color(0.6f, 0.7f, 0.9f),
                new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(430f, 24f), TextAnchor.MiddleCenter);
            UIRoot.CreateButton(mpPanel, "CREAR PARTIDA LAN", new Vector2(0.5f, 1f), new Vector2(0f, -280f),
                new Vector2(400f, 46f), () =>
                {
                    MPMode.Mode = MPModeKind.Host;
                    SceneManager.LoadScene("LosAlisos");
                });
            ipText = UIRoot.CreateText(mpPanel, "IP: " + ip, 19, Color.white, new Vector2(0.5f, 1f),
                new Vector2(0f, -334f), new Vector2(430f, 26f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UIRoot.CreateButton(mpPanel, "localhost", new Vector2(0.5f, 1f), new Vector2(-140f, -366f),
                new Vector2(130f, 42f), () => SetIp("127.0.0.1"), null, 15);
            UIRoot.CreateButton(mpPanel, "Pegar IP", new Vector2(0.5f, 1f), new Vector2(0f, -366f),
                new Vector2(130f, 42f), () => SetIp(GUIUtility.systemCopyBuffer), null, 15);
            UIRoot.CreateButton(mpPanel, "UNIRSE LAN", new Vector2(0.5f, 1f), new Vector2(140f, -366f),
                new Vector2(130f, 42f), () =>
                {
                    MPMode.Mode = MPModeKind.Client;
                    MPMode.Ip = ip;
                    SceneManager.LoadScene("LosAlisos");
                }, new Color(0.2f, 0.3f, 0.4f), 15);

            UIRoot.CreateText(mpPanel, "Steam (invitaciones desde la lista de amigos) llegará después:\nla arquitectura ya quedó lista para conectarle el transport.",
                13, new Color(0.55f, 0.55f, 0.58f), new Vector2(0.5f, 1f), new Vector2(0f, -420f), new Vector2(430f, 40f), TextAnchor.MiddleCenter);
        }

        void SetIp(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            ip = value.Trim();
            MPMode.Ip = ip;
            if (ipText != null) ipText.text = ip;
        }
    }
}
