using UnityEngine;
namespace Ecos
{
    public class DoorGate : MonoBehaviour
    {
        public int zone;public string key;public Transform leaf;public BoxCollider barrier;
        bool opening;float progress;
        public bool IsOpening{get{return opening;}}
        public void Refresh()
        {
            opening=false;bool open=GameSession.Instance.Has("gate_"+zone);progress=open?1:0;
            barrier.enabled=!open;leaf.localPosition=new Vector3(open?5:0,0,0);
            Interactable item=GetComponent<Interactable>();if(item!=null)item.label=open?"Paso abierto":"Abrir acceso: "+StoryData.Zones[zone];
        }
        public void ForceUnlockAndAnimate()
        {
            GameSession g=GameSession.Instance;if(g==null||g.Has("gate_"+zone)||opening)return;
            if(!string.IsNullOrEmpty(key))g.Data.inventory.Remove(key);
            g.Flag("gate_"+zone);opening=true;progress=0;
            Interactable item=GetComponent<Interactable>();if(item!=null)item.label="Abriendo acceso…";
            g.Audio.Play("DoorUnlockRE",.42f);
            g.Audio.PlayAt("DoorSlide",transform.position+Vector3.up,.42f);
        }
        public void Open()
        {
            GameSession g=GameSession.Instance;if(g.Has("gate_"+zone)||opening)return;
            if(!g.Data.inventory.Contains(key)){g.Audio.Play("Error",.35f);g.Notify("Está cerrada. Resuelve el acertijo del sector o busca "+StoryData.Get(key).title+".");return;}
            ForceUnlockAndAnimate();
            g.Notify("Acceso desbloqueado: "+StoryData.Zones[zone]);g.Checkpoint();
        }
        void Update()
        {
            GameSession g=GameSession.Instance;if(!opening||g==null)return;
            if(g.State!=SessionState.Playing)return;
            progress=Mathf.MoveTowards(progress,1,Time.unscaledDeltaTime/1.85f);
            leaf.localPosition=Vector3.right*(5*Mathf.SmoothStep(0,1,progress));
            if(progress>=1){opening=false;barrier.enabled=false;Interactable item=GetComponent<Interactable>();if(item!=null)item.label="Paso abierto";}
        }
    }
}
