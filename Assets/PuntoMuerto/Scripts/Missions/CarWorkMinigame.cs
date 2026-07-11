using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PuntoMuerto
{
    /// <summary>
    /// Minijuego de trabajo 3D: toda misión (honesta o sucia) se resuelve con pasos interactivos
    /// sobre el carro o la mesa (aflojar tuercas, sacar la llanta, drenar aceite, rociar pintura,
    /// limar el VIN, empacar piezas...). La cámara enfoca cada pieza y el mouse hace el trabajo:
    /// clic, mantener clic, o mantener clic y FROTAR (como el lijado del GDD 4.1).
    /// El avance se reporta por fracciones a MissionSystem — compatible con multijugador.
    /// Solo runtime: nunca va serializado en escena.
    /// </summary>
    public class CarWorkMinigame : MonoBehaviour
    {
        public static CarWorkMinigame Current { get; private set; }

        enum Accion { Clic, Mantener, Frotar }

        class Paso
        {
            public string Texto;
            public GameObject Target;
            public Accion Accion;
            public float Duracion = 1f;
            public float Progreso;
            public Vector3 CamPos, CamMira;
            public bool Girar;                      // el target rota con el avance (tornillos/tuercas)
            public Renderer Tinte;                  // lerp de color con el avance (spray/limado)
            public Color TinteDe, TinteA;
            public System.Action AlTerminar;
            public System.Action<float> AlAvanzar;  // 0..1 (fluidos, medidores)
        }

        Mission mission;
        RepairStation station;
        Transform car;
        readonly List<Paso> pasos = new List<Paso>();
        int idx;
        float chunk;
        GameObject rigRoot;
        GameObject workLight;
        Camera cam;
        FirstPersonCamera fpc;
        Vector3 camPrevPos;
        Quaternion camPrevRot;
        Text hint;
        RectTransform hintPanel;
        Material highlightMat;
        Vector3 pilePoint;
        bool finishing;

        // elevador hidráulico (mejora): sube el carro en la bahía 1 para el trabajo bajo chasis
        bool onLift;
        float liftGroundY;
        float liftT;
        const float LiftHeight = 1.15f;
        readonly List<GameObject> liftArms = new List<GameObject>();
        float durScale = 1f;   // el elevador agiliza el trabajo (pasos más cortos)

        // ---------- arranque ----------

        public static void Begin(Mission m, Transform carT, RepairStation st)
        {
            if (Current != null || m == null || m.State == MissionState.Completada) return;
            var go = new GameObject("MinigameTrabajo");
            go.AddComponent<CarWorkMinigame>().Init(m, carT, st);
        }

        void Init(Mission m, Transform carT, RepairStation st)
        {
            Current = this;
            mission = m;
            station = st;
            cam = Camera.main;
            fpc = cam != null ? cam.GetComponent<FirstPersonCamera>() : null;

            car = carT != null ? carT : FindOrSpawnWorkCar();
            pilePoint = car != null
                ? car.TransformPoint(new Vector3(2.3f, 0f, -0.8f))
                : (station != null ? station.transform.position + new Vector3(1.6f, 0f, 1.2f) : transform.position);

            rigRoot = new GameObject("RigTrabajo");

            // elevador: si está comprado y es un trabajo bajo chasis en la bahía 1, subimos el carro
            // ANTES de armar los pasos, para que las anclas de cámara/props se horneen ya elevadas.
            onLift = ShouldUseLift();
            if (onLift)
            {
                durScale = 0.7f;                       // trabajo más cómodo → pasos más rápidos
                liftGroundY = car.position.y;
                car.position += Vector3.up * LiftHeight;
            }

            BuildSteps();
            if (pasos.Count == 0)
            {
                if (onLift) { var p = car.position; p.y = liftGroundY; car.position = p; onLift = false; }
                Cleanup();
                return;
            }

            // chunk fijo por paso (WorkRequired/pasos): estable entre pausas, así al reanudar
            // podemos ubicar exactamente en qué paso quedó la obra.
            if (mission.WorkRequired <= 0f) mission.WorkRequired = pasos.Count;
            chunk = Mathf.Max(0.05f, mission.WorkRequired / pasos.Count);

            UIRoot.PushModal();
            if (fpc != null) fpc.enabled = false;
            if (cam != null) { camPrevPos = cam.transform.position; camPrevRot = cam.transform.rotation; }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            BuildHintUI();
            SetWorkLight(true);
            // reanudar con el carro AÚN elevado: las piezas montadas antes de pausar usan posiciones
            // horneadas a esa altura, así caen en su sitio.
            int start = RestoreProgress();
            if (onLift) SetupLift(animar: start == 0);
            EnterStep(start);
        }

        /// <summary>¿Este trabajo usa el elevador? Requiere la mejora comprada, un carro de cliente
        /// parado en cualquiera de las 3 bahías (todas tienen kit) y una tarea bajo el chasis.</summary>
        bool ShouldUseLift()
        {
            if (car == null || mission == null) return false;
            if (UpgradeSystem.I == null || !UpgradeSystem.I.Tiene("elevador")) return false;
            if (car.GetComponent<CarJob>() == null) return false;          // solo carros de cliente en bahía
            // bahías en x = 26.5, 35, 43.5 (z = -16); el carro debe estar sobre una de ellas
            bool enBahia = false;
            for (int i = 0; i < 3; i++)
            {
                Vector3 d = car.position - new Vector3(26.5f + i * 8.5f, car.position.y, -16f);
                if (d.sqrMagnitude <= 6.25f) { enBahia = true; break; }    // <2.5m de una bahía
            }
            if (!enBahia) return false;
            string t = mission.Title.ToLowerInvariant();
            return t.Contains("aceite") || t.Contains("llanta") || t.Contains("freno")
                || t.Contains("suspensión") || t.Contains("transmisión") || t.Contains("desarme");
        }

        /// <summary>Brazos rojos del elevador bajo el carro. En una obra nueva (animar) el carro
        /// arranca en el piso y sube; al reanudar ya queda arriba.</summary>
        void SetupLift(bool animar)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arm.name = "BrazoElevador";
                Destroy(arm.GetComponent<Collider>());
                arm.transform.SetParent(car, false);
                arm.transform.localPosition = new Vector3(0f, -0.28f, s * 0.95f);
                arm.transform.localScale = new Vector3(2.2f, 0.14f, 0.35f);
                var r = arm.GetComponent<Renderer>();
                r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                r.material.SetColor("_BaseColor", new Color(0.75f, 0.15f, 0.12f));
                liftArms.Add(arm);
            }
            if (animar)
            {
                // el carro arranca en el piso y sube durante el primer segundo (se ve elevar)
                var p = car.position; p.y = liftGroundY; car.position = p;
                liftT = 0f;
                GameEvents.Notify("Elevador hidráulico: el carro sube y el trabajo bajo el chasis es más rápido.");
            }
            else liftT = 1f; // reanudado: el carro ya está arriba
        }

        /// <summary>Obra reanudada tras pausar con ESC: adelanta los pasos ya cubiertos por WorkDone
        /// y aplica sus efectos (piezas al montón, tintes, fluidos) para que el carro se vea como quedó,
        /// no reconstruido desde cero. Devuelve el paso donde continuar.</summary>
        int RestoreProgress()
        {
            int hechos = Mathf.Clamp(Mathf.FloorToInt(mission.WorkDone / chunk + 0.001f), 0, pasos.Count - 1);
            for (int i = 0; i < hechos; i++)
            {
                var p = pasos[i];
                p.Progreso = p.Duracion;
                if (p.Girar && p.Target != null) p.Target.transform.Rotate(0f, 0f, 640f, Space.Self);
                if (p.Tinte != null) p.Tinte.material.SetColor("_BaseColor", p.TinteA);
                p.AlAvanzar?.Invoke(1f);
                p.AlTerminar?.Invoke();
            }
            return hechos;
        }

        /// <summary>Encargos de estación (Fabio) que necesitan carro: aparece parqueado junto al patio.</summary>
        Transform FindOrSpawnWorkCar()
        {
            if (station == null || mission == null) return null;
            if (mission.Type == MissionType.Piezas) return null; // trabajo de mesa
            string carName = "CarroEncargo_" + mission.Id;
            var existing = GameObject.Find(carName);
            if (existing != null) return existing.transform;
            var go = TrafficManager.BuildCar(new Color(0.35f, 0.33f, 0.3f), false);
            go.name = carName;
            go.transform.position = station.transform.position + new Vector3(2.8f, 0f, 2.4f);
            go.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            return go.transform;
        }

        // ---------- loop ----------

        void Update()
        {
            if (finishing) return;
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) { Finish(false); return; }
            if (mission == null) { Finish(false); return; }
            if (mission.State == MissionState.Completada) { Finish(true); return; }

            // el elevador sube el carro durante el primer segundo de la obra
            if (onLift && liftT < 1f && car != null)
            {
                liftT = Mathf.MoveTowards(liftT, 1f, Time.deltaTime / 1.1f);
                var cp = car.position;
                cp.y = Mathf.Lerp(liftGroundY, liftGroundY + LiftHeight, Mathf.SmoothStep(0f, 1f, liftT));
                car.position = cp;
            }

            var p = pasos[idx];

            // cámara al ancla del paso
            if (cam != null)
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, p.CamPos, 6f * Time.deltaTime);
                var dir = p.CamMira - cam.transform.position;
                if (dir.sqrMagnitude > 0.001f)
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,
                        Quaternion.LookRotation(dir.normalized), 8f * Time.deltaTime);
            }

            // resplandor del objetivo actual
            if (highlightMat != null)
            {
                float k = (Mathf.Sin(Time.time * 6f) + 1f) * 0.3f;
                highlightMat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.2f) * k);
            }

            var mouse = Mouse.current;
            if (mouse != null && p.Target != null)
            {
                bool over = PointerOver(p.Target, mouse);
                bool pressed = mouse.leftButton.isPressed;
                float add = 0f;
                switch (p.Accion)
                {
                    case Accion.Clic:
                        if (over && mouse.leftButton.wasPressedThisFrame) add = p.Duracion;
                        break;
                    case Accion.Mantener:
                        if (over && pressed) add = Time.deltaTime;
                        break;
                    case Accion.Frotar:
                        if (over && pressed) add = mouse.delta.ReadValue().magnitude * 0.0022f;
                        break;
                }
                if (add > 0f)
                {
                    float antes = p.Progreso;
                    p.Progreso = Mathf.Min(p.Progreso + add, p.Duracion);
                    if (p.Girar) p.Target.transform.Rotate(0f, 0f, 640f * ((p.Progreso - antes) / p.Duracion), Space.Self);
                    if (p.Tinte != null)
                        p.Tinte.material.SetColor("_BaseColor", Color.Lerp(p.TinteDe, p.TinteA, p.Progreso / p.Duracion));
                    p.AlAvanzar?.Invoke(p.Progreso / p.Duracion);
                    if (p.Progreso >= p.Duracion) { CompleteStep(); return; }
                }
            }

            if (hint != null)
            {
                string modo = p.Accion == Accion.Frotar ? "mantén clic y FROTA"
                    : p.Accion == Accion.Mantener ? "mantén clic" : "haz clic";
                hint.text = "PASO " + (idx + 1) + "/" + pasos.Count + " — " + p.Texto + "  (" + modo + ")\n" +
                    Mathf.RoundToInt(mission.Progress01 * 100f) + "% del trabajo   ·   ESC para pausar la obra";
            }
        }

        bool PointerOver(GameObject target, Mouse mouse)
        {
            if (cam == null) return false;
            var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            var hits = Physics.RaycastAll(ray, 10f);
            foreach (var h in hits)
                if (h.collider.transform == target.transform || h.collider.transform.IsChildOf(target.transform))
                    return true;
            return false;
        }

        void EnterStep(int i)
        {
            idx = i;
            var p = pasos[i];
            highlightMat = null;
            if (p.Target != null)
            {
                var r = p.Target.GetComponent<Renderer>();
                if (r != null)
                {
                    highlightMat = r.material;
                    highlightMat.EnableKeyword("_EMISSION");
                }
            }
        }

        void CompleteStep()
        {
            var p = pasos[idx];
            if (highlightMat != null) { highlightMat.SetColor("_EmissionColor", Color.black); highlightMat = null; }
            p.AlTerminar?.Invoke();

            bool last = idx >= pasos.Count - 1;
            float amount = last ? Mathf.Max(chunk, mission.WorkRequired - mission.WorkDone + 0.5f) : chunk;
            bool done = MissionSystem.I != null && MissionSystem.I.DoWorkChunk(mission, amount);
            HUDController.SetWorkProgress(mission.Progress01, mission.Title, mission.EsIlegal ? mission.Noise : 0f);

            if (done || last) { Finish(true); return; }
            EnterStep(idx + 1);
        }

        // ---------- cierre ----------

        void Finish(bool completed)
        {
            if (finishing) return;
            finishing = true;
            HUDController.SetWorkProgress(-1f, null, 0f);

            if (completed && car != null)
            {
                // el carro de un encargo del patio se lo llevan al terminar
                if (car.name.StartsWith("CarroEncargo_") && TrafficManager.I != null)
                    TrafficManager.I.DriveOffBack(car.gameObject);
                else if (car.GetComponent<CarJob>() != null)
                    GameEvents.Notify("\"" + mission.Title + "\" terminado. Entrega el carro con E.");
            }
            if (station != null && mission != null && station.CurrentMission == mission &&
                mission.State == MissionState.Completada)
                station.CurrentMission = null;

            Cleanup();
        }

        void Cleanup()
        {
            SetWorkLight(false);
            // bajar el carro del elevador y quitar los brazos (el carro queda parado normal en la bahía)
            if (onLift && car != null) { var p = car.position; p.y = liftGroundY; car.position = p; }
            foreach (var a in liftArms) if (a != null) Destroy(a);
            liftArms.Clear();
            if (hintPanel != null) { Destroy(hintPanel.gameObject); hintPanel = null; }
            if (rigRoot != null) Destroy(rigRoot);
            if (Current == this)
            {
                UIRoot.PopModal();
                if (fpc != null) fpc.enabled = true;
                if (cam != null) { cam.transform.position = camPrevPos; cam.transform.rotation = camPrevRot; }
            }
            Current = null;
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        void BuildHintUI()
        {
            if (UIRoot.I == null) return;
            hintPanel = UIRoot.CreatePanel(UIRoot.I.Canvas.transform, "TrabajoHint", new Vector2(0.5f, 0f),
                new Vector2(0f, 26f), new Vector2(880f, 84f), new Color(0.05f, 0.06f, 0.08f, 0.85f));
            hint = UIRoot.CreateText(hintPanel, "", 20, UIRoot.TextColor, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(860f, 76f), TextAnchor.MiddleCenter);
        }

        /// <summary>Luz de trabajo mientras dura la obra: soplete naranja si es ilegal (alerta vecinos).</summary>
        void SetWorkLight(bool on)
        {
            if (on && workLight == null)
            {
                bool ilegal = mission != null && mission.EsIlegal;
                workLight = new GameObject("LuzTrabajo");
                var anchor = car != null ? car : (station != null ? station.transform : transform);
                workLight.transform.position = anchor.position + Vector3.up * 1.4f;
                var l = workLight.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = ilegal ? new Color(1f, 0.6f, 0.25f) : new Color(0.65f, 0.8f, 1f);
                l.range = ilegal ? 8f : 5f;
                l.intensity = ilegal ? 3.5f : 2.2f;
                var flick = workLight.AddComponent<LightFlicker>();
                flick.BaseIntensity = l.intensity;
            }
            else if (!on && workLight != null)
            {
                Destroy(workLight);
                workLight = null;
            }
        }

        // ---------- fábrica de pasos y props ----------

        Vector3 W(Vector3 local) => car != null ? car.TransformPoint(local) : local;
        Vector3 WD(Vector3 dir) => car != null ? car.TransformDirection(dir) : dir;

        Paso Step(Accion a, string texto, GameObject target, float dur, Vector3 camPos, Vector3 mira,
            bool girar = false, System.Action alTerminar = null, System.Action<float> alAvanzar = null)
        {
            if (target != null && target.GetComponent<Collider>() == null)
                target.AddComponent<BoxCollider>();
            var p = new Paso
            {
                Accion = a, Texto = texto, Target = target, Duracion = Mathf.Max(0.05f, dur * durScale),
                CamPos = camPos, CamMira = mira, Girar = girar,
                AlTerminar = alTerminar, AlAvanzar = alAvanzar
            };
            pasos.Add(p);
            return p;
        }

        GameObject Prop(PrimitiveType type, string name, Vector3 worldPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(rigRoot.transform, true);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.SetColor("_BaseColor", color);
            return go;
        }

        void ToPile(GameObject piece)
        {
            if (piece == null) return;
            piece.transform.SetParent(rigRoot.transform, true);
            piece.transform.position = pilePoint +
                new Vector3(Random.Range(-0.5f, 0.5f), 0.2f + Random.value * 0.35f, Random.Range(-0.5f, 0.5f));
            piece.transform.rotation = Quaternion.Euler(Random.Range(-25f, 25f), Random.Range(0f, 360f), Random.Range(-25f, 25f));
        }

        static readonly Color Gris = new Color(0.55f, 0.55f, 0.58f);
        static readonly Color Oscuro = new Color(0.16f, 0.16f, 0.18f);
        static readonly Color Oxido = new Color(0.42f, 0.26f, 0.14f);

        Vector3 WheelLocal(int i) => new Vector3(i % 2 == 0 ? -0.85f : 0.85f, 0.32f, i < 2 ? 1.2f : -1.2f);

        GameObject FindWheel(int i)
        {
            if (car == null) return null;
            var t = car.Find("Wheel" + i);
            if (t != null) return t.gameObject;
            // la llanta ya no está (obra pausada): crear una de trabajo en su puesto
            var w = Prop(PrimitiveType.Cylinder, "Wheel" + i, W(WheelLocal(i)), new Vector3(0.64f, 0.12f, 0.64f), Oscuro);
            w.transform.rotation = car.rotation * Quaternion.Euler(0f, 0f, 90f);
            w.transform.SetParent(car, true);
            return w;
        }

        // ---------- selección de rig por misión ----------

        void BuildSteps()
        {
            string t = mission.Title.ToLowerInvariant();
            if (mission.Type == MissionType.Piezas) { RigPiezas(); return; }
            if (car == null) return; // sin dónde trabajar

            if (t.Contains("aceite")) RigAceite();
            else if (t.Contains("llanta")) RigLlantas();
            else if (t.Contains("freno") || t.Contains("suspensión")) RigFrenos();
            else if (t.Contains("tanquear")) RigTanque();
            else if (t.Contains("placa") || t.Contains("vin")) RigPlacas();
            else if (t.Contains("desarme")) RigDesarme();
            else if (t.Contains("pintura")) RigPintura();
            else if (mission.Type == MissionType.Especial) RigEspecial();
            else if (t.Contains("transmisión")) RigMotor(true);
            else RigMotor(false); // diagnóstico y demás revisiones
        }

        // ---------- rigs ----------

        /// <summary>Una rueda completa: aflojar tuerca → sacar llanta → poner nueva → apretar.</summary>
        void RigRueda(int i, string etiqueta, bool soloSacar = false)
        {
            var rueda = FindWheel(i);
            float side = i % 2 == 0 ? -1f : 1f;
            Vector3 wPos = W(WheelLocal(i));
            Vector3 outward = WD(new Vector3(side, 0f, 0f));
            Vector3 camPos = wPos + outward * 2.3f + Vector3.up * 0.75f;

            var tuerca = Prop(PrimitiveType.Cylinder, "Tuerca", wPos + outward * 0.11f,
                new Vector3(0.17f, 0.045f, 0.17f), Gris);
            tuerca.transform.rotation = Quaternion.FromToRotation(Vector3.up, outward);

            Step(Accion.Mantener, "Afloja la tuerca de la " + etiqueta, tuerca, 1f, camPos, wPos,
                girar: true, alTerminar: () => ToPile(tuerca));
            Step(Accion.Mantener, "Saca la " + etiqueta + " vieja", rueda, 0.8f, camPos, wPos,
                alTerminar: () => ToPile(rueda));

            if (soloSacar) return;

            // la llanta nueva se apoya JUNTO al cubo, entre la cámara y la rueda: así queda en
            // cuadro y se puede clicar (antes iba al montón lateral, fuera del encuadre).
            var nueva = Prop(PrimitiveType.Cylinder, "LlantaNueva", wPos + outward * 0.95f + Vector3.up * 0.02f,
                new Vector3(0.64f, 0.12f, 0.64f), new Color(0.05f, 0.05f, 0.05f));
            nueva.transform.rotation = Quaternion.FromToRotation(Vector3.up, outward);
            Step(Accion.Clic, "Monta la " + etiqueta + " nueva", nueva, 1f, camPos, wPos,
                alTerminar: () =>
                {
                    nueva.transform.SetParent(car, true);
                    nueva.transform.position = wPos;
                    nueva.transform.rotation = car.rotation * Quaternion.Euler(0f, 0f, 90f);
                });
            var tuerca2 = Prop(PrimitiveType.Cylinder, "TuercaNueva", wPos + outward * 0.11f,
                new Vector3(0.17f, 0.045f, 0.17f), Gris);
            tuerca2.transform.rotation = Quaternion.FromToRotation(Vector3.up, outward);
            Step(Accion.Mantener, "Aprieta la tuerca", tuerca2, 0.8f, camPos, wPos, girar: true);
        }

        void RigLlantas()
        {
            for (int i = 0; i < 4; i++)
                RigRueda(i, "llanta " + (i + 1) + "/4");
        }

        void RigFrenos()
        {
            var rueda = FindWheel(0);
            Vector3 wPos = W(WheelLocal(0));
            Vector3 outward = WD(Vector3.left);
            Vector3 camPos = wPos + outward * 2.3f + Vector3.up * 0.75f;

            var tuerca = Prop(PrimitiveType.Cylinder, "Tuerca", wPos + outward * 0.11f,
                new Vector3(0.17f, 0.045f, 0.17f), Gris);
            tuerca.transform.rotation = Quaternion.FromToRotation(Vector3.up, outward);
            Step(Accion.Mantener, "Afloja la tuerca", tuerca, 1f, camPos, wPos, girar: true,
                alTerminar: () => ToPile(tuerca));
            Step(Accion.Mantener, "Saca la llanta", rueda, 0.8f, camPos, wPos, alTerminar: () => ToPile(rueda));

            var caliper = Prop(PrimitiveType.Cube, "Freno", wPos - outward * 0.05f, new Vector3(0.3f, 0.34f, 0.3f), Oxido);
            var pastilla = Step(Accion.Frotar, "Cambia las pastillas y ajusta la suspensión", caliper, 1.5f,
                wPos + outward * 1.6f + Vector3.up * 0.5f, wPos);
            pastilla.Tinte = caliper.GetComponent<Renderer>();
            pastilla.TinteDe = Oxido;
            pastilla.TinteA = new Color(0.75f, 0.2f, 0.15f);

            var nueva = Prop(PrimitiveType.Cylinder, "LlantaNueva", wPos + outward * 0.95f + Vector3.up * 0.02f,
                new Vector3(0.64f, 0.12f, 0.64f), new Color(0.05f, 0.05f, 0.05f));
            nueva.transform.rotation = Quaternion.FromToRotation(Vector3.up, outward);
            Step(Accion.Clic, "Monta la llanta de nuevo", nueva, 1f, camPos, wPos,
                alTerminar: () =>
                {
                    nueva.transform.SetParent(car, true);
                    nueva.transform.position = wPos;
                    nueva.transform.rotation = car.rotation * Quaternion.Euler(0f, 0f, 90f);
                });
            var tuerca2 = Prop(PrimitiveType.Cylinder, "TuercaNueva", wPos + outward * 0.11f,
                new Vector3(0.17f, 0.045f, 0.17f), Gris);
            tuerca2.transform.rotation = Quaternion.FromToRotation(Vector3.up, outward);
            Step(Accion.Mantener, "Aprieta la tuerca", tuerca2, 0.8f, camPos, wPos, girar: true);
        }

        void RigAceite()
        {
            // La carrocería (Body) tapa z ±1.9, y 0.18..0.73: cualquier pieza bajo el carro o bajo
            // el capó cerrado quedaba oculta por su collider y no se podía clicar. Por eso ahora se
            // abre el capó y se trabaja con el tapón del cárter EN EL BORDE DELANTERO (delante de la
            // carrocería, z>1.9) y la boca de aceite POR ENCIMA del motor descubierto (y>0.73).
            Vector3 capoPos = W(new Vector3(0f, 0.78f, 1.05f));
            Vector3 camCapo = W(new Vector3(1.4f, 1.9f, 2.9f));
            var capo = Prop(PrimitiveType.Cube, "Capo", capoPos, new Vector3(1.5f, 0.05f, 1.45f), CarBodyColor());
            capo.transform.rotation = car.rotation;
            Step(Accion.Clic, "Abre el capó", capo, 1f, camCapo, capoPos, alTerminar: () =>
            {
                capo.transform.rotation = car.rotation * Quaternion.Euler(-58f, 0f, 0f);
                capo.transform.position = W(new Vector3(0f, 1.22f, 0.55f));
            });

            // drenaje: tapón del cárter asomando en el borde delantero-inferior, expuesto y de frente
            Vector3 drenaje = W(new Vector3(0f, 0.24f, 1.98f));
            Vector3 camDren = W(new Vector3(0f, 0.72f, 3.7f));
            var tapon = Prop(PrimitiveType.Cylinder, "Tapon", drenaje, new Vector3(0.15f, 0.05f, 0.15f), Gris);
            tapon.transform.rotation = Quaternion.FromToRotation(Vector3.up, WD(Vector3.forward));
            var bandeja = Prop(PrimitiveType.Cylinder, "Bandeja", W(new Vector3(0f, 0.04f, 1.98f)),
                new Vector3(0.55f, 0.05f, 0.55f), Oscuro);
            var fluido = Prop(PrimitiveType.Cylinder, "AceiteViejo", bandeja.transform.position + Vector3.up * 0.03f,
                new Vector3(0.06f, 0.005f, 0.06f), new Color(0.1f, 0.08f, 0.04f));
            Object.Destroy(fluido.GetComponent<Collider>());

            Step(Accion.Mantener, "Afloja el tapón del cárter", tapon, 1.1f, camDren, drenaje,
                girar: true, alTerminar: () => ToPile(tapon));
            Step(Accion.Mantener, "Deja drenar el aceite viejo", bandeja, 1.6f, camDren, W(new Vector3(0f, 0.12f, 1.98f)),
                alAvanzar: f => fluido.transform.localScale = new Vector3(0.06f + f * 0.42f, 0.006f, 0.06f + f * 0.42f));
            var tapon2 = Prop(PrimitiveType.Cylinder, "TaponNuevo", drenaje, new Vector3(0.15f, 0.05f, 0.15f),
                new Color(0.7f, 0.7f, 0.72f));
            tapon2.transform.rotation = Quaternion.FromToRotation(Vector3.up, WD(Vector3.forward));
            Step(Accion.Clic, "Pon el tapón nuevo", tapon2, 1f, camDren, drenaje);

            // llenado: boca de aceite sobre el motor, ya descubierto por el capó abierto
            Vector3 boca = W(new Vector3(0.25f, 0.82f, 0.95f));
            var tapaAceite = Prop(PrimitiveType.Cylinder, "BocaAceite", boca, new Vector3(0.13f, 0.06f, 0.13f), Gris);
            var lata = Prop(PrimitiveType.Cylinder, "LataAceite", boca + Vector3.up * 0.3f + WD(Vector3.right) * 0.18f,
                new Vector3(0.14f, 0.16f, 0.14f), new Color(0.8f, 0.65f, 0.2f));
            lata.transform.rotation = Quaternion.Euler(0f, 0f, -40f);
            Step(Accion.Mantener, "Vierte el aceite nuevo", tapaAceite, 1.6f, camCapo, boca,
                alAvanzar: f => lata.transform.rotation = Quaternion.Euler(0f, 0f, -40f - f * 45f));

            Step(Accion.Clic, "Cierra el capó", capo, 1f, camCapo, capoPos, alTerminar: () =>
            {
                capo.transform.rotation = car.rotation;
                capo.transform.position = capoPos;
            });
        }

        void RigTanque()
        {
            // boca de la gasolina en el guardabarros trasero, ARRIBA y DETRÁS de la rueda
            // (antes caía sobre la llanta trasera y no se veía)
            Vector3 tapa = W(new Vector3(0.9f, 0.72f, -1.62f));
            Vector3 camPos = W(new Vector3(2.9f, 1.35f, -2.0f));
            var tapon = Prop(PrimitiveType.Cylinder, "TapaTanque", tapa, new Vector3(0.15f, 0.04f, 0.15f), Gris);
            tapon.transform.rotation = Quaternion.FromToRotation(Vector3.up, WD(Vector3.right));

            // medidor 3D sobre el carro
            var medFondo = Prop(PrimitiveType.Cube, "MedidorFondo", W(new Vector3(0f, 1.75f, -1.3f)),
                new Vector3(0.85f, 0.14f, 0.03f), Oscuro);
            medFondo.transform.rotation = car.rotation;
            Object.Destroy(medFondo.GetComponent<Collider>());
            var medFill = Prop(PrimitiveType.Cube, "MedidorFill", W(new Vector3(-0.4f, 1.75f, -1.32f)),
                new Vector3(0.01f, 0.1f, 0.02f), new Color(0.3f, 0.85f, 0.35f));
            medFill.transform.rotation = car.rotation;
            Object.Destroy(medFill.GetComponent<Collider>());

            Step(Accion.Clic, "Abre la tapa del tanque", tapon, 1f, camPos, tapa,
                alTerminar: () => ToPile(tapon));
            var boca = Prop(PrimitiveType.Sphere, "BocaTanque", tapa, Vector3.one * 0.14f, Oscuro);
            Step(Accion.Mantener, "Llena el tanque (mira el medidor)", boca, 2f, camPos, tapa,
                alAvanzar: f =>
                {
                    medFill.transform.localScale = new Vector3(0.02f + f * 0.78f, 0.1f, 0.02f);
                    medFill.transform.position = W(new Vector3(-0.4f + f * 0.39f, 1.75f, -1.32f));
                });
            var tapon2 = Prop(PrimitiveType.Cylinder, "TapaNueva", tapa, new Vector3(0.13f, 0.04f, 0.13f), Gris);
            tapon2.transform.rotation = Quaternion.FromToRotation(Vector3.up, WD(Vector3.right));
            Step(Accion.Clic, "Cierra la tapa", tapon2, 1f, camPos, tapa);
            var ruedaChk = FindWheel(0);
            Step(Accion.Mantener, "Revisa la presión de las llantas", ruedaChk, 0.8f,
                W(new Vector3(-2.2f, 0.8f, 1.6f)), W(WheelLocal(0)));
        }

        void RigMotor(bool transmision)
        {
            Vector3 capoPos = W(new Vector3(0f, 0.78f, 1.05f));
            Vector3 camPos = W(new Vector3(1.4f, 1.9f, 2.9f));
            var capo = Prop(PrimitiveType.Cube, "Capo", capoPos, new Vector3(1.5f, 0.05f, 1.45f),
                CarBodyColor());
            capo.transform.rotation = car.rotation;

            Step(Accion.Clic, "Abre el capó", capo, 1f, camPos, capoPos, alTerminar: () =>
            {
                capo.transform.rotation = car.rotation * Quaternion.Euler(-58f, 0f, 0f);
                capo.transform.position = W(new Vector3(0f, 1.22f, 0.55f));
            });

            string[] labels = transmision
                ? new[] { "Drena la caja de cambios", "Ajusta el embrague", "Prueba los cambios" }
                : new[] { "Revisa las bujías", "Ajusta la banda", "Cambia el filtro" };
            Vector3[] spots =
            {
                new Vector3(-0.35f, 0.72f, 1.25f), new Vector3(0.3f, 0.7f, 0.95f), new Vector3(0f, 0.68f, 1.45f)
            };
            Color[] cols = { new Color(0.75f, 0.6f, 0.2f), new Color(0.3f, 0.5f, 0.7f), new Color(0.6f, 0.3f, 0.3f) };
            for (int i = 0; i < 3; i++)
            {
                var pieza = Prop(PrimitiveType.Sphere, "PiezaMotor", W(spots[i]), Vector3.one * 0.22f, cols[i]);
                Step(Accion.Mantener, labels[i], pieza, 1.2f, W(new Vector3(spots[i].x * 1.5f, 1.8f, 2.6f)), W(spots[i]));
            }

            Step(Accion.Clic, "Cierra el capó", capo, 1f, camPos, capoPos, alTerminar: () =>
            {
                capo.transform.rotation = car.rotation;
                capo.transform.position = capoPos;
            });
        }

        void RigPlacas(bool conVin = true)
        {
            Vector3 placaPos = W(new Vector3(0f, 0.45f, -1.93f));
            Vector3 camAtras = W(new Vector3(0.4f, 0.85f, -3.5f));
            var placa = Prop(PrimitiveType.Cube, "PlacaVieja", placaPos, new Vector3(0.62f, 0.18f, 0.03f),
                new Color(0.85f, 0.8f, 0.55f));
            placa.transform.rotation = car.rotation;

            for (int s = 0; s < 2; s++)
            {
                float sx = s == 0 ? -0.24f : 0.24f;
                var tornillo = Prop(PrimitiveType.Cylinder, "Tornillo", W(new Vector3(sx, 0.45f, -1.96f)),
                    new Vector3(0.05f, 0.025f, 0.05f), Gris);
                tornillo.transform.rotation = Quaternion.FromToRotation(Vector3.up, WD(Vector3.back));
                Step(Accion.Mantener, "Quita el tornillo de la placa (" + (s + 1) + "/2)", tornillo, 0.8f,
                    camAtras, placaPos, girar: true, alTerminar: () => ToPile(tornillo));
            }
            Step(Accion.Mantener, "Quita la placa vieja", placa, 0.7f, camAtras, placaPos,
                alTerminar: () => ToPile(placa));

            var nueva = Prop(PrimitiveType.Cube, "PlacaNueva", pilePoint + Vector3.up * 0.5f,
                new Vector3(0.62f, 0.18f, 0.03f), new Color(0.95f, 0.9f, 0.6f));
            Step(Accion.Clic, "Pon la placa nueva", nueva, 1f, camAtras, placaPos, alTerminar: () =>
            {
                nueva.transform.SetParent(car, true);
                nueva.transform.position = placaPos;
                nueva.transform.rotation = car.rotation;
            });
            for (int s = 0; s < 2; s++)
            {
                float sx = s == 0 ? -0.24f : 0.24f;
                var tornillo = Prop(PrimitiveType.Cylinder, "TornilloNuevo", W(new Vector3(sx, 0.45f, -1.96f)),
                    new Vector3(0.05f, 0.025f, 0.05f), Gris);
                tornillo.transform.rotation = Quaternion.FromToRotation(Vector3.up, WD(Vector3.back));
                Step(Accion.Mantener, "Atornilla la placa (" + (s + 1) + "/2)", tornillo, 0.8f,
                    camAtras, placaPos, girar: true);
            }

            if (conVin)
            {
                // placa VIN en la base del parabrisas, inclinada hacia arriba para que la cámara
                // la vea de frente. Los "dígitos" grabados se ven claramente y se borran al limar.
                Vector3 vinPos = W(new Vector3(0f, 0.8f, 0.55f));
                var vin = Prop(PrimitiveType.Cube, "PlacaVIN", vinPos, new Vector3(0.5f, 0.03f, 0.16f),
                    new Color(0.72f, 0.72f, 0.75f));
                vin.transform.rotation = car.rotation * Quaternion.Euler(-32f, 0f, 0f);
                var vinR = vin.GetComponent<Renderer>();
                vinR.material.EnableKeyword("_EMISSION");
                vinR.material.SetColor("_EmissionColor", new Color(0.15f, 0.15f, 0.16f));

                // número grabado: dígitos oscuros sobre la placa (se desvanecen al frotar)
                var digitos = new List<Transform>();
                for (int d = 0; d < 7; d++)
                {
                    var dig = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    dig.name = "VinDigito";
                    Object.Destroy(dig.GetComponent<Collider>());
                    dig.transform.SetParent(vin.transform, false);
                    dig.transform.localPosition = new Vector3(-0.4f + d * 0.135f, 0.55f, 0f);
                    dig.transform.localScale = new Vector3(0.09f, 0.7f, 0.55f + (d % 2) * 0.25f);
                    var dr = dig.GetComponent<Renderer>();
                    dr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    dr.material.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.11f));
                    digitos.Add(dig.transform);
                }

                var paso = Step(Accion.Frotar, "Lima el número de serie hasta borrarlo", vin, 1.8f,
                    W(new Vector3(0.15f, 1.55f, 2.05f)), vinPos,
                    alAvanzar: f =>
                    {
                        foreach (var dg in digitos)
                            dg.localScale = new Vector3(0.09f, Mathf.Max(0.001f, 0.7f * (1f - f)), dg.localScale.z);
                    });
                paso.Tinte = vinR;
                paso.TinteDe = new Color(0.72f, 0.72f, 0.75f);
                paso.TinteA = new Color(0.5f, 0.5f, 0.52f);
            }
        }

        void RigDesarme()
        {
            for (int i = 0; i < 4; i++)
                RigRueda(i, "rueda " + (i + 1) + "/4", soloSacar: true);

            Vector3 capoPos = W(new Vector3(0f, 0.78f, 1.05f));
            var capo = Prop(PrimitiveType.Cube, "Capo", capoPos, new Vector3(1.5f, 0.05f, 1.45f), CarBodyColor());
            capo.transform.rotation = car.rotation;
            Step(Accion.Mantener, "Desmonta el capó", capo, 1f, W(new Vector3(1.4f, 1.9f, 2.9f)), capoPos,
                alTerminar: () => ToPile(capo));

            var motor = Prop(PrimitiveType.Cube, "Motor", W(new Vector3(0f, 0.55f, 1.1f)),
                new Vector3(0.85f, 0.5f, 0.9f), Oscuro);
            motor.transform.rotation = car.rotation;
            Step(Accion.Mantener, "Saca el motor con el polipasto", motor, 1.8f,
                W(new Vector3(1.4f, 1.9f, 2.9f)), motor.transform.position,
                alAvanzar: f => motor.transform.position = W(new Vector3(0f, 0.55f + f * 0.9f, 1.1f)),
                alTerminar: () => ToPile(motor));

            var puerta = Prop(PrimitiveType.Cube, "Puerta", W(new Vector3(-0.87f, 0.55f, -0.2f)),
                new Vector3(0.04f, 0.5f, 1.1f), CarBodyColor());
            puerta.transform.rotation = car.rotation;
            Step(Accion.Mantener, "Desmonta la puerta", puerta, 1f,
                W(new Vector3(-2.4f, 1.1f, -0.2f)), puerta.transform.position,
                alTerminar: () => ToPile(puerta));
        }

        void RigPintura()
        {
            var bodyR = car.Find("Body") != null ? car.Find("Body").GetComponent<Renderer>() : null;
            Color colorViejo = CarBodyColor();
            Color colorNuevo = Color.HSVToRGB(Random.value, Random.Range(0.5f, 0.8f), Random.Range(0.5f, 0.85f));

            // lijar manchas de óxido a ambos lados
            Vector3[] manchas =
            {
                new Vector3(-0.87f, 0.5f, 0.8f), new Vector3(-0.87f, 0.42f, -0.9f),
                new Vector3(0.87f, 0.55f, 0.2f), new Vector3(0.87f, 0.4f, -1.2f)
            };
            foreach (var mLocal in manchas)
            {
                float side = Mathf.Sign(mLocal.x);
                var mancha = Prop(PrimitiveType.Cube, "Oxido", W(mLocal), new Vector3(0.03f, 0.28f, 0.45f), Oxido);
                mancha.transform.rotation = car.rotation;
                var paso = Step(Accion.Frotar, "Lija la mancha de óxido", mancha, 1f,
                    W(new Vector3(side * 2.4f, 0.9f, mLocal.z)), W(mLocal),
                    alTerminar: () => mancha.SetActive(false));
                paso.Tinte = mancha.GetComponent<Renderer>();
                paso.TinteDe = Oxido;
                paso.TinteA = colorViejo;
            }

            // rociar por tercios con la pistola
            string[] zonas = { "el frente", "el medio", "la parte de atrás" };
            for (int i = 0; i < 3; i++)
            {
                float z = 1.27f - i * 1.27f;
                var overlay = Prop(PrimitiveType.Cube, "ZonaPintura", W(new Vector3(0f, 0.45f, z)),
                    new Vector3(1.74f, 0.58f, 1.3f), colorViejo);
                overlay.transform.rotation = car.rotation;
                var paso = Step(Accion.Mantener, "Rocía la pintura en " + zonas[i], overlay, 1.3f,
                    W(new Vector3(2.6f, 1.2f, z)), W(new Vector3(0f, 0.45f, z)));
                paso.Tinte = overlay.GetComponent<Renderer>();
                paso.TinteDe = colorViejo;
                paso.TinteA = colorNuevo;
                if (i == 2)
                    paso.AlTerminar = () =>
                    {
                        if (bodyR != null) bodyR.material.SetColor("_BaseColor", colorNuevo);
                    };
            }
        }

        void RigPiezas()
        {
            Vector3 mesa = station != null
                ? station.transform.position + Vector3.up * 0.55f
                : (car != null ? car.position + Vector3.up * 0.9f : transform.position);
            Vector3 camPos = mesa + new Vector3(0f, 1.3f, -1.7f);

            var caja = Prop(PrimitiveType.Cube, "CajaEmbalaje", mesa + new Vector3(0.9f, 0.15f, 0.1f),
                new Vector3(0.55f, 0.4f, 0.55f), new Color(0.5f, 0.38f, 0.22f));

            for (int i = 0; i < 3; i++)
            {
                var pieza = Prop(PrimitiveType.Cube, "Pieza", mesa + new Vector3(-0.7f + i * 0.45f, 0.12f, 0f),
                    new Vector3(0.3f, 0.2f, 0.25f), Gris);
                pieza.transform.rotation = Quaternion.Euler(0f, Random.Range(-25f, 25f), 0f);
                var limar = Step(Accion.Frotar, "Lima los números de serie (" + (i + 1) + "/3)", pieza, 1.2f,
                    camPos, pieza.transform.position);
                limar.Tinte = pieza.GetComponent<Renderer>();
                limar.TinteDe = Gris;
                limar.TinteA = new Color(0.8f, 0.8f, 0.85f);
                var pz = pieza;
                Step(Accion.Clic, "Empaca la pieza en la caja", pz, 1f, camPos, caja.transform.position,
                    alTerminar: () =>
                    {
                        pz.transform.position = caja.transform.position + Vector3.up * (0.25f + Random.value * 0.1f);
                        pz.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    });
            }
        }

        void RigEspecial()
        {
            // prueba de confianza: placas + VIN, desarme parcial y empaque
            RigPlacas(conVin: true);
            RigRueda(0, "rueda delantera", soloSacar: true);
            RigRueda(3, "rueda trasera", soloSacar: true);

            Vector3 mesa = pilePoint + new Vector3(0f, 0.4f, 1f);
            var caja = Prop(PrimitiveType.Cube, "CajaCaliente", mesa, new Vector3(0.55f, 0.4f, 0.55f),
                new Color(0.5f, 0.38f, 0.22f));
            var pieza = Prop(PrimitiveType.Cube, "PiezaCaliente", pilePoint + Vector3.up * 0.9f,
                new Vector3(0.3f, 0.2f, 0.25f), Gris);
            Step(Accion.Clic, "Empaca las piezas calientes", pieza, 1f,
                mesa + new Vector3(0f, 1.2f, -1.6f), mesa,
                alTerminar: () => pieza.transform.position = caja.transform.position + Vector3.up * 0.28f);
        }

        Color CarBodyColor()
        {
            var body = car != null ? car.Find("Body") : null;
            var r = body != null ? body.GetComponent<Renderer>() : null;
            return r != null && r.material.HasProperty("_BaseColor")
                ? r.material.GetColor("_BaseColor") : new Color(0.4f, 0.4f, 0.42f);
        }
    }
}
