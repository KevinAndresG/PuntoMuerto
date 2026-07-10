using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Teléfono de la oficina: ofertas de Fabio, Vía del Poder, finales (GDD 6/7).</summary>
    public class FabioPhone : MonoBehaviour, IInteractable
    {
        public string Prompt => MissionGenerator.I != null && MissionGenerator.I.PhoneRinging
            ? "¡Contestar el teléfono!" : "Teléfono / Agenda";
        public bool CanInteract => true;

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<PhoneUI>();
            if (ui != null) ui.Open();
        }

        void Update()
        {
            bool ringing = MissionGenerator.I != null && MissionGenerator.I.PhoneRinging;
            var r = GetComponent<Renderer>();
            if (r != null && r.material.HasProperty("_EmissionColor"))
            {
                Color c = ringing && Mathf.PingPong(Time.time * 3f, 1f) > 0.5f
                    ? new Color(1f, 0.3f, 0.1f) : Color.black;
                r.material.SetColor("_EmissionColor", c);
            }
        }
    }
}
