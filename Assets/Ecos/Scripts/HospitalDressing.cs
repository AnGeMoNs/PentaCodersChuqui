using UnityEngine;
namespace Ecos
{
    public static class HospitalDressing
    {
        static Transform parent;
        static int lockerIndex,bedIndex;
        static GameObject Prop(string name,Vector3 p,float yaw=0,float scale=1,bool solid=true)
        {
            GameObject o=ModelLibrary.Create(name,parent,p,Vector3.one*scale);o.transform.localRotation=Quaternion.Euler(0,yaw,0);
            if(Mathf.Approximately(p.y,0))ModelLibrary.SetBaseHeight(o,0);
            if(solid)ModelLibrary.AddStaticCollider(o);
            if(name=="Locker")
            {
                Interactable hide=o.AddComponent<Interactable>();
                hide.id="locker_"+(lockerIndex++);
                hide.label="Esconderse en el casillero";
                hide.kind="locker";
            }
            else if(name=="Bed")
            {
                Interactable hide=o.AddComponent<Interactable>();
                hide.id="wing_bed_"+(bedIndex++);
                hide.label="Esconderse bajo la cama";
                hide.kind="bed_hide";
            }
            else if(name=="Lamp")
            {
                LevelBuilder.AddLight(o.transform,new Color(.58f,.70f,.62f),.62f,9.5f);
            }
            return o;
        }
        public static void Build(Transform root)
        {
            lockerIndex=0;bedIndex=0;
            parent=new GameObject("Ambientacion / Equipamiento original").transform;parent.SetParent(root,false);
            for(int zone=0;zone<8;zone++)
            {
                float z=zone*14;
                Prop("Extinguisher",new Vector3(-2.81f,1,z+10),-90);
                Prop("Vent",new Vector3(2.9f,2.5f,z+10),90);
                Prop("Radiator",new Vector3(-2.82f,.15f,z+9),-90,.7f);
                Prop("CableBundle",new Vector3(1.8f,0,z+10),90,.65f);
                for(int i=0;i<3;i++)
                {
                    GameObject paper=Prop("Document",new Vector3(-1.9f+i*.24f,.018f,z+9.5f+i*.4f),0,.45f);
                    paper.transform.rotation=Quaternion.Euler(90,30*i,0);
                }
                if(zone<5)Prop("BrokenWindow",new Vector3(2.91f,2,z+3),90);
                if(zone>=1&&zone<=4)Prop("Fan",new Vector3(0,3.2f,z+8));
                Prop("BioBin",new Vector3(9.3f,0,z+4.2f));
                if(zone>=2&&zone<=4)
                {
                    Prop("IVStand",new Vector3(8.9f,0,z+7.5f));Prop("Trolley",new Vector3(7.4f,0,z+7.5f));Prop("Monitor",new Vector3(7.4f,.83f,z+7.5f));
                    Prop("OxygenTank",new Vector3(-9.8f,0,z+7.8f));Prop("PrivacyScreen",new Vector3(-9.7f,0,z+4.1f),90,.8f);
                }
                if(zone>=5)
                {
                    Prop("Locker",new Vector3(9.7f,0,z+7.7f),180,1,true);
                    Prop("Sink",new Vector3(10.3f,0,z+4.4f),90,.85f);
                }
            }
            for(int zone=1;zone<8;zone++)
            {
                float z=zone*14;
                Prop("WallClock",new Vector3(-2.9f,2.4f,z+10),-90);
                Prop("ElectricalPanel",new Vector3(2.85f,.65f,z+11),90,.7f);
                Prop("ArchiveBoxes",new Vector3(9.8f,0,z+3.6f),0,.8f,true);
                if(zone>=2)Prop("MedicineShelf",new Vector3(10.55f,0,z+8),90,1,true);
                if(zone==3||zone==6)
                {
                    Prop("Stretcher",new Vector3(-6.3f,0,z+7.4f),0,.85f,true);
                    Prop("WashBasin",new Vector3(-10.55f,0,z+8),-90,.9f,true);
                }
                if(zone==2||zone==4)
                {
                    Prop("BedsideTable",new Vector3(6.3f,0,z+4),0,1,true);
                    Prop("Defibrillator",new Vector3(6.3f,.877f,z+4),0,.8f);
                }
            }
            Prop("VinylSofa",new Vector3(2.35f,0,31),-90,.85f,true);
            Prop("JanitorCart",new Vector3(-2.2f,0,80),0,.8f,true);
            Prop("DeskLamp",new Vector3(-8.9f,1.044f,20.2f),0,.8f);
            Prop("MedicalTray",new Vector3(7.4f,.320625f,49.5f),0,.75f);
            Prop("MedicalTray",new Vector3(1.9f,.82f,101),90,.8f);
            Prop("Bench",new Vector3(2.35f,0,5),-90,1,true);
            Prop("Bench",new Vector3(2.35f,0,19),-90,1,true);
            Prop("Locker",new Vector3(9.7f,0,21),180,1,true);
            Prop("Wheelchair",new Vector3(-2.1f,0,39),25);
            Prop("SurgicalLamp",new Vector3(8,3.2f,49));
            Prop("SurgicalLamp",new Vector3(-8,3.2f,62));
            Prop("MorgueDrawers",new Vector3(0,0,110));
            Prop("MorgueDrawers",new Vector3(-10.6f,0,104),-90,.75f,true);
            Prop("MorgueDrawers",new Vector3(10.6f,0,104),90,.75f,true);
            Prop("Trolley",new Vector3(1.9f,0,101),90);
            Prop("SurgicalLamp",new Vector3(-8,3.2f,104));
            Prop("Incubator",new Vector3(-10.1f,0,34),90,.9f,true);
            Prop("AutopsyTable",new Vector3(6.4f,0,91.5f),0,.85f,true);
            Prop("Microscope",new Vector3(-7.2f,1.065f,20.1f),0,1,true);
            Prop("FirstAidKit",new Vector3(-2.84f,1.55f,22),-90,1,true);
            Prop("FuseBox",new Vector3(2.82f,1.35f,75),90,1,true);
            Prop("AudioTape",new Vector3(-.25f,.885f,3.05f),25,1,true);

            // === DENSE ABANDONED HOSPITAL DRESSING IN THE 16 ASYMMETRIC ANNEX WINGS ===
            // Zone 0 Annexes (East Guard/Triage x:10..14, z:2..4; West Ambulance Bay x:-12..-14, z:0..2)
            Prop("Lamp",new Vector3(12,3.155f,3));
            Prop("Desk",new Vector3(12.5f,0,3.8f),180,.85f,true);
            Prop("Locker",new Vector3(14.2f,0,2.2f),-90,1,true);
            Prop("Wheelchair",new Vector3(10.2f,0,1.8f),-35,1,true);
            Prop("FirstAidKit",new Vector3(14.8f,1.55f,3.2f),90,1,true);
            Prop("Lamp",new Vector3(-13,3.155f,1));
            Prop("Stretcher",new Vector3(-12.5f,0,1.6f),15,.9f,true);
            Prop("ArchiveBoxes",new Vector3(-14.2f,0,.4f),0,.85f,true);
            Prop("Rubble",new Vector3(-13.8f,0,2.2f),40,.65f,true);

            // Zone 1 Annexes (West Director Suite x:-10..-16, z:24..26; East Waiting Lounge x:10..14, z:14..16)
            Prop("Lamp",new Vector3(-13,3.155f,25));
            Prop("Desk",new Vector3(-13.5f,0,25.5f),0,.9f,true);
            Prop("Locker",new Vector3(-15.8f,0,24.2f),90,1,true);
            Prop("Cabinet",new Vector3(-10.5f,0,26.2f),180,.9f,true);
            Prop("Chair",new Vector3(-13.5f,0,24.1f),0,1,true);
            Prop("Lamp",new Vector3(12,3.155f,15));
            Prop("VinylSofa",new Vector3(12.5f,0,14.2f),0,.9f,true);
            Prop("Bench",new Vector3(10.2f,0,16.2f),180,.9f,true);
            Prop("BioBin",new Vector3(14.3f,0,15.5f),0,1,true);
            Prop("WallClock",new Vector3(14.8f,2.3f,15.0f),90,1,true);

            // Zone 2 Annexes (East Neonatal Ward x:10..16, z:38..40; West Playroom Alcove x:-10..-14, z:28..30)
            Prop("Lamp",new Vector3(13,3.155f,39));
            Prop("Incubator",new Vector3(12.0f,0,39.5f),0,.9f,true);
            Prop("Incubator",new Vector3(14.6f,0,39.5f),0,.9f,true);
            Prop("Bed",new Vector3(15.6f,0,37.6f),-90,.9f,true);
            Prop("Locker",new Vector3(10.2f,0,40.2f),180,1,true);
            Prop("Lamp",new Vector3(-12,3.155f,29));
            Prop("Crib",new Vector3(-12.5f,0,28.5f),0,.9f,true);
            Prop("PrivacyScreen",new Vector3(-14.2f,0,29.8f),45,.8f,true);
            Prop("ArchiveBoxes",new Vector3(-10.4f,0,28.2f),0,.8f,true);

            // Zone 3 Annexes (West Surgical Shock Bay x:-10..-16, z:42..44; East Radiology Darkroom x:10..16, z:52..54)
            Prop("Lamp",new Vector3(-13,3.155f,43));
            Prop("SurgicalLamp",new Vector3(-12.5f,3.2f,42.5f));
            Prop("Bed",new Vector3(-12.5f,0,42.2f),0,.95f,true);
            Prop("Stretcher",new Vector3(-15.2f,0,43.8f),90,.9f,true);
            Prop("Locker",new Vector3(-10.2f,0,42.0f),0,1,true);
            Prop("IVStand",new Vector3(-14.2f,0,42.2f),0,1,true);
            Prop("Lamp",new Vector3(13,3.155f,53));
            Prop("XRay",new Vector3(13.5f,0,53.8f),180,1,true);
            Prop("Cabinet",new Vector3(15.8f,0,52.2f),-90,.9f,true);
            Prop("Wheelchair",new Vector3(10.5f,0,53.5f),40,1,true);
            Prop("OxygenTank",new Vector3(15.6f,0,53.8f),0,1,true);

            // Zone 4 Annexes (East Clinical Lab x:10..16, z:56..58; West Classified Vault x:-10..-16, z:66..68)
            Prop("Lamp",new Vector3(13,3.155f,57));
            Prop("Desk",new Vector3(13.2f,0,56.4f),0,.9f,true);
            Prop("Locker",new Vector3(15.8f,0,57.8f),-90,1,true);
            Prop("MedicineShelf",new Vector3(10.4f,0,56.2f),0,.9f,true);
            Prop("BioBin",new Vector3(15.4f,0,56.2f),0,1,true);
            Prop("Lamp",new Vector3(-13,3.155f,67));
            Prop("Cabinet",new Vector3(-12.4f,0,67.8f),180,1,true);
            Prop("Cabinet",new Vector3(-14.5f,0,67.8f),180,1,true);
            Prop("ArchiveBoxes",new Vector3(-15.8f,0,66.2f),90,.9f,true);
            Prop("Bed",new Vector3(-10.2f,0,67.6f),0,.9f,true);

            // Zone 5 Annexes (West Turbine Chamber x:-10..-16, z:70..72; East Boiler Gallery x:10..16, z:80..82)
            Prop("Lamp",new Vector3(-13,3.155f,71));
            Prop("Generator",new Vector3(-13.5f,0,70.5f),0,.9f,true);
            Prop("Locker",new Vector3(-15.8f,0,72.0f),90,1,true);
            Prop("OxygenTank",new Vector3(-10.5f,0,70.2f),0,1,true);
            Prop("CableBundle",new Vector3(-12.0f,0,72.0f),45,.7f,true);
            Prop("Lamp",new Vector3(13,3.155f,81));
            Prop("JanitorCart",new Vector3(12.2f,0,81.5f),30,.85f,true);
            Prop("Locker",new Vector3(15.8f,0,80.2f),-90,1,true);
            Prop("Cabinet",new Vector3(14.2f,0,82.0f),180,.9f,true);
            Prop("FuseBox",new Vector3(16.8f,1.4f,81.0f),90,1,true);

            // Zone 6 Annexes (East Autopsy Amphitheater x:10..16, z:84..86; West Specimen Storage x:-10..-14, z:94..96)
            Prop("Lamp",new Vector3(13,3.155f,85));
            Prop("AutopsyTable",new Vector3(13.2f,0,85.0f),0,.9f,true);
            Prop("SurgicalLamp",new Vector3(13.2f,3.2f,85.0f));
            Prop("Locker",new Vector3(15.8f,0,84.2f),-90,1,true);
            Prop("Sink",new Vector3(10.4f,0,84.2f),0,.85f,true);
            Prop("Lamp",new Vector3(-12,3.155f,95));
            Prop("MorgueDrawers",new Vector3(-13.2f,0,95.8f),180,.8f,true);
            Prop("Bed",new Vector3(-10.2f,0,95.6f),0,.9f,true);

            // Zone 7 Annexes (West Mortuary Annex A x:-10..-16, z:108..110; East Mortuary Annex C x:10..16, z:108..110)
            Prop("Lamp",new Vector3(-13,3.155f,109));
            Prop("Stretcher",new Vector3(-12.5f,0,109.5f),0,.9f,true);
            Prop("Locker",new Vector3(-15.8f,0,108.5f),90,1,true);
            Prop("PrivacyScreen",new Vector3(-10.2f,0,109.8f),0,.8f,true);
            Prop("Lamp",new Vector3(13,3.155f,109));
            Prop("AutopsyTable",new Vector3(13.0f,0,109.5f),0,.85f,true);
            Prop("Locker",new Vector3(15.8f,0,108.5f),-90,1,true);
            Prop("Wheelchair",new Vector3(10.4f,0,109.6f),-25,1,true);
        }
    }
}
