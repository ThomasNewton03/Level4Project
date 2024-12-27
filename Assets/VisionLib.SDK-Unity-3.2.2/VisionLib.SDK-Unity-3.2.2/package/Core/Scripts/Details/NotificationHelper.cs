using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Utility class to centralize the formatting of notifications from the vlUnitySDK.
    /// For internal use only.
    /// </summary>
    public static class NotificationHelper
    {
        public enum Kind
        {
            Error,
            Warning,
            Success,
            Info
        }

        public static VLSDK.LogLevel logLevel = VLSDK.LogLevel.Warning;

        public delegate void NotificationDelegate(NotificationHelper.Kind kind, string message);
        public static event NotificationDelegate OnNotification;

        public delegate void VoidDelegate();
        public static event VoidDelegate OnNotificationReset;

        /// <summary>
        /// Send error notification and log from the vlUnitySDK
        /// </summary>
        public static void SendError(object message, Object sourceObject = null)
        {
            SendError(message.ToString(), message, sourceObject);
        }

        /// <summary>
        /// Send error notification and log from the vlUnitySDK
        /// using a more detailed message for the console log
        /// </summary>
        public static void SendError(
            string notificationMessage,
            object logMessage,
            Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Error)
            {
                return;
            }
            LogHelper.LogError(logMessage, sourceObject);
            OnNotification?.Invoke(Kind.Error, notificationMessage);
        }

        /// <summary>
        /// Send warning notification and log from the vlUnitySDK
        /// </summary>
        public static void SendWarning(object message, Object sourceObject = null)
        {
            SendWarning(message.ToString(), message, sourceObject);
        }

        /// <summary>
        /// Send warning notification and log from the vlUnitySDK
        /// using a more detailed message for the console log
        /// </summary>
        public static void SendWarning(
            string notificationMessage,
            object logMessage,
            Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Warning)
            {
                return;
            }
            LogHelper.LogWarning(logMessage, sourceObject);
            OnNotification?.Invoke(Kind.Warning, notificationMessage);
        }

        /// <summary>
        /// Send success notification and log from the vlUnitySDK
        /// that appears if the logLevel is `Info` or higher
        /// </summary>
        public static void SendSuccess(object message, Object sourceObject = null)
        {
            SendSuccess(message.ToString(), message, sourceObject);
        }

        /// <summary>
        /// Send success notification and log from the vlUnitySDK
        /// that appears if the logLevel is `Info` or higher,
        /// using a more detailed message for the console log
        /// </summary>
        public static void SendSuccess(
            string notificationMessage,
            object logMessage,
            Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Info)
            {
                return;
            }
            LogHelper.LogInfo(logMessage, sourceObject);
            OnNotification?.Invoke(Kind.Success, notificationMessage);
        }

        /// <summary>
        /// Send info notification and log from the vlUnitySDK
        /// that appears if the logLevel is `Info` or higher
        /// </summary>
        public static void SendInfo(object message, Object sourceObject = null)
        {
            SendInfo(message.ToString(), message, sourceObject);
        }

        /// <summary>
        /// Send info notification and log from the vlUnitySDK
        /// that appear if the logLevel is `Info` or higher,
        /// using a more detailed message for the console log
        /// </summary>
        public static void SendInfo(
            string notificationMessage,
            object logMessage,
            Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Info)
            {
                return;
            }
            LogHelper.LogInfo(logMessage, sourceObject);
            OnNotification?.Invoke(Kind.Info, notificationMessage);
        }

        /// <summary>
        /// Clear current displayed notifications
        /// </summary>
        public static void ResetNotifications()
        {
            OnNotificationReset?.Invoke();
        }
    }
}
