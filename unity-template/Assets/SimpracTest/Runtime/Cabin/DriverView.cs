using UnityEngine;

namespace SimpracTest
{
    public static class DriverView
    {
        public static Camera Create(Transform car)
        {
            var camera = new GameObject("Ojos del conductor").AddComponent<Camera>();
            camera.transform.SetParent(car, false);
            camera.transform.localPosition = ClioLayout.Eye;
            camera.transform.localRotation = Quaternion.Euler(ClioLayout.EyeEuler);
            camera.fieldOfView = ClioLayout.FieldOfView;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 480f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.depth = 2;
            camera.enabled = true;
            camera.gameObject.AddComponent<AudioListener>();
            return camera;
        }
    }
}
