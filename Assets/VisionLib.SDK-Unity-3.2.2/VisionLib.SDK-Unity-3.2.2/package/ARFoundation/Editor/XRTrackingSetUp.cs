#if VL_ARFOUNDATION

using UnityEditor;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARFoundation;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.ARFoundation
{
    /**
     *  @brief Adds menu entries to allow adding VisionLib object tracking to an existing AR
     * Foundation scene.
     *  @ingroup ARFoundation
     */
    internal static class XRTrackingSetUp
    {
        private static ARCameraManager GetCameraManager()
        {
            return GameObject.FindObjectOfType<ARCameraManager>();
        }

        private static Camera GetARCamera()
        {
            Component component = GameObject.FindObjectOfType<ARPoseDriver>();
            if (component == null)
            {
                return null;
            }
            return component.gameObject.GetComponent<Camera>();
        }

        private static GameObject CreateAndConnectVLTracking()
        {
            var vlTracking = new GameObject("VLTracking");
            vlTracking.AddComponentUndoable<GeneralSettings>();
            vlTracking.AddComponentUndoable<TrackingConfiguration>();

            var xrCameraGameObject = ObjectFactory.CreateGameObject(
                "VLXRCamera",
                typeof(SynchronousTrackingManager),
                typeof(ScreenOrientationObserver),
                typeof(XRCamera));

            xrCameraGameObject.transform.parent = vlTracking.transform;

            var arCamera = GetARCamera();
            var cameraManager = GetCameraManager();

            var xrCameraBehaviour = xrCameraGameObject.GetComponent<XRCamera>();

            if (arCamera == null)
            {
                LogHelper.LogWarning(
                    "Could not find AR Camera. Please set AR Camera fields in XRTracker and XRCamera manually.",
                    xrCameraGameObject);
            }
            else
            {
                xrCameraBehaviour.arCamera = arCamera;
            }
            if (cameraManager == null)
            {
                LogHelper.LogWarning(
                    "Could not find AR Camera Manager. Please set AR Camera Manager fields in XRCamera manually.",
                    xrCameraGameObject);
            }
            else
            {
                xrCameraBehaviour.cameraManager = cameraManager;
            }
            return vlTracking;
        }

        [MenuItem("GameObject/VisionLib/AR Foundation/Add XR Tracking", false, 10)]
        static void AddVLTracking(MenuCommand menuCommand)
        {
            var vlTracking = CreateAndConnectVLTracking();
            // Ensure it gets re-parented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(vlTracking, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(vlTracking, "Create " + vlTracking.name);
            Selection.activeObject = vlTracking;
        }
    }
}

#endif