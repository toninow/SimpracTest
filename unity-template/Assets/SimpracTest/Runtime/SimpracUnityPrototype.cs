using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpracTest
{
    // Arranque de la escena de entrenamiento. Conserva preguntas, respuestas,
    // avance, reinicio y evaluación. El habitáculo vive en los tipos de Cabin/.
    public sealed class SimpracUnityPrototype : MonoBehaviour
    {
        public const double StartLatitude = 40.344103;
        public const double StartLongitude = -3.863962;

        // Calzada didáctica de 7,4 m con el eje dibujado en la polilínea.
        // El origen del coche queda en el centro del carril derecho.
        // No aplicar este desplazamiento a una calle de sentido único.
        const float DidacticRoadWidth = 7.4f;
        const float RightLaneCenter = DidacticRoadWidth * 0.25f;

        enum Phase { Question, Moving, Finished }

        Phase phase = Phase.Question;
        GameObject car;
        readonly List<Vector3> road = new List<Vector3>();
        readonly List<Material> roadMaterials = new List<Material>();
        DriverArmRig arms;
        GearLever lever;
        CabinInterface hud;

        float distance;
        float targetDistance;
        float movementSpeed;
        float speedKmH;
        float elapsed;
        float steering;
        int sceneIndex;
        readonly int[] faults = new int[4];
        string feedback = "";
        int selected = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<SimpracUnityPrototype>() != null) return;
            new GameObject("SimpracTest").AddComponent<SimpracUnityPrototype>();
        }

        void Start()
        {
            foreach (Camera existing in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                existing.enabled = false;
            foreach (AudioListener existing in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                existing.enabled = false;

            BuildRoad();
            car = new GameObject("Coche / referencia geográfica " + StartLatitude.ToString("0.000000") + ", " + StartLongitude.ToString("0.000000"));
            var resources = car.AddComponent<CabinResources>();
            var materials = new CabinMaterials(resources);
            Camera eyes = DriverView.Create(car.transform);
            ClioCabin cabin = ClioCabinBuilder.Build(car.transform, eyes, resources, materials);
            OccupantFigures.Build(car.transform, materials);

            arms = car.AddComponent<DriverArmRig>();
            arms.Bind(car.transform, cabin, materials);
            lever = car.AddComponent<GearLever>();
            lever.Bind(cabin, resources, materials);
            arms.Connect(lever);
            var mirrors = car.AddComponent<MirrorRig>();
            mirrors.Build(car.transform, cabin, resources, materials);

            hud = gameObject.AddComponent<CabinInterface>();
            hud.Bind(eyes, car.transform, cabin, road, resources);
            hud.AnswerSelected += SelectAnswer;
            hud.RestartRequested += ResetExercise;
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
            selected = -1;
            Array.Clear(faults, 0, faults.Length);
            UpdateCar();
            PushCabin();
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            if (phase == Phase.Moving)
            {
                distance = Mathf.MoveTowards(distance, targetDistance, movementSpeed * Time.deltaTime);
                speedKmH = Mathf.Min(45f, movementSpeed * 3.6f);
                UpdateCar();
                if (distance >= targetDistance - 0.01f)
                {
                    speedKmH = 0f;
                    sceneIndex++;
                    phase = sceneIndex >= DrivingScenario.Demo.Length ? Phase.Finished : Phase.Question;
                    feedback = "";
                    selected = -1;
                }
            }
            else speedKmH = 0f;
            PushCabin();
        }

        void PushCabin()
        {
            int gear = phase == Phase.Moving ? 2 : 0;
            if (arms != null) arms.SteerInput = steering;
            if (lever != null) lever.Gear = gear;
            // Revoluciones didácticas, sin embrague ni caja. El cuadro las suaviza.
            int shownGear = lever != null ? lever.Shown : gear;
            if (hud == null) return;
            int last = DrivingScenario.Demo.Length - 1;
            int index = Mathf.Clamp(sceneIndex, 0, last);
            DrivingScenario scenario = DrivingScenario.Demo[index];
            hud.Apply(new DriveHudState
            {
                SpeedKmh = speedKmH,
                Gear = shownGear,
                Rpm = gear == 0 ? 800f : 950f + speedKmH * 46f,
                ElapsedSeconds = elapsed,
                SceneIndex = index,
                SceneCount = DrivingScenario.Demo.Length,
                Asking = phase == Phase.Question,
                Finished = phase == Phase.Finished,
                Instruction = scenario.instruction,
                Situation = scenario.situation,
                Title = scenario.title,
                Answers = scenario.answers,
                Feedback = feedback,
                Correct = faults[0],
                Minor = faults[1],
                Deficient = faults[2],
                Eliminating = faults[3],
                Selected = selected
            });
        }

        void UpdateCar()
        {
            if (car == null || road.Count < 2) return;
            Vector3 direction;
            Vector3 center = PointAlongRoad(distance, out direction);
            car.transform.position = center + Vector3.Cross(Vector3.up, direction) * RightLaneCenter;
            Quaternion heading = Quaternion.LookRotation(direction, Vector3.up);
            car.transform.rotation = phase == Phase.Moving
                ? Quaternion.Slerp(car.transform.rotation, heading, Time.deltaTime * 4f)
                : heading;
            steering = Mathf.Clamp(Vector3.SignedAngle(car.transform.forward, direction, Vector3.up) / 25f, -1f, 1f);
        }

        Vector3 PointAlongRoad(float meters, out Vector3 forward)
        {
            float remaining = Mathf.Max(0f, meters);
            for (int i = 1; i < road.Count; i++)
            {
                Vector3 delta = road[i] - road[i - 1];
                float length = delta.magnitude;
                if (length < 0.001f) continue;
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
            selected = index;
            int grade = scenario.grades[index];
            faults[Mathf.Clamp(grade, 0, 3)]++;
            feedback = grade == 0 ? "Decisión registrada. Ejecutando maniobra…" :
                "Decisión registrada (gravedad didáctica: " + (grade == 1 ? "leve" : grade == 2 ? "deficiente" : "eliminatoria") + ").";
            phase = Phase.Moving;
            movementSpeed = grade == 0 ? 10f : 8f;
            targetDistance = Mathf.Min(TotalRoadLength() - 0.01f, distance + 47f);
            PushCabin();
        }

        void BuildRoad()
        {
            // Trazado creado para probar la conducción en Unity; NO son calles de Móstoles.
            road.AddRange(new[]
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
                Block("Calzada didáctica", null, new Vector3(DidacticRoadWidth, .07f, length + 1f),
                    (a + b) * .5f, asphalt).transform.rotation = yaw;
                foreach (float sign in new[] { -1f, 1f })
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
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightObject.transform.rotation = Quaternion.Euler(47f, -35f, 0f);
            RenderSettings.ambientLight = new Color(.67f, .70f, .75f);
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
            roadMaterials.Add(material);
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

        void OnDestroy()
        {
            for (int i = 0; i < roadMaterials.Count; i++)
                if (roadMaterials[i] != null) Destroy(roadMaterials[i]);
        }
    }
}
