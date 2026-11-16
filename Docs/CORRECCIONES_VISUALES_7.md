# Revisión 7: errores de texturas y objetos

Se conserva la revisión 6: sin linterna, visión nocturna con pilas, recarga con R y escenas oscuras.

## Cambios

- **Ventana rota:** dos triángulos del cristal tenían orientación opuesta a sus normales. La causa era una triangulación en abanico de un polígono cóncavo. Cada fragmento se divide ahora por su diagonal interior. Se actualizan el JSON de Unity, el OBJ y el generador original.
- **Paredes y pisos:** el shader calculaba el filtrado sobre coordenadas que saltaban al repetir la textura. Ahora usa derivadas de las coordenadas continuas, evitando esa causa de líneas de mipmapping en las uniones. Se utiliza además la máscara existente de rugosidad inversa (suavidad), metal y oclusión en lugar de valores uniformes. Esto no convierte las fuentes en texturas perfectamente continuas ni añade ray tracing.
- **Reconstrucción de materiales:** el generador principal recorría también los atlas adicionales y podía sobrescribir PropsAlbedo como textura lineal y cambiar su tamaño máximo. Ahora solo actualiza sus tres archivos propios; una prueba ejecuta la reconstrucción en una carpeta temporal y comprueba que los archivos adicionales permanezcan idénticos.
- **Videocámara en primera persona:** su modelo deja de proyectar sombras para evitar que tape su propio iluminador infrarrojo. Los objetos del escenario siguen proyectando sombras según la configuración del renderer.

- **Escena editable:** la exportación ahora incluye renderers y mallas de objetos desactivados (videocámara y Sebastián). Se actualizan también los prefabs de modelos no colocados en la escena, y se marcan los recursos modificados para guardarlos. Antes podían quedar referencias temporales o prefabs obsoletos.

## Validación

Pruebas estáticas: sintaxis C#, recursos, guardado/progresión revisados por el verificador existente, 54 modelos y 178972 triángulos, UV, orientación geométrica frente a normales, dimensiones y espacios de color de atlas, reconstrucción aislada de materiales e integridad del ZIP.

No se ha ejecutado el editor de Unity. No se garantiza ausencia de todos los bugs; falta compilar shaders y comprobar iluminación, colisiones, accesos y rendimiento en Play.

## Aplicar

1. Abre el ZIP en una carpeta nueva con Unity 6.0 y Built-in Render Pipeline.
2. Abre `Assets/Ecos/Scenes/Inicio.unity` y entra en Play.
3. Si usas `HospitalEditable`, respalda tus modificaciones y vuelve a generar la escena desde el menú Ecos de Chuquicamata. Una escena generada antes puede conservar mallas y ajustes antiguos.
4. Ejecuta `Ecos de Chuquicamata > Validacion > Recursos y guardado` y comprueba Console.
5. Revisa las ventanas de las primeras zonas desde ambos ángulos y las uniones de baldosas/paredes mientras caminas, con Q activado y desactivado. El cristal sigue siendo material opaco, como en la versión anterior.
6. Genera HospitalEditable, cierra y vuelve a abrir la escena. Confirma que la videocámara y Sebastián conserven sus mallas y materiales; repite la generación para comprobar la actualización.
7. Levanta la videocámara, activa Q y comprueba que el iluminador no dibuje la silueta de la propia cámara sobre la pared. Verifica también E para recoger pilas y R para recargar.

Si persiste un error visual concreto, una captura junto con la escena/zona y el primer error rojo de Console permite localizarlo con precisión.
