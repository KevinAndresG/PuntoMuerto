using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Material compartido para TextMesh creados en runtime: una sola cara y tapado por paredes
    /// (el font shader por defecto usa ZTest Always + Cull Off y se ve invertido/a través de todo).</summary>
    public static class TextStyle
    {
        static Material mat;

        public static void Apply(TextMesh tm)
        {
            if (tm == null) return;
            var r = tm.GetComponent<MeshRenderer>();
            if (r == null) return;
            if (mat == null)
            {
                var sh = Shader.Find("PuntoMuerto/Text3D");
                var fontTex = r.sharedMaterial != null ? r.sharedMaterial.mainTexture : null;
                if (sh == null || fontTex == null) return; // fallback: font shader por defecto
                mat = new Material(sh) { mainTexture = fontTex };
            }
            r.sharedMaterial = mat;
        }
    }
}
