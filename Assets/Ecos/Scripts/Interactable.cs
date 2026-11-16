using UnityEngine;
namespace Ecos
{
    public class Interactable : MonoBehaviour
    {
        public string id,label,kind,entry,requiredFlag;
        public int number;
        public string Prompt
        {
            get
            {
                GameSession g=GameSession.Instance;if(g==null)return label;
                if(kind=="locker")return g.Player!=null&&g.Player.IsHiding&&!g.Player.HidingUnderBed?"Salir del casillero":"Esconderse en el casillero (Outlast)";
                if(kind=="bed_hide")return g.Player!=null&&g.Player.IsHiding&&g.Player.HidingUnderBed?"Salir de debajo de la cama":"Esconderse bajo la cama (Outlast)";
                if(kind=="valve")return g.Has("steam_purged")?"Válvula de vapor estabilizada":"Purgar presión de vapor de la línea";
                if(kind=="clue"&&g.Has("clue_"+id))return "Objeto revisado";
                if(kind=="toy"&&g.Has("toys"))return "Secuencia completada";
                if(kind=="generator"&&g.Has("power"))return "Generador en funcionamiento";
                if(kind=="mother"&&g.Has("together"))return "Las camillas ya están reunidas";
                if(kind=="document"&&g.Data.journal.Contains(entry))return "Volver a leer: "+label;
                return label;
            }
        }
        public void Use(){if(GameSession.Instance!=null)GameSession.Instance.Interact(this);}
        public bool Available {get{return string.IsNullOrEmpty(requiredFlag)||GameSession.Instance.Has(requiredFlag);}}
    }
}
