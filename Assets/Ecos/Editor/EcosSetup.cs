using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Ecos.Editor
{
    public static class EcosSetup
    {
        const string ScenePath="Assets/Ecos/Scenes/HospitalEditable.unity";
        [MenuItem("Ecos de Chuquicamata/Generar hospital editable")]
        public static void Generate()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("Detén Play antes de generar el nivel.");return;}
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(File.Exists(ScenePath)&&!EditorUtility.DisplayDialog("Regenerar hospital","Esto reemplaza HospitalEditable.unity. Conserva una copia si ya lo editaste.","Regenerar","Cancelar"))return;
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            LevelBuilder.Build();Directory.CreateDirectory("Assets/Ecos/Generated/Meshes");Directory.CreateDirectory("Assets/Ecos/Generated/Materials");Directory.CreateDirectory("Assets/Ecos/Generated/Prefabs");AssetDatabase.Refresh();
            foreach(MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                Mesh mesh=filter.sharedMesh;if(mesh==null||AssetDatabase.Contains(mesh))continue;
                string path="Assets/Ecos/Generated/Meshes/"+mesh.name+".asset";Mesh existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing==null){AssetDatabase.CreateAsset(mesh,path);existing=mesh;}else {EditorUtility.CopySerialized(mesh,existing);EditorUtility.SetDirty(existing);}filter.sharedMesh=existing;
            }
            foreach(SkinnedMeshRenderer skin in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                Mesh mesh=skin.sharedMesh;if(mesh==null||AssetDatabase.Contains(mesh))continue;
                string path="Assets/Ecos/Generated/Meshes/"+mesh.name+".asset";Mesh existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing==null){AssetDatabase.CreateAsset(mesh,path);existing=mesh;}else{EditorUtility.CopySerialized(mesh,existing);EditorUtility.SetDirty(existing);}skin.sharedMesh=existing;
            }
            foreach(MeshCollider collider in Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {MeshFilter filter=collider.GetComponent<MeshFilter>();if(filter!=null)collider.sharedMesh=filter.sharedMesh;}
            foreach(Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(renderer.GetComponent<TextMesh>()!=null)continue;
                Material[] mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    Material mat=mats[i];if(mat==null||AssetDatabase.Contains(mat))continue;
                    string path="Assets/Ecos/Generated/Materials/"+mat.name.Replace("/","_")+".mat";Material existing=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(existing==null){AssetDatabase.CreateAsset(mat,path);existing=mat;}else {EditorUtility.CopySerialized(mat,existing);EditorUtility.SetDirty(existing);}mats[i]=existing;
                }
                renderer.sharedMaterials=mats;
            }
            foreach(string file in Directory.GetFiles("Assets/Ecos/Resources/Models","*.json"))
            {
                string name=Path.GetFileNameWithoutExtension(file);Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Ecos/Generated/Meshes/"+name+".asset");
                Mesh source=ModelLibrary.Load(name);
                if(mesh==null){mesh=source;AssetDatabase.CreateAsset(mesh,"Assets/Ecos/Generated/Meshes/"+name+".asset");}
                else if(mesh!=source){EditorUtility.CopySerialized(source,mesh);EditorUtility.SetDirty(mesh);}
                GameObject prefab;
                if(name=="Sebastian")
                {
                    SebastianRig rig=SebastianRig.Create(null,AssetDatabase.LoadAssetAtPath<Material>("Assets/Ecos/Generated/Materials/Sebastian_Presencia.mat"));rig.skin.sharedMesh=mesh;prefab=rig.gameObject;
                }
                else{prefab=new GameObject(name);prefab.AddComponent<MeshFilter>().sharedMesh=mesh;prefab.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Ecos/Generated/Materials/Ecos_Palette.mat");}
                PrefabUtility.SaveAsPrefabAsset(prefab,"Assets/Ecos/Generated/Prefabs/"+name+".prefab");Object.DestroyImmediate(prefab);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            Debug.Log("Hospital editable creado: mallas, materiales, prefabs y escena guardados. Pulsa Play.");
        }
        [MenuItem("Ecos de Chuquicamata/Abrir escena inicial")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene("Assets/Ecos/Scenes/Inicio.unity");
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Ira 15 - observar")]
        static void Observe(){SetAnger(15);}
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Ira 50 - hostigar")]
        static void Harass(){SetAnger(50);}
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Ira 85 - perseguir")]
        static void Chase(){SetAnger(85);}
        static void SetAnger(float value)
        {
            GameSession g=GameSession.Instance;
            if(!EditorApplication.isPlaying||g==null||!g.Playing||g.Data.zone<2){Debug.LogWarning("Inicia una partida y entra al area infantil o a una zona posterior antes de probar la ira.");return;}
            g.Data.anger=value;g.Enemy.Encounter();Debug.Log("Prueba de ira: "+value+". Este cambio no fuerza un guardado.");
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Bateria 0 - probar recarga")]
        static void EmptyBattery()
        {
            GameSession g=GameSession.Instance;if(EditorApplication.isPlaying&&g!=null&&g.Playing)g.Data.battery=0;
        }
        [MenuItem("Ecos de Chuquicamata/Build Windows 64 bits")]
        public static void BuildWindowsMenu(){BuildWindows();}
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Aparicion visible")]
        public static void TestGhost()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing||g.Data.zone<1){Debug.LogWarning("Entra en recepción y mira hacia un tramo libre del pasillo.");return;}
            g.Data.anger=15;g.Enemy.ResetActor();g.Enemy.Encounter();
            Debug.Log(g.Enemy.Active?"Aparición activada.":"No hay un punto visible y accesible a 6-15 m. Mira hacia el pasillo e inténtalo otra vez.");
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Persecucion de Sebastian")]
        public static void TestChase()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing||g.Data.zone<1)return;
            g.Data.anger=85;if(!g.Enemy.Active)g.Enemy.Encounter();
            Debug.Log("Ira 85: persecución activada si hay un punto de aparición válido. No fuerza un checkpoint.");
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Alternar ataques sin captura")]
        public static void PreviewAttacks()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing||g.Data.zone<1)return;
            g.Enemy.PreviewAttacks=!g.Enemy.PreviewAttacks;g.Data.anger=85;
            if(!g.Enemy.Active)g.Enemy.Encounter();
            Debug.Log(g.Enemy.PreviewAttacks?"Vista de ataques activada: sin captura, solo en Editor. Quédate cerca para ver las tres variantes.":"Captura normal restaurada.");
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Grito infantil de Sebastian")]
        public static void TestSob()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing)return;
            if(g.Enemy.Active)g.Audio.PlayEnemyVoice("SebastianScream",.6f);else g.Audio.PlayHorrorAt("SebastianScream",g.Player.transform.position+g.Player.view.transform.forward*4,.6f);
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Voz demoniaca de Sebastian")]
        public static void TestDemon()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing)return;
            if(g.Enemy.Active)g.Audio.PlayEnemyVoice("SebastianSnarl",.6f);else g.Audio.PlayHorrorAt("SebastianSnarl",g.Player.transform.position+g.Player.view.transform.forward*4,.6f);
        }
        [MenuItem("Ecos de Chuquicamata/Pruebas en Play/Grito lejano")]
        public static void TestScream()
        {
            GameSession g=GameSession.Instance;if(g!=null&&g.Playing)g.Audio.PlayHorrorAt("DistantScream",g.Player.transform.position+g.Player.view.transform.forward*6,.3f);
        }
        public static void BuildWindows()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            string path=EditorUtility.SaveFolderPanel("Carpeta de la build","","EcosWindows");if(string.IsNullOrEmpty(path))return;
            string source=File.Exists(ScenePath)?ScenePath:"Assets/Ecos/Scenes/Inicio.unity";
            var report=BuildPipeline.BuildPlayer(new[]{source},Path.Combine(path,"EcosDeChuquicamata.exe"),BuildTarget.StandaloneWindows64,BuildOptions.None);
            Debug.Log("Resultado de compilación: "+report.summary.result);
        }
    }
}
