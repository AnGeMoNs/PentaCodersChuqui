# Revisión 9 · Sebastián

En los encuentros tranquilos, Sebastián inclina la cabeza hacia el jugador y emite un llanto infantil entrecortado. Al pasar a la persecución, gruñe, acelera y acerca los brazos. La mandíbula se mueve mientras suena su voz. Los pasos se disparan cuando cada pie entra en su fase de apoyo. La voz acompaña al personaje y se atenúan sus agudos cuando hay un obstáculo entre él y el jugador. Puedes regularla en Opciones, en **Gritos y sonidos de terror**.

La malla incorpora ojos hundidos, surcos oscuros en las mejillas, dedos largos y una bata con pliegues irregulares. El rig tiene 17 huesos y admite dos influencias por vértice. El generador conserva las referencias de huesos y material en HospitalEditable y en el prefab de Sebastián. Los WAV del llanto, el gruñido, la respiración y las pisadas son de síntesis original. En `Assets/Ecos/Resources/Audio` llevan el prefijo `Sebastian`.

## Probar el encuentro

1. Abre el proyecto en Unity 6.0 desde una carpeta nueva. Entra en Play desde `Assets/Ecos/Scenes/Inicio.unity` y llega a recepción.
2. Mira hacia un tramo despejado del pasillo. En **Ecos de Chuquicamata → Pruebas en Play**, selecciona **Aparicion visible**. Escucha el llanto y observa cómo mueve la cabeza y la mandíbula.
3. En el mismo menú, selecciona **Persecucion de Sebastian**. La prueba sube la ira a 85. Camina y cambia de dirección para revisar cómo se acerca. **Llanto de Sebastian** permite escuchar su voz por separado.
4. Si trabajas con HospitalEditable, respalda tus ediciones, regenera el hospital y vuelve a abrir la escena. Usa **Ecos de Chuquicamata → Validacion → Rig de Sebastian** para comprobar las referencias.

## Verificación

`Tests/verify_sebastian.py` comprueba la jerarquía, los pesos de los vértices, la reconstrucción de la pose de reposo y 25 poses de referencia de la carrera. También comprueba los cuatro WAV. `Docs/Sebastian_Revision9.png` muestra tres poses renderizadas fuera del editor. La compilación en Unity, la escucha espacial y el ajuste visual durante una partida siguen pendientes.

El modelo se reconstruye con `SourceArt/realistic_props.py`, que llama al generador del personaje. `SourceArt/sebastian_audio.py` reconstruye sus cuatro sonidos.
