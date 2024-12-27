using System;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Manages the model-based initialization of the HoloLens tracking.
    /// </summary>
    /// @ingroup HoloLens
    /// \deprecated The HoloLensTracker is obsolete. Please use the new TrackingAnchor and a HoloLensGlobalCoordinateSystem instead.
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "holo_lens_tracker.html")]
    [AddComponentMenu("VisionLib/HoloLens/HoloLens Tracker")]
    [RequireComponent(typeof(HoloLensGlobalCoordinateSystem))]
    [System.Obsolete("The HoloLensTracker is obsolete. Please use the new TrackingAnchor and a HoloLensGlobalCoordinateSystem instead.")]
    public class HoloLensTracker : MonoBehaviour
    {
        /// <summary>
        ///  The GameObject representing the HoloLens camera.
        /// </summary>
        ///  If this is not defined, then the HoloLensTracker tries to
        ///  find the camera automatically. It will first try to find a camera
        ///  component on the current GameObject. Then it will use the main camera.
        ///  If this also fails, it will use any camera available in the current
        ///  scene.
        /// </remarks>
        [Tooltip(
            "The GameObject representing the HoloLens camera. If this is not defined, then it will get set automatically.")]
        public GameObject holoLensCamera;

        private HoloLensLocalizationHandle localizationHandle = null;

        /// <summary>
        ///  GameObject with the AR content attached to it.
        /// </summary>
        /// <remarks>
        ///  Any existing transformation of the content GameObject will get
        ///  overwritten. If you need to transform the content, then please add
        ///  a child GameObject and apply the transformation to it instead.
        /// </remarks>
        [Tooltip("GameObject with the AR content attached to it")]
        public GameObject content;

        public float smoothTime = 0.03f;

        private GameObject worldAnchorGO;

        private bool initMode = true;

        private PositionUpdateDamper interpolationTarget = new PositionUpdateDamper();

        private int updateIgnoreCounter = 0;

        private void OnTrackerInitializing()
        {
            this.interpolationTarget = new PositionUpdateDamper();
            this.updateIgnoreCounter = 0;

            this.ActivateInitMode();
        }

        private void OnTrackerInitialized()
        {
            this.localizationHandle.SetLocalizationDataInVisionLib(
                TrackingManager.Instance.Worker,
                this);
        }

        private void OnModelToWorldTransform(SimilarityTransform similarityTransform)
        {
            if (this.updateIgnoreCounter > 0)
            {
                this.updateIgnoreCounter -= 1;
                return;
            }

            // State changed from invalid to valid?
            bool valid = similarityTransform.GetValid();
            if (valid && this.initMode)
            {
                this.DeactivateInitMode();
            }

            try
            {
                ModelTransform mt = new ModelTransform(similarityTransform);
                Matrix4x4 modelViewMatrix = Matrix4x4.TRS(mt.t, mt.r, mt.s);

                // Compute the left-handed world to camera matrix
                modelViewMatrix = CameraHelper.flipY * modelViewMatrix * CameraHelper.flipX;

                // Do not set the position directly. Interpolate smoothly in the
                // Update function instead
                this.interpolationTarget.SetData(
                    modelViewMatrix.GetColumn(3),
                    Quaternion.LookRotation(
                        modelViewMatrix.GetColumn(2),
                        modelViewMatrix.GetColumn(1)),
                    Vector3.one * similarityTransform.GetS());
            }
            catch (InvalidOperationException) {}
        }

        private void OnTrackerReset()
        {
            this.ActivateInitMode();
            this.updateIgnoreCounter = 1; // Ignore the next OnExtrinsicData call,
            // because it might contain the previous
            // (valid) tracking pose
        }

        private void ActivateInitMode()
        {
            if (this.initMode)
            {
                return;
            }

            this.initMode = true;
            AttachContentToHoloLensCamera(this.content);
        }

        private void DeactivateInitMode()
        {
            if (!this.initMode)
            {
                return;
            }

            this.initMode = false;
            DetachContentFromCameraAndAttachToWorldAnchor(this.content);

            this.interpolationTarget.Invalidate();
        }

        private void AttachContentToHoloLensCamera(GameObject contentToAttach)
        {
            if (contentToAttach != null && this.holoLensCamera != null)
            {
                contentToAttach.transform.parent = this.holoLensCamera.transform;
            }
        }

        private void DetachContentFromCameraAndAttachToWorldAnchor(GameObject contentToDetach)
        {
            if (contentToDetach != null)
            {
                contentToDetach.transform.parent = this.worldAnchorGO.transform;
                // When changing the parent node, Unity will keep the same
                // position and rotation in the world. Therefore we do not have to
                // convert the initial pose to world coordinates by ourself.
            }
        }

        /// <summary>
        /// Set the content that is used as the augmentation for the
        /// holoLens tracking and switch the model which is attached
        /// to the camera when in init mode.
        /// </summary>
        public void SetContent(GameObject newContent)
        {
            if (this.initMode)
            {
                DetachContentFromCameraAndAttachToWorldAnchor(this.content);
                AttachContentToHoloLensCamera(newContent);
            }
            this.content = newContent;
        }

        private bool InitCameraReference()
        {
            // HoloLens camera specified manually or previously found?
            if (this.holoLensCamera != null)
            {
                return true;
            }

            // Look for it at the same GameObject first
            Camera camera = GetComponent<Camera>();
            if (camera != null)
            {
                this.holoLensCamera = camera.gameObject;
                return true;
            }

            // Use the main camera
            camera = CameraProvider.MainCamera;
            if (camera != null)
            {
                this.holoLensCamera = camera.gameObject;
                return true;
            }

            return false;
        }

        private void Awake()
        {
            // Create the GameObject, which represents the origin of the HoloLens
            // coordinate system in Unity
            this.worldAnchorGO = new GameObject("VLHoloLensWorldAnchor");

            if (this.content == null)
            {
                LogHelper.LogWarning(
                    "Content is null. Did you forget to set the 'content' property?",
                    this);
            }
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackerInitializing += OnTrackerInitializing;
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;
            TrackingManager.AnchorTransform("TrackedObject").OnUpdate += OnModelToWorldTransform;
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetSoft += OnTrackerReset;
            TrackingManager.OnTrackerResetHard += OnTrackerReset;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
            this.worldAnchorGO.SetActive(true);
        }

        private void OnDisable()
        {
            // GameObject not destroyed already?
            if (this.worldAnchorGO != null)
            {
                this.worldAnchorGO.SetActive(false);
            }
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetHard -= OnTrackerReset;
            TrackingManager.OnTrackerResetSoft -= OnTrackerReset;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
            TrackingManager.AnchorTransform("TrackedObject").OnUpdate -= OnModelToWorldTransform;
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
            TrackingManager.OnTrackerInitializing -= OnTrackerInitializing;
        }

        private void Start()
        {
            if (!this.InitCameraReference())
            {
                LogHelper.LogWarning("Could not find HoloLens camera");
            }

            if (this.localizationHandle == null)
            {
                this.localizationHandle = HoloLensLocalizationHandle.CreateLocalizationHandle();
            }

            this.initMode = false;
            this.ActivateInitMode();
        }

        private void OnDestroy()
        {
            if (this.worldAnchorGO != null)
            {
                Destroy(this.worldAnchorGO);
                this.worldAnchorGO = null;
            }

            this.localizationHandle?.Dispose();
        }

        private void Update()
        {
            if (this.content == null)
            {
                return;
            }

            if (!this.initMode)
            {
                this.interpolationTarget.Slerp(this.smoothTime, this.content);
            }
        }
    }
}
