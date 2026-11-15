# Sebastián · revisión 10

Esta revisión cambia el modelo que carga el juego y sustituye las tres voces sintéticas de la revisión 9. No es solamente una ilustración de referencia.

## Modelo y materiales

- Rostro con superficie continua, puente nasal, pómulos y cuencas hundidas. Ojos completamente oscuros, sin pupilas blancas ni emisión de luz.
- Cabeza con pelo sólido y mechones finos, cuello más corto, hombros redondeados, dedos, uñas, tobillos y dedos de los pies.
- Texturas propias de rostro y algodón hospitalario envejecido, cada una de **1254 × 1254 píxeles nativos**. No se han reescalado para presentarlas como 2K o 4K. La importación permite hasta 2048 y conserva la resolución original.
- Dos mapas de microrelieve derivados de las texturas. Son aproximaciones, no escaneos PBR ni mapas esculpidos de alta resolución.
- **90.792 triángulos y 74.518 vértices de render** en el personaje. Se han unido duplicados preservando costuras UV y pesos. Los demás modelos del hospital se mantienen.
- Esqueleto de 17 huesos. El rostro inferior responde a la mandíbula y la postura de persecución extiende los brazos con las muñecas hacia abajo.
- La mandíbula usa la intensidad de salida de la voz. No emplea el movimiento sinusoidal de la versión anterior.
- Material Built-in con iluminación Standard, sombras y disolución de aparición. No incluye ray tracing.

La imagen `Sebastian_Revision10.png` se calculó con la malla y sus texturas reales. Es una inspección fuera de Unity. No muestra el shader final, la iluminación del hospital ni rendimiento medido.

## Sonidos sustituidos

Las voces parten de grabaciones públicas de Freesound bajo CC0. Se incluyen los archivos fuente de preescucha en alta calidad MP3, sus autores y sus enlaces. No son grabaciones encargadas ni una voz clonada.

| Archivo | Uso | Base |
|---|---|---|
| SebastianScream / SebastianScreamB | Gritos infantiles al perseguir | Child Scream, onehugeeye |
| SebastianSob / SebastianSobB | Lamentos distantes durante apariciones | Fragmentos ralentizados del mismo grito |
| SebastianSnarl / SebastianSnarlB | Voz de monstruo con una capa infantil | Monster screams.wav, gpag1, y Child Scream |
| SebastianBreath | Respiración gutural | Short Monster Scream, djani00 |
| SebastianStep | Arrastre de pies conservado | Efecto original de revisión 9 |

Los siete archivos vocales se exportan a WAV mono de 48 kHz. El cambio de formato no recupera información perdida en la compresión MP3 de las fuentes. Las variantes evitan repetir inmediatamente la misma toma dentro de cada grupo. Los nombres Sob se conservan por compatibilidad, pero ahora contienen lamentos derivados de un grito grabado, no una grabación literal de sollozos.

Se aplican filtrado suave, ajustes moderados de velocidad y reverberación corta. Los picos individuales están alrededor de −7 dBFS. La voz se mueve con Sebastián, se atenúa con la distancia y se filtra detrás de obstáculos. La mezcla final dentro del hospital requiere escucha en Unity.

## Abrir y probar

1. Descomprime esta entrega en una carpeta nueva. Abre el proyecto con Unity 6.0 y Built-in.
2. Abre `Assets/Ecos/Scenes/Inicio.unity` y entra en Play.
3. Para escuchar las muestras, usa `Ecos de Chuquicamata > Pruebas en Play > Grito infantil de Sebastian` o `Voz demoniaca de Sebastian`.
4. En recepción, mira hacia un tramo libre del pasillo y usa `Aparicion visible`. Para la carrera, usa `Persecucion de Sebastian`.
5. Prueba la visión nocturna con clic derecho y Q. Ajusta `Gritos y sonidos de terror` desde Opciones.
6. Si trabajas con `HospitalEditable`, respalda tus modificaciones y regenera la escena con `Generar hospital editable`. Una escena generada anteriormente puede conservar la malla y el material antiguos.
7. Ejecuta `Validacion > Recursos y guardado`, `Rig de Sebastian` y `Mallas de escena editable` dentro del editor.

Los menús de aparición y persecución cambian la ira para probar el comportamiento. No fuerzan un checkpoint, aunque un guardado posterior podría conservar esa ira.

## Reconstrucción

Python no es necesario para jugar o abrir el proyecto.

- `python SourceArt/rebuild_sebastian.py` reconstruye solo la malla. Requiere numpy y scipy.
- `python SourceArt/build_sebastian_materials.py` reconstruye los materiales desde los dos PNG originales incluidos. Requiere numpy y Pillow.
- `python SourceArt/sebastian_audio.py` reconstruye las siete voces usando los MP3 incluidos. Requiere numpy, scipy y ffmpeg.
- `python SourceArt/preview_sebastian.py` genera la inspección visual. Requiere numpy y Pillow.

`SourceArt/Sebastian.obj` es una exportación estática de geometría. El rig, los materiales del personaje y las referencias de texturas se crean en Unity a partir de JSON y de los scripts. El OBJ no sustituye el prefab articulado.

## Validación y límites

Se comprueban sintaxis C#, geometría, UV, normales, pesos, poses, apoyo de pies, integridad de imágenes y archivos de audio. También se vuelven a comprobar la progresión lógica y las posiciones existentes de los muebles.

Este entorno no tiene Unity. La compilación de shaders y scripts en el motor, la escucha de la mezcla durante el juego, las deformaciones en movimiento y los FPS siguen pendientes. El resultado es un personaje procedural más detallado con materiales propios, no un escaneo humano ni una producción AAA.
