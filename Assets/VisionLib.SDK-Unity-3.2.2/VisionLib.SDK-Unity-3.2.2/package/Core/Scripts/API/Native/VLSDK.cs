using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  Static class for storing the global functions of the VisionLib SDK for Unity.
    /// </summary>
    /// @ingroup Native
    public static class VLSDK
    {
#if UNITY_EDITOR
        // Use the plugin for the editor platform
#if UNITY_EDITOR_WIN
        public const string dllName = "vlSDK";
#elif UNITY_EDITOR_OSX
        public const string dllName = "libvlSDK";
#elif UNITY_EDITOR_LINUX
        public const string dllName = "vlSDK";
#else
#warning Unsupported editor platform
        public const string dllName = "vlSDK";
#endif

        // The plugin is in an architecture specific sub-directory before the
        // deployment
#if UNITY_EDITOR_32
        public const string subDir = "x86";
#elif UNITY_EDITOR_64
        public const string subDir = "x86_64";
#else
#warning Unsupported editor architecture
        public const string subDir = "x86_64";
#endif
#else
        // Use the plugin for the build target
#if UNITY_STANDALONE_WIN
        public const string dllName = "vlSDK";
#elif UNITY_STANDALONE_OSX
        public const string dllName = "libvlSDK";
#elif UNITY_IOS
        public const string dllName = "__Internal";
#elif UNITY_ANDROID
        public const string dllName = "vlSDK";
        // Ensures that the AssetManager is not garbage collected
        private static AndroidJavaObject assetManager;
#elif UNITY_WSA_10_0
        public const string dllName = "vlSDK";
#else
#warning Unsupported build target
        public const string dllName = "vlSDK";
#endif

        // During the deployment Unity will copy the plugin from the architecture
        // specific sub-directory into the plugin directory
        public const string subDir = "";
#endif

        /// <summary>
        ///  Log levels.
        /// </summary>
        public enum LogLevel : int
        {
            /// <summary>No logging.</summary>
            Mute = 0,
            /// <summary>Error level.</summary>
            Error = 1,
            /// <summary>Warning level.</summary>
            Warning = 2,
            /// <summary>Info level.</summary>
            Info = 3,
            /// <summary>Debug level.</summary>
            Debug = 4
        }

        public static int toVlLogLevel(LogLevel logLevel)
        {
            int[] vlLogLevelMapping = new int[] {0, 1, 2, 2, 3};
            return vlLogLevelMapping[(int) logLevel];
        }

        /// <summary>
        ///  Internal image formats.
        /// </summary>
        public enum ImageFormat
        {
            /// <summary>Unsupported image format</summary>
            Undefined = 0,
            /// <summary>Grey value image.</summary>
            Grey = 1,
            /// <summary>Image with a red, green and blue channel.</summary>
            RGB = 2,
            /// <summary>Image with a red, green, blue and alpha channel.</summary>
            RGBA = 3,
            /// <summary>Image with one float channel describing the distance in meters.</summary>
            Depth = 4
        };

        // NOTICE: Make sure, that no exceptions escape from delegates, which are
        // called from unmanaged code
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LogDelegate(
            [MarshalAs(UnmanagedType.LPStr)] string message,
            IntPtr clientData);

#if !UNITY_EDITOR && UNITY_ANDROID
        private static void EnsurePermission(string permission)
        {
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            {
                UnityEngine.Android.Permission.RequestUserPermission(permission);
            }
        }
#endif

        static VLSDK()
        {
            // Add the plugin directory to the PATH environment variable, so that
            // the operating system can find the DLLs

            string pluginPath = Application.dataPath + Path.DirectorySeparatorChar + "Plugins";

            // The plugins are in architecture specific sub-directories before the
            // deployment
#if UNITY_EDITOR
            pluginPath += Path.DirectorySeparatorChar + VLSDK.subDir;
#endif

#if !UNITY_WSA_10_0 // Environment not supported for WSA apps
            string oldPath = Environment.GetEnvironmentVariable(
                "PATH",
                EnvironmentVariableTarget.Process);
            if (!oldPath.Contains(pluginPath))
            {
                // Update the PATH variable (only for the current process)
                Environment.SetEnvironmentVariable(
                    "PATH",
                    pluginPath + Path.PathSeparator + oldPath,
                    EnvironmentVariableTarget.Process);
            }
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
            vlGetVersionMajor(); // this forces IL2CPP to load libvlSDK.so
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");

            // Instantiate an AssetManager and pass it to VisionLib
            VLSDK.assetManager = activity.Call<AndroidJavaObject>("getAssets");
            AndroidJavaClass platformClass =
                new AndroidJavaClass("org.instantreality.mini.Platform");
            platformClass.SetStatic<AndroidJavaObject>("context", activity);
            platformClass.SetStatic<AndroidJavaObject>("activity", activity);
            platformClass.CallStatic("init", assetManager);

            EnsurePermission(UnityEngine.Android.Permission.Camera);
#endif
            RegisterStreamingAssetsDir();
        }

        [DllImport(dllName)]
        private static extern System.UInt32 vlGetVersionMajor();

        /// <summary>
        ///  Returns the major version number of the VisionLib plugin.
        /// </summary>
        /// <returns>The major version number.</returns>
        public static uint GetMajorVersion()
        {
            return vlGetVersionMajor();
        }

        [DllImport(dllName)]
        private static extern System.UInt32 vlGetVersionMinor();

        /// <summary>
        ///  Returns the minor version number of the VisionLib plugin.
        /// </summary>
        /// <returns>The minor version number.</returns>
        public static uint GetMinorVersion()
        {
            return vlGetVersionMinor();
        }

        [DllImport(dllName)]
        private static extern System.UInt32 vlGetVersionRevision();

        /// <summary>
        ///  Returns the revision version number of the VisionLib plugin.
        /// </summary>
        /// <returns>The revision version number.</returns>
        public static uint GetRevisionVersion()
        {
            return vlGetVersionRevision();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlGetVersionString(StringBuilder version, System.UInt32 maxSize);

        /// <summary>
        ///  Returns the current version of the VisionLib plugin as a string.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if version was gotten, <c>false</c> otherwise.
        /// </returns>
        /// <param name="version">Output version string.</param>
        public static string GetVersionString()
        {
            StringBuilder sb = new StringBuilder(128);
            if (!vlGetVersionString(sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new InvalidOperationException("GetVersionString");
            }
            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlGetVersionPostfix(
            StringBuilder version,
            System.UInt32 maxSize);

        /// <summary>
        ///  Returns the current pre-release postfix of the VisionLib plugin as a string.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if pre-release postfix was gotten, <c>false</c> otherwise.
        /// </returns>
        /// <param name="version">Output pre-release postfix string.</param>
        public static string GetVersionPostfix()
        {
            StringBuilder sb = new StringBuilder(128);
            if (!vlGetVersionPostfix(sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new InvalidOperationException("GetVersionPostfix");
            }
            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlGetVersionHashString(
            StringBuilder hashString,
            System.UInt32 maxSize);

        /// <summary>
        ///  Returns the current version hash of the VisionLib plugin as a string.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if version hash was gotten, <c>false</c> otherwise.
        /// </returns>
        /// <param name="hashString">Output version hash string.</param>
        public static string GetVersionHashString()
        {
            StringBuilder sb = new StringBuilder(128);
            if (!vlGetVersionHashString(sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new InvalidOperationException("GetVersionHashString");
            }
            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlGetVersionTimestampString(
            StringBuilder versionTimestamp,
            System.UInt32 maxSize);

        /// <summary>
        ///  Returns the current build time of the VisionLib plugin as a string.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if version time stamp was gotten, <c>false</c> otherwise.
        /// </returns>
        /// <param name="versionTimestamp">Output version time stamp.</param>
        public static string GetVersionTimestampString()
        {
            StringBuilder sb = new StringBuilder(128);
            if (!vlGetVersionTimestampString(sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new InvalidOperationException("GetVersionTimestampString");
            }
            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlAddLogListener(
            [MarshalAs(UnmanagedType.FunctionPtr)] LogDelegate listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a log listener.
        /// </summary>
        /// <remarks>
        ///  Make sure, that no exceptions escape from the log listener as
        ///  this would collide with the exception handling of the unmanaged code.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if log listener was added successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <param name="listener">
        ///  Listener which will receive all VisionLib log messages.
        /// </param>
        /// <param name="clientData">
        ///  The listener will be notified with the given pointer value.
        /// </param>
        public static bool AddLogListener(LogDelegate listener, IntPtr clientData)
        {
            return vlAddLogListener(listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlRemoveLogListener(
            [MarshalAs(UnmanagedType.FunctionPtr)] LogDelegate listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregister the given log listener.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if log listener was removed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <param name="listener">
        ///  Listener, which should be removed.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value which was used during the registration.
        /// </param>
        public static bool RemoveLogListener(LogDelegate listener, IntPtr clientData)
        {
            return vlRemoveLogListener(listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlClearLogListeners();

        /// <summary>
        ///  Removes all log listeners.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if all log listener were removed successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        public static bool ClearLogListeners()
        {
            return vlClearLogListeners();
        }

        [DllImport(dllName)]
        private static extern void vlEnableLogBuffer();

        /// <summary>
        ///  Enables log buffering.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   If log buffering is enabled, then log messages will not get dispatched
        ///   immediately. Instead they will get buffered and one must continuously
        ///   call the vlFlushLogBuffer function in order to dispatch the buffered log
        ///   messages to the registered listeners.
        ///  </para>
        ///  <para>
        ///   This has the advantage, that registered listeners will not get notified
        ///   from some arbitrary thread, which would require proper thread
        ///   synchronization.
        ///  </para>
        ///  <para>
        ///   By default log buffering is disabled.
        ///  </para>
        /// </remarks>
        public static void EnableLogBuffer()
        {
            vlEnableLogBuffer();
        }

        [DllImport(dllName)]
        private static extern void vlDisableLogBuffer();

        /// <summary>
        ///  Disables log buffering.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   If log buffering is disabled, then log messages will get dispatched
        ///   immediately. This might happen from a different thread. Therefore one
        ///   must make sure, that registered log listeners are thread-safe.
        ///  </para>
        ///  <para>
        ///   With disabled log buffering, calling the vlFlushLogBuffer function is
        ///   not necessary.
        ///  </para>
        ///  <para>
        ///   By default log buffering is disabled.
        ///  </para>
        /// </remarks>
        public static void DisableLogBuffer()
        {
            vlDisableLogBuffer();
        }

        [DllImport(dllName)]
        private static extern void vlSetLogBufferSize(System.UInt32 maxEntries);

        /// <summary>
        ///  Sets the maximum number of log messages in the log buffer.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   If log buffering is enabled, then log messages will get buffered. In
        ///   order to not allocate too much memory, the size of the log buffer is
        ///   limited to a certain number of entries. If there are too many log
        ///   messages in the buffer, then the oldest message will get removed.
        ///  </para>
        ///  <para>
        ///   By default the maximum number of buffer entries is 32.
        ///  </para>
        /// </remarks>
        public static void SetLogBufferSize(uint maxEntries)
        {
            vlSetLogBufferSize(maxEntries);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlFlushLogBuffer();

        /// <summary>
        ///  Notifies registered log listeners of all buffered log messages.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   If log buffering is enabled, then log messages will not get dispatched
        ///   immediately. Instead they will get buffered and one must continuously
        ///   call the vlFlushLogBuffer function in order to dispatch the buffered log
        ///   messages to the registered listeners.
        ///  </para>
        ///  <para>
        ///   Failing to call vlFlushLogBuffer with enabled log buffering will have
        ///   the effect, that registered log listeners will not get notified of any
        ///   log messages and old log messages will be lost forever.
        ///  </para>
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, on success <c>false</c> otherwise.
        /// </returns>
        public static bool FlushLogBuffer()
        {
            return vlFlushLogBuffer();
        }

        [DllImport(dllName)]
        private static extern System.Int32 vlGetLogLevel();

        /// <summary>
        ///  Gets the current log level of the vlSDK.
        /// </summary>
        /// <returns>
        ///  The log level (0: log, 1: error, 2: warning, 3: debug)
        /// </returns>
        public static LogLevel GetLogLevel()
        {
            return (LogLevel) vlGetLogLevel();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSetLogLevel(System.Int32 level);

        /// <summary>
        ///  Sets the log level.
        /// </summary>
        /// <remarks>
        ///  It is recommended to set the log level during development to 2,
        ///  otherwise there will be too many messages. Only for debugging purposes
        ///  it might be useful to increase the log level. Before deploying your
        ///  application you should set the log level to 0 or 1.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if log level was set successfully,
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <param name="level">New log level (<see cref="GetLogLevel"/> for a
        /// description of the different levels)</param>
        public static bool SetLogLevel(LogLevel level)
        {
            return vlSetLogLevel(toVlLogLevel(level));
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSDKUtil_set(
            [MarshalAs(UnmanagedType.LPStr)] string uri,
            IntPtr data,
            int size,
            [MarshalAs(UnmanagedType.LPStr)] string options);

        /// <summary>
        /// Writes the given data to the given uri.
        /// </summary>
        /// <param name="uri">The data is written to this path.</param>
        /// <param name="data">Data to write.</param>
        public static void Set(string uri, string data)
        {
            IntPtr dataPtr = Marshal.StringToHGlobalAnsi(data);
            if (!vlSDKUtil_set(uri, dataPtr, data.Length, ""))
            {
                LogHelper.LogWarning("Could not write to " + uri);
            }
            Marshal.FreeHGlobal(dataPtr);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern IntPtr vlSDKUtil_get(
            [MarshalAs(UnmanagedType.LPStr)] string uri,
            out int size,
            [MarshalAs(UnmanagedType.LPStr)] string options);

        /// <summary>
        /// Loads data from the given uri.
        /// </summary>
        /// <param name="uri">File path from which to load data.</param>
        /// <returns></returns>
        public static string Get(string uri)
        {
            var receivedDataPtr = vlSDKUtil_get(uri, out var dataSize, "");

            if (receivedDataPtr == IntPtr.Zero)
            {
                return "";
            }

            return Marshal.PtrToStringAnsi(receivedDataPtr, dataSize);
        }


        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSDKUtil_fileExists(
            [MarshalAs(UnmanagedType.LPStr)] string uri);

        /// <summary>
        /// Checks if file exists at the given uri.
        /// </summary>
        /// <param name="uri">File path from which to load data.</param>
        /// <returns></returns>
        public static bool FileExists(string uri)
        {
            return vlSDKUtil_fileExists(uri);
        }

        [DllImport(VLSDK.dllName)]
        private static extern void vlReleaseBinaryBuffer(IntPtr data);

        /// <summary>
        /// Releases a data pointer acquired from the vision lib
        /// </summary>
        /// <param name="data">Data pointer, which should be released.</param>
        public static void ReleaseMemory(IntPtr data)
        {
            vlReleaseBinaryBuffer(data);
        }

        [DllImport(dllName)]
        private static extern double vlSDKUtil_getPosterQuality(IntPtr image);

        /// <summary>
        /// Estimates the quality of a given image as a PosterTracker reference.
        /// </summary>
        /// <param name="image">The image in any image format.
        /// <returns>
        /// The quality between 0 (bad) and 1 (good).
        /// </returns>
        public static double GetPosterQuality(Image image)
        {
            return vlSDKUtil_getPosterQuality(image.getHandle());
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlGetHostId(StringBuilder hostID, System.UInt32 maxSize);

        /// <summary>
        ///  Returns the host ID of the current machine as string.
        /// <remarks>
        ///  The host ID is necessary for generating a license file.
        /// </remarks>
        /// </summary>
        /// <returns>
        ///  Output host ID string.
        /// </returns>
        public static string GetHostID()
        {
            StringBuilder sb = new StringBuilder(512);
            if (!vlGetHostId(sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new System.ArgumentException("Stringbuilder has incorrect size");
            }

            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlGetBundleId(StringBuilder bundleID, System.UInt32 maxSize);

        /// <summary>
        ///  Returns the bundle ID of the current application as string.
        /// <remarks>
        ///  The bundle ID is necessary for generating a license file.
        /// </remarks>
        /// </summary>
        /// <returns>
        ///  Output bundle ID string.
        /// </returns>
        public static string GetBundleID()
        {
            StringBuilder sb = new StringBuilder(512);
            if (!vlGetBundleId(sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new System.ArgumentException("Stringbuilder has incorrect size");
            }

            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSDKUtil_registerScheme(
            [MarshalAs(UnmanagedType.LPStr)] string name,
            [MarshalAs(UnmanagedType.LPStr)] string uri);

        /// <summary>
        ///  Registers a new scheme for file access
        /// </summary>
        /// <param name="name">Name of the scheme..</param>
        /// <param name="uri">File path, where the data should be written to.</param>
        /// <returns>`true` on success; `false` on failure.</returns>
        public static bool RegisterScheme(string name, string uri)
        {
            return vlSDKUtil_registerScheme(name, uri);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSDKUtil_getModelHash(
            string modelURI,
            StringBuilder modelHash,
            System.UInt32 maxSize);

        /// <summary>
        /// Creates the model hash of the given model.
        /// </summary>
        /// <param name="modelURI">Path to the model
        /// <returns>
        /// Hash code of the model
        /// </returns>
        public static string GetModelHash(string modelURI)
        {
            StringBuilder sb = new StringBuilder(512);
            if (!vlSDKUtil_getModelHash(modelURI, sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new System.ArgumentException("Stringbuilder has incorrect size");
            }

            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSDKUtil_retrievePhysicalPath(
            string uri,
            StringBuilder path,
            uint maxSize);

        /// <summary>
        /// Returns the physical path of the given uri
        /// </summary>
        /// <param name="uri">VisionLib URI </param>
        /// <returns>
        /// Physical path
        /// </returns>
        public static string GetPhysicalPath(string uri)
        {
            var sb = new StringBuilder(512);
            if (!vlSDKUtil_retrievePhysicalPath(uri, sb, Convert.ToUInt32(sb.Capacity + 1)))
            {
                throw new ArgumentException("StringBuilder has incorrect size");
            }
            return sb.ToString();
        }

        /// <summary>
        ///  Delegate for events giving no feedback
        /// </summary>
        public delegate void VoidDelegate();

        private static void RegisterStreamingAssetsDir()
        {   
#if UNITY_ANDROID && !UNITY_EDITOR
            string streamingAssetsPath = "file:///android_asset/";
#else
            string streamingAssetsPath = Application.streamingAssetsPath;
#endif
            if (!RegisterScheme("streaming-assets-dir", streamingAssetsPath))
            {
                LogHelper.LogWarning(
                    "Could not register scheme 'streaming-assets-dir' with uri '" +
                    streamingAssetsPath + "'");
            }
        }
        
        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSDKUtil_getModelHashFromSerializedModel(
            [MarshalAs(UnmanagedType.LPStr)] string modelDescription,
            IntPtr data,
            uint size,
            StringBuilder modelHash,
            System.UInt32 maxSize);

        /// <summary>
        /// Creates the model hash of the given serialized model.
        /// </summary>
        /// <param name="serializedModel">Data and description of the serialized models></param>
        /// <returns>
        /// Hash code of the model
        /// </returns>
        public static string GetModelHash(SerializedModel serializedModel)
        {
            if (serializedModel == null)
            {
                throw new ArgumentNullException("SerializedModel was null");
            }

            var binaryDataHandle = GCHandle.Alloc(serializedModel.BinaryData, GCHandleType.Pinned);
            var data = binaryDataHandle.AddrOfPinnedObject();
            var dataLength = Convert.ToUInt32(serializedModel.DataSizeInBytes);

            StringBuilder sb = new StringBuilder(512);
            try
            {
                var description = JsonHelper.ToJson(serializedModel.serializedModelDataDescriptor);

                if (!vlSDKUtil_getModelHashFromSerializedModel(
                        description,
                        data,
                        dataLength,
                        sb,
                        Convert.ToUInt32(sb.Capacity + 1)))
                {
                    throw new ArgumentException("StringBuilder has incorrect size");
                }
            }
            finally
            {
                binaryDataHandle.Free();
            }

            return sb.ToString();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(dllName)]
        private static extern bool vlSDKUtil_getModelBoundingBox(
            [MarshalAs(UnmanagedType.LPStr)] string modelDescription,
            IntPtr modelBoundingBoxOut,
            System.UInt32 matrixElementCount);

        /// <summary>
        /// Retrieves the bounding box of a 3D model specified by its URI.
        /// </summary>
        /// <param name="modelURI">The URI of the 3D model.</param>
        /// <returns>The bounding box of the 3D model.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input <paramref name="modelURI"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the model URI cannot be resolved, or if an error occurs while receiving the bounding box.</exception>
        public static Bounds GetModelBoundingBox(string modelURI)
        {
            if (string.IsNullOrEmpty(modelURI))
            {
                throw new ArgumentNullException("modelURI is null or empty");
            }

            if (!FileExists(modelURI))
            {
                throw new ArgumentException($"Could not resolve modelURI \"{modelURI}\"");
            }

            var boundingBox = new float[6];
            var boundingBoxHandle = GCHandle.Alloc(boundingBox, GCHandleType.Pinned);
            if (!vlSDKUtil_getModelBoundingBox(
                    modelURI,
                    boundingBoxHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(boundingBox.Length)))
            {
                throw new ArgumentException("Error while receiving boundingBox.");
            }

            return new Bounds()
            {
                min = new Vector3(boundingBox[0], boundingBox[1], boundingBox[2]),
                max = new Vector3(boundingBox[3], boundingBox[4], boundingBox[5])
            };
        }
    }
}
