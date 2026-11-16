using System.Collections.Generic;
using UnityEngine;
namespace Ecos
{
    public class HospitalMap : MonoBehaviour
    {
        public readonly HashSet<Vector2Int> Cells=new HashSet<Vector2Int>();
        public static readonly Vector2Int[] Directions={Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
        // Asymmetric wing layouts per zone (0..7): (side, xMin, xMax, zOffsetMin, zOffsetMax, doorX, doorZOffset)
        public static readonly int[][] WingSpecs={
            new[]{1, 5,7, 1,2, 4,1},   // Zone 0 East: Guard & Triage Annex
            new[]{-1,6,7, 0,1, 4,1},   // Zone 0 West: Collapsed Ambulance Vestibule
            new[]{-1,5,8, 5,6, 3,4},   // Zone 1 West: Director's Archive Suite (North-West)
            new[]{1, 5,7, 0,1, 4,1},   // Zone 1 East: Reception Waiting Lounge (South-East)
            new[]{1, 5,8, 5,6, 3,4},   // Zone 2 East: Neonatal Incubator Ward (North-East)
            new[]{-1,5,7, 0,1, 3,1},   // Zone 2 West: Pediatric Playroom Alcove (South-West)
            new[]{-1,5,8, 0,1, 3,1},   // Zone 3 West: Emergency Surgical Shock Bay (South-West)
            new[]{1, 5,8, 5,6, 3,4},   // Zone 3 East: Radiology Darkroom Annex (North-East)
            new[]{1, 5,8, 0,1, 4,1},   // Zone 4 East: Clinical Microscope Laboratory (South-East)
            new[]{-1,5,8, 5,6, 3,4},   // Zone 4 West: Classified Dossier Vault (North-West)
            new[]{-1,5,8, 0,1, 3,1},   // Zone 5 West: Underground Turbine Chamber (South-West)
            new[]{1, 5,8, 5,6, 3,4},   // Zone 5 East: Steam Boiler & Fuse Gallery (North-East)
            new[]{1, 5,8, 0,1, 4,1},   // Zone 6 East: Autopsy Amphitheater Annex (South-East)
            new[]{-1,5,7, 5,6, 3,4},   // Zone 6 West: Specimen Cold Storage (North-West)
            new[]{-1,5,8, 5,6, 3,4},   // Zone 7 West: Mortuary Hall A1 Annex (North-West)
            new[]{1, 5,8, 5,6, 3,4}    // Zone 7 East: Mortuary Hall C2 Annex (North-East)
        };
        static readonly int[] WingZones={0,0, 1,1, 2,2, 3,3, 4,4, 5,5, 6,6, 7,7};
        public void Init()
        {
            if(Cells.Count>0)return;
            for(int z=0;z<56;z++)for(int x=-1;x<=1;x++)Cells.Add(new Vector2Int(x,z));
            for(int zone=0;zone<8;zone++)foreach(int s in new[]{-1,1})
            {
                int center=zone*7+3;Cells.Add(new Vector2Int(s*2,center));
                for(int x=3;x<=5;x++)for(int z=center-1;z<=center+1;z++)Cells.Add(new Vector2Int(s*x,z));
            }
            for(int i=0;i<WingSpecs.Length;i++)
            {
                int zone=WingZones[i];int[] w=WingSpecs[i];
                int s=w[0],xMin=w[1],xMax=w[2],zMin=zone*7+w[3],zMax=zone*7+w[4],doorX=w[5],doorZ=zone*7+w[6];
                Cells.Add(new Vector2Int(s*doorX,doorZ));
                for(int x=Mathf.Min(doorX,xMin);x<=xMax;x++)for(int z=zMin;z<=zMax;z++)Cells.Add(new Vector2Int(s*x,z));
            }
        }
        void Awake(){Init();}
        public Vector3 Position(Vector2Int cell){return new Vector3(cell.x*2,0,cell.y*2);}
        public Vector2Int Cell(Vector3 p){return new Vector2Int(Mathf.RoundToInt(p.x/2),Mathf.RoundToInt(p.z/2));}
        public bool IsWalkable(Vector3 point)
        {
            Init();return Cells.Contains(Cell(point))&&!Physics.CheckCapsule(new Vector3(point.x,.28f,point.z),new Vector3(point.x,1.5f,point.z),.24f,1<<8,QueryTriggerInteraction.Ignore);
        }
        public bool ClearSegment(Vector3 a,Vector3 b)
        {
            Vector3 delta=b-a;delta.y=0;float distance=delta.magnitude;if(distance<.001f)return true;
            return !Physics.CapsuleCast(new Vector3(a.x,.28f,a.z),new Vector3(a.x,1.25f,a.z),.2f,delta/distance,distance,1<<8,QueryTriggerInteraction.Ignore);
        }
        public Vector3 SafePosition(Vector3 desired){Init();return Position(Nearest(Cell(desired)));}
        public bool Connected(Vector2Int a,Vector2Int b)
        {
            if(!Cells.Contains(b))return false;
            int az=a.y/7,bz=b.y/7;if(az==bz)return true;
            int gate=Mathf.Max(az,bz);return gate==1||gate==4||GameSession.Instance==null||GameSession.Instance.Has("gate_"+gate);
        }
        public List<Vector3> Path(Vector3 from,Vector3 to)
        {
            Init();Vector2Int start=Nearest(Cell(from)),end=Nearest(Cell(to));
            Queue<Vector2Int> q=new Queue<Vector2Int>();Dictionary<Vector2Int,Vector2Int> prev=new Dictionary<Vector2Int,Vector2Int>();q.Enqueue(start);prev[start]=start;
            while(q.Count>0){Vector2Int c=q.Dequeue();if(c==end)break;foreach(Vector2Int d in Directions){Vector2Int n=c+d;if(Connected(c,n)&&!prev.ContainsKey(n)&&IsWalkable(Position(n))&&ClearSegment(Position(c),Position(n))){prev[n]=c;q.Enqueue(n);}}}
            List<Vector3> path=new List<Vector3>();if(!prev.ContainsKey(end))return path;
            for(Vector2Int c=end;c!=start;c=prev[c])path.Add(Position(c));path.Reverse();return path;
        }
        Vector2Int Nearest(Vector2Int p){if(Cells.Contains(p)&&IsWalkable(Position(p)))return p;float best=float.MaxValue;Vector2Int result=Vector2Int.zero;foreach(Vector2Int c in Cells){if(!IsWalkable(Position(c)))continue;float d=(c-p).sqrMagnitude;if(d<best){best=d;result=c;}}return result;}
    }
}
