using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class ProjectBuilder
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string HospitalScenePath = "Assets/Scenes/HospitalNivel1.unity";

    static ProjectBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists("Assets/Textures/ParticulaSuave.png") && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                BuildAll();
            }
        };
    }

    [MenuItem("Ecos de Chuquicamata/Construir Proyecto Completo (Cátedra 1)")]
    public static void BuildAll()
    {
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Settings");
        EnsureFolder("Assets/Textures");
        AssetDatabase.Refresh();

        // 1. Crear textura circular suave para partículas (evita cuadrados morados en URP)
        Texture2D particleTex = CreateSoftParticleTexture("Assets/Textures/ParticulaSuave.png");

        // 2. Materiales realistas con texturas para arquitectura y todos los submateriales de los modelos 3D
        Material wallMat = CreateTexturedMaterial("ParedHospital", "Assets/Textures/Plaster.jpg", null, new Color(0.52f, 0.55f, 0.52f), 0.04f, 0.22f, new Vector2(3f, 2f));
        Material floorMat = CreateTexturedMaterial("PisoHospital", "Assets/Textures/Tiles.jpg", null, new Color(0.38f, 0.41f, 0.39f), 0.08f, 0.48f, new Vector2(6f, 10f));
        Material ceilingMat = CreateTexturedMaterial("TechoHospital", "Assets/Textures/Plaster.jpg", null, new Color(0.28f, 0.30f, 0.29f), 0.02f, 0.15f, new Vector2(4f, 8f));
        Material woodMat = CreateTexturedMaterial("MaderaAntigua", "Assets/Textures/Wood.jpg", null, new Color(0.58f, 0.42f, 0.28f), 0.05f, 0.38f, Vector2.one);
        Material steelMat = CreateTexturedMaterial("AceroMedico", "Assets/Textures/Steel.jpg", null, new Color(0.68f, 0.71f, 0.73f), 0.78f, 0.58f, Vector2.one);
        Material rustMat = CreateTexturedMaterial("MetalOxidado", "Assets/Textures/Steel.jpg", null, new Color(0.48f, 0.28f, 0.18f), 0.55f, 0.28f, Vector2.one);
        Material linenMat = CreateTexturedMaterial("SabanaHospital", "Assets/Textures/Linen.jpg", null, new Color(0.76f, 0.78f, 0.72f), 0f, 0.18f, Vector2.one);
        Material enamelMat = CreateTexturedMaterial("EsmalteVerde", "Assets/Textures/Enamel.jpg", null, new Color(0.45f, 0.60f, 0.54f), 0.22f, 0.50f, Vector2.one);
        Material vinylMat = CreateTexturedMaterial("ViniloOscuro", "Assets/Textures/Linen.jpg", null, new Color(0.26f, 0.28f, 0.30f), 0.05f, 0.45f, Vector2.one);
        Material rubberMat = CreateTexturedMaterial("GomaNegra", "Assets/Textures/Plaster.jpg", null, new Color(0.14f, 0.15f, 0.16f), 0.05f, 0.20f, Vector2.one);
        Material paperMat = CreateTexturedMaterial("PapelAntiguo", "Assets/Textures/Linen.jpg", null, new Color(0.86f, 0.79f, 0.62f), 0f, 0.15f, Vector2.one);
        Material cardboardMat = CreateTexturedMaterial("CartonArchivo", "Assets/Textures/Wood.jpg", null, new Color(0.68f, 0.52f, 0.34f), 0f, 0.20f, Vector2.one);
        Material porcelainMat = CreateTexturedMaterial("PorcelanaMedica", "Assets/Textures/Enamel.jpg", null, new Color(0.88f, 0.87f, 0.82f), 0.1f, 0.72f, Vector2.one);
        Material glassMat = CreateTexturedMaterial("VidrioMedico", "Assets/Textures/Steel.jpg", null, new Color(0.35f, 0.58f, 0.62f), 0.3f, 0.85f, Vector2.one);
        Material yellowMat = CreateTexturedMaterial("PilaAmarilla", "Assets/Textures/Enamel.jpg", null, new Color(0.85f, 0.66f, 0.20f), 0.35f, 0.52f, Vector2.one);
        Material redMat = CreateTexturedMaterial("RojoMedico", "Assets/Textures/Enamel.jpg", null, new Color(0.58f, 0.12f, 0.09f), 0.2f, 0.48f, Vector2.one);
        Material blueMat = CreateTexturedMaterial("AzulMedico", "Assets/Textures/Enamel.jpg", null, new Color(0.22f, 0.42f, 0.58f), 0.2f, 0.48f, Vector2.one);
        Material darkMat = CreateTexturedMaterial("MetalOscuro", "Assets/Textures/Steel.jpg", null, new Color(0.14f, 0.16f, 0.18f), 0.6f, 0.42f, Vector2.one);
        Material clothMat = CreateTexturedMaterial("RopaSebastian", "Assets/Textures/ClothAlbedo.jpg", "Assets/Textures/ClothNormal.png", new Color(0.65f, 0.65f, 0.65f), 0f, 0.22f, Vector2.one);
        Material skinMat = CreateTexturedMaterial("PielSebastian", "Assets/Textures/FaceAlbedo.jpg", "Assets/Textures/FaceNormal.png", new Color(0.68f, 0.62f, 0.56f), 0f, 0.28f, Vector2.one);

        Material lampBulbMat = CreateEmissiveMaterial("LuzFluorescente", new Color(0.85f, 0.95f, 0.85f), new Color(0.85f, 0.95f, 0.85f) * 2.2f);
        Material redLedMat = CreateEmissiveMaterial("LedRojo", Color.red, Color.red * 2.5f);
        Material greenLedMat = CreateEmissiveMaterial("LedVerde", Color.green, Color.green * 2.5f);
        Material hiddenCodeMat = CreateEmissiveMaterial("PistaOcultaVerde", new Color(0.35f, 1f, 0.45f), new Color(0.35f, 1f, 0.45f) * 3f);

        // Materiales de partículas compatibles con URP (sin cuadrados morados)
        Material dustParticleMat = CreateParticleMaterial("ParticulaPolvoMat", particleTex, new Color(0.82f, 0.86f, 0.82f, 0.22f), false);
        Material sparkParticleMat = CreateParticleMaterial("ParticulaChispaMat", particleTex, new Color(1.0f, 0.85f, 0.45f, 0.95f), true);

        // Mapa de materiales por nombre de submesh (Palette.mtl)
        Dictionary<string, Material> paletteMap = new Dictionary<string, Material>
        {
            { "paint", enamelMat },
            { "steel", steelMat },
            { "rust", rustMat },
            { "linen", linenMat },
            { "dark", darkMat },
            { "wood", woodMat },
            { "paper", paperMat },
            { "glass", glassMat },
            { "red", redMat },
            { "yellow", yellowMat },
            { "blue", blueMat },
            { "skin", skinMat },
            { "hair", darkMat },
            { "white", porcelainMat },
            { "light", lampBulbMat },
            { "green", enamelMat },
            { "vinyl", vinylMat },
            { "cardboard", cardboardMat },
            { "porcelain", porcelainMat },
            { "rubber", rubberMat }
        };

        // 3. Perfil de Post-Procesado URP para Visión Nocturna
        VolumeProfile nvProfile = CreateNightVisionVolumeProfile();

        // 4. Cargar Clips de Audio
        AudioClip themeMusic = LoadAudio("Assets/Audio/Music/Theme.wav");
        AudioClip hospitalMusic = LoadAudio("Assets/Audio/Music/Hospital.wav");
        AudioClip windAmbient = LoadAudio("Assets/Audio/Music/Wind.wav");
        AudioClip stepClip = LoadAudio("Assets/Audio/SFX/Step.wav");
        AudioClip tiredClip = LoadAudio("Assets/Audio/SFX/Heartbeat.wav");
        AudioClip doorOpenClip = LoadAudio("Assets/Audio/SFX/DoorUnlockRE.wav");
        AudioClip doorLockedClip = LoadAudio("Assets/Audio/SFX/PipeKnock.wav");
        AudioClip keyPressClip = LoadAudio("Assets/Audio/SFX/TypewriterKeyRE.wav");
        AudioClip unlockClip = LoadAudio("Assets/Audio/SFX/Unlock.wav");
        AudioClip errorClip = LoadAudio("Assets/Audio/SFX/Error.wav");
        AudioClip pickupClip = LoadAudio("Assets/Audio/SFX/ItemPickupRE.wav");
        AudioClip paperClip = LoadAudio("Assets/Audio/SFX/MapOpenSH.wav");
        AudioClip shutterClip = LoadAudio("Assets/Audio/SFX/Shutter.wav");
        AudioClip buzzClip = LoadAudio("Assets/Audio/SFX/FluorescentBuzzSH.wav");
        AudioClip childSobClip = LoadAudio("Assets/Audio/SFX/SebastianSob.wav");
        AudioClip stingerClip = LoadAudio("Assets/Audio/SFX/ThreatSting.wav");
        AudioClip menuClickClip = LoadAudio("Assets/Audio/SFX/REMenuClick.wav");

        // 5. Crear Prefabs Reutilizables en Assets/Prefabs/
        GameObject sparkPrefab = CreateSparkPoolablePrefab(sparkParticleMat);
        GameObject dustPrefab = CreateDustParticlesPrefab(dustParticleMat);
        GameObject pilaPrefab = CreateBatteryPrefab(paletteMap, yellowMat, pickupClip);
        GameObject docPrefab = CreateDocumentPrefab(paletteMap, paperMat, paperClip);
        GameObject puertaPrefab = CreateDoorPrefab(woodMat, steelMat, doorOpenClip, doorLockedClip);
        GameObject keypadPrefab = CreateKeypadPrefab(steelMat, redLedMat, greenLedMat, keyPressClip, unlockClip, errorClip);
        GameObject lampPrefab = CreateFlickeringLampPrefab(paletteMap, steelMat, lampBulbMat);

        GameObject stretcherPrefab = CreateModelPropPrefab("Camilla", "Assets/Models/Stretcher.obj", paletteMap, linenMat, new Vector3(0.9f, 1.0f, 2.2f));
        GameObject bedPrefab = CreateModelPropPrefab("CamaHospital", "Assets/Models/Bed.obj", paletteMap, linenMat, new Vector3(1.1f, 1.2f, 2.1f));
        GameObject chairPrefab = CreateModelPropPrefab("SillaHospital", "Assets/Models/Chair.obj", paletteMap, woodMat, new Vector3(0.6f, 1.2f, 0.6f));
        GameObject deskPrefab = CreateModelPropPrefab("Escritorio", "Assets/Models/Desk.obj", paletteMap, woodMat, new Vector3(2.6f, 1.0f, 1.1f));
        GameObject cabinetPrefab = CreateModelPropPrefab("Archivador", "Assets/Models/Cabinet.obj", paletteMap, enamelMat, new Vector3(1.2f, 2.0f, 0.7f));
        GameObject lockerPrefab = CreateModelPropPrefab("Casillero", "Assets/Models/Locker.obj", paletteMap, steelMat, new Vector3(1.0f, 2.1f, 0.7f));
        GameObject ivStandPrefab = CreateModelPropPrefab("SoporteSuero", "Assets/Models/IVStand.obj", paletteMap, steelMat, new Vector3(0.5f, 2.0f, 0.5f));
        GameObject wheelchairPrefab = CreateModelPropPrefab("SillaRuedas", "Assets/Models/Wheelchair.obj", paletteMap, steelMat, new Vector3(0.9f, 1.2f, 1.1f));
        GameObject rubblePrefab = CreateModelPropPrefab("Escombros", "Assets/Models/Rubble.obj", paletteMap, wallMat, new Vector3(1.8f, 0.6f, 1.4f));
        GameObject benchPrefab = CreateModelPropPrefab("BancaEspera", "Assets/Models/Bench.obj", paletteMap, woodMat, new Vector3(2.0f, 1.1f, 0.6f));
        GameObject shelfPrefab = CreateModelPropPrefab("EstanteMedico", "Assets/Models/MedicineShelf.obj", paletteMap, enamelMat, new Vector3(1.3f, 2.1f, 0.45f));
        GameObject trolleyPrefab = CreateModelPropPrefab("CarroMedico", "Assets/Models/Trolley.obj", paletteMap, steelMat, new Vector3(0.9f, 1.0f, 0.7f));
        GameObject screenPrefab = CreateModelPropPrefab("BiomboMedico", "Assets/Models/PrivacyScreen.obj", paletteMap, linenMat, new Vector3(1.8f, 2.0f, 0.4f));
        GameObject oxygenPrefab = CreateModelPropPrefab("TanqueOxigeno", "Assets/Models/OxygenTank.obj", paletteMap, enamelMat, new Vector3(0.45f, 1.5f, 0.45f));
        GameObject xrayPrefab = CreateModelPropPrefab("VisorRayosX", "Assets/Models/XRay.obj", paletteMap, steelMat, new Vector3(1.2f, 2.0f, 0.6f));

        // 6. Construir Escena MainMenu
        BuildMainMenuScene(wallMat, floorMat, ceilingMat, lampPrefab, wheelchairPrefab, stretcherPrefab, themeMusic, windAmbient, menuClickClip);

        // 7. Construir Escena HospitalNivel1
        BuildHospitalLevelScene(
            paletteMap, wallMat, floorMat, ceilingMat, woodMat, steelMat, clothMat, hiddenCodeMat,
            nvProfile, sparkPrefab, dustPrefab, pilaPrefab, docPrefab, puertaPrefab, keypadPrefab, lampPrefab,
            stretcherPrefab, bedPrefab, chairPrefab, deskPrefab, cabinetPrefab, lockerPrefab, ivStandPrefab,
            wheelchairPrefab, rubblePrefab, benchPrefab, shelfPrefab, trolleyPrefab, screenPrefab, oxygenPrefab, xrayPrefab,
            hospitalMusic, windAmbient, stepClip, tiredClip, shutterClip, buzzClip, errorClip, childSobClip, stingerClip, menuClickClip);

        // 8. Configurar Build Settings
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(HospitalScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(HospitalScenePath);
        Debug.Log("[Ecos de Chuquicamata] Proyecto actualizado con texturas realistas, partículas URP y estamina.");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? "Assets";
            string name = Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static Texture2D CreateSoftParticleTexture(string path)
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxRadius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Material CreateParticleMaterial(string name, Texture2D particleTex, Color tint, bool additive)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");

        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        mat.SetTexture("_BaseMap", particleTex);
        mat.SetColor("_BaseColor", tint);
        mat.SetFloat("_Surface", 1f); // Transparent
        mat.SetFloat("_Blend", additive ? 2f : 0f);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material CreateTexturedMaterial(
        string name,
        string albedoPath,
        string normalPath,
        Color tint,
        float metallic,
        float smoothness,
        Vector2 tiling)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }

        Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        if (albedo != null)
        {
            mat.SetTexture("_BaseMap", albedo);
            mat.mainTextureScale = tiling;
        }

        if (!string.IsNullOrEmpty(normalPath))
        {
            TextureImporter normImp = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (normImp != null && normImp.textureType != TextureImporterType.NormalMap)
            {
                normImp.textureType = TextureImporterType.NormalMap;
                normImp.SaveAndReimport();
            }
            Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (normalTex != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetFloat("_BumpScale", 1.0f);
            }
        }

        mat.SetColor("_BaseColor", tint);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material CreateEmissiveMaterial(string name, Color baseColor, Color emissionColor)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", baseColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void ApplyRealisticSubmaterials(GameObject root, Dictionary<string, Material> paletteMap, Material fallbackMat)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            Material[] currentMats = r.sharedMaterials;
            if (currentMats == null || currentMats.Length == 0)
            {
                r.sharedMaterial = fallbackMat;
                continue;
            }

            Material[] newMats = new Material[currentMats.Length];
            for (int m = 0; m < currentMats.Length; m++)
            {
                string rawName = currentMats[m] != null ? currentMats[m].name.ToLowerInvariant() : "";
                Material matched = fallbackMat;

                foreach (var kvp in paletteMap)
                {
                    if (rawName.Contains(kvp.Key))
                    {
                        matched = kvp.Value;
                        break;
                    }
                }
                newMats[m] = matched;
            }
            r.sharedMaterials = newMats;
        }
    }

    private static VolumeProfile CreateNightVisionVolumeProfile()
    {
        const string path = "Assets/Settings/NightVisionProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }

        if (!profile.TryGet(out ColorAdjustments colorAdj))
        {
            colorAdj = profile.Add<ColorAdjustments>(true);
        }
        colorAdj.postExposure.overrideState = true;
        colorAdj.postExposure.value = 2.4f;
        colorAdj.colorFilter.overrideState = true;
        colorAdj.colorFilter.value = new Color(0.50f, 1.0f, 0.58f, 1f);
        colorAdj.contrast.overrideState = true;
        colorAdj.contrast.value = 20f;
        colorAdj.saturation.overrideState = true;
        colorAdj.saturation.value = -50f;

        if (!profile.TryGet(out FilmGrain grain))
        {
            grain = profile.Add<FilmGrain>(true);
        }
        grain.type.overrideState = true;
        grain.type.value = FilmGrainLookup.Medium3;
        grain.intensity.overrideState = true;
        grain.intensity.value = 0.7f;

        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.42f;

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static AudioClip LoadAudio(string path)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static GameObject SavePrefab(GameObject root, string prefabName)
    {
        string path = $"Assets/Prefabs/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject InstantiateModelWithMaterials(
        string modelPath,
        Dictionary<string, Material> paletteMap,
        Material fallbackMat,
        PrimitiveType fallbackType,
        Vector3 fallbackScale)
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        GameObject instance;
        if (modelAsset != null)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            ApplyRealisticSubmaterials(instance, paletteMap, fallbackMat);
        }
        else
        {
            instance = GameObject.CreatePrimitive(fallbackType);
            instance.transform.localScale = fallbackScale;
            instance.GetComponent<Renderer>().sharedMaterial = fallbackMat;
        }
        return instance;
    }

    private static GameObject CreateModelPropPrefab(
        string prefabName,
        string modelPath,
        Dictionary<string, Material> paletteMap,
        Material fallbackMat,
        Vector3 colliderSize)
    {
        GameObject root = new GameObject(prefabName);
        GameObject model = InstantiateModelWithMaterials(modelPath, paletteMap, fallbackMat, PrimitiveType.Cube, colliderSize);
        model.name = "Model";
        model.transform.SetParent(root.transform, false);

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = colliderSize;
        box.center = new Vector3(0f, colliderSize.y * 0.5f, 0f);

        return SavePrefab(root, prefabName);
    }

    private static GameObject CreateSparkPoolablePrefab(Material sparkMat)
    {
        GameObject root = new GameObject("ChispaLuz");
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = 0.35f;
        main.startSpeed = 2.8f;
        main.startSize = 0.045f;
        main.maxParticles = 25;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });

        ParticleSystemRenderer psr = root.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = sparkMat;

        PoolableObject poolable = root.AddComponent<PoolableObject>();
        poolable.lifeTime = 1.0f;

        return SavePrefab(root, "ChispaLuz");
    }

    private static GameObject CreateDustParticlesPrefab(Material dustMat)
    {
        GameObject root = new GameObject("PolvoAmbiental");
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 6f;
        main.startSpeed = 0.06f;
        main.startSize = 0.03f;
        main.maxParticles = 100;
        main.startColor = new Color(0.82f, 0.86f, 0.82f, 0.22f);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(12f, 3f, 24f);

        ParticleSystemRenderer psr = root.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = dustMat;

        return SavePrefab(root, "PolvoAmbiental");
    }

    private static GameObject CreateBatteryPrefab(Dictionary<string, Material> paletteMap, Material fallbackMat, AudioClip pickupClip)
    {
        GameObject root = new GameObject("Pila");
        GameObject model = InstantiateModelWithMaterials("Assets/Models/Battery.obj", paletteMap, fallbackMat, PrimitiveType.Cylinder, new Vector3(0.12f, 0.2f, 0.12f));
        model.name = "Model";
        model.transform.SetParent(root.transform, false);
        model.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.12f, 0f);
        col.size = new Vector3(0.28f, 0.28f, 0.28f);

        BatteryPickup bp = root.AddComponent<BatteryPickup>();
        SerializedObject so = new SerializedObject(bp);
        so.FindProperty("interactionText").stringValue = "Recoger pila (+40%)";
        so.FindProperty("chargeAmount").floatValue = 40f;
        so.FindProperty("pickupClip").objectReferenceValue = pickupClip;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "Pila");
    }

    private static GameObject CreateDocumentPrefab(Dictionary<string, Material> paletteMap, Material paperMat, AudioClip readClip)
    {
        GameObject root = new GameObject("Documento");
        GameObject sheet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sheet.name = "HojaPapel";
        sheet.transform.SetParent(root.transform, false);
        sheet.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        sheet.transform.localScale = new Vector3(0.30f, 0.015f, 0.38f);
        sheet.GetComponent<Renderer>().sharedMaterial = paperMat;
        Object.DestroyImmediate(sheet.GetComponent<Collider>());

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.03f, 0f);
        col.size = new Vector3(0.38f, 0.10f, 0.44f);

        DocumentItem doc = root.AddComponent<DocumentItem>();
        SerializedObject so = new SerializedObject(doc);
        so.FindProperty("interactionText").stringValue = "Leer documento";
        so.FindProperty("readClip").objectReferenceValue = readClip;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "Documento");
    }

    private static GameObject CreateDoorPrefab(Material woodMat, Material steelMat, AudioClip openClip, AudioClip lockedClip)
    {
        GameObject root = new GameObject("PuertaHospital");
        GameObject hinge = new GameObject("Hinge");
        hinge.transform.SetParent(root.transform, false);
        hinge.transform.localPosition = Vector3.zero;

        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leaf.name = "HojaPuerta";
        leaf.transform.SetParent(hinge.transform, false);
        leaf.transform.localPosition = new Vector3(1.0f, 1.35f, 0f);
        leaf.transform.localScale = new Vector3(2.0f, 2.7f, 0.10f);
        leaf.GetComponent<Renderer>().sharedMaterial = woodMat;

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        handle.name = "Manilla";
        handle.transform.SetParent(hinge.transform, false);
        handle.transform.localPosition = new Vector3(1.82f, 1.05f, -0.08f);
        handle.transform.localScale = new Vector3(0.09f, 0.09f, 0.15f);
        handle.GetComponent<Renderer>().sharedMaterial = steelMat;
        Object.DestroyImmediate(handle.GetComponent<Collider>());

        DoorController dc = root.AddComponent<DoorController>();
        SerializedObject so = new SerializedObject(dc);
        so.FindProperty("doorHinge").objectReferenceValue = hinge.transform;
        so.FindProperty("openClip").objectReferenceValue = openClip;
        so.FindProperty("lockedClip").objectReferenceValue = lockedClip;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "PuertaHospital");
    }

    private static GameObject CreateKeypadPrefab(Material bodyMat, Material redLedMat, Material greenLedMat, AudioClip keyClip, AudioClip okClip, AudioClip errClip)
    {
        GameObject root = new GameObject("TecladoNumerico");
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "CuerpoTeclado";
        box.transform.SetParent(root.transform, false);
        box.transform.localScale = new Vector3(0.28f, 0.38f, 0.08f);
        box.GetComponent<Renderer>().sharedMaterial = bodyMat;

        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        led.name = "StatusLED";
        led.transform.SetParent(root.transform, false);
        led.transform.localPosition = new Vector3(0f, 0.13f, -0.045f);
        led.transform.localScale = new Vector3(0.045f, 0.045f, 0.025f);
        Renderer ledRenderer = led.GetComponent<Renderer>();
        ledRenderer.sharedMaterial = redLedMat;
        Object.DestroyImmediate(led.GetComponent<Collider>());

        KeypadPuzzle kp = root.AddComponent<KeypadPuzzle>();
        SerializedObject so = new SerializedObject(kp);
        so.FindProperty("interactionText").stringValue = "Usar teclado numérico";
        so.FindProperty("correctCode").stringValue = "1985";
        so.FindProperty("statusLightRenderer").objectReferenceValue = ledRenderer;
        so.FindProperty("unlockedLightMaterial").objectReferenceValue = greenLedMat;
        so.FindProperty("keyPressClip").objectReferenceValue = keyClip;
        so.FindProperty("successClip").objectReferenceValue = okClip;
        so.FindProperty("errorClip").objectReferenceValue = errClip;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "TecladoNumerico");
    }

    private static GameObject CreateFlickeringLampPrefab(Dictionary<string, Material> paletteMap, Material steelMat, Material bulbMat)
    {
        GameObject root = new GameObject("LuzParpadeante");
        GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        housing.name = "Carcasa";
        housing.transform.SetParent(root.transform, false);
        housing.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        housing.transform.localScale = new Vector3(0.28f, 0.06f, 1.15f);
        housing.GetComponent<Renderer>().sharedMaterial = steelMat;
        Object.DestroyImmediate(housing.GetComponent<Collider>());

        GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bulb.name = "TuboFluorescente";
        bulb.transform.SetParent(root.transform, false);
        bulb.transform.localScale = new Vector3(0.16f, 0.04f, 1.05f);
        Renderer bulbRenderer = bulb.GetComponent<Renderer>();
        bulbRenderer.sharedMaterial = bulbMat;
        Object.DestroyImmediate(bulb.GetComponent<Collider>());

        Light pointLight = root.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(0.82f, 0.92f, 0.78f);
        pointLight.range = 10f;
        pointLight.intensity = 1.4f;
        pointLight.shadows = LightShadows.Soft;

        FlickeringLight fl = root.AddComponent<FlickeringLight>();
        SerializedObject so = new SerializedObject(fl);
        so.FindProperty("lampBulbRenderer").objectReferenceValue = bulbRenderer;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "LuzParpadeante");
    }

    private static void BuildMainMenuScene(
        Material wallMat, Material floorMat, Material ceilingMat,
        GameObject lampPrefab, GameObject wheelchairPrefab, GameObject stretcherPrefab,
        AudioClip themeMusic, AudioClip windAmbient, AudioClip clickClip)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.035f, 0.03f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.01f, 0.015f, 0.012f);
        RenderSettings.fogDensity = 0.06f;

        GameObject bgRoot = new GameObject("EscenarioMenu");
        CreateBox(bgRoot.transform, "Piso", new Vector3(0f, -0.1f, 4f), new Vector3(8f, 0.2f, 14f), floorMat);
        CreateBox(bgRoot.transform, "Techo", new Vector3(0f, 3.5f, 4f), new Vector3(8f, 0.2f, 14f), ceilingMat);
        CreateBox(bgRoot.transform, "ParedIzq", new Vector3(-3f, 1.7f, 4f), new Vector3(0.25f, 3.4f, 14f), wallMat);
        CreateBox(bgRoot.transform, "ParedDer", new Vector3(3f, 1.7f, 4f), new Vector3(0.25f, 3.4f, 14f), wallMat);
        CreateBox(bgRoot.transform, "ParedFondo", new Vector3(0f, 1.7f, 10f), new Vector3(6f, 3.4f, 0.25f), wallMat);
        PlacePrefab(lampPrefab, bgRoot.transform, new Vector3(0f, 3.35f, 5f), 0f);
        PlacePrefab(wheelchairPrefab, bgRoot.transform, new Vector3(-0.8f, 0f, 6.2f), 145f);
        PlacePrefab(stretcherPrefab, bgRoot.transform, new Vector3(1.6f, 0f, 7.5f), -20f);

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0f, 1.55f, 0f);
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<UniversalAdditionalCameraData>();

        CreateAudioManagerObject(themeMusic, windAmbient);
        CreateEventSystem();

        GameObject canvasObj = CreateCanvas("MainMenuCanvas");
        MainMenuController menuCtrl = canvasObj.AddComponent<MainMenuController>();

        GameObject bgPanel = CreatePanel(canvasObj.transform, "MenuOverlay", new Color(0.01f, 0.02f, 0.015f, 0.72f));
        CreateTMP(bgPanel.transform, "Titulo", "ECOS DE CHUQUICAMATA", new Vector2(0f, 210f), new Vector2(1100f, 90f), 58f, new Color(0.80f, 0.92f, 0.78f));
        CreateTMP(bgPanel.transform, "Subtitulo", "Hospital Roy H. Glover — Capítulo 1 (Cátedra 1)", new Vector2(0f, 140f), new Vector2(900f, 45f), 28f, new Color(0.65f, 0.75f, 0.65f));

        CreateButton(bgPanel.transform, "BtnJugar", "JUGAR", new Vector2(0f, 25f), new Vector2(300f, 56f), menuCtrl.PlayGame);
        CreateButton(bgPanel.transform, "BtnControles", "CONTROLES", new Vector2(0f, -45f), new Vector2(300f, 52f), menuCtrl.ToggleControlsPanel);
        CreateButton(bgPanel.transform, "BtnSalir", "SALIR", new Vector2(0f, -115f), new Vector2(300f, 52f), menuCtrl.QuitGame);

        CreateTMP(bgPanel.transform, "VolLabel", "VOLUMEN GENERAL", new Vector2(0f, -195f), new Vector2(320f, 34f), 20f, Color.white);
        Slider volSlider = CreateSlider(bgPanel.transform, "VolumeSlider", new Vector2(0f, -230f), new Vector2(300f, 28f), menuCtrl.OnVolumeChanged);

        GameObject controlsPanel = CreatePanel(bgPanel.transform, "ControlsPanel", new Color(0.05f, 0.07f, 0.06f, 0.96f));
        RectTransform cpRt = controlsPanel.GetComponent<RectTransform>();
        cpRt.anchorMin = cpRt.anchorMax = new Vector2(0.5f, 0.5f);
        cpRt.anchoredPosition = new Vector2(450f, -20f);
        cpRt.sizeDelta = new Vector2(460f, 340f);
        CreateTMP(controlsPanel.transform, "ControlsInfo",
            "CONTROLES\n\nWASD: Movimiento\nMouse: Mirar\nShift: Correr (consume estamina)\nCtrl: Agacharse\nE: Interactuar / Leer\nClic Derecho: Levantar / Bajar Cámara\nF: Visión Nocturna\nEsc: Pausa",
            Vector2.zero, new Vector2(420f, 300f), 22f, Color.white);
        controlsPanel.SetActive(false);

        SerializedObject so = new SerializedObject(menuCtrl);
        so.FindProperty("firstLevelSceneName").stringValue = "HospitalNivel1";
        so.FindProperty("controlsPanel").objectReferenceValue = controlsPanel;
        so.FindProperty("volumeSlider").objectReferenceValue = volSlider;
        so.FindProperty("menuMusicClip").objectReferenceValue = themeMusic;
        so.FindProperty("buttonClickClip").objectReferenceValue = clickClip;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    private static void BuildHospitalLevelScene(
        Dictionary<string, Material> paletteMap,
        Material wallMat, Material floorMat, Material ceilingMat, Material woodMat, Material steelMat,
        Material clothMat, Material hiddenCodeMat,
        VolumeProfile nvProfile, GameObject sparkPrefab, GameObject dustPrefab, GameObject pilaPrefab,
        GameObject docPrefab, GameObject puertaPrefab, GameObject keypadPrefab, GameObject lampPrefab,
        GameObject stretcherPrefab, GameObject bedPrefab, GameObject chairPrefab, GameObject deskPrefab,
        GameObject cabinetPrefab, GameObject lockerPrefab, GameObject ivStandPrefab, GameObject wheelchairPrefab,
        GameObject rubblePrefab, GameObject benchPrefab, GameObject shelfPrefab, GameObject trolleyPrefab,
        GameObject screenPrefab, GameObject oxygenPrefab, GameObject xrayPrefab,
        AudioClip hospitalMusic, AudioClip windAmbient, AudioClip stepClip, AudioClip tiredClip,
        AudioClip shutterClip, AudioClip buzzClip, AudioClip errorClip, AudioClip childSobClip,
        AudioClip stingerClip, AudioClip clickClip)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.035f, 0.032f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.015f, 0.02f, 0.018f);
        RenderSettings.fogDensity = 0.08f;

        CreateAudioManagerObject(hospitalMusic, windAmbient);

        // Object Pool de Chispas + Polvo Ambiental con material URP
        GameObject effectsRoot = new GameObject("Efectos_Y_ObjectPool");
        SimpleObjectPool sparkPool = effectsRoot.AddComponent<SimpleObjectPool>();
        sparkPool.gameObjectToPool = sparkPrefab;
        sparkPool.poolSize = 8;
        sparkPool.poolCanExpand = true;

        GameObject dustInst = PlacePrefab(dustPrefab, effectsRoot.transform, new Vector3(0f, 1.8f, 10f), 0f);
        dustInst.name = "PolvoAmbiental";

        // Arquitectura del Hospital Roy H. Glover
        GameObject hospitalRoot = new GameObject("Escenario_HospitalRoyHGlover");
        BuildHospitalArchitecture(hospitalRoot.transform, wallMat, floorMat, ceilingMat);

        // Objetos 3D con texturas realistas
        GameObject propsRoot = new GameObject("Objetos_E_Interactuables");

        // --- Recepción ---
        PlacePrefab(deskPrefab, propsRoot.transform, new Vector3(-2.4f, 0f, 0.6f), 0f);
        PlacePrefab(deskPrefab, propsRoot.transform, new Vector3(4.1f, 0f, -1.6f), -90f);
        PlacePrefab(chairPrefab, propsRoot.transform, new Vector3(-2.4f, 0f, 1.6f), 175f);
        PlacePrefab(benchPrefab, propsRoot.transform, new Vector3(3.6f, 0f, -5.2f), 0f);
        PlacePrefab(benchPrefab, propsRoot.transform, new Vector3(-3.6f, 0f, -5.2f), 0f);
        PlacePrefab(wheelchairPrefab, propsRoot.transform, new Vector3(-4.4f, 0f, -2.6f), 40f);
        PlacePrefab(cabinetPrefab, propsRoot.transform, new Vector3(-5.2f, 0f, 0.8f), 90f);
        PlacePrefab(lockerPrefab, propsRoot.transform, new Vector3(-5.2f, 0f, 2.3f), 90f);
        PlacePrefab(rubblePrefab, propsRoot.transform, new Vector3(4.4f, 0f, 2.5f), 15f);

        CreateWallSign(hospitalRoot.transform, "HOSPITAL ROY H. GLOVER\nCHUQUICAMATA — RECEPCIÓN", new Vector3(0f, 2.55f, 3.84f), Vector3.zero, 3.4f);

        GameObject notaRecepcion = PlacePrefab(docPrefab, propsRoot.transform, new Vector3(-2.3f, 0.98f, 0.55f), 10f);
        notaRecepcion.name = "Nota_Recepcion";
        ConfigureDocument(notaRecepcion.GetComponent<DocumentItem>(),
            "Leer nota de recepción",
            "Nota en Recepción — Hospital Roy H. Glover",
            "\"La fecha de aquel incidente sigue escrita en estas paredes, aunque nadie pueda verla.\"\n\n" +
            "Levanta la cámara de video (Clic derecho) y activa la visión nocturna (Tecla F) para inspeccionar la pared deteriorada de la Sala de Enfermería.\n\n" +
            "Ese año de 4 dígitos abre el teclado numérico de la puerta de Administración.",
            false);

        // Pilas repartidas por el hospital
        PlacePrefab(pilaPrefab, propsRoot.transform, new Vector3(4.1f, 0.98f, -1.6f), 0f);
        PlacePrefab(pilaPrefab, propsRoot.transform, new Vector3(-1.5f, 0.98f, 0.55f), 30f);

        GameObject recLamp = PlacePrefab(lampPrefab, propsRoot.transform, new Vector3(0f, 3.35f, -0.5f), 0f);
        BindLampPool(recLamp, sparkPool);

        // --- Pasillo y Sala de Enfermería (Oeste) ---
        GameObject corrLamp = PlacePrefab(lampPrefab, propsRoot.transform, new Vector3(0f, 3.35f, 10.5f), 0f);
        BindLampPool(corrLamp, sparkPool);

        PlacePrefab(puertaPrefab, propsRoot.transform, new Vector3(-2.0f, 0f, 8.0f), 90f);
        PlacePrefab(stretcherPrefab, propsRoot.transform, new Vector3(-6.5f, 0f, 7.2f), 90f);
        PlacePrefab(bedPrefab, propsRoot.transform, new Vector3(-6.5f, 0f, 11.6f), 90f);
        PlacePrefab(ivStandPrefab, propsRoot.transform, new Vector3(-7.6f, 0f, 12.6f), 0f);
        PlacePrefab(trolleyPrefab, propsRoot.transform, new Vector3(-4.8f, 0f, 6.2f), 20f);
        PlacePrefab(screenPrefab, propsRoot.transform, new Vector3(-5.0f, 0f, 9.5f), 85f);
        PlacePrefab(oxygenPrefab, propsRoot.transform, new Vector3(-8.2f, 0f, 6.0f), 0f);
        PlacePrefab(shelfPrefab, propsRoot.transform, new Vector3(-3.4f, 0f, 13.4f), 180f);
        PlacePrefab(xrayPrefab, propsRoot.transform, new Vector3(-5.2f, 0f, 13.4f), 180f);
        PlacePrefab(pilaPrefab, propsRoot.transform, new Vector3(-4.8f, 0.98f, 6.2f), 0f);

        HiddenCode hiddenCode1 = CreateHiddenWallCode(
            propsRoot.transform,
            "PistaOculta_1985",
            "1 9 8 5\n<size=42%>INCIDENTE — SEBASTIÁN</size>",
            new Vector3(-8.82f, 1.68f, 9.5f),
            new Vector3(0f, -90f, 0f),
            hiddenCodeMat);

        HiddenCode hiddenCode2 = CreateHiddenWallCode(
            propsRoot.transform,
            "PistaOculta_Pasillo",
            "1985",
            new Vector3(-1.84f, 1.55f, 16.5f),
            new Vector3(0f, -90f, 0f),
            hiddenCodeMat);

        // --- Sala de Espera / Archivo Menor (Este) ---
        PlacePrefab(deskPrefab, propsRoot.transform, new Vector3(6.2f, 0f, 15.2f), 180f);
        PlacePrefab(chairPrefab, propsRoot.transform, new Vector3(3.5f, 0f, 12.0f), 0f);
        PlacePrefab(cabinetPrefab, propsRoot.transform, new Vector3(7.2f, 0f, 12.4f), -90f);
        PlacePrefab(lockerPrefab, propsRoot.transform, new Vector3(7.2f, 0f, 13.8f), -90f);
        PlacePrefab(pilaPrefab, propsRoot.transform, new Vector3(6.4f, 0.98f, 15.2f), 25f);

        GameObject notaTurno = PlacePrefab(docPrefab, propsRoot.transform, new Vector3(5.8f, 0.98f, 15.1f), -15f);
        notaTurno.name = "Nota_Turno";
        ConfigureDocument(notaTurno.GetComponent<DocumentItem>(),
            "Leer registro médico",
            "Registro de Turno — Chuquicamata",
            "\"Desde que cerraron el pabellón pediátrico, las luces del sector administrativo parpadean cada noche.\n\n" +
            "El expediente confidencial de Sebastián quedó bajo llave detrás de la puerta con teclado numérico.\"",
            false);

        // --- Puerta de Administración, Teclado Numérico y Trigger de Victoria ---
        CreateWallSign(hospitalRoot.transform, "ADMINISTRACIÓN", new Vector3(0f, 2.75f, 19.84f), Vector3.zero, 2.8f);
        GameObject adminDoorObj = PlacePrefab(puertaPrefab, propsRoot.transform, new Vector3(-1.0f, 0f, 20.0f), 0f);
        adminDoorObj.name = "PuertaAdministracion";
        DoorController adminDoor = adminDoorObj.GetComponent<DoorController>();
        SerializedObject doorSo = new SerializedObject(adminDoor);
        doorSo.FindProperty("isLocked").boolValue = true;
        doorSo.ApplyModifiedPropertiesWithoutUndo();

        GameObject keypadObj = PlacePrefab(keypadPrefab, propsRoot.transform, new Vector3(1.35f, 1.35f, 19.82f), 0f);
        KeypadPuzzle keypadPuzzle = keypadObj.GetComponent<KeypadPuzzle>();
        SerializedObject kpSo = new SerializedObject(keypadPuzzle);
        kpSo.FindProperty("targetDoor").objectReferenceValue = adminDoor;
        kpSo.ApplyModifiedPropertiesWithoutUndo();

        // Trigger al cruzar la puerta de Administración (OnTriggerEnter)
        GameObject victoryTriggerObj = new GameObject("TriggerPuertaAdministracion");
        victoryTriggerObj.transform.SetParent(propsRoot.transform, false);
        victoryTriggerObj.transform.position = new Vector3(0f, 1.2f, 20.8f);
        BoxCollider trigCol = victoryTriggerObj.AddComponent<BoxCollider>();
        trigCol.isTrigger = true;
        trigCol.size = new Vector3(2.4f, 2.5f, 1.2f);
        victoryTriggerObj.AddComponent<VictoryTrigger>();

        // --- Sector Administrativo y Secuencia Final ---
        PlacePrefab(deskPrefab, propsRoot.transform, new Vector3(0f, 0f, 23.4f), 0f);
        PlacePrefab(cabinetPrefab, propsRoot.transform, new Vector3(-3.5f, 0f, 22.2f), 90f);
        PlacePrefab(cabinetPrefab, propsRoot.transform, new Vector3(3.5f, 0f, 22.2f), -90f);
        PlacePrefab(chairPrefab, propsRoot.transform, new Vector3(0f, 0f, 24.5f), 180f);

        GameObject endLampObj = PlacePrefab(lampPrefab, propsRoot.transform, new Vector3(0f, 3.35f, 28.0f), 0f);
        BindLampPool(endLampObj, sparkPool);
        FlickeringLight endCorridorLight = endLampObj.GetComponent<FlickeringLight>();

        // Silueta de Sebastián con sus texturas reales
        GameObject silhouetteObj = InstantiateModelWithMaterials("Assets/Models/Sebastian.obj", paletteMap, clothMat, PrimitiveType.Capsule, new Vector3(0.5f, 1.5f, 0.5f));
        silhouetteObj.name = "SiluetaSebastian";
        silhouetteObj.transform.SetParent(propsRoot.transform, false);
        silhouetteObj.transform.position = new Vector3(0f, 0f, 28.8f);
        silhouetteObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        silhouetteObj.SetActive(false);

        GameObject expedienteObj = PlacePrefab(docPrefab, propsRoot.transform, new Vector3(0f, 0.98f, 23.35f), 5f);
        expedienteObj.name = "Expediente_Sebastian";
        ConfigureDocument(expedienteObj.GetComponent<DocumentItem>(),
            "Inspeccionar expediente de Sebastián",
            "Expediente Médico Confidencial — Sebastián (1985)",
            "HOSPITAL ROY H. GLOVER — CHUQUICAMATA\n" +
            "Año de ingreso: 1985\n" +
            "Paciente: Sebastián M. (Pabellón Pediátrico)\n\n" +
            "Observaciones: Tras el incidente nocturno en el ala norte, el paciente desapareció sin dejar rastro.\n\n" +
            "Victoria, espera a que continuemos la historia...",
            true);

        // Jugador FPS + Cámara + Visión Nocturna + Estamina
        PlayerReferences playerRefs = BuildPlayerObject(
            paletteMap,
            steelMat,
            new Vector3(0f, 0f, -4.2f),
            nvProfile,
            new[] { hiddenCode1, hiddenCode2 },
            stepClip, tiredClip, shutterClip, buzzClip, errorClip);

        // UIManager + Canvas + GameManager
        CreateEventSystem();
        BuildGameplayUIAndManagers(
            playerRefs,
            silhouetteObj,
            endCorridorLight,
            childSobClip,
            stingerClip,
            clickClip);

        EditorSceneManager.SaveScene(scene, HospitalScenePath);
    }

    private struct PlayerReferences
    {
        public PlayerMovement movement;
        public PlayerInteraction interaction;
        public VideoCameraController videoCamera;
    }

    private static PlayerReferences BuildPlayerObject(
        Dictionary<string, Material> paletteMap,
        Material fallbackMat,
        Vector3 position,
        VolumeProfile nvProfile,
        HiddenCode[] hiddenCodes,
        AudioClip stepClip,
        AudioClip tiredClip,
        AudioClip shutterClip,
        AudioClip buzzClip,
        AudioClip errorClip)
    {
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.transform.position = position;

        CharacterController cc = playerObj.AddComponent<CharacterController>();
        cc.radius = 0.35f;
        cc.height = 1.8f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        AudioSource stepSource = playerObj.AddComponent<AudioSource>();
        stepSource.spatialBlend = 0f;
        stepSource.playOnAwake = false;

        PlayerMovement movement = playerObj.AddComponent<PlayerMovement>();
        PlayerInteraction interaction = playerObj.AddComponent<PlayerInteraction>();
        CameraBattery battery = playerObj.AddComponent<CameraBattery>();

        GameObject cameraHolder = new GameObject("CameraHolder");
        cameraHolder.transform.SetParent(playerObj.transform, false);
        cameraHolder.transform.localPosition = new Vector3(0f, 1.65f, 0f);

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.SetParent(cameraHolder.transform, false);
        Camera cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.08f;
        cam.fieldOfView = 65f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camObj.AddComponent<AudioListener>();
        UniversalAdditionalCameraData camData = camObj.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        GameObject videoCamObj = new GameObject("VideoCamera");
        videoCamObj.transform.SetParent(camObj.transform, false);
        AudioSource camAudio = videoCamObj.AddComponent<AudioSource>();
        camAudio.spatialBlend = 0f;
        camAudio.playOnAwake = false;

        GameObject camModel = InstantiateModelWithMaterials("Assets/Models/Camcorder.obj", paletteMap, fallbackMat, PrimitiveType.Cube, new Vector3(0.1f, 0.1f, 0.2f));
        camModel.name = "CamcorderModel";
        camModel.transform.SetParent(videoCamObj.transform, false);
        camModel.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        foreach (Collider c in camModel.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(c);
        }

        GameObject irLightObj = new GameObject("InfraredLight");
        irLightObj.transform.SetParent(videoCamObj.transform, false);
        Light irLight = irLightObj.AddComponent<Light>();
        irLight.type = LightType.Spot;
        irLight.range = 24f;
        irLight.spotAngle = 75f;
        irLight.intensity = 4.2f;
        irLight.color = new Color(0.70f, 1.0f, 0.78f);
        irLight.enabled = false;

        GameObject nvVolObj = new GameObject("NightVisionVolume");
        nvVolObj.transform.SetParent(videoCamObj.transform, false);
        Volume nvVolume = nvVolObj.AddComponent<Volume>();
        nvVolume.isGlobal = true;
        nvVolume.priority = 10f;
        nvVolume.weight = 0f;
        nvVolume.sharedProfile = nvProfile;

        NightVisionController nvCtrl = videoCamObj.AddComponent<NightVisionController>();
        VideoCameraController vcCtrl = videoCamObj.AddComponent<VideoCameraController>();

        SerializedObject movSo = new SerializedObject(movement);
        movSo.FindProperty("cameraHolder").objectReferenceValue = cameraHolder.transform;
        movSo.FindProperty("footstepSource").objectReferenceValue = stepSource;
        movSo.FindProperty("tiredClip").objectReferenceValue = tiredClip;
        SerializedProperty stepsProp = movSo.FindProperty("footstepClips");
        stepsProp.arraySize = 1;
        stepsProp.GetArrayElementAtIndex(0).objectReferenceValue = stepClip;
        movSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject intSo = new SerializedObject(interaction);
        intSo.FindProperty("playerCamera").objectReferenceValue = camObj.transform;
        intSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject nvSo = new SerializedObject(nvCtrl);
        nvSo.FindProperty("battery").objectReferenceValue = battery;
        nvSo.FindProperty("nightVisionVolume").objectReferenceValue = nvVolume;
        nvSo.FindProperty("infraredLight").objectReferenceValue = irLight;
        nvSo.FindProperty("audioSource").objectReferenceValue = camAudio;
        nvSo.FindProperty("turnOnClip").objectReferenceValue = buzzClip;
        nvSo.FindProperty("turnOffClip").objectReferenceValue = shutterClip;
        nvSo.FindProperty("noBatteryClip").objectReferenceValue = errorClip;
        SerializedProperty codesProp = nvSo.FindProperty("hiddenCodes");
        codesProp.arraySize = hiddenCodes.Length;
        for (int i = 0; i < hiddenCodes.Length; i++)
        {
            codesProp.GetArrayElementAtIndex(i).objectReferenceValue = hiddenCodes[i];
        }
        nvSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject vcSo = new SerializedObject(vcCtrl);
        vcSo.FindProperty("nightVision").objectReferenceValue = nvCtrl;
        vcSo.FindProperty("cameraModel").objectReferenceValue = camModel.transform;
        vcSo.FindProperty("playerCamera").objectReferenceValue = cam;
        vcSo.FindProperty("audioSource").objectReferenceValue = camAudio;
        vcSo.FindProperty("raiseClip").objectReferenceValue = shutterClip;
        vcSo.FindProperty("lowerClip").objectReferenceValue = shutterClip;
        vcSo.ApplyModifiedPropertiesWithoutUndo();

        return new PlayerReferences
        {
            movement = movement,
            interaction = interaction,
            videoCamera = vcCtrl
        };
    }

    private static void BuildGameplayUIAndManagers(
        PlayerReferences playerRefs,
        GameObject sebastianSilhouette,
        FlickeringLight endCorridorLight,
        AudioClip childSobClip,
        AudioClip stingerClip,
        AudioClip clickClip)
    {
        GameObject canvasObj = CreateCanvas("UIManager_Canvas");
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // Retícula
        GameObject crosshair = CreatePanel(canvasObj.transform, "Crosshair", new Color(1f, 1f, 1f, 0.55f));
        RectTransform chRt = crosshair.GetComponent<RectTransform>();
        chRt.anchorMin = chRt.anchorMax = new Vector2(0.5f, 0.5f);
        chRt.anchoredPosition = Vector2.zero;
        chRt.sizeDelta = new Vector2(6f, 6f);

        // Barra de Estamina (Esquina inferior izquierda)
        GameObject stamBg = CreatePanel(canvasObj.transform, "StaminaBackground", new Color(0f, 0f, 0f, 0.55f));
        RectTransform stamBgRt = stamBg.GetComponent<RectTransform>();
        stamBgRt.anchorMin = stamBgRt.anchorMax = new Vector2(0f, 0f);
        stamBgRt.anchoredPosition = new Vector2(150f, 55f);
        stamBgRt.sizeDelta = new Vector2(210f, 18f);

        GameObject stamFillObj = CreatePanel(stamBg.transform, "StaminaFill", new Color(0.75f, 0.88f, 0.78f, 0.85f));
        Image stamFillImg = stamFillObj.GetComponent<Image>();
        stamFillImg.type = Image.Type.Filled;
        stamFillImg.fillMethod = Image.FillMethod.Horizontal;
        CreateTMP(canvasObj.transform, "StaminaLabel", "ESTAMINA", new Vector2(150f, 80f), new Vector2(210f, 24f), 16f, new Color(0.8f, 0.85f, 0.8f), AnchorPreset.BottomLeft);

        // HUD de Cámara y Visión Nocturna
        GameObject camHudPanel = CreatePanel(canvasObj.transform, "CameraHUDPanel", new Color(0f, 0f, 0f, 0f));
        GameObject nvOverlay = CreatePanel(camHudPanel.transform, "NightVisionOverlay", new Color(0.05f, 0.32f, 0.08f, 0.24f));
        nvOverlay.GetComponent<Image>().raycastTarget = false;

        GameObject noiseObj = new GameObject("NoiseRawImage");
        noiseObj.transform.SetParent(nvOverlay.transform, false);
        RectTransform noiseRt = noiseObj.AddComponent<RectTransform>();
        StretchFull(noiseRt);
        RawImage noiseRaw = noiseObj.AddComponent<RawImage>();
        noiseRaw.color = new Color(0.5f, 1f, 0.55f, 0.28f);
        noiseRaw.raycastTarget = false;

        GameObject recDot = CreatePanel(camHudPanel.transform, "RecDot", new Color(1f, 0.15f, 0.15f, 1f));
        RectTransform dotRt = recDot.GetComponent<RectTransform>();
        dotRt.anchorMin = dotRt.anchorMax = new Vector2(0f, 1f);
        dotRt.anchoredPosition = new Vector2(70f, -60f);
        dotRt.sizeDelta = new Vector2(18f, 18f);

        TMP_Text recTimeText = CreateTMP(camHudPanel.transform, "RecordingTimeText", "REC 00:00:00", new Vector2(210f, -60f), new Vector2(260f, 36f), 24f, Color.white, AnchorPreset.TopLeft);
        TMP_Text nvStateText = CreateTMP(camHudPanel.transform, "NightVisionStateText", "VISIÓN NOCTURNA: OFF [F]", new Vector2(0f, -60f), new Vector2(420f, 36f), 24f, Color.white, AnchorPreset.TopCenter);

        GameObject batBg = CreatePanel(camHudPanel.transform, "BatteryBackground", new Color(0f, 0f, 0f, 0.6f));
        RectTransform batBgRt = batBg.GetComponent<RectTransform>();
        batBgRt.anchorMin = batBgRt.anchorMax = new Vector2(1f, 1f);
        batBgRt.anchoredPosition = new Vector2(-160f, -65f);
        batBgRt.sizeDelta = new Vector2(220f, 34f);

        GameObject batFillObj = CreatePanel(batBg.transform, "BatteryFill", new Color(0.35f, 1f, 0.45f, 0.9f));
        Image batFillImg = batFillObj.GetComponent<Image>();
        batFillImg.type = Image.Type.Filled;
        batFillImg.fillMethod = Image.FillMethod.Horizontal;
        TMP_Text batPctText = CreateTMP(camHudPanel.transform, "BatteryPercentText", "BATERÍA: 100%", new Vector2(-160f, -105f), new Vector2(240f, 30f), 22f, Color.white, AnchorPreset.TopRight);

        TMP_Text interactText = CreateTMP(canvasObj.transform, "InteractionText", "[E] Interactuar", new Vector2(0f, -65f), new Vector2(650f, 44f), 26f, new Color(0.95f, 0.95f, 0.85f));
        TMP_Text msgText = CreateTMP(canvasObj.transform, "MessageText", "", new Vector2(0f, 130f), new Vector2(1050f, 55f), 24f, new Color(0.85f, 0.95f, 0.85f), AnchorPreset.BottomCenter);

        // Panel de Documentos
        GameObject docPanel = CreatePanel(canvasObj.transform, "DocumentPanel", new Color(0.02f, 0.02f, 0.02f, 0.88f));
        GameObject docCard = CreatePanel(docPanel.transform, "DocumentCard", new Color(0.14f, 0.13f, 0.10f, 0.98f));
        RectTransform dcRt = docCard.GetComponent<RectTransform>();
        dcRt.anchorMin = dcRt.anchorMax = new Vector2(0.5f, 0.5f);
        dcRt.sizeDelta = new Vector2(780f, 520f);
        TMP_Text docTitle = CreateTMP(docCard.transform, "DocTitle", "Documento", new Vector2(0f, 195f), new Vector2(700f, 50f), 30f, new Color(0.92f, 0.88f, 0.72f));
        TMP_Text docBody = CreateTMP(docCard.transform, "DocBody", "", new Vector2(0f, 10f), new Vector2(680f, 320f), 23f, new Color(0.90f, 0.88f, 0.80f));
        CreateButton(docCard.transform, "BtnCloseDoc", "Cerrar [E / Esc]", new Vector2(0f, -205f), new Vector2(240f, 48f), uiManager.CloseDocumentPanel);

        // Panel del Teclado Numérico
        GameObject kpPanel = CreatePanel(canvasObj.transform, "KeypadPanel", new Color(0f, 0f, 0f, 0.82f));
        GameObject kpCard = CreatePanel(kpPanel.transform, "KeypadCard", new Color(0.10f, 0.11f, 0.12f, 0.98f));
        RectTransform kpRt = kpCard.GetComponent<RectTransform>();
        kpRt.anchorMin = kpRt.anchorMax = new Vector2(0.5f, 0.5f);
        kpRt.sizeDelta = new Vector2(380f, 520f);
        TMP_Text kpDisplay = CreateTMP(kpCard.transform, "KeypadDisplay", "_ _ _ _", new Vector2(0f, 195f), new Vector2(300f, 65f), 40f, new Color(0.7f, 1f, 0.75f));

        for (int i = 1; i <= 9; i++)
        {
            int row = (i - 1) / 3;
            int col = (i - 1) % 3;
            string digit = i.ToString();
            CreateStringButton(kpCard.transform, $"Btn_{digit}", digit, new Vector2((col - 1) * 95f, 95f - row * 80f), new Vector2(80f, 65f), uiManager.OnKeypadDigitButton, digit);
        }
        CreateButton(kpCard.transform, "Btn_CLR", "CLR", new Vector2(-95f, -145f), new Vector2(80f, 65f), uiManager.OnKeypadClearButton);
        CreateStringButton(kpCard.transform, "Btn_0", "0", new Vector2(0f, -145f), new Vector2(80f, 65f), uiManager.OnKeypadDigitButton, "0");
        CreateButton(kpCard.transform, "Btn_OK", "OK", new Vector2(95f, -145f), new Vector2(80f, 65f), uiManager.OnKeypadSubmitButton);
        CreateButton(kpCard.transform, "Btn_Close", "Cerrar [Esc]", new Vector2(0f, -220f), new Vector2(220f, 44f), uiManager.OnKeypadCloseButton);

        // Panel de Pausa
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0f, 0f, 0f, 0.85f));
        PauseMenuController pauseCtrl = pausePanel.AddComponent<PauseMenuController>();
        CreateTMP(pausePanel.transform, "PauseTitle", "JUEGO EN PAUSA", new Vector2(0f, 170f), new Vector2(500f, 65f), 44f, Color.white);
        CreateButton(pausePanel.transform, "BtnResume", "REANUDAR", new Vector2(0f, 60f), new Vector2(300f, 54f), pauseCtrl.ResumeGame);
        CreateButton(pausePanel.transform, "BtnRestart", "REINICIAR NIVEL", new Vector2(0f, -10f), new Vector2(300f, 54f), pauseCtrl.RestartLevel);
        CreateButton(pausePanel.transform, "BtnMainMenu", "MENÚ PRINCIPAL", new Vector2(0f, -80f), new Vector2(300f, 54f), pauseCtrl.ReturnToMainMenu);
        CreateTMP(pausePanel.transform, "PauseVolLabel", "VOLUMEN", new Vector2(0f, -155f), new Vector2(280f, 32f), 20f, Color.white);
        Slider pauseSlider = CreateSlider(pausePanel.transform, "PauseVolumeSlider", new Vector2(0f, -190f), new Vector2(300f, 28f), pauseCtrl.OnVolumeChanged);

        SerializedObject pauseSo = new SerializedObject(pauseCtrl);
        pauseSo.FindProperty("volumeSlider").objectReferenceValue = pauseSlider;
        pauseSo.FindProperty("buttonClickClip").objectReferenceValue = clickClip;
        pauseSo.ApplyModifiedPropertiesWithoutUndo();

        // GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // Panel Final
        GameObject endingPanel = CreatePanel(canvasObj.transform, "EndingPanel", new Color(0f, 0f, 0f, 1f));
        CreateTMP(endingPanel.transform, "EndingTitle", "Fin del capítulo 1 – El Hospital", new Vector2(0f, 110f), new Vector2(1100f, 80f), 48f, new Color(0.85f, 0.92f, 0.82f));
        CreateTMP(endingPanel.transform, "EndingSubtitle", "Victoria, espera a que continuemos la historia...", new Vector2(0f, 40f), new Vector2(1000f, 55f), 28f, new Color(0.68f, 0.80f, 0.68f));
        CreateButton(endingPanel.transform, "BtnEndRestart", "REINICIAR NIVEL", new Vector2(0f, -55f), new Vector2(300f, 56f), gm.RestartLevel);
        CreateButton(endingPanel.transform, "BtnEndMenu", "MENÚ PRINCIPAL", new Vector2(0f, -130f), new Vector2(300f, 56f), gm.LoadMainMenu);

        SerializedObject uiSo = new SerializedObject(uiManager);
        uiSo.FindProperty("interactionText").objectReferenceValue = interactText;
        uiSo.FindProperty("messageText").objectReferenceValue = msgText;
        uiSo.FindProperty("staminaFillImage").objectReferenceValue = stamFillImg;
        uiSo.FindProperty("cameraHudPanel").objectReferenceValue = camHudPanel;
        uiSo.FindProperty("nightVisionOverlay").objectReferenceValue = nvOverlay;
        uiSo.FindProperty("noiseRawImage").objectReferenceValue = noiseRaw;
        uiSo.FindProperty("recIndicatorDot").objectReferenceValue = recDot;
        uiSo.FindProperty("recordingTimeText").objectReferenceValue = recTimeText;
        uiSo.FindProperty("batteryFillImage").objectReferenceValue = batFillImg;
        uiSo.FindProperty("batteryPercentText").objectReferenceValue = batPctText;
        uiSo.FindProperty("nightVisionStateText").objectReferenceValue = nvStateText;
        uiSo.FindProperty("documentPanel").objectReferenceValue = docPanel;
        uiSo.FindProperty("documentTitleText").objectReferenceValue = docTitle;
        uiSo.FindProperty("documentBodyText").objectReferenceValue = docBody;
        uiSo.FindProperty("keypadPanel").objectReferenceValue = kpPanel;
        uiSo.FindProperty("keypadDisplayText").objectReferenceValue = kpDisplay;
        uiSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        uiSo.FindProperty("endingPanel").objectReferenceValue = endingPanel;
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject gmSo = new SerializedObject(gm);
        gmSo.FindProperty("playerMovement").objectReferenceValue = playerRefs.movement;
        gmSo.FindProperty("playerInteraction").objectReferenceValue = playerRefs.interaction;
        gmSo.FindProperty("videoCamera").objectReferenceValue = playerRefs.videoCamera;
        gmSo.FindProperty("sebastianSilhouette").objectReferenceValue = sebastianSilhouette;
        gmSo.FindProperty("endingCorridorLight").objectReferenceValue = endCorridorLight;
        gmSo.FindProperty("childSoundClip").objectReferenceValue = childSobClip;
        gmSo.FindProperty("stingerClip").objectReferenceValue = stingerClip;
        gmSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateAudioManagerObject(AudioClip musicClip, AudioClip ambientClip)
    {
        GameObject audioObj = new GameObject("AudioManager");
        AudioSource musicSource = audioObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.45f;

        AudioSource ambientSource = audioObj.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.spatialBlend = 0f;
        ambientSource.volume = 0.4f;

        AudioSource sfxSource = audioObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 0.9f;

        AudioManager am = audioObj.AddComponent<AudioManager>();
        SerializedObject so = new SerializedObject(am);
        so.FindProperty("musicSource").objectReferenceValue = musicSource;
        so.FindProperty("ambientSource").objectReferenceValue = ambientSource;
        so.FindProperty("sfxSource").objectReferenceValue = sfxSource;
        so.FindProperty("defaultMusicClip").objectReferenceValue = musicClip;
        so.FindProperty("defaultAmbientClip").objectReferenceValue = ambientClip;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildHospitalArchitecture(Transform parent, Material wallMat, Material floorMat, Material ceilingMat)
    {
        const float h = 3.4f;
        const float t = 0.25f;

        CreateBox(parent, "Piso_General", new Vector3(0f, -0.1f, 12f), new Vector3(20f, 0.2f, 38f), floorMat);
        CreateBox(parent, "Techo_General", new Vector3(0f, h + 0.1f, 12f), new Vector3(20f, 0.2f, 38f), ceilingMat);

        // Recepción
        CreateBox(parent, "Rec_Sur", new Vector3(0f, h * 0.5f, -6f), new Vector3(12f, h, t), wallMat);
        CreateBox(parent, "Rec_Oeste", new Vector3(-6f, h * 0.5f, -1f), new Vector3(t, h, 10f), wallMat);
        CreateBox(parent, "Rec_Este", new Vector3(6f, h * 0.5f, -1f), new Vector3(t, h, 10f), wallMat);
        CreateBox(parent, "Rec_Norte_Izq", new Vector3(-3.5f, h * 0.5f, 4f), new Vector3(5f, h, t), wallMat);
        CreateBox(parent, "Rec_Norte_Der", new Vector3(3.5f, h * 0.5f, 4f), new Vector3(5f, h, t), wallMat);
        CreateBox(parent, "Rec_Norte_Dintel", new Vector3(0f, 3.05f, 4f), new Vector3(2f, 0.7f, t), wallMat);

        // Pasillo Principal
        CreateBox(parent, "Pasillo_Oeste_1", new Vector3(-2f, h * 0.5f, 6f), new Vector3(t, h, 4f), wallMat);
        CreateBox(parent, "Pasillo_Oeste_Dintel", new Vector3(-2f, 3.05f, 9f), new Vector3(t, 0.7f, 2f), wallMat);
        CreateBox(parent, "Pasillo_Oeste_2", new Vector3(-2f, h * 0.5f, 15f), new Vector3(t, h, 10f), wallMat);

        CreateBox(parent, "Pasillo_Este_1", new Vector3(2f, h * 0.5f, 8.5f), new Vector3(t, h, 9f), wallMat);
        CreateBox(parent, "Pasillo_Este_Dintel", new Vector3(2f, 3.05f, 14f), new Vector3(t, 0.7f, 2f), wallMat);
        CreateBox(parent, "Pasillo_Este_2", new Vector3(2f, h * 0.5f, 17.5f), new Vector3(t, h, 5f), wallMat);

        // Sala de Enfermería
        CreateBox(parent, "Enf_Sur", new Vector3(-5.5f, h * 0.5f, 5f), new Vector3(7f, h, t), wallMat);
        CreateBox(parent, "Enf_Norte", new Vector3(-5.5f, h * 0.5f, 14f), new Vector3(7f, h, t), wallMat);
        CreateBox(parent, "Enf_Oeste", new Vector3(-9f, h * 0.5f, 9.5f), new Vector3(t, h, 9f), wallMat);

        // Sala de Espera
        CreateBox(parent, "Esp_Sur", new Vector3(5f, h * 0.5f, 11f), new Vector3(6f, h, t), wallMat);
        CreateBox(parent, "Esp_Norte", new Vector3(5f, h * 0.5f, 17f), new Vector3(6f, h, t), wallMat);
        CreateBox(parent, "Esp_Este", new Vector3(8f, h * 0.5f, 14f), new Vector3(t, h, 6f), wallMat);

        // Muro Puerta de Administración
        CreateBox(parent, "Admin_Muro_Izq", new Vector3(-2.75f, h * 0.5f, 20f), new Vector3(3.5f, h, t), wallMat);
        CreateBox(parent, "Admin_Muro_Der", new Vector3(2.75f, h * 0.5f, 20f), new Vector3(3.5f, h, t), wallMat);
        CreateBox(parent, "Admin_Muro_Dintel", new Vector3(0f, 3.05f, 20f), new Vector3(2f, 0.7f, t), wallMat);

        // Sector Administrativo + Pasillo Final
        CreateBox(parent, "Admin_Oeste", new Vector3(-4.5f, h * 0.5f, 25f), new Vector3(t, h, 10f), wallMat);
        CreateBox(parent, "Admin_Este", new Vector3(4.5f, h * 0.5f, 25f), new Vector3(t, h, 10f), wallMat);
        CreateBox(parent, "Admin_Norte", new Vector3(0f, h * 0.5f, 30f), new Vector3(9f, h, t), wallMat);
    }

    private static void CreateBox(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
    }

    private static GameObject PlacePrefab(GameObject prefab, Transform parent, Vector3 pos, float rotY)
    {
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.transform.position = pos;
        inst.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        return inst;
    }

    private static void BindLampPool(GameObject lampObj, SimpleObjectPool pool)
    {
        FlickeringLight fl = lampObj.GetComponent<FlickeringLight>();
        if (fl == null) return;
        SerializedObject so = new SerializedObject(fl);
        so.FindProperty("sparkPool").objectReferenceValue = pool;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureDocument(DocumentItem doc, string prompt, string title, string body, bool isFinal)
    {
        SerializedObject so = new SerializedObject(doc);
        so.FindProperty("interactionText").stringValue = prompt;
        so.FindProperty("documentTitle").stringValue = title;
        so.FindProperty("documentBody").stringValue = body;
        so.FindProperty("isFinalExpediente").boolValue = isFinal;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateWallSign(Transform parent, string text, Vector3 pos, Vector3 euler, float fontSize)
    {
        GameObject go = new GameObject("Letrero_" + text.Split('\n')[0]);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(euler);
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.68f, 0.74f, 0.65f);
        tmp.rectTransform.sizeDelta = new Vector2(6f, 1.4f);
    }

    private static HiddenCode CreateHiddenWallCode(Transform parent, string name, string content, Vector3 pos, Vector3 euler, Material glowMat)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(euler);

        GameObject textObj = new GameObject("HiddenText3D");
        textObj.transform.SetParent(root.transform, false);
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = content;
        tmp.fontSize = 6.2f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.40f, 1.0f, 0.50f, 0.96f);
        tmp.rectTransform.sizeDelta = new Vector2(4.6f, 2.2f);

        GameObject underline = GameObject.CreatePrimitive(PrimitiveType.Cube);
        underline.name = "MarcaPared";
        underline.transform.SetParent(root.transform, false);
        underline.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        underline.transform.localScale = new Vector3(2.2f, 0.04f, 0.01f);
        underline.GetComponent<Renderer>().sharedMaterial = glowMat;
        Object.DestroyImmediate(underline.GetComponent<Collider>());

        HiddenCode hc = root.AddComponent<HiddenCode>();
        SerializedObject so = new SerializedObject(hc);
        SerializedProperty arr = so.FindProperty("visualElements");
        arr.arraySize = 2;
        arr.GetArrayElementAtIndex(0).objectReferenceValue = textObj;
        arr.GetArrayElementAtIndex(1).objectReferenceValue = underline;
        so.ApplyModifiedPropertiesWithoutUndo();

        return hc;
    }

    private static void CreateEventSystem()
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private enum AnchorPreset { Center, TopLeft, TopCenter, TopRight, BottomCenter, BottomLeft }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        StretchFull(rt);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = color.a > 0.01f;
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TMP_Text CreateTMP(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color, AnchorPreset anchor = AnchorPreset.Center)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        Vector2 a = new Vector2(0.5f, 0.5f);
        if (anchor == AnchorPreset.TopLeft) a = new Vector2(0f, 1f);
        else if (anchor == AnchorPreset.TopCenter) a = new Vector2(0.5f, 1f);
        else if (anchor == AnchorPreset.TopRight) a = new Vector2(1f, 1f);
        else if (anchor == AnchorPreset.BottomCenter) a = new Vector2(0.5f, 0f);
        else if (anchor == AnchorPreset.BottomLeft) a = new Vector2(0f, 0f);

        rt.anchorMin = rt.anchorMax = a;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityAction onClick)
    {
        GameObject go = CreatePanel(parent, name, new Color(0.16f, 0.23f, 0.18f, 0.96f));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Button btn = go.AddComponent<Button>();
        UnityEventTools.AddVoidPersistentListener(btn.onClick, onClick);
        CreateTMP(go.transform, "Label", label, Vector2.zero, size, 23f, Color.white);
        return btn;
    }

    private static Button CreateStringButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityAction<string> onClick, string argument)
    {
        GameObject go = CreatePanel(parent, name, new Color(0.16f, 0.23f, 0.18f, 0.96f));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Button btn = go.AddComponent<Button>();
        UnityEventTools.AddStringPersistentListener(btn.onClick, onClick, argument);
        CreateTMP(go.transform, "Label", label, Vector2.zero, size, 23f, Color.white);
        return btn;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size, UnityAction<float> onValueChanged)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        GameObject bg = CreatePanel(root.transform, "Background", new Color(0.12f, 0.14f, 0.12f, 1f));
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(root.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        StretchFull(faRt);

        GameObject fill = CreatePanel(fillArea.transform, "Fill", new Color(0.35f, 0.85f, 0.45f, 1f));
        RectTransform fillRt = fill.GetComponent<RectTransform>();

        Slider slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.85f;
        slider.fillRect = fillRt;
        slider.targetGraphic = bg.GetComponent<Image>();
        UnityEventTools.AddPersistentListener(slider.onValueChanged, onValueChanged);

        return slider;
    }
}
