using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Luz que se enciende de noche (postes, letreros, ventanas).
    /// IMPORTANTE: debe vivir en su propio archivo — se serializa en la escena.</summary>
    public class NightLight : MonoBehaviour
    {
        Light[] lights;
        Renderer emissiveRenderer;
        Material emissiveMat;
        Color baseEmission;
        bool hasEmission;

        void Start()
        {
            lights = GetComponentsInChildren<Light>(true);
            emissiveRenderer = GetComponentInChildren<Renderer>();
            if (emissiveRenderer != null && emissiveRenderer.sharedMaterial != null &&
                emissiveRenderer.sharedMaterial.HasProperty("_EmissionColor"))
            {
                emissiveMat = emissiveRenderer.material;
                baseEmission = emissiveMat.GetColor("_EmissionColor");
                hasEmission = baseEmission.maxColorComponent > 0.01f;
            }
        }

        void Update()
        {
            if (DayNightCycle.I == null) return;
            bool on = DayNightCycle.I.IsNight || DayNightCycle.I.Hour > 18.5f;
            foreach (var l in lights) if (l != null) l.enabled = on;
            if (hasEmission)
                emissiveMat.SetColor("_EmissionColor", on ? baseEmission : Color.black);
        }
    }
}
