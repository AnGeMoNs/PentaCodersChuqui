# Revisión 3: errores y ambientación

## Qué se corrigió

1. Guardado: se rechazan valores NaN/infinito, coordenadas fuera de los límites y batería agotada. Se normalizan batería, ira, orientación y zona, y se quitan entradas vacías o duplicadas. Un guardado inválido no reemplaza el anterior. Se conserva la lectura de versiones 1 y 2.
2. Continuar: se restaura primero el estado de puertas y objetos; si el punto guardado está ocupado por un collider, se busca un punto despejado antes de colocar al jugador.
3. Reiniciar: restablece altura y centro del controlador, postura, cámara, zoom, resistencia, temporizador de pasos y objetivo de interacción.
4. Agacharse: la comprobación para levantarse usa una cápsula y la misma capa de colisión que los muebles sólidos y puertas.
5. Persecución: el grafo de navegación comprueba colliders de mobiliario; el desplazamiento de Sebastián también comprueba el trayecto. Se añade el acercamiento directo cuando comparte celda con el jugador y el segmento está despejado. Falta probar estas situaciones dentro de Unity; no se garantiza que el grafo de dos metros resuelva todos los rodeos.
6. Ira: el incremento por descubrir una zona se aplica una sola vez por partida, evitando acumularlo al cruzar continuamente una misma frontera.
7. Pilas: recoger una pila crea checkpoint para conservar su consumo y la carga resultante.
8. Interfaz: se restaura la matriz y el estado habilitado de IMGUI incluso cuando una acción interrumpe el evento GUI.
9. Cambiar de ventana: una partida activa se pausa al perder foco.
10. Puzzles: se comprueba el panel y estado antes de procesar código, decisión de pruebas y generador, evitando repetir recompensas a través de llamadas duplicadas.
11. Escenario: la grabadora del final tiene una mesa de apoyo y una ubicación accesible desde el borde interior de la sala. Una bandeja se colocó en el estante inferior del carro para no superponerse al monitor.

## Objetos y materiales

El proyecto contiene **54 modelos originales**, 12 más que antes:

- Mesa de noche con cajones.
- Estantería de medicamentos con frascos y cajas.
- Cajas de archivo con tapas, etiquetas y cinta.
- Desfibrilador con controles, asas, paletas y cables.
- Carro de limpieza con baldes y utensilios.
- Lámpara de escritorio articulada.
- Reloj de pared con esfera, marcas y agujas.
- Camilla de transporte con estructura y correas.
- Asientos de vinilo.
- Tablero eléctrico con interruptores y conductos.
- Bandeja con instrumentos médicos.
- Lavamanos con superficie cóncava continua, desagüe y grifería.

Cuatro superficies nuevas: **vinilo, cartón, porcelana y goma**, con mapas aproximados de normales y propiedades de material. Se aplican también a partes de la silla de ruedas, la videocámara y el lavamanos anterior. Se mantienen las seis superficies de la revisión previa.

La imagen fuente adicional tiene 1254 × 1254 píxeles y cuatro regiones de 627 × 627. Se empaquetan en un atlas 1024 × 1024, con 496 × 496 píxeles útiles por material más márgenes. No se afirma resolución 4K por superficie. Los mapas técnicos derivan de la imagen y parámetros elegidos; no son mediciones de escaneo PBR.

La referencia es el terror de hospitales abandonados; no se utilizaron modelos de Resident Evil. Este sigue siendo un proyecto procedural independiente y no una producción AAA.

`Objetos_Revision3.png` muestra mallas y colores reales renderizados por software, no una captura de Unity. El vidrio es opaco y los personajes aún no tienen rig ni animación esquelética.

## Qué se comprobó aquí

- Parser de sintaxis de 17 archivos C#; no es compilación semántica.
- Integridad de 54 mallas: índices, normales unitarias y caras válidas.
- UV sin triángulos degenerados; regiones de material consistentes.
- Resoluciones y espacio de color de los atlas y mapas.
- 16 audios: formato y pico individual.
- Grafo lógico de 328 celdas: orden de puertas y alcance por etapas, sin simular la física de Unity.
- Soluciones de juguetes, caja y generador; requisitos del final.
- Conservación de visión nocturna, pistas físicas y migración del guardado.
- Inspección visual de seis nuevos modelos.
- Integridad del ZIP.

Resultados reproducibles en `RESULTADOS_ESTATICOS.txt`. No está instalado Unity, Blender ni un compilador C# en este entorno. Por eso **no se ejecutaron** la compilación de Unity, los shaders en GPU, el recorrido jugable, las pruebas de física ni la build de Windows. No se puede afirmar que el juego esté libre de todos los bugs.

## Comprobaciones incluidas para Unity

1. Abre esta entrega en una carpeta nueva con Unity 6.0.x, Built-in.
2. Espera a que termine la importación y abre Window > General > Console.
3. Ejecuta `Ecos de Chuquicamata > Validacion > Recursos y guardado`. Comprueba las mallas cargadas por Unity, tangentes, errores conocidos de shaders, texturas y casos del validador de partidas. No modifica PlayerPrefs.
4. Abre `Assets/Ecos/Scenes/Inicio.unity`, entra en Play y comienza la partida.
5. Ejecuta `Ecos de Chuquicamata > Validacion > Acceso a objetos en Play`. Es un muestreo local de posiciones y rayos de interacción; cualquier aviso requiere revisión manual y no demuestra por sí solo que el objeto sea inaccesible.
6. Prueba ambas rutas finales, muerte por batería y persecución, pausa, alt-tab, agacharse bajo objetos, reiniciar desde agachado y continuar después de recoger una pila.
7. Regenera HospitalEditable desde el menú si usabas una escena anterior. Respalda las ediciones personales antes de regenerarla.

Los menús de validación están incluidos pero no se ejecutaron aquí. El rendimiento, la navegación alrededor de muebles, la mezcla de audio y la iluminación necesitan medición dentro del motor.

## Reproducir assets

`python SourceArt/realistic_props.py` reconstruye los 54 modelos y el audio.
`python SourceArt/build_materials.py` reconstruye los atlas originales.
`python SourceArt/build_prop_materials.py` reconstruye los atlas adicionales.
Se requieren Python, numpy y Pillow. Los modelos JSON son los utilizados por el juego; los OBJ se entregan como fuente geométrica y su MTL básico no reproduce el shader de Unity.
