using System;
using System.Collections.Generic;
using UnityEngine;
namespace Ecos
{
    public static class LevelBuilder
    {
        static Transform root, architecture, props; static Material wall, floor, ceiling, trim;
        static readonly List<Light> hospitalLights = new List<Light>();
        static int bedHideIndex;
        static Material Solid(string name, Color color)
        {
            Material m = new Material(Resources.Load<Shader>("Shaders/Architecture")) { name = name, color = color }; ModelLibrary.BindTextures(m); m.SetFloat("_Tiled", name == "Baldosa gastada" ? 1 : 0); return m;
        }
        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, bool collider = true)
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube); o.name = name; o.transform.SetParent(parent, false); o.transform.localPosition = pos; o.transform.localScale = size; o.GetComponent<Renderer>().sharedMaterial = mat; o.layer = 8;
            if (!collider) { Collider c = o.GetComponent<Collider>(); c.enabled = false; if (Application.isPlaying) UnityEngine.Object.Destroy(c); else UnityEngine.Object.DestroyImmediate(c); }
            return o;
        }
        static GameObject Model(string model, Vector3 p, float yaw = 0, float scale = 1, bool collider = true)
        {
            GameObject o = ModelLibrary.Create(model, props, p, Vector3.one * scale); o.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            if (Mathf.Approximately(p.y, 0)) ModelLibrary.SetBaseHeight(o, 0);
            if (collider) ModelLibrary.AddStaticCollider(o);
            if (model == "Bed")
            {
                Interactable hide = o.AddComponent<Interactable>();
                hide.id = "bed_hide_" + (bedHideIndex++);
                hide.label = "Esconderse bajo la cama";
                hide.kind = "bed_hide";
            }
            return o;
        }
        static Interactable Item(string model, string id, string label, string kind, string entry, Vector3 p, int number = 0)
        {
            GameObject o = Model(model, p, 0, 1, false); o.layer = 8; BoxCollider hit = o.AddComponent<BoxCollider>(); Bounds bounds = o.GetComponent<MeshFilter>().sharedMesh.bounds; hit.center = bounds.center; hit.size = bounds.size; Interactable i = o.AddComponent<Interactable>(); i.id = id; i.label = label; i.kind = kind; i.entry = entry; i.number = number;
            if (model == "Battery" || model == "Key") { Box("Soporte", props, new Vector3(p.x, p.y * .5f, p.z), new Vector3(.4f, p.y, .4f), trim); BoxCollider c = o.GetComponent<BoxCollider>(); c.size += new Vector3(.15f, .1f, .15f); }
            return i;
        }
        static void Document(string entry, Vector3 p) { Box("Atril", props, new Vector3(p.x, (p.y - .29f) * .5f, p.z + .08f), new Vector3(.28f, p.y - .29f, .24f), trim); Item("Document", entry, StoryData.Get(entry).title, "document", entry, p); }
        static void Clue(string model, string id, string reward, int memory, Vector3 p)
        {
            Item(model, id, id == "frame" ? "Revisar detrás del cuadro" : id == "xray" ? "Recoger registros de Rayos X" : "Revisar cajón de llaves", "clue", reward, p, memory);
        }
        static void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, layer);
        }
        static void Gate(int zone, string key)
        {
            float z = zone * 14 - 1; GameObject gate = new GameObject("Acceso / " + StoryData.Zones[zone]); gate.transform.SetParent(root, false); gate.transform.localPosition = new Vector3(0, 0, z);
            GameObject leaf = new GameObject("Hoja_Puerta_Zona_" + zone); leaf.transform.SetParent(gate.transform, false);
            ModelLibrary.Create("Door", leaf.transform, Vector3.zero, new Vector3(2.4f, 1, 1));
            Color gateBeacon = new Color(.65f, .75f, .68f);
            if (zone == 2)
            {
                Material pedMat = Solid("Puerta_Pediatrica", new Color(.32f, .48f, .54f));
                Box("Marco_Pediatrico_Izq", gate.transform, new Vector3(-2.85f, 1.6f, 0), new Vector3(.28f, 3.2f, .26f), pedMat, false);
                Box("Marco_Pediatrico_Der", gate.transform, new Vector3(2.85f, 1.6f, 0), new Vector3(.28f, 3.2f, .26f), pedMat, false);
                ModelLibrary.Create("Vent", leaf.transform, new Vector3(0, 2.75f, -.08f), new Vector3(1.4f, .45f, 1f));
                gateBeacon = new Color(.42f, .78f, .92f);
            }
            else if (zone == 3)
            {
                Material traumaMat = Solid("Puerta_Urgencias", new Color(.52f, .16f, .14f));
                Box("Parachoques_Camilla_Inf", leaf.transform, new Vector3(0, .85f, -.09f), new Vector3(5.6f, .28f, .06f), traumaMat, false);
                Box("Parachoques_Camilla_Sup", leaf.transform, new Vector3(0, 1.75f, -.09f), new Vector3(5.6f, .18f, .06f), traumaMat, false);
                gateBeacon = new Color(.95f, .24f, .18f);
            }
            else if (zone == 5)
            {
                Material rustMat = Solid("Reja_Otherworld", new Color(.34f, .19f, .12f));
                for (int b = -4; b <= 4; b++)
                    Box("Barrote_Oxidado_" + b, leaf.transform, new Vector3(b * .62f, 1.6f, -.11f), new Vector3(.055f, 3.1f, .055f), rustMat, false);
                ModelLibrary.Create("Valve", leaf.transform, new Vector3(1.1f, 1.45f, -.14f), Vector3.one * 1.15f);
                gateBeacon = new Color(.95f, .58f, .18f);
            }
            else if (zone == 6)
            {
                Material bioMat = Solid("Compuerta_Bioseguridad", new Color(.24f, .38f, .32f));
                Box("Refuerzo_Blindado", leaf.transform, new Vector3(0, 1.5f, -.09f), new Vector3(5.4f, .45f, .07f), bioMat, false);
                ModelLibrary.Create("Valve", leaf.transform, new Vector3(0, 1.5f, -.15f), Vector3.one * 1.35f);
                gateBeacon = new Color(.45f, .92f, .42f);
            }
            else if (zone == 7)
            {
                Material morgueMat = Solid("Puerta_Camara_Morgue", new Color(.18f, .22f, .26f));
                for (int row = 0; row < 3; row++)
                    Box("Travesano_Criogenico_" + row, leaf.transform, new Vector3(0, .7f + row * .85f, -.10f), new Vector3(5.5f, .16f, .08f), morgueMat, false);
                gateBeacon = new Color(.48f, .68f, .95f);
            }
            AddLight(gate.transform, gateBeacon, .38f, 5.5f, new Vector3(0, 2.95f, -.45f), false);
            gate.layer = 8; BoxCollider barrier = gate.AddComponent<BoxCollider>(); barrier.center = new Vector3(0, 1.6f, 0); barrier.size = new Vector3(6, 3.2f, .22f);
            DoorGate d = gate.AddComponent<DoorGate>(); d.zone = zone; d.key = key; d.leaf = leaf.transform; d.barrier = barrier;
            Interactable i = gate.AddComponent<Interactable>(); i.id = "door_" + zone; i.label = "Abrir acceso: " + StoryData.Zones[zone]; i.kind = "door";
        }
        public static GameSession Build()
        {
            ModelLibrary.ClearCache(); hospitalLights.Clear(); bedHideIndex = 0; root = new GameObject("ECOS DE CHUQUICAMATA / Nivel completo").transform;
            architecture = new GameObject("Arquitectura").transform; architecture.SetParent(root, false); props = new GameObject("Modelos y objetos").transform; props.SetParent(root, false);
            HospitalMap map = root.gameObject.AddComponent<HospitalMap>(); map.Init();
            wall = Solid("Pintura envejecida", new Color(.22f, .28f, .24f)); floor = Solid("Baldosa gastada", new Color(.20f, .19f, .17f)); ceiling = Solid("Techo", new Color(.08f, .10f, .09f)); trim = Solid("Guardapolvo", new Color(.12f, .15f, .13f));
            Color[] zoneColors = { new Color(.30f, .25f, .18f), new Color(.20f, .27f, .23f), new Color(.22f, .31f, .33f), new Color(.29f, .20f, .18f), new Color(.18f, .25f, .22f), new Color(.22f, .16f, .13f), new Color(.18f, .26f, .24f), new Color(.22f, .26f, .28f) };
            Material[] zoneWalls = new Material[8]; for (int zi = 0; zi < 8; zi++) zoneWalls[zi] = Solid("Zona_" + zi, zoneColors[zi]);
            foreach (Vector2Int c in map.Cells)
            {
                wall = zoneWalls[Mathf.Clamp(c.y / 7, 0, 7)]; Vector3 p = map.Position(c); Box("Suelo", architecture, p + Vector3.down * .1f, new Vector3(2, .2f, 2), floor);
                Box("Techo", architecture, p + Vector3.up * 3.3f, new Vector3(2, .2f, 2), ceiling);
                foreach (Vector2Int d in HospitalMap.Directions) if (!map.Cells.Contains(c + d))
                {
                    Vector3 off = new Vector3(d.x, 0, d.y); Box("Muro", architecture, p + off + Vector3.up * 1.6f, new Vector3(d.x == 0 ? 2.12f : .14f, 3.2f, d.x == 0 ? .14f : 2.12f), wall);
                    Box("Franja", architecture, p + off * .91f + Vector3.up * 1.1f, new Vector3(d.x == 0 ? 2 : .03f, .18f, d.x == 0 ? .03f : 2), trim, false);
                }
            }
            for (int zone = 0; zone < 8; zone++)
            {
                float z = zone * 14;
                foreach (int side in new[] { -1, 1 })
                {
                    if (!(side == -1 && (zone == 1 || zone == 2))) Model(zone == 2 ? "Crib" : zone >= 5 ? "Cabinet" : "Bed", new Vector3(side * 9.8f, 0, z + 6), side == -1 ? 90 : -90, 1, true);
                    Model("Chair", new Vector3(side * 8, 0, z + 4), 180, 1, true);
                    GameObject lamp = Model("Lamp", new Vector3(side * 8, 3.155f, z + 6)); AddLight(lamp.transform, new Color(.68f, .78f, .66f), .30f, 5.8f, default, false);
                    Model("Rubble", new Vector3(side * 1.9f, 0, z + 10), 0, .6f);
                }
                GameObject light = Model("Lamp", new Vector3(0, 3.155f, z + 5)); AddLight(light.transform, zone >= 5 ? new Color(.82f, .48f, .26f) : zone == 3 ? new Color(.85f, .36f, .32f) : new Color(.58f, .74f, .66f), .34f, 7.0f, default, true);
                Item("Battery", "battery_" + zone, "Recoger pila para la cámara", "battery", "", new Vector3(6.6f, 1, z + 5));
                if (zone >= 5) for (int j = 0; j < 6; j++) Model("Pipe", new Vector3(2.7f, 2.7f, z + j * 2), 0, 1);
            }
            Model("Rubble", new Vector3(-6, 0, 5), 0, 1.5f);
            Document("welcome", new Vector3(-1, 1.3f, 3)); Model("BedsideTable", new Vector3(-.5f, 0, 3)); Model("Recorder", new Vector3(-.5f, .885f, 3));
            Model("Desk", new Vector3(-8, 0, 20), 0, 1, true); Model("Phone", new Vector3(-8.7f, 1.06f, 20));
            Document("reception", new Vector3(-7, 1.35f, 19)); Clue("Frame", "frame", "infant_key", 1, new Vector3(-8, 1.8f, 22.89f)); Gate(2, "infant_key");
            Document("drawing", new Vector3(-7, 1.3f, 33));
            Color[] toyColors = { new Color(.85f, .22f, .18f), new Color(.88f, .72f, .20f), new Color(.20f, .52f, .88f) };
            for (int i = 0; i < 3; i++)
            {
                Interactable toyItem = Item("Teddy", "toy_" + i, new[] { "Juguete ROJO", "Juguete AMARILLO", "Juguete AZUL" }[i], "toy", "", new Vector3(-9 + i * 1.1f, 1.07f, 35), i);
                AddLight(toyItem.transform, toyColors[i], .42f, 3.2f, default, false);
            }
            Model("Desk", new Vector3(-8, .0f, 35), 0, 1, true); Gate(3, "urgency_key");
            Document("record19", new Vector3(-7, 1.3f, 47)); Document("record85", new Vector3(7, 1.3f, 47));
            Clue("XRay", "xray", "evidence", 3, new Vector3(8, 0, 48)); Model("Wheelchair", new Vector3(-1.9f, 0, 51), 30);
            Item("Cabinet", "safe", "Archivador del expediente", "safe", "", new Vector3(-8, 0, 63)); Model("XRay", new Vector3(8, 0, 62)); Gate(5, "basement_key");
            Document("generator_manual", new Vector3(-7, 1.3f, 75)); Item("Battery", "fuse_pickup", "Recoger fusible", "key", "fuse", new Vector3(7, 1.2f, 75));
            Item("Generator", "generator", "Panel del generador", "generator", "", new Vector3(-8, 0, 77));
            GameObject valveObj = Model("Valve", new Vector3(10.68f, 1.4f, 77), 90);
            Interactable valveItem = valveObj.AddComponent<Interactable>(); valveItem.id = "steam_valve"; valveItem.label = "Purgar presión de vapor"; valveItem.kind = "valve";
            Gate(6, "pathology_key");
            Document("morgue_record", new Vector3(-7, 1.4f, 89)); Clue("Cabinet", "seal", "morgue_key", 4, new Vector3(-8, 0, 91)); Gate(7, "morgue_key");
            Item("ChildBody", "child", "Identificar camilla A1 (Sebastián)", "child", "", new Vector3(-8, 0, 104));
            Item("MotherBody", "mother", "Identificar / trasladar camilla C2 (Madre)", "mother", "", new Vector3(8, 0, 104));
            Model("BedsideTable", new Vector3(-5.6f, 0, 102.6f), 0, 1, true);
            Item("Recorder", "ending", "Cerrar la historia", "ending", "", new Vector3(-5.6f, .885f, 102.6f));
            HospitalDressing.Build(root);
            AddBloodGrimeAndLightingDetails();

            // Jugador y cámara limpia (oculta Layer 9 hasta que levantes la videocámara)
            GameObject playerObj = new GameObject("Tomas / Primera persona"); playerObj.transform.SetParent(root, false); playerObj.transform.position = new Vector3(0, .1f, 1);
            CharacterController cc = playerObj.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = .28f; cc.center = Vector3.up * .9f; cc.stepOffset = .25f;
            PlayerRig player = playerObj.AddComponent<PlayerRig>();
            GameObject camera = new GameObject("Camara del jugador"); camera.transform.SetParent(playerObj.transform, false); camera.transform.localPosition = Vector3.up * 1.62f;
            player.view = camera.AddComponent<Camera>();
            player.view.nearClipPlane = .05f;
            player.view.farClipPlane = 90f;
            player.view.fieldOfView = 72f;
            player.view.allowMSAA = true;
            player.view.clearFlags = CameraClearFlags.SolidColor;
            player.view.backgroundColor = new Color(.01f, .012f, .015f);
            player.view.cullingMask &= ~(1 << PlayerRig.GhostLayer);
            camera.AddComponent<AudioListener>();

            GameObject ir = new GameObject("Iluminador de vision nocturna"); ir.transform.SetParent(camera.transform, false);
            player.infrared = ir.AddComponent<Light>(); player.infrared.type = LightType.Spot; player.infrared.spotAngle = 100f; player.infrared.range = 32f; player.infrared.intensity = 5.2f;
            player.infrared.color = new Color(.75f, .96f, .82f);
            player.infrared.shadows = LightShadows.Soft; player.infrared.shadowBias = .02f; player.infrared.shadowNormalBias = .15f; player.infrared.enabled = false;

            GameObject eyeAdaptation = new GameObject("Luz de penumbra cercana"); eyeAdaptation.transform.SetParent(camera.transform, false); eyeAdaptation.transform.localPosition = new Vector3(0, -.08f, .06f);
            player.eyeLight = eyeAdaptation.AddComponent<Light>(); player.eyeLight.type = LightType.Point; player.eyeLight.range = 5.2f; player.eyeLight.intensity = .24f; player.eyeLight.color = new Color(.68f, .74f, .82f);
            player.eyeLight.shadows = LightShadows.None;

            GameObject cameraMesh = ModelLibrary.Create("Camcorder", camera.transform, new Vector3(.22f, -.25f, .45f), Vector3.one * .6f); cameraMesh.transform.localRotation = Quaternion.Euler(0, 180, 0); player.cameraModel = cameraMesh.transform; cameraMesh.SetActive(false);
            cameraMesh.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Sebastián en Layer 9 (invisible sin cámara; aparece al apuntar con Clic Derecho)
            GameObject actor = new GameObject("Sebastian"); actor.transform.SetParent(root, false); SebastianAI enemy = actor.AddComponent<SebastianAI>();
            foreach (Light l in hospitalLights) enemy.RegisterLamp(l);
            Material ghostMaterial = new Material(Resources.Load<Shader>("Shaders/Ghost")) { name = "Sebastian_Presencia" }; ModelLibrary.BindTextures(ghostMaterial);
            enemy.visual = SebastianRig.Create(actor.transform, ghostMaterial).transform;
            SetLayerRecursive(enemy.visual.gameObject, PlayerRig.GhostLayer);
            enemy.visual.gameObject.SetActive(false);

            GameObject systems = new GameObject("Sistemas de juego"); systems.transform.SetParent(root, false); AudioDirector audio = systems.AddComponent<AudioDirector>(); GameSession session = systems.AddComponent<GameSession>(); session.Player = player; session.Map = map; session.Enemy = enemy; session.Audio = audio; systems.AddComponent<GameUI>();
            GameObject sun = new GameObject("Luz de luna"); sun.transform.SetParent(root, false); sun.transform.rotation = Quaternion.Euler(55, -30, 0); Light moon = sun.AddComponent<Light>(); moon.type = LightType.Directional; moon.intensity = .025f; moon.shadows = LightShadows.None; moon.color = new Color(.42f, .52f, .66f);
            if (Application.isPlaying) StaticBatchingUtility.Combine(architecture.gameObject);
            return session;
        }
        static void AddSpotLight(Transform parent, Color color, float intensity, float range, float angle, Vector3 pos, Vector3 rot)
        {
            GameObject o = new GameObject("Foco_Dramatico"); o.transform.SetParent(parent, false);
            o.transform.localPosition = pos; o.transform.localRotation = Quaternion.Euler(rot);
            Light l = o.AddComponent<Light>(); l.type = LightType.Spot; l.color = color; l.intensity = intensity * .72f; l.range = range * .85f; l.spotAngle = angle;
            l.shadows = LightShadows.Soft; l.shadowStrength = .85f; l.shadowBias = .02f; l.shadowNormalBias = .15f;
            hospitalLights.Add(l);
        }
        static void AddGoreInstallation(Transform parent, string name, Vector3 pos, Material fleshMat, Material boneMat, Material chainMat)
        {
            Box(name + "_Cadena_Sup", parent, new Vector3(pos.x, 2.65f, pos.z), new Vector3(.035f, 1.1f, .035f), chainMat, false);
            Box(name + "_Gancho", parent, new Vector3(pos.x, 2.05f, pos.z), new Vector3(.18f, .04f, .04f), chainMat, false);
            Box(name + "_Torso_Visceral", parent, new Vector3(pos.x, 1.68f, pos.z), new Vector3(.36f, .68f, .26f), fleshMat, false);
            for (int r = 0; r < 4; r++)
                Box(name + "_Costilla_" + r, parent, new Vector3(pos.x, 1.52f + r * .11f, pos.z), new Vector3(.41f, .028f, .29f), boneMat, false);
            Box(name + "_Charco_Goteo", parent, new Vector3(pos.x, .004f, pos.z), new Vector3(1.65f, .006f, 1.65f), fleshMat, false);
        }
        static void AddBloodGrimeAndLightingDetails()
        {
            Transform details = new GameObject("Sangre_Gore_Suciedad_E_Iluminacion_Realista").transform;
            details.SetParent(architecture, false);
            Material bloodPoolMat = Solid("Charco de sangre coagulada", new Color(.48f, .022f, .018f)); bloodPoolMat.SetFloat("_Tiled", 1f);
            Material freshBloodMat = Solid("Sangre arterial fresca", new Color(.68f, .030f, .022f)); freshBloodMat.SetFloat("_Tiled", 1f);
            Material wallBloodMat = Solid("Salpicadura visceral", new Color(.54f, .026f, .020f));
            Material rustGrimeMat = Solid("Costra de oxido Silent Hill", new Color(.32f, .13f, .07f)); rustGrimeMat.SetFloat("_Tiled", 1f);
            Material moldStainMat = Solid("Mancha de humedad y moho", new Color(.09f, .11f, .08f));
            Material boneGoreMat = Solid("Hueso expuesto", new Color(.72f, .64f, .54f));

            for (int zone = 0; zone < 8; zone++)
            {
                float z = zone * 14f;
                Box("Mancha_Corredor_" + zone, details, new Vector3((zone % 2 == 0 ? -.55f : .48f), .003f, z + 3.2f), new Vector3(1.35f, .005f, 2.1f), zone >= 3 ? bloodPoolMat : rustGrimeMat, false);
                Box("Rastro_Arrastre_" + zone, details, new Vector3((zone % 2 == 0 ? .35f : -.42f), .0035f, z + 8.4f), new Vector3(.95f, .005f, 2.85f), zone >= 2 ? freshBloodMat : rustGrimeMat, false);
                Box("Salpicadura_Muro_Izq_" + zone, details, new Vector3(-2.92f, .95f, z + 4.5f), new Vector3(.012f, 1.45f, 1.65f), zone >= 2 ? wallBloodMat : moldStainMat, false);
                Box("Salpicadura_Muro_Der_" + zone, details, new Vector3(2.92f, 1.05f, z + 7.8f), new Vector3(.012f, 1.55f, 1.80f), zone >= 3 ? wallBloodMat : rustGrimeMat, false);
                if (zone >= 2)
                {
                    for (int claw = 0; claw < 3; claw++)
                    {
                        Box("Garra_Pared_Izq_" + zone + "_" + claw, details, new Vector3(-2.91f, 1.85f + claw * .14f, z + 6.0f + claw * .25f), new Vector3(.014f, .06f, 1.85f), freshBloodMat, false);
                        Box("Garra_Pared_Der_" + zone + "_" + claw, details, new Vector3(2.91f, 1.92f + claw * .14f, z + 9.0f + claw * .25f), new Vector3(.014f, .06f, 1.85f), freshBloodMat, false);
                    }
                }
                Box("Charco_Sala_Oeste_" + zone, details, new Vector3(-8.4f, .003f, z + 5.8f), new Vector3(2.2f, .005f, 1.85f), zone >= 3 ? freshBloodMat : bloodPoolMat, false);
                Box("Charco_Sala_Este_" + zone, details, new Vector3(8.4f, .003f, z + 6.2f), new Vector3(2.1f, .005f, 1.95f), zone >= 2 ? bloodPoolMat : rustGrimeMat, false);
                Box("Humedad_Techo_" + zone, details, new Vector3(0, 3.19f, z + 6.5f), new Vector3(2.8f, .006f, 3.2f), zone >= 4 ? wallBloodMat : moldStainMat, false);
            }
            Box("Sangre_Quirurgica_Urgencias", details, new Vector3(-6.3f, .004f, 49.4f), new Vector3(2.6f, .005f, 2.4f), freshBloodMat, false);
            Box("Sangre_Silla_Ruedas_Urgencias", details, new Vector3(-1.9f, .004f, 51.0f), new Vector3(1.5f, .005f, 1.8f), bloodPoolMat, false);
            Box("Sangre_Bahia_Trauma_Oeste", details, new Vector3(-13.5f, .004f, 43.0f), new Vector3(3.4f, .005f, 2.6f), freshBloodMat, false);
            Box("Oxido_Caldera_Subterraneo", details, new Vector3(-8.0f, .004f, 77.0f), new Vector3(3.2f, .005f, 3.0f), rustGrimeMat, false);
            Box("Sangre_Mesa_Autopsia_Patologia", details, new Vector3(6.4f, .004f, 91.5f), new Vector3(2.8f, .005f, 2.5f), freshBloodMat, false);
            Box("Sangre_Anfiteatro_Autopsia_Este", details, new Vector3(13.2f, .004f, 85.0f), new Vector3(3.2f, .005f, 2.8f), freshBloodMat, false);
            Box("Rastro_Camilla_Morgue_A1", details, new Vector3(-7.4f, .004f, 104.0f), new Vector3(2.8f, .005f, 2.4f), bloodPoolMat, false);
            Box("Rastro_Camilla_Morgue_C2", details, new Vector3(0f, .004f, 104.0f), new Vector3(14.5f, .005f, 1.35f), freshBloodMat, false);

            AddGoreInstallation(details, "Disenio_Gore_Trauma", new Vector3(-14.2f, 0, 43.2f), freshBloodMat, boneGoreMat, rustGrimeMat);
            AddGoreInstallation(details, "Disenio_Gore_Subterraneo_Oeste", new Vector3(-11.5f, 0, 71.2f), bloodPoolMat, boneGoreMat, rustGrimeMat);
            AddGoreInstallation(details, "Disenio_Gore_Subterraneo_Este", new Vector3(11.5f, 0, 80.8f), freshBloodMat, boneGoreMat, rustGrimeMat);
            AddGoreInstallation(details, "Disenio_Gore_Patologia", new Vector3(8.2f, 0, 92.2f), freshBloodMat, boneGoreMat, rustGrimeMat);
            AddGoreInstallation(details, "Disenio_Gore_Anfiteatro", new Vector3(14.8f, 0, 85.2f), freshBloodMat, boneGoreMat, rustGrimeMat);
            AddGoreInstallation(details, "Disenio_Gore_Morgue_Oeste", new Vector3(-11.5f, 0, 108.8f), bloodPoolMat, boneGoreMat, rustGrimeMat);
            AddGoreInstallation(details, "Disenio_Gore_Morgue_Este", new Vector3(11.5f, 0, 108.8f), freshBloodMat, boneGoreMat, rustGrimeMat);

            AddSpotLight(details, new Color(.75f, .92f, .88f), 1.65f, 11f, 68f, new Vector3(8f, 3.12f, 49f), new Vector3(90f, 0f, 0f));
            AddSpotLight(details, new Color(.92f, .22f, .18f), 1.35f, 10f, 72f, new Vector3(-6.3f, 3.12f, 49.4f), new Vector3(82f, 15f, 0f));
            AddSpotLight(details, new Color(.78f, .92f, .90f), 1.55f, 11f, 64f, new Vector3(-12.5f, 3.12f, 42.5f), new Vector3(90f, 0f, 0f));
            AddSpotLight(details, new Color(.72f, .86f, .92f), 1.45f, 11f, 65f, new Vector3(-8f, 3.12f, 62f), new Vector3(90f, 0f, 0f));
            AddSpotLight(details, new Color(.95f, .52f, .18f), 1.50f, 12f, 75f, new Vector3(-8f, 3.05f, 77f), new Vector3(80f, 20f, 0f));
            AddSpotLight(details, new Color(.68f, .95f, .78f), 1.75f, 12f, 66f, new Vector3(6.4f, 3.12f, 91.5f), new Vector3(90f, 0f, 0f));
            AddSpotLight(details, new Color(.68f, .95f, .78f), 1.65f, 12f, 66f, new Vector3(13.2f, 3.12f, 85.0f), new Vector3(90f, 0f, 0f));
            AddSpotLight(details, new Color(.65f, .82f, .96f), 1.80f, 12f, 70f, new Vector3(-7.3f, 3.12f, 104f), new Vector3(90f, 0f, 0f));
            AddSpotLight(details, new Color(.88f, .24f, .20f), 1.35f, 11f, 68f, new Vector3(8f, 3.12f, 104f), new Vector3(90f, 0f, 0f));
        }
        public static void AddLight(Transform parent, Color color, float strength, float range, Vector3 offset = default, bool castShadows = false)
        {
            GameObject o = new GameObject("Luz"); o.transform.SetParent(parent, false);
            o.transform.localPosition = offset == default ? Vector3.down * .2f : offset;
            Light l = o.AddComponent<Light>(); l.type = LightType.Point; l.color = color; l.intensity = strength * .65f; l.range = range * .8f;
            l.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            if (castShadows) { l.shadowStrength = .85f; l.shadowBias = .02f; l.shadowNormalBias = .15f; }
            hospitalLights.Add(l);
        }
    }
}