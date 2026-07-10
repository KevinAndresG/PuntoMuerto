using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Pizarra de mejoras (Meta D) y resumen de metas (GDD 12).</summary>
    public class UpgradeBoard : MonoBehaviour, IInteractable
    {
        public string Prompt => "Pizarra: metas y mejoras del taller";
        public bool CanInteract => true;

        public void Interact(PlayerInteraction p)
        {
            var ui = Object.FindFirstObjectByType<UpgradesUI>();
            if (ui != null) ui.Open();
        }
    }
}
