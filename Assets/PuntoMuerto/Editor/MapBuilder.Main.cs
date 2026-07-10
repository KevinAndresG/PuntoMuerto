using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuntoMuerto.EditorTools
{
    /// <summary>Construye el juego completo: texturas, escena Los Alisos, menú, build settings.</summary>
    public static partial class MapBuilder
    {
        [MenuItem("PuntoMuerto/Construir TODO (texturas + mapa + escenas)")]
        public static void BuildAll()
        {
            TextureGen.GenerateAll();
            BuildGameScene();
            BuildMenuScene();
            SetupBuildSettings();
            EditorSceneManager.OpenScene("Assets/Scenes/LosAlisos.unity");
            Debug.Log("PuntoMuerto: construcción completa.");
        }

        [MenuItem("PuntoMuerto/1. Solo texturas y materiales")]
        public static void OnlyTextures() => TextureGen.GenerateAll();

        [MenuItem("PuntoMuerto/2. Solo escena LosAlisos")]
        public static void OnlyGameScene() { BuildGameScene(); }

        [MenuItem("PuntoMuerto/3. Solo escena MainMenu")]
        public static void OnlyMenuScene() { BuildMenuScene(); }

        // ---------- helpers ----------

        static Material M(string name)
        {
            var m = TextureGen.Load(name);
            if (m == null) Debug.LogWarning("Material no encontrado: " + name);
            return m;
        }

        static GameObject Empty(string name, Transform parent, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            return go;
        }

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale,
            string mat, float rotY = 0f, bool isStatic = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            var m = M(mat);
            if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
            go.isStatic = isStatic;
            return go;
        }

        static GameObject BoxR(string name, Transform parent, Vector3 pos, Vector3 scale,
            string mat, Vector3 euler, bool isStatic = true)
        {
            var go = Box(name, parent, pos, scale, mat, 0f, isStatic);
            go.transform.rotation = Quaternion.Euler(euler);
            return go;
        }

        static GameObject Cyl(string name, Transform parent, Vector3 pos, Vector3 scale,
            string mat, bool isStatic = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var m = M(mat);
            if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
            go.isStatic = isStatic;
            return go;
        }

        static GameObject Sphere(string name, Transform parent, Vector3 pos, Vector3 scale,
            string mat, bool isStatic = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var m = M(mat);
            if (m != null) go.GetComponent<Renderer>().sharedMaterial = m;
            go.isStatic = isStatic;
            return go;
        }

        static Material text3DMat;

        /// <summary>Material asset compartido: texto de una sola cara que respeta paredes.</summary>
        static Material Text3DMaterial()
        {
            if (text3DMat != null) return text3DMat;
            const string path = "Assets/PuntoMuerto/Materials/Texto3D.mat";
            text3DMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (text3DMat == null)
            {
                var shader = Shader.Find("PuntoMuerto/Text3D");
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (shader == null || font == null) return null;
                var mat = new Material(shader) { mainTexture = font.material.mainTexture };
                System.IO.Directory.CreateDirectory("Assets/PuntoMuerto/Materials");
                AssetDatabase.CreateAsset(mat, path);
                text3DMat = mat;
            }
            return text3DMat;
        }

        static TextMesh Text3D(string text, Transform parent, Vector3 pos, float charSize,
            Color color, float rotY = 0f, int fontSize = 60, FontStyle style = FontStyle.Bold)
        {
            var go = new GameObject("Texto_" + text.Replace(" ", ""));
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            go.transform.localScale = Vector3.one * charSize;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = fontSize;
            tm.color = color;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = style;
            var m3 = Text3DMaterial();
            if (m3 != null) go.GetComponent<MeshRenderer>().sharedMaterial = m3;
            return tm;
        }

        static void Waypoint(Transform parent, Vector3 pos)
        {
            var go = new GameObject("wp");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
        }
    }
}
