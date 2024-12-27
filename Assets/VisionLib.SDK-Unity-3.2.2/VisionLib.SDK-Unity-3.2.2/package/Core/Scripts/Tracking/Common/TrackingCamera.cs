using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;
using Object = System.Object;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Camera used for rendering the augmentation.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracking_camera.html")]
    [AddComponentMenu("VisionLib/Core/Tracking Camera")]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(Transform))]
    public class TrackingCamera : MonoBehaviour, ISceneValidationCheck
    {
        private const string singleModelTrackingIsObsoleteMessage =
            " The CoordinateSystemAdjustment \"SingleModelTracking\" only exists for" +
            " backwards-compatibility with legacy VisionLib tracking scenes.\nEnsure" +
            " you really intended to use this setting. You must change this to" +
            " \"MultiModelTracking\" in all up-to-date scenes using TrackingAnchors.";

        public enum CoordinateSystemAdjustment
        {
            SingleModelTracking,
            MultiModelTracking,
            Injection
        }

        public CoordinateSystemAdjustment coordinateSystemAdjustment =
            CoordinateSystemAdjustment.MultiModelTracking;

        /// <summary>
        ///  Layer with the camera image background.
        /// </summary>
        /// <remarks>
        ///  This layer will not be rendered by the tracking camera.
        /// </remarks>
        public int backgroundLayer = 8;

        private Camera trackingCamera;

        private Matrix4x4 renderRotationMatrixFromVLToUnity = RenderRotationHelper.rotationZ0;
        private RenderRotation renderRotation = RenderRotation.CCW0;

        private float[] projectionMatrixArray = new float[16];
        private Matrix4x4 projectionMatrix = new Matrix4x4();

        private static bool IsSingleModelTrackerRequired()
        {
#pragma warning disable CS0618
            return FindObjectOfType<PosterTrackerLegacy>() || FindObjectOfType<CameraCalibration>();
#pragma warning restore CS0618
        }

        private void Start()
        {
            if (this.coordinateSystemAdjustment != CoordinateSystemAdjustment.SingleModelTracking)
            {
                return;
            }

            if (!IsSingleModelTrackerRequired())
            {
                LogHelper.LogWarning(TrackingCamera.singleModelTrackingIsObsoleteMessage, this);
            }
        }

        private void OnCameraTransform(ExtrinsicData extrinsicData)
        {
            if (this.coordinateSystemAdjustment == CoordinateSystemAdjustment.MultiModelTracking)
            {
                var modelViewMatrix = CameraHelper.flipYZ *
                                      extrinsicData.GetModelViewMatrix().inverse *
                                      CameraHelper.flipYZ;
                modelViewMatrix *= CameraHelper.flipXY;
                SetModelViewMatrix(modelViewMatrix);
            }
        }

        private void OnExtrinsicData(ExtrinsicData extrinsicData)
        {
            if (this.coordinateSystemAdjustment != CoordinateSystemAdjustment.SingleModelTracking)
            {
                return;
            }
            SetModelViewMatrix(extrinsicData.GetModelViewMatrix());
        }

        private void SetModelViewMatrix(Matrix4x4 modelViewMatrix)
        {
            try
            {
                CameraHelper.ModelViewMatrixToUnityPose(
                    modelViewMatrix,
                    this.renderRotationMatrixFromVLToUnity,
                    out var position,
                    out var rotation);
                var cameraTransform = this.trackingCamera.transform;
                cameraTransform.rotation = rotation;
                cameraTransform.position = position;
            }
            catch (InvalidOperationException) {}
        }

        private void OnIntrinsicData(IntrinsicData intrinsicData)
        {
            // Apply the intrinsic camera parameters
            if (intrinsicData.GetProjectionMatrix(
                    this.trackingCamera.nearClipPlane,
                    this.trackingCamera.farClipPlane,
                    Screen.width,
                    Screen.height,
                    this.renderRotation,
                    0,
                    this.projectionMatrixArray))
            {
                for (var i = 0; i < 16; ++i)
                {
                    this.projectionMatrix[i % 4, i / 4] = this.projectionMatrixArray[i];
                }
                this.trackingCamera.projectionMatrix = this.projectionMatrix;
            }
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            this.renderRotation = orientation.GetRenderRotation();
            this.renderRotationMatrixFromVLToUnity = this.renderRotation.GetMatrixFromVLToUnity();
        }

        private void Awake()
        {
            this.trackingCamera = this.GetComponent<Camera>();

            // Do not clear the background image
            this.trackingCamera.clearFlags = CameraClearFlags.Depth;

            // Render after the background camera
            this.trackingCamera.depth = 2;

            // Do not render the background image
            var mask = 1 << this.backgroundLayer;
            this.trackingCamera.cullingMask &= ~mask;
        }

        private void OnEnable()
        {
            OnOrientationChange(ScreenOrientationObserver.GetScreenOrientation());
            ScreenOrientationObserver.OnOrientationChange += OnOrientationChange;

            TrackingManager.OnExtrinsicData += OnExtrinsicData;
            TrackingManager.OnCameraTransform += OnCameraTransform;
            TrackingManager.OnIntrinsicData += OnIntrinsicData;
        }

        private void OnDisable()
        {
            TrackingManager.OnIntrinsicData -= OnIntrinsicData;
            TrackingManager.OnCameraTransform -= OnCameraTransform;
            TrackingManager.OnExtrinsicData -= OnExtrinsicData;
            ScreenOrientationObserver.OnOrientationChange -= OnOrientationChange;
        }

#if UNITY_EDITOR
        private const string trackingAnchorCoordinateSystemAdjustmentMessage =
            "When using TrackingAnchors in the scene, the CoordinateSystemAdjustment " +
            "may not be 'SingleModelTracking'. In the default case (no ARFoundation or " +
            "ImageInjection), the value should be set to 'MultiModelTracking'.";
        private const string singleModelTrackingCoordinateSystemAdjustmentMessage =
            "When using PosterTracker or CameraCalibration in the scene, the CoordinateSystemAdjustment " +
            "may not be 'MultiModelTracking'. In the default case (no ARFoundation or " +
            "ImageInjection), the value should be set to 'SingleModelTracking'.";

        private ReversibleAction AdjustCoordinateSystem(
            CoordinateSystemAdjustment newCoordinateSystem)
        {
            return new ReversibleAction(
                () => { this.coordinateSystemAdjustment = newCoordinateSystem; },
                this,
                "Set CoordinateSystemAdjustment to " + newCoordinateSystem);
        }

        private SetupIssue CoordinateSystemIncorrectIssue(
            CoordinateSystemAdjustment newCoordinateSystem,
            string coordinateSystemIncorrectMessage)
        {
            return new SetupIssue(
                "CoordinateSystemAdjustment incorrect",
                coordinateSystemIncorrectMessage,
                SetupIssue.IssueType.Warning,
                this.gameObject,
                AdjustCoordinateSystem(newCoordinateSystem));
        }

        public List<SetupIssue> GetSceneIssues()
        {
            var issues = new List<SetupIssue>();
            if (this.coordinateSystemAdjustment == CoordinateSystemAdjustment.SingleModelTracking &&
                FindObjectsOfType<TrackingAnchor>().Length > 0)
            {
                issues.Add(
                    CoordinateSystemIncorrectIssue(
                        CoordinateSystemAdjustment.MultiModelTracking,
                        TrackingCamera.trackingAnchorCoordinateSystemAdjustmentMessage));
            }

            if (this.coordinateSystemAdjustment != CoordinateSystemAdjustment.SingleModelTracking &&
                IsSingleModelTrackerRequired())
            {
                issues.Add(
                    CoordinateSystemIncorrectIssue(
                        CoordinateSystemAdjustment.SingleModelTracking,
                        TrackingCamera.singleModelTrackingCoordinateSystemAdjustmentMessage));
            }

            return issues.Concat(TransformSetupIssueHelper.CheckForUnexpectedScale(this.gameObject))
                .ToList();
        }

#endif
    }
}
