using System.Collections.Generic;
using UnityEngine;

namespace PuntoMuerto
{
    public enum StationKind { Bahia, Patio, Pintura }

    /// <summary>Estación de trabajo del patio: E abre el minijuego de trabajo de la misión asignada
    /// (encargos de Fabio). El trabajo sucio solo corre de noche y hace ruido/luz (riesgo cerca).</summary>
    public class RepairStation : MonoBehaviour, IInteractable
    {
        public static readonly List<RepairStation> All = new List<RepairStation>();

        public StationKind Kind = StationKind.Bahia;
        public bool Unlocked = true;
        public Mission CurrentMission;

        float coveredUntil;

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

        public bool CanInteract => CurrentMission != null && CarWorkMinigame.Current == null &&
            (Kind != StationKind.Patio || EsDeNoche) && Time.time >= coveredUntil;

        bool EsDeNoche => DayNightCycle.I != null && DayNightCycle.I.IsNight;

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void Interact(PlayerInteraction p)
        {
            if (CurrentMission == null) return;
            CarWorkMinigame.Begin(CurrentMission, null, this);
        }

        void Update()
        {
            // en red, la completa el host: soltar la misión cuando llegue el espejo
            if (CurrentMission != null && CurrentMission.State == MissionState.Completada)
            {
                CurrentMission = null;
                HUDController.SetWorkProgress(-1f, null, 0f);
            }
        }

        /// <summary>Lona: tapa el trabajo unos segundos (el espía pierde interés).</summary>
        public void Cover(float seconds) { coveredUntil = Time.time + seconds; }
    }

}
