using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpracTest
{
    public struct DriveHudState
    {
        public float SpeedKmh;
        public int Gear;
        public float Rpm;
        public float ElapsedSeconds;
        public int SceneIndex;
        public int SceneCount;
        public bool Asking;
        public bool Finished;
        public string Instruction;
        public string Situation;
        public string Title;
        public string[] Answers;
        public string Feedback;
        public int Correct;
        public int Minor;
        public int Deficient;
        public int Eliminating;
        public int Selected;
    }

    // Cuadro detrás del volante (tacómetro, velocidad y marcha) y pantalla
    // central con la pregunta a la izquierda y el mapa a la derecha.
    // El teclado sigue siendo A/B/C/D y R.
    public sealed class CabinInterface : MonoBehaviour
    {
        public event Action<int> AnswerSelected;
        public event Action RestartRequested;

        const int MapSize = 256;

        Camera eyes;
        Transform car;
        IReadOnlyList<Vector3> road;
        DriveHudState state;
        Font font;
        Sprite white;
        Sprite round;
        Text speedText;
        Text gearText;
        RectTransform rpmNeedle;
        RectTransform speedNeedle;
        Text header;
        Text situation;
        Text status;
        readonly Text[] answers = new Text[4];
        readonly Image[] rowFaces = new Image[4];
        readonly GameObject[] rows = new GameObject[4];
        Text routeLine;
        GameObject restart;
        Texture2D map;
        Color32[] mapPixels;
        GUIStyle caption;
        GUIStyle captionBox;
        Texture2D captionTexture;
        int shownSpeed = int.MinValue;
        int shownGear = int.MinValue;
        int shownScene = int.MinValue;
        int shownSelected = int.MinValue;
        bool shownAsking;
        bool shownFinished;
        float mapClock = 1f;
        float glideSpeed;
        float glideRpm = 800f;
        bool glideReady;

        public void Bind(Camera driver, Transform vehicle, ClioCabin cabin, IReadOnlyList<Vector3> centerline, CabinResources bin)
        {
            eyes = driver;
            car = vehicle;
            road = centerline;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            white = WhiteSprite(bin);
            round = CircleSprite(bin);
            map = bin.Track(new Texture2D(MapSize, MapSize, TextureFormat.RGBA32, false));
            map.wrapMode = TextureWrapMode.Clamp;
            map.filterMode = FilterMode.Bilinear;
            mapPixels = new Color32[MapSize * MapSize];
            captionTexture = bin.Track(Solid(new Color(0.04f, 0.05f, 0.07f, 0.88f)));

            BuildCluster(cabin.ClusterMount);
            BuildQuestions(cabin.QuestionMount);
            Apply(new DriveHudState
            {
                Asking = true,
                SceneCount = DrivingScenario.Demo.Length,
                Instruction = DrivingScenario.Demo[0].instruction,
                Situation = DrivingScenario.Demo[0].situation,
                Title = DrivingScenario.Demo[0].title,
                Answers = DrivingScenario.Demo[0].answers,
                Gear = 0,
                Rpm = 800f,
                Selected = -1
            });
            RedrawMap();
        }

        public void Apply(DriveHudState next)
        {
            state = next;
            if (next.SceneIndex != shownScene || next.Asking != shownAsking
                || next.Finished != shownFinished || next.Selected != shownSelected)
            {
                shownScene = next.SceneIndex;
                shownAsking = next.Asking;
                shownFinished = next.Finished;
                shownSelected = next.Selected;
                RefreshQuestions();
            }
        }

        // Las agujas no siguen un motor: suavizan la velocidad y las revoluciones
        // que entrega el ejercicio. La marcha del cuadro es la que ya muestra la palanca.
        void Update()
        {
            float dt = Time.deltaTime;
            if (!glideReady)
            {
                glideSpeed = state.SpeedKmh;
                glideRpm = state.Rpm > 1f ? state.Rpm : 800f;
                glideReady = true;
            }
            glideSpeed = Mathf.Lerp(glideSpeed, state.SpeedKmh, 1f - Mathf.Exp(-9f * dt));
            glideRpm = Mathf.Lerp(glideRpm, state.Rpm, 1f - Mathf.Exp(-3.4f * dt));
            int speed = Mathf.RoundToInt(glideSpeed);
            if (speedText != null && speed != shownSpeed)
            {
                speedText.text = speed.ToString();
                shownSpeed = speed;
            }
            if (gearText != null && state.Gear != shownGear)
            {
                gearText.text = state.Gear == 0 ? "N" : state.Gear.ToString();
                shownGear = state.Gear;
            }
            if (rpmNeedle != null)
                rpmNeedle.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(128f, -128f, Mathf.Clamp01(glideRpm / 7000f)));
            if (speedNeedle != null)
                speedNeedle.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(128f, -128f, Mathf.Clamp01(glideSpeed / 120f)));

            mapClock += dt;
            if (mapClock < 0.16f) return;
            mapClock = 0f;
            RedrawMap();
        }

        void OnGUI()
        {
            EnsureStyles();
            var current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                int index = current.keyCode == KeyCode.A ? 0 :
                    current.keyCode == KeyCode.B ? 1 :
                    current.keyCode == KeyCode.C ? 2 :
                    current.keyCode == KeyCode.D ? 3 : -1;
                if (index >= 0 && state.Asking)
                {
                    AnswerSelected?.Invoke(index);
                    current.Use();
                }
                else if (current.keyCode == KeyCode.R)
                {
                    RestartRequested?.Invoke();
                    current.Use();
                }
            }
            else if (current.type == EventType.MouseDown && current.button == 0 && eyes != null)
            {
                var pointer = current.mousePosition;
                pointer.y = Screen.height - pointer.y;
                Physics.SyncTransforms();
                Ray ray = eyes.ScreenPointToRay(pointer);
                if (Physics.Raycast(ray, out RaycastHit hit, 6f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
                {
                    string name = hit.collider.name;
                    if (name == "Reiniciar")
                    {
                        RestartRequested?.Invoke();
                        current.Use();
                    }
                    else if (name.Length == 10 && name.StartsWith("Respuesta"))
                    {
                        int index = name[9] - 'A';
                        if (index >= 0 && index < 4 && state.Asking)
                        {
                            AnswerSelected?.Invoke(index);
                            current.Use();
                        }
                    }
                }
            }

            if (caption == null) return;
            float margin = 16f;
            float width = Mathf.Min(460f, Screen.width * 0.46f);
            var area = new Rect(margin, margin, width, 48f);
            GUI.Box(area, GUIContent.none, captionBox);
            string line = state.Finished ? "Práctica finalizada." : state.Instruction;
            if (string.IsNullOrEmpty(line)) line = "…";
            GUI.Label(new Rect(area.x + 12f, area.y + 6f, area.width - 20f, area.height - 10f),
                "Instructor:  " + line, caption);
        }

        void BuildCluster(Transform mount)
        {
            var canvas = WorldCanvas("Cuadro", mount, new Vector2(860f, 360f), ClioLayout.ClusterSize.x);
            Image panel = ImageOf(canvas.transform, "Fondo", white, new Color(0.07f, 0.075f, 0.08f, 1f));
            Stretch(panel.rectTransform);

            var tachCenter = new Vector2(-230f, 8f);
            rpmNeedle = BuildDial(canvas.transform, tachCenter, 268f, 7, 6, 2, step => step.ToString());
            Label(canvas.transform, "Unidad rpm", "x1000", 16, FontStyle.Normal, new Color(0.62f, 0.68f, 0.72f),
                tachCenter + new Vector2(0f, -46f), new Vector2(80f, 22f));

            speedNeedle = BuildDial(canvas.transform, new Vector2(230f, 8f), 268f, 6, 7, 1, step => (step * 20).ToString());

            speedText = Label(canvas.transform, "Velocidad", "0", 54, FontStyle.Bold, Color.white,
                new Vector2(0f, 34f), new Vector2(150f, 72f));
            Label(canvas.transform, "Unidad", "km/h", 16, FontStyle.Normal, new Color(0.7f, 0.8f, 0.88f),
                new Vector2(0f, -8f), new Vector2(90f, 22f));
            gearText = Label(canvas.transform, "Marcha", "N", 28, FontStyle.Bold, new Color(0.55f, 0.95f, 0.72f),
                new Vector2(0f, -46f), new Vector2(70f, 36f));
            Label(canvas.transform, "Indicadores", "1/2    90°", 13, FontStyle.Normal, new Color(0.7f, 0.78f, 0.74f),
                new Vector2(0f, -96f), new Vector2(120f, 20f));
            Paint(canvas.transform);
        }

        RectTransform BuildDial(Transform parent, Vector2 center, float diameter, int divisions, int redFrom, int labelStep, Func<int, string> labelAt)
        {
            DialFace(parent, center, diameter);
            float radius = diameter * 0.5f;
            for (int step = 0; step <= divisions; step++)
            {
                float degrees = Mathf.Lerp(128f, -128f, step / (float)divisions);
                float rad = degrees * Mathf.Deg2Rad;
                var outward = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
                bool major = step % labelStep == 0;
                bool hot = step >= redFrom;
                float tickLen = major ? 18f : 10f;
                var tick = ImageOf(parent, "Marca", white, hot
                    ? new Color(0.86f, 0.18f, 0.14f)
                    : new Color(0.86f, 0.9f, 0.93f));
                var tickRect = tick.rectTransform;
                tickRect.pivot = new Vector2(0.5f, 0f);
                tickRect.sizeDelta = new Vector2(major ? 3.5f : 2f, tickLen);
                tickRect.anchoredPosition = center + outward * (radius - tickLen - 6f);
                tickRect.localEulerAngles = new Vector3(0f, 0f, degrees);
                if (major)
                {
                    Label(parent, "Escala", labelAt(step), 15, FontStyle.Normal, new Color(0.82f, 0.86f, 0.9f),
                        center + outward * (radius - tickLen - 28f), new Vector2(36f, 20f));
                }
            }

            var needle = ImageOf(parent, "Aguja", white, new Color(0.90f, 0.16f, 0.13f));
            var rect = needle.rectTransform;
            rect.pivot = new Vector2(0.5f, 0.08f);
            rect.sizeDelta = new Vector2(4f, radius - 22f);
            rect.anchoredPosition = center;
            var cap = ImageOf(parent, "Eje", round, new Color(0.92f, 0.93f, 0.94f));
            Place(cap.rectTransform, center, new Vector2(14f, 14f));
            return rect;
        }

        void DialFace(Transform parent, Vector2 center, float diameter)
        {
            var ring = ImageOf(parent, "Aro de esfera", round, new Color(0.45f, 0.5f, 0.54f, 1f));
            Place(ring.rectTransform, center, new Vector2(diameter + 8f, diameter + 8f));
            var face = ImageOf(parent, "Esfera", round, new Color(0.03f, 0.035f, 0.04f, 1f));
            Place(face.rectTransform, center, new Vector2(diameter, diameter));
        }

        void BuildQuestions(Transform mount)
        {
            var canvas = WorldCanvas("Preguntas", mount, new Vector2(840f, 480f), ClioLayout.QuestionSize.x);
            Image bezel = ImageOf(canvas.transform, "Fondo de pantalla", white, new Color(0.04f, 0.05f, 0.06f, 1f));
            Stretch(bezel.rectTransform);

            var rawGo = new GameObject("Minimapa");
            rawGo.transform.SetParent(canvas.transform, false);
            var raw = rawGo.AddComponent<RawImage>();
            raw.texture = map;
            raw.raycastTarget = false;
            Place(raw.rectTransform, new Vector2(292f, 36f), new Vector2(188f, 188f));
            Label(canvas.transform, "Aviso de mapa", "Rumbo ↑", 15, FontStyle.Bold,
                new Color(0.85f, 0.92f, 0.96f), new Vector2(292f, -78f), new Vector2(190f, 22f));
            routeLine = Label(canvas.transform, "Próxima indicación", "", 14, FontStyle.Normal,
                new Color(0.78f, 0.86f, 0.9f), new Vector2(292f, -112f), new Vector2(200f, 48f));

            Image panel = ImageOf(canvas.transform, "Tarjeta", white, new Color(0.96f, 0.97f, 0.96f, 1f));
            Place(panel.rectTransform, new Vector2(-118f, 4f), new Vector2(560f, 448f));
            header = Label(canvas.transform, "Encabezado", "Pregunta 1", 28, FontStyle.Bold,
                new Color(0.12f, 0.14f, 0.16f), new Vector2(-118f, 200f), new Vector2(520f, 32f));
            situation = Label(canvas.transform, "Situación", "", 22, FontStyle.Normal,
                new Color(0.16f, 0.18f, 0.2f), new Vector2(-118f, 150f), new Vector2(520f, 44f));
            situation.alignment = TextAnchor.MiddleLeft;
            status = Label(canvas.transform, "Estado", "", 16, FontStyle.Italic,
                new Color(0.16f, 0.18f, 0.2f), new Vector2(-118f, -186f), new Vector2(520f, 40f));
            status.alignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < 4; i++)
            {
                float y = 86f - i * 64f;
                var row = new GameObject("Respuesta" + "ABCD"[i]);
                row.transform.SetParent(canvas.transform, false);
                var rect = row.AddComponent<RectTransform>();
                Place(rect, new Vector2(-118f, y), new Vector2(520f, 56f));
                var image = row.AddComponent<Image>();
                image.sprite = white;
                image.color = new Color(0.90f, 0.91f, 0.90f, 1f);
                image.raycastTarget = false;
                rowFaces[i] = image;
                var collider = row.AddComponent<BoxCollider>();
                collider.size = new Vector3(520f, 56f, 40f);
                answers[i] = Label(row.transform, "Texto", "ABCD"[i].ToString(), 20, FontStyle.Normal,
                    new Color(0.1f, 0.12f, 0.14f), Vector2.zero, new Vector2(490f, 50f));
                answers[i].alignment = TextAnchor.MiddleLeft;
                answers[i].resizeTextForBestFit = true;
                answers[i].resizeTextMinSize = 14;
                answers[i].resizeTextMaxSize = 20;
                rows[i] = row;
            }

            restart = new GameObject("Reiniciar");
            restart.transform.SetParent(canvas.transform, false);
            var restartRect = restart.AddComponent<RectTransform>();
            Place(restartRect, new Vector2(-118f, -20f), new Vector2(360f, 44f));
            var restartImage = restart.AddComponent<Image>();
            restartImage.sprite = white;
            restartImage.color = new Color(0.16f, 0.28f, 0.38f, 1f);
            restartImage.raycastTarget = false;
            var restartCollider = restart.AddComponent<BoxCollider>();
            restartCollider.size = new Vector3(360f, 44f, 40f);
            Label(restart.transform, "Texto", "R   Volver a comenzar", 18, FontStyle.Bold,
                Color.white, Vector2.zero, new Vector2(340f, 38f));
            restart.SetActive(false);
        }

        void RefreshQuestions()
        {
            if (header == null) return;
            if (routeLine != null) routeLine.text = Shorten(state.Instruction);
            if (state.Finished)
            {
                header.text = "Resultado de la práctica";
                situation.text = "Correctas " + state.Correct + "    Leves " + state.Minor
                    + "\nDeficientes " + state.Deficient + "    Eliminatorias " + state.Eliminating;
                status.text = "El recorrido de esta escena sigue siendo ficticio.";
                SetRows(false);
                if (restart != null) restart.SetActive(true);
                return;
            }

            if (restart != null) restart.SetActive(false);
            header.text = "Pregunta " + (state.SceneIndex + 1) + " de " + Mathf.Max(1, state.SceneCount);
            situation.text = (state.Title ?? "") + "\n" + (state.Situation ?? "");
            SetRows(true);
            for (int i = 0; i < answers.Length; i++)
            {
                string choice = state.Answers != null && i < state.Answers.Length ? state.Answers[i] : "";
                answers[i].text = "ABCD"[i] + "    " + choice;
            }
            if (state.Asking)
            {
                status.text = "";
                Tint(-1);
            }
            else
            {
                status.text = state.Feedback ?? "";
                Tint(state.Selected);
            }
        }

        void Tint(int selected)
        {
            for (int i = 0; i < rowFaces.Length; i++)
            {
                if (rowFaces[i] == null) continue;
                bool on = selected == i;
                rowFaces[i].color = on
                    ? new Color(0.16f, 0.38f, 0.58f, 1f)
                    : new Color(0.90f, 0.91f, 0.90f, 1f);
                if (answers[i] != null)
                    answers[i].color = on ? Color.white : new Color(0.1f, 0.12f, 0.14f);
            }
        }

        static string Shorten(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length > 48 ? value.Substring(0, 46) + "…" : value;
        }

        void SetRows(bool visible)
        {
            for (int i = 0; i < rows.Length; i++)
                if (rows[i] != null) rows[i].SetActive(visible);
        }

        void RedrawMap()
        {
            if (map == null || car == null || road == null || road.Count < 2) return;
            int w = MapSize;
            int h = MapSize;
            var background = new Color32(14, 28, 40, 255);
            var asphalt = new Color32(168, 196, 214, 255);
            var arrow = new Color32(70, 230, 150, 255);
            var north = new Color32(230, 236, 240, 255);
            if (mapPixels == null || mapPixels.Length != w * h) mapPixels = new Color32[w * h];
            for (int i = 0; i < mapPixels.Length; i++) mapPixels[i] = background;

            Vector3 forward = car.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 1e-4f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            const float scale = 2.35f;
            for (int i = 1; i < road.Count; i++)
            {
                Project(road[i - 1], car.position, right, forward, scale, w, h, out int ax, out int ay);
                Project(road[i], car.position, right, forward, scale, w, h, out int bx, out int by);
                Stroke(mapPixels, w, h, ax, ay, bx, by, 2, asphalt);
            }

            int cx = w / 2;
            int cy = h / 2;
            for (int y = -7; y <= 11; y++)
            {
                float t = (y + 7) / 18f;
                int half = Mathf.RoundToInt(Mathf.Lerp(6f, 0f, t));
                for (int x = -half; x <= half; x++) Plot(mapPixels, w, h, cx + x, cy + y, arrow);
            }

            float northRight = Vector3.Dot(Vector3.forward, right);
            float northAhead = Vector3.Dot(Vector3.forward, forward);
            Stroke(mapPixels, w, h,
                cx + Mathf.RoundToInt(northRight * 22f), cy + Mathf.RoundToInt(northAhead * 22f),
                cx + Mathf.RoundToInt(northRight * 40f), cy + Mathf.RoundToInt(northAhead * 40f),
                1, north);
            map.SetPixels32(mapPixels);
            map.Apply(false);
        }

        static void Project(Vector3 world, Vector3 origin, Vector3 right, Vector3 forward, float scale, int w, int h, out int x, out int y)
        {
            Vector3 delta = world - origin;
            x = Mathf.RoundToInt(w * 0.5f + Vector3.Dot(delta, right) * scale);
            y = Mathf.RoundToInt(h * 0.5f + Vector3.Dot(delta, forward) * scale);
        }

        Canvas WorldCanvas(string name, Transform mount, Vector2 pixels, float widthMeters)
        {
            var go = new GameObject(name);
            go.transform.SetParent(mount, false);
            go.transform.localPosition = new Vector3(0f, 0f, 0.012f);
            go.transform.localRotation = Quaternion.identity;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = eyes;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = pixels;
            float scale = widthMeters / pixels.x;
            rect.localScale = new Vector3(scale, scale, scale);
            return canvas;
        }

        Text Label(Transform parent, string name, string value, int size, FontStyle style, Color color, Vector2 position, Vector2 box)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            Place(rect, position, box);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        Image ImageOf(Transform parent, string name, Sprite sprite, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        static void Paint(Transform root)
        {
            root.gameObject.layer = ClioLayout.CockpitLayer;
            for (int i = 0; i < root.childCount; i++) Paint(root.GetChild(i));
        }

        static Sprite CircleSprite(CabinResources bin)
        {
            const int size = 64;
            var tex = bin.Track(new Texture2D(size, size, TextureFormat.RGBA32, false));
            tex.wrapMode = TextureWrapMode.Clamp;
            float radius = size * 0.5f - 0.5f;
            var pixels = new Color[size * size];
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                pixels[y * size + x] = distance <= radius ? Color.white : new Color(0f, 0f, 0f, 0f);
            }
            tex.SetPixels(pixels);
            tex.Apply(false);
            return bin.Track(Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f));
        }

        static Sprite WhiteSprite(CabinResources bin)
        {
            var tex = bin.Track(Solid(Color.white));
            return bin.Track(Sprite.Create(tex, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 100f));
        }

        static Texture2D Solid(Color color)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { color, color, color, color });
            tex.Apply(false);
            return tex;
        }

        void EnsureStyles()
        {
            if (caption != null) return;
            int size = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 68f), 13, 16);
            caption = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.94f, 0.96f, 0.98f) }
            };
            captionBox = new GUIStyle(GUI.skin.box)
            {
                normal = { background = captionTexture }
            };
        }

        static void Plot(Color32[] pixels, int w, int h, int x, int y, Color32 color)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            pixels[y * w + x] = color;
        }

        static void Stroke(Color32[] pixels, int w, int h, int ax, int ay, int bx, int by, int radius, Color32 color)
        {
            float dx = bx - ax;
            float dy = by - ay;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy))));
            for (int i = 0; i <= steps; i++)
            {
                int x = Mathf.RoundToInt(ax + dx * i / steps);
                int y = Mathf.RoundToInt(ay + dy * i / steps);
                for (int oy = -radius; oy <= radius; oy++)
                for (int ox = -radius; ox <= radius; ox++)
                    Plot(pixels, w, h, x + ox, y + oy, color);
            }
        }
    }
}
