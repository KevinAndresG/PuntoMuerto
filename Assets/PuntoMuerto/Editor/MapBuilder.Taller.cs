using UnityEngine;

namespace PuntoMuerto.EditorTools
{
    public static partial class MapBuilder
    {
        /// <summary>Taller: garaje amplio de 3 bahías con cortinas enrollables (+2 expansión),
        /// sala de espera interior, recepción, oficina, cuarto de ventanilla trasera y patio con cerca.</summary>
        static void BuildTaller(Transform world)
        {
            var taller = Empty("Taller", world, Vector3.zero).transform;

            // ---- pisos: garaje (x13..48) + ala este de recepción/oficina + patio ----
            // el garaje se hizo MÁS PROFUNDO: la pared del fondo pasó de z=-20.7 a z=-24 (más aire tras los carros).
            Box("PisoGaraje", taller, new Vector3(30.5f, 0.06f, -17f), new Vector3(35f, 0.12f, 14f), "Concreto");
            Box("Entrada", taller, new Vector3(34.5f, 0.04f, -5f), new Vector3(43f, 0.08f, 10f), "Concreto");
            Box("PisoPatio", taller, new Vector3(39f, 0.03f, -33f), new Vector3(34f, 0.06f, 22f), "Tierra");

            // ---- garaje (bahías, x13..48; frente z=-10, fondo z=-24) ----
            float wallH = 5f;
            // pared trasera (x13..48): PUERTA al cuarto ventanilla (x29..31) + portón peatonal al patio (x33..37)
            Box("ParedTraseraA1", taller, new Vector3(21f, wallH / 2f, -24f), new Vector3(16f, wallH, 0.4f), "MuroVerde");
            Box("PuertaCuartoDintel", taller, new Vector3(30f, 4.05f, -24f), new Vector3(2f, 1.9f, 0.4f), "MuroVerde");
            Box("MarcoPuertaCuarto1", taller, new Vector3(29f, 1.55f, -24f), new Vector3(0.18f, 3.1f, 0.5f), "Madera");
            Box("MarcoPuertaCuarto2", taller, new Vector3(31f, 1.55f, -24f), new Vector3(0.18f, 3.1f, 0.5f), "Madera");
            Text3D("PRIVADO", taller, new Vector3(30f, 3.35f, -23.75f), 0.04f, new Color(0.9f, 0.6f, 0.3f), 180f);
            // puerta batiente al cuarto de la ventanilla (se abre/cierra con E)
            BuildHingedDoor(taller, "PuertaVentanilla", new Vector3(29f, 0f, -24f), false, 1f, 2f, 3.1f, 92f, "Madera");
            Box("ParedTraseraA2", taller, new Vector3(32f, wallH / 2f, -24f), new Vector3(2f, wallH, 0.4f), "MuroVerde");
            Box("ParedTraseraDintel", taller, new Vector3(35f, 4.4f, -24f), new Vector3(4.2f, 1.2f, 0.4f), "MuroVerde");
            Box("ParedTraseraB", taller, new Vector3(42.5f, wallH / 2f, -24f), new Vector3(11f, wallH, 0.4f), "MuroVerde");
            // pared OESTE del garaje (sólida; el almacén va contra ella)
            Box("ParedIzq", taller, new Vector3(13f, wallH / 2f, -17f), new Vector3(0.4f, wallH, 14f), "MuroVerde");
            // pared ESTE del garaje = pared oeste del ala de recepción, corre z-10..-24 con PUERTA de servicio
            // batiente (vano z-13..-14.4). El tramo B llega hasta el fondo nuevo y separa el garaje de la oficina.
            Box("ParedEsteGarajeA", taller, new Vector3(48f, wallH / 2f, -11.5f), new Vector3(0.4f, wallH, 3f), "MuroVerde");
            Box("ParedEsteGarajeB", taller, new Vector3(48f, wallH / 2f, -19.2f), new Vector3(0.4f, wallH, 9.6f), "MuroVerde");
            Box("ParedEsteGarajeDintel", taller, new Vector3(48f, 4.1f, -13.7f), new Vector3(0.4f, 1.8f, 1.4f), "MuroVerde");
            Box("MarcoPuertaRec1", taller, new Vector3(48f, 1.6f, -13f), new Vector3(0.5f, 3.2f, 0.18f), "Madera");
            Box("MarcoPuertaRec2", taller, new Vector3(48f, 1.6f, -14.4f), new Vector3(0.5f, 3.2f, 0.18f), "Madera");
            Text3D("← RECEPCIÓN", taller, new Vector3(47.6f, 3.5f, -13.7f), 0.045f, new Color(0.95f, 0.9f, 0.7f), 90f);
            BuildHingedDoor(taller, "PuertaOficina", new Vector3(48f, 0f, -13f), true, -1f, 1.4f, 3.2f, 90f, "Madera");
            Box("Techo", taller, new Vector3(30.5f, wallH + 0.15f, -17f), new Vector3(35.4f, 0.3f, 14.6f), "Lamina");
            // frente del garaje: pilares (3 aberturas de bahía con cortina)
            Box("Pilar1", taller, new Vector3(13.5f, 2.2f, -10f), new Vector3(0.9f, 4.4f, 0.6f), "Ladrillo");
            Box("Pilar2", taller, new Vector3(24f, 2.2f, -10f), new Vector3(1f, 4.4f, 0.6f), "Ladrillo");
            Box("Pilar3", taller, new Vector3(36f, 2.2f, -10f), new Vector3(1f, 4.4f, 0.6f), "Ladrillo");
            Box("Pilar4", taller, new Vector3(47.5f, 2.2f, -10f), new Vector3(0.9f, 4.4f, 0.6f), "Ladrillo");
            Box("Viga", taller, new Vector3(30.5f, 4.7f, -10f), new Vector3(35f, 0.7f, 0.7f), "MuroVerde");

            // ---- cortinas enrollables (una por abertura; las mueve GarageDoor según el letrero) ----
            BuildGarageDoor(taller, 1, 14.1f, 23.4f);
            BuildGarageDoor(taller, 2, 24.6f, 35.4f);
            BuildGarageDoor(taller, 3, 36.6f, 46.9f);

            // ---- slots de carros (3 bahías base a 12m + 2 de expansión que llenan los huecos) ----
            var slots = Empty("Slots", taller, Vector3.zero).transform;
            MakeSlot(slots, "Slot1", new Vector3(22f, 0f, -16f));
            MakeSlot(slots, "Slot2", new Vector3(34f, 0f, -16f));
            MakeSlot(slots, "Slot3", new Vector3(44f, 0f, -16f));
            MakeSlot(slots, "Slot4", new Vector3(28f, 0f, -16f));
            MakeSlot(slots, "Slot5", new Vector3(40f, 0f, -16f));

            // slots del patio (negocio sucio; salida hacia la rendija sur)
            var patioSlots = Empty("PatioSlots", taller, Vector3.zero).transform;
            var ps1 = new GameObject("PSlot1");
            ps1.transform.SetParent(patioSlots, false);
            ps1.transform.position = new Vector3(34f, 0f, -36f);
            ps1.transform.rotation = Quaternion.LookRotation(Vector3.back);
            var ps2 = new GameObject("PSlot2");
            ps2.transform.SetParent(patioSlots, false);
            ps2.transform.position = new Vector3(41f, 0f, -36f);
            ps2.transform.rotation = Quaternion.LookRotation(Vector3.back);
            Box("MarcaPatio1", taller, new Vector3(34f, 0.07f, -36f), new Vector3(2.6f, 0.02f, 5f), "LineaVia");
            Box("MarcaPatio2", taller, new Vector3(41f, 0.07f, -36f), new Vector3(2.6f, 0.02f, 5f), "LineaVia");
            // bloqueos visuales de slots 4 y 5 (se quitan con mejoras)
            BuildSlotLock(taller, "SlotLock4", new Vector3(28f, 0f, -16f));
            BuildSlotLock(taller, "SlotLock5", new Vector3(40f, 0f, -16f));

            // marcas de piso: 3 bahías base (la 3ª se corrió a 44 para dejar aire contra la oficina)
            float[] baseBays = { 22f, 34f, 44f };
            foreach (var bx0 in baseBays)
                Box("MarcaBahia", taller, new Vector3(bx0, 0.13f, -16f), new Vector3(2.6f, 0.02f, 5f), "LineaVia");

            // elevador decorativo en las 3 bahías base (el kit hidráulico llega con la mejora)
            foreach (var bx in baseBays)
            {
                Box("ElevPoste1", taller, new Vector3(bx - 1.8f, 1.1f, -16f), new Vector3(0.3f, 2.2f, 0.3f), "Metal");
                Box("ElevPoste2", taller, new Vector3(bx + 1.8f, 1.1f, -16f), new Vector3(0.3f, 2.2f, 0.3f), "Metal");
            }

            // ---- botón de pared ABIERTO/CERRADO: DENTRO del ala de recepción, en su pared oeste ----
            // ShopSign crea en runtime la lámpara verde/roja y el cartel flotante sobre el frente.
            // La caja mira a +x (hacia adentro de la recepción, donde está el jugador atendiendo).
            var shopSign = Empty("BotonTaller", taller, new Vector3(48.3f, 1.4f, -12f));
            Box("BotonCaja", shopSign.transform, new Vector3(48.32f, 1.4f, -12f), new Vector3(0.14f, 0.62f, 0.45f), "MetalOscuro", 0f, false);
            Box("BotonTapa", shopSign.transform, new Vector3(48.41f, 1.4f, -12f), new Vector3(0.08f, 0.28f, 0.28f), "LaminaRoja", 0f, false);
            var signCol = shopSign.AddComponent<BoxCollider>();
            signCol.size = new Vector3(0.7f, 1.1f, 0.9f);
            shopSign.AddComponent<PuntoMuerto.ShopSign>();
            // ancla fija del cartel ABIERTO/CERRADO: sobre el frente del garaje, hacia la calle
            Empty("CartelEstadoAnchor", taller, new Vector3(30.5f, 8.2f, -9.8f));

            // ---- sala de espera (nicho contra la pared ESTE, corrida al fondo para no tapar la entrada) ----
            var sala = Empty("SalaEspera", taller, Vector3.zero).transform;
            Box("BancaEspera1", sala, new Vector3(54.8f, 0.42f, -13.5f), new Vector3(0.6f, 0.12f, 2.6f), "Madera");
            Box("BancaEsperaPata1", sala, new Vector3(54.8f, 0.18f, -13.5f), new Vector3(0.5f, 0.36f, 2.4f), "MetalOscuro");
            Box("BancaEspera2", sala, new Vector3(54.8f, 0.42f, -16.5f), new Vector3(0.6f, 0.12f, 2.6f), "Madera");
            Box("BancaEsperaPata2", sala, new Vector3(54.8f, 0.18f, -16.5f), new Vector3(0.5f, 0.36f, 2.4f), "MetalOscuro");
            Box("MesitaSala", sala, new Vector3(54.6f, 0.3f, -15f), new Vector3(0.8f, 0.6f, 0.8f), "Madera");
            Cyl("MateraSala", sala, new Vector3(49.8f, 0.5f, -7.2f), new Vector3(0.5f, 0.5f, 0.5f), "LaminaVerde");
            Text3D("SALA DE ESPERA", sala, new Vector3(53f, 3.2f, -14f), 0.04f, new Color(0.95f, 0.9f, 0.7f), 0f);
            var salaLight = new GameObject("LuzSala").AddComponent<Light>();
            salaLight.transform.SetParent(sala, false);
            salaLight.transform.position = new Vector3(52f, 2.9f, -12f);
            salaLight.type = LightType.Point;
            salaLight.range = 7f; salaLight.intensity = 1.2f;
            salaLight.color = new Color(1f, 0.9f, 0.7f);

            // ---- cuarto de la VENTANILLA trasera (techado, en el patio) ----
            BuildCuartoVentanilla(taller);

            // ---- recepción (ala este; mostrador cerca del nuevo frente, de cara a la entrada) ----
            var counter = Box("Mostrador", taller, new Vector3(51f, 0.55f, -10.5f), new Vector3(3f, 1.1f, 1f), "Madera");
            counter.isStatic = false;
            counter.AddComponent<PuntoMuerto.ReceptionDesk>();
            Box("MostradorTapa", taller, new Vector3(51f, 1.12f, -10.5f), new Vector3(3.2f, 0.06f, 1.2f), "MetalOscuro");
            Empty("Recepcion", taller, new Vector3(51f, 0f, -12f));
            Text3D("RECEPCIÓN", taller, new Vector3(51f, 1.9f, -9.95f), 0.05f, new Color(0.95f, 0.9f, 0.7f), 0f);
            // PC de compras rápidas sobre el mostrador
            var pcRec = Box("PCRecepcion", taller, new Vector3(52.3f, 1.42f, -10.6f), new Vector3(0.65f, 0.5f, 0.12f), "MetalOscuro");
            pcRec.isStatic = false;
            Box("PCRecepcionScreen", taller, new Vector3(52.3f, 1.42f, -10.67f), new Vector3(0.55f, 0.4f, 0.02f), "VentanaLuz", 0f, false);
            // PCRecepcion es solo decoración: la compra se centraliza en el PC de la oficina.

            // estantería del almacén NORMAL con contadores en vivo (pared oeste del garaje)
            BuildShelf(taller, "Estanteria", new Vector3(14.5f, 0f, -18.5f), PuntoMuerto.Zona.Normal);

            // ---- letrero neón ----
            var signRoot = Empty("LetreroNeon", taller, Vector3.zero);
            Box("LetreroFondo", signRoot.transform, new Vector3(35f, 6.3f, -9.9f), new Vector3(14f, 2f, 0.3f), "MetalOscuro", 0f, false);
            var neonMat = new Material(Shader.Find("PuntoMuerto/NeonPulse"));
            neonMat.SetColor("_Color", new Color(1f, 0.4f, 0.08f));
            var tubo = Box("MarcoNeon", signRoot.transform, new Vector3(35f, 6.3f, -10.08f), new Vector3(13.4f, 1.6f, 0.06f), "MetalOscuro", 0f, false);
            tubo.GetComponent<Renderer>().sharedMaterial = neonMat;
            // texto SOBRE la cara norte del panel (hacia la calle) y mirando +z, para que se lea de frente
            Text3D("PUNTO MUERTO", signRoot.transform, new Vector3(35f, 6.7f, -9.72f), 0.14f, new Color(1f, 0.55f, 0.15f), 180f);
            Text3D("TALLER — MECÁNICA EN GENERAL", signRoot.transform, new Vector3(35f, 5.85f, -9.72f), 0.07f, new Color(1f, 0.85f, 0.5f), 180f);
            var signLight = new GameObject("LuzLetrero").AddComponent<Light>();
            signLight.transform.SetParent(signRoot.transform, false);
            signLight.transform.position = new Vector3(35f, 6f, -11.5f);
            signLight.type = LightType.Point;
            signLight.range = 10f; signLight.intensity = 2.5f;
            signLight.color = new Color(1f, 0.5f, 0.15f);
            signRoot.AddComponent<PuntoMuerto.NightLight>();

            // ---- ala ESTE cerrada y profunda: RECEPCIÓN (frente) + OFICINA (fondo) (x48..56, z-6..-24) ----
            // el frente se corrió de z-10 a z-6 (HACIA LA CARRETERA) para agrandar la recepción: la sala de
            // espera ya no estorba la entrada y hay aire de sobra. El garaje sigue en z-10; el ala sobresale.
            Box("AlaEstePiso", taller, new Vector3(52f, 0.06f, -15f), new Vector3(8.4f, 0.12f, 18.4f), "Madera");
            Box("AlaEsteTecho", taller, new Vector3(52f, wallH + 0.15f, -15f), new Vector3(8.8f, 0.3f, 18.8f), "Lamina");
            // pared oeste del tramo que sobresale (z-6..-10; más al sur la comparte con el garaje)
            Box("RecParedO", taller, new Vector3(48f, wallH / 2f, -8f), new Vector3(0.4f, wallH, 4f), "Muro");
            // frente de la RECEPCIÓN (z-6): CORTINA enrollable como las del taller (x49.2..52.8, se cierra
            // de noche con el letrero) + PUERTA peatonal batiente al lado (x53.6..55.2) para entrar cuando
            // el taller está cerrado. La cortina deja el vano de NavMesh por donde entran los clientes.
            Box("RecFrenteA", taller, new Vector3(48.6f, wallH / 2f, -6f), new Vector3(1.2f, wallH, 0.4f), "Muro");
            Box("RecFrenteDintelCortina", taller, new Vector3(51f, 4.35f, -6f), new Vector3(3.6f, 1.3f, 0.4f), "Muro");
            Box("RecFrentePilar", taller, new Vector3(53.2f, wallH / 2f, -6f), new Vector3(0.8f, wallH, 0.4f), "Muro");
            Box("RecFrenteDintelPuerta", taller, new Vector3(54.4f, 4.35f, -6f), new Vector3(1.6f, 1.3f, 0.4f), "Muro");
            Box("RecFrenteE", taller, new Vector3(55.6f, wallH / 2f, -6f), new Vector3(0.8f, wallH, 0.4f), "Muro");
            BuildGarageDoor(taller, 4, 49.2f, 52.8f, -6f);   // cortina de recepción (misma que el taller)
            BuildHingedDoor(taller, "PuertaRecepcion", new Vector3(53.6f, 0f, -6f), false, 1f, 1.6f, 3.2f, 92f, "Madera");
            Box("AlaEsteParedE", taller, new Vector3(56f, wallH / 2f, -15f), new Vector3(0.4f, wallH, 18f), "Muro");
            // pared sur de la oficina con PUERTA trasera batiente al patio (x49..50.6)
            Box("AlaEsteParedS_A", taller, new Vector3(48.4f, wallH / 2f, -24f), new Vector3(1.2f, wallH, 0.4f), "Muro");
            Box("AlaEsteParedS_Dintel", taller, new Vector3(49.8f, 4.35f, -24f), new Vector3(1.6f, 1.3f, 0.4f), "Muro");
            Box("AlaEsteParedS_B", taller, new Vector3(53.4f, wallH / 2f, -24f), new Vector3(5.6f, wallH, 0.4f), "Muro");
            BuildHingedDoor(taller, "PuertaOficinaAtras", new Vector3(49f, 0f, -24f), false, 1f, 1.6f, 3.2f, -92f, "Madera");

            // escritorio de la OFICINA (fondo del ala, corrido al ESTE para no estorbar la puerta trasera
            // x49..50.6): libro + teléfono + PC
            var desk = Box("Escritorio", taller, new Vector3(53f, 0.5f, -22.6f), new Vector3(3.4f, 1f, 1.3f), "Madera");
            desk.isStatic = false;
            desk.AddComponent<PuntoMuerto.DeskLedger>();
            Box("Libro", taller, new Vector3(51.9f, 1.08f, -22.6f), new Vector3(0.55f, 0.1f, 0.75f), "Oxido", 15f, false);
            var phone = Box("Telefono", taller, new Vector3(54.3f, 1.18f, -22.6f), new Vector3(0.45f, 0.3f, 0.45f), "MetalOscuro");
            phone.isStatic = false;
            phone.AddComponent<PuntoMuerto.FabioPhone>();
            var pc = Box("PC", taller, new Vector3(53.1f, 1.32f, -22.85f), new Vector3(0.7f, 0.55f, 0.12f), "MetalOscuro");
            pc.isStatic = false;
            var pcScreen = Box("PCScreen", taller, new Vector3(53.1f, 1.32f, -22.79f), new Vector3(0.6f, 0.45f, 0.02f), "VentanaLuz", 0f, false);
            pc.AddComponent<PuntoMuerto.DeskComputer>();

            // pizarra de metas/mejoras (pared este de la oficina, mirando al oeste hacia adentro)
            var board = Box("Pizarra", taller, new Vector3(55.6f, 1.9f, -20.5f), new Vector3(0.15f, 1.7f, 3f), "MetalOscuro");
            board.isStatic = false;
            board.AddComponent<PuntoMuerto.UpgradeBoard>();
            Text3D("METAS", taller, new Vector3(55.45f, 2.5f, -20.5f), 0.06f, new Color(0.9f, 0.9f, 0.85f), 90f);

            // catre
            var cot = Box("Catre", taller, new Vector3(50f, 0.3f, -18.6f), new Vector3(2.2f, 0.35f, 1.1f), "Madera");
            cot.isStatic = false;
            cot.AddComponent<PuntoMuerto.SleepSpot>();

            var ofiLight = new GameObject("LuzOficina").AddComponent<Light>();
            ofiLight.transform.SetParent(taller, false);
            ofiLight.transform.position = new Vector3(52f, 2.8f, -19f);
            ofiLight.type = LightType.Point;
            ofiLight.range = 8f; ofiLight.intensity = 1.4f;
            ofiLight.color = new Color(1f, 0.9f, 0.7f);

            // ---- patio trasero ----
            BuildPatio(taller);

            // ---- cerca ----
            BuildFence(taller);

            // ---- kits visuales de mejoras (inactivos; UpgradeSystem los enciende al comprar) ----
            BuildMejoras(taller);
        }

        /// <summary>Estantería con contadores en vivo. Se usa igual para el almacén normal (garaje) y
        /// la bodega turbia (patio): mismas primitivas, solo cambia el origen y la zona. El frente
        /// (local -z, tras rotar -90) mira hacia +x, donde se para el jugador a leer el stock.</summary>
        static void BuildShelf(Transform taller, string name, Vector3 o, PuntoMuerto.Zona zona)
        {
            var shelf = Empty(name, taller, o);
            shelf.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            // marco + 3 repisas; las bandejas/piezas y los conteos los arma ShelfDisplay en runtime
            Box("EstanteMarco", shelf.transform, o + new Vector3(0.12f, 1.25f, 0f), new Vector3(0.36f, 2.5f, 3.4f), "Madera");
            Box("EstanteLadoA", shelf.transform, o + new Vector3(-0.05f, 1.25f, -1.68f), new Vector3(0.62f, 2.5f, 0.12f), "MetalOscuro");
            Box("EstanteLadoB", shelf.transform, o + new Vector3(-0.05f, 1.25f, 1.68f), new Vector3(0.62f, 2.5f, 0.12f), "MetalOscuro");
            Box("EstanteTope", shelf.transform, o + new Vector3(-0.05f, 2.5f, 0f), new Vector3(0.62f, 0.12f, 3.4f), "MetalOscuro");
            for (int r = 0; r < 3; r++)
                Box("Repisa", shelf.transform, o + new Vector3(0.05f, 0.58f + r * 0.62f, 0f), new Vector3(0.62f, 0.06f, 3.3f), "MetalOscuro");
            var shelfCol = shelf.AddComponent<BoxCollider>();
            shelfCol.center = new Vector3(0f, 1.25f, 0f);
            shelfCol.size = new Vector3(3.6f, 2.5f, 0.9f);
            shelf.AddComponent<PuntoMuerto.ShelfDisplay>().Zona = zona;
        }

        /// <summary>Cortina enrollable de una abertura del frente (rodillo + panel que anima GarageDoor).</summary>
        static void BuildGarageDoor(Transform taller, int idx, float x0, float x1, float z = -10f)
        {
            float cx = (x0 + x1) / 2f;
            float w = x1 - x0;
            var door = Empty("PuertaGaraje" + idx, taller, new Vector3(cx, 4.35f, z));
            var roller = Cyl("Rodillo", door.transform, new Vector3(cx, 4.45f, z),
                new Vector3(0.34f, w / 2f, 0.34f), "MetalOscuro", false);
            roller.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            // el panel arranca enrollado (arriba): así el NavMesh se hornea con las bahías abiertas
            Box("Cortina", door.transform, new Vector3(cx, 4.2f, z),
                new Vector3(w - 0.2f, 0.3f, 0.14f), "Lamina", 0f, false);
            var gd = door.AddComponent<PuntoMuerto.GarageDoor>();
            gd.Height = 4.2f;
        }

        /// <summary>Puerta batiente funcional en un vano (E la abre/cierra). El pivote va sobre el eje de
        /// la bisagra y la hoja (con collider sólido) cuelga de él. <paramref name="alongZ"/>: la pared corre
        /// en Z (hoja se extiende en Z); si no, corre en X. <paramref name="sign"/>: dirección de la hoja
        /// desde la bisagra. <paramref name="openAngle"/>: grados y lado de apertura.</summary>
        static void BuildHingedDoor(Transform parent, string name, Vector3 hingeBase, bool alongZ, float sign,
            float width, float height, float openAngle, string mat = "Madera")
        {
            var pivot = Empty(name, parent, hingeBase + Vector3.up * (height / 2f));
            Vector3 half = alongZ ? new Vector3(0f, 0f, sign * width / 2f) : new Vector3(sign * width / 2f, 0f, 0f);
            Vector3 leafCenter = pivot.transform.position + half;
            Vector3 scale = alongZ ? new Vector3(0.11f, height, width) : new Vector3(width, height, 0.11f);
            Box("Hoja", pivot.transform, leafCenter, scale, mat, 0f, false);
            // manija cerca del borde libre de la hoja
            Vector3 knob = pivot.transform.position +
                (alongZ ? new Vector3(0.1f, -0.05f, sign * (width - 0.22f)) : new Vector3(sign * (width - 0.22f), -0.05f, 0.1f));
            Box("Manija", pivot.transform, knob, Vector3.one * 0.1f, "MetalOscuro", 0f, false);
            pivot.AddComponent<PuntoMuerto.HingedDoor>().OpenAngle = openAngle;
        }

        /// <summary>Cuarto CERRADO de la ventanilla: se entra por la puerta desde el garaje (pared trasera)
        /// y se atiende el negocio sucio por la ventanilla de la pared sur, que da al patio.
        /// La gente de afuera no ve lo que pasa adentro: la fila sucia espera en el patio.</summary>
        static void BuildCuartoVentanilla(Transform taller)
        {
            // el cuarto se corrió -3.3 en z (de z-25.4 a z-28.7 la pared sur) para acompañar el fondo
            // más profundo del garaje. La fila sucia del patio (DirtyReceptionSystem) sigue este cambio.
            var cuarto = Empty("CuartoVentanilla", taller, Vector3.zero).transform;
            Box("CuartoPiso", cuarto, new Vector3(30f, 0.05f, -26.45f), new Vector3(5.65f, 0.08f, 4.5f), "Concreto");
            Box("CuartoParedO", cuarto, new Vector3(27.2f, 1.5f, -26.4f), new Vector3(0.35f, 3f, 4.8f), "Madera");
            Box("CuartoParedE", cuarto, new Vector3(32.8f, 1.5f, -26.4f), new Vector3(0.35f, 3f, 4.8f), "Madera");
            // pared sur cerrada con hueco de VENTANILLA (x29..31, alto 0.9..2.1) hacia el patio
            Box("CuartoParedS1", cuarto, new Vector3(28.1f, 1.5f, -28.7f), new Vector3(2.2f, 3f, 0.35f), "Madera");
            Box("CuartoParedS2", cuarto, new Vector3(31.9f, 1.5f, -28.7f), new Vector3(2.2f, 3f, 0.35f), "Madera");
            Box("CuartoAntepecho", cuarto, new Vector3(30f, 0.45f, -28.7f), new Vector3(2f, 0.9f, 0.35f), "Madera");
            Box("CuartoDintel", cuarto, new Vector3(30f, 2.55f, -28.7f), new Vector3(2f, 0.9f, 0.35f), "Madera");
            Box("CuartoTecho", cuarto, new Vector3(30f, 3.05f, -26.4f), new Vector3(6.7f, 0.2f, 5.2f), "Lamina");
            Text3D("VENTANILLA", cuarto, new Vector3(30f, 2.8f, -28.95f), 0.05f, new Color(0.9f, 0.6f, 0.3f), 0f);
            // mesita de atención del lado de adentro
            Box("CuartoMesa", cuarto, new Vector3(30f, 0.45f, -27.9f), new Vector3(2.4f, 0.9f, 0.8f), "Madera");

            // la ventanilla en sí: marco alrededor del hueco de la pared sur + repisa hacia el patio
            var backWin = Empty("Ventanilla", taller, new Vector3(30f, 1.5f, -28.7f));
            Box("VentanillaMarcoL", backWin.transform, new Vector3(28.95f, 1.5f, -28.7f), new Vector3(0.14f, 1.4f, 0.45f), "Madera", 0f, false);
            Box("VentanillaMarcoR", backWin.transform, new Vector3(31.05f, 1.5f, -28.7f), new Vector3(0.14f, 1.4f, 0.45f), "Madera", 0f, false);
            Box("VentanillaMarcoT", backWin.transform, new Vector3(30f, 2.14f, -28.7f), new Vector3(2.24f, 0.14f, 0.45f), "Madera", 0f, false);
            Box("VentanillaRepisa", backWin.transform, new Vector3(30f, 0.86f, -28.9f), new Vector3(2.3f, 0.1f, 0.9f), "MetalOscuro", 0f, false);
            var winCol = backWin.AddComponent<BoxCollider>();
            winCol.size = new Vector3(2.2f, 1.5f, 1.2f);
            backWin.AddComponent<PuntoMuerto.BackWindow>();
            var winLight = new GameObject("LuzVentanilla").AddComponent<Light>();
            winLight.transform.SetParent(backWin.transform, false);
            winLight.transform.position = new Vector3(30f, 2.6f, -26.4f);
            winLight.type = LightType.Point;
            winLight.range = 6f; winLight.intensity = 1.3f;
            winLight.color = new Color(1f, 0.6f, 0.35f);
            backWin.AddComponent<PuntoMuerto.NightLight>();

            // banca de espera del negocio sucio: contra la cerca ESTE (x~53), fuera del portón
            // peatonal garaje→patio (x33..37) que antes bloqueaba. Corre a lo largo de Z.
            Box("BancaSucia", taller, new Vector3(53.2f, 0.42f, -27f), new Vector3(2.8f, 0.12f, 0.6f), "Madera", 90f);
            Box("BancaSuciaPata", taller, new Vector3(53.2f, 0.18f, -27f), new Vector3(2.6f, 0.36f, 0.5f), "MetalOscuro", 90f);
            Box("CajaBancaSucia", taller, new Vector3(53f, 0.4f, -30.5f), new Vector3(0.8f, 0.8f, 0.8f), "Madera", 20f);

            // bodega del negocio TURBIO: estantería con el stock de piezas/dispositivos, en el patio,
            // exclusiva de esta zona. Solo display; la compra turbia va por el PC de la oficina.
            BuildShelf(taller, "BodegaTurbia", new Vector3(36f, 0f, -31f), PuntoMuerto.Zona.Turbio);
            Text3D("BODEGA", taller, new Vector3(36f, 2.75f, -31f), 0.05f, new Color(0.9f, 0.55f, 0.35f), -90f);
        }

        /// <summary>Kits visuales de cada mejora, inactivos hasta comprarla (UpgradeSystem.ReapplySceneEffects).</summary>
        static void BuildMejoras(Transform taller)
        {
            var mejoras = Empty("Mejoras", taller, Vector3.zero).transform;

            // elevador hidráulico (las 3 bahías; un solo kit que la mejora activa entero)
            var elev = Empty("ElevadorKit", mejoras, Vector3.zero);
            float[] elevBays = { 22f, 34f, 44f };   // = baseBays del garaje
            for (int i = 0; i < 3; i++)
            {
                float bx = elevBays[i];
                Box("ElevRiel1", elev.transform, new Vector3(bx - 0.6f, 0.3f, -16f), new Vector3(0.35f, 0.6f, 4.4f), "LaminaRoja");
                Box("ElevRiel2", elev.transform, new Vector3(bx + 0.6f, 0.3f, -16f), new Vector3(0.35f, 0.6f, 4.4f), "LaminaRoja");
                Box("ElevTravesano", elev.transform, new Vector3(bx, 2.3f, -16f), new Vector3(3.9f, 0.25f, 0.3f), "LaminaRoja");
                Box("ElevMotor", elev.transform, new Vector3(bx - 1.9f, 0.45f, -14f), new Vector3(0.7f, 0.9f, 0.7f), "MetalOscuro");
            }
            elev.SetActive(false);

            // gabinete de herramientas
            var tools = Empty("KitHerramientas", mejoras, Vector3.zero);
            Box("Gabinete", tools.transform, new Vector3(27.3f, 0.65f, -19.9f), new Vector3(1.4f, 1.3f, 0.5f), "LaminaRoja");
            Box("GabineteTapa", tools.transform, new Vector3(27.3f, 1.34f, -19.9f), new Vector3(1.5f, 0.08f, 0.55f), "MetalOscuro");
            tools.SetActive(false);

            // estantería extra (almacén 1, dentro del garaje, contra la nueva pared del fondo z-24)
            var alm1 = Empty("Almacen1", mejoras, Vector3.zero);
            Box("EstanteExtra", alm1.transform, new Vector3(20.5f, 1.25f, -23.5f), new Vector3(3f, 2.5f, 0.5f), "Madera");
            for (int r = 0; r < 3; r++)
                Box("RepisaExtra", alm1.transform, new Vector3(20.5f, 0.7f + r * 0.62f, -23.3f), new Vector3(2.8f, 0.06f, 0.4f), "MetalOscuro");
            alm1.SetActive(false);

            // bodega trasera (almacén 2, en el patio)
            var alm2 = Empty("Almacen2", mejoras, Vector3.zero);
            Box("Bodega", alm2.transform, new Vector3(50.5f, 1f, -41.5f), new Vector3(3f, 2f, 2.4f), "Madera");
            Box("BodegaTecho", alm2.transform, new Vector3(50.5f, 2.1f, -41.5f), new Vector3(3.3f, 0.15f, 2.7f), "Lamina");
            Box("BodegaCaja1", alm2.transform, new Vector3(48.6f, 0.4f, -41f), new Vector3(0.8f, 0.8f, 0.8f), "Madera", 12f);
            alm2.SetActive(false);

            // cabina de pintura (cierra la estación del patio)
            var cab = Empty("CabinaPintura", mejoras, Vector3.zero);
            Box("CabinaFondo", cab.transform, new Vector3(46f, 1.6f, -29.4f), new Vector3(3.6f, 3.2f, 0.15f), "Lamina");
            Box("CabinaLadoL", cab.transform, new Vector3(44.3f, 1.6f, -28.5f), new Vector3(0.15f, 3.2f, 2.2f), "Lamina");
            Box("CabinaLadoR", cab.transform, new Vector3(47.7f, 1.6f, -28.5f), new Vector3(0.15f, 3.2f, 2.2f), "Lamina");
            Box("CabinaTecho", cab.transform, new Vector3(46f, 3.25f, -28.6f), new Vector3(3.6f, 0.15f, 2.6f), "Lamina");
            cab.SetActive(false);

            // furgón de recolección (patio)
            var van = Empty("Furgon", mejoras, new Vector3(52f, 0f, -33f));
            Box("FurgonCaja", van.transform, new Vector3(52f, 1.15f, -32.4f), new Vector3(2f, 1.8f, 3.2f), "LaminaVerde");
            Box("FurgonCabina", van.transform, new Vector3(52f, 0.85f, -34.8f), new Vector3(1.9f, 1.2f, 1.5f), "MetalOscuro");
            for (int i = 0; i < 4; i++)
            {
                var w = Cyl("FurgonRueda", van.transform,
                    new Vector3(i % 2 == 0 ? 51.1f : 52.9f, 0.35f, i < 2 ? -32f : -34.4f),
                    new Vector3(0.7f, 0.14f, 0.7f), "MetalOscuro");
                w.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            van.SetActive(false);

            // caja fuerte (oficina)
            var safe = Empty("CajaFuerte", mejoras, Vector3.zero);
            Box("Caja", safe.transform, new Vector3(55f, 0.55f, -23f), new Vector3(0.9f, 1.1f, 0.8f), "MetalOscuro");
            Box("CajaPuerta", safe.transform, new Vector3(54.52f, 0.55f, -23f), new Vector3(0.06f, 0.9f, 0.6f), "Metal");
            safe.SetActive(false);

            // parches de la cerca (mejora cerca1: tapa la rendija oeste)
            var parches = Empty("CercaParches", mejoras, Vector3.zero);
            Box("Parche1", parches.transform, new Vector3(21.95f, 1.05f, -31.6f), new Vector3(0.08f, 1.9f, 0.5f), "Madera");
            Box("Parche2", parches.transform, new Vector3(21.95f, 1.05f, -32.4f), new Vector3(0.08f, 1.9f, 0.5f), "Madera");
            parches.SetActive(false);
        }

        static void MakeSlot(Transform parent, string name, Vector3 pos)
        {
            var slot = new GameObject(name);
            slot.transform.SetParent(parent, false);
            slot.transform.position = pos;
            slot.transform.rotation = Quaternion.LookRotation(Vector3.forward); // salida hacia la calle
        }

        static void BuildSlotLock(Transform taller, string name, Vector3 pos)
        {
            var block = Empty(name, taller, pos);
            Box("Caja1", block.transform, pos + new Vector3(-0.8f, 0.5f, 0f), Vector3.one, "Madera", 12f);
            Box("Caja2", block.transform, pos + new Vector3(0.7f, 0.5f, 0.5f), Vector3.one * 0.9f, "Madera", -8f);
            Box("Caja3", block.transform, pos + new Vector3(0f, 1.4f, 0.2f), Vector3.one * 0.8f, "Madera", 30f);
            var t = Text3D("EN OBRA", block.transform, pos + new Vector3(0f, 2.2f, 0f), 0.05f, new Color(0.95f, 0.8f, 0.3f), 180f);
        }

        static void BuildPatio(Transform taller)
        {
            Empty("PatioCentro", taller, new Vector3(40f, 0f, -32f));

            // bloqueo de NavMesh sobre el portón peatonal garaje→patio: los NPCs (agentes) NO cruzan
            // por ahí (antes se veían caminar a través del taller hasta el patio). El jugador usa
            // CharacterController y sí pasa; los clientes sucios ya nacen dentro del patio.
            var noNpc = Empty("NoNpcPorton", taller, new Vector3(35f, 1.4f, -24f));
            var vol = noNpc.AddComponent<Unity.AI.Navigation.NavMeshModifierVolume>();
            vol.size = new Vector3(6f, 3f, 3.5f);
            vol.center = Vector3.zero;
            vol.area = 1; // Not Walkable

            // estación de desarme (sin carro fijo: el auto solo aparece cuando hay un encargo de Fabio)
            var wreck = Empty("EstacionDesarme", taller, new Vector3(30f, 0f, -30f));
            var st1 = Box("MesaDesarme", wreck.transform, new Vector3(30f, 0.4f, -33f), new Vector3(2.4f, 0.8f, 1f), "Oxido");
            st1.isStatic = false;
            var rs1 = st1.AddComponent<PuntoMuerto.RepairStation>();
            rs1.Kind = PuntoMuerto.StationKind.Patio;

            // estación de pintura
            var paint = Empty("EstacionPintura", taller, new Vector3(46f, 0f, -28f));
            Box("MarcoPintura1", paint.transform, new Vector3(44.5f, 1.5f, -28f), new Vector3(0.2f, 3f, 0.2f), "Metal");
            Box("MarcoPintura2", paint.transform, new Vector3(47.5f, 1.5f, -28f), new Vector3(0.2f, 3f, 0.2f), "Metal");
            Box("MarcoPintura3", paint.transform, new Vector3(46f, 3f, -28f), new Vector3(3.2f, 0.2f, 0.2f), "Metal");
            var st2 = Box("CompresorPintura", paint.transform, new Vector3(46f, 0.45f, -30f), new Vector3(1.2f, 0.9f, 0.8f), "Metal");
            st2.isStatic = false;
            var rs2 = st2.AddComponent<PuntoMuerto.RepairStation>();
            rs2.Kind = PuntoMuerto.StationKind.Pintura;

            // lona
            var tarp = Box("Lona", taller, new Vector3(38f, 0.25f, -25f), new Vector3(2.2f, 0.5f, 1.6f), "MuroVerde");
            tarp.isStatic = false;
            tarp.AddComponent<PuntoMuerto.TarpInteractable>();

            // chatarra
            var junk = Empty("Chatarra", taller, Vector3.zero).transform;
            Cyl("Barril1", junk, new Vector3(54f, 0.55f, -38f), new Vector3(0.7f, 0.55f, 0.7f), "Oxido");
            Cyl("Barril2", junk, new Vector3(55f, 0.55f, -36.8f), new Vector3(0.7f, 0.55f, 0.7f), "MetalOscuro");
            Cyl("Llanta1", junk, new Vector3(25f, 0.2f, -39f), new Vector3(0.9f, 0.2f, 0.9f), "MetalOscuro");
            Cyl("Llanta2", junk, new Vector3(25f, 0.55f, -39f), new Vector3(0.9f, 0.2f, 0.9f), "MetalOscuro");
            Cyl("Llanta3", junk, new Vector3(26.2f, 0.2f, -38.4f), new Vector3(0.9f, 0.2f, 0.9f), "MetalOscuro");
            Box("Palet", junk, new Vector3(46f, 0.1f, -40f), new Vector3(1.6f, 0.2f, 1.2f), "Madera", 15f);
            Box("Motor", junk, new Vector3(46f, 0.5f, -40f), new Vector3(0.8f, 0.6f, 0.7f), "Metal", 15f);

            // luz del patio
            var nlHolder = Empty("LuzPatioHolder", taller, new Vector3(40f, 4f, -31f));
            var pl = new GameObject("LuzPatio").AddComponent<Light>();
            pl.transform.SetParent(nlHolder.transform, false);
            pl.type = LightType.Point;
            pl.range = 15f; pl.intensity = 1.3f;
            pl.color = new Color(1f, 0.8f, 0.55f);
            nlHolder.AddComponent<PuntoMuerto.NightLight>();
        }

        static void BuildFence(Transform taller)
        {
            var fence = Empty("Cerca", taller, Vector3.zero).transform;
            // patio: x 22..56, z -24..-44 (la cerca oeste arranca en la pared trasera del garaje, ya en z-24)
            FenceRun(fence, new Vector3(22f, 0f, -24f), new Vector3(22f, 0f, -44f), skip: 4);   // oeste (rendija de espiar)
            // sur: portón doble (~3.6m) por donde entran los carros del negocio sucio
            FenceRun(fence, new Vector3(22f, 0f, -44f), new Vector3(56f, 0f, -44f), skip: 9, skip2: 10);
            FenceRun(fence, new Vector3(56f, 0f, -44f), new Vector3(56f, 0f, -24f), skip: -1);  // este, empata con el ala de recepción
            var rendijas = Empty("CercaRendijas", taller, Vector3.zero).transform;
            Box("Marca1", rendijas, new Vector3(21.9f, 1f, -32f), new Vector3(0.1f, 1.6f, 0.9f), "MetalOscuro");
            Box("Marca2", rendijas, new Vector3(40f, 1f, -44.1f), new Vector3(0.9f, 1.6f, 0.1f), "MetalOscuro");

            Empty("SpyPoint", taller, new Vector3(20.5f, 0f, -32f));
        }

        /// <summary>Tramo de cerca de tablas; skip/skip2 = secciones sin tabla (rendijas), -1 = ninguna.</summary>
        static void FenceRun(Transform parent, Vector3 from, Vector3 to, int skip, int skip2 = -1)
        {
            Vector3 dir = (to - from);
            float len = dir.magnitude;
            dir.Normalize();
            int sections = Mathf.Max(1, Mathf.RoundToInt(len / 1.8f));
            float sectionLen = len / sections;
            float rotY = Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg;

            for (int i = 0; i <= sections; i++)
            {
                // sin poste en medio de un portón doble (dos rendijas seguidas)
                if (skip2 == skip + 1 && i == skip2) continue;
                var p = from + dir * (i * sectionLen);
                Box("PosteCerca", parent, p + Vector3.up * 1.1f, new Vector3(0.18f, 2.2f, 0.18f), "Madera");
            }
            for (int i = 0; i < sections; i++)
            {
                if (i == skip || i == skip2) continue;
                var p = from + dir * ((i + 0.5f) * sectionLen);
                Box("Tablas", parent, p + Vector3.up * 1.05f, new Vector3(sectionLen - 0.12f, 1.9f, 0.08f), "Madera", rotY);
            }
        }

        static void BuildParkedCar(Transform parent, Vector3 pos, Color color, float rotY)
        {
            var car = Empty("CarroEstacionado", parent, pos);
            car.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(car.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            body.transform.localScale = new Vector3(1.7f, 0.55f, 3.8f);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabina";
            cabin.transform.SetParent(car.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 1.25f, -0.3f);
            cabin.transform.localScale = new Vector3(1.5f, 0.5f, 1.8f);
            cabin.GetComponent<Renderer>().sharedMaterial = M("MetalOscuro");
            for (int i = 0; i < 4; i++)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                w.name = "Rueda";
                w.transform.SetParent(car.transform, false);
                w.transform.localPosition = new Vector3(i % 2 == 0 ? -0.85f : 0.85f, 0.32f, i < 2 ? 1.2f : -1.2f);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                w.transform.localScale = new Vector3(0.64f, 0.12f, 0.64f);
                w.GetComponent<Renderer>().sharedMaterial = M("MetalOscuro");
                Object.DestroyImmediate(w.GetComponent<Collider>());
            }
        }
    }
}
