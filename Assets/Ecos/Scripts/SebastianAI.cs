using System.Collections.Generic;
using UnityEngine;
namespace Ecos
{
    public class SebastianAI : MonoBehaviour
    {
        public Transform visual;public bool Active{get;private set;}public string Mode{get;private set;}="Observar";
        public SebastianCombat Attack{get;}=new SebastianCombat();
        public bool CapturedPlayer{get{return Active&&Attack.HitConfirmed;}}
        public bool SeesTarget{get;private set;}
        public bool IsWatched{get;private set;}
        public bool IsCameraSlowed{get{return cameraFocusLevel>.25f;}}
        public float CameraFocusLevel{get{return cameraFocusLevel;}}
        public float SlowdownFactor{get{return Mathf.Lerp(1f,.26f,Mathf.SmoothStep(0f,1f,cameraFocusLevel));}}
        public float InterferenceLevel{get;private set;}
        public float MacabreIntensity{get;private set;}
        public int MacabreStyle{get;private set;}
        public float WallClimbBlend{get;private set;}
        public int WallClimbSide{get;private set;}
        public float WallClimbOffset{get;private set;}=.85f;
        public bool Searching{get{return Active&&Mode=="Perseguir"&&!SeesTarget&&memory>0;}}
        public float AlertProgress{get{return alertClock>0?1-alertClock/.75f:0;}}
        public bool Alerting{get{return alertClock>0;}}
        public float TargetDistance{get;private set;}=100;
        public float Speed{get{return currentSpeed;}}
#if UNITY_EDITOR
        public bool PreviewAttacks;
#endif
        struct LampEntry { public Light light; public float baseIntensity; }
        readonly List<LampEntry> hospitalLamps=new List<LampEntry>();
        Vector3 attackDirection;float attackCooldown,alertClock,stareClock,cameraFocusLevel,repelSoundCooldown,macabreClock,crawlSoundClock;int nextAttack;bool lungeBlocked,sawPlayerHide;
        Vector3 next,lastKnown;float timer,cooldown=12,memory,grace,repath,age,currentSpeed,vocalClock;bool wasChasing;bool noticed;Renderer ghostRenderer;MaterialPropertyBlock ghostProperties;
        public void RegisterLamp(Light l)
        {
            if(l!=null)hospitalLamps.Add(new LampEntry{light=l,baseIntensity=l.intensity});
        }
        void EnsureLampsRegistered()
        {
            if(hospitalLamps.Count>0||transform.parent==null)return;
            GameSession g=GameSession.Instance;
            foreach(Light l in transform.parent.GetComponentsInChildren<Light>(true))
            {
                if(l==null||(l.type!=LightType.Point&&l.type!=LightType.Spot))continue;
                if(g!=null&&((g.Player!=null&&l.transform.IsChildOf(g.Player.transform))||l.transform.IsChildOf(g.transform)))continue;
                hospitalLamps.Add(new LampEntry{light=l,baseIntensity=l.intensity});
            }
        }
        public void ResetActor()
        {
            if(GameSession.Instance!=null&&GameSession.Instance.Audio!=null)GameSession.Instance.Audio.StopEnemyVoice();
            Attack.Cancel();attackCooldown=alertClock=stareClock=cameraFocusLevel=repelSoundCooldown=macabreClock=crawlSoundClock=0;nextAttack=0;
            SeesTarget=false;IsWatched=false;InterferenceLevel=0;MacabreIntensity=0;MacabreStyle=0;WallClimbBlend=0;WallClimbSide=0;WallClimbOffset=.85f;
            sawPlayerHide=false;TargetDistance=100;currentSpeed=0;Active=false;
            if(visual!=null){visual.gameObject.SetActive(false);visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;}
            cooldown=12;memory=0;grace=0;repath=0;RestoreHospitalLamps();
#if UNITY_EDITOR
            PreviewAttacks=false;
#endif
        }
        void RestoreHospitalLamps()
        {
            EnsureLampsRegistered();
            for(int i=0;i<hospitalLamps.Count;i++)if(hospitalLamps[i].light!=null)hospitalLamps[i].light.intensity=hospitalLamps[i].baseIntensity;
        }
        void UpdateHospitalLamps(bool activeFlicker)
        {
            EnsureLampsRegistered();
            for(int i=0;i<hospitalLamps.Count;i++)
            {
                Light l=hospitalLamps[i].light;if(l==null)continue;
                float ambientFlicker=1f;
                if((i%3)==0)
                {
                    float s=Mathf.Sin(Time.time*17f+i*3.7f)*Mathf.Cos(Time.time*29f-i*1.9f);
                    if(s>.72f)ambientFlicker=.15f;
                }
                if(!activeFlicker){l.intensity=hospitalLamps[i].baseIntensity*ambientFlicker;continue;}
                float d=Vector3.Distance(l.transform.position,transform.position);
                if(d>16f){l.intensity=hospitalLamps[i].baseIntensity*ambientFlicker;continue;}
                float influence=Mathf.Clamp01((16f-d)/14f);
                float wave=Mathf.Sin(Time.time*38f+i*2.1f)*Mathf.Cos(Time.time*21f-i*1.3f);
                float dim=Mode=="Perseguir"?Mathf.Lerp(1f,.05f,influence):Mathf.Lerp(1f,.22f,influence);
                float flicker=wave>.22f?dim:(wave<-.40f?.02f:dim*.45f);
                l.intensity=hospitalLamps[i].baseIntensity*Mathf.Lerp(ambientFlicker,flicker,influence);
            }
        }
        public void HearNoise(Vector3 sourcePos,float radius)
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing||g.Data.zone<1)return;
            if(!Active)
            {
                if(radius>=8f)cooldown=Mathf.Max(1.5f,cooldown-3.5f);
                return;
            }
            float d=Vector3.Distance(transform.position,sourcePos);
            if(d<=radius)
            {
                lastKnown=sourcePos;memory=Mathf.Max(memory,5.5f);repath=0;
                if(Mode=="Hostigar"&&d<6.5f)g.ChangeAnger(1.5f);
            }
        }
        public void OnPlayerHid(Vector3 lockerPos,bool spotted)
        {
            if(!Active)return;
            sawPlayerHide=spotted;
            if(spotted){lastKnown=lockerPos;memory=6f;repath=0;}
            else{SeesTarget=false;memory=Mathf.Min(memory,3.2f);}
        }
        public void Encounter()
        {
            GameSession g=GameSession.Instance;if(g==null||g.Data.zone<1||Active)return;
            Vector3 point=Vector3.zero;float best=-1;
            foreach(Vector2Int cell in g.Map.Cells)
            {
                if(cell.y/7!=g.Data.zone)continue;
                Vector3 candidate=g.Map.Position(cell);float distance=Vector3.Distance(candidate,g.Player.transform.position);
                if(distance<6||distance>15||!g.Map.IsWalkable(candidate))continue;
                bool hidden=Physics.Linecast(g.Player.view.transform.position,candidate+Vector3.up,1<<8,QueryTriggerInteraction.Ignore);
                Vector3 viewport=g.Player.view.WorldToViewportPoint(candidate+Vector3.up);
                bool inView=viewport.z>0&&viewport.x>.1f&&viewport.x<.9f&&viewport.y>0&&viewport.y<1;
                if(g.Data.anger<70&&(hidden||!inView))continue;
                float score=(g.Data.anger>=70?(hidden?20:0):(inView?30:0))+distance;
                if(score<=best||g.Map.Path(candidate,g.Player.transform.position).Count==0)continue;
                point=candidate;best=score;
            }
            if(best<0){cooldown=5;return;}
            transform.position=point;Vector3 facing=g.Player.transform.position-point;facing.y=0;if(facing.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(-facing);next=point;lastKnown=g.Player.transform.position;Active=true;
            visual.gameObject.SetActive(true);visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;
            Attack.Cancel();attackCooldown=0;stareClock=0;cameraFocusLevel=0;repelSoundCooldown=0;macabreClock=1.8f;crawlSoundClock=0;
            MacabreIntensity=0;MacabreStyle=0;WallClimbBlend=0;WallClimbSide=0;sawPlayerHide=false;
            alertClock=g.Data.anger>=70?.75f:0;age=0;noticed=false;currentSpeed=0;vocalClock=Random.Range(9f,13f);wasChasing=g.Data.anger>=70;UpdateAppearance(0);
            g.Audio.PlayEnemyCue(g.Data.anger>=70,g.Data.anger>=70?.65f:.38f);
            timer=g.Data.anger>=70?20:11;memory=5.5f;grace=1.5f;repath=0;
            Mode=g.Data.anger<30?"Observar":g.Data.anger<70?"Hostigar":"Perseguir";g.Audio.Play("Heartbeat",.25f);
        }
        void UpdateAppearance(float presence)
        {
            if(ghostRenderer==null)ghostRenderer=visual.GetComponent<Renderer>();
            if(ghostProperties==null)ghostProperties=new MaterialPropertyBlock();
            float huntVal=Mode=="Perseguir"?Mathf.Max(.75f,MacabreIntensity):MacabreIntensity*.5f;
            ghostProperties.SetFloat("_Hunt",huntVal);ghostProperties.SetFloat("_Presence",presence);ghostProperties.SetFloat("_HauntTime",age);ghostRenderer.SetPropertyBlock(ghostProperties);
        }
        void UpdateMacabreLocomotion(GameSession g,float distance)
        {
            bool enraged=Mode=="Perseguir"||g.Data.anger>=50f;
            float targetMacabre=enraged?Mathf.Clamp01((g.Data.anger-35f)/45f+(Mode=="Perseguir"?.35f:0f)):0f;
            MacabreIntensity=Mathf.MoveTowards(MacabreIntensity,targetMacabre,Time.deltaTime*2.2f);
            crawlSoundClock=Mathf.Max(0f,crawlSoundClock-Time.deltaTime);

            // Desciende inmediatamente al suelo cuando está a punto de atacar o ralentizado fuertemente por la cámara IR
            if(!enraged||Attack.Active||alertClock>0f||grace>0f||distance<=SebastianCombat.StartRange+.65f||IsCameraSlowed)
            {
                WallClimbBlend=Mathf.MoveTowards(WallClimbBlend,0f,Time.deltaTime*5.5f);
                if(WallClimbBlend<=.01f)WallClimbSide=0;
                return;
            }
            macabreClock-=Time.deltaTime;
            if(macabreClock<=0f)
            {
                // Alterna entre: 0 = Carrera contorsionada / espasmódica, 1 = Trepado por las paredes, 2 = Acecho arácnido por techo/cuatro patas
                MacabreStyle=(MacabreStyle+1)%3;
                macabreClock=Random.Range(2.4f,4.2f);
                if(MacabreStyle==1||MacabreStyle==2)
                {
                    RaycastHit hitL,hitR;
                    Vector3 origin=transform.position+Vector3.up*1.1f;
                    bool hasL=Physics.Raycast(origin,-transform.right,out hitL,3.5f,1<<8,QueryTriggerInteraction.Ignore);
                    bool hasR=Physics.Raycast(origin,transform.right,out hitR,3.5f,1<<8,QueryTriggerInteraction.Ignore);
                    if(MacabreStyle==2&&g.Data.anger>=68f)
                    {
                        WallClimbSide=2; // Trepa hacia el techo / tuberías superiores
                        WallClimbOffset=.85f;
                    }
                    else if(hasL&&(!hasR||hitL.distance<=hitR.distance))
                    {
                        WallClimbSide=-1;
                        WallClimbOffset=Mathf.Clamp(hitL.distance-.38f,.25f,2.4f);
                    }
                    else if(hasR)
                    {
                        WallClimbSide=1;
                        WallClimbOffset=Mathf.Clamp(hitR.distance-.38f,.25f,2.4f);
                    }
                    else
                    {
                        WallClimbSide=2;
                        WallClimbOffset=.65f;
                    }
                    if(crawlSoundClock<=0f)
                    {
                        crawlSoundClock=2.8f;
                        g.Audio.PlayHorrorAt(WallClimbSide==2?"IndustrialClangSH":"MetalDrag",transform.position+Vector3.up*1.5f,.36f);
                    }
                }
                else
                {
                    WallClimbSide=0;
                }
            }
            // Verificar continuamente que el muro lateral siga existiendo mientras trepa
            if(WallClimbSide==-1||WallClimbSide==1)
            {
                RaycastHit wallHit;
                Vector3 dir=WallClimbSide==-1?-transform.right:transform.right;
                if(Physics.Raycast(transform.position+Vector3.up*1.1f,dir,out wallHit,3.8f,1<<8,QueryTriggerInteraction.Ignore))
                    WallClimbOffset=Mathf.Lerp(WallClimbOffset,Mathf.Clamp(wallHit.distance-.38f,.22f,2.5f),Time.deltaTime*6f);
                else if(g.Data.anger>=70f)
                    WallClimbSide=2; // Pasa al techo si se abre el pasillo
                else
                    WallClimbSide=0;
            }
            float desiredClimb=(WallClimbSide!=0&&currentSpeed>.35f)?1f:0f;
            WallClimbBlend=Mathf.MoveTowards(WallClimbBlend,desiredClimb,Time.deltaTime*(desiredClimb>WallClimbBlend?3.2f:4.5f));
        }
        bool SeesPlayer()
        {
            GameSession g=GameSession.Instance;
            if(g.Player.IsHiding&&!sawPlayerHide)return false;
            RaycastHit hit;
            return Physics.Linecast(transform.position+Vector3.up*1.1f,g.Player.view.transform.position,out hit,~0,QueryTriggerInteraction.Ignore)&&hit.collider.GetComponent<PlayerRig>()!=null;
        }
        void Update()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing||g.DoorCutsceneActive)return;
            if(!Active){cameraFocusLevel=0;InterferenceLevel=Mathf.MoveTowards(InterferenceLevel,0,Time.deltaTime*2f);UpdateHospitalLamps(false);cooldown-=Time.deltaTime;if(cooldown<=0&&g.Data.zone>=1)Encounter();return;}
            age+=Time.deltaTime;timer-=Time.deltaTime;grace=Mathf.Max(0,grace-Time.deltaTime);repath-=Time.deltaTime;repelSoundCooldown=Mathf.Max(0,repelSoundCooldown-Time.deltaTime);
            float distance=Vector3.Distance(transform.position,g.Player.transform.position);TargetDistance=distance;
            attackCooldown=Mathf.Max(0,attackCooldown-Time.deltaTime);alertClock=Mathf.Max(0,alertClock-Time.deltaTime);
            float sightRange=(g.Player.Crouching?9f:15f)+(g.Player.NightVisionActive?2f:0f);
            bool sees=distance<sightRange&&SeesPlayer();SeesTarget=sees;
            if(sees||(g.Player.Running&&distance<12f)){lastKnown=g.Player.transform.position;memory=5.5f;}else memory-=Time.deltaTime;
            Vector3 screen=g.Player.view.WorldToViewportPoint(transform.position+Vector3.up*1.1f);
            IsWatched=sees&&screen.z>0&&screen.x>.12f&&screen.x<.88f&&screen.y>.08f&&screen.y<.92f;
            bool inCamReticle=sees&&g.Player.Aiming&&distance<18f&&screen.z>0&&screen.x>.20f&&screen.x<.80f&&screen.y>.15f&&screen.y<.85f;
            float focusRate=inCamReticle?(g.Player.NightVisionActive?1.65f:1.15f)*(Input.GetMouseButton(0)?1.35f:1f):-1.4f;
            bool wasSlowed=IsCameraSlowed;
            cameraFocusLevel=Mathf.Clamp01(cameraFocusLevel+Time.deltaTime*focusRate);
            if(IsCameraSlowed)
            {
                if(!wasSlowed&&repelSoundCooldown<=0f)
                {
                    repelSoundCooldown=3.5f;
                    g.Audio.PlayHorrorAt("GhostWhisper",transform.position+Vector3.up,.38f);
                }
                if(cameraFocusLevel>.82f)attackCooldown=Mathf.Max(attackCooldown,.65f);
            }
            float proxInterference=Mathf.Clamp01((14f-distance)/12f)*(Mode=="Perseguir"?1f:.65f)+(IsWatched?.2f:0f)+(cameraFocusLevel*.25f);
            InterferenceLevel=Mathf.MoveTowards(InterferenceLevel,Mathf.Clamp01(proxInterference),Time.deltaTime*3f);
            UpdateHospitalLamps(true);
            Mode=g.Data.anger<30?"Observar":g.Data.anger<70?"Hostigar":"Perseguir";
            bool chasing=Mode=="Perseguir";
            if(chasing&&!wasChasing){alertClock=.75f;g.Audio.PlayEnemyCue(true,.65f);vocalClock=7;}
            wasChasing=chasing;vocalClock-=Time.deltaTime;
            if(vocalClock<=0&&!Attack.Active&&!g.Audio.EnemyVoicePlaying)
            {
                g.Audio.PlayEnemyCue(chasing,chasing?.58f:.32f);
                vocalClock=chasing?Random.Range(6f,10f):Random.Range(10f,15f);
            }
            if(Mode=="Perseguir"&&sees)timer=Mathf.Max(timer,2);
            UpdateMacabreLocomotion(g,distance);
            float flickerPresence=Mathf.Min(Mathf.Clamp01(age/.8f),Mathf.Clamp01(timer/.7f));
            if(IsCameraSlowed)flickerPresence*=Mathf.Lerp(1f,.65f+.35f*Mathf.Abs(Mathf.Sin(Time.time*42f)),cameraFocusLevel);
            UpdateAppearance(flickerPresence);
            if(!noticed&&sees&&age>.8f&&screen.z>0&&screen.x>0&&screen.x<1&&screen.y>0&&screen.y<1)
            {
                noticed=true;
                if(!g.Audio.EnemyVoicePlaying)g.Audio.PlayEnemyCue(chasing,chasing?.60f:.35f);
            }
            if(Attack.Active)
            {
                if(!chasing&&!Attack.HitConfirmed){Attack.Cancel();attackCooldown=.5f;}
                else {timer=Mathf.Max(timer,2);TickAttack();return;}
            }
            if(chasing&&grace<=0&&alertClock<=0&&attackCooldown<=0&&distance<SebastianCombat.StartRange&&(sees||sawPlayerHide))
            {
                if(g.Player.IsHiding&&sawPlayerHide)g.Player.ExitLocker();
                BeginAttack();return;
            }
            if(timer<=0||(Mode=="Perseguir"&&memory<=0)){g.Audio.StopEnemyVoice();Attack.Cancel();currentSpeed=0;Active=false;visual.gameObject.SetActive(false);visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;RestoreHospitalLamps();cooldown=Mathf.Lerp(28,10,g.Data.anger/100);return;}
            if(grace>0||alertClock>0){currentSpeed=0;return;}
            if(distance<12&&g.Player.Running)g.ChangeAnger(Time.deltaTime*.6f);
            if(Mode=="Observar")
            {
                currentSpeed=0;Vector3 look=g.Player.transform.position-transform.position;look.y=0;if(look.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(-look);
                if(IsWatched){stareClock+=Time.deltaTime*(1f+cameraFocusLevel);if(stareClock>2.2f||distance<4.2f){g.Audio.PlayHorrorAt("GhostWhisper",transform.position+Vector3.up,.35f);timer=Mathf.Min(timer,.45f);}}
                return;
            }
            if(Mode=="Hostigar"&&distance<3){currentSpeed=0;return;}
            if(Vector3.Distance(transform.position,next)<.02f&&repath<=0)
            {
                repath=.25f;transform.position=next;var path=g.Map.Path(transform.position,lastKnown);if(path.Count>0)next=path[0];else if(g.Map.ClearSegment(transform.position,lastKnown))next=new Vector3(lastKnown.x,0,lastKnown.z);
            }
            Vector3 d=next-transform.position;if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(-d),Time.deltaTime*8);
            float baseSpeed=Mode=="Perseguir"?Mathf.Lerp(3.2f,4.45f,g.Data.anger/100):(IsWatched&&distance>3.8f?.45f:1.45f);
            // Acelerones erráticos y macabros cuando corre con mucha ira o trepa por las paredes
            if(MacabreIntensity>.1f)
            {
                float surge=1f+.22f*MacabreIntensity*Mathf.Sin(Time.time*11.5f)*Mathf.Cos(Time.time*7.3f);
                if(WallClimbBlend>.25f)surge+=.12f*WallClimbBlend;
                baseSpeed*=surge;
            }
            float speed=baseSpeed*SlowdownFactor;
            if(chasing&&distance<1.1f&&attackCooldown>0)speed=0;
            currentSpeed=Mathf.MoveTowards(currentSpeed,speed,Time.deltaTime*(chasing?6.2f:3.2f));
            Vector3 candidate=Vector3.MoveTowards(transform.position,next,Time.deltaTime*currentSpeed);
            if(g.Map.ClearSegment(transform.position,candidate))
            {
                transform.position=candidate;
            }
            else {next=transform.position;currentSpeed=0;}
        }

        void BeginAttack()
        {
            GameSession g=GameSession.Instance;
            WallClimbBlend=0f;WallClimbSide=0;
            if(visual!=null){visual.localPosition=Vector3.zero;visual.localRotation=Quaternion.identity;}
            attackDirection=g.Player.transform.position-transform.position;attackDirection.y=0;attackDirection.Normalize();
            if(attackDirection.sqrMagnitude<.5f)return;
            transform.rotation=Quaternion.LookRotation(-attackDirection);
            Attack.Begin(nextAttack);nextAttack=(nextAttack+1)%3;lungeBlocked=false;currentSpeed=0;
            g.Audio.PlayHorrorAt("AttackWhoosh",transform.position+Vector3.up,.30f);
            g.Audio.PlayEnemyVoice(nextAttack==0?"SebastianSnarlB":"SebastianSnarl",.58f);vocalClock=8;
        }
        void TickAttack()
        {
            GameSession g=GameSession.Instance;float before=Attack.TravelProgress;
            Attack.Tick(Time.deltaTime,g.Playing);
            float step=(Attack.TravelProgress-before)*SebastianCombat.Travel*SlowdownFactor;
            Vector3 toPlayer=g.Player.transform.position-transform.position;toPlayer.y=0;
            step=Mathf.Min(step,Mathf.Max(0,toPlayer.magnitude-.58f));
            if(step>0&&!lungeBlocked)
            {
                Vector3 candidate=transform.position+attackDirection*step;
                if(g.Map.IsWalkable(candidate)&&g.Map.ClearSegment(transform.position,candidate))transform.position=candidate;
                else lungeBlocked=true;
            }
            if(Attack.HitDue)
            {
                toPlayer=g.Player.transform.position-transform.position;toPlayer.y=0;
                bool facing=toPlayer.sqrMagnitude<.001f||Vector3.Dot(attackDirection,toPlayer.normalized)>.35f;
                bool canCapture=true;
#if UNITY_EDITOR
                canCapture=!PreviewAttacks;
#endif
                bool hit=Attack.ResolveHit(toPlayer.magnitude<=SebastianCombat.HitRange&&!lungeBlocked&&canCapture,SeesPlayer(),facing);
                if(hit)g.Audio.AttackImpact();
            }
            if(Attack.CaptureDue)
            {
                g.Lose("Sebastián te alcanzó. Apúntale con la videocámara para ralentizarlo, esquiva hacia un costado o escóndete bajo una cama.");return;
            }
            if(Attack.Finished){Attack.Cancel();attackCooldown=.5f;next=transform.position;repath=0;}
        }
    }
}
