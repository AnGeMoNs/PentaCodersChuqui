using System.Collections.Generic;
namespace Ecos
{
    public class Entry
    {
        public string id,title,description;public int icon;
        public Entry(string id,string title,string description,int icon=0){this.id=id;this.title=title;this.description=description;this.icon=icon;}
    }
    public static class StoryData
    {
        public static readonly string[] Zones={"ACCESO Y ENTRADA","RECEPCIÓN Y ADMINISTRACIÓN","NEONATOLOGÍA / ÁREA INFANTIL","TRAUMATOLOGÍA Y URGENCIAS","DIAGNÓSTICO Y ARCHIVOS","SUBTERRÁNEO G","ANATOMÍA PATOLÓGICA","MORGUE"};
        public static readonly string[] Chapters={"I · EL HOSPITAL","I · EL HOSPITAL","II · EL NIÑO","III · LA VERDAD","III · LA VERDAD","IV · SEBASTIÁN","IV · SEBASTIÁN","V · JUNTOS"};
        public static readonly Dictionary<string,Entry> Entries=new Dictionary<string,Entry>
        {
            {"welcome",new Entry("welcome","La grabación de Tomás","Soy Tomás. Entré para grabar la historia de Sebastián. El hospital está abandonado, pero todavía escucho pasos.\n\nWASD: caminar. Shift: correr. Ctrl: agacharte en sigilo. R: recargar cámara. E: interactuar o esconderte en casilleros a menos de dos metros. Tab: inventario y diario.\n\nClic derecho sube o baja la videocámara. Q activa o desactiva la visión nocturna cuando está levantada. Mantén clic izquierdo para acercar el zoom. La visión nocturna consume batería mientras está encendida.\n\nEn Recepción, busca el cuadro y revísalo con E. Si escuchas a Sebastián o ves parpadear las luces, apaga la cámara o escóndete en un casillero antes de que te vea.",4)},
            {"reception",new Entry("reception","Registro administrativo","1985. Se cerró el ala infantil después del incidente. La enfermera escondió la llave detrás del cuadro de Recepción.\n\nLa llave sigue escondida detrás del marco. Acércate al cuadro y revísalo con E.")},
            {"infant_key",new Entry("infant_key","Llave del ala infantil","Una llave recuperada detrás del cuadro. Abre la puerta entre Recepción y Neonatología.",1)},
            {"drawing",new Entry("drawing","El dibujo de Sebastián","Tres juguetes me esperan: primero el ROJO, después el AZUL y al final el AMARILLO. Mamá decía que ese era el camino del sol, del cielo y de la luz.\n\nActiva los tres juguetes en ese orden armónico. Cada juguete girará al escuchar su nota; si te equivocas, el ruido alertará al hospital.",2)},
            {"urgency_key",new Entry("urgency_key","Llave de Urgencias","Apareció en el cajón de los juguetes. Permite continuar hacia Traumatología.",1)},
            {"record19",new Entry("record19","Ingreso 19","El número de ingreso de Sebastián era 19. En el archivador del área de Diagnóstico se guarda el expediente secreto.\n\nLa combinación concatena el número de ingreso de dos cifras y las dos últimas cifras del año del incidente.")},
            {"record85",new Entry("record85","La fecha tachada","El informe original fue fechado en 1985. Una anotación posterior cambió la hora de muerte. El registro de traslado de su madre también fue alterado.\n\nJunto al equipo de Rayos X hay dos registros contradictorios. Acércate y recógelos con E.")},
            {"evidence",new Entry("evidence","Registros originales","Los documentos recogidos junto al equipo de Rayos X contienen dos registros contradictorios. Madre e hijo fueron separados y sus documentos ocultados.",3)},
            {"secret",new Entry("secret","Expediente secreto","Sebastián no desapareció. Su cuerpo fue trasladado a la cámara A1; su madre, a la C2. Un informe falsificado ocultó ambos traslados.\n\nLa morgue conserva dos cuerpos separados. Reunirlos puede poner fin al eco. Esta es una historia de ficción.")},
            {"basement_key",new Entry("basement_key","Llave del subterráneo","Abre el acceso a mantenimiento del subterráneo G.",1)},
            {"fuse",new Entry("fuse","Fusible de respaldo","Instálalo en el generador antes de configurar los interruptores.",5)},
            {"generator_manual",new Entry("generator_manual","Circuito de emergencia","Inserta el fusible y opcionalmente purga la válvula de vapor en la sala derecha para reducir la ira del entorno. Después configura las fases A, B y C:\n\nFase A debe estar encendida (ON).\nFase B debe estar apagada (OFF) para evitar la sobrecarga.\nFase C debe tener el mismo estado que la Fase A (ON).\n\nVerifica el circuito para abrir el acceso a Anatomía Patológica.")},
            {"pathology_key",new Entry("pathology_key","Llave de Anatomía Patológica","Desbloquea el acceso después de restaurar el generador.",1)},
            {"morgue_record",new Entry("morgue_record","Registro de conservación","A1: Sebastián. C2: su madre.\n\nLa llave de la morgue está en el cajón sellado del armario de Anatomía Patológica. Revísalo con E.\n\nEn la morgue encontrarás dos salas. Identifica ambos cuerpos y traslada la camilla de la madre junto a la de Sebastián. Después podrás elegir cómo cerrar esta historia.")},
            {"morgue_key",new Entry("morgue_key","Llave de la morgue","Una llave recuperada del cajón de conservación. Abre el acceso a las dos salas de conservación.",1)},
            {"camera",new Entry("camera","Videocámara de Tomás","Clic derecho: subir o bajar la cámara. Q: activar visión nocturna. Mantener clic izquierdo: zoom. La cámara sufre interferencias electromagnéticas cuando Sebastián está cerca.",3)},
            {"recorder",new Entry("recorder","Grabadora","Contiene la anotación inicial y el diario de investigación. Los recuerdos usan música y texto; no incluyen doblaje.",4)}
        };
        public static Entry Get(string id){Entry e;return Entries.TryGetValue(id,out e)?e:new Entry(id,id,"");}
    }
}
