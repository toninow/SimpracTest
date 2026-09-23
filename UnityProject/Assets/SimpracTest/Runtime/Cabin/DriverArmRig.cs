using UnityEngine;
using UnityEngine.Rendering;

namespace SimpracTest
{
    // Brazos en dos huesos. Las manos van soldadas a las empuñaduras del aro,
    // así que giran con él y el codo se recoloca para alcanzar la muñeca.
    // La longitud se recalcula para no perder el contacto; un esqueleto con
    // malla skinned y dedos animados lo sustituirá cuando haya un modelo.
    public sealed class DriverArmRig : MonoBehaviour
    {
        public float SteerInput;

        Transform car;
        Transform wheel;
        Transform leftTarget;
        Transform rightTarget;
        Chain left;
        Chain right;
        float visualSteer;

        class Chain
        {
            public Transform Shoulder;
            public Transform Elbow;
            public Transform Wrist;
            public Transform UpperVisual;
            public Transform ForeVisual;
            public Transform WristSkin;
        }

        public void Bind(Transform vehicle, ClioCabin cabin, CabinMaterials mats)
        {
            car = vehicle;
            wheel = cabin.Wheel;
            leftTarget = cabin.LeftWrist;
            rightTarget = cabin.RightWrist;
            left = CreateChain("Brazo izquierdo", ClioLayout.LeftShoulder, mats);
            right = CreateChain("Brazo derecho", ClioLayout.RightShoulder, mats);
            BuildHand(cabin.LeftGrip, -1f, mats, true);
            BuildHand(cabin.RightGrip, 1f, mats, false);
        }

        void LateUpdate()
        {
            if (wheel == null) return;
            float target = -SteerInput * ClioLayout.SteerVisualDegrees;
            float blend = 1f - Mathf.Exp(-8f * Time.deltaTime);
            visualSteer = Mathf.LerpAngle(visualSteer, target, blend);
            wheel.localRotation = Quaternion.Euler(ClioLayout.WheelTilt, 0f, 0f)
                * Quaternion.AngleAxis(visualSteer, Vector3.forward);
            Solve(left, leftTarget, ClioLayout.LeftPole);
            Solve(right, rightTarget, ClioLayout.RightPole);
        }

        void Solve(Chain chain, Transform target, Vector3 poleLocal)
        {
            if (chain == null || target == null) return;
            Vector3 root = chain.Shoulder.position;
            Vector3 goal = target.position;
            Vector3 span = goal - root;
            float dist = span.magnitude;
            if (dist < 0.05f) return;

            float upper = dist * 0.56f;
            float fore = dist * 0.62f;
            Vector3 axis = span / dist;
            float along = (upper * upper - fore * fore + dist * dist) / (2f * dist);
            float height = Mathf.Sqrt(Mathf.Max(0f, upper * upper - along * along));
            Vector3 pole = root + car.TransformVector(poleLocal);
            Vector3 bend = Vector3.ProjectOnPlane(pole - root, axis);
            if (bend.sqrMagnitude < 1e-6f) bend = Vector3.Cross(axis, Vector3.up);
            Vector3 elbowPos = root + axis * along + bend.normalized * height;

            chain.Shoulder.rotation = Quaternion.FromToRotation(Vector3.up, (elbowPos - root).normalized);
            chain.Elbow.localPosition = new Vector3(0f, upper, 0f);
            chain.Elbow.rotation = Quaternion.FromToRotation(Vector3.up, (goal - elbowPos).normalized);
            chain.Wrist.localPosition = new Vector3(0f, fore, 0f);
            FitCapsule(chain.UpperVisual, upper, 0.092f);
            FitCapsule(chain.ForeVisual, fore, 0.072f);
            chain.WristSkin.localPosition = new Vector3(0f, fore * 0.9f, 0f);
        }

        static void FitCapsule(Transform visual, float length, float diameter)
        {
            visual.localPosition = new Vector3(0f, length * 0.5f, 0f);
            visual.localScale = new Vector3(diameter, length * 0.5f, diameter);
        }

        Chain CreateChain(string name, Vector3 shoulderLocal, CabinMaterials mats)
        {
            var chain = new Chain();
            var shoulder = new GameObject(name).transform;
            shoulder.SetParent(car, false);
            shoulder.localPosition = shoulderLocal;
            chain.Shoulder = shoulder;
            chain.UpperVisual = Capsule("Brazo", shoulder, mats.Sleeve);
            var elbow = new GameObject("Codo").transform;
            elbow.SetParent(shoulder, false);
            chain.Elbow = elbow;
            chain.ForeVisual = Capsule("Antebrazo", elbow, mats.Sleeve);
            chain.WristSkin = Capsule("Muñeca visible", elbow, mats.Skin);
            chain.WristSkin.localScale = new Vector3(0.058f, 0.028f, 0.058f);
            var wrist = new GameObject("Final del antebrazo").transform;
            wrist.SetParent(elbow, false);
            chain.Wrist = wrist;
            return chain;
        }

        static Transform Capsule(string name, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            go.layer = ClioLayout.CockpitLayer;
            return go.transform;
        }

        static void BuildHand(Transform grip, float thumbAlongRim, CabinMaterials mats, bool watch)
        {
            // Las piezas quedan fuera del tubo del aro (radio 0,017 m) para no atravesarlo.
            Plate(grip, "Palma", new Vector3(0.032f, 0.078f, 0.018f), new Vector3(-0.004f, 0f, -0.038f), mats.Skin);
            for (int i = 0; i < 4; i++)
            {
                float along = -0.027f + i * 0.018f;
                Plate(grip, "Dedo", new Vector3(0.014f, 0.014f, 0.012f), new Vector3(-0.002f, along, -0.03f), mats.Skin);
                Plate(grip, "Falangeta", new Vector3(0.012f, 0.012f, 0.012f), new Vector3(0.028f, along, 0f), mats.Skin);
                Plate(grip, "Yema", new Vector3(0.012f, 0.011f, 0.012f), new Vector3(0.016f, along, 0.028f), mats.Skin);
            }
            Plate(grip, "Pulgar", new Vector3(0.02f, 0.034f, 0.016f),
                new Vector3(-0.02f, thumbAlongRim * 0.042f, -0.026f), mats.Skin);
            if (!watch) return;
            Plate(grip, "Reloj", new Vector3(0.05f, 0.016f, 0.02f), new Vector3(0f, thumbAlongRim * 0.01f, -0.05f), mats.Trim);
        }

        static void Plate(Transform parent, string name, Vector3 size, Vector3 localPosition, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            go.layer = ClioLayout.CockpitLayer;
        }
    }
}
