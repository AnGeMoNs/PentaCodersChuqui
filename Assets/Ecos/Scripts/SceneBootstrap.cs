using System.Collections;
using UnityEngine;
namespace Ecos
{
    public class SceneBootstrap : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null;
            if(FindFirstObjectByType<GameSession>()==null)LevelBuilder.Build();
            yield return null;
            Destroy(gameObject);
        }
        void OnGUI()
        {
            GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
            GUI.Label(new Rect(Screen.width*.35f,Screen.height*.45f,500,80),"ECOS DE CHUQUICAMATA\nPreparando el hospital…");
        }
    }
}
