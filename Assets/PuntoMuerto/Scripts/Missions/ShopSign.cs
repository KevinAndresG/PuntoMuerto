using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Letrero ABIERTO/CERRADO del frente: controla si llegan clientes honestos.</summary>
    public class ShopSign : MonoBehaviour, IInteractable
    {
        public static bool Abierto = true;

        TextMesh text;

        public string Prompt => Abierto
            ? "Cerrar el taller (no llegarán más clientes al frente)"
            : "Abrir el taller (volver a recibir clientes)";
        public bool CanInteract => true;

        void Start()
        {
            Abierto = true;
            // el componente vive en un Empty sin escala; el cartel visual es un hijo aparte
            var go = new GameObject("TextoEstado");
            go.transform.SetParent(transform, false);
            // mirando a la calle (+z): igual que los demás letreros del frente
            go.transform.localPosition = new Vector3(0f, 0f, 0.09f);
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            go.transform.localScale = Vector3.one * 0.055f;
            text = go.AddComponent<TextMesh>();
            text.fontSize = 60;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontStyle = FontStyle.Bold;
            Refresh();
        }

        public void Interact(PlayerInteraction p)
        {
            Abierto = !Abierto;
            if (Net.IsClientOnly) GameSync.RequestSign(Abierto); // el host es la autoridad
            Refresh();
            GameEvents.Notify(Abierto
                ? "Taller ABIERTO: la cortina está subiendo."
                : "Taller CERRADO: la cortina está bajando (la ventanilla trasera sigue activa).");
        }

        bool shown = true;

        void Update()
        {
            // el estado puede cambiar por red: refrescar cuando difiera
            if (text != null && shown != Abierto) Refresh();
        }

        void Refresh()
        {
            if (text == null) return;
            shown = Abierto;
            text.text = Abierto ? "ABIERTO" : "CERRADO";
            text.color = Abierto ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.9f, 0.3f, 0.25f);
        }
    }
}
