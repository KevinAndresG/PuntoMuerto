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

            // ---- pisos (garaje ampliado hacia el oeste: ala de sala de espera) ----
            Box("PisoGaraje", taller, new Vector3(32.2f, 0.06f, -15.5f), new Vector3(31.6f, 0.12f, 11f), "Concreto");
            Box("Entrada", taller, new Vector3(32.2f, 0.04f, -5f), new Vector3(31.6f, 0.08f, 10f), "Concreto");
            Box("PisoPatio", taller, new Vector3(39f, 0.03f, -33f), new Vector3(34f, 0.06f, 22f), "Tierra");
            Box("PadSlot4", taller, new Vector3(11f, 0.05f, -12.5f), new Vector3(5f, 0.1f, 6f), "Concreto");
            Box("PadSlot5", taller, new Vector3(11f, 0.05f, -19f), new Vector3(5f, 0.1f, 6f), "Concreto");

            // ---- garaje ----
            float wallH = 5f;
            // pared trasera: PUERTA al cuarto de la ventanilla (x29..31) + portón peatonal al patio (x33..37)
            Box("ParedTraseraA1", taller, new Vector3(22.6f, wallH / 2f, -20.7f), new Vector3(12.8f, wallH, 0.4f), "MuroVerde");
            Box("PuertaCuartoDintel", taller, new Vector3(30f, 4.05f, -20.7f), new Vector3(2f, 1.9f, 0.4f), "MuroVerde");
            Box("MarcoPuertaCuarto1", taller, new Vector3(29f, 1.55f, -20.7f), new Vector3(0.18f, 3.1f, 0.5f), "Madera");
            Box("MarcoPuertaCuarto2", taller, new Vector3(31f, 1.55f, -20.7f), new Vector3(0.18f, 3.1f, 0.5f), "Madera");
            Text3D("PRIVADO", taller, new Vector3(30f, 3.35f, -20.45f), 0.04f, new Color(0.9f, 0.6f, 0.3f), 180f);
            Box("ParedTraseraA2", taller, new Vector3(32f, wallH / 2f, -20.7f), new Vector3(2f, wallH, 0.4f), "MuroVerde");
            Box("ParedTraseraDintel", taller, new Vector3(35f, 4.4f, -20.7f), new Vector3(4.2f, 1.2f, 0.4f), "MuroVerde");
            Box("ParedTraseraB", taller, new Vector3(42.5f, wallH / 2f, -20.7f), new Vector3(11f, wallH, 0.4f), "MuroVerde");
            Box("ParedIzq", taller, new Vector3(16.2f, wallH / 2f, -15.35f), new Vector3(0.4f, wallH, 10.7f), "MuroVerde");
            // frente del ala oeste (sala de espera) con ventana a la calle
            Box("ParedFrenteSala", taller, new Vector3(19.35f, wallH / 2f, -10f), new Vector3(6.3f, wallH, 0.4f), "MuroVerde");
            Box("VentanaSala", taller, new Vector3(19.35f, 2.3f, -10.25f), new Vector3(3.2f, 1.5f, 0.06f), "VentanaLuz", 0f, false);
            // pared derecha con PUERTA hacia la oficina (hueco z -13..-17, alineado con la pared oeste de la oficina)
            Box("ParedDerA", taller, new Vector3(47.8f, wallH / 2f, -11.5f), new Vector3(0.4f, wallH, 3f), "MuroVerde");
            Box("ParedDerB", taller, new Vector3(47.8f, wallH / 2f, -18.85f), new Vector3(0.4f, wallH, 3.7f), "MuroVerde");
            Box("ParedDerDintel", taller, new Vector3(47.8f, 4.1f, -15f), new Vector3(0.4f, 1.8f, 4.2f), "MuroVerde");
            Box("MarcoPuertaOfi1", taller, new Vector3(47.8f, 1.6f, -13.05f), new Vector3(0.5f, 3.2f, 0.18f), "Madera");
            Box("MarcoPuertaOfi2", taller, new Vector3(47.8f, 1.6f, -16.95f), new Vector3(0.5f, 3.2f, 0.18f), "Madera");
            Text3D("OFICINA", taller, new Vector3(47.4f, 3.5f, -15f), 0.05f, new Color(0.95f, 0.9f, 0.7f), -90f);
            Box("Techo", taller, new Vector3(32.2f, wallH + 0.15f, -15.4f), new Vector3(32f, 0.3f, 11.6f), "Lamina");
            // pilares del frente (3 aberturas con cortina)
            Box("Pilar1", taller, new Vector3(22.5f, 2.2f, -10f), new Vector3(0.9f, 4.4f, 0.6f), "Ladrillo");
            Box("Pilar2", taller, new Vector3(31f, 2.2f, -10f), new Vector3(1f, 4.4f, 0.6f), "Ladrillo");
            Box("Pilar3", taller, new Vector3(39.5f, 2.2f, -10f), new Vector3(1f, 4.4f, 0.6f), "Ladrillo");
            Box("Pilar4", taller, new Vector3(48f, 2.2f, -10f), new Vector3(0.9f, 4.4f, 0.6f), "Ladrillo");
            Box("Viga", taller, new Vector3(35f, 4.7f, -10f), new Vector3(26.5f, 0.7f, 0.7f), "MuroVerde");

            // ---- cortinas enrollables (una por abertura; las mueve GarageDoor según el letrero) ----
            BuildGarageDoor(taller, 1, 22.95f, 30.5f);
            BuildGarageDoor(taller, 2, 31.5f, 39f);
            BuildGarageDoor(taller, 3, 40f, 47.55f);

            // ---- slots de carros ----
            var slots = Empty("Slots", taller, Vector3.zero).transform;
            MakeSlot(slots, "Slot1", new Vector3(26.5f, 0f, -16f));
            MakeSlot(slots, "Slot2", new Vector3(35f, 0f, -16f));
            MakeSlot(slots, "Slot3", new Vector3(43.5f, 0f, -16f));
            MakeSlot(slots, "Slot4", new Vector3(11f, 0f, -12.5f));
            MakeSlot(slots, "Slot5", new Vector3(11f, 0f, -19f));

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
            BuildSlotLock(taller, "SlotLock4", new Vector3(11f, 0f, -12.5f));
            BuildSlotLock(taller, "SlotLock5", new Vector3(11f, 0f, -19f));

            // marcas de piso por bahía
            for (int i = 0; i < 3; i++)
                Box("MarcaBahia", taller, new Vector3(26.5f + i * 8.5f, 0.13f, -16f), new Vector3(2.6f, 0.02f, 5f), "LineaVia");

            // elevador decorativo en bahía 1 (el kit hidráulico llega con la mejora)
            Box("ElevPoste1", taller, new Vector3(24.7f, 1.1f, -16f), new Vector3(0.3f, 2.2f, 0.3f), "Metal");
            Box("ElevPoste2", taller, new Vector3(28.3f, 1.1f, -16f), new Vector3(0.3f, 2.2f, 0.3f), "Metal");

            // ---- botón de pared ABIERTO/CERRADO en el pilar de recepción ----
            // ShopSign crea en runtime la lámpara verde/roja y el cartel flotante sobre el pilar.
            // botón montado en la cara INTERIOR del Pilar4 (z<-10 = dentro del garaje), mirando al interior
            var shopSign = Empty("BotonTaller", taller, new Vector3(48f, 1.5f, -10.55f));
            Box("BotonCaja", shopSign.transform, new Vector3(48f, 1.5f, -10.48f), new Vector3(0.45f, 0.62f, 0.14f), "MetalOscuro", 0f, false);
            Box("BotonTapa", shopSign.transform, new Vector3(48f, 1.5f, -10.57f), new Vector3(0.28f, 0.28f, 0.08f), "LaminaRoja", 0f, false);
            var signCol = shopSign.AddComponent<BoxCollider>();
            signCol.size = new Vector3(0.8f, 1f, 0.6f);
            shopSign.AddComponent<PuntoMuerto.ShopSign>();
            // ancla fija del cartel ABIERTO/CERRADO: centrado sobre el frente del taller (x=35), alto y hacia la calle
            Empty("CartelEstadoAnchor", taller, new Vector3(35f, 8.2f, -9.8f));

            // ---- sala de espera INTERIOR (ala oeste del garaje) ----
            var sala = Empty("SalaEspera", taller, Vector3.zero).transform;
            Box("PisoSala", sala, new Vector3(19f, 0.135f, -15.5f), new Vector3(5.2f, 0.03f, 9.4f), "Madera");
            Box("BancaEspera1", sala, new Vector3(17.3f, 0.42f, -13.2f), new Vector3(0.6f, 0.12f, 3f), "Madera");
            Box("BancaEsperaPata1", sala, new Vector3(17.3f, 0.18f, -13.2f), new Vector3(0.5f, 0.36f, 2.8f), "MetalOscuro");
            Box("BancaEspera2", sala, new Vector3(17.3f, 0.42f, -17.8f), new Vector3(0.6f, 0.12f, 3f), "Madera");
            Box("BancaEsperaPata2", sala, new Vector3(17.3f, 0.18f, -17.8f), new Vector3(0.5f, 0.36f, 2.8f), "MetalOscuro");
            Box("MesitaSala", sala, new Vector3(19.6f, 0.3f, -15.5f), new Vector3(0.9f, 0.6f, 0.9f), "Madera");
            Cyl("MateraSala", sala, new Vector3(17.3f, 0.5f, -19.9f), new Vector3(0.6f, 0.5f, 0.6f), "LaminaVerde");
            Text3D("SALA DE ESPERA", sala, new Vector3(16.5f, 3.3f, -15.5f), 0.05f, new Color(0.95f, 0.9f, 0.7f), 90f);
            var salaLight = new GameObject("LuzSala").AddComponent<Light>();
            salaLight.transform.SetParent(sala, false);
            salaLight.transform.position = new Vector3(19f, 2.9f, -15.5f);
            salaLight.type = LightType.Point;
            salaLight.range = 7f; salaLight.intensity = 1.2f;
            salaLight.color = new Color(1f, 0.9f, 0.7f);

            // ---- cuarto de la VENTANILLA trasera (techado, en el patio) ----
            BuildCuartoVentanilla(taller);

            // ---- recepción (frente derecho del garaje) ----
            var counter = Box("Mostrador", taller, new Vector3(45.2f, 0.55f, -12f), new Vector3(2.8f, 1.1f, 1f), "Madera");
            counter.isStatic = false;
            counter.AddComponent<PuntoMuerto.ReceptionDesk>();
            Box("MostradorTapa", taller, new Vector3(45.2f, 1.12f, -12f), new Vector3(3f, 0.06f, 1.2f), "MetalOscuro");
            Empty("Recepcion", taller, new Vector3(44f, 0f, -8f));
            Text3D("RECEPCIÓN", taller, new Vector3(45.2f, 1.8f, -11.3f), 0.05f, new Color(0.95f, 0.9f, 0.7f), 180f);
            // PC de compras rápidas sobre el mostrador
            var pcRec = Box("PCRecepcion", taller, new Vector3(46.3f, 1.42f, -12f), new Vector3(0.65f, 0.5f, 0.12f), "MetalOscuro");
            pcRec.isStatic = false;
            Box("PCRecepcionScreen", taller, new Vector3(46.3f, 1.42f, -11.93f), new Vector3(0.55f, 0.4f, 0.02f), "VentanaLuz", 0f, false);
            pcRec.AddComponent<PuntoMuerto.DeskComputer>();

            // estantería de repuestos con contadores en vivo (pared izquierda del garaje)
            var shelf = Empty("Estanteria", taller, new Vector3(24.3f, 0f, -19f));
            shelf.transform.rotation = Quaternion.Euler(0f, -90f, 0f); // frente (local -z) hacia el interior del garaje (+x)
            Box("EstanteMarco", shelf.transform, new Vector3(24.3f, 1.25f, -19f), new Vector3(0.6f, 2.5f, 3.4f), "Madera");
            for (int r = 0; r < 3; r++)
                Box("Repisa", shelf.transform, new Vector3(24.55f, 0.7f + r * 0.62f, -19f), new Vector3(0.5f, 0.06f, 3.2f), "MetalOscuro");
            // cajitas de colores por tipo de repuesto
            string[] boxMats = { "Oxido", "MetalOscuro", "Metal", "LaminaRoja", "LaminaVerde", "Madera" };
            for (int b = 0; b < 6; b++)
            {
                float col = b % 2 == 0 ? -0.8f : 0.8f;
                float row = 0.85f + (b / 2) * 0.62f;
                Box("CajaRepuesto", shelf.transform, new Vector3(24.55f, row, -19f + col), new Vector3(0.45f, 0.35f, 0.9f), boxMats[b]);
            }
            var shelfCol = shelf.AddComponent<BoxCollider>();
            shelfCol.center = new Vector3(0f, 1.25f, 0f);
            shelfCol.size = new Vector3(3.6f, 2.5f, 0.9f);
            shelf.AddComponent<PuntoMuerto.ShelfDisplay>();

            // ---- letrero neón ----
            var signRoot = Empty("LetreroNeon", taller, Vector3.zero);
            Box("LetreroFondo", signRoot.transform, new Vector3(35f, 6.3f, -9.9f), new Vector3(14f, 2f, 0.3f), "MetalOscuro", 0f, false);
            var neonMat = new Material(Shader.Find("PuntoMuerto/NeonPulse"));
            neonMat.SetColor("_Color", new Color(1f, 0.4f, 0.08f));
            var tubo = Box("MarcoNeon", signRoot.transform, new Vector3(35f, 6.3f, -10.08f), new Vector3(13.4f, 1.6f, 0.06f), "MetalOscuro", 0f, false);
            tubo.GetComponent<Renderer>().sharedMaterial = neonMat;
            Text3D("PUNTO MUERTO", signRoot.transform, new Vector3(35f, 6.7f, -10.25f), 0.14f, new Color(1f, 0.55f, 0.15f), 180f);
            Text3D("TALLER — MECÁNICA EN GENERAL", signRoot.transform, new Vector3(35f, 5.85f, -10.25f), 0.07f, new Color(1f, 0.85f, 0.5f), 180f);
            var signLight = new GameObject("LuzLetrero").AddComponent<Light>();
            signLight.transform.SetParent(signRoot.transform, false);
            signLight.transform.position = new Vector3(35f, 6f, -11.5f);
            signLight.type = LightType.Point;
            signLight.range = 10f; signLight.intensity = 2.5f;
            signLight.color = new Color(1f, 0.5f, 0.15f);
            signRoot.AddComponent<PuntoMuerto.NightLight>();

            // ---- oficina (x 48..56, z -10..-20) ----
            Box("OfiParedN", taller, new Vector3(52f, 1.6f, -10.2f), new Vector3(8.4f, 3.2f, 0.35f), "Muro");
            Box("OfiParedS", taller, new Vector3(52f, 1.6f, -19.8f), new Vector3(8.4f, 3.2f, 0.35f), "Muro");
            Box("OfiParedE", taller, new Vector3(55.8f, 1.6f, -15f), new Vector3(0.35f, 3.2f, 10f), "Muro");
            // pared oeste con puerta amplia (hueco z -13..-17)
            Box("OfiParedO1", taller, new Vector3(48.2f, 1.6f, -11.5f), new Vector3(0.35f, 3.2f, 2.6f), "Muro");
            Box("OfiParedO2", taller, new Vector3(48.2f, 1.6f, -18.4f), new Vector3(0.35f, 3.2f, 2.8f), "Muro");
            Box("OfiTecho", taller, new Vector3(52f, 3.3f, -15f), new Vector3(8.8f, 0.25f, 10.4f), "Lamina");
            Box("OfiPiso", taller, new Vector3(52f, 0.08f, -15f), new Vector3(8f, 0.1f, 9.6f), "Madera");

            // escritorio: libro + teléfono + PC
            var desk = Box("Escritorio", taller, new Vector3(52.5f, 0.5f, -12.3f), new Vector3(3.4f, 1f, 1.3f), "Madera");
            desk.isStatic = false;
            desk.AddComponent<PuntoMuerto.DeskLedger>();
            Box("Libro", taller, new Vector3(51.4f, 1.08f, -12.3f), new Vector3(0.55f, 0.1f, 0.75f), "Oxido", 15f, false);
            var phone = Box("Telefono", taller, new Vector3(53.8f, 1.18f, -12.3f), new Vector3(0.45f, 0.3f, 0.45f), "MetalOscuro");
            phone.isStatic = false;
            phone.AddComponent<PuntoMuerto.FabioPhone>();
            var pc = Box("PC", taller, new Vector3(52.6f, 1.32f, -12.5f), new Vector3(0.7f, 0.55f, 0.12f), "MetalOscuro");
            pc.isStatic = false;
            var pcScreen = Box("PCScreen", taller, new Vector3(52.6f, 1.32f, -12.44f), new Vector3(0.6f, 0.45f, 0.02f), "VentanaLuz", 0f, false);
            pc.AddComponent<PuntoMuerto.DeskComputer>();

            // pizarra de metas/mejoras DENTRO de la oficina (pared este, mirando adentro)
            var board = Box("Pizarra", taller, new Vector3(55.5f, 1.9f, -15.5f), new Vector3(0.15f, 1.7f, 3f), "MetalOscuro");
            board.isStatic = false;
            board.AddComponent<PuntoMuerto.UpgradeBoard>();
            Text3D("METAS", taller, new Vector3(55.35f, 2.5f, -15.5f), 0.06f, new Color(0.9f, 0.9f, 0.85f), -90f);

            // catre
            var cot = Box("Catre", taller, new Vector3(52.5f, 0.3f, -18.6f), new Vector3(2.2f, 0.35f, 1.1f), "Madera");
            cot.isStatic = false;
            cot.AddComponent<PuntoMuerto.SleepSpot>();

            var ofiLight = new GameObject("LuzOficina").AddComponent<Light>();
            ofiLight.transform.SetParent(taller, false);
            ofiLight.transform.position = new Vector3(52f, 2.8f, -15f);
            ofiLight.type = LightType.Point;
            ofiLight.range = 7f; ofiLight.intensity = 1.4f;
            ofiLight.color = new Color(1f, 0.9f, 0.7f);

            // ---- patio trasero ----
            BuildPatio(taller);

            // ---- cerca ----
            BuildFence(taller);

            // ---- kits visuales de mejoras (inactivos; UpgradeSystem los enciende al comprar) ----
            BuildMejoras(taller);
        }

        /// <summary>Cortina enrollable de una abertura del frente (rodillo + panel que anima GarageDoor).</summary>
        static void BuildGarageDoor(Transform taller, int idx, float x0, float x1)
        {
            float cx = (x0 + x1) / 2f;
            float w = x1 - x0;
            var door = Empty("PuertaGaraje" + idx, taller, new Vector3(cx, 4.35f, -10f));
            var roller = Cyl("Rodillo", door.transform, new Vector3(cx, 4.45f, -10f),
                new Vector3(0.34f, w / 2f, 0.34f), "MetalOscuro", false);
            roller.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            // el panel arranca enrollado (arriba): así el NavMesh se hornea con las bahías abiertas
            Box("Cortina", door.transform, new Vector3(cx, 4.2f, -10f),
                new Vector3(w - 0.2f, 0.3f, 0.14f), "Lamina", 0f, false);
            var gd = door.AddComponent<PuntoMuerto.GarageDoor>();
            gd.Height = 4.2f;
        }

        /// <summary>Cuarto CERRADO de la ventanilla: se entra por la puerta desde el garaje (pared trasera)
        /// y se atiende el negocio sucio por la ventanilla de la pared sur, que da al patio.
        /// La gente de afuera no ve lo que pasa adentro: la fila sucia espera en el patio.</summary>
        static void BuildCuartoVentanilla(Transform taller)
        {
            var cuarto = Empty("CuartoVentanilla", taller, Vector3.zero).transform;
            Box("CuartoPiso", cuarto, new Vector3(30f, 0.05f, -23.15f), new Vector3(5.65f, 0.08f, 4.5f), "Concreto");
            Box("CuartoParedO", cuarto, new Vector3(27.2f, 1.5f, -23.1f), new Vector3(0.35f, 3f, 4.8f), "Madera");
            Box("CuartoParedE", cuarto, new Vector3(32.8f, 1.5f, -23.1f), new Vector3(0.35f, 3f, 4.8f), "Madera");
            // pared sur cerrada con hueco de VENTANILLA (x29..31, alto 0.9..2.1) hacia el patio
            Box("CuartoParedS1", cuarto, new Vector3(28.1f, 1.5f, -25.4f), new Vector3(2.2f, 3f, 0.35f), "Madera");
            Box("CuartoParedS2", cuarto, new Vector3(31.9f, 1.5f, -25.4f), new Vector3(2.2f, 3f, 0.35f), "Madera");
            Box("CuartoAntepecho", cuarto, new Vector3(30f, 0.45f, -25.4f), new Vector3(2f, 0.9f, 0.35f), "Madera");
            Box("CuartoDintel", cuarto, new Vector3(30f, 2.55f, -25.4f), new Vector3(2f, 0.9f, 0.35f), "Madera");
            Box("CuartoTecho", cuarto, new Vector3(30f, 3.05f, -23.1f), new Vector3(6.7f, 0.2f, 5.2f), "Lamina");
            Text3D("VENTANILLA", cuarto, new Vector3(30f, 2.8f, -25.65f), 0.05f, new Color(0.9f, 0.6f, 0.3f), 0f);
            // mesita de atención del lado de adentro
            Box("CuartoMesa", cuarto, new Vector3(30f, 0.45f, -24.6f), new Vector3(2.4f, 0.9f, 0.8f), "Madera");

            // la ventanilla en sí: marco alrededor del hueco de la pared sur + repisa hacia el patio
            var backWin = Empty("Ventanilla", taller, new Vector3(30f, 1.5f, -25.4f));
            Box("VentanillaMarcoL", backWin.transform, new Vector3(28.95f, 1.5f, -25.4f), new Vector3(0.14f, 1.4f, 0.45f), "Madera", 0f, false);
            Box("VentanillaMarcoR", backWin.transform, new Vector3(31.05f, 1.5f, -25.4f), new Vector3(0.14f, 1.4f, 0.45f), "Madera", 0f, false);
            Box("VentanillaMarcoT", backWin.transform, new Vector3(30f, 2.14f, -25.4f), new Vector3(2.24f, 0.14f, 0.45f), "Madera", 0f, false);
            Box("VentanillaRepisa", backWin.transform, new Vector3(30f, 0.86f, -25.6f), new Vector3(2.3f, 0.1f, 0.9f), "MetalOscuro", 0f, false);
            var winCol = backWin.AddComponent<BoxCollider>();
            winCol.size = new Vector3(2.2f, 1.5f, 1.2f);
            backWin.AddComponent<PuntoMuerto.BackWindow>();
            var winLight = new GameObject("LuzVentanilla").AddComponent<Light>();
            winLight.transform.SetParent(backWin.transform, false);
            winLight.transform.position = new Vector3(30f, 2.6f, -23.1f);
            winLight.type = LightType.Point;
            winLight.range = 6f; winLight.intensity = 1.3f;
            winLight.color = new Color(1f, 0.6f, 0.35f);
            backWin.AddComponent<PuntoMuerto.NightLight>();

            // banca de espera del negocio sucio: contra la cerca ESTE (x~53), fuera del portón
            // peatonal garaje→patio (x33..37) que antes bloqueaba. Corre a lo largo de Z.
            Box("BancaSucia", taller, new Vector3(53.2f, 0.42f, -27f), new Vector3(2.8f, 0.12f, 0.6f), "Madera", 90f);
            Box("BancaSuciaPata", taller, new Vector3(53.2f, 0.18f, -27f), new Vector3(2.6f, 0.36f, 0.5f), "MetalOscuro", 90f);
            Box("CajaBancaSucia", taller, new Vector3(53f, 0.4f, -30.5f), new Vector3(0.8f, 0.8f, 0.8f), "Madera", 20f);
        }

        /// <summary>Kits visuales de cada mejora, inactivos hasta comprarla (UpgradeSystem.ReapplySceneEffects).</summary>
        static void BuildMejoras(Transform taller)
        {
            var mejoras = Empty("Mejoras", taller, Vector3.zero).transform;

            // elevador hidráulico (bahía 1)
            var elev = Empty("ElevadorKit", mejoras, Vector3.zero);
            Box("ElevRiel1", elev.transform, new Vector3(25.9f, 0.3f, -16f), new Vector3(0.35f, 0.6f, 4.4f), "LaminaRoja");
            Box("ElevRiel2", elev.transform, new Vector3(27.1f, 0.3f, -16f), new Vector3(0.35f, 0.6f, 4.4f), "LaminaRoja");
            Box("ElevTravesano", elev.transform, new Vector3(26.5f, 2.3f, -16f), new Vector3(3.9f, 0.25f, 0.3f), "LaminaRoja");
            Box("ElevMotor", elev.transform, new Vector3(24.6f, 0.45f, -14f), new Vector3(0.7f, 0.9f, 0.7f), "MetalOscuro");
            elev.SetActive(false);

            // gabinete de herramientas
            var tools = Empty("KitHerramientas", mejoras, Vector3.zero);
            Box("Gabinete", tools.transform, new Vector3(27.3f, 0.65f, -19.9f), new Vector3(1.4f, 1.3f, 0.5f), "LaminaRoja");
            Box("GabineteTapa", tools.transform, new Vector3(27.3f, 1.34f, -19.9f), new Vector3(1.5f, 0.08f, 0.55f), "MetalOscuro");
            tools.SetActive(false);

            // estantería extra (almacén 1, dentro del garaje)
            var alm1 = Empty("Almacen1", mejoras, Vector3.zero);
            Box("EstanteExtra", alm1.transform, new Vector3(20.5f, 1.25f, -20.35f), new Vector3(3f, 2.5f, 0.5f), "Madera");
            for (int r = 0; r < 3; r++)
                Box("RepisaExtra", alm1.transform, new Vector3(20.5f, 0.7f + r * 0.62f, -20.15f), new Vector3(2.8f, 0.06f, 0.4f), "MetalOscuro");
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
            Box("Caja", safe.transform, new Vector3(55.2f, 0.55f, -17f), new Vector3(0.9f, 1.1f, 0.8f), "MetalOscuro");
            Box("CajaPuerta", safe.transform, new Vector3(54.72f, 0.55f, -17f), new Vector3(0.06f, 0.9f, 0.6f), "Metal");
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

            // estación de desarme
            var wreck = Empty("EstacionDesarme", taller, new Vector3(30f, 0f, -30f));
            BuildParkedCar(taller, new Vector3(30f, 0.3f, -30f), new Color(0.5f, 0.28f, 0.2f), 20f);
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
            // patio: x 22..56, z -21..-44 (la cerca este iba en x=58, justo sobre el carril del anillo)
            FenceRun(fence, new Vector3(22f, 0f, -21f), new Vector3(22f, 0f, -44f), skip: 5);   // oeste (rendija de espiar)
            // sur: portón doble (~3.6m) por donde entran los carros del negocio sucio
            FenceRun(fence, new Vector3(22f, 0f, -44f), new Vector3(56f, 0f, -44f), skip: 9, skip2: 10);
            FenceRun(fence, new Vector3(56f, 0f, -44f), new Vector3(56f, 0f, -20f), skip: -1);  // este, empata con la oficina
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
