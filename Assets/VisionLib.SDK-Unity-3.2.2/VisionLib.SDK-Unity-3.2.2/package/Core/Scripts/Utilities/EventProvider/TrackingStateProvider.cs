using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Visometry.VisionLib.SDK.Core.API;
using UnityEngine.Serialization;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This behaviour fires UnityEvents for TrackingManager.OnTrackingStates
    ///  events.
    /// </summary>
    /// <remarks>
    ///  This could for example be used to activate / disable certain GameObjects
    ///  depending on the current tracking state.
    /// </remarks>
    /// @ingroup Core
    /// \deprecated The TrackingStateProvider component is obsolete. Use the TrackingEvents of the TrackingAnchor, the PosterTracker or the CameraCalibration instead
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracking_state_provider.html")]
    [AddComponentMenu("VisionLib/Core/Tracking State Provider")]
    [System.Obsolete(
        "The TrackingStateProvider component is obsolete. Use the TrackingEvents of the TrackingAnchor, the PosterTracker or the CameraCalibration instead")]
    public class TrackingStateProvider : MonoBehaviour, ISceneValidationCheck
    {
        [Tooltip(
            "Name of the object whose tracking states should be observed." +
            "\nShould be 'TrackedObject' for Single Model Tracking, the anchor name for Multi Model Tracking.")]
        public string trackingAnchorName = "TrackedObject";

        /// <summary>
        ///  Event fired once after the tracking state changed to "tracked".
        /// </summary>
        [FormerlySerializedAs("justTrackedEvent")]
        public UnityEvent tracked = new UnityEvent();

        /// <summary>
        ///  Event fired once after the tracking state changed to "critical".
        /// </summary>
        [FormerlySerializedAs("justCriticalEvent")]
        public UnityEvent trackingCritical = new UnityEvent();

        /// <summary>
        ///  Event fired once after the tracking state changed to "lost".
        /// </summary>
        [FormerlySerializedAs("justLostEvent")]
        public UnityEvent trackingLost = new UnityEvent();

        private string previousState = "";

        void HandleTrackerInitializing()
        {
            this.previousState = "";
        }

        void HandleTrackingStates(TrackingState state)
        {
            for (int i = 0; i < state.objects.Length; ++i)
            {
                var obj = state.objects[i];

                if (obj.name == this.trackingAnchorName)
                {
                    if (obj.state == "tracked")
                    {
                        if (this.previousState != obj.state)
                        {
                            this.tracked.Invoke();
                        }
                    }
                    else if (obj.state == "critical")
                    {
                        if (this.previousState != obj.state)
                        {
                            this.trackingCritical.Invoke();
                        }
                    }
                    else if (obj.state == "lost")
                    {
                        if (this.previousState != obj.state)
                        {
                            this.trackingLost.Invoke();
                        }
                    }

                    this.previousState = obj.state;

                    break;
                }
            }
        }

        void OnEnable()
        {
            TrackingManager.OnTrackerInitializing += HandleTrackerInitializing;
            TrackingManager.OnTrackingStates += HandleTrackingStates;
        }

        void OnDisable()
        {
            TrackingManager.OnTrackingStates -= HandleTrackingStates;
            TrackingManager.OnTrackerInitializing -= HandleTrackerInitializing;
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.tracked, this.trackingCritical, this.trackingLost},
                this.gameObject);
        }
#endif
    }
}
