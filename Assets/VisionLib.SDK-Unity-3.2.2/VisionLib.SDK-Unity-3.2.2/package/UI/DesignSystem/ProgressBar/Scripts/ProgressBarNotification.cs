using UnityEngine;

namespace Visometry.DesignSystem
{
    /// <summary>
    /// Creates an info notification with a progress bar child.
    /// The info notification will be replaced by a success
    /// notification as soon as the progress bar's
    /// loading state turns to "Finished".
    /// </summary>
    public class ProgressBarNotification
    {
        private LinearProgressBar progressBar;
        private string notificationCategory;

        public ProgressBarNotification(
            string inProgressText,
            float minValue,
            float maxValue,
            string category)
        {
            this.progressBar = LinearProgressBar.Instantiate();
            this.progressBar.SetMinMaxValues(minValue, maxValue);
            this.progressBar.Value = minValue;

            this.notificationCategory = category;

            NotificationManager.AddNotification(
                new NotificationObject(
                    "Info",
                    inProgressText,
                    Notification.Kind.Info,
                    Notification.Type.Inline,
                    null,
                    this.notificationCategory,
                    this.progressBar.gameObject,
                    false,
                    1));
        }

        public float Value
        {
            get
            {
                return this.progressBar.Value;
            }
            set
            {
                this.progressBar.Value = value;
            }
        }

        public void Finish(string message)
        {
            NotificationManager.AddNotification(
                new NotificationObject(
                    "Success",
                    message,
                    Notification.Kind.Success,
                    Notification.Type.Inline,
                    null,
                    this.notificationCategory,
                    null,
                    true,
                    1));
        }

        public void Abort(string message)
        {
            NotificationManager.AddNotification(
                new NotificationObject(
                    "Info",
                    message,
                    Notification.Kind.Info,
                    Notification.Type.Inline,
                    null,
                    this.notificationCategory));
        }
    }
}
