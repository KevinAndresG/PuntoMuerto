using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Mostrador de recepción. Archivo propio: se serializa en escena.</summary>
    public class ReceptionDesk : MonoBehaviour, IInteractable
    {
        public string Prompt => ReceptionSystem.I != null && ReceptionSystem.I.Queue.Count > 0
            ? "Atender clientes (" + ReceptionSystem.I.Queue.Count + " esperando)"
            : "Mostrador de recepción (sin clientes)";
        public bool CanInteract => true;

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<ReceptionUI>();
            if (ui != null) ui.Open();
        }
    }
}
