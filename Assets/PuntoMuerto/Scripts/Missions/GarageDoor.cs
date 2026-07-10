using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Cortina enrollable de una bahía del frente: baja al cerrar el taller y sube al abrirlo.
    /// Lee ShopSign.Abierto cada frame, así también reacciona al estado sincronizado en multijugador.</summary>
    public class GarageDoor : MonoBehaviour
    {
        public float Height = 4.2f;   // alto de la cortina desplegada
        public float Speed = 2.4f;    // m/s de subida/bajada

        Transform panel;              // hijo "Cortina" que cuelga del rodillo
        float openness = 1f;          // 1 = enrollada (abierto), 0 = cerrada

        void Start()
        {
            panel = transform.Find("Cortina");
            openness = ShopSign.Abierto ? 1f : 0f;
            Apply();
        }

        void Update()
        {
            float target = ShopSign.Abierto ? 1f : 0f;
            if (Mathf.Approximately(openness, target)) return;
            openness = Mathf.MoveTowards(openness, target, (Speed / Height) * Time.deltaTime);
            Apply();
        }

        void Apply()
        {
            if (panel == null) return;
            float h = Mathf.Lerp(Height, 0.3f, openness); // enrollada deja el borde visible
            var s = panel.localScale;
            s.y = h;
            panel.localScale = s;
            panel.localPosition = new Vector3(0f, -h / 2f, 0f);
        }
    }
}
