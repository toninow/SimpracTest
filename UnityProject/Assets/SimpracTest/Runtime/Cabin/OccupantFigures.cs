using UnityEngine;
using UnityEngine.Rendering;

namespace SimpracTest
{
    // Figuras de referencia para el asiento del profesor y el asiento trasero.
    // Proporciones de adulto sentado, ropa sencilla y cinturón. No son personas
    // escaneadas: un humanoide con rig las puede reemplazar sin tocar la conducción.
    public static class OccupantFigures
    {
        public static void Build(Transform car, CabinMaterials mats)
        {
            var instructor = Person(car, "Instructor", new Vector3(0.50f, 0f, 0.02f), -8f,
                mats.Navy, mats.Khaki, mats, ClioLayout.CockpitLayer);
            Clipboard(instructor, mats);

            Person(car, "Examinador", new Vector3(0.08f, 0f, -0.84f), 4f,
                mats.ExaminerCloth, mats.ExaminerCloth, mats, ClioLayout.ExaminerLayer);
        }

        static Transform Person(Transform car, string name, Vector3 position, float yaw, Material shirt, Material legs, CabinMaterials mats, int layer)
        {
            var root = new GameObject(name).transform;
            root.SetParent(car, false);
            root.localPosition = position;
            root.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box("Banqueta", root, new Vector3(0.46f, 0.09f, 0.46f), new Vector3(0f, 0.46f, 0.04f),
                new Vector3(-6f, 0f, 0f), mats.Fabric, layer);
            Box("Respaldo", root, new Vector3(0.44f, 0.52f, 0.09f), new Vector3(0f, 0.78f, -0.16f),
                new Vector3(-16f, 0f, 0f), mats.Fabric, layer);
            Box("Reposacabezas", root, new Vector3(0.22f, 0.14f, 0.07f), new Vector3(0f, 1.12f, -0.12f),
                new Vector3(-10f, 0f, 0f), mats.Fabric, layer);

            Capsule("Torso", root, new Vector3(0f, 0.64f, 0.02f), new Vector3(0f, 1.02f, -0.02f), 0.15f, shirt, layer);
            Capsule("Cuello", root, new Vector3(0f, 1.04f, 0f), new Vector3(0f, 1.12f, 0.01f), 0.045f, mats.Skin, layer);
            Sphere("Cabeza", root, new Vector3(0.2f, 0.22f, 0.19f), new Vector3(0f, 1.22f, 0.02f), mats.Skin, layer);
            Sphere("Pelo", root, new Vector3(0.19f, 0.12f, 0.18f), new Vector3(0f, 1.30f, -0.005f), mats.Hair, layer);

            Capsule("Brazo interior", root, new Vector3(-0.16f, 0.98f, 0.02f), new Vector3(-0.2f, 0.74f, 0.18f), 0.045f, shirt, layer);
            Capsule("Antebrazo interior", root, new Vector3(-0.2f, 0.74f, 0.18f), new Vector3(-0.12f, 0.58f, 0.36f), 0.038f, shirt, layer);
            Sphere("Mano interior", root, new Vector3(0.07f, 0.05f, 0.09f), new Vector3(-0.1f, 0.56f, 0.40f), mats.Skin, layer);

            Capsule("Brazo exterior", root, new Vector3(0.16f, 0.98f, 0f), new Vector3(0.24f, 0.76f, 0.1f), 0.045f, shirt, layer);
            Capsule("Antebrazo exterior", root, new Vector3(0.24f, 0.76f, 0.1f), new Vector3(0.22f, 0.64f, 0.28f), 0.038f, shirt, layer);
            Sphere("Mano exterior", root, new Vector3(0.07f, 0.05f, 0.09f), new Vector3(0.2f, 0.62f, 0.32f), mats.Skin, layer);

            Capsule("Muslo interior", root, new Vector3(-0.08f, 0.54f, 0.08f), new Vector3(-0.16f, 0.5f, 0.46f), 0.07f, legs, layer);
            Capsule("Pierna interior", root, new Vector3(-0.16f, 0.5f, 0.46f), new Vector3(-0.16f, 0.32f, 0.58f), 0.05f, legs, layer);
            Capsule("Muslo exterior", root, new Vector3(0.1f, 0.54f, 0.06f), new Vector3(0.16f, 0.5f, 0.38f), 0.07f, legs, layer);
            Capsule("Pierna exterior", root, new Vector3(0.16f, 0.5f, 0.38f), new Vector3(0.18f, 0.32f, 0.5f), 0.05f, legs, layer);

            Capsule("Cinturón", root, new Vector3(0.14f, 1.0f, 0.08f), new Vector3(-0.12f, 0.58f, 0.12f), 0.012f, mats.Belt, layer);
            return root;
        }

        static void Clipboard(Transform person, CabinMaterials mats)
        {
            Box("Carpeta", person, new Vector3(0.16f, 0.22f, 0.008f), new Vector3(0.24f, 0.7f, 0.34f),
                new Vector3(70f, 20f, 8f), mats.PlasticDark, ClioLayout.CockpitLayer);
            Box("Papel", person, new Vector3(0.13f, 0.18f, 0.004f), new Vector3(0.25f, 0.71f, 0.35f),
                new Vector3(70f, 20f, 8f), mats.Paper, ClioLayout.CockpitLayer);
        }

        static Transform Box(string name, Transform parent, Vector3 size, Vector3 position, Vector3 euler, Material material, int layer)
        {
            return Shape(name, PrimitiveType.Cube, parent, size, position, Quaternion.Euler(euler), material, layer);
        }

        static void Sphere(string name, Transform parent, Vector3 size, Vector3 position, Material material, int layer)
        {
            Shape(name, PrimitiveType.Sphere, parent, size, position, Quaternion.identity, material, layer);
        }

        static void Capsule(string name, Transform parent, Vector3 from, Vector3 to, float radius, Material material, int layer)
        {
            Vector3 delta = to - from;
            float length = Mathf.Max(0.02f, delta.magnitude);
            Shape(name, PrimitiveType.Capsule, parent,
                new Vector3(radius * 2f, length * 0.5f, radius * 2f),
                (from + to) * 0.5f,
                Quaternion.FromToRotation(Vector3.up, delta.normalized),
                material, layer);
        }

        static Transform Shape(string name, PrimitiveType type, Transform parent, Vector3 size, Vector3 position, Quaternion rotation, Material material, int layer)
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
