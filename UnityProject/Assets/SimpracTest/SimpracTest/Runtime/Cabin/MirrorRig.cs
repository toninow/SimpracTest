using UnityEngine;

namespace SimpracTest
{
    // Tres cámaras detrás de los cristales. El habitáculo está en otra capa,
    // así que el espejo no se dibuja a sí mismo ni reentra en la textura.
    // Los laterales alternan fotogramas; el central, que puede mostrar al
    // examinador, se actualiza siempre.
    public sealed class MirrorRig : MonoBehaviour
    {
        Camera center;
        Camera left;
        Camera right;

        public void Build(Transform car, ClioCabin cabin, CabinResources bin, CabinMaterials mats)
        {
            int cockpit = 1 << ClioLayout.CockpitLayer;
            int examiner = 1 << ClioLayout.ExaminerLayer;
            int world = ~(cockpit | examiner);

            center = Create(car, "Cámara del retrovisor interior",
                ClioLayout.CenterMirror, new Vector3(0.06f, 1.0f, -2.7f),
                48f, bin.Track(Texture("Retrovisor interior", 512, 192)), world | examiner);
            left = Create(car, "Cámara del retrovisor izquierdo",
                ClioLayout.LeftMirror, ClioLayout.LeftMirror + new Vector3(-1.8f, -0.15f, -7.5f),
                60f, bin.Track(Texture("Retrovisor izquierdo", 256, 160)), world);
            right = Create(car, "Cámara del retrovisor derecho",
                ClioLayout.RightMirror, ClioLayout.RightMirror + new Vector3(1.5f, -0.12f, -7.5f),
                58f, bin.Track(Texture("Retrovisor derecho", 256, 144)), world);

            Assign(cabin.CenterMirrorGlass, center.targetTexture, bin, mats);
            Assign(cabin.LeftMirrorGlass, left.targetTexture, bin, mats);
            Assign(cabin.RightMirrorGlass, right.targetTexture, bin, mats);
        }

        void Update()
        {
            if (left != null) left.enabled = (Time.frameCount & 1) == 0;
            if (right != null) right.enabled = (Time.frameCount & 1) == 1;
        }

        static Camera Create(Transform car, string name, Vector3 localPosition, Vector3 lookAtLocal, float fov, RenderTexture target, int mask)
        {
            var camera = new GameObject(name).AddComponent<Camera>();
            camera.transform.SetParent(car, false);
            camera.transform.localPosition = localPosition;
            Vector3 direction = lookAtLocal - localPosition;
            camera.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            camera.fieldOfView = fov;
            camera.nearClipPlane = 0.06f;
            camera.farClipPlane = 220f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.cullingMask = mask;
            camera.targetTexture = target;
            camera.depth = -2;
            camera.allowMSAA = false;
            camera.useOcclusionCulling = false;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.enabled = true;
            return camera;
        }

        static RenderTexture Texture(string name, int width, int height)
        {
            var texture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            {
                name = name,
                antiAliasing = 1,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.Create();
            return texture;
        }

        static void Assign(Transform glass, RenderTexture texture, CabinResources bin, CabinMaterials mats)
        {
            if (glass == null) return;
            var renderer = glass.GetComponent<Renderer>();
            if (renderer == null) return;
            renderer.sharedMaterial = mats.UnlitTexture(bin, texture, true);
        }

        void OnDestroy()
        {
            Release(center);
            Release(left);
            Release(right);
        }

        static void Release(Camera camera)
        {
            if (camera != null) camera.targetTexture = null;
        }
    }
}
