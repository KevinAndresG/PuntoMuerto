using System.Collections.Generic;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Tráfico ambiental: carros civiles en loop y patrulla de policía.</summary>
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager I { get; private set; }

        public int MaxCars = 4;
        readonly List<VehicleAI> cars = new List<VehicleAI>();
        VehicleAI patrol;
        float patrolCheck;

        static readonly Color[] CarColors =
        {
            new Color(0.7f, 0.25f, 0.2f), new Color(0.25f, 0.35f, 0.55f),
            new Color(0.85f, 0.8f, 0.7f), new Color(0.3f, 0.45f, 0.3f),
            new Color(0.5f, 0.5f, 0.55f), new Color(0.75f, 0.55f, 0.25f)
        };

        void Awake() { I = this; }

        void Start()
        {
            InvokeRepeating(nameof(TrySpawnCar), 2f, 10f);
        }

        void TrySpawnCar()
        {
            cars.RemoveAll(c => c == null);
            bool night = DayNightCycle.I != null && DayNightCycle.I.IsNight;
            int target = night ? 1 : MaxCars;
            if (cars.Count >= target) return;

            var path = WaypointPath.Find(Random.value > 0.5f ? "Waypoints/ViaLoopA" : "Waypoints/ViaLoopB");
            if (path == null || path.Points.Length < 2) return;

            var car = BuildCar(CarColors[Random.Range(0, CarColors.Length)], false);
            var ai = car.AddComponent<VehicleAI>();
            ai.Speed = Random.Range(5.5f, 9.5f);
            ai.MaxLaps = Random.Range(1, 3); // 1-2 vueltas y se va: tráfico variado, sin trenes eternos
            // arranca en un punto aleatorio del loop, rumbo al siguiente waypoint
            ai.SetPath(path.Points, true, false, Random.Range(0, path.Points.Length));
            cars.Add(ai);
        }

        void Update()
        {
            // patrulla: frecuencia según Calor (GDD 2.2, umbral -20)
            patrolCheck -= Time.deltaTime;
            if (patrolCheck <= 0f)
            {
                patrolCheck = 30f;
                bool nightPatrol = DayNightCycle.I != null && DayNightCycle.I.IsNight;
                // de noche ronda más seguido: riesgo nocturno (menos ojos, pero la ley pasa)
                bool wantPatrol = MetasManager.I != null &&
                    (MetasManager.I.Reputacion <= -20f || Random.value < (nightPatrol ? 0.45f : 0.2f));
                if (wantPatrol && patrol == null)
                {
                    var path = WaypointPath.Find("Waypoints/ViaLoopA");
                    if (path != null)
                    {
                        var car = BuildCar(new Color(0.15f, 0.2f, 0.45f), true);
                        patrol = car.AddComponent<VehicleAI>();
                        patrol.Speed = MetasManager.I != null && MetasManager.I.Reputacion <= -20f ? 5f : 7f;
                        patrol.SetPath(path.Points, true);
                    }
                }
                else if (!wantPatrol && patrol != null && MetasManager.I.Reputacion > -20f)
                {
                    Destroy(patrol.gameObject);
                    patrol = null;
                }
            }
        }

        /// <summary>Carro de cliente: entra de la vía al parqueadero del taller.</summary>
        public GameObject SpawnClientCar(Color color, Vector3 parkSpot)
        {
            var entry = GameObject.Find("Waypoints/EntradaTaller");
            var car = BuildCar(color, false);
            var ai = car.AddComponent<VehicleAI>();
            ai.Speed = 6f;
            Transform[] path;
            if (entry != null && entry.transform.childCount > 0)
            {
                path = new Transform[entry.transform.childCount + 1];
                for (int i = 0; i < entry.transform.childCount; i++) path[i] = entry.transform.GetChild(i);
                var end = new GameObject("ParkSpot").transform;
                end.position = parkSpot;
                path[path.Length - 1] = end;
            }
            else
            {
                var end = new GameObject("ParkSpot").transform;
                end.position = parkSpot;
                path = new Transform[] { end };
            }
            ai.SetPath(path, false);
            return car;
        }

        /// <summary>Carro de cliente sucio: entra por la rendija sur de la cerca al patio.</summary>
        public GameObject SpawnDirtyClientCar(Color color, Vector3 parkSpot)
        {
            var car = BuildCar(color, false);
            var ai = car.AddComponent<VehicleAI>();
            ai.Speed = 5.5f;
            var p0 = new GameObject("dirty0").transform; p0.position = new Vector3(39f, 0.08f, -70f);
            var p1 = new GameObject("dirty1").transform; p1.position = new Vector3(39f, 0.08f, -50f);
            var p2 = new GameObject("dirty2").transform; p2.position = new Vector3(39f, 0.08f, -45.5f); // rendija
            var p3 = new GameObject("dirty3").transform; p3.position = parkSpot;
            ai.SetPath(new[] { p0, p1, p2, p3 }, false);
            Destroy(p0.gameObject, 60f);
            Destroy(p1.gameObject, 60f);
            Destroy(p2.gameObject, 60f);
            Destroy(p3.gameObject, 60f);
            return car;
        }

        /// <summary>Salida del patio: por la rendija sur hacia el anillo y desaparece.</summary>
        public void DriveOffBack(GameObject car)
        {
            if (car == null) return;
            var ai = car.GetComponent<VehicleAI>();
            if (ai == null) ai = car.AddComponent<VehicleAI>();
            ai.Speed = 6f;
            var p0 = new GameObject("bexit0").transform; p0.position = car.transform.position;
            var p1 = new GameObject("bexit1").transform; p1.position = new Vector3(39f, 0.08f, -42.5f); // frente a la rendija
            var p2 = new GameObject("bexit2").transform; p2.position = new Vector3(39f, 0.08f, -50f);
            var p3 = new GameObject("bexit3").transform; p3.position = new Vector3(20f, 0.08f, -62f);
            ai.SetPath(new[] { p0, p1, p2, p3 }, false, true);
            Destroy(p0.gameObject, 40f);
            Destroy(p1.gameObject, 40f);
            Destroy(p2.gameObject, 40f);
            Destroy(p3.gameObject, 40f);
        }

        /// <summary>Carro rechazado/cansado: arranca de donde está, sale a la calle y desaparece.</summary>
        public void DriveOff(GameObject car)
        {
            if (car == null) return;
            var ai = car.GetComponent<VehicleAI>();
            if (ai == null) ai = car.AddComponent<VehicleAI>();
            ai.Speed = 7f;
            var p0 = new GameObject("exit0").transform; p0.position = car.transform.position;
            var p1 = new GameObject("exit1").transform; p1.position = new Vector3(48f, 0.08f, 2f);
            var p2 = new GameObject("exit2").transform; p2.position = new Vector3(75f, 0.08f, 2f);
            ai.SetPath(new[] { p0, p1, p2 }, false, true);
            Destroy(p0.gameObject, 40f);
            Destroy(p1.gameObject, 40f);
            Destroy(p2.gameObject, 40f);
        }

        public static GameObject BuildCar(Color color, bool police)
        {
            var root = new GameObject(police ? "Patrulla" : "Carro");
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.6f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(1.7f, 0.55f, 3.8f);
            body.GetComponent<Renderer>().material = mat;

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 0.95f, -0.3f);
            cabin.transform.localScale = new Vector3(1.5f, 0.5f, 1.8f);
            var cabinMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cabinMat.SetColor("_BaseColor", new Color(0.15f, 0.18f, 0.22f));
            cabinMat.SetFloat("_Smoothness", 0.8f);
            cabin.GetComponent<Renderer>().material = cabinMat;

            var wheelMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wheelMat.SetColor("_BaseColor", new Color(0.08f, 0.08f, 0.08f));
            for (int i = 0; i < 4; i++)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                w.name = "Wheel" + i;
                w.transform.SetParent(root.transform, false);
                float x = i % 2 == 0 ? -0.85f : 0.85f;
                float z = i < 2 ? 1.2f : -1.2f;
                w.transform.localPosition = new Vector3(x, 0.32f, z);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                w.transform.localScale = new Vector3(0.64f, 0.12f, 0.64f);
                w.GetComponent<Renderer>().material = wheelMat;
                Object.Destroy(w.GetComponent<Collider>());
            }

            if (police)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "LightBar";
                bar.transform.SetParent(root.transform, false);
                bar.transform.localPosition = new Vector3(0f, 1.3f, -0.3f);
                bar.transform.localScale = new Vector3(0.9f, 0.15f, 0.35f);
                var barMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                barMat.SetColor("_BaseColor", Color.white);
                barMat.EnableKeyword("_EMISSION");
                barMat.SetColor("_EmissionColor", new Color(1.5f, 0.2f, 0.2f));
                bar.GetComponent<Renderer>().material = barMat;
                bar.AddComponent<PoliceLightBar>();

                var l1 = new GameObject("RedLight").AddComponent<Light>();
                l1.transform.SetParent(bar.transform, false);
                l1.transform.localPosition = new Vector3(-0.4f, 0.5f, 0f);
                l1.type = LightType.Point; l1.color = Color.red; l1.range = 6f; l1.intensity = 3f;
                var l2 = new GameObject("BlueLight").AddComponent<Light>();
                l2.transform.SetParent(bar.transform, false);
                l2.transform.localPosition = new Vector3(0.4f, 0.5f, 0f);
                l2.type = LightType.Point; l2.color = Color.blue; l2.range = 6f; l2.intensity = 3f;
            }

            // colisionador simple del carro completo
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.7f, 0f);
            col.size = new Vector3(1.8f, 1.4f, 4f);

            return root;
        }
    }

    /// <summary>Alterna luces rojas/azules de la patrulla.</summary>
    public class PoliceLightBar : MonoBehaviour
    {
        Light[] lights;
        float t;

        void Start() { lights = GetComponentsInChildren<Light>(); }

        void Update()
        {
            t += Time.deltaTime * 6f;
            bool phase = Mathf.FloorToInt(t) % 2 == 0;
            if (lights == null || lights.Length < 2) return;
            lights[0].enabled = phase;
            lights[1].enabled = !phase;
        }
    }
}
