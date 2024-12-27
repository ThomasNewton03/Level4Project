using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  The PosterTracker contains all functions, which are specific
    ///  for the PosterTracker.
    /// </summary>
    /// \deprecated The PosterTrackerLegacy uses the old tracking setup where only the camera moves. Use the PosterTracker instead.
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "poster_tracker_legacy.html")]
    [Obsolete("The PosterTrackerLegacy uses the old tracking setup where only the camera moves. Use the PosterTracker instead.")]
    public class PosterTrackerLegacy : MonoBehaviour, ISceneValidationCheck
    {
        [SerializeField]
        private TrackingStateEventsProvider trackingEvents = new TrackingStateEventsProvider();
        
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

        protected void OnEnable()
        {
            this.trackingEvents.EnableEvents();
        }
        
        protected void OnDisable()
        {
            this.trackingEvents.DisableEvents();
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return this.trackingEvents.CheckForBrokenReferences(this.gameObject);
        }
#endif
    }
}
