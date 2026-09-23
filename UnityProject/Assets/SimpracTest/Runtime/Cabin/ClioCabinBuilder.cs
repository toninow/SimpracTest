using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace SimpracTest
{
    // Habitáculo procedural de un utilitario de acceso: negro y gris, plástico
    // mate. La foto adjunta es solo referencia de encuadre. El coche no es un
    // Renault; el centro del volante lleva la marca Spt.
    public static class ClioCabinBuilder
    {
        public static ClioCabin Build(Transform car, Camera eyes, CabinResources bin, CabinMaterials mats)
        {
            var cabin = new ClioCabin();
            cabin.Root = new GameObject("Habitáculo").transform;
            cabin.Root.SetParent(car, false);

            Dashboard(cabin.Root, bin, mats);
            DoorsPillarsAndGlass(cabin.Root, mats);
            DriverSeat(cabin.Root, mats);
            cabin.Wheel = SteeringWheel(cabin, bin, mats);
            CenterStack(cabin, eyes, mats);
            ClusterBinnacle(cabin, eyes, mats);
            Shifter(cabin, bin, mats);
            Exterior(car, mats);
            MirrorFrames(cabin, eyes, mats);
            CabinLight(cabin.Root);
            return cabin;
        }

        static void Dashboard(Transform root, CabinResources bin, CabinMaterials mats)
        {
            var pad = SmoothProfile(new[]
            {
                new Vector2(0.80f, 1.05f), new Vector2(0.84f, 1.00f),
                new Vector2(0.88f, 0.92f), new Vector2(0.90f, 0.84f),
                new Vector2(0.88f, 0.76f), new Vector2(0.82f, 0.66f),
                new Vector2(0.74f, 0.56f), new Vector2(0.64f, 0.48f),
                new Vector2(0.52f, 0.40f), new Vector2(0.40f, 0.36f)
            });
            MeshPart("Salpicadero", ProceduralMeshes.Ribbon(pad, -0.78f, 0.76f), root,
                Vector3.zero, Quaternion.identity, mats.Plastic, ClioLayout.CockpitLayer, bin);

            var cowl = new List<Vector2>
            {
                new Vector2(0.93f, 0.90f),
                new Vector2(0.98f, 0.78f),
                new Vector2(1.02f, 0.66f),
                new Vector2(0.96f, 0.60f)
            };
            MeshPart("Visera del parabrisas", ProceduralMeshes.Ribbon(cowl, -0.72f, 0.72f), root,
                Vector3.zero, Quaternion.identity, mats.PlasticDark, ClioLayout.CockpitLayer, bin);

            Box("Suelo del habitáculo", root, new Vector3(1.55f, 0.04f, 1.35f),
                new Vector3(0f, 0.22f, 0.15f), Vector3.zero, mats.Fabric, ClioLayout.CockpitLayer);
            Box("Alfombrilla", root, new Vector3(0.48f, 0.015f, 0.55f),
                new Vector3(-0.34f, 0.25f, 0.28f), Vector3.zero, mats.PlasticDark, ClioLayout.CockpitLayer);

            Vent(root, new Vector3(-0.08f, 0.90f, 0.76f), mats);
            Vent(root, new Vector3(0.44f, 0.90f, 0.78f), mats);
        }

        static List<Vector2> SmoothProfile(Vector2[] keys)
        {
            var pad = new List<Vector2>();
            for (int i = 0; i < keys.Length - 1; i++)
            {
                for (int step = 0; step < 3; step++)
                {
                    float t = step / 3f;
                    float eased = t * t * (3f - 2f * t);
                    pad.Add(Vector2.Lerp(keys[i], keys[i + 1], eased));
                }
            }
            pad.Add(keys[keys.Length - 1]);
            return pad;
        }

        static void Vent(Transform root, Vector3 center, CabinMaterials mats)
        {
            Box("Rejilla", root, new Vector3(0.16f, 0.07f, 0.02f), center, Vector3.zero,
                mats.PlasticDark, ClioLayout.CockpitLayer);
            for (int i = 0; i < 3; i++)
            {
                Box("Lama", root, new Vector3(0.14f, 0.008f, 0.012f),
                    center + new Vector3(0f, -0.02f + i * 0.02f, -0.008f),
                    new Vector3(-18f, 0f, 0f), mats.Trim, ClioLayout.CockpitLayer);
            }
        }

        static void DoorsPillarsAndGlass(Transform root, CabinMaterials mats)
        {
            Span("Pilar A izquierdo", root, new Vector3(-0.60f, 0.84f, 0.96f),
                new Vector3(-0.42f, 1.32f, 0.20f), 0.075f, 0.09f, mats.Plastic);
            Span("Pilar A derecho", root, new Vector3(0.66f, 0.84f, 1.00f),
                new Vector3(0.50f, 1.32f, 0.24f), 0.075f, 0.09f, mats.Plastic);
            Span("Marco superior", root, new Vector3(-0.42f, 1.32f, 0.20f),
                new Vector3(0.50f, 1.32f, 0.24f), 0.08f, 0.07f, mats.PlasticDark);
            Box("Techo", root, new Vector3(1.15f, 0.04f, 0.85f),
                new Vector3(0.02f, 1.38f, -0.15f), Vector3.zero, mats.Fabric, ClioLayout.CockpitLayer);
            Box("Parasol conductor", root, new Vector3(0.34f, 0.012f, 0.16f),
                new Vector3(-0.34f, 1.30f, 0.28f), new Vector3(78f, 8f, 0f), mats.Fabric, ClioLayout.CockpitLayer);

            Door(root, -1f, mats);
            Door(root, 1f, mats);

            // El hueco entre pilares es el parabrisas: no hay luna opaca.
            Span("Limpiaparabrisas izquierdo", root, new Vector3(-0.46f, 0.84f, 0.98f),
                new Vector3(-0.08f, 0.90f, 1.02f), 0.012f, 0.02f, mats.PlasticDark);
            Span("Limpiaparabrisas derecho", root, new Vector3(0.10f, 0.84f, 1.00f),
                new Vector3(0.42f, 0.89f, 1.04f), 0.012f, 0.02f, mats.PlasticDark);
        }

        static void Door(Transform root, float side, CabinMaterials mats)
        {
            float x = side < 0f ? -0.73f : 0.76f;
            float yaw = side * -8f;
            Box("Panel de puerta", root, new Vector3(0.06f, 0.42f, 0.78f),
                new Vector3(x, 0.62f, 0.12f), new Vector3(0f, yaw, 0f),
                mats.Plastic, ClioLayout.CockpitLayer);
            float inset = side * -0.045f;
            Box("Apoyabrazos", root, new Vector3(0.045f, 0.03f, 0.30f),
                new Vector3(x + inset, 0.64f, 0.16f), new Vector3(0f, yaw, 0f),
                mats.PlasticSoft, ClioLayout.CockpitLayer);
            Box("Tirador", root, new Vector3(0.018f, 0.018f, 0.12f),
                new Vector3(x + inset, 0.70f, 0.22f), new Vector3(0f, yaw, 0f),
                mats.Trim, ClioLayout.CockpitLayer);
            Box("Tejido de puerta", root, new Vector3(0.012f, 0.16f, 0.34f),
                new Vector3(x + inset * 0.6f, 0.66f, -0.02f), new Vector3(0f, yaw, 0f),
                mats.Fabric, ClioLayout.CockpitLayer);
            Span(side < 0f ? "Cintura de puerta izquierda" : "Cintura de puerta derecha", root,
                new Vector3(x, 0.86f, 0.55f), new Vector3(x * 0.92f, 0.98f, -0.15f),
                0.03f, 0.025f, mats.PlasticDark);
        }

        static void CabinLight(Transform root)
        {
            var lamp = new GameObject("Luz de techo").AddComponent<Light>();
            lamp.transform.SetParent(root, false);
            lamp.transform.localPosition = new Vector3(-0.08f, 1.20f, 0.12f);
            lamp.type = LightType.Point;
            lamp.range = 2.6f;
            lamp.intensity = 2.6f;
            lamp.color = new Color(1f, 0.95f, 0.88f);
            lamp.shadows = LightShadows.None;
        }

        static void DriverSeat(Transform root, CabinMaterials mats)
        {
            var seat = new GameObject("Asiento del conductor").transform;
            seat.SetParent(root, false);
            seat.localPosition = new Vector3(-0.36f, 0f, -0.02f);
            Box("Banqueta", seat, new Vector3(0.46f, 0.10f, 0.48f),
                new Vector3(0f, 0.46f, 0.02f), new Vector3(-8f, 0f, 0f), mats.Fabric, ClioLayout.CockpitLayer);
            Box("Respaldo", seat, new Vector3(0.46f, 0.55f, 0.10f),
                new Vector3(0f, 0.78f, -0.20f), new Vector3(-18f, 0f, 0f), mats.Fabric, ClioLayout.CockpitLayer);
            Box("Reposacabezas", seat, new Vector3(0.22f, 0.16f, 0.08f),
                new Vector3(0f, 1.12f, -0.16f), new Vector3(-12f, 0f, 0f), mats.Fabric, ClioLayout.CockpitLayer);
            Box("Regazo", seat, new Vector3(0.42f, 0.08f, 0.28f),
                new Vector3(0f, 0.58f, 0.16f), new Vector3(-24f, 0f, 0f), mats.Jeans, ClioLayout.CockpitLayer);
        }

        static Transform SteeringWheel(ClioCabin cabin, CabinResources bin, CabinMaterials mats)
        {
            var wheel = new GameObject("Volante").transform;
            wheel.SetParent(cabin.Root, false);
            wheel.localPosition = ClioLayout.Wheel;
            wheel.localRotation = Quaternion.Euler(ClioLayout.WheelTilt, 0f, 0f);

            MeshPart("Aro", ProceduralMeshes.Rim(ClioLayout.WheelRadius, 0.034f, 0.024f, 72, 14),
                wheel, Vector3.zero, Quaternion.identity, mats.Rubber, ClioLayout.CockpitLayer, bin);
            MeshPart("Aro interior", ProceduralMeshes.Torus(ClioLayout.WheelRadius - 0.028f, 0.0045f, 10, 36),
                wheel, new Vector3(0f, 0f, -0.004f), Quaternion.identity, mats.Trim, ClioLayout.CockpitLayer, bin);

            Spoke(wheel, 188f, mats);
            Spoke(wheel, -8f, mats);
            Spoke(wheel, -90f, mats);
            Buttons(wheel, 168f, mats);
            Buttons(wheel, 12f, mats);

            // El eje local -Z del aro mira al conductor. El centro y la marca salen hacia él.
            Cylinder("Centro del volante", wheel, 0.040f, 0.032f,
                new Vector3(0f, -0.012f, -0.02f), new Vector3(-90f, 0f, 0f), mats.Plastic, ClioLayout.CockpitLayer);
            WheelMark(wheel);

            cabin.LeftGrip = Grip(wheel, 185f, "Empuñadura izquierda");
            cabin.RightGrip = Grip(wheel, -5f, "Empuñadura derecha");
            cabin.LeftWrist = Wrist(cabin.LeftGrip);
            cabin.RightWrist = Wrist(cabin.RightGrip);
            return wheel;
        }

        static void Spoke(Transform wheel, float degrees, CabinMaterials mats)
        {
            float rad = degrees * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Box("Radio", wheel, new Vector3(0.11f, 0.038f, 0.02f),
                dir * 0.09f + new Vector3(0f, 0f, -0.01f),
                new Vector3(0f, 0f, degrees), mats.Plastic, ClioLayout.CockpitLayer);
            Box("Inserto", wheel, new Vector3(0.07f, 0.012f, 0.008f),
                dir * 0.1f + new Vector3(0f, 0f, -0.024f),
                new Vector3(0f, 0f, degrees), mats.Trim, ClioLayout.CockpitLayer);
        }

        static void Buttons(Transform wheel, float degrees, CabinMaterials mats)
        {
            float rad = degrees * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            for (int i = 0; i < 3; i++)
            {
                Box("Tecla", wheel, new Vector3(0.022f, 0.012f, 0.008f),
                    dir * 0.11f + new Vector3(0f, (i - 1) * 0.018f, -0.03f),
                    new Vector3(0f, 0f, degrees), mats.PlasticSoft, ClioLayout.CockpitLayer);
            }
        }

        static void WheelMark(Transform wheel)
        {
            var mark = new GameObject("Marca Spt");
            mark.transform.SetParent(wheel, false);
            mark.transform.localPosition = new Vector3(0f, -0.012f, -0.046f);
            mark.transform.localRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            mark.layer = ClioLayout.CockpitLayer;

            var canvas = mark.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = mark.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 64f);
            rect.localScale = Vector3.one * (0.05f / 160f);

            var textObject = new GameObject("Spt");
            textObject.transform.SetParent(mark.transform, false);
            textObject.layer = ClioLayout.CockpitLayer;
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textObject.AddComponent<Text>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.font = font;
            text.text = "Spt";
            text.fontSize = 46;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.90f, 0.92f, 0.93f);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
        }

        static Transform Grip(Transform wheel, float degrees, string name)
        {
            float rad = degrees * Mathf.Deg2Rad;
            var grip = new GameObject(name).transform;
            grip.SetParent(wheel, false);
            grip.localPosition = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * ClioLayout.WheelRadius;
            grip.localRotation = Quaternion.Euler(0f, 0f, degrees);
            return grip;
        }

        static Transform Wrist(Transform grip)
        {
            var wrist = new GameObject("Muñeca").transform;
            wrist.SetParent(grip, false);
            // Hacia el conductor (el eje -Z del volante, antes de la inclinación del aro).
            wrist.localPosition = new Vector3(0f, -0.01f, -0.06f);
            return wrist;
        }

        static void CenterStack(ClioCabin cabin, Camera eyes, CabinMaterials mats)
        {
            Box("Torre central", cabin.Root, new Vector3(0.32f, 0.20f, 0.07f),
                new Vector3(0.20f, 0.90f, 0.80f), new Vector3(12f, -6f, 0f),
                mats.Plastic, ClioLayout.CockpitLayer);
            Box("Consola", cabin.Root, new Vector3(0.34f, 0.16f, 0.55f),
                new Vector3(0.16f, 0.50f, 0.38f), new Vector3(-18f, 0f, 0f),
                mats.PlasticDark, ClioLayout.CockpitLayer);
            Box("Apoyabrazos de consola", cabin.Root, new Vector3(0.22f, 0.06f, 0.28f),
                new Vector3(0.16f, 0.58f, 0.12f), new Vector3(-12f, 0f, 0f),
                mats.PlasticSoft, ClioLayout.CockpitLayer);

            for (int i = 0; i < 3; i++)
            {
                Vector3 at = new Vector3(0.08f + i * 0.12f, 0.74f, 0.62f);
                Cylinder("Mando de clima", cabin.Root, 0.028f, 0.016f,
                    at, new Vector3(78f, 0f, 0f), mats.PlasticSoft, ClioLayout.CockpitLayer);
                Box("Índice", cabin.Root, new Vector3(0.006f, 0.016f, 0.006f),
                    at + new Vector3(0f, 0.012f, -0.012f), Vector3.zero, mats.Trim, ClioLayout.CockpitLayer);
            }

            cabin.QuestionMount = Mount("Pantalla central", ClioLayout.QuestionScreen, eyes);
            Frame(cabin.QuestionMount, ClioLayout.QuestionSize, mats.PlasticDark);
        }

        static void ClusterBinnacle(ClioCabin cabin, Camera eyes, CabinMaterials mats)
        {
            Box("Visera del cuadro", cabin.Root, new Vector3(0.27f, 0.016f, 0.05f),
                new Vector3(ClioLayout.Cluster.x, ClioLayout.Cluster.y + 0.075f, ClioLayout.Cluster.z + 0.04f),
                new Vector3(16f, 0f, 0f), mats.PlasticDark, ClioLayout.CockpitLayer);
            Span("Columna de dirección", cabin.Root, new Vector3(-0.36f, 0.72f, 0.62f),
                ClioLayout.Wheel + new Vector3(0f, 0f, -0.04f), 0.07f, 0.07f, mats.PlasticDark);

            cabin.ClusterMount = Mount("Cuadro digital", ClioLayout.Cluster, eyes);
            Frame(cabin.ClusterMount, ClioLayout.ClusterSize, mats.PlasticDark);
        }

        static void Shifter(ClioCabin cabin, CabinResources bin, CabinMaterials mats)
        {
            Box("Base de la palanca", cabin.Root, new Vector3(0.12f, 0.015f, 0.16f),
                ClioLayout.ShifterPivot + new Vector3(0f, 0.01f, 0f), Vector3.zero,
                mats.PlasticDark, ClioLayout.CockpitLayer);

            cabin.GearPivot = new GameObject("Palanca de cambios").transform;
            cabin.GearPivot.SetParent(cabin.Root, false);
            cabin.GearPivot.localPosition = ClioLayout.ShifterPivot;

            MeshPart("Fuelle", ProceduralMeshes.Frustum(0.05f, 0.018f, 0.11f, 14),
                cabin.GearPivot, new Vector3(0f, 0.015f, 0f), Quaternion.identity,
                mats.Plastic, ClioLayout.CockpitLayer, bin);
            Cylinder("Varilla", cabin.GearPivot, 0.011f, ClioLayout.ShifterLength,
                new Vector3(0f, 0.02f + ClioLayout.ShifterLength * 0.5f, 0f),
                Vector3.zero, mats.Trim, ClioLayout.CockpitLayer);
            float tip = 0.02f + ClioLayout.ShifterLength;
            Sphere("Pomo", cabin.GearPivot, new Vector3(0.052f, 0.04f, 0.048f),
                new Vector3(0f, tip, 0f), mats.PlasticDark, ClioLayout.CockpitLayer);
            Box("Tapa del pomo", cabin.GearPivot, new Vector3(0.036f, 0.006f, 0.03f),
                new Vector3(0f, tip + 0.02f, 0f), Vector3.zero, mats.PlasticSoft, ClioLayout.CockpitLayer);
        }

        static void Exterior(Transform car, CabinMaterials mats)
        {
            Box("Capó", car, new Vector3(1.55f, 0.08f, 0.95f),
                new Vector3(0f, 0.80f, 1.55f), new Vector3(-6f, 0f, 0f), mats.Body, 0);
            Box("Paragolpes", car, new Vector3(1.62f, 0.28f, 0.12f),
                new Vector3(0f, 0.48f, 2.05f), Vector3.zero, mats.Body, 0);
            Box("Techo exterior", car, new Vector3(1.35f, 0.06f, 1.7f),
                new Vector3(0f, 1.42f, -0.35f), Vector3.zero, mats.Body, 0);
            Box("Puerta exterior izquierda", car, new Vector3(0.06f, 0.55f, 1.15f),
                new Vector3(-0.86f, 0.72f, 0.15f), Vector3.zero, mats.Body, 0);
            Box("Puerta exterior derecha", car, new Vector3(0.06f, 0.55f, 1.15f),
                new Vector3(0.86f, 0.72f, 0.15f), Vector3.zero, mats.Body, 0);
            Span("Pilar C izquierdo", car, new Vector3(-0.62f, 0.7f, -0.35f),
                new Vector3(-0.48f, 1.38f, -1.15f), 0.08f, 0.08f, mats.Body, 0);
            Span("Pilar C derecho", car, new Vector3(0.62f, 0.7f, -0.35f),
                new Vector3(0.48f, 1.38f, -1.15f), 0.08f, 0.08f, mats.Body, 0);
            // Marco del portón, con hueco: el retrovisor interior ve la vía detrás.
            Box("Marco superior del portón", car, new Vector3(1.2f, 0.06f, 0.05f),
                new Vector3(0f, 1.38f, -1.22f), Vector3.zero, mats.Body, 0);
            Box("Marco inferior del portón", car, new Vector3(1.2f, 0.12f, 0.05f),
                new Vector3(0f, 0.62f, -1.22f), Vector3.zero, mats.Body, 0);
            Box("Marco izquierdo del portón", car, new Vector3(0.08f, 0.72f, 0.05f),
                new Vector3(-0.56f, 1.0f, -1.22f), Vector3.zero, mats.Body, 0);
            Box("Marco derecho del portón", car, new Vector3(0.08f, 0.72f, 0.05f),
                new Vector3(0.56f, 1.0f, -1.22f), Vector3.zero, mats.Body, 0);
        }

        static void MirrorFrames(ClioCabin cabin, Camera eyes, CabinMaterials mats)
        {
            cabin.LeftMirrorGlass = MirrorHead("Retrovisor izquierdo", cabin.Root, eyes,
                ClioLayout.LeftMirror, ClioLayout.LeftMirrorSize, new Vector3(-0.04f, 0f, 0.02f), mats);
            cabin.RightMirrorGlass = MirrorHead("Retrovisor derecho", cabin.Root, eyes,
                ClioLayout.RightMirror, ClioLayout.RightMirrorSize, new Vector3(0.035f, 0f, 0.02f), mats);
            cabin.CenterMirrorGlass = MirrorHead("Retrovisor interior", cabin.Root, eyes,
                ClioLayout.CenterMirror, ClioLayout.CenterMirrorSize, new Vector3(0f, 0.025f, 0f), mats);
            Span("Soporte del retrovisor", cabin.Root, ClioLayout.CenterMirror + new Vector3(0f, 0.04f, 0f),
                ClioLayout.CenterMirror + new Vector3(0f, 0.12f, 0.02f), 0.015f, 0.015f, mats.PlasticDark);
        }

        static Transform MirrorHead(string name, Transform root, Camera eyes, Vector3 position, Vector2 size, Vector3 housingShift, CabinMaterials mats)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(root, false);
            pivot.localPosition = position;
            ClioLayout.FaceDriver(pivot, eyes.transform.position);

            Box("Carcasa", pivot, new Vector3(size.x + 0.03f, size.y + 0.025f, 0.03f),
                housingShift + new Vector3(0f, 0f, -0.02f), Vector3.zero, mats.PlasticDark, ClioLayout.CockpitLayer);
            var glass = Box("Cristal", pivot, new Vector3(size.x, size.y, 0.004f),
                new Vector3(0f, 0f, 0.004f), Vector3.zero, mats.Screen, ClioLayout.CockpitLayer);
            return glass;
        }

        static Transform Mount(string name, Vector3 localPosition, Camera eyes)
        {
            var mount = new GameObject(name).transform;
            mount.SetParent(eyes.transform.parent, false);
            mount.localPosition = localPosition;
            ClioLayout.FaceDriver(mount, eyes.transform.position);
            return mount;
        }

        static void Frame(Transform mount, Vector2 size, Material material)
        {
            float t = 0.012f;
            float z = -0.006f;
            Box("Marco superior", mount, new Vector3(size.x + t * 2f, t, 0.008f),
                new Vector3(0f, size.y * 0.5f + t * 0.5f, z), Vector3.zero, material, ClioLayout.CockpitLayer);
            Box("Marco inferior", mount, new Vector3(size.x + t * 2f, t, 0.008f),
                new Vector3(0f, -size.y * 0.5f - t * 0.5f, z), Vector3.zero, material, ClioLayout.CockpitLayer);
            Box("Marco izquierdo", mount, new Vector3(t, size.y, 0.008f),
                new Vector3(-size.x * 0.5f - t * 0.5f, 0f, z), Vector3.zero, material, ClioLayout.CockpitLayer);
            Box("Marco derecho", mount, new Vector3(t, size.y, 0.008f),
                new Vector3(size.x * 0.5f + t * 0.5f, 0f, z), Vector3.zero, material, ClioLayout.CockpitLayer);
        }

        static Transform MeshPart(string name, Mesh mesh, Transform parent, Vector3 position, Quaternion rotation, Material material, int layer, CabinResources bin)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = bin.Track(mesh);
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            go.layer = layer;
            return go.transform;
        }

        static Transform Box(string name, Transform parent, Vector3 size, Vector3 position, Vector3 euler, Material material, int layer)
        {
            return Primitive(name, PrimitiveType.Cube, parent, size, position, Quaternion.Euler(euler), material, layer);
        }

        static Transform Sphere(string name, Transform parent, Vector3 size, Vector3 position, Material material, int layer)
        {
            return Primitive(name, PrimitiveType.Sphere, parent, size, position, Quaternion.identity, material, layer);
        }

        static Transform Cylinder(string name, Transform parent, float radius, float length, Vector3 position, Vector3 euler, Material material, int layer)
        {
            return Primitive(name, PrimitiveType.Cylinder, parent,
                new Vector3(radius * 2f, length * 0.5f, radius * 2f),
                position, Quaternion.Euler(euler), material, layer);
        }

        public static Transform Span(string name, Transform parent, Vector3 from, Vector3 to, float width, float depth, Material material, int layer = ClioLayout.CockpitLayer)
        {
            Vector3 delta = to - from;
            return Primitive(name, PrimitiveType.Cube, parent,
                new Vector3(width, delta.magnitude, depth),
                (from + to) * 0.5f,
                Quaternion.FromToRotation(Vector3.up, delta.normalized),
                material, layer);
        }

        static Transform Primitive(string name, PrimitiveType type, Transform parent, Vector3 size, Vector3 position, Quaternion rotation, Material material, int layer)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = size;
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            go.layer = layer;
            return go.transform;
        }
    }
}
