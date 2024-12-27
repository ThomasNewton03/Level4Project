using System;
using Visometry.DesignSystem;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.UI
{
    public static class DesignSystemNotificationAdapter
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            NotificationHelper.OnNotification += OnNotification;
            NotificationHelper.OnNotificationReset += NotificationManager.ResetNotifications;
        }

        private static Notification.Kind ToDesignSystem(NotificationHelper.Kind kind)
        {
            switch (kind)
            {
                case NotificationHelper.Kind.Error:
                    return Notification.Kind.Error;
                case NotificationHelper.Kind.Warning:
                    return Notification.Kind.Warning;
                case NotificationHelper.Kind.Success:
                    return Notification.Kind.Success;
                case NotificationHelper.Kind.Info:
                    return Notification.Kind.Info;
            }
            throw new ArgumentException("Unknown notification kind: " + kind);
        }

        private static void OnNotification(NotificationHelper.Kind kind, string message)
        {
            NotificationManager.AddSimpleNotification(ToDesignSystem(kind), message);
        }
    }
}
