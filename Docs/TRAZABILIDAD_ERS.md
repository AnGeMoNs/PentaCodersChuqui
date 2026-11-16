# Revisión autorizada por el usuario: Visión Nocturna

Esta actualización modifica expresamente el ERS original: la cámara deja de revelar ecos y de tomar fotografías. Se utiliza exclusivamente como ayuda visual con visión nocturna y zoom. Los recuerdos ilustrados siguen siendo eventos narrativos activados por interacción o hitos.

| Sistema | Estado de implementación fuente |
|---|---|
| RF-01 menú | Conservado |
| RF-02 controles y resistencia | Conservados |
| RF-03 linterna | Conservada; se suspende mientras funciona visión nocturna |
| RF-04 pilas | Conservadas, +25% hasta 100% |
| RF-05 interacción | E a 2 m; ahora también obtiene las tres pistas antes fotográficas |
| RF-06/07 inventario y diario | Conservados; textos actualizados |
| RF-08 cámara fotográfica | Sustituido por cámara con iluminación nocturna y zoom, según petición del usuario |
| RF-09/12 recuerdos | Conservados como narrativa, sin detección espectral |
| RF-10/11 puertas y código | Conservados; las llaves se obtienen físicamente |
| RF-13 batería cero | Conservado; gasto continuo de visión nocturna |
| RF-14 Sebastián e ira | Conservado; la cámara ya no modifica la ira |
| RF-15 audio | Conservado |
| RF-16 checkpoints | Esquema v2 y migración de descubrimientos v1 |
| RF-17/18 opciones y pausa | Conservadas |
| RF-19 finales | Conservados; el final completo requiere registros recogidos, no fotografías |
| RF-20 captura | Conservado |

## Sustitución de los bloqueos de progresión

- Cuadro de Recepción: E recupera la llave infantil una sola vez.
- Rayos X de Urgencias: E recoge los registros originales una sola vez.
- Armario de Anatomía Patológica: E recupera la llave de la morgue una sola vez.
- El guardado convierte las banderas antiguas echo_frame, echo_xray y echo_seal en clue_frame, clue_xray y clue_seal. No recrea llaves ya consumidas.

## Ampliación artística

De 24 a 42 modelos originales, 18 adicionales distribuidos por zonas. Nuevos shaders de arquitectura envejecida y postprocesado de visión nocturna. No se han utilizado modelos ni texturas de Outlast: la referencia se limita al funcionamiento de una videocámara para orientarse en la oscuridad.

Los RF representan implementación en código, no aceptación validada. Todos los requisitos de rendimiento, estabilidad, tiempos, mezcla de audio y usabilidad necesitan mediciones dentro de Unity. El nivel sigue siendo una representación ficticia horizontal de ocho sectores, no una reconstrucción arquitectónica del hospital real.
