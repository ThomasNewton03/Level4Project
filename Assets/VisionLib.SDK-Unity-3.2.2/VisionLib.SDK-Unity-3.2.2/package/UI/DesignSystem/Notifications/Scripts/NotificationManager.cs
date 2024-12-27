using System;
using UnityEngine;

namespace Visometry.DesignSystem
{
    /// <summary>
    /// The NotificationManager is used to send events which initialize
    /// the instantiation of notifications.
    /// Add a `NotificationDisplay` component to a canvas or as its child
    /// to visualize notifications from the vlUnitySDK.
    /// </summary>
    public static class NotificationManager
    {
        public delegate void AddNotificationAction(NotificationObject notificationObject);
        public static event AddNotificationAction OnNotificationAdded;

        public delegate void ResetNotificationAction();
        public static event ResetNotificationAction OnNotificationReset;

        /// <summary>
        /// Invoke an event to initialize the given notification.
        /// </summary>
        public static void AddNotification(NotificationObject newNotification)
        {
            OnNotificationAdded?.Invoke(newNotification);
        }

        private static string ToString(Notification.Kind kind)
        {
            switch (kind)
            {
                case Notification.Kind.Error:
                    return "Error";
                case Notification.Kind.Warning:
                    return "Warning";
                case Notification.Kind.Success:
                    return "Success";
                case Notification.Kind.Info:
                    return "Info";
            }
            throw new ArgumentException("Unknown notification kind: " + kind);
        }

        public static void AddSimpleNotification(Notification.Kind kind, string message)
        {
            AddNotification(
                new NotificationObject(ToString(kind), message, kind, Notification.Type.Inline));
        }

        public static void ResetNotifications()
        {
            OnNotificationReset?.Invoke();
        }
    }
}
