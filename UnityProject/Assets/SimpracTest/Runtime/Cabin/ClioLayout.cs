using UnityEngine;

namespace SimpracTest
{
    // Cotas de un utilitario de segmento B, volante a la izquierda, tomadas como
    // referencia de encuadre. No identifican una marca. El ajuste fino está
    // concentrado en estas constantes.
    public static class ClioLayout
    {
        public const int CockpitLayer = 8;
        public const int ExaminerLayer = 9;

        // Ojos en el asiento delantero izquierdo, no en el eje del coche.
        // El guiño de 5° hacia la derecha mira al centro del parabrisas.
        public static readonly Vector3 Eye = new Vector3(-0.34f, 1.08f, -0.08f);
        public static readonly Vector3 EyeEuler = new Vector3(4f, 2f, 0f);
        // Unos 58° verticales se acercan al encuadre de la foto, sin el efecto
        // de gran angular que agrandaba la pantalla central.
        public const float FieldOfView = 58f;

        public static readonly Vector3 Wheel = new Vector3(-0.34f, 0.86f, 0.50f);
        public const float WheelTilt = 18f;
        public const float WheelRadius = 0.172f;
        public const float WheelTube = 0.016f;
        public const float SteerVisualDegrees = 78f;

        // Dos esferas en el hueco superior del aro. El centro está en la línea
        // de visión que pasa entre el buje y la parte alta del volante.
        public static readonly Vector3 Cluster = new Vector3(-0.34f, 0.895f, 0.64f);
        public static readonly Vector2 ClusterSize = new Vector2(0.25f, 0.105f);

        // Pantalla de la consola, a la derecha del volante: pregunta a un lado y mapa al otro.
        public static readonly Vector3 QuestionScreen = new Vector3(0.20f, 0.90f, 0.74f);
        public static readonly Vector2 QuestionSize = new Vector2(0.28f, 0.16f);

        public static readonly Vector3 ShifterPivot = new Vector3(0.14f, 0.56f, 0.36f);
        public const float ShifterLength = 0.18f;

        public static readonly Vector3 LeftMirror = new Vector3(-0.76f, 1.01f, 0.50f);
        public static readonly Vector3 CenterMirror = new Vector3(0.02f, 1.27f, 0.40f);
        public static readonly Vector3 RightMirror = new Vector3(0.70f, 1.03f, 0.86f);

        public static readonly Vector2 LeftMirrorSize = new Vector2(0.16f, 0.10f);
        public static readonly Vector2 CenterMirrorSize = new Vector2(0.24f, 0.09f);
        public static readonly Vector2 RightMirrorSize = new Vector2(0.13f, 0.08f);

        // Hombros de un adulto sentado. La suma de los huesos alcanza el aro y el pomo.
        public static readonly Vector3 LeftShoulder = new Vector3(-0.56f, 0.80f, 0f);
        public static readonly Vector3 RightShoulder = new Vector3(-0.12f, 0.80f, 0f);
        public static readonly Vector3 LeftPole = new Vector3(-0.45f, -0.9f, -0.05f);
        public static readonly Vector3 RightPole = new Vector3(0.42f, -0.9f, 0.02f);
        public const float UpperArm = 0.28f;
        public const float ForeArm = 0.26f;

        public static void FaceDriver(Transform subject, Vector3 eyeWorld)
        {
            Vector3 toEye = eyeWorld - subject.position;
            if (toEye.sqrMagnitude < 1e-6f) return;
            subject.rotation = Quaternion.LookRotation(toEye, Vector3.up);
        }
    }
}
