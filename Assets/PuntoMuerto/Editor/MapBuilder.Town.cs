using Unity.AI.Navigation;
using UnityEngine;

namespace PuntoMuerto.EditorTools
{
    public static partial class MapBuilder
    {
        static readonly string[] HouseWallMats = { "Muro", "MuroVerde", "MuroAzul", "MuroRosa", "Ladrillo" };
        static readonly string[] RoofMats = { "LaminaRoja", "Lamina", "LaminaVerde" };

        // posiciones y rotación (hacia la calle) de las 10 casas — hogares de los NPCs
        public static readonly Vector3[] HousePositions =
        {
            new Vector3(-50f, 0f, 20f), new Vector3(-30f, 0f, 30f), new Vector3(-50f, 0f, 45f),
            new Vector3(-14f, 0f, 45f), new Vector3(-30f, 0f, -20f), new Vector3(-50f, 0f, -34f),
            new Vector3(-24f, 0f, -48f), new Vector3(13f, 0f, -32f), new Vector3(16f, 0f, -48f),
            new Vector3(52f, 0f, 14f)
        };
        static readonly float[] HouseRots = { 90f, 180f, 90f, 270f, 180f, 90f, 0f, 270f, 0f, 180f };

        static void BuildGround(Transform world)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo";
            ground.transform.SetParent(world, false);
            ground.transform.localScale = new Vector3(32f, 1f, 32f);
            ground.GetComponent<Renderer>().sharedMaterial = M("Pasto");
            ground.isStatic = true;

            // colinas de fondo (low-poly, como las refs)
            var hills = Empty("Colinas", world, Vector3.zero).transform;
            var rnd = new System.Random(7);
            for (int i = 0; i < 14; i++)
            {
                float ang = i / 14f * Mathf.PI * 2f;
                float dist = 150f + (float)rnd.NextDouble() * 60f;
                float w = 60f + (float)rnd.NextDouble() * 80f;
                float h = 18f + (float)rnd.NextDouble() * 22f;
                var hill = Sphere("Colina" + i, hills,
                    new Vector3(Mathf.Cos(ang) * dist, -6f, Mathf.Sin(ang) * dist),
                    new Vector3(w, h, w * 0.8f), i % 2 == 0 ? "Copa1" : "Copa2");
                Object.DestroyImmediate(hill.GetComponent<Collider>());
            }
        }

        static void BuildRoads(Transform world)
        {
            var roads = Empty("Vias", world, Vector3.zero).transform;

            // calle principal (X) y avenida (Z) cruzan TODO el mapa (antes se cortaban en el pasto);
            // alturas ligeramente distintas para evitar z-fighting en los cruces.
            // Área NavMesh 3 = "vía": costo alto para peatones (cruzan solo por las cebras).
            RoadArea(Box("CallePrincipal", roads, new Vector3(0f, 0.02f, 0f), new Vector3(320f, 0.04f, 8f), "Asfalto"));
            RoadArea(Box("Avenida", roads, new Vector3(0f, 0.024f, 0f), new Vector3(8f, 0.04f, 320f), "Asfalto"));
            // anillo
            RoadArea(Box("AnilloN", roads, new Vector3(0f, 0.028f, 60f), new Vector3(128f, 0.04f, 8f), "Asfalto"));
            RoadArea(Box("AnilloS", roads, new Vector3(0f, 0.028f, -60f), new Vector3(128f, 0.04f, 8f), "Asfalto"));
            RoadArea(Box("AnilloE", roads, new Vector3(60f, 0.032f, 0f), new Vector3(8f, 0.04f, 128f), "Asfalto"));
            RoadArea(Box("AnilloO", roads, new Vector3(-60f, 0.032f, 0f), new Vector3(8f, 0.04f, 128f), "Asfalto"));

            // cebras peatonales: únicos puntos "baratos" para cruzar la vía
            var cruces = Empty("Cebras", roads, Vector3.zero).transform;
            Crosswalk(cruces, new Vector3(9f, 0f, 0f), true);    // cruce central, lado este
            Crosswalk(cruces, new Vector3(-9f, 0f, 0f), true);   // cruce central, lado oeste
            Crosswalk(cruces, new Vector3(0f, 0f, 9f), false);   // cruce central, lado norte
            Crosswalk(cruces, new Vector3(0f, 0f, -9f), false);  // cruce central, lado sur
            Crosswalk(cruces, new Vector3(30f, 0f, 0f), true);   // frente al taller
            Crosswalk(cruces, new Vector3(-26f, 0f, 0f), true);  // frente al súper

            // líneas centrales discontinuas en TODAS las vías, saltando las intersecciones
            var lines = Empty("Lineas", roads, Vector3.zero).transform;
            for (int x = -156; x <= 156; x += 6)
                if (Mathf.Abs(x) > 6 && Mathf.Abs(Mathf.Abs(x) - 60) > 7)
                    Box("l", lines, new Vector3(x, 0.055f, 0f), new Vector3(2.4f, 0.02f, 0.25f), "LineaVia");
            for (int z = -156; z <= 156; z += 6)
                if (Mathf.Abs(z) > 6 && Mathf.Abs(Mathf.Abs(z) - 60) > 7)
                    Box("l", lines, new Vector3(0f, 0.055f, z), new Vector3(0.25f, 0.02f, 2.4f), "LineaVia");
            for (int x = -54; x <= 54; x += 6)
                if (Mathf.Abs(x) > 6)
                {
                    Box("l", lines, new Vector3(x, 0.055f, 60f), new Vector3(2.4f, 0.02f, 0.25f), "LineaVia");
                    Box("l", lines, new Vector3(x, 0.055f, -60f), new Vector3(2.4f, 0.02f, 0.25f), "LineaVia");
                }
            for (int z = -54; z <= 54; z += 6)
                if (Mathf.Abs(z) > 6)
                {
                    Box("l", lines, new Vector3(60f, 0.055f, z), new Vector3(0.25f, 0.02f, 2.4f), "LineaVia");
                    Box("l", lines, new Vector3(-60f, 0.055f, z), new Vector3(0.25f, 0.02f, 2.4f), "LineaVia");
                }

            // andenes junto a calle principal y avenida (terminan antes del anillo)
            var walks = Empty("Andenes", world, Vector3.zero).transform;
            Box("AndenN", walks, new Vector3(0f, 0.05f, 5.5f), new Vector3(112f, 0.1f, 3f), "Anden");
            Box("AndenS", walks, new Vector3(0f, 0.05f, -5.5f), new Vector3(112f, 0.1f, 3f), "Anden");
            Box("AndenE", walks, new Vector3(5.5f, 0.05f, 0f), new Vector3(3f, 0.1f, 112f), "Anden");
            Box("AndenO", walks, new Vector3(-5.5f, 0.05f, 0f), new Vector3(3f, 0.1f, 112f), "Anden");
        }

        static void BuildHouse(Transform parent, int index, Vector3 pos, float rotY)
        {
            var house = Empty("Casa_" + (index + 1), parent, pos);
            house.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            var t = house.transform;
            string wall = HouseWallMats[index % HouseWallMats.Length];
            string roof = RoofMats[index % RoofMats.Length];

            // cuerpo
            Box("Cuerpo", t, pos + Vector3.up * 1.5f, new Vector3(8f, 3f, 7f), wall, rotY);
            // techo a dos aguas
            BoxR("TechoA", t, pos + new Vector3(0f, 3.6f, 0f) , new Vector3(8.6f, 0.2f, 4.6f), roof,
                new Vector3(28f, rotY, 0f));
            BoxR("TechoB", t, pos + new Vector3(0f, 3.6f, 0f), new Vector3(8.6f, 0.2f, 4.6f), roof,
                new Vector3(-28f, rotY, 0f));
            // puerta y ventanas (frente = -Z local)
            var fwd = Quaternion.Euler(0f, rotY, 0f) * Vector3.back;
            var right = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;
            Box("Puerta", t, pos + fwd * 3.55f + Vector3.up * 1f, new Vector3(1.1f, 2f, 0.15f), "Madera", rotY);
            var v1 = Box("Ventana1", t, pos + fwd * 3.55f + right * 2.2f + Vector3.up * 1.7f,
                new Vector3(1.3f, 1f, 0.12f), "VentanaLuz", rotY);
            var v2 = Box("Ventana2", t, pos + fwd * 3.55f - right * 2.2f + Vector3.up * 1.7f,
                new Vector3(1.3f, 1f, 0.12f), "VentanaLuz", rotY);
            v1.isStatic = false; v1.AddComponent<PuntoMuerto.NightLight>();
            v2.isStatic = false; v2.AddComponent<PuntoMuerto.NightLight>();
        }

        static void BuildPlaza(Transform world)
        {
            var plaza = Empty("Plaza", world, Vector3.zero).transform;
            Box("Piso", plaza, new Vector3(32f, 0.06f, 32f), new Vector3(28f, 0.12f, 28f), "Anden");

            // kiosco central
            Cyl("KioscoBase", plaza, new Vector3(32f, 0.3f, 32f), new Vector3(5f, 0.3f, 5f), "Concreto");
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                Cyl("Columna", plaza, new Vector3(32f + Mathf.Cos(a) * 2f, 1.6f, 32f + Mathf.Sin(a) * 2f),
                    new Vector3(0.15f, 1.3f, 0.15f), "Madera");
            }
            Cyl("KioscoTecho", plaza, new Vector3(32f, 3.2f, 32f), new Vector3(5.6f, 0.5f, 5.6f), "LaminaRoja");

            // bancas
            for (int i = 0; i < 4; i++)
            {
                float a = i / 4f * Mathf.PI * 2f + 0.4f;
                var p = new Vector3(32f + Mathf.Cos(a) * 9f, 0.3f, 32f + Mathf.Sin(a) * 9f);
                Box("Banca", plaza, p, new Vector3(2f, 0.12f, 0.6f), "Madera", -a * Mathf.Rad2Deg);
                Box("BancaPata", plaza, p - Vector3.up * 0.15f, new Vector3(1.8f, 0.3f, 0.5f), "MetalOscuro", -a * Mathf.Rad2Deg);
            }

            BuildChurch(plaza);
        }

        static void BuildChurch(Transform parent)
        {
            var church = Empty("Iglesia", parent, Vector3.zero).transform;
            // separada del anillo norte para que los carros no rocen la nave
            var basePos = new Vector3(32f, 0f, 47.5f);
            Box("Nave", church, basePos + Vector3.up * 3f, new Vector3(10f, 6f, 14f), "Muro");
            BoxR("TechoA", church, basePos + new Vector3(0f, 6.9f, 0f), new Vector3(6.4f, 0.2f, 14.6f), "LaminaRoja", new Vector3(0f, 0f, 32f));
            BoxR("TechoB", church, basePos + new Vector3(0f, 6.9f, 0f), new Vector3(6.4f, 0.2f, 14.6f), "LaminaRoja", new Vector3(0f, 0f, -32f));
            // torre campanario (como ref2)
            Box("Torre", church, basePos + new Vector3(0f, 5f, -8.5f), new Vector3(3.4f, 10f, 3.4f), "Muro");
            Box("TorreTecho", church, basePos + new Vector3(0f, 10.6f, -8.5f), new Vector3(3.8f, 1.2f, 3.8f), "LaminaRoja");
            Box("Campana", church, basePos + new Vector3(0f, 9.2f, -8.5f), new Vector3(0.8f, 0.9f, 0.8f), "Farol").isStatic = false;
            Box("Puerta", church, basePos + new Vector3(0f, 1.4f, -10.25f), new Vector3(2.2f, 2.8f, 0.2f), "Madera");
            var cruz1 = Box("CruzV", church, basePos + new Vector3(0f, 12f, -8.5f), new Vector3(0.18f, 1.4f, 0.18f), "Madera");
            Box("CruzH", church, basePos + new Vector3(0f, 12.3f, -8.5f), new Vector3(0.8f, 0.16f, 0.16f), "Madera");
        }

        static void BuildSuper(Transform world)
        {
            var super = Empty("SuperLosAlisos", world, Vector3.zero).transform;
            var p = new Vector3(-28f, 0f, 14f);
            Box("Cuerpo", super, p + Vector3.up * 2.2f, new Vector3(16f, 4.4f, 10f), "Muro");
            Box("Marquesina", super, p + new Vector3(0f, 3.6f, -5.4f), new Vector3(16.6f, 0.25f, 2.2f), "LaminaVerde");
            // vitrina
            var vit = Box("Vitrina", super, p + new Vector3(0f, 1.5f, -5.05f), new Vector3(10f, 2.2f, 0.15f), "VentanaLuz");
            vit.isStatic = false; vit.AddComponent<PuntoMuerto.NightLight>();
            Box("Puerta", super, p + new Vector3(6f, 1.2f, -5.05f), new Vector3(1.6f, 2.4f, 0.15f), "MetalOscuro");
            // letrero
            var signHolder = Empty("LetreroSuper", super, Vector3.zero);
            Box("Letrero", signHolder.transform, p + new Vector3(0f, 4.9f, -5.1f), new Vector3(10f, 1.1f, 0.2f), "MetalOscuro", 0f, false);
            Text3D("SÚPER LOS ALISOS", signHolder.transform, p + new Vector3(0f, 4.9f, -5.25f), 0.09f,
                new Color(0.95f, 0.9f, 0.75f), 180f);
            signHolder.AddComponent<PuntoMuerto.NightLight>();
            var l = new GameObject("LuzLetrero").AddComponent<Light>();
            l.transform.SetParent(signHolder.transform, false);
            l.transform.position = p + new Vector3(0f, 4.6f, -6f);
            l.type = LightType.Point; l.range = 6f; l.intensity = 1.6f; l.color = new Color(1f, 0.95f, 0.8f);
        }

        static void BuildWaterTower(Transform world)
        {
            var wt = Empty("TorreDeAgua", world, Vector3.zero).transform;
            // fuera del anillo vial (antes las patas quedaban sobre la vía este)
            var p = new Vector3(48f, 0f, -52f);
            for (int i = 0; i < 4; i++)
            {
                float a = i / 4f * Mathf.PI * 2f + Mathf.PI / 4f;
                Cyl("Pata", wt, p + new Vector3(Mathf.Cos(a) * 2f, 5f, Mathf.Sin(a) * 2f),
                    new Vector3(0.25f, 5f, 0.25f), "Oxido");
            }
            Cyl("Tanque", wt, p + Vector3.up * 12f, new Vector3(5.5f, 2.2f, 5.5f), "Oxido");
            Cyl("TanqueTapa", wt, p + Vector3.up * 14.4f, new Vector3(4.5f, 0.6f, 4.5f), "MetalOscuro");
        }

        static void BuildTreesAndLamps(Transform world)
        {
            var props = Empty("Props", world, Vector3.zero).transform;
            var rnd = new System.Random(12);

            Vector3[] treeSpots =
            {
                new Vector3(24f, 0f, 24f), new Vector3(40f, 0f, 24f), new Vector3(24f, 0f, 40f), new Vector3(40f, 0f, 40f),
                new Vector3(-40f, 0f, 12f), new Vector3(-14f, 0f, 22f), new Vector3(-42f, 0f, -14f),
                new Vector3(14f, 0f, 18f), new Vector3(-12f, 0f, -22f), new Vector3(-56f, 0f, 54f),
                new Vector3(54f, 0f, 34f), new Vector3(-36f, 0f, 52f), new Vector3(20f, 0f, -14f),
                new Vector3(-16f, 0f, -56f), new Vector3(36f, 0f, 54f)
            };
            foreach (var s in treeSpots)
            {
                float h = 2.2f + (float)rnd.NextDouble() * 1.5f;
                Cyl("Tronco", props, s + Vector3.up * h * 0.5f, new Vector3(0.35f, h * 0.5f, 0.35f), "TroncoArbol");
                var c1 = Sphere("Copa", props, s + Vector3.up * (h + 1.1f), Vector3.one * (2.6f + (float)rnd.NextDouble()), rnd.Next(2) == 0 ? "Copa1" : "Copa2");
                Object.DestroyImmediate(c1.GetComponent<Collider>());
            }

            // postes de luz en calle principal y avenida
            for (int x = -70; x <= 70; x += 20)
            {
                BuildLamp(props, new Vector3(x, 0f, x % 40 == 0 ? 6.8f : -6.8f));
            }
            for (int z = -70; z <= 70; z += 20)
            {
                if (Mathf.Abs(z) < 8) continue;
                BuildLamp(props, new Vector3(z % 40 == 0 ? 6.8f : -6.8f, 0f, z));
            }
            BuildLamp(props, new Vector3(24f, 0f, 28f));
            BuildLamp(props, new Vector3(40f, 0f, 36f));

            // postes de energía con cables (como ref)
            var polesRoot = Empty("PostesEnergia", world, Vector3.zero).transform;
            Vector3 prevTop = Vector3.zero;
            int pi = 0;
            for (int x = -75; x <= 75; x += 25)
            {
                var basePos = new Vector3(x, 0f, 8.2f);
                Cyl("Poste", polesRoot, basePos + Vector3.up * 4.5f, new Vector3(0.22f, 4.5f, 0.22f), "Madera");
                Box("Cruceta", polesRoot, basePos + Vector3.up * 8.4f, new Vector3(2.4f, 0.15f, 0.15f), "Madera");
                var top = basePos + Vector3.up * 8.4f;
                if (pi > 0)
                {
                    var wire = new GameObject("Cable");
                    wire.transform.SetParent(polesRoot, false);
                    var lr = wire.AddComponent<LineRenderer>();
                    lr.positionCount = 3;
                    var mid = (prevTop + top) * 0.5f + Vector3.down * 0.7f;
                    lr.SetPositions(new[] { prevTop + Vector3.up * 0.1f, mid, top + Vector3.up * 0.1f });
                    lr.startWidth = 0.05f; lr.endWidth = 0.05f;
                    lr.material = M("MetalOscuro");
                    lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                prevTop = top;
                pi++;
            }
        }

        static void BuildLamp(Transform parent, Vector3 basePos)
        {
            var lamp = Empty("Farol", parent, basePos);
            lamp.isStatic = false;
            Cyl("Poste", lamp.transform, basePos + Vector3.up * 2.4f, new Vector3(0.16f, 2.4f, 0.16f), "MetalOscuro");
            float armDir = basePos.z > 0f || basePos.x > 0f ? -1f : 1f;
            bool alongX = Mathf.Abs(basePos.z) > Mathf.Abs(basePos.x) || Mathf.Abs(basePos.z) > 6f;
            Vector3 armOffset = alongX ? new Vector3(0f, 0f, armDir * 0.9f) : new Vector3(armDir * 0.9f, 0f, 0f);
            Box("Brazo", lamp.transform, basePos + Vector3.up * 4.7f + armOffset * 0.5f,
                alongX ? new Vector3(0.12f, 0.12f, 1.1f) : new Vector3(1.1f, 0.12f, 0.12f), "MetalOscuro");
            var head = Box("Cabeza", lamp.transform, basePos + Vector3.up * 4.6f + armOffset,
                new Vector3(0.5f, 0.18f, 0.5f), "Farol");
            head.isStatic = false;
            var l = new GameObject("Luz").AddComponent<Light>();
            l.transform.SetParent(head.transform, false);
            l.transform.position = basePos + Vector3.up * 4.4f + armOffset;
            l.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            l.type = LightType.Spot;
            l.spotAngle = 95f;
            l.range = 12f;
            l.intensity = 2.6f;
            l.color = new Color(1f, 0.85f, 0.6f);
            l.shadows = LightShadows.None;
            lamp.AddComponent<PuntoMuerto.NightLight>();
        }

        /// <summary>Marca una vía como área NavMesh 3 ("vía": costo alto para peatones en runtime).</summary>
        static void RoadArea(GameObject road)
        {
            var mod = road.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.area = 3;
        }

        /// <summary>Cebra peatonal: franjas blancas + NavMeshModifierVolume que devuelve el área
        /// caminable normal — el único punto barato para que un NPC cruce la vía.</summary>
        static void Crosswalk(Transform parent, Vector3 pos, bool cruzaCallePrincipal)
        {
            var cw = Empty("Cebra", parent, pos);
            for (int i = -3; i <= 3; i++)
            {
                var off = cruzaCallePrincipal ? new Vector3(0f, 0f, i * 1.1f) : new Vector3(i * 1.1f, 0f, 0f);
                var size = cruzaCallePrincipal ? new Vector3(2.4f, 0.02f, 0.55f) : new Vector3(0.55f, 0.02f, 2.4f);
                Box("Franja", cw.transform, pos + off + Vector3.up * 0.06f, size, "LineaVia");
            }
            var vol = cw.AddComponent<NavMeshModifierVolume>();
            vol.center = Vector3.up * 0.5f;
            vol.size = cruzaCallePrincipal ? new Vector3(3.2f, 2f, 13f) : new Vector3(13f, 2f, 3.2f);
            vol.area = 0;
        }

        static void BuildTown(Transform world)
        {
            BuildGround(world);
            BuildRoads(world);
            var houses = Empty("Casas", world, Vector3.zero).transform;
            for (int i = 0; i < HousePositions.Length; i++)
                BuildHouse(houses, i, HousePositions[i], HouseRots[i]);
            BuildPlaza(world);
            BuildSuper(world);
            BuildWaterTower(world);
            BuildTreesAndLamps(world);
        }
    }
}
