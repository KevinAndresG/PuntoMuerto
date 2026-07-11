using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>PC: catálogo del proveedor. Archivo propio: se serializa en escena.</summary>
    public class DeskComputer : MonoBehaviour, IInteractable
    {
        public string Prompt => "PC: comprar repuestos y mercado negro";
        public bool CanInteract => true;

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<SupplierUI>();
            if (ui != null) ui.Open();
        }
    }
}
