using UnityEngine;
using System;
using System.Threading.Tasks;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.ARFoundation
{
    /**
     *  @ingroup ARFoundation
     *  \deprecated Please use a TrackingAnchor instead.
     */
    [Obsolete("Please use a TrackingAnchor instead.")]
    [AddComponentMenu("VisionLib/AR Foundation/XR Tracker")]
    [HelpURL(DocumentationLink.APIReferenceURI.ARFoundation + "x_r_tracker.html")]
    public class XRTracker : MonoBehaviour
    {
        /// <summary>
        ///  GameObject with the AR content attached to it.
        /// </summary>
        /// <remarks>
        ///  This script will change the transformation of that GameObject according to the tracking
        ///  result.
        /// </remarks>
        [Tooltip("GameObject with the AR content attached to it")]
        public GameObject content;

        /// <summary>
        ///  AR Camera from ARFoundation
        /// </summary>
        /// <remarks>
        ///  It is automatically created inside the `AR Session Origin` Prefab
        /// </remarks>
        [Tooltip("AR Camera from AR Foundation")]
        [DisplayName("AR Camera")]
        public GameObject arCamera;

        /// <summary>
        ///  Reference to the camera used to define the initial pose.
        /// </summary>
        [Tooltip("Reference to the camera used to define the initial pose.")]
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

        /// <summary>
        ///  Interpolation time to apply updates to the content Transform.
        /// </summary>
        /// <remarks>
        ///  Set to 0 to directly apply tracking results without smoothing.
        /// </remarks>
        [Tooltip("Interpolation time to apply updates to the content Transform.")]
        public float smoothTime = 0.03f;

        private GameObject worldAnchorGO;
        private Vector3 referenceInitPosition = Vector3.zero;
        private Quaternion referenceInitRotation = Quaternion.identity;
        private bool initPoseReady = false;
        private bool reset = false;
        private const int maxSetGlobalObjectPoseCommands = 5;
        private int setGlobalObjectPoseCounter = 0;
        private PositionUpdateDamper interpolationTarget = new PositionUpdateDamper();

        private void Awake()
        {
            this.worldAnchorGO = new GameObject("WorldAnchor");

            if (this.content == null)
            {
                LogHelper.LogWarning(
                    "Content is null. Did you forget to set the 'content' property?",
                    this);
            }

            if (this.initCamera != null)
            {
                // Store the reference transformation so we can restore it later
                this.referenceInitPosition = this.initCamera.transform.position;
                this.referenceInitRotation = this.initCamera.transform.rotation;
            }
            else
            {
                LogHelper.LogError("InitCamera not found", this);
            }
        }

        void OnEnable()
        {
            TrackingManager.OnTrackerInitializing += OnTrackerInitializing;
            TrackingManager.AnchorTransform("TrackedObject").OnUpdate += OnSimilarityTransform;
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetSoft += ActivateInitMode;
            TrackingManager.OnTrackerResetHard += ActivateInitMode;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
            this.worldAnchorGO.SetActive(true);
        }

        void OnDisable()
        {
            // GameObject not destroyed already?
            if (this.worldAnchorGO != null)
            {
                this.worldAnchorGO.SetActive(false);
            }
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetHard -= ActivateInitMode;
            TrackingManager.OnTrackerResetSoft -= ActivateInitMode;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
            TrackingManager.AnchorTransform("TrackedObject").OnUpdate -= OnSimilarityTransform;
            TrackingManager.OnTrackerInitializing -= OnTrackerInitializing;
        }

        private void Start()
        {
            this.ActivateInitMode();
        }

        private void Update()
        {
            if (this.content == null)
            {
                return;
            }

            if (this.reset)
            {
                this.initCamera.transform.position = this.referenceInitPosition;
                this.initCamera.transform.rotation = this.referenceInitRotation;
                this.reset = false;
            }

            UpdateContentTransform();
            if (this.IsInitMode())
            {
                SetInitPose();
            }
        }

        private void OnSimilarityTransform(SimilarityTransform simTrans)
        {
            if (simTrans.GetValid())
            {
                DeactivateInitMode();
                var visionLibToUnityWorld = CameraHelper.flipXY;
                this.interpolationTarget.SetData(
                    visionLibToUnityWorld * new ModelTransform(simTrans));
                UpdateContentTransform();
            }
            else
            {
                ActivateInitMode();
            }
        }

        private void OnTrackerInitializing()
        {
            this.initPoseReady = false;
            this.reset = false;
            this.setGlobalObjectPoseCounter = 0;
            this.interpolationTarget = new PositionUpdateDamper();

            this.ActivateInitMode();
        }

        private void ActivateInitMode()
        {
            AttachContentTo(this.arCamera);
        }

        private void DeactivateInitMode()
        {
            AttachContentTo(this.worldAnchorGO);
        }

        private bool IsInitMode()
        {
            return this.content.transform.parent == this.arCamera.transform;
        }

        private void AttachContentTo(GameObject newParent)
        {
            if (this.content == null || newParent == null ||
                this.content.transform.parent == newParent.transform)
            {
                return;
            }
            this.interpolationTarget.Invalidate();
            this.content.transform.parent = newParent.transform;
        }

        /// <summary>
        ///  Restores the original transformation.
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
        ///   XRTracker. If <see cref="usePoseFromTrackingConfig"/> is
        ///   set to <c>true</c>, then this will restore the transformation from
        ///   the tracking configuration.
        ///  </para>
        /// </remarks>
        public void Reset()
        {
            this.reset = true;
        }

        private bool IsReady()
        {
            return this.initPoseReady && this.initCamera != null;
        }

        private void UpdateContentTransform()
        {
            if (this.IsInitMode())
            {
                // Turn the camera pose into a content transformation
                Matrix4x4 worldToInitCameraMatrix = this.initCamera.transform.worldToLocalMatrix;
                Vector3 initContentLocalPosition = worldToInitCameraMatrix.GetColumn(3);
                Quaternion initContentLocalOrientation =
                    CameraHelper.QuaternionFromMatrix(worldToInitCameraMatrix);

                this.content.transform.localPosition = initContentLocalPosition;
                this.content.transform.localRotation = initContentLocalOrientation;
                if (this.keepUpright)
                {
                    Vector3 contentUp = this.content.transform.rotation * this.upAxis;
                    Quaternion upRotation = Quaternion.FromToRotation(contentUp, Vector3.up);
                    this.content.transform.rotation = upRotation * this.content.transform.rotation;
                }
            }
            else
            {
                this.interpolationTarget.Slerp(this.smoothTime, this.content);
            }
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
            }
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
                out this.referenceInitPosition,
                out this.referenceInitRotation);

            this.initPoseReady = true;
            this.reset = true; // This will set the new pose during the next Update call
        }

        private void GetInitPose()
        {
            TrackingManager.CatchCommandErrors(GetInitPoseAsync(), this);
        }

        private async Task<WorkerCommands.CommandWarnings> SetInitPoseAsync()
        {
            if (!IsReady())
            {
                return WorkerCommands.NoWarnings();
            }

            if (this.content == null)
            {
                LogHelper.LogWarning("Content is not specified", this);
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

            var warnings = await ModelTrackerCommands.SetGlobalObjectPoseAsync(
                TrackingManager.Instance.Worker,
                ObjectToInitParam(this.content));
            this.ThrowIfNotAlive();
            this.setGlobalObjectPoseCounter -= 1;
            return warnings;
        }

        private void SetInitPose()
        {
            TrackingManager.CatchCommandErrors(SetInitPoseAsync(), this);
        }

        private ModelTrackerCommands.InitPose ObjectToInitParam(GameObject gameObject)
        {
            Matrix4x4 globalObjectMatrix = gameObject.transform.localToWorldMatrix;
            globalObjectMatrix = CameraHelper.flipXY * CameraHelper.flipX * globalObjectMatrix *
                                 CameraHelper.flipX;

            Vector3 t = globalObjectMatrix.GetColumn(3);
            Quaternion r = Quaternion.LookRotation(
                globalObjectMatrix.GetColumn(2),
                globalObjectMatrix.GetColumn(1));

            return new ModelTrackerCommands.InitPose(t, r);
        }

        private void OnDestroy()
        {
            if (this.worldAnchorGO != null)
            {
                Destroy(this.worldAnchorGO);
                this.worldAnchorGO = null;
            }
        }
    }
}
