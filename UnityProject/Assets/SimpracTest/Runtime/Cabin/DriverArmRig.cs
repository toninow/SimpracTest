using UnityEngine;
using UnityEngine.Rendering;

namespace SimpracTest
{
    // Dos huesos de longitud fija y una mano por muñeca. La derecha sale del
    // aro, agarra el pomo y vuelve; la izquierda no suelta el volante.
    // No hay embrague: la secuencia solo acompaña el cambio de marcha visual.
    public sealed class DriverArmRig : MonoBehaviour
    {
        public float SteerInput;

        enum Phase { Wheel, Depart, Travel, Hold, Return }

        Transform car;
        Transform wheel;
        Transform leftGrip;
        Transform rightGrip;
        Transform leftTarget;
        Transform rightTarget;
        GearLever lever;
        Chain left;
        Chain right;
        Hand leftHand;
        Hand rightHand;
        Phase phase = Phase.Wheel;
        float clock;
        float visualSteer;
        Vector3 fromLocal;
        Quaternion rotLocal;

        class Chain
        {
            public Transform Shoulder;
            public Transform Elbow;
            public Transform Wrist;
            public Vector3 LastBend;
        }

        class Hand
        {
            public Transform Root;
            public readonly Digit[] Digits = new Digit[5];
        }

        class Digit
        {
            public readonly Transform[] Segs = new Transform[3];
            public float Along;
            public bool Thumb;
        }

        public void Bind(Transform vehicle, ClioCabin cabin, CabinMaterials mats)
        {
            car = vehicle;
            wheel = cabin.Wheel;
            leftGrip = cabin.LeftGrip;
            rightGrip = cabin.RightGrip;
            leftTarget = cabin.LeftWrist;
            rightTarget = cabin.RightWrist;
            left = CreateChain("Brazo izquierdo", ClioLayout.LeftShoulder, mats);
            right = CreateChain("Brazo derecho", ClioLayout.RightShoulder, mats);
            leftHand = CreateHand(left.Wrist, mats, true);
            rightHand = CreateHand(right.Wrist, mats, false);
        }

        public void Connect(GearLever shift)
        {
            lever = shift;
        }

        void LateUpdate()
        {
            if (wheel == null || car == null) return;
            float target = -SteerInput * ClioLayout.SteerVisualDegrees;
            float blend = 1f - Mathf.Exp(-8f * Time.deltaTime);
            visualSteer = Mathf.LerpAngle(visualSteer, target, blend);
            wheel.localRotation = Quaternion.Euler(ClioLayout.WheelTilt, 0f, 0f)
                * Quaternion.AngleAxis(visualSteer, Vector3.forward);

            if (lever != null && phase == Phase.Wheel && lever.Gear != lever.Shown)
                Enter(Phase.Depart);

            Solve(left, leftTarget.position, leftGrip.rotation, ClioLayout.LeftPole);
            Pose(leftHand, 0.9f);

            Vector3 goal;
            Quaternion wrist;
            float curl;
            AdvanceRight(out goal, out wrist, out curl);
            Solve(right, goal, wrist, ClioLayout.RightPole);
            Pose(rightHand, curl);

            if (lever == null) return;
            lever.HandHolding = phase == Phase.Hold;
            lever.Pose(Time.deltaTime);
            if (phase == Phase.Hold && lever.Settled && clock > 0.22f) Enter(Phase.Return);
        }

        void Enter(Phase next)
        {
            clock = 0f;
            phase = next;
            fromLocal = car.InverseTransformPoint(right.Wrist.position);
            rotLocal = Quaternion.Inverse(car.rotation) * right.Wrist.rotation;
        }

        void AdvanceRight(out Vector3 goal, out Quaternion wrist, out float curl)
        {
            clock += Time.deltaTime;
            Vector3 grip = rightTarget.position;
            Quaternion gripRot = rightGrip.rotation;
            if (phase == Phase.Wheel || lever == null)
            {
                goal = grip;
                wrist = gripRot;
                curl = 0.9f;
                return;
            }

            Vector3 origin = car.TransformPoint(fromLocal);
            Quaternion originRot = car.rotation * rotLocal;
            if (phase == Phase.Depart)
            {
                float u = Ease(clock / 0.28f);
                goal = Vector3.Lerp(origin, Lift(), u);
                wrist = Quaternion.Slerp(originRot, PalmToward(goal, KnobPoint()), u);
                curl = Mathf.Lerp(0.9f, 0.28f, u);
                if (clock >= 0.28f) Enter(Phase.Travel);
                return;
            }

            if (phase == Phase.Travel)
            {
                float u = Ease(clock / 0.42f);
                goal = Vector3.Lerp(origin, KnobGrab(), u);
                wrist = Quaternion.Slerp(originRot, PalmToward(goal, KnobPoint()), u);
                curl = Mathf.Lerp(0.28f, 0.95f, u);
                if (clock >= 0.42f) Enter(Phase.Hold);
                return;
            }

            if (phase == Phase.Hold)
            {
                goal = KnobGrab();
                wrist = PalmToward(goal, KnobPoint());
                curl = 0.95f;
                return;
            }

            float back = Ease(clock / 0.5f);
            goal = Vector3.Lerp(origin, grip, back);
            wrist = Quaternion.Slerp(originRot, gripRot, back);
            curl = Mathf.Lerp(0.95f, 0.9f, back);
            if (clock >= 0.5f) phase = Phase.Wheel;
        }

        Vector3 Lift()
        {
            return rightTarget.position + car.up * 0.09f + car.right * 0.03f;
        }

        Vector3 KnobPoint()
        {
            if (lever == null || lever.Pivot == null) return rightTarget.position;
            return lever.Pivot.TransformPoint(new Vector3(0f, 0.02f + ClioLayout.ShifterLength, 0f));
        }

        Vector3 KnobGrab()
        {
            return KnobPoint() + car.TransformVector(new Vector3(-0.035f, 0.015f, -0.045f));
        }

        Quaternion PalmToward(Vector3 hand, Vector3 focus)
        {
            Vector3 away = hand - focus;
            if (away.sqrMagnitude < 1e-6f) away = -car.forward;
            return Quaternion.LookRotation(away.normalized, car.up);
        }

        static float Ease(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        void Solve(Chain chain, Vector3 goal, Quaternion handWorld, Vector3 poleLocal)
        {
            Vector3 root = chain.Shoulder.position;
            float upper = ClioLayout.UpperArm;
            float fore = ClioLayout.ForeArm;
            Vector3 span = goal - root;
            float dist = span.magnitude;
            float max = upper + fore - 0.004f;
            float min = Mathf.Abs(upper - fore) + 0.004f;
            if (dist < 1e-4f) span = car.forward * min;
            dist = Mathf.Clamp(span.magnitude, min, max);
            Vector3 axis = span.normalized;
            goal = root + axis * dist;

            float along = (upper * upper - fore * fore + dist * dist) / (2f * dist);
            float height = Mathf.Sqrt(Mathf.Max(0f, upper * upper - along * along));
            Vector3 pole = root + car.TransformVector(poleLocal);
            Vector3 bend = Vector3.ProjectOnPlane(pole - root, axis);
            if (bend.sqrMagnitude < 1e-6f) bend = chain.LastBend.sqrMagnitude > 1e-6f
                ? chain.LastBend
                : Vector3.Cross(axis, Vector3.up);
            bend.Normalize();
            chain.LastBend = bend;
            Vector3 elbowPos = root + axis * along + bend * height;

            chain.Shoulder.rotation = Quaternion.FromToRotation(Vector3.up, (elbowPos - root).normalized);
            chain.Elbow.rotation = Quaternion.FromToRotation(Vector3.up, (goal - elbowPos).normalized);
            chain.Wrist.rotation = handWorld;
        }

        Chain CreateChain(string name, Vector3 shoulderLocal, CabinMaterials mats)
        {
            var chain = new Chain();
            var shoulder = new GameObject(name).transform;
            shoulder.SetParent(car, false);
            shoulder.localPosition = shoulderLocal;
            chain.Shoulder = shoulder;
            Limb("Brazo", shoulder, ClioLayout.UpperArm, 0.085f, mats.Sleeve);
            var elbow = new GameObject("Codo").transform;
            elbow.SetParent(shoulder, false);
            elbow.localPosition = new Vector3(0f, ClioLayout.UpperArm, 0f);
            chain.Elbow = elbow;
            Limb("Antebrazo", elbow, ClioLayout.ForeArm, 0.068f, mats.Sleeve);
            var wrist = new GameObject("Muñeca").transform;
            wrist.SetParent(elbow, false);
            wrist.localPosition = new Vector3(0f, ClioLayout.ForeArm, 0f);
            chain.Wrist = wrist;
            return chain;
        }

        Hand CreateHand(Transform wrist, CabinMaterials mats, bool watch)
        {
            var hand = new Hand();
            var root = new GameObject("Mano").transform;
            root.SetParent(wrist, false);
            hand.Root = root;
            Limb("Palma", root, 0.078f, 0.034f, mats.Skin);
            root.GetChild(0).localPosition = new Vector3(0f, 0f, 0.02f);
            root.GetChild(0).localRotation = Quaternion.identity;
            root.GetChild(0).localScale = new Vector3(0.042f, 0.05f, 0.022f);

            for (int i = 0; i < 4; i++)
            {
                var digit = new Digit { Along = -0.028f + i * 0.018f };
                for (int s = 0; s < 3; s++) digit.Segs[s] = Capsule("Dedo", root, mats.Skin);
                hand.Digits[i] = digit;
            }
            var thumb = new Digit { Along = -0.04f, Thumb = true };
            for (int s = 0; s < 3; s++) thumb.Segs[s] = Capsule("Pulgar", root, mats.Skin);
            hand.Digits[4] = thumb;

            if (watch)
            {
                var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
                band.name = "Reloj";
                band.transform.SetParent(wrist, false);
                band.transform.localPosition = new Vector3(0f, -0.02f, 0f);
                band.transform.localScale = new Vector3(0.046f, 0.012f, 0.02f);
                Finish(band, mats.Trim);
            }
            return hand;
        }

        void Pose(Hand hand, float curl)
        {
            if (hand == null) return;
            for (int i = 0; i < hand.Digits.Length; i++)
            {
                Digit digit = hand.Digits[i];
                Vector3[] points = digit.Thumb ? ThumbPoints(curl) : FingerPoints(digit.Along, curl);
                float radius = digit.Thumb ? 0.011f : 0.0085f;
                for (int s = 0; s < 3; s++) Fit(digit.Segs[s], points[s], points[s + 1], radius);
            }
        }

        static Vector3[] FingerPoints(float along, float curl)
        {
            var tube = new Vector3(0f, along, 0.05f);
            Vector3 a = tube + new Vector3(-0.002f, 0f, -0.026f);
            Vector3 b = tube + Vector3.Lerp(new Vector3(0.004f, 0f, -0.038f), new Vector3(0.02f, 0f, -0.006f), curl);
            Vector3 c = tube + Vector3.Lerp(new Vector3(0.008f, 0f, -0.048f), new Vector3(0.028f, 0f, 0.012f), curl);
            Vector3 d = tube + Vector3.Lerp(new Vector3(0.004f, 0f, -0.054f), new Vector3(0.012f, 0f, 0.03f), curl);
            return new[] { a, b, c, d };
        }

        static Vector3[] ThumbPoints(float curl)
        {
            Vector3 a = new Vector3(-0.02f, -0.03f, 0.01f);
            Vector3 b = Vector3.Lerp(new Vector3(-0.04f, -0.02f, -0.01f), new Vector3(-0.01f, -0.01f, 0.03f), curl);
            Vector3 c = Vector3.Lerp(new Vector3(-0.05f, 0.01f, -0.02f), new Vector3(0.01f, 0.01f, 0.045f), curl);
            Vector3 d = Vector3.Lerp(new Vector3(-0.045f, 0.03f, -0.03f), new Vector3(0.02f, 0.02f, 0.055f), curl);
            return new[] { a, b, c, d };
        }

        static void Fit(Transform seg, Vector3 from, Vector3 to, float radius)
        {
            Vector3 delta = to - from;
            float length = Mathf.Max(0.008f, delta.magnitude);
            seg.localPosition = (from + to) * 0.5f;
            seg.localRotation = Quaternion.FromToRotation(Vector3.up, delta / length);
            seg.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
        }

        static void Limb(string name, Transform parent, float length, float diameter, Material material)
        {
            var seg = Capsule(name, parent, material);
            seg.localPosition = new Vector3(0f, length * 0.5f, 0f);
            seg.localScale = new Vector3(diameter, length * 0.5f, diameter);
        }

        static Transform Capsule(string name, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent, false);
            Finish(go, material);
            return go.transform;
        }

        static void Finish(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            go.layer = ClioLayout.CockpitLayer;
        }
    }
}
