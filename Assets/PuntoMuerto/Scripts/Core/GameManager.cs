using UnityEngine;

namespace PuntoMuerto
{
    public enum GamePhase { Manana, Tarde, Noche }

    /// <summary>Estado global de la partida: día, dinero, pausa.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Estado")]
        public int Day = 1;
        public int CleanMoney = 800;
        public int DirtyMoney = 0;
        public bool IsJefe;
        public int DaysAsJefe;
        public bool TemporadaContinua;

        [Header("Estadísticas")]
        public int TotalEarned;
        public int MissionsCompleted;

        // ganancias del día (para el Libro y el resumen)
        [HideInInspector] public int DayCleanEarned;
        [HideInInspector] public int DayDirtyEarned;

        public bool GamePaused { get; private set; }
        public int TotalMoney => CleanMoney + DirtyMoney;

        /// <summary>El menú lo activa al pulsar CONTINUAR.</summary>
        public static bool LoadRequested;

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
        }

        void Start()
        {
            if (LoadRequested)
            {
                LoadRequested = false;
                SaveSystem.Load();
            }
            GameEvents.OnDayStart?.Invoke(Day);
            GameEvents.Notify("Día " + Day + " — Los Alisos. El taller abre a las 8:00.");
        }

        public void AddMoney(int amount, bool dirty)
        {
            if (dirty) { DirtyMoney += amount; if (amount > 0) DayDirtyEarned += amount; }
            else { CleanMoney += amount; if (amount > 0) DayCleanEarned += amount; }
            if (amount > 0) TotalEarned += amount;
            GameEvents.OnMetasChanged?.Invoke();
        }

        public bool Spend(int amount, bool preferDirty = false)
        {
            if (TotalMoney < amount) return false;
            if (preferDirty)
            {
                int d = Mathf.Min(DirtyMoney, amount);
                DirtyMoney -= d; amount -= d;
                CleanMoney -= amount;
            }
            else
            {
                int c = Mathf.Min(CleanMoney, amount);
                CleanMoney -= c; amount -= c;
                DirtyMoney -= amount;
            }
            GameEvents.OnMetasChanged?.Invoke();
            return true;
        }

        /// <summary>Cambio de día sin pausas ni resúmenes (medianoche o al dormir).</summary>
        public void AdvanceDay()
        {
            Day++;
            if (IsJefe) DaysAsJefe++;
            DayCleanEarned = 0;
            DayDirtyEarned = 0;
            GameEvents.OnDayStart?.Invoke(Day);
            GameEvents.Notify("Día " + Day + ".");
            SaveSystem.Save(); // autoguardado al cambiar de día
        }

        public void SetPaused(bool paused)
        {
            GamePaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
