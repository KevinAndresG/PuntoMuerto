using System;

namespace PuntoMuerto
{
    /// <summary>Hub central de eventos del juego.</summary>
    public static class GameEvents
    {
        public static Action<int> OnDayStart;
        public static Action<int> OnDayEnd;
        public static Action<float> OnHourTick;
        public static Action OnMetasChanged;
        public static Action<Mission> OnMissionAdded;
        public static Action<Mission> OnMissionCompleted;
        public static Action<Mission> OnMissionFailed;
        public static Action<NPCController> OnSpyStarted;
        public static Action<NPCController> OnSpyResolved;
        public static Action<NPCController> OnNPCDisappeared;
        public static Action<string> OnNotification;
        public static Action OnUpgradesChanged;

        public static void Notify(string msg) => OnNotification?.Invoke(msg);

        /// <summary>Limpia todas las suscripciones (al cargar escena nueva).</summary>
        public static void Clear()
        {
            OnDayStart = null; OnDayEnd = null; OnHourTick = null;
            OnMetasChanged = null; OnMissionAdded = null; OnMissionCompleted = null;
            OnMissionFailed = null; OnSpyStarted = null; OnSpyResolved = null;
            OnNPCDisappeared = null; OnNotification = null; OnUpgradesChanged = null;
        }
    }
}
