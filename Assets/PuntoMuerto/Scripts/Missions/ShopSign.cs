using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Botón de pared ABIERTO/CERRADO en el pilar de la recepción: controla si llegan
    /// clientes honestos. La caja la construye MapBuilder; aquí se crean la lámpara verde/roja
    /// y el cartel flotante (billboard con vaivén) que muestra el estado desde lejos.</summary>
    public class ShopSign : MonoBehaviour, IInteractable
    {
        public static bool Abierto = true;

        TextMesh text;
        Transform cartel;
        Vector3 cartelBase;
        Light lamp;
        Renderer boton;
        bool shown = true;

        public string Prompt => Abierto
            ? "Pulsar el botón: CERRAR el taller (no llegarán más clientes al frente)"
            : "Pulsar el botón: ABRIR el taller (volver a recibir clientes)";
        public bool CanInteract => true;

        void Start()
        {
            Abierto = true;

            // lámpara de estado sobre la caja del botón
            var lampGo = new GameObject("LamparaEstado");
            lampGo.transform.SetParent(transform, false);
            lampGo.transform.localPosition = new Vector3(0f, 0.7f, 0.35f);
            lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.range = 5f;
            lamp.intensity = 1.8f;

            // cartel flotante centrado sobre el frente del taller (ancla fija; no cuelga del botón)
            var cartelGo = new GameObject("CartelFlotante");
            cartelGo.transform.SetParent(transform, false);
            var anchor = GameObject.Find("CartelEstadoAnchor");
            cartelGo.transform.position = anchor != null
                ? anchor.transform.position
                : transform.position + new Vector3(0f, 4.1f, 0.4f);
            cartelGo.transform.localScale = Vector3.one * 0.07f;
            text = cartelGo.AddComponent<TextMesh>();
            text.fontSize = 60;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontStyle = FontStyle.Bold;
            TextStyle.Apply(text);
            cartel = cartelGo.transform;
            cartelBase = cartel.position;

            var tapa = transform.Find("BotonTapa");
            if (tapa != null) boton = tapa.GetComponent<Renderer>();

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

        void Update()
        {
            // el estado puede cambiar por red: refrescar cuando difiera
            if (shown != Abierto) Refresh();

            if (cartel != null)
            {
                cartel.position = cartelBase + Vector3.up * (Mathf.Sin(Time.time * 1.5f) * 0.12f);
                if (Camera.main != null)
                    cartel.rotation = Quaternion.LookRotation(cartel.position - Camera.main.transform.position);
            }
        }

        void Refresh()
        {
            shown = Abierto;
            var c = Abierto ? new Color(0.35f, 0.9f, 0.4f) : new Color(0.95f, 0.3f, 0.25f);
            if (text != null)
            {
                text.text = Abierto ? "ABIERTO" : "CERRADO";
                text.color = c;
            }
            if (lamp != null) lamp.color = c;
            if (boton != null)
            {
                var m = boton.material; // instancia propia: no tocar el sharedMaterial del builder
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_EmissionColor"))
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", c * 1.4f);
                }
            }
        }
    }
}
