using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Lona del patio: cubre el trabajo ante un espía. Archivo propio: se serializa en escena.</summary>
    public class TarpInteractable : MonoBehaviour, IInteractable
    {
        public string Prompt => "Cubrir el trabajo con la lona";
        public bool CanInteract => FenceSpySystem.I != null && FenceSpySystem.I.ActiveSpy != null;

        public void Interact(PlayerInteraction p)
        {
            foreach (var st in RepairStation.All)
                if (st.Kind == StationKind.Patio) st.Cover(6f);
            GameEvents.Notify("Cubriste el trabajo. Espera a que se vaya...");
        }
    }
}
