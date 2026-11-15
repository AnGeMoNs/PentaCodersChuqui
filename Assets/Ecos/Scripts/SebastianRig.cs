using UnityEngine;
namespace Ecos
{
    // Pose animation for the original 17-bone mesh. Driven by actual displacement + macabre wall/ceiling climbing & contortions.
    public class SebastianRig : MonoBehaviour
    {
        public Transform[] bones;
        public SkinnedMeshRenderer skin;
        [SerializeField] Vector3[] restPositions;
        Vector3 previous;float phase,clock,motion,chase,turnLean;Quaternion previousRotation;
        public static SebastianRig Create(Transform parent,Material material)
        {
            GameObject root=new GameObject("Sebastian articulado");root.transform.SetParent(parent,false);
            SebastianRig rig=root.AddComponent<SebastianRig>();
            RigBone[] definitions=ModelLibrary.LoadRig("Sebastian");
            rig.bones=new Transform[definitions.Length];rig.restPositions=new Vector3[definitions.Length];
            for(int i=0;i<definitions.Length;i++)
            {
                RigBone definition=definitions[i];Transform bone=new GameObject(definition.name).transform;
                bone.SetParent(definition.parent<0?root.transform:rig.bones[definition.parent],false);
                bone.localPosition=definition.position-(definition.parent<0?Vector3.zero:definitions[definition.parent].position);
                rig.bones[i]=bone;rig.restPositions[i]=bone.localPosition;
            }
            rig.skin=root.AddComponent<SkinnedMeshRenderer>();rig.skin.sharedMesh=ModelLibrary.Load("Sebastian");
            rig.skin.bones=rig.bones;rig.skin.rootBone=rig.bones[0];rig.skin.sharedMaterial=material;
            rig.skin.quality=SkinQuality.Bone2;rig.skin.localBounds=new Bounds(new Vector3(0,.85f,-.1f),new Vector3(2.4f,2.6f,2.6f));
            rig.skin.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            rig.ResetPose();return rig;
        }
        public void ResetPose()
        {
            previous=transform.parent!=null?transform.parent.position:transform.position;
            previousRotation=transform.parent!=null?transform.parent.rotation:transform.rotation;
            phase=clock=motion=chase=turnLean=0;
            transform.localPosition=Vector3.zero;transform.localRotation=Quaternion.identity;
            if(bones==null||restPositions==null)return;
            for(int i=0;i<bones.Length;i++){bones[i].localPosition=restPositions[i];bones[i].localRotation=Quaternion.identity;}
        }
        void OnEnable(){ResetPose();}
        void Rotate(int index,float x,float y,float z){bones[index].localRotation=Quaternion.Euler(x,y,z);}
        void Leg(int thigh,int calf,int foot,float angle,float hipHeight,float splayRoll=0f)
        {
            float z=-Mathf.Sin(angle)*Mathf.Lerp(.10f,.23f,chase)*motion;
            float y=.10f+Mathf.Max(0,Mathf.Cos(angle))*.115f*motion;
            float down=hipHeight-y,length=Mathf.Clamp(Mathf.Sqrt(down*down+z*z),.10f,.559f);
            float aim=Mathf.Atan2(-z,down)*Mathf.Rad2Deg;
            float hip=Mathf.Acos(Mathf.Clamp((.32f*.32f+length*length-.24f*.24f)/(.64f*length),-1,1))*Mathf.Rad2Deg;
            float knee=Mathf.Acos(Mathf.Clamp((length*length-.32f*.32f-.24f*.24f)/(.1536f),-1,1))*Mathf.Rad2Deg;
            Rotate(thigh,aim+hip,splayRoll*.35f,splayRoll);Rotate(calf,-knee,0,0);Rotate(foot,-aim-hip+knee,0,0);
        }
        void LateUpdate()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing||!g.Enemy.Active||g.Enemy.visual!=transform||bones==null)return;
            float dt=Time.deltaTime;if(dt<=0)return;
            Vector3 actorPos=g.Enemy.transform.position;
            Quaternion actorRot=g.Enemy.transform.rotation;
            float distance=Vector3.Distance(actorPos,previous);previous=actorPos;
            if(distance>1)distance=0;
            clock+=dt;float speed=distance/dt;
            float turning=Mathf.DeltaAngle(previousRotation.eulerAngles.y,actorRot.eulerAngles.y)/dt;
            previousRotation=actorRot;turnLean=Mathf.Lerp(turnLean,Mathf.Clamp(turning/55,-1,1),1-Mathf.Exp(-dt*8));
            motion=Mathf.MoveTowards(motion,Mathf.Clamp01(speed/1.2f),dt*6);
            chase=Mathf.MoveTowards(chase,g.Enemy.Mode=="Perseguir"?1:0,dt*3);

            float macabre=g.Enemy.MacabreIntensity;
            int style=g.Enemy.MacabreStyle;
            float wallBlend=g.Enemy.WallClimbBlend;
            int wallSide=g.Enemy.WallClimbSide;
            float wallOff=g.Enemy.WallClimbOffset;

            int previousContact=Mathf.FloorToInt(phase/Mathf.PI+.5f);
            float cadenceBoost=1f+macabre*.45f+wallBlend*.35f;
            phase+=(distance/Mathf.Lerp(1.25f,2.0f,chase)*Mathf.PI*2)*cadenceBoost;
            if(Mathf.FloorToInt(phase/Mathf.PI+.5f)!=previousContact)
                g.Audio.PlayHorrorAt("SebastianStep",transform.position,.22f+.08f*macabre);

            // Posicionamiento y rotación de trepado por paredes / techo o zig-zag errático en el suelo
            if(wallBlend>.001f)
            {
                if(wallSide==-1||wallSide==1)
                {
                    float climbY=1.18f+.18f*Mathf.Sin(phase*1.4f)+.08f*Mathf.Cos(clock*19f);
                    Vector3 climbPos=new Vector3(-wallSide*wallOff*wallBlend,climbY*wallBlend,0f);
                    Quaternion climbRot=Quaternion.Euler(22f*wallBlend,wallSide*12f*wallBlend,wallSide*74f*wallBlend);
                    transform.localPosition=climbPos;
                    transform.localRotation=climbRot;
                }
                else
                {
                    // Acecho invertido por el techo / tuberías superiores (wallSide == 2)
                    float ceilY=1.98f+.09f*Mathf.Sin(phase*2f);
                    float swayX=.22f*Mathf.Sin(phase*.8f)*wallBlend;
                    transform.localPosition=new Vector3(swayX,ceilY*wallBlend,0f);
                    transform.localRotation=Quaternion.Euler(-24f*wallBlend,Mathf.Sin(clock*9f)*14f*wallBlend,168f*wallBlend);
                }
            }
            else
            {
                float erraticX=macabre>.1f?Mathf.Sin(clock*15f)*Mathf.Cos(clock*9.5f)*.14f*macabre*motion:0f;
                float spiderDrop=(style==2&&macabre>.2f&&!g.Enemy.Attack.Active)?-.22f*macabre*motion:0f;
                float erraticRoll=macabre>.1f?Mathf.Sin(clock*11.5f)*19f*macabre*motion:0f;
                float erraticPitch=(style==2&&macabre>.2f)?-28f*macabre*motion:0f;
                transform.localPosition=new Vector3(erraticX,spiderDrop,0f);
                transform.localRotation=Quaternion.Euler(erraticPitch,0f,erraticRoll);
            }

            float stride=Mathf.Sin(phase)*motion,opposite=-stride;
            float reach=Mathf.Clamp01((4.5f-g.Enemy.TargetDistance)/2.5f)*chase;
            float breathe=Mathf.Sin(clock*2.4f);
            float camSlow=g.Enemy.CameraFocusLevel;
            float pelvisOffset=-motion*(.05f+.025f*chase)+.012f*Mathf.Sin(phase*2)*motion*(1f+macabre*.8f);
            bones[1].localPosition=restPositions[1]+Vector3.up*pelvisOffset;
            Rotate(1,0,stride*8f*macabre,Mathf.Sin(phase)*10f*macabre);

            // Columna vertebral: contorsiones quebradas y arqueo arácnido al trepar
            float spinePitch=-9f-chase*24f+breathe*1.3f+camSlow*14f;
            float spineYaw=stride*6f*(1f+macabre*1.4f);
            float spineRoll=2f+stride*5f-turnLean*8f*motion;
            if(macabre>.05f)
            {
                float spasmSpine=Mathf.Sin(clock*23f)*Mathf.Cos(clock*17f)*18f*macabre;
                spinePitch+=wallBlend>.2f?-32f*wallBlend:(style==0?22f*Mathf.Sin(clock*9f)*macabre:-26f*macabre);
                spineRoll+=spasmSpine+Mathf.Sin(phase)*16f*macabre;
            }
            Rotate(2,spinePitch,spineYaw,spineRoll);

            // Cabeza y mandíbula: giros antinaturales de 120° estilo Jacob's Ladder / Silent Hill y mandíbula desencajada
            Vector3 target=g.Enemy.transform.InverseTransformPoint(g.Player.view.transform.position)-new Vector3(0,1.335f,0);
            float yaw=Mathf.Clamp(Mathf.Atan2(-target.x,-target.z)*Mathf.Rad2Deg,-42,42);
            float twitchPhase=clock%3.2f;
            float twitch=twitchPhase<.32f?Mathf.Sin(twitchPhase/.32f*Mathf.PI)*18:0;
            if(macabre>.05f)
            {
                float violentSnap=Mathf.Sin(clock*53f)*Mathf.Cos(clock*37f)*34f*macabre;
                float neckDislocate=(Mathf.Sin(clock*6.8f)>.58f?82f:-38f)*macabre;
                twitch+=violentSnap+neckDislocate;
            }
            if(camSlow>.1f)twitch+=Mathf.Sin(clock*38f)*camSlow*16f;
            if(g.Enemy.Searching)yaw=Mathf.Sin(clock*1.7f)*28;
            float headPitch=8f+chase*12f-camSlow*18f+(wallBlend>.2f?36f*wallBlend:Mathf.Sin(clock*31f)*16f*macabre);
            Rotate(3,headPitch,yaw+Mathf.Cos(clock*29f)*(camSlow*12f+macabre*18f),Mathf.Lerp(14,-4,chase)+breathe*2+twitch);

            float mouth=2f+20f*g.Audio.EnemyVoiceLevel+camSlow*14f+macabre*(18f+16f*Mathf.Abs(Mathf.Sin(clock*21f)));
            Rotate(4,-Mathf.Clamp(mouth,0f,48f),0,Mathf.Sin(clock*41f)*8f*macabre);

            // Brazos y manos: zarpazos arácnidos al trepar paredes/techo o carrera dislocada asimétrica
            float shieldL=camSlow*45f,shieldR=camSlow*42f;
            if(wallBlend>.15f)
            {
                float clawL=Mathf.Sin(phase*1.6f)*58f;
                float clawR=-clawL;
                Rotate(5,78f+clawL,-24f*wallBlend,-28f-14f*Mathf.Cos(phase*1.6f));
                Rotate(7,78f+clawR,24f*wallBlend,28f+14f*Mathf.Sin(phase*1.6f));
                Rotate(6,34f+Mathf.Max(0,clawL)*.6f,0,-14f);
                Rotate(8,34f+Mathf.Max(0,clawR)*.6f,0,14f);
            }
            else
            {
                float macabreArmL=macabre*(28f*Mathf.Sin(phase*1.5f+1.2f)+18f*Mathf.Sin(clock*33f));
                float macabreArmR=macabre*(32f*Mathf.Cos(phase*1.5f)-18f*Mathf.Cos(clock*29f));
                float armSplay=macabre*(18f+12f*Mathf.Sin(clock*14f));
                Rotate(5,Mathf.Lerp(chase*30-stride*38,chase*98-stride*12,reach)+shieldL+macabreArmL,camSlow*18f-armSplay,-7-chase*10-armSplay);
                Rotate(7,Mathf.Lerp(chase*24+stride*38,chase*94+stride*12,reach)+shieldR+macabreArmR,-camSlow*18f+armSplay,10+chase*10+armSplay);
                Rotate(6,8+chase*14+Mathf.Max(0,stride)*14+camSlow*32f+macabre*22f*Mathf.Abs(Mathf.Sin(clock*18f)),0,-5-macabre*12f);
                Rotate(8,12+chase*16+Mathf.Max(0,opposite)*14+camSlow*32f+macabre*22f*Mathf.Abs(Mathf.Cos(clock*18f)),0,8+macabre*12f);
            }
            Rotate(13,-6-chase*14+Mathf.Sin(clock*39f)*22f*macabre,chase*140,-12-macabre*18f);
            Rotate(14,-9-chase*12+Mathf.Cos(clock*39f)*22f*macabre,-chase*140,9+macabre*18f);

            if(g.Enemy.Alerting&&!g.Enemy.Attack.Active)
            {
                float t=g.Enemy.AlertProgress*.75f;
                float blend=Mathf.Clamp01(t/.1f)*Mathf.Clamp01((.75f-t)/.15f);
                SebastianPoseKey pose=SebastianAnimation.Alert(t);ApplyUpperPose(pose,blend);
                pelvisOffset=Mathf.Lerp(pelvisOffset,pose.pelvis,blend);
            }
            if(g.Enemy.Attack.Active)
            {
                float t=g.Enemy.Attack.Elapsed;
                float blend=Mathf.Clamp01(t/.10f)*Mathf.Clamp01((SebastianCombat.Duration-t)/.28f);
                SebastianPoseKey pose=SebastianAnimation.Attack(g.Enemy.Attack.Variant,t);ApplyUpperPose(pose,blend);
                pelvisOffset=Mathf.Lerp(pelvisOffset,pose.pelvis,blend);
            }
            bones[1].localPosition=restPositions[1]+Vector3.up*pelvisOffset;
            float legSplay=wallBlend*26f+macabre*10f;
            Leg(9,10,15,phase,.66f+pelvisOffset,-legSplay);
            Leg(11,12,16,phase+Mathf.PI,.66f+pelvisOffset,legSplay);
        }
        void BlendBone(int index,Vector3 rotation,float blend)
        {bones[index].localRotation=Quaternion.Slerp(bones[index].localRotation,Quaternion.Euler(rotation),blend);}
        void ApplyUpperPose(SebastianPoseKey pose,float blend)
        {
            BlendBone(2,pose.spine,blend);BlendBone(3,pose.head,blend);
            BlendBone(4,new Vector3(-pose.jaw,0,0),blend);
            BlendBone(5,pose.leftArm,blend);BlendBone(7,pose.rightArm,blend);
            BlendBone(6,pose.leftForearm,blend);BlendBone(8,pose.rightForearm,blend);
            BlendBone(13,pose.leftHand,blend);BlendBone(14,pose.rightHand,blend);
        }
    }
}
