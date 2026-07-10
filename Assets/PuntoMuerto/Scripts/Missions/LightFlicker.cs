using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Parpadeo de luz de soplete/trabajo.</summary>
    public class LightFlicker : MonoBehaviour
    {
        public float BaseIntensity = 3f;
        Light l;
        void Start() { l = GetComponent<Light>(); }
        void Update() { if (l != null) l.intensity = BaseIntensity * Random.Range(0.6f, 1.3f); }
    }
}
