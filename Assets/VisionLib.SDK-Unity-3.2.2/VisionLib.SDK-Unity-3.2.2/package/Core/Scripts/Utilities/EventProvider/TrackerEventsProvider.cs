using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This behaviour fires UnityEvents for static TrackingManager events.
    /// </summary>
    /// <remarks>
    ///  This could for example be used to activate / disable certain GameObjects
    ///  depending on if the tracker has been initialized or stopped.
    /// </remarks>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracker_events_provider.html")]
    [AddComponentMenu("VisionLib/Core/Tracker Events Provider")]
    public class TrackerEventsProvider : MonoBehaviour, ISceneValidationCheck
    {
        /// <summary>
        ///  Event fired when the tracker has been initialized.
        /// </summary>
        public UnityEvent trackerInitialized = new UnityEvent();

        /// <summary>
        ///  Event fired when the tracker has been stopped".
        /// </summary>
        public UnityEvent trackerStopped = new UnityEvent();

        void HandleTrackerInitialized()
        {
            this.trackerInitialized.Invoke();
        }

        void HandleTrackerStopped()
        {
            this.trackerStopped.Invoke();
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackerInitialized += HandleTrackerInitialized;
            TrackingManager.OnTrackerStopped += HandleTrackerStopped;
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackerInitialized -= HandleTrackerInitialized;
            TrackingManager.OnTrackerStopped -= HandleTrackerStopped;
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.trackerInitialized, this.trackerStopped},
                this.gameObject);
        }
#endif
    }
}
