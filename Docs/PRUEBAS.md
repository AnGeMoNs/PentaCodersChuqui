# Validación de la actualización Visión Nocturna

## Comprobaciones automatizadas fuera de Unity

Tests/verify_project.py revisa sintaxis C#, índices/normales/caras de las 42 mallas, referencias de recursos, escenas, paquetes integrados, picos individuales de los 16 WAV, conectividad de 328 celdas y progresión de las ocho zonas. También comprueba que PhotoEcho y Spectral ya no se usan, que existen los tres objetos interactivos de sustitución, que el postprocesado y el iluminador están conectados y que está presente la migración v1 → v2.

Estas comprobaciones no compilan contra Unity ni ejecutan el shader. La inspección de recursos no demuestra que la partida sea jugable de principio a fin.

## Pruebas pendientes en Unity

1. Importar en Unity 6.0 Built-in y confirmar consola sin errores. Abrir Inicio y jugar.
2. Clic derecho levanta cámara; Q alterna visión nocturna; clic derecho la baja. Comprobar los tres estados: guardada, cámara color y cámara nocturna.
3. Con F apagada y una sala oscura, activar visión nocturna: debe iluminar la geometría frontal, teñir la vista de verde y mostrar grano/viñeta. Las paredes deben ocultar lo que hay detrás.
4. Mantener clic izquierdo produce zoom sin fotos, flash, sonidos de obturador, gasto instantáneo ni cambios de ira.
5. Medir batería con visión nocturna durante 60 s: descenso de 10,8 puntos, salvo redondeo del HUD. Bajando cámara con linterna apagada no debe gastar. Levantada en color consume 2,1 puntos/minuto.
6. Abrir nota/inventario/pausa: no debe gastar batería; al cerrar se recupera el estado de la cámara. La pausa detiene el audio.
7. Sin usar cámara, completar las tres interacciones: cuadro → llave infantil; Rayos X → registros; armario → llave morgue. Repetir no duplica recompensas.
8. Completar juguetes 1–3–2, código 1985 y generador ON/OFF/ON. Probar errores y reintentos.
9. Abrir puertas con llave consumida; salir y continuar para comprobar persistencia.
10. Final Juntos con registros y expediente conservado; El silencio sin pruebas o al callar.
11. Comprobar que camillas, bancos, casilleros y cámaras de conservación no bloqueen entradas, pistas ni la salida final.
12. Recorrer cada zona de día/editor y con visión nocturna para revisar materiales, sombras, mobiliario y colisiones.
13. Probar Sebastián con los menús de ira 15, 50 y 85; confirmar captura en persecución.
14. Probar derrota por batería cero y recuperación desde checkpoint.
15. Cargar un checkpoint de la versión anterior: conservar llaves consumidas, puertas, pruebas y decisiones; no volver a otorgar pistas ya obtenidas.
16. Generar HospitalEditable de nuevo. Guardar/cerrar/reabrir y comprobar que no falten componentes ni materiales. No reutilizar escenas antiguas sin regenerarlas.
17. Compilar Windows x64, verificar que el shader de visión nocturna esté incluido y hacer un recorrido completo.
18. Medir FPS, tiempos de guardado/carga, mezcla final y legibilidad 1080p. No hay resultados medidos de estas pruebas en esta entrega.

Referencia del postprocesado Built-in:
https://docs.unity3d.com/6000.0/ScriptReference/MonoBehaviour.OnRenderImage.html


## Materiales y geometría
Ejecutar `python Tests/verify_materials.py`. Verifica UVs, regiones de material y atlas. Se requiere numpy y Pillow. Los shaders y el rendimiento requieren verificación en Unity; no se han probado aquí.
