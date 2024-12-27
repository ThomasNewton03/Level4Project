using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Shows notifications and log for VisionLib issues and tracking events.
    /// For internal use only.
    /// </summary>
    /// @ingroup Core
    public class NotificationAdapter
    {
        private IssuesToEventsAdapter issuesToEventsAdapter = new IssuesToEventsAdapter();

        public void ActivateNotifications()
        {
            this.issuesToEventsAdapter.RegisterToVLIssues();

            this.issuesToEventsAdapter.OnIssue += LogVLIssue;

            TrackingManager.OnTrackerInitializing += ResetNotifications;
        }

        public void DeactivateNotifications()
        {
            this.issuesToEventsAdapter.UnregisterFromVLIssues();

            this.issuesToEventsAdapter.OnIssue -= LogVLIssue;

            TrackingManager.OnTrackerInitializing -= ResetNotifications;
        }

        private void LogVLEvent(string log)
        {
            NotificationHelper.SendInfo(log);
        }

        private void LogVLIssue(
            Issue.IssueType issueType,
            string message,
            string details,
            MonoBehaviour caller = null)
        {
            if (message == null || message == "")
            {
                return;
            }

            message = message.Trim();
            details = details.Trim();

            string messageWithDetails = GetIssueWithDetails(message, details);

            if (issueType == Issue.IssueType.Error)
            {
                NotificationHelper.SendError(message, messageWithDetails, caller);
            }
            else if(issueType == Issue.IssueType.Warning)
            {
                NotificationHelper.SendWarning(message, messageWithDetails, caller);
            }
            else
            {
                NotificationHelper.SendInfo(message, messageWithDetails, caller);
            }
        }

        private string GetIssueWithDetails(string message, string details = "")
        {
            var messageWithDetails = message;
            if (details != "")
            {
                messageWithDetails += "\n\n" + details;
            }
            return messageWithDetails + "\n";
        }

        private void ResetNotifications()
        {
            NotificationHelper.ResetNotifications();
        }
    }
}
