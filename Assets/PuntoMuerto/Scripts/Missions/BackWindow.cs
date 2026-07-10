using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Ventanilla trasera del garaje: atender el negocio sucio.</summary>
    public class BackWindow : MonoBehaviour, IInteractable
    {
        public string Prompt => DirtyReceptionSystem.I != null && DirtyReceptionSystem.I.Queue.Count > 0
            ? "Ventanilla trasera (" + DirtyReceptionSystem.I.Queue.Count + " esperando)"
            : "Ventanilla trasera (nadie espera)";

        public bool CanInteract => true;

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<DirtyReceptionUI>();
            if (ui != null) ui.Open();
        }
    }
}
