using System;
using UnityEngine;
namespace Ecos
{
    [Serializable] public struct SebastianPoseKey
    {
        public float time,pelvis,jaw;
        public Vector3 spine,head,leftArm,rightArm,leftForearm,rightForearm,leftHand,rightHand;
        public static SebastianPoseKey Mix(SebastianPoseKey a,SebastianPoseKey b,float t)
        {
            return new SebastianPoseKey{time=Mathf.Lerp(a.time,b.time,t),pelvis=Mathf.Lerp(a.pelvis,b.pelvis,t),jaw=Mathf.Lerp(a.jaw,b.jaw,t),
                spine=Vector3.Lerp(a.spine,b.spine,t),head=Vector3.Lerp(a.head,b.head,t),leftArm=Vector3.Lerp(a.leftArm,b.leftArm,t),rightArm=Vector3.Lerp(a.rightArm,b.rightArm,t),
                leftForearm=Vector3.Lerp(a.leftForearm,b.leftForearm,t),rightForearm=Vector3.Lerp(a.rightForearm,b.rightForearm,t),leftHand=Vector3.Lerp(a.leftHand,b.leftHand,t),rightHand=Vector3.Lerp(a.rightHand,b.rightHand,t)};
        }
    }
    [Serializable] public class SebastianAttackAnimation {public string name;public SebastianPoseKey[] keys;}
    [Serializable] public class SebastianAnimationSet {public SebastianAttackAnimation[] attacks;public SebastianPoseKey[] alert;}
    public static class SebastianAnimation
    {
        static SebastianAnimationSet data;
        public static void ClearCache(){data=null;}
        static SebastianAnimationSet Data
        {
            get
            {
                if(data==null)
                {
                    TextAsset asset=Resources.Load<TextAsset>("Sebastian/Motion");
                    if(asset==null)throw new InvalidOperationException("Falta Sebastian/Motion.json");
                    data=JsonUtility.FromJson<SebastianAnimationSet>(asset.text);
                    if(data.attacks==null||data.attacks.Length!=3||data.alert==null)throw new InvalidOperationException("Animaciones de Sebastián incompletas.");
                }
                return data;
            }
        }
        static SebastianPoseKey Sample(SebastianPoseKey[] keys,float time)
        {
            for(int i=1;i<keys.Length;i++)if(time<=keys[i].time)
            {
                float t=Mathf.InverseLerp(keys[i-1].time,keys[i].time,time);t=t*t*(3-2*t);
                return SebastianPoseKey.Mix(keys[i-1],keys[i],t);
            }
            return keys[keys.Length-1];
        }
        public static SebastianPoseKey Attack(int variant,float time){return Sample(Data.attacks[Mathf.Clamp(variant,0,2)].keys,time);}
        public static SebastianPoseKey Alert(float time){return Sample(Data.alert,time);}
    }
}
