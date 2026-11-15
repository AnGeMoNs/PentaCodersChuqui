using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Ecos.Editor
{
    public static class EcosValidation
    {
        static void Check(bool condition,string reason){if(!condition)throw new InvalidOperationException(reason);}
        [MenuItem("Ecos de Chuquicamata/Validacion/Recursos y guardado")]
        public static void ValidateAssets()
        {
            int count=0;
            foreach(string file in Directory.GetFiles("Assets/Ecos/Resources/Models","*.json"))
            {
                string name=Path.GetFileNameWithoutExtension(file);Mesh mesh=ModelLibrary.Load(name);count++;
                Check(mesh.vertexCount>0&&mesh.uv.Length==mesh.vertexCount&&mesh.uv2.Length==mesh.vertexCount,"UV o malla inválidos: "+name);
                foreach(Vector4 t in mesh.tangents)Check(!float.IsNaN(t.x)&&!float.IsInfinity(t.x)&&new Vector3(t.x,t.y,t.z).sqrMagnitude>.5f,"Tangente inválida: "+name);
            }
            Check(count==60,"Se esperaban 60 modelos, encontrados: "+count);
            foreach(string name in new[]{"Grime","Architecture","NightVision","Ghost"})
            {
                Shader shader=Resources.Load<Shader>("Shaders/"+name);Check(shader!=null,"Falta shader: "+name);
                Check(!ShaderUtil.ShaderHasError(shader),"Shader con errores: "+name+". Consulta Console.");
            }
            foreach(string name in new[]{"Albedo","DetailNormal","MaterialMask","PropsAlbedo","PropsNormal","PropsMask"})
            {
                Texture2D texture=Resources.Load<Texture2D>("PBR/"+name);Check(texture!=null,"Falta textura: "+name);
                TextureImporter importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                Check(importer!=null&&importer.sRGBTexture==name.Contains("Albedo"),"Espacio de color incorrecto: "+name);
            }
            foreach(string name in new[]{"FaceAlbedo","FaceNormal","ClothAlbedo","ClothNormal"})
            {
                Texture2D texture=Resources.Load<Texture2D>("Sebastian/"+name);Check(texture!=null,"Falta textura de Sebastián: "+name);
                TextureImporter importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                Check(importer!=null&&importer.sRGBTexture==name.Contains("Albedo"),"Espacio de color incorrecto: "+name);
                Check(texture.width>=1024&&texture.height>=1024,"Textura reducida al importar: "+name);
            }
            Check(SaveService.Validate(new SaveData()),"Se rechaza un guardado inicial válido.");
            foreach(string name in new[]{"DistantScream","GhostWhisper","MetalDrag","GhostArrival","SebastianSob","SebastianSnarl","SebastianBreath","SebastianStep","SebastianScream","SebastianScreamB","SebastianSnarlB","SebastianSobB","SaveRoomRE","SilentPromiseSH","HallwayExploreRE","OtherworldIndustrialSH","TypewriterKeyRE","ItemPickupRE","MapOpenSH","MandolinStingSH","IndustrialClangSH","MonsterGroanRE","GurneyRollSH","FluorescentBuzzSH"})
                Check(Resources.Load<AudioClip>("Audio/"+name)!=null,"Falta sonido: "+name);
            Check(!SaveService.Validate(new SaveData{x=float.NaN}),"Acepta coordenada NaN.");
            Check(!SaveService.Validate(new SaveData{anger=float.PositiveInfinity}),"Acepta ira infinita.");
            Check(SaveService.Validate(new SaveData{battery=0,spareBatteries=1}),"Debe permitir guardar una cámara descargada.");
            Check(!SaveService.Validate(new SaveData{x=100}),"Acepta ubicación fuera del hospital.");
            Check(!SaveService.Validate(new SaveData{flags=null}),"Acepta lista de progreso nula.");
            Check(!SaveService.Validate(new SaveData{version=99}),"Acepta una versión desconocida.");
            SaveData duplicate=new SaveData();duplicate.inventory.Add("camera");duplicate.inventory.Add("camera");duplicate.inventory.Add("");
            Check(SaveService.Validate(duplicate)&&duplicate.inventory.Count==1,"Inventario duplicado o vacío.");
            SaveData bounds=new SaveData{battery=150,anger=-10,yaw=725};Check(SaveService.Validate(bounds)&&bounds.battery==100&&bounds.anger==0&&Mathf.Approximately(bounds.yaw,5),"Normalización incorrecta.");
            Debug.Log("PASS: 60 modelos, UV, tangentes, shaders importados, texturas y validación de guardados. No sustituye un playtest.");
        }
        [MenuItem("Ecos de Chuquicamata/Validacion/Mallas de escena editable")]
        public static void ValidateSceneMeshes()
        {
            if(EditorApplication.isPlaying){Debug.LogWarning("Ejecuta esta comprobación fuera de Play, con HospitalEditable abierto.");return;}
            Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path=="Assets/Ecos/Scenes/HospitalEditable.unity","Abre HospitalEditable antes de validar.");
            int count=0;
            foreach(MeshFilter filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                Check(filter.sharedMesh!=null,"Falta malla en: "+filter.name);
                Check(AssetDatabase.Contains(filter.sharedMesh),"Malla temporal sin guardar: "+filter.name);
                Renderer renderer=filter.GetComponent<Renderer>();
                MeshCollider collider=filter.GetComponent<MeshCollider>();
                if(collider!=null)Check(collider.sharedMesh==filter.sharedMesh&&AssetDatabase.Contains(collider.sharedMesh),"Collider con malla temporal: "+filter.name);
                if(renderer!=null)foreach(Material mat in renderer.sharedMaterials)
                    Check(mat!=null&&AssetDatabase.Contains(mat),"Material temporal o ausente: "+filter.name);
                count++;
            }
            Check(count>0,"No se encontraron mallas.");
            foreach(SkinnedMeshRenderer skin in UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                Check(skin.sharedMesh!=null&&AssetDatabase.Contains(skin.sharedMesh),"Malla articulada sin guardar: "+skin.name);
                Check(skin.bones.Length==skin.sharedMesh.bindposes.Length,"Huesos o bind poses ausentes: "+skin.name);
                foreach(Transform bone in skin.bones)Check(bone!=null,"Referencia de hueso perdida: "+skin.name);
            }
            Debug.Log("PASS: referencias persistentes en "+count+" mallas, incluidos objetos desactivados. Repite después de reabrir la escena.");
        }
        [MenuItem("Ecos de Chuquicamata/Validacion/Acceso a objetos en Play")]
        public static void ValidateInteractions()
        {
            GameSession g=GameSession.Instance;
            if(!EditorApplication.isPlaying||g==null||!g.Ready){Debug.LogWarning("Abre Inicio y entra en Play antes de revisar accesos.");return;}
            Physics.SyncTransforms();int checkedItems=0,failures=0;
            foreach(Interactable item in UnityEngine.Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None))
            {
                if(item.kind=="door"||item.kind=="locker"||item.kind=="bed_hide")continue;
                Collider col=item.GetComponent<Collider>();if(col==null||!col.enabled)continue;
                bool reachable=false;Vector3 target=col.bounds.center;
                for(int ring=1;ring<=4&&!reachable;ring++)for(int direction=0;direction<32&&!reachable;direction++)
                {
                    float angle=direction*Mathf.PI/16;Vector3 foot=new Vector3(target.x+Mathf.Cos(angle)*ring*.35f,0,target.z+Mathf.Sin(angle)*ring*.35f);
                    if(!g.Map.IsWalkable(foot))continue;
                    Vector3 eye=foot+Vector3.up*1.62f,delta=target-eye;RaycastHit hit;
                    reachable=delta.magnitude<=2&&Physics.Raycast(eye,delta.normalized,out hit,2,~0,QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<Interactable>()==item;
                }
                checkedItems++;if(!reachable){failures++;Debug.LogWarning("Revisar acceso manual: "+item.id,item);}
            }
            Debug.Log("Objetos comprobados: "+checkedItems+"; accesos que requieren revisión: "+failures+". Muestreo local, no prueba del recorrido completo.");
        }
        [MenuItem("Ecos de Chuquicamata/Validacion/Colisiones y apoyo de muebles")]
        public static void ValidateColliders()
        {
            GameSession g=UnityEngine.Object.FindFirstObjectByType<GameSession>();
            Check(g!=null,"Abre la escena generada o entra en Play desde Inicio.");
            string[] floorNames={"Bed","Desk","Chair","Crib","Wheelchair","Cabinet","XRay","Generator","MotherBody","ChildBody","IVStand","Trolley","OxygenTank","Locker","Bench","Sink","BioBin","PrivacyScreen","MorgueDrawers","BedsideTable","MedicineShelf","ArchiveBoxes","JanitorCart","Stretcher","VinylSofa","WashBasin","Incubator","AutopsyTable"};
            int count=0;Physics.SyncTransforms();
            foreach(MeshFilter filter in g.transform.root.GetComponentsInChildren<MeshFilter>(true))
            {
                if(filter.transform.IsChildOf(g.Player.transform)||filter.transform.IsChildOf(g.Enemy.transform)||filter.transform.IsChildOf(g.transform))continue;
                if(filter.GetComponentInParent<DoorGate>()!=null)continue;
                if(filter.sharedMesh==null||!File.Exists("Assets/Ecos/Resources/Models/"+filter.sharedMesh.name+".json"))continue;
                Collider collider=filter.GetComponent<Collider>();
                Check(collider!=null&&collider.enabled&&!collider.isTrigger&&filter.gameObject.layer==8,"Falta colisión sólida: "+filter.name);
                if(Array.IndexOf(floorNames,filter.sharedMesh.name)>=0)
                    Check(Mathf.Abs(filter.GetComponent<Renderer>().bounds.min.y)<.015f,"Mueble sin apoyo en el suelo: "+filter.name);
                count++;
            }
            Check(count>0,"No hay modelos para comprobar.");
            Debug.Log("PASS: "+count+" modelos sólidos y apoyo de muebles. No sustituye probar colisiones al caminar y agacharse.");
        }
        [MenuItem("Ecos de Chuquicamata/Validacion/Rig de Sebastian")]
        public static void ValidateRig()
        {
            Mesh mesh=ModelLibrary.Load("Sebastian");Check(mesh.bindposes.Length==17,"El rig debe contener 17 huesos.");
            Check(mesh.boneWeights.Length==mesh.vertexCount,"Faltan pesos de vértices.");
            foreach(BoneWeight weight in mesh.boneWeights)
                Check(weight.boneIndex0>=0&&weight.boneIndex0<17&&weight.boneIndex1>=0&&weight.boneIndex1<17&&Mathf.Abs(weight.weight0+weight.weight1-1)<.0001f,"Pesos de rig inválidos.");
            SebastianRig rig=UnityEngine.Object.FindFirstObjectByType<SebastianRig>(FindObjectsInactive.Include);
            Check(rig!=null&&rig.bones.Length==17&&rig.skin!=null,"Genera HospitalEditable o entra en Play para validar el personaje.");
            foreach(Transform bone in rig.bones)Check(bone!=null,"Falta un hueso de Sebastián.");
            Debug.Log("PASS: malla, pesos y 17 huesos. Comprueba visualmente carrera, mandíbula, pausa y recarga de escena.");
        }

        [MenuItem("Ecos de Chuquicamata/Validacion/Ataques y musica")]
        public static void ValidateHorrorDirector()
        {
            SebastianCombat a=new SebastianCombat();
            Check(a.Begin(0)&&!a.Begin(1),"No debe comenzar un segundo ataque simultáneo.");
            a.Tick(1,false);Check(a.Elapsed==0,"El ataque avanza con el juego pausado.");
            a.Tick(float.NaN,true);Check(a.Elapsed==0,"Tiempo inválido aceptado.");
            a.Tick(1,true);Check(a.Elapsed<=.101f&&!a.HitDue,"Un tirón de FPS salta la anticipación.");
            a.Cancel();Check(!a.Active&&!a.HitConfirmed,"Cancelación incompleta.");
            for(int scenario=0;scenario<4;scenario++)
            {
                a.Begin(scenario%3);
                for(int frame=0;frame<33;frame++)a.Tick(1f/60,true);
                Check(!a.HitDue&&!a.ResolveHit(true,true,true),"Impacto antes del instante previsto.");
                while(!a.HitDue)a.Tick(1f/60,true);
                bool hit=a.ResolveHit(scenario!=0,scenario!=1,scenario!=2);
                Check(hit==(scenario==3),"No respeta alcance, pared o dirección.");
                Check(!a.ResolveHit(true,true,true),"Impacto duplicado.");
                if(hit)
                {
                    Check(!a.CaptureDue,"La captura omite la animación de impacto.");
                    for(int frame=0;frame<15;frame++)a.Tick(1f/60,true);
                    Check(a.CaptureDue,"La captura no termina.");
                }
                else
                {
                    while(!a.Finished)a.Tick(1f/60,true);
                    Check(!a.CaptureDue,"Un ataque fallido captura al jugador.");
                }
                a.Cancel();
            }
            foreach(string name in new[]{"DreadLayer","ChaseLayer","PanicLayer"})
            {
                AudioClip clip=Resources.Load<AudioClip>("Audio/"+name);
                Check(clip!=null&&Mathf.Abs(clip.length-16)<.01f&&clip.frequency==48000,"Capa musical incompatible: "+name);
            }
            foreach(string name in new[]{"ThreatSting","AttackWhoosh","AttackHit"})Check(Resources.Load<AudioClip>("Audio/"+name)!=null,"Falta sonido: "+name);
            for(int variant=0;variant<3;variant++)SebastianAnimation.Attack(variant,SebastianCombat.Impact);
            Debug.Log("PASS: anticipación, pausa, alcance, línea de visión recibida, impacto único, fallo, captura y tres capas musicales. Prueba todavía los colliders reales en Play.");
        }
    }
}
