# Revisión 4: pulido de la experiencia

Mantiene los 54 modelos, los diez materiales, las zonas, acertijos y visión nocturna. Esta revisión se centra en control, claridad y continuidad de la partida.

## Cambios

- Movimiento con aceleración de 18 m/s² y frenado de 26 m/s². Se conservan las velocidades máximas. Al pausar se borra la velocidad acumulada para evitar deslizarse al volver.
- Los pasos dependen de la distancia recorrida y del contacto con el suelo. Empujar una pared sin desplazarse deja de generar pasos.
- Sensibilidad de mirada al 78% al levantar la cámara y al 52% al hacer zoom. No se añade balanceo de cabeza ni temblor artificial de cámara.
- Asistencia estrecha de interacción: primero se usa el rayo central; si no encuentra un objeto, se prueban cuatro rayos cercanos al centro. Cada rayo respeta su primer collider y mantiene alcance de dos metros; no atraviesa paredes. Requiere prueba manual en esquinas y con objetos pequeños.
- Avisos de batería a 20%, 10% y 5%, sin repetirse cada fotograma; se rearman al recargar por encima de 25%.
- Mensajes de objetos revisados, secuencias completadas y documentos ya leídos.
- Objetivo adaptado a las llaves y puertas concretas, al fusible y a cada paso final.
- El aviso de checkpoint tiene su propia ubicación y ya no sobrescribe el mensaje de una pista. Solo aparece si la escritura informa éxito.
- Confirmación dentro del menú antes de reemplazar una partida al comenzar otra nueva.
- El diario elimina una selección anterior que no pertenece a la partida actual, evitando mostrar entradas desbloqueadas en una partida previa.
- El checkpoint previo válido se conserva en una clave de respaldo. Si el principal no se puede leer o validar, se intenta el respaldo. Esto protege ante corrupción individual, pero no ante borrado completo de PlayerPrefs, pérdida de disco o todos los fallos de escritura.
- Las apariciones de Sebastián buscan un punto despejado y conectado en la zona, entre 6 y 15 metros. Prefieren posiciones ocultas por colliders; si no hay ninguna posición válida, reintentan tras cinco segundos.
- Cambiar de zona no reubica una aparición activa. Al aparecer hay 1,5 segundos sin movimiento ni captura.
- Alcance visual de 15 metros, reducido a 9 metros si el jugador está agachado. Correr conserva el ruido que permite al enemigo localizarlo a menos de 11 metros. Agacharse no garantiza invisibilidad a corta distancia.
- Una persecución que mantiene contacto visual no termina por el temporizador inicial. Romper contacto visual y dejar de correr permite que se agote la memoria del enemigo.
- La búsqueda de rutas queda limitada a una vez cada 0,25 segundos al alcanzar o bloquearse en un punto, reduciendo intentos repetidos por fotograma.

## Verificación

Ejecutadas aquí: sintaxis de los 17 archivos C# mediante parser, integridad de 54 mallas y sus UV/materiales, mapas de texturas, audio individual y controles estáticos de progresión. Se conserva el ZIP íntegro. Ver RESULTADOS_ESTATICOS.txt.

No ejecutadas aquí: compilación C# y shaders en Unity, física, control con ratón, ambas rutas finales, reproducción de audio y medición de rendimiento. Unity no está instalado en el entorno de creación. Los parámetros de movimiento y persecución son decisiones iniciales de balance que requieren playtest; no se afirma que estén libres de errores.

## Prueba manual sugerida

1. Abrir Inicio.unity en Unity 6.0.x Built-in desde una carpeta nueva.
2. Ejecutar Ecos de Chuquicamata > Validacion > Recursos y guardado.
3. Caminar, frenar, correr contra una pared y agacharse; comprobar postura y pasos.
4. Levantar cámara, alternar visión nocturna y usar zoom. Comprobar sensibilidad y consumo.
5. Interactuar con documentos y llaves desde el centro y bordes de la mira; comprobar que una pared los bloquea.
6. Resolver cada acertijo y comprobar que el objetivo indica la siguiente puerta. Confirmar que el aviso de guardado no borra la pista.
7. Empezar otra partida, cancelar la confirmación y verificar que continúa el checkpoint anterior. Si se confirma, comprobar que el diario no muestra la selección de la partida vieja.
8. En la zona infantil o posterior, usar el menú de prueba de ira 85. Comprobar distancia de aparición, pausa inicial, recorrido entre muebles, pérdida de contacto al esconderse y ausencia de teletransporte al cambiar de zona.
9. Ejecutar Validacion > Acceso a objetos en Play y revisar los avisos manualmente.

Los menús de validación de Unity están incluidos, pero no se ejecutaron aquí. Guarda una copia de HospitalEditable antes de regenerarlo si lo modificaste.
