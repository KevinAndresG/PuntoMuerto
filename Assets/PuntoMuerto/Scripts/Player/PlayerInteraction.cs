using UnityEngine;
using UnityEngine.InputSystem;

namespace PuntoMuerto
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(PlayerInteraction player);
    }

    /// <summary>Interacción por crosshair: raycast desde el centro de la cámara, E para interactuar.</summary>
    public class PlayerInteraction : MonoBehaviour
    {
        public float Range = 3.4f;
        public IInteractable Current { get; private set; }

        Camera cam;
        static RaycastHit[] hits = new RaycastHit[16];

        void Update()
        {
            if (GameManager.I != null && GameManager.I.GamePaused) { Current = null; return; }
            if (UIRoot.ModalOpen) { Current = null; HUDController.SetPrompt(null); HUDController.SetCrosshair(false); return; }
            if (cam == null) { cam = Camera.main; if (cam == null) return; }

            Current = AimTarget();
            bool ok = Current != null && Current.CanInteract;
            HUDController.SetPrompt(ok ? "[E] " + Current.Prompt : null);
            HUDController.SetCrosshair(ok);

            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame && ok)
                Current.Interact(this);
        }

        IInteractable AimTarget()
        {
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            // spherecast: puntería tolerante; muchos interactuables tienen decorados encima (tapas, pantallas)
            int n = Physics.SphereCastNonAlloc(ray, 0.12f, hits, Range, ~0, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, 0, n, HitComparer.I);

            IInteractable best = null;
            float bestDist = float.MaxValue, solidDist = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                var col = hits[i].collider;
                if (col.transform.root == transform.root) continue; // ignorar al propio jugador
                var it = col.GetComponentInParent<IInteractable>();
                if (it != null) { if (hits[i].distance < bestDist) { bestDist = hits[i].distance; best = it; } }
                else if (!col.isTrigger && hits[i].distance < solidDist) solidDist = hits[i].distance;
            }
            // un sólido bloquea solo si el interactuable queda claramente detrás (pared sí, tapa del mostrador no)
            return best != null && bestDist < solidDist + 0.6f ? best : null;
        }

        class HitComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitComparer I = new HitComparer();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }
}
