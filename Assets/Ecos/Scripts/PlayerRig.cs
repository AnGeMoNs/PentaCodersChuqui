using UnityEngine;
namespace Ecos
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRig : MonoBehaviour
    {
        public Camera view;public Light infrared;public Light eyeLight;public Transform cameraModel;
        [SerializeField] float walkSpeed=2.8f,runSpeed=5.2f,crouchSpeed=1.4f;
        public float Stamina{get;private set;}=1;
        public bool Running{get;private set;}
        public bool Crouching{get;private set;}
        public bool Aiming{get;private set;}
        public bool IsHiding{get;private set;}
        public bool HidingUnderBed{get;private set;}
        public float NoiseLevel{get;private set;}
        public float PanicLevel{get;private set;}
        public bool NightVisionEnabled{get;private set;}=true;
        public bool NightVisionActive{get{return GameSession.Instance!=null&&GameSession.Instance.Playing&&Aiming&&NightVisionEnabled&&GameSession.Instance.Data.battery>0;}}
        public Interactable Target{get;private set;}
        CharacterController body;Collider activeHideCollider;float pitch,vertical,stepClock,bobPhase,tiltRoll,heartbeatClock;Vector3 planarVelocity,hideReturnPos;float hideReturnYaw;int batteryWarnings;bool exhausted;
        static readonly int[] batteryThresholds={20,10,5};
        static readonly Vector2[] aimOffsets={new Vector2(.012f,0),new Vector2(-.012f,0),new Vector2(0,.018f),new Vector2(0,-.018f)};
        void Awake(){body=GetComponent<CharacterController>();}
        public void Teleport(SaveData data)
        {
            if(body==null)body=GetComponent<CharacterController>();body.enabled=false;IsHiding=false;HidingUnderBed=false;activeHideCollider=null;transform.position=new Vector3(data.x,data.y,data.z);transform.rotation=Quaternion.Euler(0,data.yaw,0);vertical=0;pitch=0;stepClock=0;bobPhase=0;tiltRoll=0;heartbeatClock=0;NoiseLevel=0;PanicLevel=0;planarVelocity=Vector3.zero;batteryWarnings=0;Running=false;Crouching=false;body.height=1.8f;body.center=Vector3.up*.9f;view.transform.localPosition=Vector3.up*1.62f;view.transform.localRotation=Quaternion.identity;view.fieldOfView=GameSession.Instance!=null?GameSession.Instance.FieldOfView:72;Stamina=1;exhausted=false;Aiming=false;NightVisionEnabled=true;infrared.enabled=false;if(eyeLight!=null)eyeLight.enabled=true;Target=null;cameraModel.gameObject.SetActive(false);body.enabled=true;
        }
        public void ToggleLocker(Transform locker)
        {
            GameSession g=GameSession.Instance;if(g==null||body==null)return;
            if(IsHiding){ExitLocker();return;}
            if(locker==null)return;
            bool spotted=g.Enemy!=null&&g.Enemy.Active&&g.Enemy.SeesTarget&&g.Enemy.TargetDistance<11f;
            hideReturnPos=transform.position;hideReturnYaw=transform.eulerAngles.y;
            activeHideCollider=locker.GetComponent<Collider>();
            IsHiding=true;HidingUnderBed=false;Running=false;Crouching=false;Aiming=false;planarVelocity=Vector3.zero;NoiseLevel=0;
            body.enabled=false;
            Vector3 forward=-locker.forward;forward.y=0;if(forward.sqrMagnitude<.01f)forward=Vector3.back;forward.Normalize();
            transform.position=locker.position+forward*.12f+Vector3.up*.03f;
            transform.rotation=Quaternion.LookRotation(forward);
            pitch=0;view.transform.localPosition=new Vector3(0,1.48f,0);view.transform.localRotation=Quaternion.identity;
            g.Audio.PlayAt("DoorSlide",locker.position+Vector3.up,.28f);
            if(g.Enemy!=null)g.Enemy.OnPlayerHid(locker.position+forward*.85f,spotted);
            g.Notify(spotted?"¡Sebastián te vio entrar al casillero!":"Oculto en el casillero. Pulsa E para salir cuando pase el peligro.");
        }
        public void ToggleBedHide(Transform bed)
        {
            GameSession g=GameSession.Instance;if(g==null||body==null)return;
            if(IsHiding){ExitLocker();return;}
            if(bed==null)return;
            bool spotted=g.Enemy!=null&&g.Enemy.Active&&g.Enemy.SeesTarget&&g.Enemy.TargetDistance<11f;
            hideReturnPos=transform.position;hideReturnYaw=transform.eulerAngles.y;
            activeHideCollider=bed.GetComponent<Collider>();
            IsHiding=true;HidingUnderBed=true;Running=false;Crouching=true;Aiming=false;planarVelocity=Vector3.zero;NoiseLevel=0;
            body.enabled=false;
            Vector3 lookOut=hideReturnPos-bed.position;lookOut.y=0;if(lookOut.sqrMagnitude<.01f)lookOut=bed.right;lookOut.Normalize();
            transform.position=new Vector3(bed.position.x,.02f,bed.position.z);
            transform.rotation=Quaternion.LookRotation(lookOut);
            pitch=-4f;view.transform.localPosition=new Vector3(0,.24f,0);view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            g.Audio.Play("Step",.25f);
            if(g.Enemy!=null)g.Enemy.OnPlayerHid(bed.position+lookOut*.95f,spotted);
            g.Notify(spotted?"¡Sebastián te vio esconderte bajo la cama!":"Oculto bajo la cama (estilo Outlast). Pulsa E para salir.");
        }
        public void ExitLocker()
        {
            if(!IsHiding)return;
            GameSession g=GameSession.Instance;bool wasBed=HidingUnderBed;IsHiding=false;HidingUnderBed=false;
            bool hadCol=activeHideCollider!=null&&activeHideCollider.enabled;
            if(hadCol){activeHideCollider.enabled=false;Physics.SyncTransforms();}
            Vector3 safe=g!=null&&g.Map!=null&&!g.Map.IsWalkable(hideReturnPos)?g.Map.SafePosition(hideReturnPos):hideReturnPos;
            if(hadCol){activeHideCollider.enabled=true;Physics.SyncTransforms();}
            activeHideCollider=null;
            transform.position=new Vector3(safe.x,Mathf.Max(.03f,safe.y),safe.z);
            transform.rotation=Quaternion.Euler(0,hideReturnYaw,0);
            view.transform.localPosition=Vector3.up*1.62f;
            body.enabled=true;
            if(g!=null)
            {
                g.Audio.PlayAt(wasBed?"Step":"DoorSlide",transform.position+Vector3.up,.25f);
                g.Notify(wasBed?"Saliste de debajo de la cama.":"Saliste del casillero.");
            }
        }
        void UpdateOutlastAtmosphere(GameSession g)
        {
            // Penumbra atmosférica mejorada cuando la visión nocturna está apagada y 32m de alcance claro con IR encendido
            float targetFog=NightVisionActive?.012f:Mathf.Lerp(.048f,.028f,g.Brightness);
            RenderSettings.fogDensity=Mathf.Lerp(RenderSettings.fogDensity,targetFog,Time.deltaTime*8f);
            RenderSettings.fogColor=NightVisionActive?Color.black:new Color(.010f,.013f,.016f);
            RenderSettings.reflectionIntensity=NightVisionActive?.42f:.62f;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            float b=NightVisionActive?Mathf.Lerp(.016f,.038f,g.Brightness):Mathf.Lerp(.024f,.068f,g.Brightness);
            RenderSettings.ambientSkyColor=new Color(b*.92f,b*1.04f,b*1.16f);
            RenderSettings.ambientEquatorColor=new Color(b*.88f,b*.96f,b*1.02f);
            RenderSettings.ambientGroundColor=new Color(b*.52f,b*.48f,b*.46f);
            RenderSettings.ambientLight=new Color(b,b*1.05f,b*1.08f);
            if(eyeLight!=null)
            {
                eyeLight.enabled=g.Playing;
                eyeLight.intensity=NightVisionActive?.32f:Mathf.Lerp(.36f,.82f,g.Brightness);
                eyeLight.range=NightVisionActive?5.5f:Mathf.Lerp(6.2f,8.5f,g.Brightness);
            }
        }
        void Update()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Ready)return;
            if(!g.Playing){planarVelocity=Vector3.zero;Running=false;NoiseLevel=0;infrared.enabled=false;if(eyeLight!=null)eyeLight.enabled=false;cameraModel.gameObject.SetActive(false);Target=null;return;}
            if(g.Enemy.CapturedPlayer){if(IsHiding)ExitLocker();planarVelocity=Vector3.zero;Running=false;NoiseLevel=0;Target=null;return;}
            if(Input.GetKeyDown(KeyCode.R))g.ReloadCamera();
            UpdateOutlastAtmosphere(g);
            if(IsHiding)
            {
                if(Input.GetMouseButtonDown(1))Aiming=!Aiming;
                if(Aiming&&Input.GetKeyDown(KeyCode.Q))NightVisionEnabled=!NightVisionEnabled;
                cameraModel.gameObject.SetActive(Aiming);infrared.enabled=NightVisionActive;
                if(infrared.enabled)infrared.intensity=6.8f;
                float lookSens=g.Sensitivity*(HidingUnderBed?.65f:.55f);
                float yawLimit=HidingUnderBed?42f:25f;
                float yawDelta=Mathf.Clamp(Input.GetAxis("Mouse X")*lookSens,-yawLimit,yawLimit);
                transform.Rotate(0,yawDelta,0);
                pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*lookSens,HidingUnderBed?-16f:-28f,HidingUnderBed?20f:28f);
                view.transform.localPosition=new Vector3(0,HidingUnderBed?.24f:1.48f,0);
                view.transform.localRotation=Quaternion.Euler(pitch,0,0);
                view.fieldOfView=Mathf.Lerp(view.fieldOfView,Aiming?50f:62f,Time.deltaTime*8);
                Stamina=Mathf.Clamp01(Stamina+Time.deltaTime*.22f);
                g.Data.battery=Mathf.Max(0,g.Data.battery-Time.deltaTime*(NightVisionActive?.35f:0));
                UpdatePanicAndHeartbeat(g);
                if(Input.GetKeyDown(KeyCode.E)){ExitLocker();return;}
                return;
            }
            if(Input.GetMouseButtonDown(1))Aiming=!Aiming;
            if(Aiming&&Input.GetKeyDown(KeyCode.Q))NightVisionEnabled=!NightVisionEnabled;
            cameraModel.gameObject.SetActive(Aiming);
            float interference=g.Enemy!=null?g.Enemy.InterferenceLevel:0f;
            bool irFlickerOff=interference>.45f&&Mathf.Sin(Time.time*53f)>.78f;
            infrared.enabled=NightVisionActive&&!irFlickerOff;
            if(infrared.enabled)infrared.intensity=6.8f*(1f-interference*.28f*Mathf.Abs(Mathf.Sin(Time.time*37f)));
            Crouching=Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl);
            if(!Crouching&&Physics.CheckCapsule(transform.position+Vector3.up*1.05f,transform.position+Vector3.up*1.54f,.25f,1<<8,QueryTriggerInteraction.Ignore))Crouching=true;
            body.height=Crouching?1.05f:1.8f;body.center=Vector3.up*body.height*.5f;
            float aimSensitivity=g.Sensitivity*(Aiming?(Input.GetMouseButton(0)?.52f:.78f):1);
            float mouseX=Input.GetAxis("Mouse X")*aimSensitivity;
            transform.Rotate(0,mouseX,0);pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*aimSensitivity,-80,80);
            float x=(Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),z=(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0);
            Vector3 move=Vector3.ClampMagnitude(transform.right*x+transform.forward*z,1);
            if(exhausted&&Stamina>.25f)exhausted=false;
            Running=Input.GetKey(KeyCode.LeftShift)&&!Crouching&&!Aiming&&!exhausted&&move.sqrMagnitude>0;
            Stamina=Mathf.Clamp01(Stamina+Time.deltaTime*(Running?-.25f:.16f));if(Stamina<=0){exhausted=true;Running=false;}
            if(body.isGrounded&&vertical<0)vertical=-2;vertical-=20*Time.deltaTime;
            float speed=Crouching?crouchSpeed:Running?runSpeed:walkSpeed;
            planarVelocity=Vector3.MoveTowards(planarVelocity,move*speed,Time.deltaTime*(move.sqrMagnitude>.01f?18:26));
            Vector3 before=transform.position;
            body.Move((planarVelocity+Vector3.up*vertical)*Time.deltaTime);
            Vector3 displacement=transform.position-before;displacement.y=0;
            float stepSpeed=displacement.magnitude/Mathf.Max(.0001f,Time.deltaTime);
            NoiseLevel=stepSpeed<.15f?0f:(Crouching?.08f:Running?1f:.38f);
            if(body.isGrounded&&displacement.sqrMagnitude>.000001f)
            {
                stepClock+=displacement.magnitude;
                bobPhase+=displacement.magnitude*(Crouching?4.2f:Running?4.8f:3.9f);
                if(stepClock>=(Crouching?1.25f:Running?1.45f:1.55f))
                {
                    g.Audio.Play("Step",Crouching?.1f:Running?.34f:.22f);stepClock=0;
                    if(g.Enemy!=null&&!Crouching)g.Enemy.HearNoise(transform.position,Running?13f:5.5f);
                }
            }
            else bobPhase+=Time.deltaTime*1.6f;
            UpdatePanicAndHeartbeat(g);
            float baseEyeY=Crouching?.92f:1.62f;
            float bobAmp=stepSpeed>.15f?(Crouching?.014f:Running?.042f:.022f):.006f;
            float bobY=Mathf.Sin(bobPhase)*bobAmp;
            float bobX=Mathf.Cos(bobPhase*.5f)*bobAmp*.45f;
            float tremor=PanicLevel*.008f*Mathf.Sin(Time.time*29f);
            view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,new Vector3(bobX+tremor,baseEyeY+bobY,0),Time.deltaTime*12);
            tiltRoll=Mathf.Lerp(tiltRoll,Mathf.Clamp(-x*1.2f-mouseX*.22f,-2.5f,2.5f),Time.deltaTime*8);
            view.transform.localRotation=Quaternion.Euler(pitch+tremor*18f,0,tiltRoll);
            view.fieldOfView=Mathf.Lerp(view.fieldOfView,Aiming?(Input.GetMouseButton(0)?37:55):g.FieldOfView+PanicLevel*2.5f,Time.deltaTime*10);
            g.Data.battery=Mathf.Max(0,g.Data.battery-Time.deltaTime*(NightVisionActive?.35f:0));
            if(g.Enemy!=null&&g.Enemy.IsCameraSlowed&&NightVisionActive)
                g.Data.battery=Mathf.Max(0,g.Data.battery-Time.deltaTime*.20f);
            if(g.Data.battery<=0)infrared.enabled=false;
            if(g.Data.battery>25)batteryWarnings=0;
            int[] thresholds=batteryThresholds;
            for(int i=0;i<thresholds.Length;i++)if(g.Data.battery<=thresholds[i]&&(batteryWarnings&(1<<i))==0)
            {batteryWarnings|=1<<i;g.Notify("Batería baja: "+Mathf.CeilToInt(g.Data.battery)+"%. Busca una pila o apaga la iluminación cuando sea seguro.");}
            Target=null;RaycastHit hit;
            if(Physics.Raycast(view.transform.position,view.transform.forward,out hit,2,~0,QueryTriggerInteraction.Ignore))
            {
                Interactable item=hit.collider.GetComponentInParent<Interactable>();if(item!=null&&item.Available)Target=item;
            }
            if(Target==null)
            {
                Vector2[] offsets=aimOffsets;
                foreach(Vector2 offset in offsets)
                {
                    Ray ray=view.ViewportPointToRay(new Vector3(.5f+offset.x,.5f+offset.y,0));
                    if(Physics.Raycast(ray,out hit,2,~0,QueryTriggerInteraction.Ignore))
                    {
                        Interactable item=hit.collider.GetComponentInParent<Interactable>();
                        if(item!=null&&item.Available){Target=item;break;}
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.E)&&Target!=null)Target.Use();
        }
        void UpdatePanicAndHeartbeat(GameSession g)
        {
            float targetPanic=0f;
            if(g.Enemy!=null&&g.Enemy.Active)
            {
                float prox=Mathf.Clamp01((16f-g.Enemy.TargetDistance)/14f);
                targetPanic=Mathf.Clamp01(prox*(g.Enemy.Mode=="Perseguir"?1f:.65f)+(g.Enemy.SeesTarget?.25f:0f));
            }
            if(exhausted)targetPanic=Mathf.Max(targetPanic,.35f);
            PanicLevel=Mathf.MoveTowards(PanicLevel,targetPanic,Time.deltaTime*1.8f);
            if(PanicLevel>.12f)
            {
                heartbeatClock-=Time.deltaTime;
                if(heartbeatClock<=0f)
                {
                    heartbeatClock=Mathf.Lerp(1.05f,.38f,PanicLevel);
                    g.Audio.Play("Heartbeat",Mathf.Lerp(.12f,.42f,PanicLevel)*g.HorrorVolume);
                }
            }
            else heartbeatClock=0f;
        }
    }
}
