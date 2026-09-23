using UnityEngine;

namespace SimpracTest
{
    // Cotas aproximadas de un utilitario de segmento B, volante a la izquierda.
    // No son medidas de fábrica de un Renault Clio: sirven para componer la
    // vista del conductor. El ajuste fino está concentrado en estas constantes.
    public static class ClioLayout
    {
        public const int CockpitLayer = 8;
        public const int ExaminerLayer = 9;

        // Ojos en el asiento delantero izquierdo, no en el eje del coche.
        // El guiño de 5° hacia la derecha mira al centro del parabrisas.
        public static readonly Vector3 Eye = new Vector3(-0.36f, 1.10f, -0.06f);
        public static readonly Vector3 EyeEuler = new Vector3(8f, 5f, 0f);
        // 76° vertical deja en cuadro, en un monitor 16:9, el volante, la palanca,
        // la pantalla y los tres retrovisores. Es más abierto que la visión útil
        // de un monitor estrecho; está elegido para esta vista única.
        public const float FieldOfView = 76f;

        public static readonly Vector3 Wheel = new Vector3(-0.36f, 0.90f, 0.47f);
        public const float WheelTilt = 20f;
        public const float WheelRadius = 0.185f;
        public const float WheelTube = 0.017f;
        public const float SteerVisualDegrees = 78f;

        public static readonly Vector3 Cluster = new Vector3(-0.36f, 0.975f, 0.66f);
        public static readonly Vector2 ClusterSize = new Vector2(0.27f, 0.145f);

        public static readonly Vector3 QuestionScreen = new Vector3(0.22f, 0.98f, 0.61f);
        public static readonly Vector2 QuestionSize = new Vector2(0.40f, 0.26f);

        public static readonly Vector3 ShifterPivot = new Vector3(0.14f, 0.56f, 0.36f);
        public const float ShifterLength = 0.18f;

        public static readonly Vector3 LeftMirror = new Vector3(-0.76f, 1.01f, 0.50f);
        public static readonly Vector3 CenterMirror = new Vector3(0.02f, 1.27f, 0.40f);
        public static readonly Vector3 RightMirror = new Vector3(0.70f, 1.03f, 0.86f);

        public static readonly Vector2 LeftMirrorSize = new Vector2(0.16f, 0.10f);
        public static readonly Vector2 CenterMirrorSize = new Vector2(0.24f, 0.09f);
        public static readonly Vector2 RightMirrorSize = new Vector2(0.13f, 0.08f);

        // Hombros bajos y abiertos: los antebrazos suben hacia el aro.
        public static readonly Vector3 LeftShoulder = new Vector3(-0.64f, 0.76f, -0.02f);
        public static readonly Vector3 RightShoulder = new Vector3(-0.10f, 0.76f, -0.02f);
        public static readonly Vector3 LeftPole = new Vector3(-0.42f, -0.55f, -0.2f);
        public static readonly Vector3 RightPole = new Vector3(0.42f, -0.55f, -0.2f);

        public static void FaceDriver(Transform subject, Vector3 eyeWorld)
        {
            Vector3 toEye = eyeWorld - subject.position;
            if (toEye.sqrMagnitude < 1e-6f) return;
            subject.rotation = Quaternion.LookRotation(toEye, Vector3.up);
        }
    }
}
