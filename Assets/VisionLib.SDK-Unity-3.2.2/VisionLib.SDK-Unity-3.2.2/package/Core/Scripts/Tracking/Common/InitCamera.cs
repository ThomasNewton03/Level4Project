using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Threading.Tasks;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Camera used to define the initial pose.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   It's possible to change the camera position and orientation at
    ///   runtime. The new initial pose will then be used while the tracking is
    ///   lost.
    ///  </para>
    ///  <para>
    ///   If there is no InitCamera in the scene or the
    ///   InitCamera is disabled, then the initial pose from the
    ///   tracking configuration file will be used.
    ///  </para>
    ///  <para>
    ///   Make sure, that there is only one active InitCamera in
    ///   the scene. Otherwise both components will try to set the initial pose,
    ///   which will lead to unexpected behaviour.
    ///  </para>
    ///  <para>
    ///   Right now this behaviour does not work with the HoloLens model-based
    ///   tracking. In that case use the HoloLensInitCamera
    ///   or VLHoloLensInitCamera prefab instead.
    ///  </para>
    /// </remarks>
    /// \deprecated InitCamera is obsolete and should not be used. It only exists for
    ///  backwards-compatibility with legacy VisionLib tracking scenes.
    ///  Configure init poses directly on TrackingAnchor Objects.
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "init_camera.html")]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("VisionLib/Core/Init Camera")]
    [Obsolete(
        "InitCamera is obsolete and should not be used. It only exists for" +
        " backwards-compatibility with legacy VisionLib tracking scenes." +
        " Configure init poses directly on TrackingAnchor Objects.")]
    public class InitCamera : MonoBehaviour
    {
        /// <summary>
        ///  Reference to the Camera component attached to the GameObject.
        /// </summary>
        private Camera initCamera;

        /// <summary>
        ///  Layer with the camera image background.
        /// </summary>
        /// <remarks>
        ///  This layer will not be rendered.
        /// </remarks>
        public int backgroundLayer = 8;

        /// <summary>
        ///  Use the last valid camera pose as initialization pose.
        /// </summary>
        /// <remarks>
        ///  Since this might results in an awkward InitCamera transformation, it's
        ///  recommended to give the user the option to restore the original pose
        ///  using the <see cref="ResetToOriginalPose"/> function.
        /// </remarks>
        [Tooltip("Use the last valid pose as initialization pose")]
        public bool useLastValidPose;

        /// <summary>
        ///  Overwrite init camera transformation with values from tracking
        ///  configuration on tracking start.
        /// </summary>
        /// <remarks>
        ///  The InitCamera can then be transformed afterwards, but will get
        ///  overwritten again after loading a new tracking configuration.
        /// </remarks>
        [Tooltip(
            "Overwrite init camera transformation with values from tracking configuration on tracking start")]
        [FormerlySerializedAs("overwriteOnLoad")]
        public bool usePoseFromTrackingConfig;

        private Matrix4x4 renderRotationMatrixFromVLToUnity = RenderRotationHelper.rotationZ0;
        private RenderRotation renderRotation = RenderRotation.CCW0;

        private float[] projectionMatrixArray = new float[16];
        private Matrix4x4 projectionMatrix = new Matrix4x4();

        private bool settingInitPose;
        private bool gotInitPose = false;
        private bool resetToOriginalPose;

        private Vector3 originalPosition;
        private Quaternion originalOrientation;

        private TransformCache initPoseInBackend;

        /// <summary>
        ///  Restores the original transformation of the InitCamera.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This might be useful if the InitCamera was transformed in some
        ///   awkward way for some reason (e.g. because
        ///   <see cref="useLastValidPose"/> is set to <c>true</c> and the tracking
        ///   failed) and we quickly want to restore the original state.
        ///  </para>
        ///  <para>
        ///   If <see cref="usePoseFromTrackingConfig"/> is set to <c>false</c>, then this
        ///   will restore the transformation during the initialization of the
        ///   InitCamera. If <see cref="usePoseFromTrackingConfig"/> is set to
        ///   <c>true</c>, then this will restore the transformation from the
        ///   tracking configuration.
        ///  </para>
        /// </remarks>
        public void ResetToOriginalPose()
        {
            this.resetToOriginalPose = true;
        }

        [System.Obsolete(
            "The `void Reset()` function of InitCamera is obsolete. Use the ResetToOriginalPose functions instead.")]
        /// \deprecated The `void Reset()` function of InitCamera is obsolete. Use the ResetToOriginalPose functions instead.
        public void Reset()
        {
            ResetToOriginalPose();
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            this.initPoseInBackend.Invalidate();
            this.renderRotation = orientation.GetRenderRotation();
            this.renderRotationMatrixFromVLToUnity = this.renderRotation.GetMatrixFromVLToUnity();
        }

        private void OnTrackerInitializing()
        {
            this.settingInitPose = false;
            this.resetToOriginalPose = false;
        }

        private void OnTrackerInitialized()
        {
            this.initPoseInBackend.Invalidate();
            if (!this.usePoseFromTrackingConfig)
            {
                this.UpdateInitPoseIfTransformChanged();
                this.gotInitPose = true;
            }
            else
            {
                this.GetInitPose();
            }
        }

        private void OnExtrinsicData(ExtrinsicData extrinsicData)
        {
            if (!this.useLastValidPose || !this.gotInitPose || !extrinsicData.GetValid())
            {
                return;
            }

            try
            {
                Vector3 position;
                Quaternion rotation;
                CameraHelper.ModelViewMatrixToUnityPose(
                    extrinsicData.GetModelViewMatrix(),
                    this.renderRotationMatrixFromVLToUnity,
                    out position,
                    out rotation);
                this.initCamera.transform.rotation = rotation;
                this.initCamera.transform.position = position;
            }
            catch (InvalidOperationException) {}
        }

        private void OnIntrinsicData(IntrinsicData intrinsicData)
        {
            // Apply the intrinsic camera parameters
            if (intrinsicData.GetProjectionMatrix(
                    this.initCamera.nearClipPlane,
                    this.initCamera.farClipPlane,
                    Screen.width,
                    Screen.height,
                    this.renderRotation,
                    0,
                    projectionMatrixArray))
            {
                for (int i = 0; i < 16; ++i)
                {
                    projectionMatrix[i % 4, i / 4] = projectionMatrixArray[i];
                }
                this.initCamera.projectionMatrix = projectionMatrix;
            }
        }

        private async Task GetInitPoseAsync()
        {
            this.gotInitPose = false;

            var maybeInitPose =
                await ModelTrackerCommands.GetInitPoseAsync(TrackingManager.Instance.Worker);

            this.ThrowIfNotAlive();
            if (!maybeInitPose.HasValue)
            {
                NotificationHelper.SendWarning(
                    "Tried to read initPose from Tracking Config but it did not contain any.",
                    this);
                return;
            }

            var initPose = maybeInitPose.Value;

            CameraHelper.VLPoseToCamera(
                new Vector3(initPose.t[0], initPose.t[1], initPose.t[2]),
                new Quaternion(initPose.r[0], initPose.r[1], initPose.r[2], initPose.r[3]),
                out var position,
                out var orientation);

            this.initCamera.transform.position = position;
            this.initCamera.transform.rotation = orientation;
            SetOriginalPose(position, orientation);
            this.gotInitPose = true;
        }

        /// <summary>
        /// Receives the current InitPose from VisionLib and sets it internally.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        private void GetInitPose()
        {
            TrackingManager.CatchCommandErrors(GetInitPoseAsync(), this);
        }

        private async Task<WorkerCommands.CommandWarnings> SetInitPoseAsync()
        {
            this.settingInitPose = true;
            var initPose = new ModelTrackerCommands.InitPose(this.initCamera, this.renderRotation);

            var warnings = await ModelTrackerCommands.SetInitPoseAsync(
                TrackingManager.Instance.Worker,
                initPose);
            this.ThrowIfNotAlive();

            this.settingInitPose = false;
            return warnings;
        }

        /// <summary>
        /// Sets the internal InitPose inside VisionLib.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        private void SetInitPose()
        {
            TrackingManager.CatchCommandErrors(SetInitPoseAsync(), this);
        }

        private void UpdateInitPoseIfTransformChanged()
        {
            if (this.initPoseInBackend.UpdateTransform(
                    new ModelTransform(this.initCamera.transform)))
            {
                SetInitPose();
            }
        }

        private void Initialize()
        {
            if (this.initPoseInBackend != null)
            {
                return;
            }

            this.initPoseInBackend = new TransformCache();
        }

        private void Awake()
        {
            Initialize();
            this.initCamera = GetComponent<Camera>();

            // Store the original transformation so we can restore it later
            SetOriginalPose(this.initCamera.transform.position, this.initCamera.transform.rotation);
        }

        private void OnEnable()
        {
            Initialize();
            OnOrientationChange(ScreenOrientationObserver.GetScreenOrientation());
            ScreenOrientationObserver.OnOrientationChange += OnOrientationChange;

            TrackingManager.OnExtrinsicData += OnExtrinsicData;
            TrackingManager.OnIntrinsicData += OnIntrinsicData;
            TrackingManager.OnTrackerInitializing += OnTrackerInitializing;
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;

            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                OnTrackerInitialized();
            }
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
            TrackingManager.OnTrackerInitializing -= OnTrackerInitializing;
            TrackingManager.OnIntrinsicData -= OnIntrinsicData;
            TrackingManager.OnExtrinsicData -= OnExtrinsicData;

            ScreenOrientationObserver.OnOrientationChange -= OnOrientationChange;
        }

        private void Update()
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return;
            }

            if (this.resetToOriginalPose && this.gotInitPose)
            {
                this.initCamera.transform.position = this.originalPosition;
                this.initCamera.transform.rotation = this.originalOrientation;
                this.resetToOriginalPose = false;
            }

            if (!this.settingInitPose && this.gotInitPose)
            {
                this.UpdateInitPoseIfTransformChanged();
            }
        }

        /// <summary>
        /// Get the init pose values which are calculated using
        /// the init camera set in the InitCamera.
        /// </summary>
        /// <remarks>
        /// This way the init pose can be re-used
        /// e.g. in the tracking configuration.
        /// </remarks>
        /// <returns>Init pose values as string in json format</returns>
        public string GetInitPoseAsString()
        {
            var initPose = new ModelTrackerCommands.InitPose(
                this.initCamera ? this.initCamera : GetComponent<Camera>(),
                this.renderRotation);

            return JsonHelper.ToJson(initPose, true);
        }

        /// <summary>
        /// Set the original init camera position and rotation.
        /// </summary>
        /// <remarks>
        /// The init cameras transform will be reset to this pose when InitCamera.Reset()
        /// is called
        /// </remarks>
        private void SetOriginalPose(Vector3 position, Quaternion orientation)
        {
            this.originalPosition = position;
            this.originalOrientation = orientation;
        }

#if UNITY_EDITOR
        /*
         * Aligns the InitCamera with the current camera position and rotation in the scene view.
         * This is useful for setting an InitPose more easily.
         */
        public void AlignWithView()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                var sceneViewTransform = SceneView.lastActiveSceneView.camera.transform;
                this.transform.position = sceneViewTransform.position;
                this.transform.rotation = sceneViewTransform.rotation;
            }
        }
#endif
    }
}
