using System;
namespace Ecos
{
    // Engine-independent timing. Movement and line-of-sight remain owned by SebastianAI.
    public sealed class SebastianCombat
    {
        public const float Windup=.36f,Impact=.56f,CaptureDelay=.22f,Duration=1.20f;
        public const float StartRange=1.25f,HitRange=1.0f,Travel=1.10f;
        public bool Active{get;private set;}
        public bool HitConfirmed{get;private set;}
        public bool Resolved{get;private set;}
        public int Variant{get;private set;}
        public float Elapsed{get;private set;}
        public bool HitDue{get{return Active&&!Resolved&&Elapsed>=Impact;}}
        float capturedTime;
        public bool CaptureDue{get{return Active&&HitConfirmed&&capturedTime>=CaptureDelay;}}
        public bool Finished{get{return Active&&!HitConfirmed&&Elapsed>=Duration;}}
        public float TravelProgress{get{return Clamp((Elapsed-Windup)/(Impact-Windup));}}
        static float Clamp(float value){return Math.Max(0,Math.Min(1,value));}
        public bool Begin(int variant)
        {
            if(Active)return false;
            Variant=Math.Max(0,Math.Min(2,variant));Elapsed=capturedTime=0;Resolved=HitConfirmed=false;Active=true;return true;
        }
        public void Tick(float delta,bool playing)
        {
            if(!Active||!playing||delta<=0||float.IsNaN(delta)||float.IsInfinity(delta))return;
            float step=Math.Min(delta,.1f);
            if(HitConfirmed)capturedTime+=step;
            Elapsed=Math.Min(Duration,Elapsed+step);
        }
        public bool ResolveHit(bool withinReach,bool clearLine,bool facing)
        {
            if(!HitDue)return false;
            Resolved=true;HitConfirmed=withinReach&&clearLine&&facing;return HitConfirmed;
        }
        public void Cancel(){Active=HitConfirmed=Resolved=false;Elapsed=capturedTime=0;}
    }
}
