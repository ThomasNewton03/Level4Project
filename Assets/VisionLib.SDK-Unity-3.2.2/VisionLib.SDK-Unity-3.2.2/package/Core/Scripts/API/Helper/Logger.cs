using AOT;
using System;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    public class Logger : IDisposable
    {
        private static string prefix = "[VisionLib-Native] ";

        [MonoPInvokeCallback(typeof(VLSDK.LogDelegate))]
        private static void DispatchLogCallback(string message, IntPtr clientData)
        {
            try
            {
                var splitMessage = message.Split(new char[] {':'}, 2);
                var logMessage = Logger.prefix + splitMessage[1].Trim();
                switch (splitMessage[0])
                {
                    case "ERROR":
                        LogHelper.LogError(logMessage);
                        break;
                    case "WARNING":
                        LogHelper.LogWarning(logMessage);
                        break;
                    case "DEBUG":
                        LogHelper.LogDebug(logMessage);
                        break;
                    default:
                        Debug.Log(message);
                        break;
                }
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static VLSDK.LogDelegate dispatchLogCallbackDelegate =
            new VLSDK.LogDelegate(DispatchLogCallback);

        protected bool disposed = false;

        public Logger()
        {
            if (!VLSDK.AddLogListener(dispatchLogCallbackDelegate, IntPtr.Zero))
            {
                LogHelper.LogWarning("Failed to add log listener");
            }
        }

        ~Logger()
        {
            // The finalizer was called implicitly from the garbage collector
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Prevent multiple calls to Dispose
            if (this.disposed)
            {
                return;
            }

            // Was dispose called explicitly by the user?
            if (disposing)
            {
                // Dispose managed resources (those that implement IDisposable)
            }

            // Clean up unmanaged resources
            if (!VLSDK.RemoveLogListener(dispatchLogCallbackDelegate, IntPtr.Zero))
            {
                LogHelper.LogWarning("Failed to remove log listener");
            }

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="Logger"/>. The <see cref="Dispose"/> method leaves
        ///  the <see cref="Logger"/> in an unusable state. After calling
        ///  <see cref="Dispose"/>, you must release all references to the
        ///  <see cref="Logger"/> so the garbage collector can reclaim the
        ///  memory that the <see cref="Logger"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        public void SetLogBufferSize(int maxEntries)
        {
            if (maxEntries < 0)
            {
                LogHelper.LogWarning("LogBufferSize must be zero or larger");
                return;
            }
            VLSDK.SetLogBufferSize(Convert.ToUInt32(maxEntries));
        }

        public void EnableLogBuffer()
        {
            VLSDK.EnableLogBuffer();
        }

        public void DisableLogBuffer()
        {
            VLSDK.DisableLogBuffer();
        }

        public bool FlushLogBuffer()
        {
            return VLSDK.FlushLogBuffer();
        }

        public VLSDK.LogLevel GetLogLevel()
        {
            return VLSDK.GetLogLevel();
        }

        public bool SetLogLevel(VLSDK.LogLevel level)
        {
            return VLSDK.SetLogLevel(level);
        }
    }
}
