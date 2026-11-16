# Revisión 8 — colisiones, colocación y apariciones

Proyecto fuente para Unity 6.0, Built-in Render Pipeline. Conserva los acertijos, las pilas, la recarga con R y la cámara de visión nocturna sin linterna.

## Objetos y escena

- Los modelos estáticos de ambos constructores llevan ahora MeshCollider en la capa de obstáculos. Sillas, carros, sillas de ruedas, biombos, lavamanos y otros equipos ya no quedan deliberadamente sin colisión. Los interactuables conservan BoxCollider para facilitar su selección. La videocámara en mano y el fantasma no usan colisión sólida.
- Los muebles colocados en el suelo alinean la base real de la malla con y=0. Corrige, entre otros, cilindros de oxígeno y biombos cuyo origen no coincidía con sus patas.
- Se retiraron camas que invadían los escritorios, se separaron equipos de rayos X de carros/portasueros y se apartaron basureros de lavamanos.
- Se corrigieron alturas de monitor, teléfono, lámpara de escritorio, desfibrilador, bandejas y grabadoras. Se añadió una mesa bajo la grabadora de entrada. Se despejó la superficie del carro clínico de geometría incorporada que atravesaba el monitor.
- El cuadro de recepción y la válvula de mantenimiento están apoyados contra sus paredes. Los carteles tienen placa y suspensión; lámparas y ventiladores se apoyan en el techo. El acceso tiene techo para sostener sus luminarias.
- La mesa final deja espacio para la camilla trasladada. Si el jugador ocupa ese espacio, se busca una posición libre después del traslado.
- Los colliders generados se enlazan a las mallas guardadas al exportar HospitalEditable, para que sobrevivan al cierre de la escena.

## Materiales y fantasma

- Paredes y pisos utilizan ahora el mapa de normales existente mediante proyección triplanar, además del albedo y las máscaras de material. La humedad añade variación suave de color y brillo cerca del suelo.
- Sebastián incorpora una bata con pliegues, borde irregular y proporciones faciales revisadas. Un material propio desatura su aspecto, añade una emisión tenue y permite su aparición/desaparición gradual.
- Desde recepción, con ira baja o media, las apariciones buscan un punto visible, sin muro delante, conectado por el mapa y a 6–15 metros. Si no existe, se reintenta tras cinco segundos. En persecución se conservan los puntos ocultos y el margen inicial antes de capturar.
- Las apariciones no atraviesan paredes para mostrarse. El desplazamiento del enemigo sigue comprobando el mapa y obstáculos. La geometría permanece procedural y sin rig esquelético; esto no es un personaje fotorrealista ni una producción AAA.

## Audio

Cuatro WAV originales sintetizados: DistantScream (grito lejano), GhostWhisper (susurro sin palabras), MetalDrag (arrastre metálico) y GhostArrival (entrada del fantasma). Incluyen envolventes y colas de eco; no son grabaciones de actores.

Los sonidos ambientales aparecen desde recepción, con intervalos de 22–42 segundos de juego y sin repetir inmediatamente el anterior. Se suspenden nuevos eventos ambientales cuando Sebastián está activo. Las apariciones tienen sus propios sonidos al aparecer y al ser vistas. Las fuentes son posicionales; un filtro reduce agudos cuando hay un obstáculo entre fuente y oyente. No simula acústica física completa de habitaciones.

En Opciones está el control **Gritos y sonidos de terror**, independiente del volumen general. Las voces posicionales se pausan al leer documentos o pausar. No se inicia un nuevo susto desde menús.

## Comprobaciones realizadas y límites

- Auditoría Python de las llamadas reales de colocación en ambos constructores: 239 colocaciones, alineación al suelo y ausencia de intersecciones entre las cajas envolventes de los muebles de suelo incluidos en la prueba.
- Comprobación adicional de la camilla en su posición final y seis apoyos sobre mesas/estantes.
- Mallas, índices, normales, UV, espacios de color, recursos, WAV y sintaxis C# revisados por los verificadores incluidos.
- Lámina de inspección `Objetos_Revision8.png`: geometría y albedo renderizados fuera de Unity; no representa la iluminación final ni el shader del fantasma.

**No se ejecutó Unity aquí.** Quedan pendientes compilación de shaders/C#, recorrido completo, colisiones del CharacterController, mallas de colisión después de reabrir escena, escucha de la mezcla final y rendimiento. La auditoría de cajas no demuestra que todos los rincones sean accesibles ni detecta cualquier contacto entre triángulos. Los MeshCollider detallados pueden aumentar el coste de carga/física. No se añadió ray tracing y no se promete ultrarrealismo ni ausencia absoluta de errores.

## Abrir y probar

1. Descomprime en una carpeta nueva y abre el proyecto con Unity 6.0 usando Built-in.
2. Abre `Assets/Ecos/Scenes/Inicio.unity`. Para editar el escenario usa **Ecos de Chuquicamata > Generar hospital editable**. Respalda cambios propios antes de regenerar: una escena antigua conserva las posiciones anteriores.
3. Ejecuta los menús de **Validacion**: Recursos y guardado; Colisiones y apoyo de muebles; Mallas de escena editable (con HospitalEditable abierto). Repite la última después de cerrar y reabrir.
4. En Play recorre el hospital y ejecuta **Validacion > Acceso a objetos en Play**. Prueba caminar y agacharte contra carros, biombos y camas; comprueba las pistas y puertas.
5. En recepción mira hacia un tramo libre del pasillo. Usa **Pruebas en Play > Aparicion visible** y **Grito lejano** para revisar los nuevos eventos. La prueba de aparición cambia temporalmente la ira a 15; no fuerza un guardado.
6. Comprueba Q, R, recogida de pilas, pausa durante un sonido y traslado de la camilla final. Si aparece un material rosado, copia el primer error de shader de Console.

Reconstrucción: `realistic_props.py` regenera modelos/audio base; `polish_audio.py` regenera sonidos mecánicos; `horror_audio.py` genera los cuatro nuevos. Los atlas se regeneran con `build_materials.py` y `build_prop_materials.py`. Los scripts están en SourceArt y requieren Python, numpy y Pillow. Las pruebas C# estáticas y de colocación requieren también tree-sitter y tree-sitter-c-sharp.
