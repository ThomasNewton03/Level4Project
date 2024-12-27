using UnityEngine;
using UnityEngine.Serialization;
using System.Threading.Tasks;
using UnityEditor;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Camera used to define the initial pose for the HoloLens model-based
    ///  tracking.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   If there is no HoloLensInitCamera in the scene or the
    ///   HoloLensInitCamera is disabled, then the HoloLens model-based
    ///   tracking will not work correctly.
    ///  </para>
    ///  <para>
    ///   It's possible to change the camera position and orientation at
    ///   runtime. The new initial pose will then be used while the tracking is
    ///   lost.
    ///  </para>
    ///  <para>
    ///   Please make sure, that there is only one active
    ///   HoloLensInitCamera in the scene. Otherwise both behaviours
    ///   will try to set the initial pose, which will lead to unexpected
    ///   behaviour.
    ///  </para>
    /// </remarks>
    /// @ingroup HoloLens
    /// \deprecated HoloLensInitCamera is obsolete. Please use the new TrackingAnchor instead.
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "holo_lens_init_camera.html")]
    [AddComponentMenu("VisionLib/HoloLens/HoloLens Init Camera")]
    [System.Obsolete("HoloLensInitCamera is obsolete. Please use the new TrackingAnchor instead.")]
    public class HoloLensInitCamera : MonoBehaviour
    {
        /// <summary>
        ///  Reference to used HoloLensTracker.
        /// </summary>
        /// <remarks>
        ///  If this is not defined, then the first found
        ///  HoloLensTracker will be used automatically.
        /// </remarks>
        public HoloLensTracker holoLensTracker;

        /// <summary>
        ///  Reference to the Camera.
        /// </summary>
        /// <remarks>
        ///  If this is not defined, then the Camera component attached to the
        ///  current GameObject will be used automatically.
        /// </remarks>
        public Camera initCamera;

        /// <summary>
        ///  Overwrite camera transformation with values from tracking
        ///  configuration.
        /// </summary>
        /// <remarks>
        ///  The InitCamera can then be transformed afterwards, but will get
        ///  overwritten again after loading a new tracking configuration.
        /// </remarks>
        [Tooltip("Overwrite camera transformation with values from tracking configuration")]
        [FormerlySerializedAs("overwriteOnLoad")]
        public bool usePoseFromTrackingConfig;

        /// <summary>
        ///  Adapt initial pose to always be upright.
        /// </summary>
        /// <remarks>
        ///  This only works correctly, if <see cref="upAxis"/> actually is the
        ///  up-axis of the content.
        /// </remarks>
        [Tooltip("Adapt initial pose to always be upright")]
        public bool keepUpright = false;

        /// <summary>
        ///  Defines the up-axis used by the <see cref="keepUpright"/> option.
        /// </summary>
        /// <remarks>
        ///  In Unity usually the y-axis points up.
        /// </remarks>
        [Tooltip(
            "Defines the up-axis used by the keepUpright option (usually the y-axis points up)")]
        [OnlyShowIf("keepUpright", true)]
        public Vector3 upAxis = Vector3.up;

        private RenderRotation renderRotation = RenderRotation.CCW0;

        private float[] projectionMatrixArray = new float[16];
        private Matrix4x4 projectionMatrix = new Matrix4x4();

        private bool initPoseReady;
        private bool initMode = true;
        private bool resetToOriginalPose;

        private Vector3 originalPosition;
        private Quaternion originalOrientation;

        private int updateIgnoreCounter = 0;

        private const int maxSetGlobalObjectPoseCommands = 5;
        private int setGlobalObjectPoseCounter = 0;

        /// <summary>
        ///  Restores the original transformation of the InitCamera.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This might be useful if the InitCamera was transformed in some
        ///   awkward way for some reason and we quickly want to restore the
        ///   original state.
        ///  </para>
        ///  <para>
        ///   If <see cref="usePoseFromTrackingConfig"/> is set to <c>false</c>, then this
        ///   will restore the transformation during the initialization of the
        ///   HoloLensInitCamera. If <see cref="usePoseFromTrackingConfig"/> is
        ///   set to <c>true</c>, then this will restore the transformation from
        ///   the tracking configuration.
        ///  </para>
        /// </remarks>
        public void ResetToOriginalPose()
        {
            this.resetToOriginalPose = true;
        }

        /// \deprecated The `Reset()` function of HoloLensInitCamera is obsolete. Use `ResetToOriginalPose()` instead.
        [System.Obsolete("The `Reset()` function of HoloLensInitCamera is obsolete. Use `ResetToOriginalPose()` instead.")]
        public void Reset()
        {
            ResetToOriginalPose();
        }

        private bool IsReady()
        {
            return this.initPoseReady && this.initCamera != null;
        }

        private void OnTrackerInitializing()
        {
            this.initPoseReady = false;
            this.resetToOriginalPose = false;
            this.initMode = true;
            this.updateIgnoreCounter = 0;
            this.setGlobalObjectPoseCounter = 0;
        }

        private void OnTrackerInitialized()
        {
            if (!this.usePoseFromTrackingConfig)
            {
                this.initPoseReady = true;
            }
            else
            {
                this.GetInitPose();
                // We wait for the initial pose. initPoseReady will then get set
                // inside the callback from GetInitPose.
            }
        }

        private void OnExtrinsicData(ExtrinsicData extrinsicData)
        {
            if (this.updateIgnoreCounter > 0)
            {
                this.updateIgnoreCounter -= 1;
                return;
            }

            bool valid = extrinsicData.GetValid();

            // State changed from invalid to valid?
            if (valid && this.initMode)
            {
                this.initMode = false;
            }
            // State changed from valid to invalid?
            else if (!valid && !this.initMode)
            {
                // Do not go to initialization mode, because the HoloLens is able to
                // relocate itself
                // this.initMode = true;
            }
        }

        private void OnIntrinsicData(IntrinsicData intrinsicData)
        {
            if (this.initCamera == null)
            {
                return;
            }

            // Apply the intrinsic camera parameters
            if (intrinsicData.GetProjectionMatrix(
                    this.initCamera.nearClipPlane,
                    this.initCamera.farClipPlane,
                    Screen.width,
                    Screen.height,
                    this.renderRotation,
                    0,
                    this.projectionMatrixArray))
            {
                for (int i = 0; i < 16; ++i)
                {
                    this.projectionMatrix[i % 4, i / 4] = this.projectionMatrixArray[i];
                }
                this.initCamera.projectionMatrix = this.projectionMatrix;
            }
        }

        private void OnTrackerReset()
        {
            this.initMode = true;
            // Ignore the next OnExtrinsicData call,
            // because it might contain the previous
            // (valid) tracking pose
            this.updateIgnoreCounter = 1;
        }

        private async Task GetInitPoseAsync()
        {
            var maybeInitPose = await ModelTrackerCommands.GetInitPoseAsync(TrackingManager.Instance.Worker);
            this.ThrowIfNotAlive();
            if (!maybeInitPose.HasValue)
            {
                NotificationHelper.SendWarning("Tried to read initPose from Tracking Config but it did not contain any.", this);
                return;
            }
            var initPose = maybeInitPose.Value;
            CameraHelper.VLPoseToCamera(
                new Vector3(initPose.t[0], initPose.t[1], initPose.t[2]),
                new Quaternion(initPose.r[0], initPose.r[1], initPose.r[2], initPose.r[3]),
                out this.originalPosition,
                out this.originalOrientation);

            this.initPoseReady = true;
            this.resetToOriginalPose =
                true; // This will set the new pose during the next Update call
        }

        private void GetInitPose()
        {
            TrackingManager.CatchCommandErrors(GetInitPoseAsync(), this);
        }

        private async Task<WorkerCommands.CommandWarnings> SetInitPoseAsync()
        {
            if (!IsReady())
            {
                LogHelper.LogWarning("SetInitPose called while not ready");
                return WorkerCommands.NoWarnings();
            }

            GameObject content =
                (this.holoLensTracker != null ? this.holoLensTracker.content : null);
            if (content == null)
            {
                LogHelper.LogWarning(
                    "No HoloLensTracker in the scene or its content is not specified");
                return WorkerCommands.NoWarnings();
            }

            if (!TrackingManager.DoesTrackerExistAndIsRunning())
            {
                return WorkerCommands.NoWarnings();
            }

            // To prevent the vlSDK from getting more `SetGlobalObjectPose` calls
            // than can be processed in time, we limit the amount of
            // `SetGlobalObjectPose` commands.
            if (this.setGlobalObjectPoseCounter >= maxSetGlobalObjectPoseCommands)
            {
                return WorkerCommands.NoWarnings();
            }
            this.setGlobalObjectPoseCounter += 1;

            // Turn the camera pose into a content transformation
            Matrix4x4 worldToInitCameraMatrix = this.initCamera.transform.worldToLocalMatrix;
            Vector3 initContentLocalPosition = worldToInitCameraMatrix.GetColumn(3);
            Quaternion initContentLocalOrientation =
                CameraHelper.QuaternionFromMatrix(worldToInitCameraMatrix);

            content.transform.localPosition = initContentLocalPosition;
            content.transform.localRotation = initContentLocalOrientation;
            if (this.keepUpright)
            {
                Vector3 contentUp = content.transform.rotation * this.upAxis;
                Quaternion upRotation = Quaternion.FromToRotation(contentUp, Vector3.up);
                content.transform.rotation = upRotation * content.transform.rotation;
            }

            Matrix4x4 globalObjectMatrix = content.transform.localToWorldMatrix;

            // Compute the right-handed global object transformation
            globalObjectMatrix = CameraHelper.flipY * globalObjectMatrix * CameraHelper.flipX;

            // We need to provide the global coordinate system once and push the
            // current position of the content to the tracker in every frame.
            Vector3 t = globalObjectMatrix.GetColumn(3);
            Quaternion r = Quaternion.LookRotation(
                globalObjectMatrix.GetColumn(2),
                globalObjectMatrix.GetColumn(1));
            try
            {
                return await ModelTrackerCommands.SetGlobalObjectPoseAsync(
                    TrackingManager.Instance.Worker,
                    new ModelTrackerCommands.InitPose(t, r));
            }
            finally
            {
                this.setGlobalObjectPoseCounter -= 1;
            }
        }

        private void SetInitPose()
        {
            TrackingManager.CatchCommandErrors(SetInitPoseAsync(), this);
        }

        private void Awake()
        {
            // Get the initCamera from the current GameObject, if it wasn't
            // specified explicitly
            if (this.initCamera == null)
            {
                this.initCamera = this.GetComponent<Camera>();
            }

            if (this.initCamera != null)
            {
                // Store the original transformation so we can restore it later
                this.originalPosition = this.initCamera.transform.position;
                this.originalOrientation = this.initCamera.transform.rotation;
            }
            else
            {
                LogHelper.LogWarning(
                    "InitCamera not found. Please add a Camera to the GameObject or set its reference manually.",
                    this);
            }
        }

        private void Start()
        {
            // Find the first HoloLensTracker, if it wasn't specified
            // explicitly
            if (this.holoLensTracker == null)
            {
                // Automatically find HoloLensTracker
                this.holoLensTracker = FindObjectOfType<HoloLensTracker>();
                if (this.holoLensTracker == null)
                {
                    LogHelper.LogWarning("No GameObject with HoloLensTracker found");
                }
            }
        }

        private void OnEnable()
        {
            TrackingManager.OnExtrinsicData += OnExtrinsicData;
            TrackingManager.OnIntrinsicData += OnIntrinsicData;
            TrackingManager.OnTrackerInitializing += OnTrackerInitializing;
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetSoft += OnTrackerReset;
            TrackingManager.OnTrackerResetHard += OnTrackerReset;
#pragma warning restore CS0618 // Tracker Reset events are obsolete

            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                OnTrackerInitialized();
            }
        }

        private void OnDisable()
        {
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetHard -= OnTrackerReset;
            TrackingManager.OnTrackerResetSoft -= OnTrackerReset;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
            TrackingManager.OnTrackerInitializing -= OnTrackerInitializing;
            TrackingManager.OnIntrinsicData -= OnIntrinsicData;
            TrackingManager.OnExtrinsicData -= OnExtrinsicData;
        }

        private void Update()
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return;
            }

            if (IsReady())
            {
                if (this.resetToOriginalPose)
                {
                    this.initCamera.transform.position = this.originalPosition;
                    this.initCamera.transform.rotation = this.originalOrientation;
                    this.resetToOriginalPose = false;
                }

                if (this.initMode)
                {
                    // This must be called continuously. Otherwise the content will
                    // appear anchored somewhere in the world and not positioned
                    // in front of the camera
                    this.SetInitPose();
                }
            }
        }

#if UNITY_EDITOR
        /*
         * Aligns the HoloLensInitCamera with the current camera position and rotation in the scene
         * view. This is useful for setting an InitPose more easily.
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
