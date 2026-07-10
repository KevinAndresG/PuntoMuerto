using UnityEngine;
using UnityEngine.InputSystem;

namespace PuntoMuerto
{
    /// <summary>Cámara en primera persona: yaw en el jugador, pitch en la cámara. Oculta el cuerpo del dueño.</summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        /// <summary>Si hay cámara FP activa, PlayerController no auto-rota hacia el movimiento.</summary>
        public static bool Active;

        public Transform Target;
        public float Sensitivity = 0.12f;
        public float EyeHeight = 1.72f;

        float pitch;
        float smoothHeight;
        Transform current;

        void OnEnable() { Active = true; smoothHeight = EyeHeight; }
        void OnDisable() { Active = false; }

        public void SetTarget(Transform t) { Target = t; current = null; }

        void LateUpdate()
        {
            if (Target == null) return;
            if (current != Target) { current = Target; HideBody(current); }

            bool uiOpen = UIRoot.ModalOpen || (GameManager.I != null && GameManager.I.GamePaused);
            var mouse = Mouse.current;
            var pc = Target.GetComponent<PlayerController>();
            bool knocked = pc != null && pc.KnockedDown;

            if (mouse != null && !uiOpen && !knocked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                Target.Rotate(0f, delta.x * Sensitivity, 0f);
                pitch = Mathf.Clamp(pitch - delta.y * Sensitivity, -75f, 80f);
            }

            Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = uiOpen;

            // atropello: la cámara cae al suelo y rueda; luego se levanta
            float height = knocked ? 0.55f : EyeHeight;
            float roll = knocked ? 40f : 0f;
            smoothHeight = Mathf.Lerp(smoothHeight, height, 10f * Time.deltaTime);

            transform.position = Target.position + Vector3.up * smoothHeight;
            var desired = Quaternion.Euler(pitch, Target.eulerAngles.y, roll);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 14f * Time.deltaTime);
        }

        static void HideBody(Transform t)
        {
            // el dueño no ve su propio cuerpo, pero sí su sombra
            foreach (var r in t.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }
}
