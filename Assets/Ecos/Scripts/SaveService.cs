using System;
using System.Collections.Generic;
using UnityEngine;
namespace Ecos
{
    [Serializable] public class SaveData
    {
        public int version=2,zone=0,spareBatteries=0;public float battery=100,anger=10,x=0,y=.1f,z=1,yaw=0;
        public List<string> flags=new List<string>(),inventory=new List<string>(),journal=new List<string>();
    }
    public static class SaveService
    {
        const string Key="Ecos_Checkpoint_v1";
        static bool Finite(float n){return !float.IsNaN(n)&&!float.IsInfinity(n);}
        public static bool Validate(SaveData s)
        {
            if(s==null||(s.version!=1&&s.version!=2)||s.zone<0||s.zone>7||s.flags==null||s.inventory==null||s.journal==null)return false;
            if(!Finite(s.battery)||!Finite(s.anger)||!Finite(s.x)||!Finite(s.y)||!Finite(s.z)||!Finite(s.yaw)||s.battery<0)return false;
            if(Mathf.Abs(s.x)>18||s.z< -1||s.z>111||s.y< -1||s.y>3.3f)return false;
            s.battery=Mathf.Clamp(s.battery,0,100);s.anger=Mathf.Clamp(s.anger,0,100);s.yaw=Mathf.Repeat(s.yaw,360);
            s.spareBatteries=Mathf.Clamp(s.spareBatteries,0,99);s.zone=Mathf.Clamp(Mathf.FloorToInt(s.z/14),0,7);
            s.flags.RemoveAll(string.IsNullOrEmpty);s.inventory.RemoveAll(string.IsNullOrEmpty);s.journal.RemoveAll(string.IsNullOrEmpty);
            s.flags=new List<string>(new HashSet<string>(s.flags));s.inventory=new List<string>(new HashSet<string>(s.inventory));s.journal=new List<string>(new HashSet<string>(s.journal));
            return true;
        }
        static SaveData Decode(string json)
        {
            try
            {
                SaveData s=JsonUtility.FromJson<SaveData>(json);if(!Validate(s))return null;
                if(s.version==1){foreach(string id in new[]{"frame","xray","seal"})if(s.flags.Contains("echo_"+id)&&!s.flags.Contains("clue_"+id))s.flags.Add("clue_"+id);s.version=2;}
                return s;
            }
            catch(Exception){return null;}
        }
        public static SaveData Read()
        {
            SaveData current=PlayerPrefs.HasKey(Key)?Decode(PlayerPrefs.GetString(Key)):null;
            return current??(PlayerPrefs.HasKey(Key+"_backup")?Decode(PlayerPrefs.GetString(Key+"_backup")):null);
        }
        public static bool Write(SaveData data)
        {
            if(!Validate(data)){Debug.LogWarning("Checkpoint descartado: datos inválidos. Se conserva el guardado anterior.");return false;}
            try
            {
                if(PlayerPrefs.HasKey(Key))
                {
                    string previous=PlayerPrefs.GetString(Key);
                    if(Decode(previous)!=null)PlayerPrefs.SetString(Key+"_backup",previous);
                }
                PlayerPrefs.SetString(Key,JsonUtility.ToJson(data));PlayerPrefs.Save();return true;
            }
            catch(Exception error){Debug.LogWarning("No se pudo guardar el checkpoint: "+error.Message);return false;}
        }
    }
}
