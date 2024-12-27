using System;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Utility class to centralize the formatting of logs.
    /// For internal use only.
    /// </summary>
    public static class LogHelper
    {
        public static VLSDK.LogLevel logLevel = VLSDK.LogLevel.Warning;

        private static string prefix = "[VisionLib] ";

        public static void LogException(Exception e, UnityEngine.Object sourceObject = null)
        {
            Debug.LogException(e, sourceObject);
        }

        /// <summary>
        /// Log an error message if the logLevel is `Error` or higher
        /// </summary>
        public static void LogError(object message, UnityEngine.Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Error)
            {
                return;
            }
            Debug.LogError(LogHelper.prefix + message, sourceObject);
        }

        /// <summary>
        /// Log a warning message if the logLevel is `Warning` or higher
        /// </summary>
        public static void LogWarning(object message, UnityEngine.Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Warning)
            {
                return;
            }
            Debug.LogWarning(LogHelper.prefix + message, sourceObject);
        }

        /// <summary>
        /// Log an info message if the logLevel is `Info` or higher
        /// </summary>
        public static void LogInfo(object message, UnityEngine.Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Info)
            {
                return;
            }
            Debug.Log(LogHelper.prefix + message, sourceObject);
        }

        /// <summary>
        /// Log a debug message if the logLevel is `Debug` or higher
        /// </summary>
        public static void LogDebug(string message, UnityEngine.Object sourceObject = null)
        {
            if (logLevel < VLSDK.LogLevel.Debug)
            {
                return;
            }
            Debug.Log(LogHelper.prefix + message, sourceObject);
        }
    }
}
