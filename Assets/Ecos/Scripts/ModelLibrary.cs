using System;
using System.Collections.Generic;
using UnityEngine;
namespace Ecos
{
    [Serializable] public class RigBone {public string name;public int parent;public Vector3 position;}
    [Serializable] public class ModelData { public float[] vertices,normals,colors,uv,groups,weight0; public int[] triangles,bone0,bone1;public RigBone[] rig; }
    public static class ModelLibrary
    {
        static readonly Dictionary<string,Mesh> cache=new Dictionary<string,Mesh>();
        static Material material;
        static readonly Dictionary<string,RigBone[]> rigs=new Dictionary<string,RigBone[]>();
        public static Material SharedMaterial
        {
            get
            {
                if(material==null){Shader shader=Resources.Load<Shader>("Shaders/Grime");if(shader==null)throw new Exception("Falta Resources/Shaders/Grime.shader");material=new Material(shader){name="Ecos_Palette"};BindTextures(material);}
                return material;
            }
        }
        public static void BindTextures(Material target)
        {
            if(target.HasProperty("_FaceTex"))
            {
                target.SetTexture("_FaceTex",Resources.Load<Texture2D>("Sebastian/FaceAlbedo"));
                target.SetTexture("_FaceNormal",Resources.Load<Texture2D>("Sebastian/FaceNormal"));
                target.SetTexture("_ClothTex",Resources.Load<Texture2D>("Sebastian/ClothAlbedo"));
                target.SetTexture("_ClothNormal",Resources.Load<Texture2D>("Sebastian/ClothNormal"));return;
            }
            {target.SetTexture("_MainTex",Resources.Load<Texture2D>("PBR/Albedo"));target.SetTexture("_DetailNormal",Resources.Load<Texture2D>("PBR/DetailNormal"));target.SetTexture("_MaterialMask",Resources.Load<Texture2D>("PBR/MaterialMask"));
            if(target.HasProperty("_PropsTex")){target.SetTexture("_PropsTex",Resources.Load<Texture2D>("PBR/PropsAlbedo"));target.SetTexture("_PropsNormal",Resources.Load<Texture2D>("PBR/PropsNormal"));target.SetTexture("_PropsMask",Resources.Load<Texture2D>("PBR/PropsMask"));}}
        }
        public static Mesh Load(string name)
        {
            Mesh existing;if(cache.TryGetValue(name,out existing)&&existing!=null)return existing;
            TextAsset text=Resources.Load<TextAsset>("Models/"+name);if(text==null)throw new Exception("Falta modelo: "+name);
            ModelData d=JsonUtility.FromJson<ModelData>(text.text);int count=d.vertices.Length/3;
            Vector3[] vertices=new Vector3[count],normals=new Vector3[count];Color[] colors=new Color[count];
            for(int i=0;i<count;i++){int j=i*3;vertices[i]=new Vector3(d.vertices[j],d.vertices[j+1],d.vertices[j+2]);normals[i]=new Vector3(d.normals[j],d.normals[j+1],d.normals[j+2]);colors[i]=new Color(d.colors[j],d.colors[j+1],d.colors[j+2],1);}
            Vector2[] uv=new Vector2[count],groups=new Vector2[count];
            for(int i=0;i<count;i++){uv[i]=new Vector2(d.uv[i*2],d.uv[i*2+1]);groups[i]=new Vector2(d.groups[i*2],d.groups[i*2+1]);}
            Mesh mesh=new Mesh{name=name};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.vertices=vertices;mesh.uv=uv;mesh.uv2=groups;mesh.normals=normals;mesh.colors=colors;mesh.triangles=d.triangles;
            if(d.rig!=null&&d.rig.Length>0)
            {
                if(d.bone0==null||d.bone1==null||d.weight0==null||d.bone0.Length!=count||d.bone1.Length!=count||d.weight0.Length!=count)throw new Exception("Pesos de rig inválidos: "+name);
                BoneWeight[] weights=new BoneWeight[count];Matrix4x4[] bind=new Matrix4x4[d.rig.Length];
                for(int i=0;i<count;i++)weights[i]=new BoneWeight{boneIndex0=d.bone0[i],boneIndex1=d.bone1[i],weight0=d.weight0[i],weight1=1-d.weight0[i]};
                for(int i=0;i<bind.Length;i++)bind[i]=Matrix4x4.Translate(-d.rig[i].position);
                mesh.boneWeights=weights;mesh.bindposes=bind;rigs[name]=d.rig;
            }
            mesh.RecalculateTangents();mesh.RecalculateBounds();cache[name]=mesh;return mesh;
        }
        public static GameObject Create(string name,Transform parent,Vector3 position,Vector3 scale)
        {
            GameObject o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localScale=scale;
            o.AddComponent<MeshFilter>().sharedMesh=Load(name);o.AddComponent<MeshRenderer>().sharedMaterial=SharedMaterial;return o;
        }
        public static void SetBaseHeight(GameObject o,float height)
        {
            Renderer renderer=o.GetComponent<Renderer>();Vector3 p=o.transform.position;
            p.y+=height-renderer.bounds.min.y;o.transform.position=p;
        }
        public static void AddStaticCollider(GameObject o)
        {
            o.layer=8;MeshCollider c=o.AddComponent<MeshCollider>();
            c.sharedMesh=o.GetComponent<MeshFilter>().sharedMesh;c.convex=false;
        }
        public static RigBone[] LoadRig(string name)
        {
            Load(name);
            if(!rigs.TryGetValue(name,out RigBone[] rig))throw new InvalidOperationException("El modelo no contiene huesos: "+name);
            return rig;
        }
        public static void ClearCache() { cache.Clear(); rigs.Clear(); material = null; }
    }
}
