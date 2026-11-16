using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ecos.Editor
{
    public static class EcosSetup
    {
        const string ScenePath = "Assets/Ecos/Scenes/HospitalEditable.unity";

        [MenuItem("Ecos de Chuquicamata/Generar hospital editable")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            LevelBuilder.Build();

            Directory.CreateDirectory("Assets/Ecos/Generated/Meshes");
            Directory.CreateDirectory("Assets/Ecos/Generated/Materials");
            AssetDatabase.Refresh();

            foreach (MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null || AssetDatabase.Contains(mesh)) continue;
                string path = "Assets/Ecos/Generated/Meshes/" + mesh.name + ".asset";
                Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing == null) { AssetDatabase.CreateAsset(mesh, path); existing = mesh; }
                filter.sharedMesh = existing;
            }

            foreach (SkinnedMeshRenderer skin in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Mesh mesh = skin.sharedMesh;
                if (mesh == null || AssetDatabase.Contains(mesh)) continue;
                string path = "Assets/Ecos/Generated/Meshes/" + mesh.name + ".asset";
                Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing == null) { AssetDatabase.CreateAsset(mesh, path); existing = mesh; }
                skin.sharedMesh = existing;
            }

            foreach (MeshCollider collider in Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                MeshFilter filter = collider.GetComponent<MeshFilter>();
                if (filter != null) collider.sharedMesh = filter.sharedMesh;
            }

            foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material mat = mats[i];
                    if (mat == null || AssetDatabase.Contains(mat)) continue;
                    string path = "Assets/Ecos/Generated/Materials/" + mat.name.Replace("/", "_") + ".mat";
                    Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (existing == null) { AssetDatabase.CreateAsset(mat, path); existing = mat; }
                    mats[i] = existing;
                }
                renderer.sharedMaterials = mats;
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("Hospital editable guardado correctamente.");
        }

        [MenuItem("Ecos de Chuquicamata/Build Windows 64 bits")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string path = EditorUtility.SaveFolderPanel("Carpeta de la build", "", "EcosWindows");
            if (string.IsNullOrEmpty(path)) return;
            string source = File.Exists(ScenePath) ? ScenePath : "Assets/Ecos/Scenes/Inicio.unity";
            BuildPipeline.BuildPlayer(new[] { source }, Path.Combine(path, "EcosDeChuquicamata.exe"), BuildTarget.StandaloneWindows64, BuildOptions.None);
        }
    }
}
