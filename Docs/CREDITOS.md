# Créditos y procedencia

- Concepto, historia base, personajes y requisitos: ERS v1.0 de PENTACODERS proporcionado por el usuario. Equipo indicado en el documento: Angelo Herrera, Kevin Arancibia, Gabriel Jara y Marina Rojas.
- Esta implementación de código, distribución del nivel, modelos y audio procedural se generó para el usuario con asistencia de IA. No debe atribuirse su implementación a los integrantes sin que efectivamente la revisen o modifiquen.
- Modelos: 54 recursos originales construidos con geometría matemática; generadores base y extendido en Python y exportación OBJ/MTL incluidos. No utilizan assets de la Asset Store.
- Audio actual: 29 efectos y pistas sintetizados originales, más siete voces de Sebastián basadas en tres grabaciones externas CC0. Autores, licencias y modificaciones en SourceArt/AudioSources/CREDITS.md.
- Ilustraciones: un atlas de seis paneles creado con la herramienta integrada de generación de imágenes. El prompt se conserva en SourceArt/illustration_prompt.txt. No son fotografías históricas.
- Render de documentación: Docs/Modelos_3D.png utiliza las mallas JSON del proyecto. No representa una ejecución en Unity.
- Unity es el motor de terceros y requiere una licencia válida según sus condiciones aplicables. El paquete no redistribuye el editor ni sus bibliotecas.

Puedes usar y modificar el código, las mallas y el audio creados para esta entrega en tu proyecto. Los recursos originales de código, geometría y audio sintetizado se entregan bajo MIT; consulta LICENSE_CODE_ASSETS.txt. Las grabaciones vocales de terceros conservan su procedencia CC0. El atlas generado con IA se incluye para uso en el proyecto bajo los términos aplicables a su generación; no se promete exclusividad ni registro de derechos de autor sobre esa imagen. Los derechos sobre el ERS original no se alteran.


## Revisión visual
Seis texturas originales generadas mediante image_gen integrado; prompts y resoluciones reales en SourceArt/TextureSources/PROMPTS.txt. Mallas revisadas y mapas derivados por código original. No se extrajeron recursos de juegos comerciales. Ver CAMBIOS_MATERIALES_Y_MODELOS.md para alcance y limitaciones.

Revisión 3: doce modelos originales adicionales y cuatro superficies generadas con image_gen integrado. Prompts en SourceArt/TextureSources/PROMPTS.txt. Vista de inspección: Objetos_Revision3.png.

Revisión 5: DoorSlide.wav y PipeKnock.wav son sonidos originales sintetizados con SourceArt/polish_audio.py.


## Revisión 10
Dos texturas originales generadas para rostro y vestuario de Sebastián, con resolución nativa 1254 × 1254 y mapas de microrelieve derivados. Prompts en SourceArt/SebastianTextures/PROMPTS.txt. Ninguna imagen de referencia del usuario se redistribuye dentro del proyecto. Las voces se basan en grabaciones de onehugeeye, gpag1 y djani00 en Freesound, bajo CC0.


## Revisión 11
Tres capas musicales sincronizadas y tres efectos originales de detección, movimiento e impacto. Se generan con SourceArt/chase_music.py mediante osciladores, ruido filtrado y resonancias. No se utilizan muestras musicales externas. El video de inspección utiliza las mallas y los datos de animación reales del proyecto, renderizados con OpenGL fuera de Unity.
