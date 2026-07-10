using UnityEngine;
using UnityEngine.InputSystem;

namespace PuntoMuerto
{
    /// <summary>Movimiento del jugador (Input System). WASD relativo a cámara, Shift correr, atropello con caída.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float WalkSpeed = 3.5f;
        public float RunSpeed = 6.5f;
        public float RotationSpeed = 12f;
        public float JumpSpeed = 6.5f;
        public bool InputEnabled = true;

        /// <summary>Derribado por un carro: sin control hasta levantarse.</summary>
        public bool KnockedDown { get; private set; }

        CharacterController cc;
        Transform cam;
        Transform visualPivot;
        float verticalVel;
        float knockTimer;
        Vector3 knockVel;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            // pivote para tumbar el cuerpo visual sin tocar el CharacterController
            visualPivot = new GameObject("VisualPivot").transform;
            visualPivot.SetParent(transform, false);
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var ch = transform.GetChild(i);
                if (ch != visualPivot && ch.GetComponent<Renderer>() != null)
                    ch.SetParent(visualPivot, true);
            }
        }

        void Start()
        {
            if (Camera.main != null) cam = Camera.main.transform;
        }

        /// <summary>Un carro golpeó al jugador: cae, pierde control y se levanta solo.</summary>
        public void Knockdown(Vector3 dir)
        {
            if (KnockedDown) return;
            KnockedDown = true;
            knockTimer = 2.2f;
            dir.y = 0f;
            knockVel = dir.normalized * 7f;
            verticalVel = 3.5f;
            GameEvents.Notify("¡Te atropellaron! Levántate...");
        }

        void Update()
        {
            if (cc == null || !cc.enabled) return;
            if (GameManager.I != null && GameManager.I.GamePaused) return;

            if (KnockedDown) { UpdateKnockdown(); return; }

            Vector2 move = Vector2.zero;
            bool run = false;
            var kb = Keyboard.current;
            if (InputEnabled && kb != null && !UIRoot.ModalOpen)
            {
                if (kb.wKey.isPressed) move.y += 1f;
                if (kb.sKey.isPressed) move.y -= 1f;
                if (kb.dKey.isPressed) move.x += 1f;
                if (kb.aKey.isPressed) move.x -= 1f;
                run = kb.leftShiftKey.isPressed;
            }

            Vector3 dir = Vector3.zero;
            if (move.sqrMagnitude > 0.01f)
            {
                move.Normalize();
                Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
                fwd.y = 0f; fwd.Normalize();
                Vector3 right = cam != null ? cam.right : Vector3.right;
                right.y = 0f; right.Normalize();
                dir = fwd * move.y + right * move.x;
                // en primera persona el yaw lo maneja la cámara; en tercera rotamos hacia el movimiento
                if (!FirstPersonCamera.Active)
                {
                    Quaternion target = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, target, RotationSpeed * Time.deltaTime);
                }
            }

            float speed = run ? RunSpeed : WalkSpeed;
            if (cc.isGrounded)
            {
                verticalVel = -1f;
                if (InputEnabled && kb != null && !UIRoot.ModalOpen && kb.spaceKey.wasPressedThisFrame)
                    verticalVel = JumpSpeed;
            }
            else verticalVel -= 20f * Time.deltaTime;
            Vector3 vel = dir * speed + Vector3.up * verticalVel;
            cc.Move(vel * Time.deltaTime);
        }

        void UpdateKnockdown()
        {
            knockTimer -= Time.deltaTime;
            knockVel = Vector3.Lerp(knockVel, Vector3.zero, 4f * Time.deltaTime);
            if (cc.isGrounded && verticalVel < 0f) verticalVel = -1f;
            else verticalVel -= 20f * Time.deltaTime;
            cc.Move((knockVel + Vector3.up * verticalVel) * Time.deltaTime);

            // tumbado los primeros ~1.5s, se levanta en los últimos 0.7s
            float tilt = knockTimer > 0.7f ? 80f : Mathf.Lerp(0f, 80f, knockTimer / 0.7f);
            if (visualPivot != null) visualPivot.localRotation = Quaternion.Euler(tilt, 0f, 0f);

            if (knockTimer <= 0f)
            {
                KnockedDown = false;
                if (visualPivot != null) visualPivot.localRotation = Quaternion.identity;
            }
        }
    }
}
