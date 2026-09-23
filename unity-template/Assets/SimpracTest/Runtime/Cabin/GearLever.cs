using UnityEngine;
using UnityEngine.Rendering;

namespace SimpracTest
{
    // La palanca se mueve cuando cambia la marcha del ejercicio.
    // 0 es punto muerto. El esquema del pomo es una rejilla, no un grabado de fábrica.
    public sealed class GearLever : MonoBehaviour
    {
        public int Gear;
        public int Shown { get; private set; }
        public bool HandHolding;
        public Transform Pivot => pivot;

        Transform pivot;

        public bool Settled
        {
            get
            {
                if (pivot == null) return true;
                return Shown == Gear && Quaternion.Angle(pivot.localRotation, Aim(Gear)) < 3f;
            }
        }

        public void Bind(ClioCabin cabin, CabinResources bin, CabinMaterials mats)
        {
            pivot = cabin.GearPivot;
            if (pivot == null) return;

            var plate = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plate.name = "Esquema de marchas";
            plate.transform.SetParent(pivot, false);
            plate.transform.localPosition = new Vector3(0f, 0.02f + ClioLayout.ShifterLength + 0.024f, 0f);
            plate.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            plate.transform.localScale = new Vector3(0.034f, 0.026f, 1f);
            var renderer = plate.GetComponent<Renderer>();
            renderer.sharedMaterial = mats.UnlitTexture(bin, bin.Track(DrawGate()), false);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var collider = plate.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            plate.layer = ClioLayout.CockpitLayer;
        }

        // La palanca no se mueve hasta que la mano derecha ha agarrado el pomo.
        public void Pose(float deltaTime)
        {
            if (pivot == null) return;
            int goal = HandHolding ? Gear : Shown;
            float blend = 1f - Mathf.Exp(-9f * Mathf.Max(0f, deltaTime));
            pivot.localRotation = Quaternion.Slerp(pivot.localRotation, Aim(goal), blend);
            if (HandHolding && Quaternion.Angle(pivot.localRotation, Aim(Gear)) < 3f) Shown = Gear;
        }

        static Quaternion Aim(int gear)
        {
            Vector3 gate = Offset(gear) * 0.042f;
            float reach = Mathf.Max(0.05f, ClioLayout.ShifterLength);
            return Quaternion.Euler(
                Mathf.Atan2(gate.z, reach) * Mathf.Rad2Deg,
                0f,
                -Mathf.Atan2(gate.x, reach) * Mathf.Rad2Deg);
        }

        static Vector3 Offset(int gear)
        {
            switch (gear)
            {
                case 1: return new Vector3(-1f, 0f, 1f);
                case 2: return new Vector3(-1f, 0f, -1f);
                case 3: return new Vector3(0f, 0f, 1f);
                case 4: return new Vector3(0f, 0f, -1f);
                case 5: return new Vector3(1f, 0f, 1f);
                case -1: return new Vector3(1f, 0f, -1f);
                default: return Vector3.zero;
            }
        }

        static Texture2D DrawGate()
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "Rejilla de marchas" };
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[n * n];
            var bg = new Color32(28, 30, 32, 255);
            var ink = new Color32(230, 232, 234, 255);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
            Stamp(pixels, n, 16, 12, 16, 52, ink);
            Stamp(pixels, n, 32, 12, 32, 52, ink);
            Stamp(pixels, n, 48, 12, 48, 52, ink);
            Stamp(pixels, n, 16, 32, 48, 32, ink);
            tex.SetPixels32(pixels);
            tex.Apply(false);
            return tex;
        }

        static void Stamp(Color32[] pixels, int n, int x0, int y0, int x1, int y1, Color32 color)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (int i = 0; i <= steps; i++)
            {
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, steps == 0 ? 0f : i / (float)steps));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, steps == 0 ? 0f : i / (float)steps));
                for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                {
                    int px = x + ox;
                    int py = y + oy;
                    if (px >= 0 && py >= 0 && px < n && py < n) pixels[py * n + px] = color;
                }
            }
        }
    }
}
