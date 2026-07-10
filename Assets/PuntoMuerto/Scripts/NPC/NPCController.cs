using UnityEngine;
using UnityEngine.AI;

namespace PuntoMuerto
{
    public enum NPCActivity { Deambular, IrACasa, EnCasa, Espiar, VisitarTaller, Desaparecido }

    /// <summary>NPC del pueblo: rutina por andenes, casa de noche, visitas al taller, espionaje.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCController : MonoBehaviour, IInteractable
    {
        public string NpcName = "Vecino";
        public Vector3 HomePosition;
        public string HouseName;
        public bool EsCliente = true;
        public NPCActivity Activity = NPCActivity.Deambular;

        [HideInInspector] public NPCSuspicion Suspicion;
        [HideInInspector] public int DisappearOnDay = -1;
        [HideInInspector] public float OwlUntil = -1f; // >0: esta noche sigue en la calle hasta esa hora

        NavMeshAgent agent;
        float waitUntil;
        float goHomeDeadline;
        bool hiddenAtHome;
        Vector3 spyReturnPos;
        Transform tallerTarget;
        bool dialoguePaused;

        public string Prompt => "Hablar con " + NpcName +
            (Suspicion != null && Suspicion.CurrentState != SuspicionState.Normal ? " (" + Suspicion.StateLabel + ")" : "");
        public bool CanInteract => Activity != NPCActivity.Desaparecido && Activity != NPCActivity.EnCasa;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            Suspicion = GetComponent<NPCSuspicion>();
            if (Suspicion == null) Suspicion = gameObject.AddComponent<NPCSuspicion>();
        }

        void Start()
        {
            // si el spawn quedó fuera del NavMesh (junto a la casa), reubicar al punto más cercano
            if (agent != null && !agent.isOnNavMesh &&
                NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 8f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            if (NPCManager.I != null) NPCManager.I.Register(this);
        }

        void Update()
        {
            if (Activity == NPCActivity.Desaparecido) return;
            if (GameManager.I == null || GameManager.I.GamePaused) return;
            if (agent == null || !agent.isOnNavMesh) return;
            if (dialoguePaused) return; // quieto mientras habla con el jugador

            float hour = DayNightCycle.I != null ? DayNightCycle.I.Hour : 12f;

            switch (Activity)
            {
                case NPCActivity.Deambular:
                    bool esNoche = hour >= 19f || hour < 6f;
                    bool trasnochando = OwlUntil > 0f && hour >= 19f && hour < OwlUntil;
                    if (esNoche && !trasnochando) { GoHome(); break; }
                    if (!agent.pathPending && agent.remainingDistance < 0.6f)
                    {
                        if (Time.time > waitUntil) PickWanderTarget();
                    }
                    break;

                case NPCActivity.IrACasa:
                    if (!agent.pathPending && agent.remainingDistance < 0.8f) EnterHouse();
                    else if (Time.time > goHomeDeadline)
                    {
                        // no encontró camino: que no se quede parado afuera toda la noche
                        if (NavMesh.SamplePosition(HomePosition, out NavMeshHit home, 6f, NavMesh.AllAreas))
                            agent.Warp(home.position);
                        EnterHouse();
                    }
                    break;

                case NPCActivity.EnCasa:
                    if (hour >= 7f && hour < 19f) LeaveHouse();
                    break;

                case NPCActivity.Espiar:
                    // FenceSpySystem controla la salida de este estado
                    break;

                case NPCActivity.VisitarTaller:
                    if (!agent.pathPending && agent.remainingDistance < 0.8f && Time.time > waitUntil)
                    {
                        Activity = NPCActivity.Deambular;
                        PickWanderTarget();
                    }
                    break;
            }
        }

        void PickWanderTarget()
        {
            var pts = NPCManager.I != null ? NPCManager.I.SidewalkPoints : null;
            if (pts == null || pts.Length == 0) return;
            var p = pts[Random.Range(0, pts.Length)];
            agent.SetDestination(p.position);
            waitUntil = Time.time + Random.Range(1f, 6f);
        }

        public void GoHome()
        {
            Activity = NPCActivity.IrACasa;
            goHomeDeadline = Time.time + 75f;
            if (agent.isOnNavMesh) agent.SetDestination(HomePosition);
        }

        /// <summary>Ronda nocturna: se queda deambulando hasta la hora dada.</summary>
        public void BecomeNightOwl(float untilHour)
        {
            if (Activity == NPCActivity.Desaparecido || Activity == NPCActivity.EnCasa) return;
            OwlUntil = untilHour;
            Activity = NPCActivity.Deambular;
            PickWanderTarget();
        }

        /// <summary>Entra a la casa: se oculta hasta la mañana (nada de estatuas en el andén).</summary>
        void EnterHouse()
        {
            Activity = NPCActivity.EnCasa;
            SetVisible(false);
        }

        void LeaveHouse()
        {
            Activity = NPCActivity.Deambular;
            OwlUntil = -1f;
            SetVisible(true);
            PickWanderTarget();
        }

        void SetVisible(bool v)
        {
            if (hiddenAtHome == !v) return;
            hiddenAtHome = !v;
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = v;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = v;
        }

        public void GoSpy(Vector3 spyPoint)
        {
            if (Activity == NPCActivity.Desaparecido) return;
            SetVisible(true);
            spyReturnPos = transform.position;
            Activity = NPCActivity.Espiar;
            if (agent.isOnNavMesh) { agent.speed = 2.2f; agent.SetDestination(spyPoint); }
        }

        public bool ReachedSpyPoint(Vector3 spyPoint) =>
            Vector3.Distance(transform.position, spyPoint) < 1.5f;

        public void EndSpy()
        {
            if (Activity != NPCActivity.Espiar) return;
            Activity = NPCActivity.Deambular;
            agent.speed = 1.6f;
            if (agent.isOnNavMesh) agent.SetDestination(spyReturnPos);
        }

        public void VisitTaller(Vector3 receptionPoint, float stayMinutes)
        {
            if (Activity == NPCActivity.Desaparecido) return;
            SetVisible(true);
            Activity = NPCActivity.VisitarTaller;
            waitUntil = Time.time + stayMinutes;
            if (agent.isOnNavMesh) agent.SetDestination(receptionPoint);
        }

        public void Disappear()
        {
            Activity = NPCActivity.Desaparecido;
            gameObject.SetActive(false);
        }

        /// <summary>Se detiene y mira al jugador mientras dura el diálogo.</summary>
        public void PauseForDialogue(Transform facing)
        {
            dialoguePaused = true;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            if (facing != null)
            {
                var d = facing.position - transform.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(d);
            }
        }

        public void ResumeAfterDialogue()
        {
            dialoguePaused = false;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        }

        public void Interact(PlayerInteraction player)
        {
            var ui = Object.FindFirstObjectByType<DialogueUI>();
            if (ui != null)
            {
                PauseForDialogue(player.transform);
                ui.Open(this);
            }
        }
    }
}
