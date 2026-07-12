using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Puerta batiente funcional: E la abre o la cierra girando la hoja sobre su bisagra.
    /// La hoja (con collider sólido) es hija de este pivote, que se ubica sobre el eje de la bisagra;
    /// al rotar el pivote, la hoja barre el vano. La geometría la arma MapBuilder (BuildHingedDoor);
    /// aquí solo vive la lógica de giro. El signo de OpenAngle define hacia qué lado abre.</summary>
    public class HingedDoor : MonoBehaviour, IInteractable
    {
        public float OpenAngle = 92f;   // grados de apertura (el signo elige el lado)
        public float Speed = 260f;      // grados/seg

        bool open;
        float cur;
        Quaternion closed;

        void Start() { closed = transform.localRotation; }

        public string Prompt => open ? "Cerrar la puerta" : "Abrir la puerta";
        public bool CanInteract => true;
        public void Interact(PlayerInteraction p) { open = !open; }

        void Update()
        {
            float target = open ? OpenAngle : 0f;
            if (Mathf.Approximately(cur, target)) return;
            cur = Mathf.MoveTowards(cur, target, Speed * Time.deltaTime);
            transform.localRotation = closed * Quaternion.Euler(0f, cur, 0f);
        }
    }
}
