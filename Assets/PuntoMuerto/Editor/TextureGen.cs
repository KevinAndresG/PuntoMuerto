using System.IO;
using UnityEditor;
using UnityEngine;

namespace PuntoMuerto.EditorTools
{
    /// <summary>Genera texturas procedurales y materiales URP para el pueblo.</summary>
    public static class TextureGen
    {
        const string TexDir = "Assets/PuntoMuerto/Textures";
        const string MatDir = "Assets/PuntoMuerto/Materials";

        public static void GenerateAll()
        {
            Directory.CreateDirectory(TexDir);
            Directory.CreateDirectory(MatDir);

            // --- texturas ---
            SavePNG(Noise(512, new Color(0.16f, 0.16f, 0.17f), 0.05f, 60f), "asfalto");
            SavePNG(SidewalkTiles(), "anden");
            SavePNG(Planks(), "madera");
            SavePNG(Noise(256, new Color(0.87f, 0.8f, 0.66f), 0.05f, 24f), "muro");
            SavePNG(Bricks(), "ladrillo");
            SavePNG(Noise(256, new Color(0.32f, 0.42f, 0.22f), 0.07f, 40f), "pasto");
            SavePNG(Corrugated(), "lamina");
            SavePNG(Noise(256, new Color(0.42f, 0.33f, 0.24f), 0.08f, 50f), "tierra");
            SavePNG(Noise(256, new Color(0.55f, 0.55f, 0.57f), 0.04f, 30f), "concreto");
            AssetDatabase.Refresh();

            // --- materiales ---
            Mat("Asfalto", "asfalto", Color.white, 0.05f, tiling: 8f);
            Mat("Anden", "anden", Color.white, 0.05f, tiling: 6f);
            Mat("Madera", "madera", Color.white, 0.1f, tiling: 2f);
            Mat("Muro", "muro", Color.white, 0.05f, tiling: 3f);
            Mat("MuroVerde", "muro", new Color(0.65f, 0.78f, 0.68f), 0.05f, tiling: 3f);
            Mat("MuroAzul", "muro", new Color(0.66f, 0.72f, 0.82f), 0.05f, tiling: 3f);
            Mat("MuroRosa", "muro", new Color(0.88f, 0.7f, 0.62f), 0.05f, tiling: 3f);
            Mat("Ladrillo", "ladrillo", Color.white, 0.02f, tiling: 4f);
            Mat("Pasto", "pasto", Color.white, 0.0f, tiling: 20f);
            Mat("Lamina", "lamina", Color.white, 0.35f, tiling: 4f);
            Mat("LaminaRoja", "lamina", new Color(0.7f, 0.32f, 0.26f), 0.3f, tiling: 4f);
            Mat("LaminaVerde", "lamina", new Color(0.35f, 0.45f, 0.4f), 0.3f, tiling: 4f);
            Mat("Tierra", "tierra", Color.white, 0.0f, tiling: 6f);
            Mat("Concreto", "concreto", Color.white, 0.05f, tiling: 4f);
            SolidMat("LineaVia", new Color(0.85f, 0.82f, 0.7f), 0.1f);
            SolidMat("TroncoArbol", new Color(0.38f, 0.28f, 0.18f), 0.05f);
            SolidMat("Copa1", new Color(0.25f, 0.45f, 0.2f), 0.02f);
            SolidMat("Copa2", new Color(0.32f, 0.5f, 0.24f), 0.02f);
            SolidMat("Metal", new Color(0.45f, 0.47f, 0.5f), 0.6f);
            SolidMat("MetalOscuro", new Color(0.2f, 0.21f, 0.23f), 0.5f);
            SolidMat("Oxido", new Color(0.5f, 0.3f, 0.2f), 0.1f);
            EmissiveMat("NeonNaranja", new Color(0.1f, 0.05f, 0.02f), new Color(2.5f, 0.9f, 0.15f));
            EmissiveMat("NeonBlanco", new Color(0.1f, 0.1f, 0.1f), new Color(1.8f, 1.7f, 1.4f));
            EmissiveMat("VentanaLuz", new Color(0.12f, 0.1f, 0.06f), new Color(1.6f, 1.2f, 0.5f));
            EmissiveMat("Farol", new Color(0.2f, 0.2f, 0.18f), new Color(1.8f, 1.6f, 1.1f));
            SolidMat("Piel", new Color(0.85f, 0.65f, 0.5f), 0.05f);

            AssetDatabase.SaveAssets();
            Debug.Log("TextureGen: texturas y materiales listos.");
        }

        // ---------- generadores ----------

        static Texture2D Noise(int size, Color baseColor, float amp, float scale)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, true);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x / (float)size * scale, y / (float)size * scale);
                    float n2 = Mathf.PerlinNoise(x / (float)size * scale * 4f, y / (float)size * scale * 4f) * 0.5f;
                    float v = 1f + ((n + n2) / 1.5f - 0.5f) * amp * 2f;
                    t.SetPixel(x, y, new Color(baseColor.r * v, baseColor.g * v, baseColor.b * v));
                }
            t.Apply();
            return t;
        }

        static Texture2D SidewalkTiles()
        {
            int s = 256;
            var t = Noise(s, new Color(0.6f, 0.58f, 0.55f), 0.05f, 20f);
            var dark = new Color(0.42f, 0.4f, 0.38f);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                    if (x % 128 < 3 || y % 128 < 3) t.SetPixel(x, y, dark);
            t.Apply();
            return t;
        }

        static Texture2D Planks()
        {
            int s = 256;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, true);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    int plank = x / 32;
                    float shade = 0.75f + Mathf.PerlinNoise(plank * 7.3f, y / 45f) * 0.4f;
                    var c = new Color(0.45f * shade, 0.3f * shade, 0.18f * shade);
                    if (x % 32 < 2) c *= 0.55f;
                    t.SetPixel(x, y, c);
                }
            t.Apply();
            return t;
        }

        static Texture2D Bricks()
        {
            int s = 256;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, true);
            var mortar = new Color(0.7f, 0.66f, 0.6f);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    int row = y / 32;
                    int xx = x + (row % 2 == 0 ? 0 : 32);
                    float shade = 0.8f + Mathf.PerlinNoise(xx / 15f, y / 15f) * 0.35f;
                    var c = new Color(0.62f * shade, 0.3f * shade, 0.22f * shade);
                    if (y % 32 < 3 || xx % 64 < 3) c = mortar;
                    t.SetPixel(x, y, c);
                }
            t.Apply();
            return t;
        }

        static Texture2D Corrugated()
        {
            int s = 256;
            var t = new Texture2D(s, s, TextureFormat.RGBA32, true);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float wave = Mathf.Sin(x / 8f * Mathf.PI) * 0.15f + 0.85f;
                    float rust = Mathf.PerlinNoise(x / 40f, y / 40f) > 0.72f ? 0.7f : 1f;
                    t.SetPixel(x, y, new Color(0.65f * wave * rust, 0.66f * wave * rust, 0.68f * wave));
                }
            t.Apply();
            return t;
        }

        // ---------- guardado ----------

        static void SavePNG(Texture2D tex, string name)
        {
            File.WriteAllBytes(TexDir + "/" + name + ".png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void Mat(string name, string tex, Color tint, float smooth, float tiling = 1f)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + tex + ".png");
            m.SetTexture("_BaseMap", texture);
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_Smoothness", smooth);
            m.SetTextureScale("_BaseMap", Vector2.one * tiling);
            AssetDatabase.CreateAsset(m, MatDir + "/" + name + ".mat");
        }

        static void SolidMat(string name, Color color, float smooth)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smooth);
            AssetDatabase.CreateAsset(m, MatDir + "/" + name + ".mat");
        }

        static void EmissiveMat(string name, Color baseColor, Color emission)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", baseColor);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", emission);
            AssetDatabase.CreateAsset(m, MatDir + "/" + name + ".mat");
        }

        public static Material Load(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/" + name + ".mat");
    }
}
