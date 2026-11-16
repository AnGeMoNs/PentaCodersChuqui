using System.Collections.Generic;
using UnityEngine;
namespace Ecos
{
    public enum SessionState { Menu,Playing,Paused,Dead,Ending }
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance{get;private set;}
        public PlayerRig Player;public HospitalMap Map;public SebastianAI Enemy;public AudioDirector Audio;
        public SaveData Data{get;private set;}=new SaveData();
        public SessionState State{get;private set;}=SessionState.Menu;
        public string Panel{get;private set;}="";
        public string DocumentId="",Message="",Result="",Code="",InspectItemId="camera";
        public float MessageUntil,MemoryStarted,MemorySubtitleUntil,SavedUntil,DoorCutsceneUntil,DeathSequenceStarted,EndingSequenceStarted;
        public int MemoryIndex,DoorCutsceneZone,SelectedMapZone;
        public bool EndingTruthChosen{get;private set;}
        public bool[] Switches=new bool[3];
        public int ToyProgress{get{return toyStep;}}
        public bool DoorCutsceneActive{get{return State==SessionState.Playing&&Time.unscaledTime<DoorCutsceneUntil;}}
        public float DeathJumpscareElapsed{get{return State==SessionState.Dead?Time.unscaledTime-DeathSequenceStarted:0f;}}
        public float EndingCinematicElapsed{get{return State==SessionState.Ending?Time.unscaledTime-EndingSequenceStarted:0f;}}
        public float InspectYaw{get{return inspectYaw;}}
        public float InspectPitch{get{return inspectPitch;}}
        public float InspectZoom{get{return inspectZoom;}}
        public float Volume=.7f,Brightness=.62f,Sensitivity=2,FieldOfView=72,NoiseAmount=1,HorrorVolume=.65f,MusicVolume=.72f;public bool MinimalHUD;
        public bool Ready{get{return initialized;}}
        public bool Playing{get{return State==SessionState.Playing&&Panel=="";}}
        int toyStep;readonly int[] toyOrder={0,2,1};bool initialized;
        GameObject doorCutsceneRig,inspectRig,inspectModelObj,jumpscareRig,endingRig;
        Camera doorCamera,inspectCamera,jumpscareCamera,endingCamera;
        Light doorLight,jumpscareRedLight,jumpscareWhiteLight,endingKeyLight,endingRimLight;
        SebastianRig jumpscareActor,endingActor;
        MaterialPropertyBlock endingGhostProps;
        float inspectYaw=25f,inspectPitch=12f,inspectZoom=1f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics(){Instance=null;}
        void Awake(){Instance=this;}
        void Start()
        {
            Volume=PlayerPrefs.GetFloat("EcosVolume",.7f);Brightness=PlayerPrefs.GetFloat("EcosBrightness",.62f);Sensitivity=PlayerPrefs.GetFloat("EcosSensitivity",2);
            MusicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("EcosMusicVolume",.72f));
            HorrorVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("EcosHorrorVolume",.65f));Volume=Mathf.Clamp01(Volume);Brightness=Mathf.Clamp01(Brightness);Sensitivity=Mathf.Clamp(Sensitivity,.3f,5);
            FieldOfView=Mathf.Clamp(PlayerPrefs.GetFloat("EcosFOV",72),60,95);NoiseAmount=Mathf.Clamp01(PlayerPrefs.GetFloat("EcosNoise",1));MinimalHUD=PlayerPrefs.GetInt("EcosMinimalHUD",0)==1;
            SetupAuxiliaryCameras();
            Audio.Initialize();Audio.Zone(0);Enemy.ResetActor();ApplyOptions();SetCursor(false);initialized=true;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogDensity=.036f;RenderSettings.fogColor=new Color(.010f,.013f,.016f);RenderSettings.reflectionIntensity=.60f;
            QualitySettings.pixelLightCount=12;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowProjection=ShadowProjection.CloseFit;QualitySettings.shadowDistance=65;QualitySettings.shadowCascades=4;QualitySettings.vSyncCount=1;Application.targetFrameRate=60;
        }
        void SetupAuxiliaryCameras()
        {
            doorCutsceneRig=new GameObject("Camara_Apertura_Puerta");doorCutsceneRig.transform.SetParent(transform,false);
            doorCamera=doorCutsceneRig.AddComponent<Camera>();doorCamera.nearClipPlane=.05f;doorCamera.farClipPlane=45f;doorCamera.fieldOfView=58f;
            doorCamera.clearFlags=CameraClearFlags.SolidColor;doorCamera.backgroundColor=new Color(.01f,.015f,.012f);doorCamera.depth=5;
            doorLight=doorCutsceneRig.AddComponent<Light>();doorLight.type=LightType.Spot;doorLight.spotAngle=85f;doorLight.range=14f;doorLight.intensity=2.8f;doorLight.color=new Color(.72f,.82f,.74f);
            doorCutsceneRig.SetActive(false);

            inspectRig=new GameObject("Inspector_3D_RE");inspectRig.transform.SetParent(transform,false);inspectRig.transform.position=new Vector3(0,-45f,0);
            GameObject camObj=new GameObject("InspectCamera");camObj.transform.SetParent(inspectRig.transform,false);camObj.transform.localPosition=new Vector3(0,0,-1.25f);
            inspectCamera=camObj.AddComponent<Camera>();inspectCamera.nearClipPlane=.02f;inspectCamera.farClipPlane=10f;inspectCamera.fieldOfView=42f;
            inspectCamera.clearFlags=CameraClearFlags.SolidColor;inspectCamera.backgroundColor=new Color(.025f,.03f,.028f,1f);inspectCamera.depth=10;
            GameObject keyLightObj=new GameObject("InspectKeyLight");keyLightObj.transform.SetParent(inspectRig.transform,false);keyLightObj.transform.localPosition=new Vector3(.9f,.8f,-1.1f);
            Light kl=keyLightObj.AddComponent<Light>();kl.type=LightType.Point;kl.range=6f;kl.intensity=2.4f;kl.color=new Color(.95f,.90f,.80f);
            GameObject rimLightObj=new GameObject("InspectRimLight");rimLightObj.transform.SetParent(inspectRig.transform,false);rimLightObj.transform.localPosition=new Vector3(-.8f,-.5f,.7f);
            Light rl=rimLightObj.AddComponent<Light>();rl.type=LightType.Point;rl.range=5f;rl.intensity=1.6f;rl.color=new Color(.45f,.68f,.72f);
            inspectRig.SetActive(false);

            jumpscareRig=new GameObject("Jumpscare_Grito_Pantalla_Completa");jumpscareRig.transform.SetParent(transform,false);jumpscareRig.transform.position=new Vector3(0,-85f,0);
            GameObject jCamObj=new GameObject("JumpscareCamera");jCamObj.transform.SetParent(jumpscareRig.transform,false);jCamObj.transform.localPosition=new Vector3(0,1.31f,-.34f);
            jumpscareCamera=jCamObj.AddComponent<Camera>();jumpscareCamera.nearClipPlane=.02f;jumpscareCamera.farClipPlane=8f;jumpscareCamera.fieldOfView=54f;
            jumpscareCamera.clearFlags=CameraClearFlags.SolidColor;jumpscareCamera.backgroundColor=new Color(.02f,.005f,.005f,1f);jumpscareCamera.depth=20;
            GameObject jRedObj=new GameObject("JumpscareRedLight");jRedObj.transform.SetParent(jumpscareRig.transform,false);jRedObj.transform.localPosition=new Vector3(.22f,1.42f,-.45f);
            jumpscareRedLight=jRedObj.AddComponent<Light>();jumpscareRedLight.type=LightType.Point;jumpscareRedLight.range=4f;jumpscareRedLight.intensity=3.8f;jumpscareRedLight.color=new Color(.95f,.12f,.08f);
            GameObject jWhiteObj=new GameObject("JumpscareSurgicalLight");jWhiteObj.transform.SetParent(jumpscareRig.transform,false);jWhiteObj.transform.localPosition=new Vector3(-.20f,1.18f,-.42f);
            jumpscareWhiteLight=jWhiteObj.AddComponent<Light>();jumpscareWhiteLight.type=LightType.Point;jumpscareWhiteLight.range=4f;jumpscareWhiteLight.intensity=2.4f;jumpscareWhiteLight.color=new Color(.82f,.92f,.88f);
            Material screamMat=new Material(Resources.Load<Shader>("Shaders/Ghost")){name="Jumpscare_Presencia"};ModelLibrary.BindTextures(screamMat);
            jumpscareActor=SebastianRig.Create(jumpscareRig.transform,screamMat);
            jumpscareActor.transform.localPosition=Vector3.zero;
            jumpscareActor.transform.localRotation=Quaternion.identity;
            jumpscareRig.SetActive(false);

            endingRig=new GameObject("Cinematica_Final_3D_Sebastian");endingRig.transform.SetParent(transform,false);endingRig.transform.position=new Vector3(0,-125f,0);
            GameObject eCamObj=new GameObject("EndingCinematicCamera");eCamObj.transform.SetParent(endingRig.transform,false);eCamObj.transform.localPosition=new Vector3(0,1.45f,-2.35f);
            endingCamera=eCamObj.AddComponent<Camera>();endingCamera.nearClipPlane=.03f;endingCamera.farClipPlane=18f;endingCamera.fieldOfView=46f;
            endingCamera.clearFlags=CameraClearFlags.SolidColor;endingCamera.backgroundColor=new Color(.018f,.024f,.028f,1f);endingCamera.depth=22;
            GameObject eKeyObj=new GameObject("EndingKeyLight");eKeyObj.transform.SetParent(endingRig.transform,false);eKeyObj.transform.localPosition=new Vector3(.6f,2.4f,-1.4f);
            endingKeyLight=eKeyObj.AddComponent<Light>();endingKeyLight.type=LightType.Point;endingKeyLight.range=10f;endingKeyLight.intensity=2.8f;endingKeyLight.color=new Color(.95f,.86f,.68f);
            GameObject eRimObj=new GameObject("EndingRimLight");eRimObj.transform.SetParent(endingRig.transform,false);eRimObj.transform.localPosition=new Vector3(-.9f,1.8f,1.5f);
            endingRimLight=eRimObj.AddComponent<Light>();endingRimLight.type=LightType.Point;endingRimLight.range=9f;endingRimLight.intensity=2.1f;endingRimLight.color=new Color(.48f,.78f,.92f);
            GameObject childBed=ModelLibrary.Create("ChildBody",endingRig.transform,new Vector3(-.68f,0,.15f),Vector3.one*.95f);ModelLibrary.SetBaseHeight(childBed,-125f);
            GameObject motherBed=ModelLibrary.Create("MotherBody",endingRig.transform,new Vector3(.68f,0,.15f),Vector3.one*.95f);ModelLibrary.SetBaseHeight(motherBed,-125f);
            ModelLibrary.Create("SurgicalLamp",endingRig.transform,new Vector3(0,3.05f,.15f),Vector3.one*.9f);
            Material endingMat=new Material(Resources.Load<Shader>("Shaders/Ghost")){name="Ending_Sebastian_Presencia"};ModelLibrary.BindTextures(endingMat);
            endingActor=SebastianRig.Create(endingRig.transform,endingMat);
            endingActor.transform.localPosition=new Vector3(-.05f,0,-.42f);
            endingActor.transform.localRotation=Quaternion.identity;
            endingRig.SetActive(false);
        }
        void UpdateDeathJumpscare()
        {
            if(jumpscareRig==null||jumpscareActor==null||jumpscareActor.bones==null||jumpscareActor.bones.Length<17)return;
            float t=DeathJumpscareElapsed;
            float lunge=Mathf.Clamp01(t/.22f);
            float shakeAmp=t<2.4f?Mathf.Lerp(.024f,.007f,t/2.4f):.003f;
            float sx=Mathf.Sin(t*67f)*shakeAmp,sy=Mathf.Cos(t*53f)*shakeAmp;
            float camZ=Mathf.Lerp(-.56f,-.29f,lunge)+Mathf.Sin(t*39f)*.008f;
            jumpscareCamera.transform.localPosition=new Vector3(sx,1.305f+sy,camZ);
            jumpscareCamera.transform.localRotation=Quaternion.Euler(Mathf.Sin(t*43f)*4.5f,Mathf.Cos(t*37f)*3.5f,Mathf.Sin(t*59f)*7.5f);
            jumpscareCamera.fieldOfView=52f+Mathf.Sin(t*31f)*5f;
            float jawOpen=36f+12f*Mathf.Abs(Mathf.Sin(t*28f));
            float headPitch=-8f+Mathf.Sin(t*34f)*11f;
            float headYaw=Mathf.Sin(t*23f)*19f;
            float headRoll=Mathf.Cos(t*41f)*22f;
            jumpscareActor.bones[2].localRotation=Quaternion.Euler(-18f+Mathf.Sin(t*19f)*5f,0,Mathf.Sin(t*27f)*6f);
            jumpscareActor.bones[3].localRotation=Quaternion.Euler(headPitch,headYaw,headRoll);
            jumpscareActor.bones[4].localRotation=Quaternion.Euler(-jawOpen,0,0);
            jumpscareActor.bones[5].localRotation=Quaternion.Euler(105f+Mathf.Sin(t*31f)*12f,18f,-14f);
            jumpscareActor.bones[7].localRotation=Quaternion.Euler(102f+Mathf.Cos(t*29f)*12f,-18f,14f);
            jumpscareActor.bones[6].localRotation=Quaternion.Euler(24f,0,-12f);
            jumpscareActor.bones[8].localRotation=Quaternion.Euler(24f,0,12f);
            jumpscareActor.bones[13].localRotation=Quaternion.Euler(-22f,135f,-18f);
            jumpscareActor.bones[14].localRotation=Quaternion.Euler(-22f,-135f,18f);
            if(jumpscareRedLight!=null)jumpscareRedLight.intensity=3.2f+2.2f*Mathf.Sin(t*47f);
            if(jumpscareWhiteLight!=null)jumpscareWhiteLight.intensity=1.8f+1.6f*Mathf.Cos(t*61f);
        }
        void UpdateEndingCinematic()
        {
            if(endingRig==null||endingActor==null||endingActor.bones==null||endingActor.bones.Length<17)return;
            float t=EndingCinematicElapsed;
            float breathe=Mathf.Sin(t*1.8f);
            if(endingKeyLight!=null)endingKeyLight.color=EndingTruthChosen?new Color(.98f,.85f,.62f):new Color(.52f,.68f,.88f);
            if(endingRimLight!=null)endingRimLight.color=EndingTruthChosen?new Color(.62f,.88f,.95f):new Color(.35f,.48f,.68f);

            float presence=1f;
            if(t<4.5f)
            {
                float u=t/4.5f;
                float angle=Mathf.Lerp(-38f,24f,Mathf.SmoothStep(0f,1f,u))*Mathf.Deg2Rad;
                float radius=Mathf.Lerp(2.35f,1.95f,u);
                endingCamera.transform.localPosition=new Vector3(Mathf.Sin(angle)*radius,1.42f+u*.12f,-Mathf.Cos(angle)*radius);
                endingCamera.transform.LookAt(endingRig.transform.position+new Vector3(.12f,1.08f,-.05f));
                endingCamera.fieldOfView=45f;
                endingActor.bones[2].localRotation=Quaternion.Euler(-4f+breathe*1.2f,-16f,0);
                endingActor.bones[3].localRotation=Quaternion.Euler(14f,-26f,4f);
                endingActor.bones[4].localRotation=Quaternion.Euler(-2f,0,0);
                endingActor.bones[5].localRotation=Quaternion.Euler(48f,-24f,-12f);
                endingActor.bones[6].localRotation=Quaternion.Euler(22f,0,-8f);
                endingActor.bones[7].localRotation=Quaternion.Euler(14f,8f,8f);
                endingActor.bones[8].localRotation=Quaternion.Euler(10f,0,6f);
            }
            else if(t<9.0f)
            {
                float u=(t-4.5f)/4.5f;
                endingCamera.transform.localPosition=new Vector3(Mathf.Lerp(-.28f,.18f,u),1.32f,Mathf.Lerp(-1.05f,-.88f,u));
                endingCamera.transform.LookAt(endingRig.transform.position+new Vector3(-.05f,1.30f,-.42f));
                endingCamera.fieldOfView=38f;
                float nod=Mathf.Sin(Mathf.Clamp01((u-.25f)/.45f)*Mathf.PI)*11f;
                endingActor.bones[2].localRotation=Quaternion.Euler(-2f+breathe,Mathf.Lerp(-16f,0f,u),0);
                endingActor.bones[3].localRotation=Quaternion.Euler(4f+nod,Mathf.Lerp(-24f,0f,Mathf.SmoothStep(0f,1f,u)),Mathf.Sin(t*1.2f)*2f);
                endingActor.bones[4].localRotation=Quaternion.Euler(-3f-2f*Mathf.Max(0,Mathf.Sin(t*2.5f)),0,0);
                float waveRaise=Mathf.Sin(Mathf.Clamp01(u*1.4f)*Mathf.PI);
                endingActor.bones[7].localRotation=Quaternion.Euler(Mathf.Lerp(14f,62f,waveRaise),-12f,10f);
                endingActor.bones[8].localRotation=Quaternion.Euler(Mathf.Lerp(10f,38f,waveRaise),0,8f);
                endingActor.bones[5].localRotation=Quaternion.Euler(16f,0,-8f);
                endingActor.bones[6].localRotation=Quaternion.Euler(12f,0,-5f);
            }
            else
            {
                float u=Mathf.Clamp01((t-9.0f)/4.5f);
                endingCamera.transform.localPosition=new Vector3(Mathf.Sin(t*.25f)*.35f,Mathf.Lerp(1.55f,2.45f,u),Mathf.Lerp(-1.65f,-2.85f,u));
                endingCamera.transform.LookAt(endingRig.transform.position+new Vector3(0,1.05f,0));
                endingCamera.fieldOfView=48f;
                endingActor.bones[2].localRotation=Quaternion.Euler(6f*u+breathe,0,0);
                endingActor.bones[3].localRotation=Quaternion.Euler(-16f*u,0,0);
                endingActor.bones[4].localRotation=Quaternion.Euler(-2f,0,0);
                endingActor.bones[5].localRotation=Quaternion.Euler(12f,0,-14f*u);
                endingActor.bones[7].localRotation=Quaternion.Euler(12f,0,14f*u);
                presence=Mathf.Lerp(1f,.08f,Mathf.SmoothStep(0f,1f,u));
            }
            if(endingGhostProps==null)endingGhostProps=new MaterialPropertyBlock();
            endingGhostProps.SetFloat("_Hunt",0f);
            endingGhostProps.SetFloat("_Presence",presence);
            endingGhostProps.SetFloat("_HauntTime",t);
            endingActor.skin.SetPropertyBlock(endingGhostProps);
        }
        public static string GetInspectModelName(string itemId)
        {
            switch(itemId)
            {
                case "camera":return "Camcorder";
                case "recorder":return "Recorder";
                case "welcome":return "AudioTape";
                case "infant_key":case "urgency_key":case "basement_key":case "pathology_key":case "morgue_key":return "Key";
                case "fuse":return "Battery";
                case "drawing":return "Teddy";
                case "evidence":return "XRay";
                case "secret":return "ArchiveBoxes";
                default:return "Document";
            }
        }
        public void OpenInspect(string itemId)
        {
            if(string.IsNullOrEmpty(itemId))itemId="camera";
            InspectItemId=itemId;inspectYaw=25f;inspectPitch=14f;inspectZoom=1f;
            if(inspectModelObj!=null)Destroy(inspectModelObj);
            string modelName=GetInspectModelName(itemId);
            inspectModelObj=ModelLibrary.Create(modelName,inspectRig.transform,Vector3.zero,Vector3.one);
            Renderer r=inspectModelObj.GetComponent<Renderer>();
            if(r!=null)
            {
                Bounds b=r.bounds;float maxDim=Mathf.Max(.05f,Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z)));
                float scale=.58f/maxDim;inspectModelObj.transform.localScale=Vector3.one*scale;
                Bounds scaledB=r.bounds;
                inspectModelObj.transform.position=inspectRig.transform.position-(scaledB.center-inspectModelObj.transform.position);
            }
            Audio.PlayUIConfirm();OpenPanel("inspect");
        }
        public void ResetInspectView(){inspectYaw=25f;inspectPitch=14f;inspectZoom=1f;Audio.PlayUIClick();}
        public void UnlockAndShowDoor(int gateZone)
        {
            foreach(DoorGate gate in FindObjectsByType<DoorGate>(FindObjectsSortMode.None))
            {
                if(gate.zone==gateZone)
                {
                    gate.ForceUnlockAndAnimate();
                    DoorCutsceneZone=gateZone;
                    DoorCutsceneUntil=Time.unscaledTime+2.35f;
                    if(doorCutsceneRig!=null)
                    {
                        float z=gateZone*14-1f;
                        doorCutsceneRig.transform.position=new Vector3(0,1.65f,z-3.6f);
                        doorCutsceneRig.transform.rotation=Quaternion.Euler(3f,0,0);
                        doorCutsceneRig.SetActive(true);
                    }
                    break;
                }
            }
        }
        public bool IsZoneCleared(int z)
        {
            switch(z)
            {
                case 0:return Data.journal.Contains("welcome");
                case 1:return Has("clue_frame")||Has("gate_2");
                case 2:return Has("toys")||Has("gate_3");
                case 3:return Has("clue_xray")&&Data.journal.Contains("record19")&&Data.journal.Contains("record85");
                case 4:return Has("secret")||Has("gate_5");
                case 5:return Has("power")||Has("gate_6");
                case 6:return Has("clue_seal")||Has("gate_7");
                case 7:return Has("together");
                default:return false;
            }
        }
        public bool Has(string flag){return Data.flags.Contains(flag);}
        public void Flag(string flag){if(!Has(flag))Data.flags.Add(flag);}
        public void Add(string id)
        {
            if(!Data.inventory.Contains(id))Data.inventory.Add(id);
            InspectItemId=id;
            Notify("Obtenido: "+StoryData.Get(id).title+" · Pulsa F para inspeccionar en 3D");
            Audio.PlayItemPickup();
        }
        public void ChangeAnger(float delta){Data.anger=Mathf.Clamp(Data.anger+delta,0,100);}
        public void Notify(string message){Message=message;MessageUntil=Time.unscaledTime+5;}
        public void NewGame()
        {
            Data=new SaveData();Data.inventory.Add("camera");Data.inventory.Add("recorder");Data.journal.Add("welcome");Flag("visited_0");StartData();Checkpoint();Flashback(0);
        }
        public void Continue()
        {
            SaveData saved=SaveService.Read();if(saved==null){Notify("No hay un checkpoint válido.");return;}Data=saved;StartData();Notify("Checkpoint recuperado · "+StoryData.Zones[Data.zone]);
        }
        void StartData()
        {
            State=SessionState.Playing;Panel="";toyStep=0;Switches=new bool[3];Code="";DoorCutsceneUntil=0;MemorySubtitleUntil=0;
            if(jumpscareRig!=null)jumpscareRig.SetActive(false);
            if(endingRig!=null)endingRig.SetActive(false);
            Audio.ResetTransient();RefreshWorld();Physics.SyncTransforms();
            Vector3 savedPosition=new Vector3(Data.x,Data.y,Data.z);
            if(!Map.IsWalkable(savedPosition)){Vector3 safe=Map.SafePosition(new Vector3(0,0,Data.zone*14+2));Data.x=safe.x;Data.y=.03f;Data.z=safe.z;}
            SelectedMapZone=Data.zone;Flag("visited_"+Data.zone);
            Player.Teleport(Data);Enemy.ResetActor();Audio.Zone(Data.zone);SyncPause();
        }
        void LateUpdate()
        {
            if(!initialized)return;
            if(State==SessionState.Dead)UpdateDeathJumpscare();
            else if(State==SessionState.Ending)UpdateEndingCinematic();
        }
        void Update()
        {
            if(!initialized)return;
            if(doorCutsceneRig!=null&&doorCutsceneRig.activeSelf&&!DoorCutsceneActive)doorCutsceneRig.SetActive(false);
            if(DoorCutsceneActive&&(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)))
            {
                DoorCutsceneUntil=0;if(doorCutsceneRig!=null)doorCutsceneRig.SetActive(false);
            }
            if(Panel=="inspect"&&inspectModelObj!=null)
            {
                if(Input.GetMouseButton(0))
                {
                    inspectYaw-=Input.GetAxis("Mouse X")*4.5f;
                    inspectPitch=Mathf.Clamp(inspectPitch+Input.GetAxis("Mouse Y")*4.5f,-75f,75f);
                }
                float scroll=Input.mouseScrollDelta.y;
                if(Mathf.Abs(scroll)>.01f)inspectZoom=Mathf.Clamp(inspectZoom+scroll*.12f,.65f,1.65f);
                if(Input.GetMouseButtonDown(1))ResetInspectView();
                inspectCamera.fieldOfView=Mathf.Lerp(inspectCamera.fieldOfView,42f/inspectZoom,Time.unscaledDeltaTime*12f);
                inspectModelObj.transform.rotation=Quaternion.Euler(inspectPitch,inspectYaw,0);
            }
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(Panel=="decision"){Notify("Elige qué hacer con el expediente.");}
                else if(Panel=="inspect"){Audio.PlayUIClick();OpenPanel("inventory");}
                else if(Panel!=""){Audio.PlayUIClick();ClosePanel();}
                else if(Player!=null&&Player.IsHiding)Player.ExitLocker();
                else if(State==SessionState.Playing){Audio.PlayUIClick();Pause();}
                else if(State==SessionState.Paused){Audio.PlayUIClick();Resume();}
            }
            if(Input.GetKeyDown(KeyCode.M)&&State==SessionState.Playing&&Panel!="decision")
            {
                Audio.PlayMapToggle();
                if(Panel=="map")ClosePanel();else{SelectedMapZone=Data.zone;OpenPanel("map");}
            }
            if(Input.GetKeyDown(KeyCode.F)&&State==SessionState.Playing&&Panel!="decision")
            {
                if(Panel=="inspect"){Audio.PlayUIClick();ClosePanel();}
                else if(Panel==""||Panel=="inventory"||Panel=="document")
                {
                    string targetItem=Panel=="document"&&!string.IsNullOrEmpty(DocumentId)?DocumentId:InspectItemId;
                    OpenInspect(targetItem);
                }
            }
            if(Input.GetKeyDown(KeyCode.H)&&State==SessionState.Playing){Audio.PlayUIClick();if(Panel=="help")ClosePanel();else if(Panel=="")OpenPanel("help");}
            if(Input.GetKeyDown(KeyCode.Tab)&&State==SessionState.Playing&&Panel!="decision")
            {
                Audio.PlayUIClick();
                if(Panel=="inventory"||Panel=="map"||Panel=="inspect")ClosePanel();else if(Panel=="")OpenPanel("inventory");
            }
            if(Playing)
            {
                int zone=Mathf.Clamp(Mathf.FloorToInt(Player.transform.position.z/14),0,7);
                if(zone!=Data.zone)
                {
                    Data.zone=zone;SelectedMapZone=zone;Flag("visited_"+zone);Audio.Zone(zone);
                    if(zone>=2&&!Has("anger_zone_"+zone)){Flag("anger_zone_"+zone);ChangeAnger(6);}
                    Checkpoint();Notify(StoryData.Chapters[zone]+" · "+StoryData.Zones[zone]+" · Usa la cámara IR (Clic Der. + Q) a larga distancia o para ralentizar a Sebastián");Enemy.Encounter();
                    EventManager.Instance?.TriggerZoneChanged(zone,StoryData.Zones[zone]);
                }
                if(Data.zone>=2)ChangeAnger(Time.deltaTime*.012f);
                if(Player.transform.position.y< -5)Lose("Caíste fuera del recorrido.");
            }
            SyncPause();
        }
        void SyncPause()
        {
            if(inspectRig!=null)inspectRig.SetActive(Panel=="inspect");
            if(jumpscareRig!=null)jumpscareRig.SetActive(State==SessionState.Dead);
            if(endingRig!=null)endingRig.SetActive(State==SessionState.Ending);
            Time.timeScale=Playing?1:0;AudioListener.pause=State==SessionState.Paused;SetCursor(Playing);
            if(GameManager.Instance!=null)
            {
                GameStates mapped=State==SessionState.Menu?GameStates.GameStart:(State==SessionState.Dead||State==SessionState.Ending)?GameStates.GameOver:GameStates.InGame;
                GameManager.Instance.SetState(mapped);
            }
        }
        void SetCursor(bool locked){Cursor.lockState=locked?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!locked;}
        public void Pause(){State=SessionState.Paused;SyncPause();}
        public void Resume(){State=SessionState.Playing;Panel="";SyncPause();}
        public void Menu(){Audio.ResetTransient();State=SessionState.Menu;Panel="";DoorCutsceneUntil=0;if(doorCutsceneRig!=null)doorCutsceneRig.SetActive(false);if(jumpscareRig!=null)jumpscareRig.SetActive(false);if(endingRig!=null)endingRig.SetActive(false);Enemy.ResetActor();SyncPause();}
        public void OpenPanel(string panel){Panel=panel;SyncPause();}
        public void ClosePanel(){if(Panel=="options")SaveOptions();Panel="";SyncPause();}
        public void Options(){Audio.PlayUIClick();OpenPanel("options");}
        public void ApplyOptions()
        {
            AudioListener.volume=Volume;
            float b=Player!=null&&Player.NightVisionActive?Mathf.Lerp(.016f,.038f,Brightness):Mathf.Lerp(.024f,.068f,Brightness);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(b*.92f,b*1.04f,b*1.16f);
            RenderSettings.ambientEquatorColor=new Color(b*.88f,b*.96f,b*1.02f);
            RenderSettings.ambientGroundColor=new Color(b*.52f,b*.48f,b*.46f);
            RenderSettings.ambientLight=new Color(b,b*1.05f,b*1.08f);
        }
        public void SaveOptions(){PlayerPrefs.SetFloat("EcosMusicVolume",MusicVolume);PlayerPrefs.SetFloat("EcosHorrorVolume",HorrorVolume);PlayerPrefs.SetFloat("EcosFOV",FieldOfView);PlayerPrefs.SetFloat("EcosNoise",NoiseAmount);PlayerPrefs.SetInt("EcosMinimalHUD",MinimalHUD?1:0);PlayerPrefs.SetFloat("EcosVolume",Volume);PlayerPrefs.SetFloat("EcosBrightness",Brightness);PlayerPrefs.SetFloat("EcosSensitivity",Sensitivity);PlayerPrefs.Save();ApplyOptions();}
        public void Checkpoint()
        {
            if(State!=SessionState.Playing)return;
            Vector3 p=Player.transform.position;Data.x=p.x;Data.y=Mathf.Max(.03f,p.y);Data.z=p.z;Data.yaw=Player.transform.eulerAngles.y;Data.zone=Mathf.Clamp(Mathf.FloorToInt(p.z/14),0,7);
            if(SaveService.Write(Data)){SavedUntil=Time.unscaledTime+3;Audio.PlayTypewriterSave();}else{SavedUntil=0;Notify("No se pudo guardar. Revisa Console antes de salir de la partida.");}
        }
        public void Lose(string reason)
        {
            if(State!=SessionState.Playing)return;
            if(Player!=null&&Player.IsHiding)Player.ExitLocker();
            DoorCutsceneUntil=0;if(doorCutsceneRig!=null)doorCutsceneRig.SetActive(false);
            Result=reason;State=SessionState.Dead;Panel="";DeathSequenceStarted=Time.unscaledTime;
            if(jumpscareRig!=null){jumpscareRig.SetActive(true);UpdateDeathJumpscare();}
            Audio.Play("SebastianScreamB",.55f*HorrorVolume);
            Audio.Play("SebastianScream",.48f*HorrorVolume);
            Audio.Play("ThreatSting",.45f*HorrorVolume);
            Audio.Play("Death",.42f);
            SyncPause();
        }
        public void Flashback(int index)
        {
            MemoryIndex=index;MemoryStarted=Time.unscaledTime;MemorySubtitleUntil=Time.unscaledTime+7.0f;
            if(Panel=="memory"||Panel=="decision")ClosePanel();
            Audio.Play("Memory",.32f);
            Audio.PlayClueSting();
            EventManager.Instance?.TriggerMemoryUnlocked(index);
        }
        public void FinishMemory(){MemorySubtitleUntil=0;if(Panel=="memory")ClosePanel();}
        public void Read(string id){DocumentId=id;InspectItemId=id;Audio.PlayUIConfirm();OpenPanel("document");}
        void ResetToysPose()
        {
            foreach(Interactable item in FindObjectsByType<Interactable>(FindObjectsSortMode.None))
                if(item.kind=="toy")item.transform.localRotation=Quaternion.identity;
        }
        public void Interact(Interactable item)
        {
            if(!Playing)return;
            switch(item.kind)
            {
                case "locker":
                    Player.ToggleLocker(item.transform);break;
                case "bed_hide":
                    Player.ToggleBedHide(item.transform);break;
                case "valve":
                    if(Has("steam_purged")){Notify("La línea de vapor ya está estabilizada.");break;}
                    Flag("steam_purged");item.transform.Rotate(0,0,90f);ChangeAnger(-5);
                    Audio.PlayAt("DoorSlide",item.transform.position,.35f);
                    EventManager.Instance?.TriggerParticleBurst(item.transform.position,new Color(.82f,.88f,.92f,.7f));
                    Checkpoint();Notify("Presión de vapor purgada. El generador trabajará de forma más estable y silenciosa.");break;
                case "hint":Notify(item.entry);break;
                case "clue":
                    if(Has("clue_"+item.id)){Notify("Ya revisaste este objeto.");break;}
                    Flag("clue_"+item.id);Add(item.entry);ChangeAnger(-8);
                    if(item.entry=="evidence"&&!Data.journal.Contains("evidence"))Data.journal.Add("evidence");
                    if(item.id=="frame")UnlockAndShowDoor(2);
                    else if(item.id=="seal")UnlockAndShowDoor(7);
                    Checkpoint();Flashback(item.number);break;
                case "document":
                    if(!Data.journal.Contains(item.entry)){Data.journal.Add(item.entry);if(!Data.inventory.Contains(item.entry))Data.inventory.Add(item.entry);ChangeAnger(-2);Audio.Play("Collect");EventManager.Instance?.TriggerDocumentCollected(item.entry);}
                    Read(item.entry);Checkpoint();break;
                case "battery":
                    if(Data.spareBatteries>=99){Notify("No puedes llevar más pilas.");break;}
                    Data.spareBatteries++;Flag("taken_"+item.id);item.gameObject.SetActive(false);Audio.PlayItemPickup();Checkpoint();Notify("Pila guardada. Pulsa R para recargar la cámara.");break;
                case "key":Add(item.entry);Flag("taken_"+item.id);item.gameObject.SetActive(false);Checkpoint();break;
                case "door":item.GetComponent<DoorGate>().Open();break;
                case "toy":
                    if(Has("toys")){Notify("Los juguetes ya desbloquearon la puerta de Urgencias.");break;}
                    Audio.PlayAt("Bell"+(item.number+1),item.transform.position,.4f);
                    if(item.number==toyOrder[toyStep])
                    {
                        toyStep++;item.transform.localRotation=Quaternion.Euler(0,28f*(toyStep%2==0?-1:1),0);
                        string[] stepNames={"ROJO (Sol)","ROJO → AZUL (Cielo)","ROJO → AZUL → AMARILLO (Luz)"};
                        Notify("Armonía infantil ("+toyStep+" / 3): "+stepNames[toyStep-1]);
                        if(toyStep==3){Flag("toys");Add("urgency_key");ChangeAnger(-10);UnlockAndShowDoor(3);Flashback(2);Checkpoint();}
                    }
                    else
                    {
                        toyStep=0;ResetToysPose();ChangeAnger(12);Audio.Play("Error");
                        if(Enemy!=null)Enemy.HearNoise(item.transform.position,14f);
                        Notify("La disonancia rompió el orden del dibujo y alertó al hospital. Empieza nuevamente: ROJO → AZUL → AMARILLO.");
                    }
                    break;
                case "safe":if(Has("secret")){Read("secret");}else{Code="";Audio.PlayUIClick();OpenPanel("safe");}break;
                case "generator":if(Has("power"))Notify("El generador ya está funcionando.");else{Audio.PlayUIClick();OpenPanel("generator");}break;
                case "child":Flag("child_seen");Audio.PlayUIConfirm();Notify("Camilla A1: Sebastián. Freno de transferencia liberado; busca a su madre en la sala derecha (C2).");Checkpoint();break;
                case "mother":
                    if(!Has("child_seen")){Notify("Camilla C2: su madre. Identifica primero a Sebastián en la otra sala.");break;}
                    if(Has("together")){Notify("Las camillas ya están juntas.");break;}
                    Flag("together");ChangeAnger(-35);RefreshWorld();Physics.SyncTransforms();
                    if(!Map.IsWalkable(Player.transform.position))
                    {
                        CharacterController controller=Player.GetComponent<CharacterController>();controller.enabled=false;
                        Player.transform.position=Map.SafePosition(Player.transform.position)+Vector3.up*.03f;controller.enabled=true;
                    }
                    Checkpoint();Flashback(5);break;
                case "ending":if(!Has("together"))Notify("Primero identifica a Sebastián y reúne ambas camillas.");else{Audio.PlayUIConfirm();OpenPanel("ending");}break;
            }
        }
        public void SubmitCode()
        {
            if(State!=SessionState.Playing||Panel!="safe"||Has("secret"))return;
            if(Code!="1985")
            {
                Code="";Audio.Play("Error");ChangeAnger(7);
                if(Enemy!=null&&Player!=null)Enemy.HearNoise(Player.transform.position,15f);
                Notify("Alarma de bloqueo: código incorrecto. Revisa Ingreso 19 (19) y La fecha tachada (1985).");return;
            }
            Flag("secret");if(!Data.journal.Contains("secret"))Data.journal.Add("secret");Add("secret");Add("basement_key");Audio.Play("Unlock");OpenPanel("decision");
        }
        public void DecideEvidence(bool preserve)
        {
            if(State!=SessionState.Playing||Panel!="decision"||Has("preserved")||Has("destroyed"))return;
            if(preserve){Flag("preserved");ChangeAnger(-15);}else{Flag("destroyed");ChangeAnger(40);Data.inventory.Remove("secret");}
            ClosePanel();UnlockAndShowDoor(5);Checkpoint();Flashback(3);
        }
        public void Generator()
        {
            if(State!=SessionState.Playing||Panel!="generator"||Has("power"))return;
            if(!Data.inventory.Contains("fuse")){Notify("Falta el fusible. Busca en la sala de enfrente.");return;}
            if(!(Switches[0]&&!Switches[1]&&Switches[2]))
            {
                ChangeAnger(10);Audio.Play("Error");
                if(Enemy!=null&&Player!=null)Enemy.HearNoise(Player.transform.position,16f);
                Notify("Sobrecarga en el circuito. Fase A: ON · Fase B: OFF · Fase C: igual a Fase A.");return;
            }
            Data.inventory.Remove("fuse");Flag("power");Add("pathology_key");ClosePanel();UnlockAndShowDoor(6);Checkpoint();Notify("Energía trifásica restaurada. Puerta de Anatomía Patológica abierta.");
        }
        public void End(bool tellTruth)
        {
            if(!Has("together"))return;
            if(Player!=null&&Player.IsHiding)Player.ExitLocker();
            DoorCutsceneUntil=0;if(doorCutsceneRig!=null)doorCutsceneRig.SetActive(false);
            EndingTruthChosen=tellTruth&&Has("preserved")&&Data.inventory.Contains("evidence");
            Result=EndingTruthChosen?
                "JUNTOS\n\nReuniste a Sebastián y a su madre y conservaste las pruebas. Tomás sale del hospital con una historia que por fin podrá ser escuchada.\n\nEl eco se apaga. Esta vez, nadie quedó atrás.":
                "EL SILENCIO\n\nMadre e hijo descansan juntos, pero la verdad permanece incompleta o Tomás elige callar.\n\nAfuera amanece. En la grabadora todavía se oye una pregunta: ¿alguien nos recordará?";
            State=SessionState.Ending;Panel="";EndingSequenceStarted=Time.unscaledTime;
            Enemy.ResetActor();
            if(endingRig!=null){endingRig.SetActive(true);UpdateEndingCinematic();}
            Audio.Play("Memory",.45f);SyncPause();
        }
        public void RefreshWorld()
        {
            foreach(Interactable item in FindObjectsByType<Interactable>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(item.kind=="battery"||item.kind=="key")item.gameObject.SetActive(!Has("taken_"+item.id));
                if(item.kind=="mother"){item.transform.position=Has("together")?new Vector3(-6.6f,0,104):new Vector3(8,0,104);ModelLibrary.SetBaseHeight(item.gameObject,0);}
            }
            foreach(DoorGate gate in FindObjectsByType<DoorGate>(FindObjectsSortMode.None))gate.Refresh();
        }
        public string Objective()
        {
            if(!Has("gate_2"))return "Explora Recepción o usa la cámara IR (Clic Der. + Q) y revisa detrás del cuadro.";
            if(!Has("toys"))return "Lee el dibujo y activa los juguetes en el orden armónico (ROJO → AZUL → AMARILLO).";
            if(!Has("gate_3"))return "Avanza por la puerta desbloqueada hacia Urgencias.";
            if(!Has("secret"))return "Lee los registros de Urgencias (Ingreso 19 y 1985) y abre el archivador de Diagnóstico.";
            if(!Has("gate_5"))return "Cruza el acceso desbloqueado hacia el Subterráneo G.";
            if(!Has("power"))return Data.inventory.Contains("fuse")?"Purga la válvula opcionalmente, instala el fusible y configura el generador (A:ON, B:OFF, C:ON).":"Busca el fusible en la sala frente al generador.";
            if(!Has("gate_6"))return "Cruza el acceso desbloqueado hacia Anatomía Patológica.";
            if(!Has("gate_7"))return "Revisa el cajón sellado de Anatomía Patológica para abrir la Morgue.";
            if(!Has("child_seen"))return "Identifica la camilla A1 de Sebastián, en la sala izquierda.";
            if(!Has("together"))return "Encuentra a su madre en la sala derecha (C2) y reúne las camillas.";
            return "Revisa la grabadora sobre la mesa junto a las camillas.";
        }
        void OnApplicationFocus(bool focused){if(!focused&&initialized&&Playing)Pause();}
        public void ReloadCamera()
        {
            if(!Playing)return;
            if(Data.battery>=99.9f){Notify("La cámara ya tiene la carga completa.");return;}
            if(Data.spareBatteries<=0){Notify("No tienes pilas. Busca en las salas del hospital.");return;}
            Data.spareBatteries--;Data.battery=100;Audio.PlayItemPickup();Checkpoint();Notify("Cámara recargada. Pilas restantes: "+Data.spareBatteries);
        }
        public void Quit()
        {
            Time.timeScale=1;AudioListener.pause=false;
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
            #else
            Application.Quit();
            #endif
        }
        void OnDestroy(){if(Instance==this)Instance=null;Time.timeScale=1;AudioListener.pause=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    }
}
