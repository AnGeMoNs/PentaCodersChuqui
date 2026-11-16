# Sebastián · ataques y música · revisión 11

Esta entrega conserva el modelo, las texturas y las voces grabadas de la revisión 10. Amplía la animación del esqueleto y reemplaza la antigua capa de tensión constante por una partitura adaptativa original.

## Animaciones

- Acecho con respiración, inclinación de cabeza y movimiento lento.
- Gesto de reconocimiento de 0,75 segundos antes de iniciar la carrera.
- Carrera más encorvada, brazos alternados y balanceo al girar.
- Brazos extendidos cuando se acerca al jugador.
- Búsqueda con movimientos de cabeza cuando pierde el contacto visual.
- Tres ataques alternados, zarpazo izquierdo, zarpazo derecho y agarre frontal.
- Recuperación visible después de fallar y mandíbula con amplitud limitada para reducir deformaciones.

Las poses de ataque están en `Assets/Ecos/Resources/Sebastian/Motion.json`. Se aplican al esqueleto existente mediante código e interpolación suave. No son clips de captura de movimiento ni requieren un Animator Controller.

## Cuándo puede capturarte

Sebastián comienza el ataque a menos de 1,25 m, con visión directa y después de terminar la protección de aparición. La preparación dura 0,36 s. La embestida transcurre entre 0,36 y 0,56 s, con un desplazamiento máximo de 1,10 m. Su dirección queda fijada al comenzar, por lo que puedes apartarte hacia un costado.

El impacto se evalúa una sola vez. Requiere estar a un metro o menos, dentro del arco frontal y con visión directa. El desplazamiento comprueba la cápsula contra los obstáculos del mapa y se detiene antes de superponer los centros del personaje y el jugador. Las colisiones físicas finales deben comprobarse en Unity.

Si conecta, hay una breve animación de captura de 0,22 s antes de la derrota. Durante ese tramo se bloquea el movimiento del jugador. Si falla, termina su recuperación y espera 0,5 s antes de poder iniciar otro ataque. Los tirones de FPS no pueden avanzar más de 0,1 s de ataque en una sola actualización.

Pausar o abrir un panel detiene la secuencia. Reiniciar el checkpoint o volver al menú cancela el ataque. La cámara de visión nocturna y las pilas conservan sus controles.

## Música adaptativa

Tres capas originales de 16 segundos, 120 BPM y 48 kHz se inician con el mismo reloj de audio.

| Capa | Función |
|---|---|
| DreadLayer | Graves y texturas sostenidas durante el acecho |
| ChaseLayer | Pulsos, percusión y figuras disonantes durante la persecución |
| PanicLayer | Agudos, raspados y mayor densidad cuando está muy cerca |

La mezcla responde a la detección, la distancia y el estado de ataque. Al perderlo, desciende gradualmente durante varios segundos. Un acento breve marca la detección, con un intervalo mínimo de diez segundos entre repeticiones. El movimiento del ataque y el impacto tienen sonidos propios.

La música se detiene en muerte, final y menú, y se pausa al abrir paneles o pausar la partida. El tema anterior queda para el menú. **Opciones > Música de tensión** permite regular estas capas por separado de los gritos. El volumen se guarda en PlayerPrefs.

Los seis recursos nuevos son síntesis musical y diseño sonoro originales. No contienen música extraída de otros juegos. Las voces grabadas de la revisión anterior siguen teniendo sus créditos CC0.

## Probar en Unity

1. Abre `Assets/Ecos/Scenes/Inicio.unity` y entra en recepción.
2. Ejecuta `Ecos de Chuquicamata > Pruebas en Play > Persecucion de Sebastian`.
3. Para mirar los movimientos sin morir, activa `Alternar ataques sin captura`. Permanece cerca para ver los tres ataques. Vuelve a usar el menú para restaurar la captura normal. Esta función solo existe en el editor y se desactiva al reiniciar el actor.
4. Ejecuta `Ecos de Chuquicamata > Validacion > Ataques y musica`. Comprueba reglas de anticipación, pausa, fallo, captura única, retraso tras el impacto y recursos musicales.
5. Prueba también una puerta cerrada, un mueble entre ambos, esquivar hacia un costado, pausar a mitad del ataque y reintentar el checkpoint.
6. Comprueba la transición musical al escapar y cambia el volumen desde Opciones.

Si utilizas una `HospitalEditable` antigua, respalda tus cambios y regenera la escena. Abre la entrega en una carpeta nueva para conservar tu versión anterior.

## Vista previa y validación

`Sebastian_Animaciones_11.mp4` muestra 16 segundos de poses calculadas con la malla real y los mismos datos de ataque. Incluye una mezcla de muestra de la música. Es una inspección de animación en sitio fuera de Unity, sin la escena, sus colisiones ni el shader final. `Musica_Persecucion_11.wav` permite escuchar la muestra por separado.

Se revisan sintaxis C#, recursos, geometría, referencias de audio, 206 poses de ataque y detección, apoyo de pies, límites de render y bucles musicales. El menú de pruebas C# está incluido para ejecutarse dentro del editor. El entorno de creación no tiene Unity, por lo que la compilación del proyecto, las pruebas de combate con física real, la mezcla final y los FPS siguen pendientes.

## Reconstrucción opcional

- `python SourceArt/chase_music.py` genera la música y los efectos, con numpy y scipy.
- `python SourceArt/preview_motion.py` genera el video, con numpy, scipy, Pillow, moderngl/EGL y ffmpeg.
- `python Tests/verify_horror_director.py` comprueba poses y audio, con numpy y scipy.

Python no es necesario para abrir o jugar el proyecto en Unity.
