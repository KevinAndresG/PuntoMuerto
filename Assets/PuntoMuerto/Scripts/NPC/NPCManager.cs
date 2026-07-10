using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Registro global de NPCs: desapariciones, piso de sospecha, puntos de andén.</summary>
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager I { get; private set; }

        public float GlobalSuspicionFloor;
        public List<NPCController> NPCs = new List<NPCController>();
        public List<string> DisappearedNames = new List<string>();

        Transform[] sidewalkPoints;
        readonly List<NPCController> pendingDisappear = new List<NPCController>();

        public Transform[] SidewalkPoints
        {
            get
            {
                if (sidewalkPoints == null || sidewalkPoints.Length == 0)
                {
                    var root = GameObject.Find("Waypoints/Andenes");
                    if (root != null)
                    {
                        sidewalkPoints = new Transform[root.transform.childCount];
                        for (int i = 0; i < root.transform.childCount; i++)
                            sidewalkPoints[i] = root.transform.GetChild(i);
                    }
                }
                return sidewalkPoints;
            }
        }

        void Awake() { I = this; }
        void OnEnable() { GameEvents.OnDayStart += OnDayStart; GameEvents.OnHourTick += OnHourTick; }
        void OnDisable() { GameEvents.OnDayStart -= OnDayStart; GameEvents.OnHourTick -= OnHourTick; }

        /// <summary>Al caer la noche, a veces alguien se queda rondando (riesgo nocturno).</summary>
        void OnHourTick(float hour)
        {
            if (Mathf.FloorToInt(hour) != 20 || Random.value > 0.4f) return;
            var owl = RandomNPC();
            if (owl != null && owl.Activity != NPCActivity.EnCasa)
            {
                owl.BecomeNightOwl(Random.Range(22.5f, 24f));
                GameEvents.Notify("Hay alguien rondando por el pueblo a esta hora...");
            }
        }

        public void Register(NPCController npc)
        {
            if (!NPCs.Contains(npc)) NPCs.Add(npc);
        }

        public NPCController RandomNPC(NPCController except = null)
        {
            var vivos = NPCs.Where(n => n != except && n.Activity != NPCActivity.Desaparecido).ToList();
            return vivos.Count == 0 ? null : vivos[Random.Range(0, vivos.Count)];
        }

        public NPCController RandomClient()
        {
            var vivos = NPCs.Where(n => n.EsCliente && n.Activity != NPCActivity.Desaparecido).ToList();
            return vivos.Count == 0 ? null : vivos[Random.Range(0, vivos.Count)];
        }

        /// <summary>Reportar a Fabio: 1-2 días de normalidad aparente, luego desaparece (GDD 4.4.4).</summary>
        public void ScheduleDisappearance(NPCController npc)
        {
            npc.DisappearOnDay = GameManager.I.Day + Random.Range(1, 3);
            pendingDisappear.Add(npc);
            GameEvents.Notify("Fabio: \"Entendido. No pienses más en eso.\"");
        }

        void OnDayStart(int day)
        {
            for (int i = pendingDisappear.Count - 1; i >= 0; i--)
            {
                var npc = pendingDisappear[i];
                if (npc == null) { pendingDisappear.RemoveAt(i); continue; }
                if (day == npc.DisappearOnDay - 1 && npc.DisappearOnDay > day)
                {
                    GameEvents.Notify("De noche, un auto desconocido se estacionó un momento frente a la casa de " + npc.NpcName + "...");
                }
                else if (day >= npc.DisappearOnDay)
                {
                    DoDisappear(npc);
                    pendingDisappear.RemoveAt(i);
                }
            }

            // pueblo más alerta si la reputación colectiva es baja (GDD 4.4.1)
            if (day % 7 == 0 && MetasManager.I != null && MetasManager.I.Reputacion < 0f)
            {
                foreach (var n in NPCs)
                    if (n.Activity != NPCActivity.Desaparecido)
                        n.Suspicion.Add(Random.Range(1f, 2f));
            }
        }

        /// <summary>Aplica una desaparición ya consumada (al cargar partida).</summary>
        public void DisappearByName(string npcName)
        {
            var npc = NPCs.FirstOrDefault(n => n.NpcName == npcName);
            if (npc != null && npc.Activity != NPCActivity.Desaparecido) DoDisappear(npc);
        }

        void DoDisappear(NPCController npc)
        {
            if (!DisappearedNames.Contains(npc.NpcName)) DisappearedNames.Add(npc.NpcName);
            npc.Disappear();
            GameEvents.OnNPCDisappeared?.Invoke(npc);
            GameEvents.Notify(npc.NpcName + " no apareció hoy. Su casa está a oscuras.");

            if (!string.IsNullOrEmpty(npc.HouseName))
            {
                var house = GameObject.Find(npc.HouseName);
                if (house != null)
                {
                    foreach (var l in house.GetComponentsInChildren<Light>(true)) l.enabled = false;
                    foreach (var nl in house.GetComponentsInChildren<NightLight>(true)) nl.enabled = false;
                    CreateSeVendeSign(house.transform);
                }
            }
        }

        void CreateSeVendeSign(Transform house)
        {
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "SeVende";
            sign.transform.SetParent(house, false);
            sign.transform.localPosition = new Vector3(0f, 0.9f, -4.2f);
            sign.transform.localScale = new Vector3(1.4f, 0.8f, 0.06f);
            var r = sign.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.SetColor("_BaseColor", new Color(0.9f, 0.9f, 0.85f));

            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.SetParent(sign.transform, false);
            post.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            post.transform.localScale = new Vector3(0.05f, 1.2f, 0.8f);
            post.GetComponent<Renderer>().material = r.material;

            var textGo = new GameObject("Texto");
            textGo.transform.SetParent(sign.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            textGo.transform.localScale = new Vector3(0.06f, 0.15f, 1f);
            var tm = textGo.AddComponent<TextMesh>();
            tm.text = "SE VENDE";
            tm.fontSize = 48;
            tm.color = new Color(0.75f, 0.1f, 0.1f);
            tm.anchor = TextAnchor.MiddleCenter;
        }
    }
}
