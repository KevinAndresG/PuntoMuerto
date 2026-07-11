using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Punto de recolección (familia C): recoger paquete y llevarlo al patio.
    /// En multijugador el host es la autoridad: los paquetes se espejan en los clientes
    /// y cualquiera de los dos socios puede recoger/entregar (el equipo carga como uno).</summary>
    public class PickupPoint : MonoBehaviour, IInteractable
    {
        public Mission Mission;
        public bool IsDropoff;

        public string Prompt => IsDropoff ? "Entregar el paquete" : "Recoger el paquete";
        public bool CanInteract => Mission != null && Mission.State != MissionState.Completada &&
            (IsDropoff ? MissionSystem.I.CarryingPickup == Mission : MissionSystem.I.CarryingPickup == null);

        public void Interact(PlayerInteraction p)
        {
            if (Net.IsClientOnly)
            {
                GameSync.RequestPickup(Mission.Id, IsDropoff); // el host ejecuta y espeja
                return;
            }
            DoInteract();
        }

        /// <summary>Lógica real (solo autoridad): también la invoca GameSync cuando pide un cliente.</summary>
        public void DoInteract()
        {
            if (Mission == null) return;
            GameSync.MirrorPickupTaken(Mission.Id, IsDropoff);
            if (!IsDropoff)
            {
                MissionSystem.I.CarryingPickup = Mission;
                GameEvents.Notify("Paquete recogido. Llévalo al patio del taller sin llamar la atención.");
                SpawnDropoff(Mission);
                Destroy(gameObject);
            }
            else
            {
                MissionSystem.I.CarryingPickup = null;
                MissionSystem.I.Complete(Mission);
                Destroy(gameObject);
            }
        }

        public static void SpawnForMission(Mission m)
        {
            var root = GameObject.Find("Waypoints/Recoleccion");
            Vector3 pos = new Vector3(-40f, 0f, 30f);
            if (root != null && root.transform.childCount > 0)
                pos = root.transform.GetChild(Random.Range(0, root.transform.childCount)).position;
            Create(m, pos, false);
            GameSync.MirrorPickupSpawn(m, pos, false);
            GameEvents.Notify("Punto de recogida marcado: busca el paquete en el pueblo.");
        }

        static void SpawnDropoff(Mission m)
        {
            var patio = GameObject.Find("Taller/PatioCentro");
            Vector3 pos = patio != null ? patio.transform.position : new Vector3(48f, 0f, -34f);
            Create(m, pos, true);
            GameSync.MirrorPickupSpawn(m, pos, true);
        }

        /// <summary>Espejo en el cliente: crea el paquete sin volver a espejar.</summary>
        public static void CreateLocal(Mission m, Vector3 pos, bool dropoff) => Create(m, pos, dropoff);

        static void Create(Mission m, Vector3 pos, bool dropoff)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = dropoff ? "EntregaPaquete" : "PaqueteRecoleccion";
            go.transform.position = pos + Vector3.up * 0.3f;
            go.transform.localScale = new Vector3(0.7f, 0.6f, 0.7f);
            var r = go.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.SetColor("_BaseColor", dropoff ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.55f, 0.4f, 0.25f));
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", dropoff ? new Color(0f, 0.3f, 0f) : new Color(0.3f, 0.2f, 0f));
            var pp = go.AddComponent<PickupPoint>();
            pp.Mission = m;
            pp.IsDropoff = dropoff;
            var col = go.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = Vector3.one * 2.5f;
        }
    }
}
