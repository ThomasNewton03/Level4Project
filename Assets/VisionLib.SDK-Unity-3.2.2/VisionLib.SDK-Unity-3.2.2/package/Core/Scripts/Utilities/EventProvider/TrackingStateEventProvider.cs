using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This behaviour fires UnityEvents for TrackingManager.OnTrackingStates
    ///  events.
    /// </summary>
    /// @ingroup Core
    [Serializable]
    public class TrackingStateEventsProvider
    {
        private readonly string trackingAnchorName;

        [CanBeNull]
        private string currentState;

        /// <summary>
        ///  Event fired once after the tracking state changed to "tracked".
        /// </summary>
        public UnityEvent OnTracked = new UnityEvent();

        /// <summary>
        ///  Event fired once after the tracking state changed to "critical".
        /// </summary>
        public UnityEvent OnTrackingCritical = new UnityEvent();

        /// <summary>
        ///  Event fired once after the tracking state changed to "lost".
        /// </summary>
        public UnityEvent OnTrackingLost = new UnityEvent();

        public TrackingStateEventsProvider(string trackingAnchorName = "TrackedObject")
        {
            this.trackingAnchorName = trackingAnchorName;
        }

        public void EnableEvents()
        {
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;
            TrackingManager.OnTrackingStates += HandleTrackingStates;
        }

        public void DisableEvents()
        {
            TrackingManager.OnTrackingStates -= HandleTrackingStates;
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
        }

        private void OnTrackerInitialized()
        {
            this.currentState = null;
        }

        private void HandleTrackingStates(TrackingState state)
        {
            try
            {
                var objectState = state.objects.First(obj => obj.name == this.trackingAnchorName)
                    .state;
                HandleTrackingState(objectState);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(
                    "No tracking state for anchor \"" + this.trackingAnchorName + "\" found.");
            }
        }

        private void HandleTrackingState(string stateString)
        {
            if (this.currentState == stateString)
            {
                return;
            }
            switch (stateString)
            {
                case "tracked":
                {
                    this.OnTracked.Invoke();
                    break;
                }
                case "critical":
                {
                    this.OnTrackingCritical.Invoke();
                    break;
                }
                case "lost":
                {
                    this.OnTrackingLost.Invoke();
                    break;
                }
            }

            this.currentState = stateString;
        }

#if UNITY_EDITOR
        public List<SetupIssue> CheckForBrokenReferences(GameObject sourceObject)
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.OnTracked, this.OnTrackingCritical, this.OnTrackingLost},
                sourceObject);
        }
#endif
    }
}
