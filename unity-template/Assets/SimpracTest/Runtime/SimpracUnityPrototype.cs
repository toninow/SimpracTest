using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpracTest
{
    // Fase 1: prototipo autocontenido. Arranca al pulsar Play en una escena vacía HDRP.
    // NO descarga calles ni afirma reproducir el examen real de la DGT.
    public sealed class SimpracUnityPrototype : MonoBehaviour
    {
        public const double StartLatitude = 40.344103;
        public const double StartLongitude = -3.863962;
        const int CockpitLayer = 8;
        const float LaneOffset = 1.55f;

        enum Phase { Question, Moving, Finished }
        Phase phase = Phase.Question;
        GameObject car;
        Camera driverCamera;
        Transform wheel;
        readonly List<Vector3> road = new List<Vector3>();
        readonly List<Camera> mirrors = new List<Camera>();
        readonly List<RenderTexture> mirrorTextures = new List<RenderTexture>();
        Texture2D mapTexture;
        Texture2D panelTexture;
        Texture2D buttonTexture;
        GUIStyle labelStyle;
        GUIStyle smallStyle;
        GUIStyle titleStyle;
        GUIStyle buttonStyle;
        GUIStyle panelStyle;

        float distance;
        float targetDistance;
        float movementSpeed;
        float speedKmH;
        float elapsed;
        float mapClock;
        float steering;
        int sceneIndex;
        int[] faults = new int[4];
        string feedback = "";
        Vector3 screenLocal = new Vector3(.52f, -.34f, 1.16f);
        Vector2 screenSize = new Vector2(.92f, .64f);
        Vector3 clusterLocal = new Vector3(-.42f, -.35f, 1.05f);
        Vector2 clusterSize = new Vector2(.54f, .30f);
        readonly Vector3[] mirrorLocal =
        {
            new Vector3(-1.10f, .17f, 1.23f),
            new Vector3(0f, .57f, 1.14f),
            new Vector3(1.10f, .17f, 1.23f)
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<SimpracUnityPrototype>() != null) return;
            new GameObject("SimpracTest · Unity / maqueta funcional").AddComponent<SimpracUnityPrototype>();
        }

        void Start()
        {
            foreach (Camera existing in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                existing.enabled = false;
            foreach (AudioListener existing in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                existing.enabled = false;

            BuildRoad();
            BuildCar();
            BuildCockpit();
            BuildOccupants();
            BuildMirrors();
            CreateUIAssets();
            ResetExercise();
        }

        void ResetExercise()
        {
            phase = Phase.Question;
            distance = 0f;
            targetDistance = 0f;
            movementSpeed = 0f;
            speedKmH = 0f;
            elapsed = 0f;
            sceneIndex = 0;
            steering = 0f;
            feedback = "";
            Array.Clear(faults, 0, faults.Length);
            UpdateCar();
            DrawMap();
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            if (phase == Phase.Moving)
            {
                distance = Mathf.MoveTowards(distance, targetDistance, movementSpeed * Time.deltaTime);
                speedKmH = Mathf.Min(45f, movementSpeed * 3.6f);
                UpdateCar();
                if (distance >= targetDistance - .01f)
                {
                    speedKmH = 0f;
                    sceneIndex++;
                    phase = sceneIndex >= DrivingScenario.Demo.Length ? Phase.Finished : Phase.Question;
                    feedback = "";
                }
            }
            else speedKmH = 0f;

            if (wheel != null)
                wheel.localRotation = Quaternion.Euler(12f, 0f, Mathf.LerpAngle(
                    wheel.localEulerAngles.z, -steering * 28f, Time.deltaTime * 6f));

            mapClock += Time.deltaTime;
            if (mapClock > .16f)
            {
                mapClock = 0f;
                DrawMap();
            }
        }

        void UpdateCar()
        {
            if (car == null || road.Count < 2) return;
            Vector3 direction;
            Vector3 center = PointAlongRoad(distance, out direction);
            car.transform.position = center + Vector3.Cross(Vector3.up, direction) * LaneOffset;
            Quaternion heading = Quaternion.LookRotation(direction, Vector3.up);
            car.transform.rotation = phase == Phase.Moving
                ? Quaternion.Slerp(car.transform.rotation, heading, Time.deltaTime * 4f) : heading;
            steering = Mathf.Clamp(Vector3.SignedAngle(car.transform.forward, direction, Vector3.up) / 25f, -1f, 1f);
        }

        Vector3 PointAlongRoad(float meters, out Vector3 forward)
        {
            float remaining = Mathf.Max(0f, meters);
            for (int i = 1; i < road.Count; i++)
            {
                Vector3 delta = road[i] - road[i - 1];
                float length = delta.magnitude;
                if (length < .001f) continue;
                if (remaining <= length || i == road.Count - 1)
                {
                    forward = delta / length;
                    return Vector3.Lerp(road[i - 1], road[i], Mathf.Clamp01(remaining / length));
                }
                remaining -= length;
            }
            forward = Vector3.forward;
            return road[road.Count - 1];
        }

        float TotalRoadLength()
        {
            float total = 0f;
            for (int i = 1; i < road.Count; i++) total += Vector3.Distance(road[i - 1], road[i]);
            return total;
        }

        void SelectAnswer(int index)
        {
            if (phase != Phase.Question || sceneIndex >= DrivingScenario.Demo.Length) return;
            var scenario = DrivingScenario.Demo[sceneIndex];
            if (index < 0 || index >= scenario.answers.Length) return;
            int grade = scenario.grades[index];
            faults[Mathf.Clamp(grade, 0, 3)]++;
            feedback = grade == 0 ? "Decisión registrada. Ejecutando maniobra…" :
                "Decisión registrada (gravedad didáctica: " + (grade == 1 ? "leve" : grade == 2 ? "deficiente" : "eliminatoria") + ").";
            phase = Phase.Moving;
            movementSpeed = grade == 0 ? 10f : 8f;
            targetDistance = Mathf.Min(TotalRoadLength() - .01f, distance + 47f);
        }

        void BuildRoad()
        {
            // Trazado creado para probar la conducción en Unity; NO son calles de Móstoles.
            road.AddRange(new []
            {
                new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 85f),
                new Vector3(0f, 0f, 152f), new Vector3(26f, 0f, 186f),
                new Vector3(88f, 0f, 190f), new Vector3(118f, 0f, 235f),
                new Vector3(118f, 0f, 310f), new Vector3(118f, 0f, 390f)
            });
            Material asphalt = MakeMaterial(new Color(.13f, .15f, .17f));
            Material grass = MakeMaterial(new Color(.29f, .38f, .27f));
            Material white = MakeMaterial(new Color(.87f, .86f, .78f));
            Material concrete = MakeMaterial(new Color(.48f, .47f, .44f));
            Material wall = MakeMaterial(new Color(.60f, .59f, .56f));

            Block("Terreno", null, new Vector3(1000f, .15f, 1000f), new Vector3(40f, -.13f, 160f), grass);
            for (int i = 1; i < road.Count; i++)
            {
                Vector3 a = road[i - 1], b = road[i];
                Vector3 delta = b - a;
                float length = delta.magnitude;
                if (length < .01f) continue;
                Quaternion yaw = Quaternion.LookRotation(delta.normalized, Vector3.up);
                Block("Calzada didáctica", null, new Vector3(7.4f, .07f, length + 1f),
                    (a + b) * .5f, asphalt).transform.rotation = yaw;
                foreach (float sign in new [] {-1f, 1f})
                {
                    var edge = Block("Marca borde", null, new Vector3(.07f, .012f, length),
                        (a + b) * .5f + yaw * new Vector3(sign * 3.55f, .048f, 0f), white);
                    edge.transform.rotation = yaw;
                    var sidewalk = Block("Acera", null, new Vector3(2.1f, .12f, length),
                        (a + b) * .5f + yaw * new Vector3(sign * 4.85f, -.005f, 0f), concrete);
                    sidewalk.transform.rotation = yaw;
                }
                for (float traveled = 3f; traveled < length - 2f; traveled += 10f)
                {
                    Vector3 position = a + delta.normalized * traveled;
                    Block("Marca discontinua", null, new Vector3(.11f, .013f, 3.2f),
                        position + Vector3.up * .05f, white).transform.rotation = yaw;
                }
                // Fachadas sencillas para que haya referencias visibles en los retrovisores.
                if (i % 2 == 1)
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float height = 8f + (i % 3) * 3f;
                        var building = Block("Edificio de entrenamiento", null, new Vector3(13f, height, 10f),
                            (a + b) * .5f + yaw * new Vector3(side * 16f, height / 2f, 0f), wall);
                        building.transform.rotation = yaw;
                    }
                }
            }
            var lightObject = new GameObject("Luz solar de la maqueta");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;light.intensity = 1.3f;
            lightObject.transform.rotation = Quaternion.Euler(47f, -35f, 0f);
            RenderSettings.ambientLight = new Color(.67f, .70f, .75f);
        }

        void BuildCar()
        {
            car = new GameObject("Coche / coordenada inicial: " + StartLatitude + ", " + StartLongitude);
            driverCamera = new GameObject("Ojos del conductor").AddComponent<Camera>();
            driverCamera.transform.SetParent(car.transform, false);
            driverCamera.transform.localPosition = new Vector3(-.42f, 1.28f, -.17f);
            driverCamera.transform.localRotation = Quaternion.identity;
            driverCamera.fieldOfView = 76f;
            driverCamera.nearClipPlane = .045f;
            driverCamera.farClipPlane = 460f;
            driverCamera.depth = 2;
            driverCamera.clearFlags = CameraClearFlags.Skybox;
            driverCamera.enabled = true;
            driverCamera.gameObject.AddComponent<AudioListener>();
            Block("Capó exterior simplificado", car.transform, new Vector3(1.7f, .13f, .95f),
                new Vector3(0f, .78f, 1.68f), MakeMaterial(new Color(.10f, .26f, .35f))).layer = CockpitLayer;
        }

        Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }

        GameObject Block(string title, Transform parent, Vector3 scale, Vector3 position, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = title;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = position;
            go.GetComponent<Renderer>().sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return go;
        }

        GameObject Sphere(string title, Transform parent, Vector3 scale, Vector3 position, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = title;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.transform.localPosition = position;
            go.GetComponent<Renderer>().sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return go;
        }

        GameObject Tube(string title, Transform parent, Vector3 a, Vector3 b, float radius, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = title;
            go.transform.SetParent(parent, false);
            Vector3 delta = b - a;
            go.transform.localPosition = (a + b) * .5f;
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            go.transform.localScale = new Vector3(radius * 2f, delta.magnitude * .5f, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return go;
        }

        void Ring(string title, Transform parent, Vector3 center, float radius, float thickness, Material material)
        {
            const int segments = 40;
            var points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                points[i] = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }
            for (int i = 0; i < segments; i++)
                Tube(title, parent, points[i], points[(i + 1) % segments], thickness, material);
        }

        void MarkCockpitLayer(Transform root)
        {
            root.gameObject.layer = CockpitLayer;
            foreach (Transform child in root) MarkCockpitLayer(child);
        }

        void BuildCockpit()
        {
            Transform cabin = new GameObject("Habitáculo provisional / Clio de acceso").transform;
            cabin.SetParent(driverCamera.transform, false);
            Material plastic = MakeMaterial(new Color(.085f, .095f, .11f));
            Material trim = MakeMaterial(new Color(.24f, .27f, .30f));
            Material screen = MakeMaterial(new Color(.015f, .026f, .043f));
            Material sleeve = MakeMaterial(new Color(.10f, .16f, .24f));
            Material skin = MakeMaterial(new Color(.56f, .36f, .25f));

            Block("Salpicadero", cabin, new Vector3(2.38f, .28f, .45f),
                new Vector3(.16f, -.77f, 1.22f), plastic);
            Block("Visera", cabin, new Vector3(2.38f, .055f, .42f),
                new Vector3(.16f, -.61f, 1.30f), trim);
            Block("Pilar A izquierdo", cabin, new Vector3(.07f, 1.43f, .09f),
                new Vector3(-1.22f, .16f, 1.24f), plastic).transform.localRotation = Quaternion.Euler(0f, 0f, -9f);
            Block("Pilar A derecho", cabin, new Vector3(.07f, 1.43f, .09f),
                new Vector3(1.24f, .16f, 1.24f), plastic).transform.localRotation = Quaternion.Euler(0f, 0f, 9f);
            Block("Marco superior parabrisas", cabin, new Vector3(2.50f, .07f, .13f),
                new Vector3(0f, .94f, 1.2f), plastic);
            Block("Instrumentos digitales y hueco del minimapa", cabin,
                new Vector3(.58f, .32f, .06f), clusterLocal, screen);
            Block("Pantalla central de preguntas", cabin,
                new Vector3(screenSize.x + .07f, screenSize.y + .07f, .07f),
                screenLocal, screen);
            Block("Consola central", cabin, new Vector3(.37f, .31f, .83f),
                new Vector3(.40f, -1.05f, .78f), trim);
            // Volante, palanca manual y brazos en primera persona: aún no tienen rig profesional.
            wheel = new GameObject("Volante manual").transform;
            wheel.SetParent(cabin, false);
            wheel.localPosition = new Vector3(-.40f, -.39f, .98f);
            Ring("Aro volante", wheel, Vector3.zero, .27f, .017f, plastic);
            Block("Centro volante", wheel, new Vector3(.20f, .15f, .085f), new Vector3(0f, 0f, .018f), trim);
            foreach (float side in new [] {-1f, 1f})
            {
                Tube("Radio volante", wheel, new Vector3(side * .07f, 0f, .02f),
                    new Vector3(side * .235f, .04f, 0f), .032f, trim);
            }
            Tube("Columna cambio", cabin, new Vector3(.42f, -.94f, .50f),
                new Vector3(.42f, -.74f, .50f), .015f, trim);
            Sphere("Pomo cambio", cabin, new Vector3(.12f, .085f, .11f),
                new Vector3(.42f, -.72f, .50f), plastic);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 elbow = new Vector3(side < 0 ? -.79f : .18f, -.93f, .18f);
                Vector3 wrist = new Vector3(-.40f + side * .23f, -.36f, .94f);
                Vector3 fore = Vector3.Lerp(elbow, wrist, .67f);
                Tube("Brazo del conductor", cabin, elbow, fore, .047f, sleeve);
                Tube("Antebrazo y muñeca", cabin, fore, wrist, .035f, skin);
                Sphere("Mano conductor", cabin, new Vector3(.09f, .07f, .10f), wrist, skin);
            }
            MarkCockpitLayer(cabin);
        }

        void BuildOccupants()
        {
            Material dark = MakeMaterial(new Color(.15f, .19f, .25f));
            Material skin = MakeMaterial(new Color(.61f, .45f, .34f));
            MakePassenger("Instructor · asiento delantero derecho", new Vector3(.61f, 0f, .67f), dark, skin, CockpitLayer);
            MakePassenger("Examinador · asiento trasero derecho", new Vector3(.60f, 0f, -1.22f), dark, skin, 0);
        }

        void MakePassenger(string title, Vector3 location, Material clothes, Material skin, int layer)
        {
            var passenger = new GameObject(title);
            passenger.transform.SetParent(car.transform, false);
            passenger.transform.localPosition = location;
            var chair = Block("Asiento", passenger.transform, new Vector3(.44f, .84f, .20f),
                new Vector3(0f, .72f, -.18f), clothes);
            Sphere("Torso", passenger.transform, new Vector3(.38f, .56f, .29f), new Vector3(0f, .91f, .04f), clothes);
            Sphere("Cabeza", passenger.transform, new Vector3(.23f, .25f, .22f), new Vector3(0f, 1.39f, .035f), skin);
            foreach (float s in new [] {-1f, 1f})
                Tube("Brazo", passenger.transform, new Vector3(s * .19f, 1.12f, .03f),
                    new Vector3(s * .23f, .72f, .26f), .047f, clothes);
            if (layer == CockpitLayer) MarkCockpitLayer(passenger.transform);
        }

        void BuildMirrors()
        {
            Material dark = MakeMaterial(new Color(.08f, .09f, .11f));
            for (int i = 0; i < 3; i++)
            {
                float yaw = i == 0 ? 193f : i == 1 ? 180f : 167f;
                var camera = new GameObject(i == 0 ? "Espejo izquierdo" :
                    i == 1 ? "Retrovisor interior" : "Espejo derecho").AddComponent<Camera>();
                camera.transform.SetParent(car.transform, false);
                camera.transform.localPosition = new Vector3(i == 0 ? -.86f : i == 2 ? .86f : 0f,
                    i == 1 ? 1.57f : 1.33f, i == 1 ? .12f : .54f);
                camera.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                camera.fieldOfView = i == 1 ? 52f : 67f;
                camera.nearClipPlane = .07f;
                camera.farClipPlane = 350f;
                camera.cullingMask &= ~(1 << CockpitLayer);
                camera.depth = -1;
                var target = new RenderTexture(i == 1 ? 512 : 320, i == 1 ? 192 : 160, 16,
                    RenderTextureFormat.ARGB32);
                target.name = camera.name + " · textura dinámica";
                target.Create();
                camera.targetTexture = target;
                mirrors.Add(camera);
                mirrorTextures.Add(target);
                // El marco real queda integrado en el habitáculo; las imágenes se
                // posicionan sobre él usando la proyección de la cámara principal.
                var frame = Block("Marco " + camera.name, driverCamera.transform,
                    new Vector3(i == 1 ? .43f : .34f, i == 1 ? .19f : .22f, .045f),
                    mirrorLocal[i], dark);
                frame.layer = CockpitLayer;
            }
        }

        void CreateUIAssets()
        {
            panelTexture = Solid(new Color(.035f, .055f, .085f, .96f));
            buttonTexture = Solid(new Color(.13f, .22f, .33f, 1f));
            mapTexture = new Texture2D(224, 128, TextureFormat.RGBA32, false);
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            mapTexture.filterMode = FilterMode.Bilinear;
            SetupStyles();
        }

        Texture2D Solid(Color color)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, color);t.Apply();return t;
        }

        void SetupStyles()
        {
            int baseSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 68f), 11, 17);
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(.91f, .96f, 1f) },
                fontSize = baseSize, wordWrap = true
            };
            smallStyle = new GUIStyle(labelStyle)
            {
                fontSize = Mathf.Max(10, baseSize - 2),
                normal = { textColor = new Color(.65f, .80f, .91f) }
            };
            titleStyle = new GUIStyle(labelStyle)
            {
                fontSize = baseSize + 2, fontStyle = FontStyle.Bold
            };
            panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = panelTexture } };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = buttonTexture, textColor = Color.white },
                hover = { background = buttonTexture, textColor = Color.white },
                active = { background = buttonTexture, textColor = new Color(.65f, .92f, 1f) },
                fontSize = baseSize, wordWrap = true,
                alignment = TextAnchor.MiddleLeft, padding = new RectOffset(8, 5, 3, 3)
            };
        }

        Rect ProjectPanel(Vector3 centerLocal, Vector2 size)
        {
            if (driverCamera == null) return new Rect(0, 0, 1, 1);
            Vector3 a = driverCamera.WorldToScreenPoint(driverCamera.transform.TransformPoint(
                centerLocal + new Vector3(-size.x * .5f, size.y * .5f, 0f)));
            Vector3 b = driverCamera.WorldToScreenPoint(driverCamera.transform.TransformPoint(
                centerLocal + new Vector3(size.x * .5f, -size.y * .5f, 0f)));
            if (a.z <= 0f || b.z <= 0f) return new Rect(0, 0, 1, 1);
            return Rect.MinMaxRect(a.x, Screen.height - a.y, b.x, Screen.height - b.y);
        }

        void OnGUI()
        {
            if (driverCamera == null || labelStyle == null) return;
            if (Event.current.type == EventType.KeyDown)
            {
                int index = Event.current.keyCode == KeyCode.A ? 0 :
                    Event.current.keyCode == KeyCode.B ? 1 :
                    Event.current.keyCode == KeyCode.C ? 2 :
                    Event.current.keyCode == KeyCode.D ? 3 : -1;
                if (index >= 0 && phase == Phase.Question)
                {
                    SelectAnswer(index);Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.R)
                {
                    ResetExercise();Event.current.Use();
                }
            }

            float margin = Mathf.Max(10f, Screen.width * .015f);
            GUI.Box(new Rect(margin, margin, Mathf.Min(510f, Screen.width * .46f), 62f),
                GUIContent.none, panelStyle);
            GUI.Label(new Rect(margin + 9f, margin + 4f, Mathf.Min(495f, Screen.width * .45f), 22f),
                "INSTRUCTOR · " + (phase == Phase.Finished ? "Fin de la práctica" :
                    DrivingScenario.Demo[Mathf.Min(sceneIndex, DrivingScenario.Demo.Length - 1)].instruction), smallStyle);
            GUI.Label(new Rect(margin + 9f, margin + 30f, Mathf.Min(495f, Screen.width * .45f), 25f),
                "Salida indicada: 40.344103, -3.863962 · CALLES FICTICIAS", smallStyle);
            var cluster = ProjectPanel(clusterLocal, clusterSize);
            GUI.Box(cluster, GUIContent.none, panelStyle);
            float inset = Mathf.Max(3f, cluster.width * .025f);
            GUI.Label(new Rect(cluster.x + inset, cluster.y + inset,
                cluster.width * .34f, 21f), Mathf.RoundToInt(speedKmH) + " km/h", titleStyle);
            GUI.Label(new Rect(cluster.x + inset, cluster.y + 24f,
                cluster.width * .31f, 24f), phase == Phase.Moving ? "2ª marcha" : "N", smallStyle);
            if (mapTexture != null)
            {
                var mapArea = new Rect(cluster.x + cluster.width * .35f, cluster.y + inset,
                    cluster.width * .62f, cluster.height - inset * 2f);
                GUI.DrawTexture(mapArea, mapTexture, ScaleMode.StretchToFill, false);
                GUI.Label(new Rect(mapArea.x + 3f, mapArea.y + 3f, mapArea.width - 6f, 20f),
                    "RUTA DE DEMOSTRACIÓN", smallStyle);
            }

            for (int i = 0; i < mirrorTextures.Count; i++)
            {
                Vector2 size = i == 1 ? new Vector2(.39f, .155f) : new Vector2(.30f, .18f);
                var mirrorRect = ProjectPanel(mirrorLocal[i], size);
                GUI.DrawTextureWithTexCoords(mirrorRect, mirrorTextures[i], new Rect(1f, 0f, -1f, 1f));
            }

            Rect screen = ProjectPanel(screenLocal, screenSize);
            if (screen.width < 250f || screen.height < 215f ||
                screen.x < 5f || screen.xMax > Screen.width - 5f || screen.yMax > Screen.height - 5f)
            {
                screen = new Rect(Screen.width * .57f, Screen.height * .43f,
                    Mathf.Min(Screen.width * .41f, 530f), Mathf.Min(Screen.height * .47f, 400f));
            }
            GUI.Box(screen, GUIContent.none, panelStyle);
            float pad = Mathf.Max(5f, screen.width * .025f);
            Rect content = new Rect(screen.x + pad, screen.y + pad,
                screen.width - pad * 2f, screen.height - pad * 2f);
            GUI.Label(new Rect(content.x, content.y, content.width, 22f),
                "SimpracTest  ·  PRÁCTICA  ·  " + (phase == Phase.Finished ? "RESULTADO" :
                    "ESCENA " + (sceneIndex + 1) + "/" + DrivingScenario.Demo.Length), titleStyle);
            float y = content.y + 26f;
            if (phase == Phase.Finished)
            {
                GUI.Label(new Rect(content.x, y, content.width, 65f),
                    "Fin de la maqueta. Correctas: " + faults[0] +
                    " · Leves: " + faults[1] + " · Deficientes: " + faults[2] +
                    " · Eliminatorias: " + faults[3], labelStyle);
                y += 78f;
                if (GUI.Button(new Rect(content.x, y, content.width, 33f),
                    "R · Volver a comenzar", buttonStyle)) ResetExercise();
            }
            else
            {
                DrivingScenario scenario = DrivingScenario.Demo[sceneIndex];
                GUI.Label(new Rect(content.x, y, content.width, 22f), scenario.title, titleStyle);
                y += 24f;
                GUI.Label(new Rect(content.x, y, content.width, 38f), scenario.situation, smallStyle);
                y += 40f;
                if (phase == Phase.Question)
                {
                    float gap = Mathf.Clamp(content.height * .012f, 2f, 6f);
                    float choiceHeight = Mathf.Max(24f, (content.yMax - y - gap * 3f) * .25f);
                    for (int i = 0; i < scenario.answers.Length; i++)
                    {
                        if (GUI.Button(new Rect(content.x, y, content.width, choiceHeight),
                            "ABCD"[i] + " · " + scenario.answers[i], buttonStyle))
                        {
                            SelectAnswer(i);
                            break;
                        }
                        y += choiceHeight + gap;
                    }
                }
                else GUI.Label(new Rect(content.x, y, content.width, 68f),
                    feedback + "\nLa carretera y el minimapa avanzan juntos.", labelStyle);
            }

            GUI.Label(new Rect(margin, Screen.height - 52f, Mathf.Min(Screen.width - 2f * margin, 900f), 43f),
                "A/B/C/D · Responder     R · Reiniciar     |     " +
                "Tiempo " + Mathf.FloorToInt(elapsed / 60f).ToString("00") + ":" +
                Mathf.FloorToInt(elapsed % 60f).ToString("00") +
                "     |     Prototipo Unity · recorrido ficticio, sin señales oficiales", smallStyle);
        }

        void DrawMap()
        {
            if (mapTexture == null || car == null) return;
            int w = mapTexture.width, h = mapTexture.height;
            var pixels = new Color32[w * h];
            Color32 bg = new Color32(10, 25, 41, 255);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
            float scale = .55f;
            Vector3 carPos = car.transform.position;
            for (int i = 1; i < road.Count; i++)
            {
                Vector3 a = road[i - 1], b = road[i];
                int ax = Mathf.RoundToInt(w / 2f + (a.x - carPos.x) * scale);
                int ay = Mathf.RoundToInt(h / 2f + (a.z - carPos.z) * scale);
                int bx = Mathf.RoundToInt(w / 2f + (b.x - carPos.x) * scale);
                int by = Mathf.RoundToInt(h / 2f + (b.z - carPos.z) * scale);
                Line(pixels, w, h, ax, ay, bx, by, new Color32(155, 183, 201, 255));
            }
            Dot(pixels, w, h, w / 2, h / 2, 4, new Color32(61, 238, 168, 255));
            // Norte del mapa arriba; la flecha gira según la orientación del coche.
            Vector3 f = car.transform.forward;
            Line(pixels, w, h, w / 2, h / 2,
                w / 2 + Mathf.RoundToInt(f.x * 10), h / 2 + Mathf.RoundToInt(f.z * 10),
                new Color32(61, 238, 168, 255));
            mapTexture.SetPixels32(pixels);
            mapTexture.Apply(false);
        }

        static void Dot(Color32[] p, int w, int h, int x, int y, int radius, Color32 color)
        {
            for (int yy = -radius; yy <= radius; yy++)
            for (int xx = -radius; xx <= radius; xx++)
            {
                int px = x + xx, py = y + yy;
                if (px >= 0 && px < w && py >= 0 && py < h &&
                    xx * xx + yy * yy <= radius * radius) p[py * w + px] = color;
            }
        }

        static void Line(Color32[] p, int w, int h, int ax, int ay, int bx, int by, Color32 color)
        {
            float dx = bx - ax, dy = by - ay;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy))));
            for (int i = 0; i <= steps; i++)
            {
                int x = Mathf.RoundToInt(ax + dx * i / steps);
                int y = Mathf.RoundToInt(ay + dy * i / steps);
                if (x >= 0 && x < w && y >= 0 && y < h) p[y * w + x] = color;
            }
        }

        void OnDestroy()
        {
            foreach (Camera cam in mirrors) if (cam != null) cam.targetTexture = null;
            foreach (RenderTexture rt in mirrorTextures)
            {
                if (rt == null) continue;
                rt.Release();Destroy(rt);
            }
            if (mapTexture != null) Destroy(mapTexture);
            if (panelTexture != null) Destroy(panelTexture);
            if (buttonTexture != null) Destroy(buttonTexture);
        }
    }
}
