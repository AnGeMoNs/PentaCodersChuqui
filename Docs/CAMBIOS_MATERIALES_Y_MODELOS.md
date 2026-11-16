# Revisión de modelos y materiales

Esta entrega mejora los 42 modelos originales del proyecto Unity Built-in. No es una reproducción de assets de Outlast o Resident Evil ni una producción AAA terminada. Mantiene la visión nocturna y los acertijos de la revisión anterior.

## Geometría

- Bordes biselados en muebles y carcasas; tubos de al menos 24 lados; superficies curvas con normales suaves.
- Camas con sábanas onduladas, almohadas redondeadas y barandas; cuerpos cubiertos con volumen bajo la sábana.
- Silla de ruedas rehecha con neumáticos huecos, aros, 24 radios por rueda y apoyabrazos.
- Biombo con tela ondulada, caras por ambos lados y anillas.
- Videocámara con anillos del objetivo, mandos, rejillas y agarres.
- Manómetro con aguja y marcas, tornillería en muebles y equipos.
- Sebastián conserva un modelo procedural sin rig; tiene curvas suavizadas y detalles de manos, rostro y ropa. No constituye un personaje realista esculpido ni animado profesionalmente.

| Modelo | Triángulos anteriores | Triángulos actuales |
|---|---:|---:|
| Cama | 588 | 12212 |
| Silla de ruedas | 632 | 9640 |
| Videocámara | 264 | 4280 |
| Biombo | 384 | 12108 |
| Sebastián | 956 | 10572 |

Total de los 42 assets únicos: 131472 triángulos; las instancias del nivel multiplican este total. No se añadieron LODs ni se midió rendimiento en Unity.

## Materiales

Seis superficies originales generadas: esmalte descascarado, acero oxidado, lino manchado, madera, yeso húmedo y baldosas. Las fuentes tienen **1254 × 1254 píxeles**. Se reducen para un atlas de **4096 × 2048** con seis regiones útiles de **1008 × 1008** y márgenes de 8 píxeles. El tamaño del atlas no significa que cada material tenga resolución 4K.

El shader de objetos usa color base, normales de detalle y máscara de metalicidad, suavidad y oclusión aproximada. Los mapas técnicos derivan de la luminancia del color, con parámetros por material: no son escaneos PBR medidos ni normales horneadas de esculturas. Las paredes y el suelo usan proyección triplanar del color y valores de rugosidad distintos. Papel, piel, vidrio y plástico conservan un tratamiento sencillo de color; no todas las superficies tienen texturas dedicadas.

`Assets/Ecos/Resources/PBR` contiene los tres atlas. `SourceArt/TextureSources` contiene las fuentes JPEG de las seis superficies. `SourceArt/detail_mesh.py`, `build_materials.py` y `extend_assets.py` permiten reproducir el procesamiento técnico y la geometría. La generación de imágenes original no se reproduce con Python.

`Docs/Modelos_Detallados.png` es una inspección renderizada de las mallas y el color base reales, mediante un rasterizador de software; **no es una captura de Unity** y no representa reflejos, sombras ni visión nocturna dentro del motor.

## Abrir

Descomprime en una carpeta nueva. Abre con Unity 6.0.x y carga `Assets/Ecos/Scenes/Inicio.unity`. Para editar el hospital completo, usa `Ecos de Chuquicamata > Generar hospital editable`. Una escena editable antigua conserva mallas y materiales antiguos: haz una copia de tus cambios antes de regenerarla. No basta con reemplazar el ZIP sin abrir la carpeta nueva.

El proyecto sigue usando Built-in; no instalar URP/HDRP para esta entrega. No necesita compras de assets. Los controles de cámara y las partidas guardadas siguen siendo los de la actualización de visión nocturna.

## Validación

Pasaron la sintaxis C# mediante parser, integridad de las 42 mallas, normales unitarias, índices, coordenadas UV sin triángulos degenerados, selección de materiales, tamaño/importación lineal de mapas técnicos y controles de progresión anteriores.

No se ejecutó ni compiló en Unity. Quedan pendientes compilación de shaders y C#, playtest completo, reflejos e iluminación a corta distancia, consumo de memoria, FPS y build Windows. Las comprobaciones estáticas no sustituyen estas pruebas.
