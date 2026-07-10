using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace PuntoMuerto.EditorTools
{
    public static partial class MapBuilder
    {
        static readonly string[] NpcNames =
        {
            "Don Ernesto", "Doña Marta", "Rosa", "El Chino", "Padre Benito",
            "Lucía", "Ramiro", "La Profe Carmen", "Tulio", "Vieja Elvira"
        };

        static readonly Color[] NpcColors =
        {
            new Color(0.55f, 0.35f, 0.25f), new Color(0.6f, 0.45f, 0.6f), new Color(0.75f, 0.4f, 0.4f),
            new Color(0.35f, 0.45f, 0.6f), new Color(0.25f, 0.25f, 0.3f), new Color(0.7f, 0.6f, 0.35f),
            new Color(0.4f, 0.55f, 0.4f), new Color(0.65f, 0.5f, 0.3f), new Color(0.45f, 0.4f, 0.55f),
            new Color(0.5f, 0.5f, 0.45f)
        };

        public static void BuildGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var world = new GameObject("Mundo").transform;
            BuildTown(world);
            BuildTaller(world);
            BuildWaypoints();
            BuildLighting();
            BuildSystems();
            BuildPlayer();
            BuildNPCs();
            BuildPrefabs();
            BakeNavMesh(world);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/LosAlisos.unity");
            Debug.Log("Escena LosAlisos construida y guardada.");
        }

        static void BuildWaypoints()
        {
            var root = new GameObject("Waypoints").transform;

            // andenes (para deambular de NPCs)
            var andenes = Empty("Andenes", root, Vector3.zero).transform;
            for (int x = -50; x <= 50; x += 20)
            {
                Waypoint(andenes, new Vector3(x, 0f, 5.5f));
                Waypoint(andenes, new Vector3(x, 0f, -5.5f));
            }
            for (int z = -50; z <= 50; z += 20)
            {
                if (Mathf.Abs(z) < 8) continue;
                Waypoint(andenes, new Vector3(5.5f, 0f, z));
                Waypoint(andenes, new Vector3(-5.5f, 0f, z));
            }
            Waypoint(andenes, new Vector3(26f, 0f, 26f));
            Waypoint(andenes, new Vector3(38f, 0f, 28f));
            Waypoint(andenes, new Vector3(32f, 0f, 40f));
            Waypoint(andenes, new Vector3(-26f, 0f, 10f)); // frente al súper
            Waypoint(andenes, new Vector3(30f, 0f, -7f));  // frente al taller

            // loops de carros (anillo, dos sentidos)
            var loopA = Empty("ViaLoopA", root, Vector3.zero);
            loopA.AddComponent<PuntoMuerto.WaypointPath>();
            float yCar = 0.08f;
            Vector3[] ringA =
            {
                new Vector3(58f, yCar, 58f), new Vector3(0f, yCar, 58f), new Vector3(-58f, yCar, 58f),
                new Vector3(-58f, yCar, 0f), new Vector3(-58f, yCar, -58f), new Vector3(0f, yCar, -58f),
                new Vector3(58f, yCar, -58f), new Vector3(58f, yCar, 0f)
            };
            foreach (var p in ringA) Waypoint(loopA.transform, p);

            var loopB = Empty("ViaLoopB", root, Vector3.zero);
            loopB.AddComponent<PuntoMuerto.WaypointPath>();
            Vector3[] ringB =
            {
                new Vector3(62f, yCar, 0f), new Vector3(62f, yCar, -62f), new Vector3(0f, yCar, -62f),
                new Vector3(-62f, yCar, -62f), new Vector3(-62f, yCar, 0f), new Vector3(-62f, yCar, 62f),
                new Vector3(0f, yCar, 62f), new Vector3(62f, yCar, 62f)
            };
            foreach (var p in ringB) Waypoint(loopB.transform, p);

            // entrada de clientes al taller
            var entrada = Empty("EntradaTaller", root, Vector3.zero);
            Waypoint(entrada.transform, new Vector3(75f, yCar, 2f));
            Waypoint(entrada.transform, new Vector3(48f, yCar, 2f));
            Waypoint(entrada.transform, new Vector3(38f, yCar, -3f));

            // puntos de recolección (familia C)
            var rec = Empty("Recoleccion", root, Vector3.zero).transform;
            Waypoint(rec, new Vector3(-36f, 0f, 12f));
            Waypoint(rec, new Vector3(24f, 0f, 42f));
            Waypoint(rec, new Vector3(-42f, 0f, -42f));
            Waypoint(rec, new Vector3(54f, 0f, 42f));
            Waypoint(rec, new Vector3(-12f, 0f, -54f));
        }

        static void BuildLighting()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.6f, 0.55f, 0.5f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.53f, 0.58f);
        }

        static void BuildSystems()
        {
            var systems = new GameObject("Systems");
            systems.AddComponent<PuntoMuerto.GameManager>();
            var cycle = systems.AddComponent<PuntoMuerto.DayNightCycle>();
            var sunGo = GameObject.Find("Sun");
            if (sunGo != null) cycle.Sun = sunGo.GetComponent<Light>();
            systems.AddComponent<PuntoMuerto.MetasManager>();
            systems.AddComponent<PuntoMuerto.BankSystem>();
            systems.AddComponent<PuntoMuerto.LedgerSystem>();
            systems.AddComponent<PuntoMuerto.UpgradeSystem>();
            systems.AddComponent<PuntoMuerto.EndingSystem>();
            systems.AddComponent<PuntoMuerto.MissionSystem>();
            systems.AddComponent<PuntoMuerto.MissionGenerator>();
            systems.AddComponent<PuntoMuerto.InventorySystem>();
            systems.AddComponent<PuntoMuerto.ReceptionSystem>();
            systems.AddComponent<PuntoMuerto.DirtyReceptionSystem>();
            systems.AddComponent<PuntoMuerto.BayManager>();
            systems.AddComponent<PuntoMuerto.NPCManager>();
            systems.AddComponent<PuntoMuerto.FenceSpySystem>();
            systems.AddComponent<PuntoMuerto.TrafficManager>();
            systems.AddComponent<PuntoMuerto.NetworkBootstrap>();

            var ui = new GameObject("UI");
            ui.AddComponent<PuntoMuerto.UIRoot>();
            ui.AddComponent<PuntoMuerto.HUDController>();
            ui.AddComponent<PuntoMuerto.DialogueUI>();
            ui.AddComponent<PuntoMuerto.PhoneUI>();
            ui.AddComponent<PuntoMuerto.LedgerUI>();
            ui.AddComponent<PuntoMuerto.UpgradesUI>();
            ui.AddComponent<PuntoMuerto.DirtyReceptionUI>();
            ui.AddComponent<PuntoMuerto.EndingUI>();
            ui.AddComponent<PuntoMuerto.PauseMenu>();
            ui.AddComponent<PuntoMuerto.SandingMinigameUI>();
            ui.AddComponent<PuntoMuerto.ReceptionUI>();
            ui.AddComponent<PuntoMuerto.SupplierUI>();
        }

        static GameObject BuildCharacterVisual(string name, Color overol, bool isPlayer)
        {
            var go = new GameObject(name);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Cuerpo";
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", overol);
            body.GetComponent<Renderer>().sharedMaterial = mat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Cabeza";
            head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.95f, 0f);
            head.transform.localScale = Vector3.one * 0.45f;
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = M("Piel");

            if (isPlayer)
            {
                // gorra del mecánico
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = "Gorra";
                cap.transform.SetParent(go.transform, false);
                cap.transform.localPosition = new Vector3(0f, 2.14f, 0.02f);
                cap.transform.localScale = new Vector3(0.4f, 0.07f, 0.45f);
                Object.DestroyImmediate(cap.GetComponent<Collider>());
                cap.GetComponent<Renderer>().sharedMaterial = M("MetalOscuro");
            }
            return go;
        }

        static void BuildPlayer()
        {
            var player = BuildCharacterVisual("Player", new Color(0.25f, 0.35f, 0.55f), true);
            player.transform.position = new Vector3(38f, 0.05f, -6f);
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.95f, 0f);
            cc.radius = 0.35f;
            player.AddComponent<PuntoMuerto.PlayerController>();
            player.AddComponent<PuntoMuerto.PlayerInteraction>();

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            var fpc = camGo.AddComponent<PuntoMuerto.FirstPersonCamera>();
            fpc.Target = player.transform;
            camGo.transform.position = player.transform.position + new Vector3(0f, 1.7f, 0f);
            cam.farClipPlane = 400f;
        }

        static void BuildNPCs()
        {
            var npcRoot = new GameObject("NPCs").transform;
            for (int i = 0; i < NpcNames.Length; i++)
            {
                var home = HousePositions[i];
                // punto FRENTE a la puerta, fuera del footprint de la casa (evita islas de navmesh)
                var fwd = Quaternion.Euler(0f, HouseRots[i], 0f) * Vector3.back;
                var frontDoor = home + fwd * 6f;
                var npc = BuildCharacterVisual("NPC_" + NpcNames[i].Replace(" ", ""), NpcColors[i], false);
                npc.transform.SetParent(npcRoot, false);
                npc.transform.position = frontDoor + new Vector3(0f, 0.05f, 0f);

                var agent = npc.AddComponent<NavMeshAgent>();
                agent.speed = 1.6f;
                agent.radius = 0.35f;
                agent.height = 1.9f;
                agent.angularSpeed = 240f;

                var ctrl = npc.AddComponent<PuntoMuerto.NPCController>();
                ctrl.NpcName = NpcNames[i];
                ctrl.HomePosition = frontDoor;
                ctrl.HouseName = "Casa_" + (i + 1);
                ctrl.EsCliente = i != 4 && i != 9; // el padre y la vieja Elvira no traen carro

                var col = npc.AddComponent<CapsuleCollider>();
                col.height = 1.9f;
                col.center = new Vector3(0f, 0.95f, 0f);
                col.radius = 0.4f;
            }
        }

        static void BuildPrefabs()
        {
            string resDir = "Assets/PuntoMuerto/Resources";
            Directory.CreateDirectory(resDir);

            // PlayerNet prefab (multijugador)
            var p = BuildCharacterVisual("PlayerNet", new Color(0.25f, 0.35f, 0.55f), true);
            var cc = p.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.center = new Vector3(0f, 0.95f, 0f); cc.radius = 0.35f;
            p.AddComponent<PuntoMuerto.PlayerController>();
            p.AddComponent<PuntoMuerto.PlayerInteraction>();
            p.AddComponent<Unity.Netcode.NetworkObject>();
            p.AddComponent<PuntoMuerto.ClientAuthTransform>();
            p.AddComponent<PuntoMuerto.PlayerNet>();
            PrefabUtility.SaveAsPrefabAsset(p, resDir + "/PlayerNet.prefab");
            Object.DestroyImmediate(p);

            // GameSync prefab (estado compartido)
            var s = new GameObject("GameSyncNet");
            s.AddComponent<Unity.Netcode.NetworkObject>();
            s.AddComponent<PuntoMuerto.GameSync>();
            PrefabUtility.SaveAsPrefabAsset(s, resDir + "/GameSyncNet.prefab");
            Object.DestroyImmediate(s);
        }

        static void BakeNavMesh(Transform world)
        {
            var surface = world.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.layerMask = ~0;
            surface.BuildNavMesh();
        }

        public static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);

            var ui = new GameObject("UI");
            ui.AddComponent<PuntoMuerto.UIRoot>();
            ui.AddComponent<PuntoMuerto.MainMenu>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("Escena MainMenu construida y guardada.");
        }

        static void SetupBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/LosAlisos.unity", true)
            };
        }
    }
}
