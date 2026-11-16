using UnityEngine;

namespace Ecos
{
    public class SebastianRig : MonoBehaviour
    {
        public Transform[] bones;
        public SkinnedMeshRenderer skin;
        [SerializeField] private Vector3[] restPositions;

        public static SebastianRig Create(Transform parent, Material material)
        {
            GameObject root = new GameObject("Sebastian articulado");
            root.transform.SetParent(parent, false);
            SebastianRig rig = root.AddComponent<SebastianRig>();

            RigBone[] defs = ModelLibrary.LoadRig("Sebastian");
            rig.bones = new Transform[defs.Length];
            rig.restPositions = new Vector3[defs.Length];

            for (int i = 0; i < defs.Length; i++)
            {
                Transform bone = new GameObject(defs[i].name).transform;
                bone.SetParent(defs[i].parent < 0 ? root.transform : rig.bones[defs[i].parent], false);
                bone.localPosition = defs[i].position - (defs[i].parent < 0 ? Vector3.zero : defs[defs[i].parent].position);
                rig.bones[i] = bone;
                rig.restPositions[i] = bone.localPosition;
            }

            rig.skin = root.AddComponent<SkinnedMeshRenderer>();
            rig.skin.sharedMesh = ModelLibrary.Load("Sebastian");
            rig.skin.bones = rig.bones;
            rig.skin.rootBone = rig.bones[0];
            rig.skin.sharedMaterial = material;
            rig.skin.updateWhenOffscreen = true;
            return rig;
        }

        public void ResetPose()
        {
            if (bones == null) return;
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].localPosition = restPositions[i];
                bones[i].localRotation = Quaternion.identity;
            }
        }

        void LateUpdate()
        {
            GameSession g = GameSession.Instance;
            if (g == null || !g.Playing || !g.Enemy.Active || bones == null || bones.Length < 13) return;

            float mult = g.Data.anger >= 65f ? 13f : 9f;
            float balanceo = g.Enemy.Speed > 0.1f ? Mathf.Sin(Time.time * mult) * 32f : 0f;
            float espasmo = g.Data.anger >= 50f ? Mathf.Sin(Time.time * 27f) * 4f : 0f;

            bones[5].localRotation = Quaternion.Euler(balanceo + espasmo, 0f, -10f);
            bones[7].localRotation = Quaternion.Euler(-balanceo - espasmo, 0f, 10f);
            bones[9].localRotation = Quaternion.Euler(-balanceo, 0f, 0f);
            bones[11].localRotation = Quaternion.Euler(balanceo, 0f, 0f);
        }
    }
}