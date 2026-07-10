using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Sistema de la cerca (GDD 4.3): espionaje mientras haces trabajo ilegal de noche.</summary>
    public class FenceSpySystem : MonoBehaviour
    {
        public static FenceSpySystem I { get; private set; }

        [Range(0f, 1f)] public float NoiseLevel;   // lo suben las estaciones ilegales activas
        public float ReactionWindow = 14f;

        public NPCController ActiveSpy { get; private set; }
        public float SpyTimer { get; private set; }
        public bool SpyArrived { get; private set; }

        Vector3 spyPoint;
        float accumulatedChance;

        void Awake() { I = this; }

        void Start()
        {
            var sp = GameObject.Find("Taller/SpyPoint");
            spyPoint = sp != null ? sp.transform.position : new Vector3(52f, 0f, -38f);
        }

        void Update()
        {
            if (GameManager.I == null || GameManager.I.GamePaused) return;
            if (!Net.IsAuthority) return; // el host simula a los espías (el ruido remoto llega por RPC)

            if (ActiveSpy != null)
            {
                UpdateActiveSpy();
                return;
            }

            if (NoiseLevel <= 0.01f) { accumulatedChance = 0f; return; }

            // de día hay más ojos en la calle: trabajar sucio a plena luz es más arriesgado
            bool night = DayNightCycle.I != null && DayNightCycle.I.IsNight;
            float dayMult = night ? 1f : 1.6f;
            float fenceMult = UpgradeSystem.I != null ? UpgradeSystem.I.FenceRiskMultiplier : 1f;
            accumulatedChance += Time.deltaTime * (0.006f + NoiseLevel * 0.02f) * fenceMult * dayMult;
            if (Random.value < accumulatedChance * Time.deltaTime * 10f)
            {
                accumulatedChance = 0f;
                var npc = NPCManager.I != null ? NPCManager.I.RandomNPC() : null;
                if (npc != null && npc.Activity != NPCActivity.EnCasa)
                {
                    ActiveSpy = npc;
                    SpyArrived = false;
                    SpyTimer = ReactionWindow;
                    npc.GoSpy(spyPoint);
                    GameEvents.OnSpyStarted?.Invoke(npc);
                    // sin aviso: al jugador le toca estar pendiente de la cerca
                }
            }
        }

        void UpdateActiveSpy()
        {
            if (ActiveSpy.Activity == NPCActivity.Desaparecido) { ClearSpy(); return; }

            if (!SpyArrived)
            {
                if (ActiveSpy.ReachedSpyPoint(spyPoint)) SpyArrived = true;
                return;
            }

            // si dejaste de hacer ruido, se aburre y se va con sospecha mínima
            if (NoiseLevel <= 0.01f)
            {
                ActiveSpy.Suspicion.Add(3f);
                GameEvents.Notify(ActiveSpy.NpcName + " se fue sin ver nada claro (+3 sospecha).");
                ClearSpy();
                return;
            }

            SpyTimer -= Time.deltaTime;
            if (SpyTimer <= 0f)
            {
                // vio lo que no debía (GDD: +10..25 según qué tan comprometedor)
                float amount = Mathf.Lerp(10f, 25f, NoiseLevel);
                ActiveSpy.Suspicion.Add(amount);
                MetasManager.I.CambiarReputacion(Random.Range(-8f, -3f),
                    ActiveSpy.NpcName + " vio algo por la cerca");
                ClearSpy();
            }
        }

        /// <summary>Confrontar al espía antes de que registre la sospecha.</summary>
        public void ConfrontSpy()
        {
            if (ActiveSpy == null) return;
            ActiveSpy.Suspicion.Add(2f);
            GameEvents.Notify("Distrajiste a " + ActiveSpy.NpcName + " a tiempo (+2 sospecha leve).");
            ClearSpy();
        }

        void ClearSpy()
        {
            if (ActiveSpy != null)
            {
                ActiveSpy.EndSpy();
                GameEvents.OnSpyResolved?.Invoke(ActiveSpy);
            }
            ActiveSpy = null;
            SpyArrived = false;
        }

        // Las estaciones registran su ruido cada frame mientras trabajan
        float noiseThisFrame;
        void LateUpdate()
        {
            NoiseLevel = noiseThisFrame;
            noiseThisFrame = 0f;
        }
        public void RegisterNoise(float noise)
        {
            noiseThisFrame = Mathf.Max(noiseThisFrame, noise);
        }
    }
}
