using UnityEngine;
namespace Ecos
{
    public class GameUI : MonoBehaviour
    {
        GUIStyle title,heading,body,small,button,tabButton,badgeStyle,screamStyle;Texture2D atlas;Vector2 inventoryScroll,documentScroll,optionsScroll;string selected="welcome";bool ready;
        readonly string[] memories={
            "I · EL HOSPITAL — Tomás entra al Roy H. Glover. La penumbra deja ver rastros de sangre y abandono; sube la videocámara (Clic Der. + Q) para ver a gran distancia.",
            "LA LLAVE DEL RECUERDO — La enfermera ocultó la llave detrás del bastidor. La puerta hacia Neonatología se abre.",
            "II · EL NIÑO — Sebastián repite los colores del dibujo: sol, cielo y luz. El acceso a Urgencias se desbloquea.",
            "III · LA VERDAD — Dos nombres fueron separados del informe de 1985. El acceso al Subterráneo G queda abierto.",
            "IV · SEBASTIÁN — En el cajón sellado aparece la llave de conservación. Las puertas de la Morgue se abren.",
            "V · JUNTOS — Las dos camillas descansan por fin una junto a la otra. Vuelve a la grabadora para cerrar la historia."
        };
        void Init()
        {
            if(ready)return;ready=true;atlas=Resources.Load<Texture2D>("Art/Recuerdos");
            title=new GUIStyle(GUI.skin.label){fontSize=44,fontStyle=FontStyle.Bold,wordWrap=true};title.normal.textColor=new Color(.90f,.86f,.76f);
            heading=new GUIStyle(title){fontSize=26};heading.normal.textColor=new Color(.86f,.24f,.20f);
            body=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};body.normal.textColor=new Color(.84f,.85f,.82f);
            small=new GUIStyle(body){fontSize=14};small.normal.textColor=new Color(.68f,.72f,.69f);
            badgeStyle=new GUIStyle(small){fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};badgeStyle.normal.textColor=new Color(.95f,.88f,.62f);
            screamStyle=new GUIStyle(title){fontSize=58,alignment=TextAnchor.MiddleCenter};screamStyle.normal.textColor=new Color(.95f,.12f,.10f);
            button=new GUIStyle(GUI.skin.button){fontSize=17,fixedHeight=40,wordWrap=true,fontStyle=FontStyle.Bold};
            button.normal.textColor=new Color(.90f,.88f,.82f);
            tabButton=new GUIStyle(button){fixedHeight=36,fontSize=15};
        }
        void Fill(Rect r,Color color){Color old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void OutlineBox(Rect r,Color fill,Color border,float thickness=1.5f)
        {
            Fill(r,fill);
            Fill(new Rect(r.x,r.y,r.width,thickness),border);
            Fill(new Rect(r.x,r.yMax-thickness,r.width,thickness),border);
            Fill(new Rect(r.x,r.y,thickness,r.height),border);
            Fill(new Rect(r.xMax-thickness,r.y,thickness,r.height),border);
        }
        void Backdrop(float alpha=.96f)
        {
            Fill(new Rect(0,0,1280,720),new Color(.018f,.020f,.022f,alpha));
            Fill(new Rect(0,0,1280,3),new Color(.68f,.14f,.12f,.9f));
            Fill(new Rect(0,717,1280,3),new Color(.68f,.14f,.12f,.9f));
        }
        void DrawOutlastDarknessOverlay(GameSession g)
        {
            if(g.Player==null||g.Player.NightVisionActive)return;
            // Viñeta periférica sutil en modo sin cámara: permite ver claramente la iluminación, sombras,
            // texturas realistas y rastros de sangre cercanos sin oscurecer el centro de la pantalla.
            float edgeAlpha=Mathf.Lerp(.22f,.08f,g.Brightness);
            Fill(new Rect(0,0,1280,54),new Color(0,0,0,edgeAlpha));
            Fill(new Rect(0,666,1280,54),new Color(0,0,0,edgeAlpha));
            Fill(new Rect(0,54,75,612),new Color(0,0,0,edgeAlpha*.85f));
            Fill(new Rect(1205,54,75,612),new Color(0,0,0,edgeAlpha*.85f));
        }
        void DrawHubTopBar(GameSession g,string activeTab)
        {
            OutlineBox(new Rect(40,20,1200,48),new Color(.04f,.045f,.05f,.95f),new Color(.35f,.14f,.14f,.9f),1.5f);
            GUILayout.BeginArea(new Rect(52,26,1176,38));
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(activeTab=="map"?"● [ M ] MAPA TÁCTICO":"[ M ] MAPA TÁCTICO",tabButton,GUILayout.Width(240))){g.Audio.PlayMapToggle();g.SelectedMapZone=g.Data.zone;g.OpenPanel("map");GUIUtility.ExitGUI();}
            if(GUILayout.Button(activeTab=="inventory"?"● [ TAB ] MALETÍN Y ARCHIVOS":"[ TAB ] MALETÍN Y ARCHIVOS",tabButton,GUILayout.Width(270))){g.Audio.PlayUIClick();g.OpenPanel("inventory");GUIUtility.ExitGUI();}
            if(GUILayout.Button(activeTab=="inspect"?"● [ F ] INSPECTOR 3D":"[ F ] INSPECTOR 3D",tabButton,GUILayout.Width(230))){g.OpenInspect(selected);GUIUtility.ExitGUI();}
            if(GUILayout.Button(activeTab=="help"?"● [ H ] SUPERVIVENCIA":"[ H ] SUPERVIVENCIA",tabButton,GUILayout.Width(210))){g.Audio.PlayUIClick();g.OpenPanel("help");GUIUtility.ExitGUI();}
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("VOLVER [ ESC ]",tabButton,GUILayout.Width(170))){g.Audio.PlayUIClick();g.ClosePanel();GUIUtility.ExitGUI();}
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        void Art(Rect r,int index)
        {
            if(atlas==null)return;int col=index%3,row=index/3;
            GUI.DrawTextureWithTexCoords(r,atlas,new Rect(col/3f,1-(row+1)/2f,1/3f,1/2f));
        }
        bool Button(string text){bool clicked=GUILayout.Button(text,button);if(clicked&&GameSession.Instance!=null&&GameSession.Instance.Audio!=null)GameSession.Instance.Audio.PlayUIClick();return clicked;}
        void Icon(Rect r,int kind)
        {
            OutlineBox(r,new Color(.08f,.10f,.11f),new Color(.48f,.18f,.16f),1f);
            Color color=new Color(.86f,.76f,.52f);
            if(kind==1){Fill(new Rect(r.x+8,r.y+8,12,12),color);Fill(new Rect(r.x+18,r.y+14,18,4),color);Fill(new Rect(r.x+30,r.y+17,4,8),color);}
            else if(kind==3){Fill(new Rect(r.x+7,r.y+12,29,20),color);Fill(new Rect(r.x+17,r.y+16,11,11),Color.black);Fill(new Rect(r.x+9,r.y+8,11,5),color);}
            else{Fill(new Rect(r.x+12,r.y+7,21,29),color);for(int i=0;i<3;i++)Fill(new Rect(r.x+16,r.y+13+i*6,13,2),new Color(.1f,.16f,.13f));}
        }
        void OnGUI()
        {
            Matrix4x4 previousMatrix=GUI.matrix;bool previousEnabled=GUI.enabled;
            try{DrawGUI();}finally{GUI.matrix=previousMatrix;GUI.enabled=previousEnabled;}
        }
        void DrawGUI()
        {
            GameSession g=GameSession.Instance;if(g==null||!g.Ready)return;Init();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
            if(g.Panel!=""){Panel(g);Toast(g);return;}
            if(g.State==SessionState.Menu)
            {
                Backdrop(1f);
                OutlineBox(new Rect(675,60,545,595),new Color(.03f,.03f,.035f),new Color(.55f,.14f,.12f),2f);
                Art(new Rect(685,70,525,525),0);
                GUI.Label(new Rect(695,605,505,40),"ARCHIVO CLASIFICADO #1985 · ROY H. GLOVER",small);
                GUILayout.BeginArea(new Rect(65,60,560,600));
                GUILayout.Label("BIOHAZARD FILE // 1985",small);
                GUILayout.Label("ECOS DE\nCHUQUICAMATA",title);
                Fill(new Rect(0,148,460,3),new Color(.75f,.14f,.12f));
                GUILayout.Space(18);
                GUILayout.Label("SURVIVAL HORROR EN PRIMERA PERSONA",heading);
                GUILayout.Space(8);
                GUILayout.Label("Explora el Hospital Roy H. Glover entre luces dramáticas, sombras suaves, rastros de sangre y banda sonora inspirada en Resident Evil y Silent Hill. Usa la cámara IR a larga distancia y para ralentizar a Sebastián.",body);
                GUILayout.Space(24);
                if(Button("▸  NUEVA PARTIDA")){if(SaveService.Read()!=null)g.OpenPanel("newgame");else g.NewGame();GUIUtility.ExitGUI();}
                GUI.enabled=SaveService.Read()!=null;
                if(Button("▸  CONTINUAR PARTIDA (CHECKPOINT)")){g.Continue();GUIUtility.ExitGUI();}
                GUI.enabled=true;
                if(Button("▸  CONFIGURACIÓN Y AUDIO")){g.Options();GUIUtility.ExitGUI();}
                if(Button("▸  SALIR AL ESCRITORIO")){g.Quit();GUIUtility.ExitGUI();}
                GUILayout.Space(22);
                OutlineBox(new Rect(0,460,550,115),new Color(.04f,.045f,.05f,.9f),new Color(.30f,.12f,.12f),1f);
                GUI.Label(new Rect(14,470,522,95),"PENTACODERS · Edición Survival Horror\n• [Modo sin cámara mejorado]: Sombras dinámicas, reflejos húmedos en sangre y penumbra visible\n• [Clic Der. + Q]: Visión nocturna IR de 32m + Ralentizar a Sebastián\n• Música y SFX de Resident Evil (Save Room, R.P.D.) y Silent Hill (Promise, Otherworld)",small);
                GUILayout.EndArea();Toast(g);return;
            }
            if(g.State==SessionState.Paused)
            {
                Backdrop(.94f);
                OutlineBox(new Rect(350,95,580,520),new Color(.03f,.035f,.04f,.98f),new Color(.68f,.16f,.14f),2f);
                GUILayout.BeginArea(new Rect(385,120,510,470));
                GUILayout.Label("PARTIDA EN PAUSA",title);
                GUILayout.Label("SECTOR ACTUAL: "+StoryData.Zones[g.Data.zone],heading);
                GUILayout.Space(18);
                if(Button("▸  REANUDAR EXPEDICIÓN")){g.Resume();GUIUtility.ExitGUI();}
                if(Button("▸  ABRIR MAPA TÁCTICO [ M ]")){g.Resume();g.SelectedMapZone=g.Data.zone;g.OpenPanel("map");GUIUtility.ExitGUI();}
                if(Button("▸  MALETÍN E INVENTARIO [ TAB ]")){g.Resume();g.OpenPanel("inventory");GUIUtility.ExitGUI();}
                if(Button("▸  OPCIONES DE SISTEMA")){g.Options();GUIUtility.ExitGUI();}
                if(Button("▸  VOLVER AL MENÚ PRINCIPAL")){g.Menu();GUIUtility.ExitGUI();}
                GUILayout.Space(16);
                GUILayout.Label("Los checkpoints se registran automáticamente al avanzar de sector o desbloquear accesos.",small);
                GUILayout.EndArea();return;
            }
            if(g.State==SessionState.Dead)
            {
                float t=g.DeathJumpscareElapsed;
                float flash=Mathf.Abs(Mathf.Sin(t*38f));
                Fill(new Rect(0,0,1280,28),new Color(.65f,.04f,.04f,.45f+.35f*flash));
                Fill(new Rect(0,692,1280,28),new Color(.65f,.04f,.04f,.45f+.35f*flash));
                for(int s=0;s<14;s++)
                {
                    float sy=Mathf.Repeat(t*420f+s*55f,720f);
                    Fill(new Rect(0,sy,1280,2),new Color(.85f,.08f,.08f,.18f));
                }
                if(t<2.15f)
                {
                    float jx=Mathf.Sin(t*75f)*14f,jy=Mathf.Cos(t*63f)*10f;
                    GUI.Label(new Rect(240+jx,580+jy,800,90),"¡EL ECO TE ALCANZÓ!",screamStyle);
                    return;
                }
                OutlineBox(new Rect(690,395,540,285),new Color(.02f,.015f,.018f,.90f),new Color(.82f,.14f,.12f,.95f),2f);
                GUILayout.BeginArea(new Rect(715,412,490,255));
                GUILayout.Label("HAS MUERTO",title);
                GUILayout.Space(6);GUILayout.Label(g.Result,body);GUILayout.Space(14);
                GUI.enabled=SaveService.Read()!=null;
                if(Button("▸  REINTENTAR DESDE CHECKPOINT")){g.Continue();GUIUtility.ExitGUI();}
                GUI.enabled=true;
                if(Button("▸  VOLVER AL MENÚ PRINCIPAL")){g.Menu();GUIUtility.ExitGUI();}
                GUILayout.EndArea();return;
            }
            if(g.State==SessionState.Ending)
            {
                float t=g.EndingCinematicElapsed;
                Fill(new Rect(0,0,1280,86),new Color(.01f,.012f,.015f,.92f));
                Fill(new Rect(0,614,1280,106),new Color(.01f,.012f,.015f,.92f));
                Fill(new Rect(0,84,1280,2),new Color(.75f,.62f,.32f,.85f));
                Fill(new Rect(0,614,1280,2),new Color(.75f,.62f,.32f,.85f));
                string shotTitle=t<4.5f?"CINEMÁTICA FINAL 3D · I. EL REENCUENTRO EN LA MORGUE":
                                 t<9.0f?"CINEMÁTICA FINAL 3D · II. LA DESPEDIDA DE SEBASTIÁN":
                                 "CINEMÁTICA FINAL 3D · III. EL ECO SE APAGA AL AMANECER";
                string shotSubtitle=t<4.5f?"Sebastián contempla la camilla de su madre por fin reunida junto a la suya y extiende su mano en silencio…":
                                    t<9.0f?"El niño gira el rostro hacia Tomás sin dolor ni ira, agradeciendo que alguien escuchara su historia…":
                                    "Su presencia espectral se disuelve lentamente en la luz mientras el hospital Roy H. Glover queda en paz.";
                GUI.Label(new Rect(42,26,950,32),shotTitle,heading);
                GUI.Label(new Rect(42,630,1180,60),shotSubtitle,body);
                if(t>=9.5f)
                {
                    OutlineBox(new Rect(690,115,545,470),new Color(.02f,.025f,.03f,.90f),new Color(.75f,.62f,.32f,.9f),2f);
                    GUILayout.BeginArea(new Rect(715,135,495,430));
                    GUILayout.Label("EPÍLOGO · 1985",small);
                    GUILayout.Label(g.EndingTruthChosen?"FINAL VERDADERO: JUNTOS":"FINAL ALTERNATIVO: EL SILENCIO",heading);
                    GUILayout.Space(12);
                    GUILayout.Label(g.Result,body);
                    GUILayout.FlexibleSpace();
                    if(Button("▸  VOLVER AL MENÚ PRINCIPAL")){g.Menu();GUIUtility.ExitGUI();}
                    GUILayout.EndArea();
                }
                return;
            }
            if(g.DoorCutsceneActive)
            {
                Fill(new Rect(0,0,1280,92),new Color(.01f,.012f,.015f,.94f));
                Fill(new Rect(0,618,1280,102),new Color(.01f,.012f,.015f,.94f));
                Fill(new Rect(0,90,1280,2),new Color(.75f,.18f,.14f,.9f));
                Fill(new Rect(0,618,1280,2),new Color(.75f,.18f,.14f,.9f));
                GUI.Label(new Rect(40,26,900,36),"ACCESO DESBLOQUEADO · ABRIENDO PUERTA EN VIVO",heading);
                GUI.Label(new Rect(40,56,900,28),"Sector habilitado: "+StoryData.Zones[g.DoorCutsceneZone]+"  ·  [ ESPACIO ] Omitir vista",small);
                if(Time.unscaledTime<g.MemorySubtitleUntil)
                    GUI.Label(new Rect(40,636,1200,64),memories[g.MemoryIndex],body);
                return;
            }
            DrawOutlastDarknessOverlay(g);
            if(g.Player!=null&&g.Player.IsHiding)
            {
                if(g.Player.HidingUnderBed)
                {
                    Fill(new Rect(0,0,1280,185),new Color(.015f,.018f,.016f,.97f));
                    for(int fold=0;fold<16;fold++)
                        Fill(new Rect(fold*80,175+(fold%2)*14,82,24),new Color(.025f,.03f,.028f,.94f));
                    Fill(new Rect(0,605,1280,115),new Color(.01f,.012f,.01f,.94f));
                    Fill(new Rect(0,185,95,420),new Color(.015f,.018f,.018f,.95f));
                    Fill(new Rect(1185,185,95,420),new Color(.015f,.018f,.018f,.95f));
                    GUI.Label(new Rect(360,635,680,35),"[ E ]  Salir de debajo de la cama · [ Clic Der. + Q ] Visión nocturna",body);
                }
                else
                {
                    Fill(new Rect(0,0,1280,145),new Color(.01f,.015f,.012f,.94f));
                    Fill(new Rect(0,550,1280,170),new Color(.01f,.015f,.012f,.94f));
                    Fill(new Rect(0,145,190,405),new Color(.01f,.015f,.012f,.94f));
                    Fill(new Rect(1090,145,190,405),new Color(.01f,.015f,.012f,.94f));
                    for(int slat=0;slat<4;slat++)Fill(new Rect(190,210+slat*85,900,26),new Color(.015f,.02f,.018f,.88f));
                    GUI.Label(new Rect(390,585,620,35),"[ E ]  Salir del casillero · Conteniendo la respiración",body);
                }
            }
            if(g.Enemy!=null&&g.Enemy.InterferenceLevel>.08f)
            {
                float alpha=g.Enemy.InterferenceLevel*.22f*Mathf.Abs(Mathf.Sin(Time.unscaledTime*31f));
                Fill(new Rect(0,0,1280,720),new Color(.16f,.02f,.02f,alpha));
            }
            DrawResidentEvilHUD(g);
            if(g.Data.battery<=0)GUI.Label(new Rect(340,590,650,30),"SIN BATERÍA IR · [ R ] Recargar ("+g.Data.spareBatteries+" pilas)",body);
            else if(g.Player!=null&&!g.Player.NightVisionActive&&!g.Player.IsHiding)
                GUI.Label(new Rect(355,648,640,28),"MODO SIN CÁMARA (PENUMBRA) · [ CLIC DER. + Q ] Visión Nocturna IR de Largo Alcance",badgeStyle);
            if(g.Player==null||!g.Player.IsHiding)
            {
                Fill(new Rect(639,355,2,10),new Color(.85f,.85f,.8f,.55f));
                Fill(new Rect(635,359,10,2),new Color(.85f,.85f,.8f,.55f));
            }
            if(g.Player!=null&&g.Player.Aiming)
            {
                Color c=g.Player.NightVisionActive?new Color(.35f,.92f,.55f,.85f):new Color(.86f,.78f,.62f,.8f);
                Fill(new Rect(240,130,60,2),c);Fill(new Rect(240,130,2,60),c);
                Fill(new Rect(980,130,60,2),c);Fill(new Rect(1038,130,2,60),c);
                Fill(new Rect(240,570,60,2),c);Fill(new Rect(240,510,2,60),c);
                Fill(new Rect(980,570,60,2),c);Fill(new Rect(1038,510,2,60),c);
                Fill(new Rect(280,152,10,10),new Color(.88f,.16f,.14f,Mathf.Abs(Mathf.Sin(Time.unscaledTime*4f))));
                string camStatus=g.Player.NightVisionActive?"REC // VISIÓN NOCTURNA IR (OUTLAST MODE)":"REC // ÓPTICA SIN IR (PULSA Q PARA ACTIVAR IR)";
                if(g.Enemy!=null&&g.Enemy.InterferenceLevel>.2f)camStatus+=" · ¡RADIOESTÁTICA!";
                GUI.Label(new Rect(298,145,680,30),camStatus,body);
                if(g.Enemy!=null&&g.Enemy.CameraFocusLevel>.05f)
                {
                    int slowPct=Mathf.RoundToInt((1f-g.Enemy.SlowdownFactor)*100f);
                    Color lockCol=new Color(.95f,.78f,.22f,.95f);
                    OutlineBox(new Rect(390,485,500,42),new Color(.03f,.05f,.04f,.92f),lockCol,1.5f);
                    GUI.Label(new Rect(405,490,470,22),"⚡ ENFOQUE IR SOBRE SEBASTIÁN · RALENTIZADO -"+slowPct+"%",badgeStyle);
                    Meter(new Rect(410,514,460,6),g.Enemy.CameraFocusLevel,lockCol);
                }
                GUI.Label(new Rect(260,540,760,30),"[Q] Visión Nocturna · [Apuntar a Sebastián] Ralentizarlo · [Clic Izq.] Zoom · [R] Recargar",small);
            }
            if(g.Player!=null&&!g.Player.IsHiding&&g.Player.Target!=null)
            {
                OutlineBox(new Rect(370,552,540,42),new Color(.02f,.025f,.03f,.92f),new Color(.75f,.20f,.16f,.9f),1.5f);
                GUI.Label(new Rect(388,560,510,30),"[ E ]  "+g.Player.Target.Prompt,body);
            }
            if(Time.unscaledTime<g.MemorySubtitleUntil)
            {
                OutlineBox(new Rect(140,600,1000,56),new Color(.02f,.022f,.025f,.94f),new Color(.68f,.55f,.28f,.85f),1.5f);
                GUI.Label(new Rect(158,608,964,44),memories[g.MemoryIndex],small);
            }
            if(!g.MinimalHUD)
            {
                Fill(new Rect(0,682,1280,38),new Color(.015f,.018f,.02f,.9f));
                GUI.Label(new Rect(24,690,1240,26),"[Clic Der. + Q] Cámara IR (Ver lejos y ralentizar a Sebastián) · [E] Interactuar / Esconderse · [M] Mapa RE · [Tab] Maletín · [F] Inspector 3D",small);
            }
            Toast(g);
        }
        void Meter(Rect r,float value,Color color)
        {
            Fill(r,new Color(.10f,.12f,.13f,.95f));Fill(new Rect(r.x,r.y,r.width*Mathf.Clamp01(value),r.height),color);
        }
        void DrawECGMonitor(Rect r,float panic,float anger)
        {
            bool danger=panic>.6f||anger>=70f;
            bool caution=!danger&&(panic>.22f||anger>=35f);
            Color ecgColor=danger?new Color(.92f,.18f,.16f):caution?new Color(.92f,.68f,.18f):new Color(.25f,.85f,.42f);
            string stateLabel=danger?"DANGER":caution?"CAUTION":"FINE";
            OutlineBox(r,new Color(.02f,.025f,.028f,.92f),ecgColor,1.5f);
            float speed=danger?9f:caution?5.5f:3.2f;
            float phase=Time.unscaledTime*speed;
            int segments=28;float segW=(r.width-95f)/segments;
            for(int i=0;i<segments;i++)
            {
                float u=(i/(float)segments)*Mathf.PI*2f+phase;
                float m=Mathf.Repeat(u,Mathf.PI*2f);
                float pulse=m>.8f&&m<1.1f?Mathf.Sin((m-.8f)/.3f*Mathf.PI*2f)*14f:Mathf.Sin(u)*2f;
                float y=r.y+r.height*.5f-pulse;
                Fill(new Rect(r.x+10+i*segW,Mathf.Clamp(y,r.y+4,r.yMax-6),segW+1,2.5f),ecgColor);
            }
            GUI.Label(new Rect(r.xMax-80,r.y+8,74,24),stateLabel,badgeStyle);
        }
        void DrawResidentEvilHUD(GameSession g)
        {
            if(!g.MinimalHUD)
            {
                OutlineBox(new Rect(24,22,560,64),new Color(.02f,.025f,.03f,.88f),new Color(.55f,.16f,.14f,.85f),1.5f);
                GUI.Label(new Rect(38,28,535,24),"OBJETIVO ACTUAL  ·  [ M ] Abrir Mapa del Hospital",badgeStyle);
                GUI.Label(new Rect(38,50,535,30),g.Objective(),small);
            }
            bool showRight=!g.MinimalHUD||g.Player.Aiming||g.Data.battery<=20||g.Player.Stamina<.98f||g.Player.PanicLevel>.15f;
            if(showRight)
            {
                OutlineBox(new Rect(980,22,276,158),new Color(.02f,.025f,.03f,.90f),new Color(.35f,.38f,.40f,.8f),1.5f);
                DrawECGMonitor(new Rect(994,32,248,38),g.Player!=null?g.Player.PanicLevel:0f,g.Data.anger);
                GUI.Label(new Rect(996,76,245,22),"BATERÍA IR: "+Mathf.CeilToInt(g.Data.battery)+"%  |  PILAS: "+g.Data.spareBatteries,small);
                Meter(new Rect(996,98,244,6),g.Data.battery/100f,g.Data.battery<=20?new Color(.88f,.25f,.20f):new Color(.32f,.82f,.52f));
                string hideStatus=g.Player.IsHiding?(g.Player.HidingUnderBed?"BAJO CAMA":"CASILLERO"):g.Player.Crouching?"SIGILO":g.Player.Running?"CORRIENDO":"NORMAL";
                GUI.Label(new Rect(996,110,245,22),"RESISTENCIA  |  "+hideStatus,small);
                Meter(new Rect(996,132,244,5),g.Player.Stamina,new Color(.55f,.75f,.82f));
                GUI.Label(new Rect(996,144,245,22),"RADIOESTÁTICA / IRA: "+Mathf.RoundToInt(g.Data.anger)+"%",small);
            }
        }
        void DrawMapPanel(GameSession g)
        {
            DrawHubTopBar(g,"map");
            OutlineBox(new Rect(40,80,760,600),new Color(.025f,.032f,.040f,.98f),new Color(.25f,.42f,.52f,.9f),2f);
            for(int gx=80;gx<800;gx+=40)Fill(new Rect(gx,80,1,600),new Color(.12f,.20f,.26f,.25f));
            for(int gy=120;gy<680;gy+=40)Fill(new Rect(40,gy,760,1),new Color(.12f,.20f,.26f,.25f));
            GUI.Label(new Rect(60,92,680,28),"PLANO ARQUITECTÓNICO ASIMÉTRICO · HOSPITAL ROY H. GLOVER",heading);
            Fill(new Rect(60,645,16,16),new Color(.16f,.42f,.58f));GUI.Label(new Rect(82,643,155,22),"Sector Despejado",small);
            Fill(new Rect(245,645,16,16),new Color(.58f,.16f,.16f));GUI.Label(new Rect(267,643,165,22),"Pistas / Sangre activa",small);
            Fill(new Rect(440,645,16,16),new Color(.18f,.20f,.22f));GUI.Label(new Rect(462,643,120,22),"Sin explorar",small);
            Fill(new Rect(590,645,14,14),new Color(.95f,.82f,.25f));GUI.Label(new Rect(610,643,170,22),"▲ Tomás (En tiempo real)",small);

            int[] westOffset={-8, 8, -8, -8, 8, -8, 8, 8};
            int[] eastOffset={-6, -8, 8, 8, -8, 8, -8, 8};
            int[] westWidth={155, 195, 165, 195, 195, 195, 165, 195};
            int[] eastWidth={175, 165, 195, 195, 195, 195, 195, 195};

            for(int z=0;z<8;z++)
            {
                int row=7-z;
                float y=132f+row*62f;
                bool visited=g.Has("visited_"+z)||z<=g.Data.zone;
                bool cleared=g.IsZoneCleared(z);
                bool isCurrent=z==g.Data.zone;
                bool isSelected=z==g.SelectedMapZone;
                Color roomFill=!visited?new Color(.12f,.14f,.16f,.85f):cleared?new Color(.12f,.34f,.48f,.92f):new Color(.48f,.14f,.14f,.92f);
                Color border=isCurrent?new Color(.95f,.82f,.25f):isSelected?new Color(.85f,.85f,.85f):new Color(.32f,.45f,.52f);

                float wW=westWidth[z];
                OutlineBox(new Rect(265-wW,y+6+westOffset[z]*.45f,wW-5,38),roomFill,border,isCurrent?2f:1f);
                OutlineBox(new Rect(265,y,290,50),roomFill,border,isCurrent?2.5f:1.5f);
                float eW=eastWidth[z];
                OutlineBox(new Rect(560,y+6+eastOffset[z]*.45f,eW,38),roomFill,border,isCurrent?2f:1f);

                string zoneLabel=(z+1).ToString("00")+" · "+(visited?StoryData.Zones[z]:"SECTOR DESCONOCIDO");
                if(GUI.Button(new Rect(65,y,695,50),zoneLabel,GUIStyle.none))
                {
                    g.Audio.PlayUIClick();g.SelectedMapZone=z;
                }
                GUI.Label(new Rect(275,y+6,270,22),zoneLabel,badgeStyle);
                string subStatus=!visited?"SIN EXPLORAR":cleared?"✓ SECTOR DESPEJADO":"! ACERTIJO / RASTROS EN SALAS";
                GUI.Label(new Rect(275,y+26,270,20),subStatus,small);
                GUI.Label(new Rect(275-wW,y+15+westOffset[z]*.45f,wW-18,22),visited?"Ala Oeste + Anexo":"---",small);
                GUI.Label(new Rect(570,y+15+eastOffset[z]*.45f,eW-16,22),visited?(g.Has("taken_battery_"+z)?"Ala Este (Limpia)":"Ala Este · ⚡ Pila"):"---",small);

                if(z>0)
                {
                    bool gateUnlocked=z==1||z==4||g.Has("gate_"+z);
                    Color gateCol=gateUnlocked?new Color(.22f,.78f,.38f):new Color(.88f,.20f,.18f);
                    Fill(new Rect(390,y+50,40,12),gateCol);
                }
            }
            if(g.Player!=null)
            {
                Vector3 p=g.Player.transform.position;
                int pz=Mathf.Clamp(Mathf.FloorToInt(p.z/14f),0,7);
                float localZ=Mathf.Clamp01((p.z-pz*14f)/14f);
                float py=132f+(7-pz)*62f+50f-localZ*50f;
                float px=410f+Mathf.Clamp(p.x/16f,-1f,1f)*310f;
                OutlineBox(new Rect(px-9,py-9,18,18),new Color(.95f,.82f,.20f),Color.black,2f);
                GUI.Label(new Rect(px-32,py-26,80,20),"▲ TOMÁS",badgeStyle);
            }

            OutlineBox(new Rect(820,80,420,600),new Color(.03f,.035f,.04f,.98f),new Color(.55f,.16f,.14f,.9f),2f);
            GUILayout.BeginArea(new Rect(842,100,376,560));
            int sz=Mathf.Clamp(g.SelectedMapZone,0,7);
            bool szVisited=g.Has("visited_"+sz)||sz<=g.Data.zone;
            GUILayout.Label(StoryData.Chapters[sz],small);
            GUILayout.Label(szVisited?StoryData.Zones[sz]:"SECTOR NO EXPLORADO",heading);
            Fill(new Rect(0,68,360,2),new Color(.68f,.16f,.14f));
            GUILayout.Space(14);
            string[] doorTypes={
                "Vestíbulo Principal Abierto",
                "Arco Administrativo Abierto",
                "Puerta Pediátrica Brookhaven (Cristal y Madera)",
                "Compuerta Roja de Trauma y Urgencias (Estilo R.P.D.)",
                "Paso de Archivos Clínicos Abierto",
                "Reja Oxidada del Otherworld con Válvula (Estilo Silent Hill)",
                "Compuerta Blindada de Bioseguridad (Estilo Umbrella)",
                "Puerta Acorazada de Cámara Criogénica Mortuoria"
            };
            GUILayout.Label("TIPO DE PUERTA DEL SECTOR:\n• "+doorTypes[sz],small);
            GUILayout.Space(10);
            GUILayout.Label("ESCONDITES Y DEFENSA IR EN EL SECTOR:\n• Camas hospitalarias y casilleros (Pulsa E)\n• Apuntar la cámara IR a Sebastián lo ralentiza hasta un 74%",small);
            GUILayout.Space(10);
            string[] zoneNotes={
                "Vestíbulo con garita este, humedad y bahía de ambulancias colapsada al oeste.",
                "Suite de dirección al noroeste y sala de espera al sureste. Revisa el cuadro.",
                "Pabellón neonatal ampliado al noreste con incubadoras y sala de juegos al suroeste.",
                "Bahía quirúrgica bañada en sangre al suroeste y cuarto oscuro de Rayos X al noreste.",
                "Laboratorio de microscopía al sureste y bóveda de expedientes al noroeste (Código 1985).",
                "Cámara de turbinas oxidada al suroeste y galería de calderas/fusibles al noreste.",
                "Anfiteatro de autopsias con rastros viscerales al sureste y depósito al noroeste.",
                "Cámaras mortuorias gemelas A1 y C2 con rastro sangriento entre las camillas."
            };
            GUILayout.Label("ARQUITECTURA Y PISTAS DEL SECTOR:",badgeStyle);
            GUILayout.Label(szVisited?zoneNotes[sz]:"Avanza hasta este sector para registrar sus alas y anexos en el plano.",body);
            GUILayout.Space(14);
            GUILayout.Label("OBJETIVO GLOBAL EN CURSO:",badgeStyle);
            GUILayout.Label(g.Objective(),body);
            GUILayout.FlexibleSpace();
            if(Button("▸  CERRAR MAPA [ M ]")){g.ClosePanel();GUIUtility.ExitGUI();}
            GUILayout.EndArea();
        }
        void DrawInspectPanel(GameSession g)
        {
            Fill(new Rect(0,0,1280,82),new Color(.015f,.018f,.022f,.95f));
            Fill(new Rect(0,600,1280,120),new Color(.015f,.018f,.022f,.95f));
            DrawHubTopBar(g,"inspect");
            Entry e=StoryData.Get(g.InspectItemId);
            OutlineBox(new Rect(40,95,430,485),new Color(.025f,.03f,.035f,.92f),new Color(.55f,.16f,.14f,.9f),1.5f);
            GUILayout.BeginArea(new Rect(58,112,394,450));
            GUILayout.Label("INSPECCIÓN 3D // ESTILO RE7 & VILLAGE",small);
            GUILayout.Label(e.title,heading);
            GUILayout.Space(12);
            GUILayout.Label(e.description,body);
            GUILayout.Space(18);
            bool backAngle=Mathf.Abs(Mathf.DeltaAngle(g.InspectYaw,180f))<52f||Mathf.Abs(g.InspectPitch)>38f;
            if(backAngle)
            {
                OutlineBox(new Rect(0,285,390,95),new Color(.12f,.22f,.16f,.95f),new Color(.35f,.85f,.52f),1.5f);
                string secretDetail=g.InspectItemId=="record19"||g.InspectItemId=="record85"||g.InspectItemId=="secret"||g.InspectItemId=="evidence"?
                    "✓ DETALLE EXAMINADO AL DORSO:\nSello clínico: 'Ingreso #19 — Año 1985 — Combinación de archivo: 1985'":
                    g.InspectItemId.Contains("key")?
                    "✓ DETALLE EXAMINADO EN LA LLAVE:\nGrabado en el bronce: 'Hospital Roy H. Glover · Apertura automática habilitada'":
                    "✓ DETALLE EXAMINADO EN EL MODELO 3D:\nPropiedad registrada en el inventario forense de Tomás (Chuquicamata, 1985).";
                GUI.Label(new Rect(12,293,368,80),secretDetail,small);
            }
            else
            {
                GUILayout.Label("💡 PISTA DE INSPECCIÓN: Arrastra con el clic izquierdo para girar el modelo 3D hacia el reverso y descubrir grabados ocultos.",small);
            }
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if(Button("RESTABLECER VISTA"))g.ResetInspectView();
            if(Button("VOLVER AL MALETÍN")){g.OpenPanel("inventory");GUIUtility.ExitGUI();}
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUI.Label(new Rect(520,618,720,32),"ARRASTRAR CLIC IZQ.: Rotar 360°  ·  RUEDA DEL RATÓN: Zoom ("+g.InspectZoom.ToString("0.0")+"x)  ·  CLIC DER.: Centrar",body);
            GUI.Label(new Rect(520,652,720,26),"Ángulo actual: Yaw "+Mathf.RoundToInt(g.InspectYaw)+"° / Pitch "+Mathf.RoundToInt(g.InspectPitch)+"°  ·  [ F / ESC ] Salir del inspector",small);
        }
        void Panel(GameSession g)
        {
            if(g.Panel=="inspect"){DrawInspectPanel(g);return;}
            Backdrop(.97f);
            if(g.Panel=="map"){DrawMapPanel(g);return;}
            if(g.Panel=="memory")
            {
                g.FinishMemory();return;
            }
            if(g.Panel=="inventory")
            {
                DrawHubTopBar(g,"inventory");
                if(!g.Data.inventory.Contains(selected)&&!g.Data.journal.Contains(selected))selected=g.Data.inventory.Count>0?g.Data.inventory[0]:"camera";
                OutlineBox(new Rect(40,80,500,600),new Color(.03f,.035f,.04f,.96f),new Color(.42f,.16f,.14f,.9f),1.5f);
                GUILayout.BeginArea(new Rect(58,95,464,570));
                GUILayout.Label("MALETÍN DE OBJETOS",heading);
                inventoryScroll=GUILayout.BeginScrollView(inventoryScroll);
                foreach(string id in g.Data.inventory)if(!g.Data.journal.Contains(id)&&Button("▸  "+StoryData.Get(id).title))selected=id;
                GUILayout.Space(12);
                GUILayout.Label("EXPEDIENTES Y ARCHIVOS CLÍNICOS",heading);
                foreach(string id in g.Data.journal)if(Button("📄  "+StoryData.Get(id).title))selected=id;
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                OutlineBox(new Rect(560,80,680,600),new Color(.03f,.035f,.04f,.96f),new Color(.42f,.16f,.14f,.9f),1.5f);
                Entry e=StoryData.Get(selected);
                Icon(new Rect(585,105,46,46),e.icon);
                GUI.Label(new Rect(645,105,560,60),e.title,title);
                Fill(new Rect(585,165,630,2),new Color(.68f,.16f,.14f));
                GUILayout.BeginArea(new Rect(585,180,630,380));
                documentScroll=GUILayout.BeginScrollView(documentScroll);
                GUILayout.Label(e.description,body);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                if(GUI.Button(new Rect(585,575,360,46),"🔍  INSPECCIONAR MODELO 3D [ F ]",button)){g.OpenInspect(selected);GUIUtility.ExitGUI();}
                if(GUI.Button(new Rect(960,575,255,46),"ABRIR MAPA [ M ]",button)){g.Audio.PlayMapToggle();g.SelectedMapZone=g.Data.zone;g.OpenPanel("map");GUIUtility.ExitGUI();}
                return;
            }
            if(g.Panel=="document")
            {
                DrawHubTopBar(g,"inventory");
                Entry e=StoryData.Get(g.DocumentId);
                OutlineBox(new Rect(180,85,920,585),new Color(.03f,.035f,.04f,.98f),new Color(.65f,.18f,.16f),2f);
                GUILayout.BeginArea(new Rect(215,105,850,545));
                GUILayout.Label("LECTURA DE ARCHIVO CLÍNICO",small);
                GUILayout.Label(e.title,title);
                GUILayout.Space(16);
                documentScroll=GUILayout.BeginScrollView(documentScroll);GUILayout.Label(e.description,body);GUILayout.EndScrollView();
                GUILayout.Space(16);
                GUILayout.BeginHorizontal();
                if(Button("🔍  INSPECCIONAR EN 3D [ F ]")){g.OpenInspect(g.DocumentId);GUIUtility.ExitGUI();}
                if(Button("✓  GUARDAR Y CONTINUAR [ ESC ]")){g.ClosePanel();GUIUtility.ExitGUI();}
                GUILayout.EndHorizontal();
                GUILayout.EndArea();return;
            }
            OutlineBox(new Rect(260,80,760,585),new Color(.03f,.035f,.04f,.98f),new Color(.65f,.18f,.16f),2f);
            GUILayout.BeginArea(new Rect(295,100,690,545));
            if(g.Panel=="newgame")
            {
                GUILayout.Label("¿COMENZAR NUEVA PARTIDA?",heading);GUILayout.Space(20);
                GUILayout.Label("Se sobrescribirá el registro de checkpoint actual. Puedes cancelar para conservar tu progreso.",body);GUILayout.Space(25);
                if(Button("▸  CONFIRMAR NUEVA PARTIDA")){g.NewGame();GUIUtility.ExitGUI();}
                if(Button("▸  CANCELAR")){g.ClosePanel();GUIUtility.ExitGUI();}
            }
            if(g.Panel=="help")
            {
                GUILayout.Label("MANUAL DE SUPERVIVENCIA // ROY H. GLOVER",heading);GUILayout.Space(12);
                GUILayout.Label("• [Modo sin cámara]: Penumbra visible con luces, sombras dinámicas y reflejos sobre charcos de sangre\n• [Clic Der. + Q]: Subir videocámara y activar Visión Nocturna IR de largo alcance (32m)\n• [Apuntar con la cámara a Sebastián]: Lo ralentiza hasta un 74% y frena su embestida\n• [E en Camas o Casilleros]: Esconderse bajo la cama o dentro del casillero  ·  [M] Mapa RE  ·  [F] Inspector 3D",body);
                GUILayout.Space(14);GUILayout.Label("ILUMINACIÓN Y BANDA SONORA: Puedes ajustar el brillo del modo sin cámara en Opciones. Escucha las pistas de Save Room, exploración R.P.D. y el Otherworld industrial estilo Silent Hill para anticipar el peligro.",body);
                GUILayout.Space(14);GUILayout.Label("OBJETIVO EN CURSO:",badgeStyle);GUILayout.Label(g.Objective(),body);
                GUILayout.Space(16);if(Button("▸  VOLVER AL HOSPITAL")){g.ClosePanel();GUIUtility.ExitGUI();}
            }
            if(g.Panel=="options")
            {
                GUILayout.Label("CONFIGURACIÓN DE SISTEMA",title);optionsScroll=GUILayout.BeginScrollView(optionsScroll);
                GUILayout.Label("Volumen maestro",body);g.Volume=GUILayout.HorizontalSlider(g.Volume,0,1);GUILayout.Space(10);
                GUILayout.Label("Efectos de terror, sirena y radioestática",body);g.HorrorVolume=GUILayout.HorizontalSlider(g.HorrorVolume,0,1);GUILayout.Space(10);
                GUILayout.Label("Música ambiental (Resident Evil & Silent Hill)",body);g.MusicVolume=GUILayout.HorizontalSlider(g.MusicVolume,0,1);GUILayout.Space(10);
                GUILayout.Label("Exposición / Brillo (Modo sin cámara e IR)",body);g.Brightness=GUILayout.HorizontalSlider(g.Brightness,0,1);GUILayout.Space(10);
                GUILayout.Label("Sensibilidad de cámara",body);g.Sensitivity=GUILayout.HorizontalSlider(g.Sensitivity,.3f,5);GUILayout.Space(10);
                GUILayout.Label("Campo de visión (FOV): "+Mathf.RoundToInt(g.FieldOfView)+"°",body);g.FieldOfView=GUILayout.HorizontalSlider(g.FieldOfView,60,95);GUILayout.Space(10);
                GUILayout.Label("Intensidad de grano IR",body);g.NoiseAmount=GUILayout.HorizontalSlider(g.NoiseAmount,0,1);GUILayout.Space(10);
                if(Button(g.MinimalHUD?"HUD TÁCTICO: DISCRETO":"HUD TÁCTICO: COMPLETO (ECG + OBJETIVO)"))g.MinimalHUD=!g.MinimalHUD;
                GUILayout.EndScrollView();g.ApplyOptions();GUILayout.Space(10);
                if(Button("▸  GUARDAR Y VOLVER")){g.ClosePanel();GUIUtility.ExitGUI();}
            }
            if(g.Panel=="safe")
            {
                GUILayout.Label("CERRADURA DE ARCHIVO CLASIFICADO",heading);
                string clue19=g.Data.journal.Contains("record19")?"✓ Ingreso registrado: 19":"? Ingreso: revisa la sala izquierda de Urgencias";
                string clue85=g.Data.journal.Contains("record85")?"✓ Año del incidente: 1985 (85)":"? Año: revisa la fecha tachada en Urgencias";
                GUILayout.Label("Concatena Ingreso (2 cifras) + Año del incidente (2 cifras)\n"+clue19+" · "+clue85,small);
                GUILayout.Space(8);GUILayout.Label(g.Code.PadRight(4,'_'),title);
                for(int row=0;row<3;row++){GUILayout.BeginHorizontal();for(int col=1;col<=3;col++){int digit=row*3+col;if(Button(digit.ToString())&&g.Code.Length<4)g.Code+=digit;}GUILayout.EndHorizontal();}
                GUILayout.BeginHorizontal();if(Button("BORRAR"))g.Code="";if(Button("0")&&g.Code.Length<4)g.Code+="0";if(Button("DESBLOQUEAR")){g.SubmitCode();GUIUtility.ExitGUI();}GUILayout.EndHorizontal();GUILayout.Space(12);if(Button("CERRAR")){g.ClosePanel();GUIUtility.ExitGUI();}
            }
            if(g.Panel=="decision")
            {
                GUILayout.Label("DECISIÓN DE EXPEDIENTE SECRETO",heading);GUILayout.Space(18);GUILayout.Label(StoryData.Get("secret").description,body);GUILayout.Space(22);
                if(Button("▸  CONSERVAR EL EXPEDIENTE (REDUCE LA IRA Y ABRE SUBTERRÁNEO)")){g.DecideEvidence(true);GUIUtility.ExitGUI();}
                if(Button("▸  DESTRUIR LAS PRUEBAS (AUMENTA LA IRA Y ABRE SUBTERRÁNEO)")){g.DecideEvidence(false);GUIUtility.ExitGUI();}
            }
            if(g.Panel=="generator")
            {
                GUILayout.Label("GENERADOR TRIFÁSICO DE EMERGENCIA",heading);GUILayout.Space(12);
                string fuseState=g.Data.inventory.Contains("fuse")?"✓ Fusible listo para instalar":"✗ Falta el fusible (busca en la sala frente al generador)";
                string steamState=g.Has("steam_purged")?"✓ Presión de vapor estabilizada":"! Válvula de vapor sin purgar (opcional en sala derecha)";
                int load=(g.Switches[0]?110:0)+(g.Switches[1]?180:0)+(g.Switches[2]?110:0);
                string loadState=g.Switches[1]?"SOBRECARGA EN LÍNEA B ("+load+"V)":(g.Switches[0]&&g.Switches[2]?"SINCRONÍA ESTABLE (220V)":"TENSIÓN INCOMPLETA ("+load+"V)");
                GUILayout.Label(fuseState+"\n"+steamState+"\nDiagnóstico de red: "+loadState,small);
                GUILayout.Space(16);GUILayout.BeginHorizontal();for(int i=0;i<3;i++)if(Button("Fase "+((char)('A'+i))+" / "+(g.Switches[i]?"ON":"OFF")))g.Switches[i]=!g.Switches[i];GUILayout.EndHorizontal();GUILayout.Space(18);if(Button("▸  INSTALAR FUSIBLE Y ABRIR PUERTA")){g.Generator();GUIUtility.ExitGUI();}if(Button("CERRAR")){g.ClosePanel();GUIUtility.ExitGUI();}
            }
            if(g.Panel=="ending")
            {
                GUILayout.Label("¿QUÉ HARÁS CON LA HISTORIA?",heading);GUILayout.Space(20);GUILayout.Label("Madre e hijo ya están juntos. Para revelar toda la verdad necesitas haber conservado el expediente y recogido los registros junto al equipo de Rayos X.",body);GUILayout.Space(25);
                if(Button("▸  CONTAR LO QUE OCURRIÓ (INICIAR CINEMÁTICA 3D)")){g.End(true);GUIUtility.ExitGUI();}if(Button("▸  ABANDONAR EL HOSPITAL EN SILENCIO (INICIAR CINEMÁTICA 3D)")){g.End(false);GUIUtility.ExitGUI();}if(Button("VOLVER A INVESTIGAR")){g.ClosePanel();GUIUtility.ExitGUI();}
            }
            GUILayout.EndArea();
        }
        void Toast(GameSession g)
        {
            if(Time.unscaledTime<g.SavedUntil)GUI.Label(new Rect(1010,185,250,28),"✓ CHECKPOINT REGISTRADO",badgeStyle);
            if(Time.unscaledTime<g.MessageUntil)
            {
                OutlineBox(new Rect(180,636,920,40),new Color(.02f,.022f,.025f,.95f),new Color(.65f,.18f,.16f,.85f),1f);
                GUI.Label(new Rect(198,644,884,28),g.Message,small);
            }
        }
    }
}
