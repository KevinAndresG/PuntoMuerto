using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Escritorio con el Libro Real/Oficial: lavar dinero, pagar banco, préstamos.</summary>
    public class DeskLedger : MonoBehaviour, IInteractable
    {
        public string Prompt => "Libro Real / Libro Oficial";
        public bool CanInteract => true;

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<LedgerUI>();
            if (ui != null) ui.Open();
        }
    }
}
