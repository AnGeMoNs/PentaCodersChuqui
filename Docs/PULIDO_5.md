# Revisión 5: presentación, sonido y comodidad

Mantiene las mecánicas, acertijos, guardados, 54 modelos y diez superficies existentes.

## Cambios implementados

- HUD con barras de batería y resistencia, indicador numérico de batería y color de advertencia bajo el 20%.
- HUD discreto opcional: oculta el objetivo permanente, la ira y la banda inferior. La batería aparece con la cámara levantada o si está baja; la resistencia aparece mientras está por debajo del 98%. Los mensajes de interacción y las advertencias se conservan.
- Ayuda con H: controles, explicación de cámara, consejo para escapar y objetivo actual. Detiene la simulación igual que el diario; H o el botón cierran la ayuda.
- Campo de visión ajustable entre 60° y 95°. Solo afecta a la vista normal; los encuadres de cámara y zoom se conservan.
- Intensidad del grano y las líneas nocturnas entre 0 y 1. Reducirlos a cero mantiene la iluminación y el color de visión nocturna.
- Las tres preferencias nuevas se conservan en PlayerPrefs junto a volumen, brillo y sensibilidad. El menú de opciones incluye desplazamiento vertical.
- Puertas con desplazamiento suave durante 1,25 segundos. La colisión sigue activa hasta terminar; al pausar también se detiene la animación. La llave se consume una vez. Cargar una puerta ya desbloqueada la coloca completamente abierta.
- Sonido original de arrastre mecánico al abrir puertas, emitido desde la puerta.
- Sonidos de los juguetes emitidos desde su posición.
- Golpes ambientales originales de tuberías en salas laterales: a partir del ala infantil, solo durante exploración sin enemigo activo, con intervalos de 20 a 35 segundos después del primer intervalo. No dañan al jugador ni cambian la ira.
- Seis emisores de sonido reutilizables; atenuación lineal hasta 22 metros. Sonido posicional básico, sin simulación de oclusión por paredes ni reverberación por sala. No se afirma audio binaural.
- Transición ambiental que parte del volumen real de la fuente anterior, evitando saltos al cruzar zonas con rapidez.
- Al empezar, continuar o volver al menú se detienen efectos transitorios de la partida anterior.

## Verificaciones realizadas

Parser de sintaxis sobre 17 archivos C#, integridad de las 54 mallas y UV/materiales, atlas y espacios de color, controles de progresión lógica de ocho zonas y 18 WAV con picos individuales por debajo de -6 dBFS. También se comprobó la integridad del ZIP. Los resultados están en RESULTADOS_ESTATICOS.txt.

No se ejecutó Unity en este entorno: siguen pendientes compilación C#/shaders, reproducción y mezcla real de audio, comportamiento de puertas, uso del menú con distintas resoluciones, FPS y recorrido completo. Las verificaciones estáticas no equivalen a un playtest.

## Probar esta revisión

1. Descomprimir en una carpeta nueva y abrir Inicio.unity con Unity 6.0.x Built-in.
2. Ejecutar el menú Ecos de Chuquicamata > Validacion > Recursos y guardado.
3. En Opciones, variar campo de visión y ruido nocturno; activar HUD discreto. Guardar, cerrar y volver a abrir para confirmar persistencia.
4. En una partida, presionar H; comprobar objetivo y controles, y volver con H.
5. Abrir una puerta con llave, comprobar sonido y esperar su apertura. Probar pausa a mitad del recorrido y después continuar.
6. Escuchar los juguetes desde ambos lados. Los auriculares ayudan a comprobar la localización izquierda/derecha, pero no son obligatorios.
7. Explorar desde la zona infantil para escuchar golpes ocasionales. Comprobar que no se generan golpes nuevos en pausa o durante una aparición activa.
8. Verificar que nueva partida y continuar conservan el reinicio correcto de cámara y postura de la revisión anterior.

Si usabas HospitalEditable, respalda tus cambios antes de regenerarlo. Los menús de validación están incluidos pero no se ejecutaron aquí.

## Fuentes de audio

Los dos WAV nuevos, DoorSlide y PipeKnock, se sintetizan de forma original con SourceArt/polish_audio.py (Python + numpy). No utilizan grabaciones de otros juegos. Ejecuta ese script después del generador general si reconstruyes los sonidos desde cero. La normalización individual no garantiza ausencia de saturación al mezclar muchas fuentes en tiempo real.
