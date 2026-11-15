using UnityEngine;
namespace Ecos
{
    public class AudioDirector : MonoBehaviour
    {
        AudioSource ambience,previous,music,saveRoomSource,exploreSource,fx,enemyVoice,scoreSting,radioStatic;AudioLowPassFilter enemyFilter;float enemyVoiceVolume;
        readonly AudioSource[] scoreLayers=new AudioSource[3];
        readonly string[] scoreNames={"DreadLayer","ChaseLayer","PanicLayer"};
        bool scoreRunning,scorePaused,huntCueLatched;double scoreStart;float threat,stingCooldown,sirenCooldown;
        public float MusicThreat{get{return threat;}}
        readonly float[] vocalSamples=new float[256];
        readonly string[] quietVoices={"SebastianSob","SebastianSobB","SebastianBreath"};
        readonly string[] huntVoices={"SebastianScream","SebastianScreamB","SebastianSnarl","SebastianSnarlB"};
        int lastQuiet=-1,lastHunt=-1;
        public float EnemyVoiceLevel{get;private set;}
        public bool EnemyVoicePlaying{get{return enemyVoice!=null&&enemyVoice.isPlaying;}}
        float zoneFade=1,previousVolume,ambientClock=14;int zone=-1;AudioSource[] spatial=new AudioSource[6];AudioLowPassFilter[] filters=new AudioLowPassFilter[6];int voice,lastScare=-1;
        readonly string[] scares={"PipeKnock","DistantScream","GhostWhisper","MetalDrag","IndustrialClangSH","MonsterGroanRE","GurneyRollSH","FluorescentBuzzSH"};
        public void Initialize()
        {
            for(int i=0;i<spatial.Length;i++){spatial[i]=Source(false);spatial[i].spatialBlend=1;spatial[i].rolloffMode=AudioRolloffMode.Linear;spatial[i].minDistance=1;spatial[i].maxDistance=24;spatial[i].dopplerLevel=0;filters[i]=spatial[i].gameObject.AddComponent<AudioLowPassFilter>();filters[i].cutoffFrequency=18000;}
            enemyVoice=Source(false);enemyVoice.spatialBlend=1;enemyVoice.rolloffMode=AudioRolloffMode.Linear;enemyVoice.minDistance=1.5f;enemyVoice.maxDistance=26;enemyVoice.dopplerLevel=0;enemyFilter=enemyVoice.gameObject.AddComponent<AudioLowPassFilter>();
            ambience=Source(true);previous=Source(true);music=Source(true);saveRoomSource=Source(true);exploreSource=Source(true);fx=Source(false);scoreSting=Source(false);radioStatic=Source(true);
            scoreSting.clip=Resources.Load<AudioClip>("Audio/ThreatSting");scoreSting.priority=50;
            radioStatic.clip=Resources.Load<AudioClip>("Audio/RadioStatic");radioStatic.priority=60;radioStatic.volume=0;
            if(radioStatic.clip!=null)radioStatic.Play();
            for(int i=0;i<scoreLayers.Length;i++)
            {scoreLayers[i]=Source(true);scoreLayers[i].name=scoreNames[i];scoreLayers[i].clip=Resources.Load<AudioClip>("Audio/"+scoreNames[i]);scoreLayers[i].priority=70;}

            music.clip=Resources.Load<AudioClip>("Audio/SilentPromiseSH")??Resources.Load<AudioClip>("Audio/Theme");
            if(music.clip!=null){music.Play();music.volume=.15f;}
            saveRoomSource.clip=Resources.Load<AudioClip>("Audio/SaveRoomRE")??Resources.Load<AudioClip>("Audio/Theme");
            if(saveRoomSource.clip!=null){saveRoomSource.Play();saveRoomSource.volume=0f;}
            exploreSource.clip=Resources.Load<AudioClip>("Audio/HallwayExploreRE");
            if(exploreSource.clip!=null){exploreSource.Play();exploreSource.volume=0f;}
        }
        AudioSource Source(bool loop){GameObject child=new GameObject("Audio source");child.transform.SetParent(transform,false);AudioSource s=child.AddComponent<AudioSource>();s.loop=loop;s.playOnAwake=false;s.spatialBlend=0;s.volume=0;return s;}
        public void Play(string name,float volume=.45f){if(fx==null)return;AudioClip c=Resources.Load<AudioClip>("Audio/"+name);if(c!=null){fx.volume=1;fx.PlayOneShot(c,volume);}}
        public void PlayUIClick(){Play("REMenuClick",.32f);}
        public void PlayUIConfirm(){Play("REMenuConfirm",.38f);}
        public void PlayMapToggle(){Play("MapOpenSH",.38f);}
        public void PlayTypewriterSave(){Play("TypewriterKeyRE",.36f);}
        public void PlayItemPickup(){Play("ItemPickupRE",.40f);}
        public void PlayClueSting(){Play("MandolinStingSH",.36f);}
        public void PlayAt(string name,Vector3 position,float volume=.3f)
        {
            AudioClip clip=Resources.Load<AudioClip>("Audio/"+name);if(clip==null||spatial[0]==null)return;
            AudioSource source=spatial[voice];voice=(voice+1)%spatial.Length;source.Stop();source.transform.position=position;source.volume=volume;source.clip=clip;source.Play();
        }
        public void PlayHorrorAt(string name,Vector3 position,float volume=.3f)
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Playing)return;PlayAt(name,position,volume*g.HorrorVolume);
        }
        public void PlayEnemyVoice(string name,float volume=.4f)
        {
            GameSession g=GameSession.Instance;if(enemyVoice==null||g==null||!g.Playing||!g.Enemy.Active)return;
            AudioClip clip=Resources.Load<AudioClip>("Audio/"+name);if(clip==null)return;
            enemyVoice.Stop();enemyVoiceVolume=volume;enemyVoice.transform.position=g.Enemy.transform.position+Vector3.up*1.2f;
            enemyVoice.clip=clip;enemyVoice.volume=volume*g.HorrorVolume;enemyVoice.pitch=Random.Range(.985f,1.015f);enemyVoice.Play();
        }
        public void PlayEnemyCue(bool hunting,float volume=.55f)
        {
            string[] choices=hunting?huntVoices:quietVoices;int last=hunting?lastHunt:lastQuiet;
            int pick=Random.Range(0,choices.Length-(last>=0?1:0));if(last>=0&&pick>=last)pick++;
            if(hunting)lastHunt=pick;else lastQuiet=pick;
            PlayEnemyVoice(choices[pick],volume);
        }
        public void StopEnemyVoice(){EnemyVoiceLevel=0;if(enemyVoice!=null&&enemyVoice.clip!=null){enemyVoice.Stop();enemyVoice.clip=null;}}
        public void ResetTransient(){StopEnemyVoice();StopScore();if(radioStatic!=null)radioStatic.volume=0;ambientClock=14;sirenCooldown=0;lastScare=lastQuiet=lastHunt=-1;foreach(AudioSource source in spatial)if(source!=null)source.Stop();if(fx!=null)fx.Stop();}
        public void Zone(int value)
        {
            if(value==zone)return;zone=value;
            AudioSource tmp=previous;previous=ambience;previousVolume=previous.volume;ambience=tmp;ambience.Stop();ambience.clip=Resources.Load<AudioClip>("Audio/"+(zone==0?"Wind":zone>=5?"Basement":"Hospital"));ambience.volume=0;ambience.Play();zoneFade=0;
            if(exploreSource!=null)
            {
                string targetTrack=zone>=4?"OtherworldIndustrialSH":"HallwayExploreRE";
                AudioClip nextExplore=Resources.Load<AudioClip>("Audio/"+targetTrack);
                if(nextExplore!=null&&exploreSource.clip!=nextExplore)
                {
                    exploreSource.clip=nextExplore;exploreSource.Play();
                }
            }
        }
        void Update()
        {
            GameSession g=GameSession.Instance;if(g==null||ambience==null)return;
            zoneFade=Mathf.MoveTowards(zoneFade,1,Time.unscaledDeltaTime/1.5f);ambience.volume=.24f*zoneFade;previous.volume=previousVolume*(1-zoneFade);
            foreach(AudioSource source in spatial)if(source!=null){if(!g.Playing)source.Pause();else source.UnPause();}
            if(radioStatic!=null)
            {
                float targetStatic=g.Playing&&g.Enemy!=null&&g.Enemy.Active?g.Enemy.InterferenceLevel*.30f*g.HorrorVolume:0f;
                radioStatic.volume=Mathf.MoveTowards(radioStatic.volume,targetStatic,Time.unscaledDeltaTime*1.8f);
            }
            if(g.Playing&&g.Data.zone>=1&&!g.Enemy.Active)
            {
                ambientClock-=Time.deltaTime;
                if(ambientClock<=0)
                {
                    ambientClock=Random.Range(14f,26f);int pick=Random.Range(0,scares.Length-1);if(pick>=lastScare&&lastScare>=0)pick++;
                    lastScare=pick;PlayHorrorAt(scares[pick],new Vector3(Random.value>.5f?8:-8,1.5f,g.Data.zone*14+6),.34f);
                }
            }
            if(g.Playing)for(int i=0;i<spatial.Length;i++)if(spatial[i].isPlaying)
            {bool blocked=Physics.Linecast(g.Player.view.transform.position,spatial[i].transform.position,1<<8,QueryTriggerInteraction.Ignore);filters[i].cutoffFrequency=Mathf.Lerp(filters[i].cutoffFrequency,blocked?1400:18000,Time.deltaTime*5);}
            if(enemyVoice!=null)
            {
                if(!g.Enemy.Active||g.State==SessionState.Dead||g.State==SessionState.Menu||g.State==SessionState.Ending)StopEnemyVoice();
                else if(!g.Playing)enemyVoice.Pause();
                else
                {
                    enemyVoice.UnPause();enemyVoice.transform.position=g.Enemy.transform.position+Vector3.up*1.2f;
                    enemyVoice.volume=enemyVoiceVolume*g.HorrorVolume;
                    float energy=0;
                    if(enemyVoice.isPlaying)
                    {
                        enemyVoice.GetOutputData(vocalSamples,0);
                        for(int sample=0;sample<vocalSamples.Length;sample++)energy+=vocalSamples[sample]*vocalSamples[sample];
                    }
                    EnemyVoiceLevel=Mathf.Lerp(EnemyVoiceLevel,Mathf.Clamp01(Mathf.Sqrt(energy/vocalSamples.Length)*12),1-Mathf.Exp(-Time.deltaTime*18));
                    bool blocked=Physics.Linecast(g.Player.view.transform.position,enemyVoice.transform.position,1<<8,QueryTriggerInteraction.Ignore);
                    enemyFilter.cutoffFrequency=Mathf.Lerp(enemyFilter.cutoffFrequency,blocked?1400:18000,Time.deltaTime*5);
                }
            }
            UpdateScore(g);
        }

        public void AttackImpact()
        {
            GameSession g=GameSession.Instance;if(g!=null&&g.Playing)Play("AttackHit",.45f*g.HorrorVolume);
        }
        void StartScore()
        {
            scoreStart=AudioSettings.dspTime+.12;
            foreach(AudioSource layer in scoreLayers)if(layer!=null&&layer.clip!=null)
            {layer.Stop();layer.timeSamples=0;layer.volume=0;layer.PlayScheduled(scoreStart);}
            scoreRunning=true;scorePaused=false;
        }
        void StopScore()
        {
            foreach(AudioSource layer in scoreLayers)if(layer!=null){layer.Stop();layer.volume=0;}
            if(scoreSting!=null)scoreSting.Stop();scoreRunning=scorePaused=huntCueLatched=false;threat=stingCooldown=0;
        }
        static float Ramp(float a,float b,float value)
        {float t=Mathf.InverseLerp(a,b,value);return t*t*(3-2*t);}
        void UpdateScore(GameSession g)
        {
            bool inMenuOrEnding=g.State==SessionState.Menu||g.State==SessionState.Ending;
            bool inSaveRoomUI=g.State==SessionState.Paused||(g.State==SessionState.Playing&&(g.Panel=="inventory"||g.Panel=="map"||g.Panel=="inspect"||g.Panel=="document"||g.Panel=="options"));
            float targetMenuTheme=inMenuOrEnding?.16f*g.MusicVolume:0f;
            float targetSaveRoom=inSaveRoomUI?.13f*g.MusicVolume:0f;
            music.volume=Mathf.MoveTowards(music.volume,targetMenuTheme,Time.unscaledDeltaTime*.9f);
            if(saveRoomSource!=null)saveRoomSource.volume=Mathf.MoveTowards(saveRoomSource.volume,targetSaveRoom,Time.unscaledDeltaTime*1.1f);
            scoreSting.volume=.40f*g.MusicVolume;
            if(g.State==SessionState.Menu||g.State==SessionState.Dead||g.State==SessionState.Ending)
            {
                if(exploreSource!=null)exploreSource.volume=Mathf.MoveTowards(exploreSource.volume,0f,Time.unscaledDeltaTime*1.5f);
                if(scoreRunning)StopScore();return;
            }
            if(!g.Playing)
            {
                if(exploreSource!=null)exploreSource.volume=Mathf.MoveTowards(exploreSource.volume,.03f*g.MusicVolume,Time.unscaledDeltaTime*1.2f);
                if(scoreRunning&&!scorePaused)
                {
                    if(AudioSettings.dspTime<scoreStart){foreach(AudioSource layer in scoreLayers)layer.Stop();scoreRunning=false;}
                    else {foreach(AudioSource layer in scoreLayers)layer.Pause();scorePaused=true;}
                }
                scoreSting.Pause();return;
            }
            if(!scoreRunning)StartScore();
            else if(scorePaused){foreach(AudioSource layer in scoreLayers)layer.UnPause();scorePaused=false;}
            scoreSting.UnPause();
            float target=0;bool hunt=g.Enemy.Active&&g.Enemy.Mode=="Perseguir";
            if(g.Enemy.Active)
            {
                float proximity=Mathf.Clamp01((12-g.Enemy.TargetDistance)/10);
                target=hunt?(g.Enemy.SeesTarget?.62f+.38f*proximity:.42f):.10f+.18f*proximity;
                if(g.Enemy.Attack.Active)target=1;
            }
            threat=Mathf.MoveTowards(threat,target,Time.deltaTime*(target>threat?1.5f:.13f));
            float volume=g.MusicVolume;
            if(exploreSource!=null)
            {
                float exploreTarget=Mathf.Clamp01(1f-threat*1.8f)*.12f*volume;
                exploreSource.volume=Mathf.MoveTowards(exploreSource.volume,exploreTarget,Time.deltaTime*.9f);
            }
            scoreLayers[0].volume=.15f*Mathf.Clamp01(threat*3)*volume;
            scoreLayers[1].volume=.25f*Ramp(.30f,.80f,threat)*volume;
            scoreLayers[2].volume=.18f*Ramp(.75f,1,threat)*volume;
            stingCooldown=Mathf.Max(0,stingCooldown-Time.deltaTime);
            sirenCooldown=Mathf.Max(0,sirenCooldown-Time.deltaTime);
            bool signal=hunt&&(g.Enemy.SeesTarget||g.Enemy.Alerting||g.Enemy.Attack.Active);
            if(signal&&!huntCueLatched&&stingCooldown<=0)
            {
                scoreSting.Play();stingCooldown=10;
                if(sirenCooldown<=0){Play("SirenWarning",.28f*g.HorrorVolume);sirenCooldown=22f;}
            }
            huntCueLatched=signal;
        }
    }
}
