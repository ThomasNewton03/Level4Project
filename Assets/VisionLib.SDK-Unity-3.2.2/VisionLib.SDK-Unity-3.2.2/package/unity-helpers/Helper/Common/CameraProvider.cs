using UnityEngine;

namespace Visometry.Helpers
{
    /// <summary>
    /// Helper class to provide a cached reference to Camera.Main. If Camera.Main does not exist
    /// the cached reference is set to the first camera in the scene. This may happen if the
    /// Camera.Main tag is not set on the GameObject.
    /// </summary>
    public static class CameraProvider
    {
        private static Camera mainCamera;
        public static Camera MainCamera
        {
            get
            {
                if (CameraProvider.mainCamera == null)
                {
                    CameraProvider.mainCamera = FindMainCamera();
                }
                return CameraProvider.mainCamera;
            }
        }

        private static Camera FindMainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("No camera with Camera.Main tag has been found in the scene.");
            }
            return camera;
        }
    }
}
