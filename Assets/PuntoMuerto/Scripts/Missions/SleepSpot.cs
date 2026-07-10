using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>Catre de la trastienda: dormir. Fundido a negro y despiertas a las 7:00.</summary>
    public class SleepSpot : MonoBehaviour, IInteractable
    {
        public string Prompt => "Dormir hasta las 7:00";
        public bool CanInteract => !sleeping && DayNightCycle.I != null &&
            (DayNightCycle.I.Hour >= 19f || DayNightCycle.I.Hour < 6f);

        bool sleeping;

        public void Interact(PlayerInteraction p)
        {
            if (!sleeping) StartCoroutine(SleepFade());
        }

        IEnumerator SleepFade()
        {
            sleeping = true;
            UIRoot.PushModal();
            var overlay = UIRoot.CreateFullscreenPanel(UIRoot.I.Canvas.transform, "SleepFade",
                new Color(0f, 0f, 0f, 0f));
            var img = overlay.GetComponent<Image>();

            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.2f)
            {
                img.color = new Color(0f, 0f, 0f, t);
                yield return null;
            }
            img.color = Color.black;

            DayNightCycle.I.SleepUntilMorning();
            GameEvents.Notify("Despiertas a las 7:00. Los clientes llegan desde las 8:00.");
            yield return new WaitForSeconds(0.7f);

            for (float t = 1f; t > 0f; t -= Time.deltaTime / 1.2f)
            {
                img.color = new Color(0f, 0f, 0f, t);
                yield return null;
            }

            UIRoot.PopModal();
            Destroy(overlay.gameObject);
            sleeping = false;
        }
    }
}
