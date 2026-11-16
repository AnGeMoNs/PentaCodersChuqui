# Revisión 6: cámara, pilas y oscuridad

Se elimina la linterna del jugador y la tecla F. La cámara es la única fuente portátil de iluminación: clic derecho la levanta, Q alterna visión nocturna y mantener clic izquierdo acerca el zoom.

## Pilas

E recoge las pilas y las guarda como repuestos, incluso con carga completa. R consume un repuesto y repone la carga al 100%; la carga anterior se descarta. Con carga completa no se consume un repuesto. Sin repuestos se muestra un aviso. Hay ocho pilas repartidas por las salas, una por zona.

Solo la visión nocturna encendida gasta carga: 0,35 puntos porcentuales por segundo, aproximadamente 4 minutos y 46 segundos por pila a uso continuo. Bajar la cámara o apagar la visión nocturna conserva la carga. Los paneles y la pausa detienen el consumo.

Llegar a cero apaga la visión nocturna y muestra un aviso para recargar; ya no provoca una derrota automática. Puedes seguir moviéndote y recoger pilas. La captura por Sebastián sigue siendo una condición de derrota. La recarga es instantánea; no se ha añadido una animación de manos.

El HUD muestra carga y cantidad de repuestos. Los repuestos y los pickups consumidos se guardan en el checkpoint; se permiten guardados con carga cero. Las partidas anteriores continúan con cero repuestos y conservan sus pilas aún no recogidas.

## Iluminación y sombras

- Iluminación ambiental reducida a 0,008–0,045 según la opción Brillo.
- Luz de luna reducida a 0,025; niebla oscura para evitar que ilumine visualmente el fondo.
- Luces fijas con 22% de su intensidad anterior y 65% del alcance anterior, dejando sectores oscuros entre luces.
- Sombras suaves en luces fijas, luna y emisor de visión nocturna. Distancia global de sombras: 40 metros; resolución alta.
- Se conservan materiales con iluminación Standard y los mapas de normales ya incluidos. Las sombras se calculan sobre las superficies en tiempo real, no se pintan sobre las texturas.

**No se implementó ray tracing.** El proyecto continúa en Built-in con sombras rasterizadas. Estas no incluyen iluminación global por ray tracing, reflejos trazados, SSAO nuevo ni horneado de luz. La cantidad de luces que producen sombras simultáneamente depende del renderizado y del equipo; no se garantiza sombra de cada luz en cada superficie. El coste de las sombras y la legibilidad en zonas oscuras requieren medición y playtest en Unity.

## Instalar y probar

Abrir el ZIP en una carpeta nueva con Unity 6.0.x Built-in, cargar Assets/Ecos/Scenes/Inicio.unity. Si usas HospitalEditable, respalda tus modificaciones y regenera la escena para eliminar la linterna antigua y actualizar luces.

Probar: F no hace nada; Q requiere cámara levantada; la visión nocturna ilumina y produce sombras; recoger una pila no modifica la carga; R gasta un repuesto y recarga; repetir R con carga completa no gasta otro; al llegar a cero puedes caminar y recargar; guardar y continuar conserva repuestos. El menú Pruebas en Play > Bateria 0 - probar recarga permite comprobar el estado descargado.

Pasaron los controles estáticos de sintaxis, recursos, mallas, materiales, audio y progresión. No se ejecutó Unity: pendientes compilación, apariencia final de sombras, consumo real de GPU y recorrido completo con esta oscuridad. Los documentos de revisiones anteriores conservan sus descripciones históricas; esta revisión reemplaza sus reglas de linterna y derrota por batería.
