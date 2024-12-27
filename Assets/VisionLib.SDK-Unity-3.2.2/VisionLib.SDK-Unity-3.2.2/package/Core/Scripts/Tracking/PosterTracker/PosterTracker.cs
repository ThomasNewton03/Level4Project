using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Manages positioning of the augmentation of the poster
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "poster_tracker.html")]
    [AddComponentMenu("VisionLib/Core/Poster Tracker")]
    public class PosterTracker : MonoBehaviour, ISceneValidationCheck
    {
        [SerializeField]
        private TrackingStateEventsProvider trackingEvents = new TrackingStateEventsProvider();
        
        /// <summary>
        ///  GameObject with the AR content attached to it.
        /// </summary>
        /// <remarks>
        ///  Any existing transformation of the content GameObject will get
        ///  overwritten. If you need to transform the content, then add
        ///  a child GameObject and apply the transformation to it instead.
        /// </remarks>
        [Tooltip("GameObject with the AR content attached to it")]
        public GameObject content;

        public float smoothTime = 0.03f;

        private readonly PositionUpdateDamper interpolationTarget = new PositionUpdateDamper();

        private void Awake()
        {
            if (this.content == null)
            {
                LogHelper.LogWarning(
                    "Content is null. Did you forget to set the 'content' property?",
                    this);
            }
        }

        private void OnEnable()
        {
            this.trackingEvents.EnableEvents();
            TrackingManager.OnTrackerInitialized += InvalidateTargetTransform;
            TrackingManager.AnchorTransform("TrackedObject").OnUpdate += OnModelToWorldTransform;
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetHard += InvalidateTargetTransform;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
        }

        private void Update()
        {
            if (this.content == null)
            {
                return;
            }
            this.interpolationTarget.Slerp(this.smoothTime, this.content);
        }

        protected void OnDisable()
        {
#pragma warning disable CS0618 // Tracker Reset events are obsolete
            TrackingManager.OnTrackerResetHard -= InvalidateTargetTransform;
#pragma warning restore CS0618 // Tracker Reset events are obsolete
            TrackingManager.AnchorTransform("TrackedObject").OnUpdate -= OnModelToWorldTransform;
            TrackingManager.OnTrackerInitialized -= InvalidateTargetTransform;
            this.trackingEvents.DisableEvents();
        }

        public async Task ResetTrackingHardAsync()
        {
            await PosterTrackerCommands.ResetHardAsync(TrackingManager.Instance.Worker);
#pragma warning disable CS0618 // OnTrackerResetHard is obsolete
            TrackingManager.InvokeOnTrackerResetHard();
#pragma warning restore CS0618 // OnTrackerResetHard is obsolete
            NotificationHelper.SendInfo("Tracker reset");
        }

        /// <summary>
        ///  Reset the tracking and all keyframes.
        /// </summary>
        public void ResetTrackingHard()
        {
            TrackingManager.CatchCommandErrors(ResetTrackingHardAsync(), this);
        }
        
        private void InvalidateTargetTransform()
        {
            this.interpolationTarget.Invalidate();
        }

        private void OnModelToWorldTransform(SimilarityTransform similarityTransform)
        {
            if (!similarityTransform.GetValid())
            {
                return;
            }

            try
            {
                var mt = new ModelTransform(similarityTransform);
                var modelViewMatrix = Matrix4x4.TRS(mt.t, mt.r, mt.s);

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
        
#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return this.trackingEvents.CheckForBrokenReferences(this.gameObject);
        }
#endif
    }
}
