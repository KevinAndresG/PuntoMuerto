using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    [System.Serializable]
    public class SaveData
    {
        public int Day;
        public int CleanMoney;
        public int DirtyMoney;
        public bool IsJefe;
        public int DaysAsJefe;
        public bool Temporada;
        public int TotalEarned;
        public int MissionsCompleted;
        public int DeudaPagada;
        public float Reputacion;
        public float Fabio;
        public int Leverage;
        public int Reportes;
        public int Evidencia;
        public float RiesgoAuditoria;
        public int TotalLavado;
        public List<string> Upgrades = new List<string>();
        public List<int> InvTypes = new List<int>();
        public List<int> InvCounts = new List<int>();
        public List<string> Desaparecidos = new List<string>();
        public List<int> PrestamoCuota = new List<int>();
        public List<int> PrestamoSemanas = new List<int>();
    }

    /// <summary>Guardado persistente en JSON (persistentDataPath).</summary>
    public static class SaveSystem
    {
        static string PathFile => Path.Combine(Application.persistentDataPath, "puntomuerto_save.json");

        public static bool HasSave => File.Exists(PathFile);

        public static void Save()
        {
            var g = GameManager.I; var m = MetasManager.I;
            if (g == null || m == null) return;
            var d = new SaveData
            {
                Day = g.Day,
                CleanMoney = g.CleanMoney,
                DirtyMoney = g.DirtyMoney,
                IsJefe = g.IsJefe,
                DaysAsJefe = g.DaysAsJefe,
                Temporada = g.TemporadaContinua,
                TotalEarned = g.TotalEarned,
                MissionsCompleted = g.MissionsCompleted,
                DeudaPagada = m.DeudaPagada,
                Reputacion = m.Reputacion,
                Fabio = m.Fabio,
                Leverage = m.Leverage,
                Reportes = m.ReportesAFabio,
                Evidencia = m.EvidenciaFalsa,
                RiesgoAuditoria = LedgerSystem.I.RiesgoAuditoria,
                TotalLavado = LedgerSystem.I.TotalLavado,
                Upgrades = UpgradeSystem.I.Compradas.ToList(),
                InvTypes = InventorySystem.I.SaveTypes(),
                InvCounts = InventorySystem.I.SaveCounts(),
                Desaparecidos = NPCManager.I != null ? NPCManager.I.DisappearedNames.ToList() : new List<string>(),
                PrestamoCuota = BankSystem.I.Prestamos.Select(p => p.Cuota).ToList(),
                PrestamoSemanas = BankSystem.I.Prestamos.Select(p => p.SemanasRestantes).ToList()
            };
            File.WriteAllText(PathFile, JsonUtility.ToJson(d, true));
            GameEvents.Notify("Partida guardada.");
        }

        public static void Load()
        {
            if (!HasSave) return;
            var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFile));
            var g = GameManager.I; var m = MetasManager.I;

            g.Day = d.Day;
            g.CleanMoney = d.CleanMoney;
            g.DirtyMoney = d.DirtyMoney;
            g.IsJefe = d.IsJefe;
            g.DaysAsJefe = d.DaysAsJefe;
            g.TemporadaContinua = d.Temporada;
            g.TotalEarned = d.TotalEarned;
            g.MissionsCompleted = d.MissionsCompleted;
            m.DeudaPagada = d.DeudaPagada;
            m.Reputacion = d.Reputacion;
            m.Fabio = d.Fabio;
            m.Leverage = d.Leverage;
            m.ReportesAFabio = d.Reportes;
            m.EvidenciaFalsa = d.Evidencia;
            LedgerSystem.I.RiesgoAuditoria = d.RiesgoAuditoria;
            LedgerSystem.I.TotalLavado = d.TotalLavado;

            UpgradeSystem.I.Compradas = new HashSet<string>(d.Upgrades);
            foreach (var id in d.Upgrades) UpgradeSystem.I.ReapplySceneEffects(id);

            InventorySystem.I.LoadFrom(d.InvTypes, d.InvCounts);

            BankSystem.I.Prestamos.Clear();
            for (int i = 0; i < d.PrestamoCuota.Count; i++)
                BankSystem.I.Prestamos.Add(new Loan { Cuota = d.PrestamoCuota[i], SemanasRestantes = d.PrestamoSemanas[i] });

            if (NPCManager.I != null)
                foreach (var name in d.Desaparecidos)
                    NPCManager.I.DisappearByName(name);

            GameEvents.OnMetasChanged?.Invoke();
            GameEvents.Notify("Partida cargada — Día " + g.Day + ".");
        }

        public static void Delete() { if (HasSave) File.Delete(PathFile); }
    }
}
