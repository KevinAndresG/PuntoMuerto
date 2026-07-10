# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Proyecto

**Punto Muerto** — simulador cooperativo de taller mecánico + crimen en el pueblo Los Alisos (Unity 6000.3.15f1, URP, PC/Steam). El diseño completo está en `GDD_v2.md`; los valores de balance (metas, umbrales, pagos) vienen de ahí — consultarlo antes de tocar números. Referencias visuales: `ref.png`, `ref2.png` (low-poly, atardecer, letrero neón).

Casi todo el contenido es **generado por código**: no hay prefabs de UI ni escenas armadas a mano. Los modelos son placeholders (primitivas) que se reemplazarán por assets reales.

## Flujo de trabajo

No hay build/test por CLI: el desarrollo es contra el editor de Unity vía las herramientas MCP (`mcp__unity__*` / `mcp__unity-mcp__*`, ambas apuntan al mismo editor).

- **Compilar/verificar**: escribir los `.cs` con herramientas de archivos, luego `AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport)` vía `Unity_RunCommand` y revisar `Unity_GetConsoleLogs` (errores).
- **Regenerar el mapa/escenas**: menú `PuntoMuerto → Construir TODO` o `PuntoMuerto.EditorTools.MapBuilder.BuildAll()` vía RunCommand. Regenera texturas+materiales, la escena `LosAlisos`, `MainMenu`, NavMesh y build settings **desde cero** — cualquier edición manual de escena se pierde; los cambios de mapa van en `Assets/PuntoMuerto/Editor/MapBuilder.*.cs`.
- **Probar**: entrar a Play mode y ejecutar self-tests con `Unity_RunCommand` (manipular `DayNightCycle.I.Hour`, invocar `GameEvents.OnHourTick`, llamar sistemas directamente). Nunca editar scripts durante Play: el domain reload anula los singletons estáticos.
- La captura de cámara MCP renderiza con iluminación propia — para validar día/noche leer `RenderSettings`/`Sun.intensity` por código.
- Si el editor está sin foco, Play mode casi no corre frames (`runInBackground` ya está activado en PlayerSettings, pero tenerlo presente al medir movimiento).

## Arquitectura

Todo el runtime vive en `Assets/PuntoMuerto/Scripts/` bajo el namespace `PuntoMuerto`, organizado en singletons MonoBehaviour (`X.I`) colgados del GameObject `Systems` de la escena, comunicados por el hub estático de eventos `GameEvents` (Core/GameEvents.cs).

- **Core/**: `GameManager` (día, dinero limpio/sucio, pausa), `DayNightCycle` (hora 7→24, fases, sol/ambiente; fin de día dispara resumen), `MetasManager` (las 4 metas paralelas del GDD §2 con sus umbrales), `BankSystem` (préstamos, cuotas semanales), `LedgerSystem` (lavado + riesgo de auditoría), `UpgradeSystem` (catálogo estático de mejoras; efectos de escena por nombre de GameObject), `InventorySystem` (repuestos/consumibles/piezas ilegales, capacidad), `EndingSystem` (6 finales), `SaveSystem` (JSON en persistentDataPath; `GameManager.LoadRequested` conecta el CONTINUAR del menú).
- **Missions/**: `Mission` es una clase plana **deliberadamente NO serializable** — si se marca `[Serializable]`, Unity serializa por valor los campos públicos en MonoBehaviours y crea instancias en blanco rompiendo la identidad por referencia (bug ya sufrido; no reintroducir). Flujo honesto: `MissionGenerator` agenda clientes → `ReceptionSystem` (cola, aceptar/rechazar) → `BayManager` (slots 3+2, spawnea el carro con `CarJob` para trabajarlo con E sostenida; al completar el carro sale solo). Flujo ilegal: oferta nocturna de Fabio por teléfono → `RepairStation` (Kind Patio/Pintura) en el patio o `PickupPoint` (recolección). Trabajar registra ruido en `FenceSpySystem`.
- **NPC/**: `NPCController` (NavMeshAgent; rutina deambular de día por waypoints `Waypoints/Andenes`, casa a las 19h, visitas al taller, espiar) + `NPCSuspicion` (FSM individual del GDD §4.4: Normal/Alerta/SospechaAlta/Investigando con despistar/sobornar/incriminar/reportar). `NPCManager` maneja desapariciones (letrero SE VENDE) y el piso global de sospecha. `TrafficManager`/`VehicleAI` mueven carros por waypoints (`Waypoints/ViaLoopA/B`), no por NavMesh.
- **UI/**: 100% construida por código con la fábrica `UIRoot` (fuente builtin `LegacyRuntime.ttf`, uGUI legacy Text). Cada pantalla es un componente en el GameObject `UI` que arma su panel al abrir. `UIRoot.ModalOpen` (contador estático push/pop) bloquea input de jugador/cámara — todo modal debe hacer PushModal/PopModal balanceado.
- **Net/**: multiplayer por IP con Netcode (`MPMode` estático elegido en el menú → `NetworkBootstrap` crea NetworkManager en runtime; prefabs `PlayerNet`/`GameSyncNet` en `Assets/PuntoMuerto/Resources/`). Host autoritativo; `GameSync` replica metas/dinero/hora a clientes cada 2s. Steam (invitaciones/lobbies) pendiente: cambiar UnityTransport por un transport de Steamworks.

### Reglas del generador de mapa (`Assets/PuntoMuerto/Editor/`)

- `MapBuilder` es una clase `static partial` repartida en Main/Town/Taller/Agents. `TextureGen` crea texturas procedurales PNG y materiales URP en `Assets/PuntoMuerto/Textures|Materials`.
- **Nunca parentar hijos visuales a primitivas escaladas** — la escala se multiplica (bug histórico del letrero gigante). Usar un Empty intermedio sin escala.
- **Todo MonoBehaviour que MapBuilder agregue a la escena (o que vaya en un prefab) debe estar en un archivo .cs con su mismo nombre** — si no, queda como "missing script" al recargar la escena (bug histórico masivo). Solo los componentes agregados exclusivamente en runtime (CarJob, PoliceLightBar, LightFlicker) pueden compartir archivo.
- Los NPCs deben spawnear **frente a la puerta de su casa (~6m fuera del footprint)**: el interior de un cubo sólido genera una isla de NavMesh encerrada y el agente queda atrapado.
- Sistemas de runtime localizan objetos de escena **por nombre/ruta** (`Taller/Slots`, `Taller/SpyPoint`, `Taller/Recepcion`, `Waypoints/Andenes`, `SlotLock4/5`, `CercaRendijas`...) — renombrar algo en MapBuilder exige actualizar el script que lo busca.

## Gotchas del entorno

- El proyecto vive en OneDrive: `Unity_PackageManager_ExecuteAction` puede fallar con EPERM al renombrar en `Library/PackageCache`. Alternativa que funciona: editar `Packages/manifest.json` + `Client.Resolve()` vía RunCommand.
- La conexión MCP de Unity a veces se revoca tras recompilar; el usuario debe re-aprobar en `Project Settings → AI → Unity MCP`.
- Idioma: código con nombres/comentarios en español (dominio) y convenciones C# estándar; la comunicación con el usuario es en español.
