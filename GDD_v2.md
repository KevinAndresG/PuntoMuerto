# [NOMBRE POR DEFINIR — ver sección 0.1]
## Game Design Document — v3.0

---

## 0. FICHA RÁPIDA

| | |
|---|---|
| **Género** | Simulador de negocio cooperativo + crimen/gestión de riesgo, en pueblo semi-abierto |
| **Jugadores** | 2-4 (diseñado core para 2, escalable) |
| **Plataforma** | PC (Steam) |
| **Motor** | Unity |
| **Perspectiva** | Tercera persona |
| **Duración de sesión** | 20-40 min por "día" |
| **Referencias tonales** | I Know a Guy, Papers Please, Stardew Valley (estructura de días/progresión abierta), Overcooked |
| **Pitch en una línea** | Heredaste un taller mecánico en quiebra en un pueblo donde todos se conocen. Un cartel te ofrece salvarlo — a cambio de que de noche lo uses para desaparecer autos robados, mover piezas ilegales, y lidiar con quien empiece a hacer demasiadas preguntas. Con el tiempo, podrías dejar de trabajar para ellos y empezar a ser tú quien da las órdenes. |

### Nombre

**"Punto Muerto"** — término mecánico real (neutral de la caja de cambios) que también significa "situación sin salida".
---

## 1. PREMISA Y TRAMA

### 1.1 Setup narrativo

El jugador (o los jugadores) heredan un taller mecánico de barrio en **Los Alisos**, un pueblo pequeño y tranquilo donde todos se conocen y las noticias corren en horas. El padre/tío que dejó el taller murió o se retiró dejando deudas con el banco.

**Fabio "El Contador" Reyes**, representante de una organización que opera en la región, se aparece con una propuesta: él cubre la deuda del banco a cambio de que el taller reciba, de manera discreta, vehículos que necesitan "desaparecer", piezas que necesitan moverse sin papeles, y ocasionalmente, "gente que necesita ayuda con un problema".

El jugador acepta (no hay elección real de rechazar al inicio). A partir de ahí, el juego es manejar **un negocio visible dentro de un pueblo donde todos observan**, mientras construyes una operación oculta que depende exactamente de que el pueblo siga confiando en ti.

### 1.2 Por qué el pueblo importa

En un pueblo pequeño **la reputación se propaga**. Un cliente que sospecha no solo deja de venir — le cuenta a su vecino, a su primo, al que juega dominó con el alcalde. El pueblo es el "medidor de Calor" hecho mundo: lo ves, lo escuchas, lo sientes en cómo te saludan (o no) al pasar.

### 1.3 Tono

Serio pero no gore — el peligro es económico, legal, social y moral, no de combate directo. La violencia, cuando ocurre, sucede fuera de cámara y se infiere, nunca se muestra ni se confirma explícitamente (ver sección 4.4). Tono de pueblo pequeño con secretos.

### 1.4 Por qué YA NO hay capítulos con contenido fijo

**Este es el cambio más importante de esta versión del documento, así que lo explico directamente:** la versión anterior dividía el juego en 5 "Capítulos" donde cada uno desbloqueaba mecánicas específicas en un orden fijo (Capítulo 1 = tutorial, Capítulo 2 = clientes recurrentes, etc.). Eso se siente estático y de sesiones separadas — exactamente lo contrario a lo que este diseño necesita.

En su lugar, el juego es un **sistema de días continuo** (como Stardew Valley o un juego de gestión abierto): no hay "niveles" ni checkpoints narrativos obligatorios. Juegas día tras día, y el contenido nuevo se va desbloqueando de dos formas que corren en paralelo (mezcladas, según tu preferencia):

- **Por hito/logro:** alcanzar cierto monto pagado al banco, cierto nivel de reputación con un grupo de NPCs, cierta cantidad de encargos completados de un tipo, etc.
- **Por tiempo jugado:** ciertos eventos narrativos (la primera visita de un inspector, la oferta de Fabio de subir de nivel) tienen una ventana de aparición ligada a cuántos días reales de juego han pasado, independientemente de si el jugador "se lo ganó" activamente — esto simula que el mundo sigue su curso aunque el jugador vaya lento en otras metas.

No existe un final de "Capítulo 1" que te obligue a esperar para desbloquear "Capítulo 2". Todo el contenido narrativo y de sistemas convive desde el principio como posibilidades — algunas simplemente requieren que hayas alcanzado cierto punto para activarse.

---

## 2. LAS METAS PARALELAS (el corazón de la progresión)

En vez de capítulos, el jugador gestiona **cuatro metas/medidores que avanzan simultáneamente**, a su propio ritmo, sin que una bloquee a las otras. Esto es lo que reemplaza la estructura de "Capítulo 1, 2, 3...". Los valores abajo son un punto de partida concreto para prototipar y balancear — números de referencia, no reglas grabadas en piedra; ajústalos jugando.

### 2.1 Meta A — Deuda con el Banco

**Rango:** $0 (libre) hasta el monto heredado inicial: **$100.000**. Es deliberadamente la meta de más largo aliento del juego — a un ritmo normal de juego, pagarla completa debería tomar bastante más tiempo que subir cómodamente la Meta D o la Meta B, para que se sienta como el verdadero "maratón" de fondo, no algo que resuelves en las primeras semanas de juego.

| Umbral pagado | Qué desbloquea |
|---|---|
| 25% pagado ($25.000) | Se habilita la opción de pedir el primer préstamo pequeño (tope: $8.000, cuota semanal fija a 10 semanas) |
| 50% pagado ($50.000) | Préstamos medianos disponibles (tope: $20.000, a 14 semanas), mejor tasa de interés que el préstamo inicial |
| 100% pagado ($100.000, deuda original saldada) | Insignia de "Libre de Banco": tasas de interés más bajas en todo préstamo futuro (-20% de interés respecto a la tasa base), condición necesaria para el final "Taller Limpio" |

**Riesgo de préstamos:** cada préstamo activo tiene una cuota semanal. Si te atrasas una cuota, el Calor pasivo (Meta B, cara negativa) sube +5 puntos y el banco manda una notificación visible (carta en el buzón del taller). Dos cuotas atrasadas seguidas: +15 adicionales y un "tasador" visita el taller (evento jugable de orden y presentación, distinto en tono al inspector policial pero mecánicamente similar: debes tener el taller en regla o sube más el Calor).

**Por qué el resto de la economía NO sube en la misma proporción (nota de balance):** si los pagos por trabajo y los costos de mejora subieran ×6-7 igual que la deuda, tardarías lo mismo en pagarla que con $15.000 — sería solo un cambio cosmético de ceros, no el efecto real de "meta larga" que buscas. Por eso aquí el resto de valores sube de forma moderada (aprox. ×2-3, no ×6-7): suficiente para que nada se sienta ridículamente barato al lado de una deuda de seis cifras, pero sin igualar el ritmo — así la deuda sí tarda notablemente más en pagarse que en subir el resto de tus metas.

### 2.2 Meta B — Reputación del Pueblo / Calor (medidor bidireccional, 0-100)

Técnicamente son **dos caras del mismo medidor** para simplificar el sistema: un solo valor de -100 (Calor máximo/terror del pueblo) a +100 (reputación máxima/pueblo confía plenamente), con 0 como punto neutral de inicio. Este medidor no está atado a dinero, así que no necesita recalibrarse por el cambio de escala de la deuda — se mantiene igual que antes:

**Movimientos de referencia:**
- Trabajo honesto bien hecho y bien cobrado: +1 a +3 por cliente, según satisfacción.
- Encargo sucio entregado a tiempo sin ser visto: 0 (neutral, es "invisible" por diseño).
- NPC espía por la cerca y no lo detienes a tiempo (4.3): -3 a -8 según qué tan comprometedor era lo que vio.
- Sospecha resuelta con "despistar" exitoso: 0 (se neutraliza sin costo, pero solo funciona si la sospecha del NPC individual es baja).
- Sospecha resuelta con soborno: -1 (leve, por el riesgo del rastro) pero neutraliza la amenaza inmediata.
- Reportar a Fabio (desaparición implícita): -10 de golpe, más un adicional de **-3 acumulativo permanente** cada vez que se repite (representa que el pueblo empieza a atar cabos con el tiempo, aunque nunca lo compruebe).
- Preguntas policiales/inspecciones mal manejadas: -5 a -15 según el resultado.

**Umbrales de evento (positivos y negativos):**

| Valor | Efecto |
|---|---|
| +30 | Clientes empiezan a traer pedidos de mayor valor y a recomendarte con otros (más frecuencia de clientes "premium") |
| +60 | Acceso a un proveedor de piezas legítimas de mejor calidad/precio; el pueblo te defiende activamente si Fabio o un tercero intentan intimidarte |
| -20 | El patrullero empieza a pasar notablemente más seguido frente al taller (tensión ambiental, sin evento aún) |
| -40 | Empiezan inspecciones aleatorias activas (evento jugable de esconder evidencia) |
| -70 | Riesgo real de allanamiento nocturno si hay efectivo/autos sin terminar a la vista |
| -90 | Umbral crítico: si se sostiene por más de 3 días sin bajar, se dispara automáticamente el final "Caída" (ver sección 7) — es la única meta que puede forzar un cierre no buscado por el jugador |

### 2.3 Meta C — Relación con Fabio (0-100)

Tampoco es un valor monetario, se mantiene igual:
- Sube +5 a +10 por encargo cumplido a tiempo y con buena calidad; +15 extra si aceptas un encargo de riesgo alto que Fabio ofrece como "prueba de confianza" (disponibles ocasionalmente, más seguido cuanto más alta ya esté la meta).
- Baja -10 a -20 por rechazar un encargo, -15 por entregar tarde, -25 por calidad tan mala que el auto/pieza es identificado después.
- **Umbral de 70+** sostenido junto con el "leverage" propio (ver 6.1) habilita la Vía del Poder.
- **Umbral por debajo de 20** con deuda activa con Fabio (encargos aceptados pero no completados) dispara eventos de amenaza/intimidación creciente.

### 2.4 Meta D — Mejora del Taller y Capacidad de Negocio

No es un número único sino un **árbol simple de mejoras compradas** (herramientas, bahías, patio trasero, cerca, caja fuerte, vehículo propio). Con el escalado moderado explicado en 2.1 (aprox. ×2-3 respecto a la versión de $15.000, no ×6-7):

| Tier | Ejemplos | Costo aproximado | Requisito |
|---|---|---|---|
| Básico | Herramientas mejoradas, reparar la cerca | $500-$2.000 | Ninguno |
| Medio | Segunda bahía, elevador hidráulico, cerca sólida | $4.000-$10.000 | A veces requiere préstamo mediano (Meta A al 50%) |
| Avanzado | Equipo de pintura profesional, vehículo propio de recolección, caja fuerte grande | $12.000-$25.000 | Casi siempre requiere préstamo, algunos requieren Meta B por encima de +30 (proveedor de confianza) |

**Ingresos de referencia (también moderadamente escalados, para que ganar dinero se sienta acorde a estos nuevos costos):**
- Cliente honesto promedio: $150-$600 por trabajo (antes ~$50-$200).
- Encargo sucio simple (placas): $800-$1.500.
- Encargo sucio complejo (re-numeración, desarme completo): $2.500-$6.000.
- Venta de piezas ilegales por lote: $500-$3.000 según calidad de ocultamiento.

Con estos números, pagar el 25% de la deuda ($25.000) sigue siendo alcanzable en un tramo razonable de juego (semanas, no meses de juego real), pero llegar al 100% ($100.000) requiere sostener ese ritmo mucho más tiempo — mientras que subir de tier en el taller o alcanzar reputación alta con el pueblo puede lograrse notablemente antes, dándote la sensación de "mi vida mejora" sin que eso signifique que ya casi terminaste de pagarle al banco.

**Por qué esto se siente progresivo y no estático:** en cualquier día de juego, el jugador puede estar simultáneamente a una fracción de pagar la deuda, con reputación media-alta, construyendo relación con Fabio, y ahorrando para una mejora de tier medio — las cuatro barras avanzan a ritmos distintos según cómo elijas jugar cada día, y no hay un punto en que "ya hiciste todo lo que este tramo del juego ofrece" porque no hay tramos.

---

## 3. CORE LOOP DIARIO (sin cambios de fondo, se mantiene)

### BLOQUE A — Mañana (10-15 min)
- Revisar agenda del día (clientes agendados, piezas pendientes).
- Atender clientes honestos según su horario — los ves llegar por la calle antes de que entren.
- Tensión pasiva: llamadas de Fabio o de compradores de piezas que puedes atender ahora o dejar para la noche.

### BLOQUE B — Tarde / Gestión (5-10 min)
- Pedir inventario, revisar el Libro Real vs Oficial, pagar cuentas (renta, préstamos del banco si los tienes, cuota a Fabio si aplica).

### BLOQUE C — Noche / Encargo sucio (15-20 min)
- El taller cierra al público. Trabajo en el patio trasero: autos, piezas, o recolección (ver sección 4).
- Riesgo activo de espionaje por la cerca (sección 4.3).
- Cierre: resumen del día — dinero ganado (limpio/sucio), cambio en las 4 metas paralelas.

---

## 4. SISTEMAS DE GAMEPLAY DETALLADOS

### 4.1 Negocio honesto — Clientes y reparaciones

- Tipos de trabajo: cambio de aceite/llantas, diagnóstico de motor, frenos/suspensión, pintura/carrocería (minijuego de lijado como mecánica táctil central).
- 8-12 NPCs nombrados y recurrentes con rutinas visibles en el pueblo.
- **Pedidos que aún no puedes cumplir (evento raro y memorable, como pediste):** de forma poco frecuente, un cliente —a veces uno legítimo, a veces alguien con un encargo de perfil ilegal— pide algo que tu taller todavía no puede hacer (una reparación que requiere una herramienta que no tienes, un tipo de pieza rara que no puedes procesar aún). Cuando esto pasa, el juego te lo marca como una oportunidad especial: puedes **"enfocarte"** en desbloquear esa capacidad (ahorrar/priorizar la mejora específica que lo permite) a cambio de que ese cliente esté dispuesto a esperar y pagar mucho mejor que un encargo normal cuando vuelvas a tenerlo disponible. Al ser raro, cada vez que ocurre se siente como un evento memorable y una meta clara de corto plazo, no como una mecánica constante que reemplaza el resto del sistema de metas.

### 4.2 Encargos de Fabio — familias de misión

Se mantienen las 4 familias ya definidas, ahora sin atarse a "capítulos":

- **A. Misiones de Auto** (placas, VIN, repintado, desarme completo).
- **B. Misiones de Piezas Ilegales** (clasificar, alterar número de serie, empacar, vender/negociar con compradores).
- **C. Misiones de Recolección/Logística** (recoger o entregar algo en un punto específico del pueblo o la carretera).
- **D. Encargos Especiales** (combinaciones con un giro — autos ya dañados, piezas "calientes").

Cuáles están disponibles en un momento dado depende de tu Meta C (relación con Fabio: más confianza, encargos más variados y de mayor valor) y de tu Meta D (mejor equipo, puedes procesar piezas o autos más complejos) — no de "en qué capítulo estás".

### 4.3 El sistema de la cerca — espionaje físico en el patio trasero

- El taller tiene zona frontal (legítima, visible desde la calle) y patio trasero (donde ocurre el trabajo ilegal), separados por una cerca imperfecta con rendijas.
- Mientras trabajas en tareas ilegales, hay probabilidad periódica de que un NPC transeúnte se detenga a espiar — la probabilidad depende del ruido/luz que genera la tarea (soplete y amoladora suben el riesgo; trabajo silencioso de noche lo baja).
- Aviso de UI (ícono de ojo/contorno) con ventana de reacción real: puedes taparte, cubrir lo que se ve, o ir a confrontar/distraer al NPC antes de que la sospecha se registre.
- Mejoras de la cerca (reparar tablas, cerca sólida, mamparas internas) reducen pero nunca eliminan del todo el riesgo.

### 4.4 NPCs que sospechan — diagrama de estados completo

Cada NPC del pueblo tiene, en segundo plano, una variable individual de **Sospecha** (0-100, distinta de la Meta B global — esta es personal por NPC) que determina en qué estado está respecto al jugador. El diagrama de estados es el siguiente:

```
[NORMAL] ──(gana sospecha)──> [ALERTA] ──(gana más)──> [SOSPECHA ALTA] ──(sin manejo)──> [INVESTIGANDO]
   ↑                              │                          │                                │
   │                          (jugador                  (jugador                        (jugador debe
   │                           la maneja                 la maneja                        actuar o
   │                           con éxito)                con éxito)                       escala más)
   │                              │                          │                                │
   └──────────────────────────────┴──────────────────────────┘                                │
                                                                                                 ▼
                                                                                    [RESOLUCIÓN DEL JUGADOR]
                                                                                    (despistar / sobornar /
                                                                                     incriminar / reportar
                                                                                     a Fabio / no hacer nada)
                                                                                                 │
                                                    ┌────────────────────────────────────────────┼─────────────────────┐
                                                    ▼                                            ▼                     ▼
                                          [NEUTRALIZADO]                              [CONSECUENCIA: NPC          [ESCALA A
                                          (vuelve a NORMAL,                            DESAPARECE]                AUTORIDAD]
                                           con memoria de que                          (solo si "reportar          (NPC va a la
                                           pasó algo raro —                            a Fabio", ver 4.4.4)        policía si no
                                           sospecha residual                                                       se resolvió)
                                           baja permanece)
```

#### 4.4.1 Estado NORMAL (0-24 de Sospecha individual)

El NPC funciona con su rutina normal, sin ninguna señal especial. Este es el estado de partida de todo NPC del pueblo.

**Cómo sube (fuentes que ya definimos):**
- **Timer de entrega atrasado (3.3):** cada día de retraso en un encargo cuyo "dueño original" es este NPC suma +8 a +15 de Sospecha individual, según cuántos días lleve atrasado.
- **Espionaje por la cerca (4.3):** si el NPC te espía y no reaccionas a tiempo, suma +10 a +25 de golpe según qué tan comprometedor era lo que vio (una tarea silenciosa parcialmente vista suma menos que sorprenderte a mitad de un desarme con soplete encendido).
- **Acumulación pasiva menor:** estar cerca del taller de noche repetidamente sin evidencia directa (simplemente "algo se siente raro") puede sumar +1 a +2 por semana si Meta B global ya está baja — el pueblo entero está más alerta cuando la reputación colectiva cae.

#### 4.4.2 Estado ALERTA (25-49 de Sospecha individual)

El NPC empieza a mostrar señales visibles pero no confrontativas: lo ves merodeando un poco más cerca del taller de lo normal, hace un comentario ambiguo en diálogo casual ("Oye, ¿ese auto de anoche no me suena de algún lado?"), o simplemente reduce ligeramente su calidez habitual contigo.

**Opciones del jugador en este estado (bajo costo, alta efectividad):**
- **Despistar/convencer:** diálogo corto donde le muestras algo inocente o le das una explicación creíble. Tasa de éxito alta en este estado temprano (referencia: 80%+). Si funciona, Sospecha baja a 0-5 y vuelve a NORMAL. Si falla (poco común aquí), sube a 50+ directamente por "quedar mal parado" en la excusa.
- **No hacer nada:** el estado no baja solo con el tiempo si la fuente de sospecha sigue activa (ej: el encargo sigue atrasado), pero sí decae lentamente (-1 a -2 por día) si ya no hay nueva causa activa.

#### 4.4.3 Estado SOSPECHA ALTA (50-74 de Sospecha individual)

El NPC ya está activamente incómodo: hace preguntas directas ("¿Qué es exactamente lo que hacen en las noches ahí atrás?"), se le ve hablando con otros vecinos sobre el tema (visible si pasas cerca en el pueblo — diálogo ambiental de fondo entre él y un tercero), o en casos de clientes recurrentes, reduce su frecuencia de visitas al taller.

**Opciones del jugador (más costosas, menor garantía):**
- **Despistar/convencer:** tasa de éxito baja a ~40-50% en este estado — ya no basta una excusa simple.
- **Sobornar:** pagar (referencia: $500-$2.000 según el NPC y su "precio", más caro si Meta B global ya es baja) para que "no vio nada". Efectividad alta (~85%) pero dos costos ocultos: (1) el NPC ahora *sabe* que ocultas algo, aunque se calle — su Sospecha baja a un piso de 15-20 en vez de 0, nunca vuelve del todo a NORMAL limpio; (2) deja un registro en tu Libro (si pagas con dinero sucio, cuenta como gasto no reportado, sección 4.5).
- **Incriminar/desviar:** requiere haber preparado evidencia falsa previamente (un ítem que puedes fabricar en ciertos encargos). Si tienes el ítem disponible, redirige la sospecha a otro NPC random del pueblo (quien sube su propia Sospecha individual +20, iniciando su propio ciclo) y este NPC baja a NORMAL. Moralmente el más "sucio" de las opciones no letales.

#### 4.4.4 Estado INVESTIGANDO (75-100 de Sospecha individual) — el punto de no retorno suave

Si no se maneja en SOSPECHA ALTA, el NPC entra en este estado: ya no solo sospecha, está buscando confirmar activamente (ronda el patio trasero de noche con más frecuencia, intenta hablar con el patrullero, o en el caso de un cliente reconoce visualmente una pieza/auto suyo en tu taller).

**Opciones del jugador aquí (todas de alto riesgo o alto costo):**
- **Sobornar:** todavía posible pero mucho más caro (2-3× el costo de SOSPECHA ALTA) y con menor efectividad (~50%).
- **Incriminar/desviar:** sigue disponible si tienes el ítem, mismo efecto que antes.
- **Reportar a Fabio (consecuencia implícita):** aquí es donde se activa lo que ya definimos en detalle — el jugador NUNCA ejecuta nada directamente. Fabio responde con una línea ambigua. Pasan 1-2 días de normalidad aparente (el NPC puede aparecer una vez más, nervioso). Luego, de noche, un auto desconocido se estaciona brevemente frente a su casa — visible de lejos, sin detalle. Al día siguiente, el NPC ya no aparece; su casa queda a oscuras, y con el tiempo un letrero de "SE VENDE/ARRIENDA" aparece. Nunca se confirma qué pasó. Efectos mecánicos: Sospecha de ese NPC se vuelve irrelevante (ya no existe en el mundo), pero Meta B global baja -10 de golpe +3 permanente acumulativo por cada vez que se usa esta opción (ya definido en 2.2).
- **No hacer nada:** si el jugador no actúa, hay una probabilidad creciente por día (referencia: +10% acumulativo diario) de que el NPC **escale a la autoridad por su cuenta** — va directamente al patrullero o a un inspector con lo que sospecha. Esto no es una desaparición ni un final automático, pero dispara un evento de investigación activa (visita de un inspector con causa concreta, mucho más difícil de manejar que una inspección aleatoria genérica) y sube la Meta B negativamente de forma notable (-15 a -25).

#### 4.4.5 Resumen de flujo para diseño/implementación

- La variable de Sospecha por NPC es local a cada personaje, no global — puedes tener 2-3 NPCs en distintos estados simultáneamente, cada uno gestionado o ignorado de forma independiente.
- El decaimiento pasivo (bajar solo con el tiempo) existe pero es lento y solo aplica en NORMAL/ALERTA — a partir de SOSPECHA ALTA, el jugador debe actuar activamente o el estado escala, nunca baja solo.
- Consecuencia acumulativa a nivel de mundo: usar "reportar a Fabio" repetidamente no solo baja Meta B — también hace que el pueblo entero tenga un "piso" de Sospecha pasiva ligeramente más alto para todos los NPCs restantes (representa que la gente está más nerviosa/observadora en general), haciendo el juego progresivamente más difícil de gestionar si abusas de esa opción — un incentivo de diseño natural para no volverla la solución por defecto.

### 4.5 El Libro Real vs. el Libro Oficial

Sin cambios: el jugador decide cuánto dinero sucio disfrazar como ingresos legítimos, con riesgo de auditoría si sobre-declara sin respaldo de clientela real, y capacidad limitada de efectivo escondido si sub-declara.

### 4.6 Préstamos del banco (nuevo, conecta con Meta A)

- Una vez pagada una porción de la deuda original (hito, no tiempo), el banco ofrece préstamos nuevos para mejoras específicas del taller (bahía adicional, elevador hidráulico, equipo de pintura profesional).
- **Riesgo real, como confirmaste:** un préstamo tiene cuotas con plazo. No pagar a tiempo sube tu Calor de forma directa (el banco manda notificaciones visibles, y en casos extremos un tasador visita el taller — evento jugable similar a una inspección, pero de origen legal, no policial) y puede empeorar tu tasa en préstamos futuros.
- Esto le da al jugador una tensión de "¿me endeudo más para crecer más rápido, sabiendo que eso también sube mi exposición?" — paralela pero distinta a la tensión con Fabio, y conecta directamente Meta A con Meta D.

---

## 5. EL MUNDO: LOS ALISOS (sin cambios de fondo)

### 5.1 Estructura semi-abierta

Pueblo recorrible y visualmente completo (calles, casas, plaza, comercios), pero solo el taller y su entorno inmediato son interactuables a fondo. El resto funciona como ambientación viva, fuente de rumor visual, y puntos de misión temporales durante encargos específicos.

### 5.2 El taller y sus alrededores

Recepción/mostrador, bahías de trabajo, patio trasero con la cerca, oficina/trastienda (Libro Real/Oficial, caja fuerte, llamadas de Fabio), y la calle frente al taller donde ves llegar a los clientes con anticipación real.

### 5.3 Ciclo día/noche real en el mundo

El pueblo cambia de luz, sonido y población según la hora — de día hay movimiento normal, de noche las calles están casi vacías salvo el patrullero ocasional.

---

## 6. LA VÍA DEL PODER — tomar el control (hito alcanzable en cualquier momento, no un capítulo)

### 6.1 Cómo se activa

Requiere una combinación de umbrales en las Metas B, C y el leverage propio (variable nueva, ver abajo):
- **Meta C (relación con Fabio) ≥ 70** — confía en ti lo suficiente para no anticipar el golpe.
- **Leverage acumulado ≥ un umbral** (referencia: 3 de 5 "piezas de leverage" posibles). El leverage es una lista de hechos que el jugador va acumulando de forma incidental, no un número abstracto: documentos comprometedores encontrados en encargos de alto nivel (cada uno es un objeto/ítem concreto que puedes revisar en tu inventario/oficina), y relaciones propias con 2+ compradores o proveedores que ya no dependen de Fabio como intermediario (esto se marca solo cuando completaste con éxito varias ventas directas con ellos, sección 4.2-B).
- El juego señaliza esto desde etapas tempranas: la primera vez que un encargo de alto nivel te da la opción de "quedarte con una copia" de algo comprometedor (en vez de entregarlo todo a Fabio), un tooltip/diálogo deja claro que estás empezando a construir algo. Es opcional en cada encargo puntual, así que el jugador que quiere esta vía la persigue activamente.

### 6.2 Cómo se ejecuta el reemplazo (flujo concreto, sin escena especial)

Confirmaste que prefieres esto simple — así que el flujo es:

1. Cuando se cumplen los umbrales de 6.1, aparece una nueva opción permanente en el teléfono/agenda de la oficina: **"Confrontar a Fabio"**. No es obligatoria ni tiene límite de tiempo — el jugador la usa cuando quiere.
2. Al seleccionarla, se abre una pantalla de diálogo con 2-3 líneas de texto que resumen la jugada (ej: Fabio reacciona según cuánto leverage tengas — el texto varía un poco según tus piezas de leverage específicas, dando algo de variedad sin necesitar una escena jugable nueva) y una elección final de confirmación ("¿Presentas tu posición a Fabio? Esto no se puede deshacer").
3. Resultado calculado en el momento según tus valores de Meta C y leverage:
   - **Si Meta C ≥ 70 Y leverage ≥ umbral:** pantalla de resultado exitoso — "Fabio acepta los términos. A partir de ahora, Los Alisos es tuyo." El juego marca internamente el estado "Jefe" = true y aplica los cambios de 6.3 desde el día siguiente.
   - **Si el jugador lo intenta sin cumplir ambos umbrales** (el juego puede permitir intentarlo igual, con una advertencia clara de que el riesgo es alto si no estás listo): pantalla de resultado fallido — deriva directamente al final "Golpe Fallido" (sección 7).
4. No hay minijuego de tensión ni cinemática — es una decisión de alto riesgo comunicada por texto y una pantalla de resultado, coherente con el resto del juego (que ya comunica sus momentos grandes vía UI/mensajes, como la desaparición implícita de NPCs en 4.4).

### 6.3 Qué cambia si tomas el control (efectivo desde el día siguiente)

- Ya no reportas a Fabio ni recibes su presión — el teléfono deja de sonar con sus llamadas; en su lugar, tú decides qué encargos acepta el taller, a qué compradores vendes, y a qué precio (un panel nuevo en la oficina reemplaza el flujo de "Fabio llama y ofrece").
- Meta B (Calor) sigue existiendo igual que antes, pero ahora puedes delegar parte del riesgo: un mini-sistema de "mandos medios" (NPCs que reclutas o intimidas) que manejan encargos menores a cambio de un corte de ganancias — mecánicamente, reduces cuánto Calor generas tú directamente a cambio de menos ganancia por encargo delegado.
- El pueblo, con el tiempo (umbral simple: acumular X días como "Jefe"), empieza a tratarte distinto en diálogos ambientales — menos calidez, más cautela — sin que el juego lo explicite como bueno o malo.

### 6.4 Por qué esto funciona sin capítulos fijos

Al ser un hito de logro combinado y no un evento de capítulo, el jugador que juega "políticamente" desde el principio puede llegar ahí mucho antes que alguien que juega de forma más pasiva — la velocidad de tu progreso depende de tus decisiones día a día, no de una estructura de niveles.

---

## 7. FINALES — cómo se "gana" y cómo se comunica

### 7.1 Flujo general de cierre

Confirmaste que prefieres esto simple: **no hay cinemáticas, solo un resumen/mensaje en pantalla claro cuando cierras una etapa**, similar a una pantalla de resultado de fin de partida. En concreto:

- Para los finales que el jugador elige activamente (Taller Limpio, El Nuevo Nombre, Traición Calculada): aparece una nueva opción en el teléfono/agenda ("Cerrar cuentas con el banco y retirarte", "Consolidar tu posición como Jefe", etc.) una vez se cumplen sus condiciones — el jugador la activa cuando quiere, sin presión de tiempo.
- Al activarla: pantalla de resumen con las estadísticas clave de tu partida (días jugados, dinero total ganado, Meta B/C finales, cuántas veces usaste "reportar a Fabio") y 2-4 líneas de texto que narran el desenlace según tus números.
- Para los finales que se disparan automáticamente por condición crítica (Caída, Golpe Fallido): la pantalla de resumen aparece de forma inmediata cuando se cruza el umbral (ej. Meta B en -90 sostenido 3 días), sin que el jugador tenga que buscarlo.
- Tras cualquier pantalla de resumen, un botón claro **"Continuar en Los Alisos"** te lleva directo a Temporada Continua (sección 8) con tu estado actual — así el "final" nunca se siente como un muro que corta el juego, solo un marcador de haber cerrado una etapa.

### 7.2 Condiciones concretas por final

**Si seguiste como empleado/subordinado de Fabio:**
1. **"Taller Limpio"** — condición: Meta A al 100% (deuda saldada) + jugador activa manualmente "Cerrar cuentas con el banco y retirarte". Variante de tono: si Meta B ≥ +30 al momento de cerrar, el pueblo te respalda visiblemente en el resumen; si Meta B es baja, el cierre es más silencioso/solitario.
2. **"Imperio de Fabio"** — condición: Meta C ≥ 70 + el jugador elige activamente la opción de "Aceptar ascenso de Fabio" (alternativa a "Confrontar a Fabio" de la Vía del Poder) cuando esta se ofrece.
3. **"Caída"** — se dispara automáticamente, sin acción del jugador, si Meta B llega a -90 y se sostiene 3 días seguidos sin subir.
4. **"Traición calculada"** (secreto) — condición: acumular 4 de 5 piezas de leverage (igual que 6.1) pero en vez de confrontar a Fabio, usar la opción alternativa "Entregar evidencia a las autoridades" que se habilita en paralelo una vez tienes ese leverage.

**Si tomaste el control de la operación (sección 6):**
5. **"El Nuevo Nombre"** — condición: éxito en "Confrontar a Fabio" (6.2) + jugador elige activamente cerrar esa etapa vía "Consolidar tu posición como Jefe" cuando lo desee. La Temporada Continua arranca directamente en este estatus.
6. **"Golpe Fallido"** — se dispara automáticamente como resultado de intentar "Confrontar a Fabio" sin cumplir los umbrales de 6.1.

**Matiz por el sistema de sospecha (4.4):** usar "reportar a Fabio" repetidamente resta puntos permanentes a Meta B (ver 2.2) — esto se refleja automáticamente en el resumen de cualquier final vía el valor final de Meta B, sin necesitar lógica adicional.

---

## 8. TEMPORADA CONTINUA (el "infinito" real)

Tras cualquier final, o si el jugador simplemente nunca decide "cerrar" la historia, el juego sigue como un loop de gestión continuo: el pueblo sigue generando clientela, encargos, y eventos aleatorios (incluyendo ocasionalmente los "pedidos que aún no puedes cumplir" de la sección 4.1) indefinidamente, con dificultad y complejidad creciente. Si terminaste como "El Nuevo Nombre", Temporada Continua arranca con ese estatus de poder ya en tus manos, incluyendo el sistema de mandos medios.

Esto es lo que le da al juego duración indefinida más allá de las 10-20 horas de contenido narrativo dirigido — comparable a cómo un jugador de Stardew Valley sigue jugando años de granja después de completar los hitos "principales" del juego.

---

## 9. COOPERACIÓN ENTRE 2+ JUGADORES (sin cambios de fondo)

- Roles fluidos e intercambiables: cualquiera puede atender clientes, trabajar encargos, o moverse por el pueblo.
- Cooperación real en tareas de dos manos y en misiones de recolección (uno conduce/vigila, el otro carga/interactúa).
- Con el sistema de la cerca: si ambos están en el patio y llega un espía, hay decisión inmediata de quién intercepta y quién sigue trabajando.
- Las 4 metas paralelas son compartidas por el equipo — las decisiones de uno (aceptar un encargo, reportar a un NPC) afectan las metas de ambos, lo cual da pie a negociación real entre jugadores sobre qué camino tomar.

---

## 10. VIABILIDAD PARA DESARROLLO CON APOYO DE IA

### 10.1 Por qué el sistema de metas paralelas no es más complejo de construir que capítulos fijos

De hecho es más simple en varios sentidos: en vez de scriptear transiciones narrativas rígidas entre "niveles", construyes:
- Cuatro variables numéricas (Metas A-D) con umbrales que disparan eventos/desbloqueos — es lógica de sistemas directa, muy traducible a código con ayuda de IA.
- Un pool de eventos/misiones que se activan cuando se cumplen sus condiciones de umbral, en vez de una secuencia lineal de niveles — más parecido a un sistema de quests de juego de rol abierto que a niveles de plataformas.

### 10.2 Orden recomendado de construcción

1. Minijuego de reparación/lijado (validar el "feel" táctil).
2. Loop de un día completo con negocio honesto solamente, sin el pueblo aún.
3. Implementar las 4 Metas paralelas como variables numéricas simples con 2-3 umbrales cada una, sin contenido narrativo todavía — validar que el sistema de progresión "se siente vivo" incluso con contenido placeholder.
4. Añadir el pueblo como espacio visible/recorrible.
5. Sistema de Libro Real/Oficial y primer encargo sucio de Fabio.
6. Sistema de la cerca (4.3) como mecánica aislada.
7. Sistema de sospecha de NPCs (4.4).
8. Préstamos del banco (4.6) conectando Meta A y Meta D.
9. Vía del poder (sección 6) y finales (sección 7).
10. Multiplayer sobre el prototipo ya validado en solitario.
11. Temporada Continua y expansión de contenido narrativo (más NPCs, más variantes de encargo, eventos raros del tipo "pedido que aún no puedes cumplir").

### 10.3 Dónde la IA puede acelerar más

- Generación de diálogos ambientales de NPCs y de las líneas de "chisme" que reflejan el estado de tus 4 metas.
- Scripts de sistemas (las máquinas de estado de cada Meta, la lógica de umbrales y desbloqueos, el sistema de la cerca).
- Iteración de balance numérico (cuánto sube cada Meta por acción, curvas de costo de préstamos y mejoras) — se puede prototipar y ajustar rápido generando variaciones para probar.

Lo que sigue dependiendo de tu criterio humano: si el minijuego de lijado se siente bien, si el ritmo de las 4 metas avanzando en paralelo se siente vivo y no abrumador, y si el tono de las desapariciones implícitas aterriza como querías.

---

## 11. GANCHO VIRAL / SHAREABILIDAD

- El momento de un NPC dejando de aparecer tras "reportarlo a Fabio" — sin confirmación — sigue siendo el gancho narrativo más fuerte para clips reflexivos.
- El sistema de la cerca (alguien espiando mientras trabajas, el ícono de aviso, la carrera por taparlo a tiempo) es el momento de pánico cooperativo más directo y frecuente del juego.
- El progreso simultáneo de las 4 metas se presta a contenido de "mira mi run" tipo build/estrategia (¿enfocarse en pagar la deuda rápido? ¿en tomar el control cuanto antes?), dando variedad de historias que compartir entre jugadores distintos.
- El minijuego táctil de lijado/limado sigue siendo el clip corto perfecto de alta calidad sensorial.

---

## 12. RIESGOS Y MITIGACIONES

| Riesgo | Mitigación |
|---|---|
| Sin capítulos, el jugador puede sentirse perdido sobre "qué hacer ahora" | Un panel/pizarra en la oficina que resume visualmente el estado de las 4 metas y sugiere la siguiente acción lógica (sin forzarla) |
| Los eventos por hito combinado (ej. la vía del poder) pueden sentirse invisibles si el jugador no sabe que existen | Sembrar pistas tempranas (diálogos, documentos) desde etapas iniciales, no solo cuando ya casi se cumplen las condiciones |
| El pueblo "decorativo" puede sentirse vacío si no se maneja bien | Rutinas simples pero creíbles de NPCs de fondo, diálogo ambiental barato que refleje el estado de tus metas |
| El sistema de desaparición implícita puede sentirse "gratis" si no tiene peso | Reputación del Pueblo y finales reflejan acumulativamente el uso de esta opción, no solo una vez |
| El espionaje por la cerca puede sentirse injusto si es puramente aleatorio | Probabilidad atada a ruido/luz real de la tarea, siempre con ventana de reacción visible |
| Cuatro metas paralelas pueden ser difíciles de balancear numéricamente | Prototipar cada meta por separado con valores simples antes de integrarlas todas juntas (paso 3 de 10.2) |

---

## 13. PRÓXIMOS PASOS SUGERIDOS

1. Decidir el nombre definitivo (sección 0.1).
2. Prototipar el minijuego de lijado/limado.
3. Llevar los valores de referencia de las 4 Metas (sección 2) a una hoja de cálculo y simular unos cuantos "días tipo" a mano (o con ayuda de IA) para verificar que el ritmo de subida/bajada se siente razonable antes de programarlo.
4. Bocetar el layout de Los Alisos.
5. Escribir el guion de diálogos ambientales base (los que reflejan el estado de las metas y del pueblo, no de "capítulos"), incluyendo variantes por cada estado del sistema de sospecha (4.4.1-4.4.4).
6. Implementar el sistema de sospecha (4.4) como una máquina de estados aislada y probarla con NPCs de prueba antes de conectarla al resto del juego — al ser lógica bien definida (estados, umbrales, transiciones), es directamente traducible a código con ayuda de IA.
7. Prototipar la pantalla de "Confrontar a Fabio" y las pantallas de resumen de final (7.1) como UI simple — es contenido barato de iterar y define el tono de los momentos más importantes del juego.
