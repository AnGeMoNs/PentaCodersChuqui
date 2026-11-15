using UnityEngine;
namespace Ecos
{
    [RequireComponent(typeof(Camera))]
    public class NightVisionEffect : MonoBehaviour
    {
        public PlayerRig owner;
        Material material;
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(owner==null){Graphics.Blit(source,destination);return;}
            if(material==null)
            {
                Shader shader=Resources.Load<Shader>("Shaders/NightVision");
                if(shader==null||!shader.isSupported){Graphics.Blit(source,destination);return;}
                material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
            }
            GameSession g=GameSession.Instance;
            material.SetFloat("_NightVisionOn",owner.NightVisionActive?1f:0f);
            material.SetFloat("_Brightness",g!=null?g.Brightness:.62f);
            material.SetFloat("_NoiseTime",Time.unscaledTime);
            material.SetFloat("_NoiseAmount",g!=null?g.NoiseAmount:1f);
            material.SetFloat("_Interference",g!=null&&g.Enemy!=null?g.Enemy.InterferenceLevel:0f);
            Graphics.Blit(source,destination,material);
        }
        void OnDisable(){if(material!=null){Destroy(material);material=null;}}
    }
}
