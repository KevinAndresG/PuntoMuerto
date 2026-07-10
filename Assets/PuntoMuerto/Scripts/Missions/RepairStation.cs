using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuntoMuerto
{
    public enum StationKind { Bahia, Patio, Pintura }

    /// <summary>Estación de trabajo: mantén E cerca para avanzar la misión asignada.</summary>
    public class RepairStation : MonoBehaviour, IInteractable
    {
        public static readonly List<RepairStation> All = new List<RepairStation>();

        public StationKind Kind = StationKind.Bahia;
        public bool Unlocked = true;
        public Mission CurrentMission;

        Transform player;
        bool working;
        float coveredUntil;
        GameObject sparks;

        public string Prompt
        {
            get
            {
                if (CurrentMission == null) return null;
                if (Kind == StationKind.Patio && !EsDeNoche) return CurrentMission.Title + " (espera la noche)";
                if (Time.time < coveredUntil) return "Trabajo cubierto...";
                return "Trabajar: " + CurrentMission.Title + " " + Mathf.RoundToInt(CurrentMission.Progress01 * 100f) + "%";
            }
        }

        public bool CanInteract => CurrentMission != null &&
            (Kind != StationKind.Patio || EsDeNoche) && Time.time >= coveredUntil;

        bool EsDeNoche => DayNightCycle.I != null && DayNightCycle.I.IsNight;

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void Interact(PlayerInteraction p)
        {
            if (CurrentMission == null) return;
            // pintura => minijuego de lijado
            if (CurrentMission.Title.Contains("Pintura") || Kind == StationKind.Pintura)
            {
                var mg = Object.FindFirstObjectByType<SandingMinigameUI>();
                if (mg != null) { mg.Open(CurrentMission, this); return; }
            }
            player = p.transform;
            working = true;
        }

        void Update()
        {
            if (!working || CurrentMission == null) { SetSparks(false); return; }
            var kb = Keyboard.current;
            bool holding = kb != null && kb.eKey.isPressed;
            bool near = player != null && Vector3.Distance(player.position, transform.position) < 3.2f;
            if (!holding || !near || Time.time < coveredUntil ||
                (Kind == StationKind.Patio && !EsDeNoche) || UIRoot.ModalOpen)
            {
                working = false;
                SetSparks(false);
                return;
            }

            SetSparks(CurrentMission.EsIlegal && CurrentMission.Noise > 0.4f);
            bool done = MissionSystem.I.DoWork(CurrentMission, Time.deltaTime);
            HUDController.SetWorkProgress(CurrentMission.Progress01, CurrentMission.Title,
                CurrentMission.EsIlegal ? CurrentMission.Noise : 0f);
            if (done)
            {
                CurrentMission = null;
                working = false;
                SetSparks(false);
                HUDController.SetWorkProgress(-1f, null, 0f);
            }
        }

        /// <summary>Lona: tapa el trabajo unos segundos (el espía pierde interés).</summary>
        public void Cover(float seconds) { coveredUntil = Time.time + seconds; working = false; }

        void SetSparks(bool on)
        {
            if (on && sparks == null)
            {
                sparks = new GameObject("SopleteLight");
                sparks.transform.SetParent(transform, false);
                sparks.transform.localPosition = Vector3.up * 1.2f;
                var l = sparks.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1f, 0.7f, 0.3f);
                l.range = 7f;
                l.intensity = 3.5f;
                var flick = sparks.AddComponent<LightFlicker>();
                flick.BaseIntensity = 3.5f;
            }
            else if (!on && sparks != null)
            {
                Destroy(sparks);
                sparks = null;
            }
        }
    }

}
