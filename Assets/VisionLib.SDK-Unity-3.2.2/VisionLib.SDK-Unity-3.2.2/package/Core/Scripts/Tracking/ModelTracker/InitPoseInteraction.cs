using System;
using System.Collections.Generic;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.Helpers;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///     The <see cref="InitPoseInteraction"/> manages manipulation of
    ///     <see cref="TrackingAnchor"/>'s init pose via a <see cref="IObjectPoseInteractionProvider"/>.
    ///     Adding a <see cref="InitPoseInteraction"/> to the same
    ///     <see cref="GameObject"/> as a <see cref="TrackingAnchor"/> automatically sets up init
    ///     pose interaction for this <see cref="TrackingAnchor"/>.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.initPoseInteraction)]
    [RequireComponent(typeof(TrackingAnchor))]
    [RequireComponent(typeof(IObjectPoseInteractionProvider))]
    [DisallowMultipleComponent]
#pragma warning disable CS0618
    public class InitPoseInteraction : MonoBehaviour, IObjectPoseInteractionEventListener,
#pragma warning restore CS0618
        ISceneValidationCheck
    {
        private IObjectPoseInteractionProvider objectPoseInteractionProvider;
        private TrackingAnchor trackingAnchor;

        private bool isInitialized;
        private bool isInteractionInProgress;
        private bool? lastValueOfShowInitPoseGuideWhileDisabled;

        [Tooltip(
            "Disable the interaction while the tracking target is being tracked " +
            "(i.e. while tracking state is \"tracked\" or \"critical\").")]
        public bool disableDuringTracking;

        private void OnEnable()
        {
            if (!this.isInitialized)
            {
                Initialize();
            }
            SubscribeToTrackingStateEvents();
            this.objectPoseInteractionProvider.InteractionStarted += OnInteractionStarted;
            this.objectPoseInteractionProvider.TargetTransformChanged += OnTransformChanged;
            this.objectPoseInteractionProvider.InteractionEnded += OnInteractionEnded;
        }

        private void OnDisable()
        {
            UnsubscribeFromTrackingStateEvents();
            if (this.objectPoseInteractionProvider != null)
            {
                this.objectPoseInteractionProvider.InteractionEnded -= OnInteractionEnded;
                this.objectPoseInteractionProvider.TargetTransformChanged -= OnTransformChanged;
                this.objectPoseInteractionProvider.InteractionStarted -= OnInteractionStarted;
            }
        }

        private void Update()
        {
            // GameObjectPoseInteractions take place in LateUpdate(). So we move the GameObject
            // to the InitPose beforehand.
            MoveGameObjectToInitPose();
        }

        private void SubscribeToTrackingStateEvents()
        {
            this.trackingAnchor.OnTracked.AddListener(DisableInteraction);
            this.trackingAnchor.OnTrackingCritical.AddListener(DisableInteraction);
            this.trackingAnchor.OnTrackingLost.AddListener(EnableInteraction);
        }

        private void UnsubscribeFromTrackingStateEvents()
        {
            this.trackingAnchor.OnTracked.RemoveListener(DisableInteraction);
            this.trackingAnchor.OnTrackingCritical.RemoveListener(DisableInteraction);
            this.trackingAnchor.OnTrackingLost.RemoveListener(EnableInteraction);
        }

        private void EnableInteraction()
        {
            if (this.disableDuringTracking)
            {
                this.objectPoseInteractionProvider.SetEnabled(true);
            }
        }

        private void DisableInteraction()
        {
            if (this.disableDuringTracking)
            {
                this.objectPoseInteractionProvider.SetEnabled(false);
            }
        }

        /// <summary>
        /// The GameObjectPoseInteraction translates 2D screen space user inputs into the
        /// according 3D movements in world space. These are then applied to the GameObject.
        ///
        /// For this to work, i.e. to result in the expected 2D GameObject movements in the game
        /// view (i.e. on screen), the relative pose between the rendering camera and the GameObject
        /// at the time of interaction must be correct.
        ///
        /// In the context of a <see cref="TrackingAnchor"/>, the "correct" relative pose
        /// is the initPose. Therefore, we must move the GameObject to the init pose with respect
        /// to the game view camera's current pose _before_ the interaction takes place.
        ///
        /// Ths function moves the GameObject to the initPose.
        /// If the <see cref="TrackingAnchor"/> (i.e. this GameObject) is itself augmented
        /// content, this is done via the <see cref="AugmentationHandler"/>. Otherwise, the init
        /// pose is directly applied to the GameObject.
        ///
        /// NOTE: The relative pose between the rendering camera and the GameObject may also
        /// not change after the interaction has concluded until the current frame is rendered.
        /// If your code moves either the camera, the GameObject, or both in or after Update()
        /// you will have to ensure maintenance of the InitPose relationship yourself. 
        /// </summary>
        private void MoveGameObjectToInitPose()
        {
            if (IsThisGameObjectAugmentedContent())
            {
                this.trackingAnchor.UpdateInitPose();
                return;
            }
            var maybeInitPose = this.trackingAnchor.GetCurrentInitPoseInWorldCoordinateSystem();
            if (this.trackingAnchor && maybeInitPose.HasValue)
            {
                this.gameObject.transform.SetPose(maybeInitPose.Value);
            }
        }

        private bool IsThisGameObjectAugmentedContent()
        {
            return GetComponent<RenderedObject>();
        }

        public void OnInteractionStarted()
        {
            if (this.isInteractionInProgress || !this.trackingAnchor.IsReferenceValidAndEnabled())
            {
                return;
            }
            this.isInteractionInProgress = true;
            SetTrackingAnchorPoseToLastTrackingResult();
            DisableAnchorAndShowInitPoseGuide();
        }

        private void DisableAnchorAndShowInitPoseGuide()
        {
            // Do not replace the stored value if it has not been restored yet.
            if (!this.lastValueOfShowInitPoseGuideWhileDisabled.HasValue &&
                TrackingManager.DoesTrackerExistAndIsRunning())
            {
                this.lastValueOfShowInitPoseGuideWhileDisabled =
                    this.trackingAnchor.ShowInitPoseGuideWhileDisabled;
                this.trackingAnchor.ShowInitPoseGuideWhileDisabled = true;
            }
            this.trackingAnchor.enabled = false;
        }

        public void OnTransformChanged()
        {
            if (!this.isInteractionInProgress)
            {
                return;
            }
            this.trackingAnchor.SetTransformAsInitPose();
        }

        public async void OnInteractionEnded()
        {
            if (!this.trackingAnchor || !this.isInteractionInProgress)
            {
                return;
            }
            this.trackingAnchor.enabled = true;
            this.isInteractionInProgress = false;
            if (TrackingManager.DoesTrackerExistAndIsRunning())
            {
                this.trackingAnchor.SetTransformAsInitPose();
                await this.trackingAnchor.OnAnchorEnabled.AsTask();
            }
            // If interaction has already started again, do not restore the value
            if (!this.isInteractionInProgress &&
                this.lastValueOfShowInitPoseGuideWhileDisabled.HasValue)
            {
                this.trackingAnchor.ShowInitPoseGuideWhileDisabled =
                    this.lastValueOfShowInitPoseGuideWhileDisabled.Value;
                this.lastValueOfShowInitPoseGuideWhileDisabled = null;
            }
        }

        private void Initialize()
        {
            if (this.trackingAnchor != null && this.objectPoseInteractionProvider != null)
            {
                return;
            }
            this.trackingAnchor = this.gameObject.GetComponent<TrackingAnchor>();
            this.objectPoseInteractionProvider =
                this.gameObject.GetComponent<IObjectPoseInteractionProvider>();
            this.objectPoseInteractionProvider.SetInteractionCamera(
                this.trackingAnchor.GetSLAMCamera());
        }

        private void SetTrackingAnchorPoseToLastTrackingResult()
        {
            if (!this.trackingAnchor.IsReferenceValidAndEnabled())
            {
                return;
            }
            var startingPose = this.trackingAnchor.GetLastTrackingPose();
            if (!startingPose.HasValue)
            {
                return;
            }
            CameraHelper.SetUnityRotationTranslationTo(
                out var rotation,
                out var position,
                startingPose.Value);
            this.trackingAnchor.transform.SetPositionAndRotation(position, rotation);
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            Initialize();

            var TrackingAnchorCamera = this.trackingAnchor.GetSLAMCamera();
            var interactionCamera = this.objectPoseInteractionProvider.GetInteractionCamera();

            if (TrackingAnchorCamera == interactionCamera && TrackingAnchorCamera != null)
            {
                return SetupIssue.NoIssues();
            }

            var solutions = new List<ISetupIssueSolution>();
            if (TrackingAnchorCamera != null)
            {
                solutions.Add(
                    new ReversibleAction(
                        () =>
                        {
                            this.objectPoseInteractionProvider.SetInteractionCamera(
                                TrackingAnchorCamera);
                        },
                        (Object) (this.objectPoseInteractionProvider),
                        "Use Camera from TrackingAnchor in both components"));
            }
            if (interactionCamera != null)
            {
                solutions.Add(
                    new ReversibleAction(
                        () => { this.trackingAnchor.SetSLAMCamera(interactionCamera); },
                        this.trackingAnchor,
                        "Use Camera from GameObjectPoseInteraction in both components"));
            }
            if (CameraProvider.MainCamera != null)
            {
                solutions.Add(
                    new ReversibleAction(
                        () =>
                        {
                            this.trackingAnchor.SetSLAMCamera(CameraProvider.MainCamera);
                            this.objectPoseInteractionProvider.SetInteractionCamera(CameraProvider.MainCamera);
                        },
                        (Object) (this.objectPoseInteractionProvider),
                        "Use CameraProvider.MainCamera in both components"));
            }

            return new List<SetupIssue>()
            {
                new SetupIssue(
                    "Camera in TrackingAnchor and GameObjectPoseInteraction not set correctly",
                    "To work correctly, both components have to reference the same (valid) camera.",
                    SetupIssue.IssueType.Error,
                    this.gameObject,
                    solutions.ToArray())
            };
        }
#endif
    }
}
