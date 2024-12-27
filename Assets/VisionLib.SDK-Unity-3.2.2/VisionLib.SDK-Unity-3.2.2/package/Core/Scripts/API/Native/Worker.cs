using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  The Worker is a wrapper for an Worker object. The Worker object manages
    ///  the tracking thread.
    /// </summary>
    /// @ingroup Native
    public class Worker : IDisposable
    {
        // NOTICE: Make sure, that no exceptions escape from delegates, which are
        // called from unmanaged code
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void BoolCallback(bool data, IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void StringCallback(
            [MarshalAs(UnmanagedType.LPStr)] string message,
            IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void JsonStringCallback(
            [MarshalAs(UnmanagedType.LPStr)] string errorJson,
            [MarshalAs(UnmanagedType.LPStr)] string dataJson,
            IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void JsonStringAndBinaryCallback(
            [MarshalAs(UnmanagedType.LPStr)] string error,
            [MarshalAs(UnmanagedType.LPStr)] string result,
            IntPtr data,
            System.UInt32 dataSize,
            IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ImageWrapperCallback(IntPtr handle, IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ExtrinsicDataWrapperCallback(IntPtr handle, IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SimilarityTransformWrapperCallback(IntPtr handle, IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void IntrinsicDataWrapperCallback(IntPtr handle, IntPtr clientData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CalibratedImageWrapperCallback(IntPtr handle, IntPtr clientData);

        private IntPtr handle;
        private bool disposed = false;
        private bool owner;
        private bool synchronous = false;

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlNew_Worker();

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlNew_SyncWorker();

        /// <summary>
        ///  Constructor of Worker.
        /// </summary>
        public Worker(bool syncWorker = false)
        {
            this.synchronous = syncWorker;
            if (!this.synchronous)
            {
                this.handle = vlNew_Worker();
            }
            else
            {
                this.handle = vlNew_SyncWorker();
            }
            this.owner = true;
        }

        ~Worker()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        public bool GetDisposed()
        {
            return this.disposed;
        }

        [DllImport(VLSDK.dllName)]
        private static extern void vlDelete_Worker(IntPtr worker);

        private void Dispose(bool disposing)
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
            if (this.owner)
            {
                vlDelete_Worker(this.handle);
            }
            this.handle = IntPtr.Zero;

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="Worker"/>. The <see cref="Dispose"/> method leaves
        ///  the <see cref="Worker"/> in an unusable state. After calling
        ///  <see cref="Dispose"/>, you must release all references to the
        ///  <see cref="Worker"/> so the garbage collector can reclaim the
        ///  memory that the <see cref="Worker"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_Start(IntPtr worker);

        /// <summary>
        ///  Starts the tracking thread.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if the thread was started successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool Start()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_Start(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_Stop(IntPtr worker);

        /// <summary>
        ///  Stops the tracking thread.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if the thread was stopped successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool Stop()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            var stopSuccessful = vlWorker_Stop(this.handle);
            VLSDK.FlushLogBuffer();
            return stopSuccessful;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RunOnceSync(IntPtr worker);

        /// <summary>
        ///  Processes the enqueued commands and the tracking once.
        /// </summary>
        /// <remarks>
        ///  This function only works, if the Worker was created as synchronous
        ///  instance. The target number of FPS will get ignored. After calling
        ///  this function you should call Worker.ProcessCallbacks and
        ///  Worker.PollEvents to invoke callbacks and registered listeners.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, on success;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool RunOnceSync()
        {
            if (!this.synchronous)
            {
                LogHelper.LogWarning("RunOnceSync cannot be executed asynchronous");
                return false;
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RunOnceSync(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_SetLicenseFilePath(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string licenseFilePath);

        /// <summary>
        ///  Sets the path of the license file in the system.
        /// <remarks>
        ///   Calling of this function is mandatory for starting the tracking configuration.
        /// </remarks>
        /// </summary>
        /// <param name="path">The absolute location of the file.</param>
        /// <returns>
        ///  <c>true</c>, on success;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool SetLicenseFilePath(string licenseFilePath)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_SetLicenseFilePath(this.handle, licenseFilePath);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_SetLicenseFileData(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string licenseFileData);

        /// <summary>
        ///  Allows to inject the license data from memory
        /// </summary>
        /// <param name="data">String with the license data.</param>
        /// <returns>
        ///  <c>true</c>, on success;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool SetLicenseFileData(string licenseFileData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_SetLicenseFileData(this.handle, licenseFileData);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetLicenseInformation(IntPtr worker);

        /// <summary>
        ///  Retrieves the license information object.
        /// </summary>
        /// <returns>
        ///  <c>LicenseInformation</c>, if the license information was acquired successfully;
        ///  <c>null</c> otherwise.
        /// </returns>
        public LicenseInformation GetLicenseInformation()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            IntPtr trackingStateJsonStringPtr = vlWorker_GetLicenseInformation(this.handle);
            string trackingStateJson = Marshal.PtrToStringAnsi(trackingStateJsonStringPtr);
            VLSDK.ReleaseMemory(trackingStateJsonStringPtr);

            return JsonHelper.FromJson<LicenseInformation>(trackingStateJson);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_LoadPlugin(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string name);

        /// <summary>
        ///  Loads the Plugin with the given name.
        /// </summary>
        /// <param name="pluginName">The name of the plugin to load</param>
        /// <returns>
        ///  <c>true</c>, on success;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool LoadPlugin(string pluginName)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_LoadPlugin(this.handle, pluginName);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddCameraCalibrationDB(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string uri);

        /// <summary>
        ///  Adds a custom camera calibration database file.
        /// </summary>
        /// <remarks>
        ///  The calibration database must be added before loading a tracking
        ///  configuration.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the camera calibration database URI was added
        ///  successfully; <c>false</c> otherwise. <c>false</c> will also be
        ///  returned, if the URI was added already.
        /// </returns>
        /// <param name="uri">URI to the camera calibration database file</param>
        public bool AddCameraCalibrationDB(string uri)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }
            return vlWorker_AddCameraCalibrationDB(this.handle, uri);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetImageSync(IntPtr worker);

        /// <summary>
        ///  Returns a pointer to the camera image.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This function only works, if the Worker was created as synchronous
        ///   instance.
        ///  </para>
        ///  <para>
        ///   NOTICE: This functions is experimental and might get removed in
        ///   future.
        ///  </para>
        /// </remarks>
        /// <returns>
        ///  <c>VLImageWrapper</c>, on success;
        ///  <c>null</c>, otherwise.
        /// </returns>
        public Image GetImageSync()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("GetImageSync cannot be executed asynchronous");
                return null;
            }

            IntPtr imageHandle = vlWorker_GetImageSync(this.handle);
            if (imageHandle != IntPtr.Zero)
            {
                return new Image(imageHandle, false);
            }
            else
            {
                return null;
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_IsRunning(IntPtr worker);

        /// <summary>
        ///  Returns whether the thread is currently running or not.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if the thread is running;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool IsRunning()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_IsRunning(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_PushJsonCommand(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string jsonString,
            [MarshalAs(UnmanagedType.FunctionPtr)] JsonStringCallback callback,
            IntPtr clientData);

        /// <summary>
        ///  Enqueues a command for the tracking thread.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   A callback will called once after the processing has finished.
        ///  </para>
        ///  <para>
        ///   The different commands are defined inside the
        ///   <see cref="WorkerCommands"/> namespace.
        ///  </para>
        /// </remarks>
        /// <param name="cmd">
        ///  The command object.
        /// </param>
        /// <param name="callback">
        ///  Callback, which will be called inside <see cref="ProcessCallbacks"/>
        ///  after the command was processed.
        /// </param>
        /// <param name="clientData">
        ///  The callback function will be called with the given pointer value.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the command was enqueue successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool PushCommand(
            WorkerCommands.CommandBase cmd,
            JsonStringCallback callback,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            string jsonCmd = JsonHelper.ToJson(cmd);
            LogHelper.LogDebug("Pushed JsonCommand: " + jsonCmd);

            return vlWorker_PushJsonCommand(this.handle, jsonCmd, callback, clientData);
        }

        internal Task<string> PushCommandAsync(WorkerCommands.CommandBase cmd)
        {
            return Command.ExecuteAsync(this, cmd);
        }

        internal async Task<T> PushCommandAsync<T>(WorkerCommands.CommandBase cmd)
        {
            string result = await PushCommandAsync(cmd);
            return JsonHelper.FromJson<T>(string.IsNullOrEmpty(result) ? "{}" : result);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_ProcessCallbacks(IntPtr worker);

        /// <summary>
        ///  Executes all enqueued callbacks.
        /// </summary>
        /// <remarks>
        ///  Callbacks aren't called immediately from the tracking thread in
        ///  order to avoid synchronization problems. Instead this method should
        ///  be called regularly from the main thread.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the command was enqueue successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="PushCommand"/>
        public bool ProcessCallbacks()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_ProcessCallbacks(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_ProcessJsonCommandSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string jsonString,
            [MarshalAs(UnmanagedType.FunctionPtr)] JsonStringCallback callback,
            IntPtr clientData);

        /// <summary>
        ///  Executes a command synchronously.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   This command only works with a synchronous worker.
        ///  </para>
        ///  <para>
        ///   A callback will called once after the processing has finished.
        ///  </para>
        ///  <para>
        ///   The different commands are defined inside the
        ///   <see cref="WorkerCommands"/> namespace.
        ///  </para>
        /// </remarks>
        /// <param name="cmd">
        ///  The command object.
        /// </param>
        /// <param name="callback">
        ///  Callback, which will be called inside <see cref="ProcessCallbacks"/>
        ///  after the command was processed.
        /// </param>
        /// <param name="clientData">
        ///  The callback function will be called with the given pointer value.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the command was enqueue successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool ProcessCommand(
            WorkerCommands.CommandBase cmd,
            JsonStringCallback callback,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            string jsonCmd = JsonHelper.ToJson(cmd);
            LogHelper.LogDebug("Process JsonCommand: " + jsonCmd);

            return vlWorker_ProcessJsonCommandSync(this.handle, jsonCmd, callback, clientData);
        }
        
        internal string ProcessCommand(WorkerCommands.CommandBase cmd)
        {
            return Command.ExecuteSync(this, cmd);
        }

        internal T ProcessCommand<T>(WorkerCommands.CommandBase cmd)
        {
            var result = ProcessCommand(cmd);
            return JsonHelper.FromJson<T>(string.IsNullOrEmpty(result) ? "{}" : result);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddImageListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ImageWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for image events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an image event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool AddImageListener(ImageWrapperCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddImageListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveImageListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ImageWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from image events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool RemoveImageListener(ImageWrapperCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveImageListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddDebugImageListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ImageWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for debug image events.
        ///  DebugImageListeners are removed automatically, whenever the tracker
        ///  gets destroyed.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an debug image event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool AddDebugImageListener(ImageWrapperCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddDebugImageListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveDebugImageListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ImageWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from debug image events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool RemoveDebugImageListener(ImageWrapperCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveDebugImageListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddExtrinsicDataListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ExtrinsicDataWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for extrinsic data events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an extrinsic data event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="ExtrinsicData"/>
        public bool AddExtrinsicDataListener(
            ExtrinsicDataWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddExtrinsicDataListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveExtrinsicDataListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ExtrinsicDataWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from extrinsic data events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool RemoveExtrinsicDataListener(
            ExtrinsicDataWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveExtrinsicDataListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddIntrinsicDataListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntrinsicDataWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for intrinsic data events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an intrinsic data event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  argument.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="IntrinsicData"/>
        public bool AddIntrinsicDataListener(
            IntrinsicDataWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddIntrinsicDataListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveIntrinsicDataListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntrinsicDataWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from intrinsic data events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool RemoveIntrinsicDataListener(
            IntrinsicDataWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveIntrinsicDataListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddCalibratedImageListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] CalibratedImageWrapperCallback listener,
            IntPtr clientData,
            int format);

        /// <summary>
        ///  Registers a listener for calibrated image events of a specific format.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if a calibrated image event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  argument.
        /// </param>
        /// <param name="format">
        ///  The image format for which the listener is receiving events.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="CalibratedImage"/>
        public bool AddCalibratedImageListener(
            CalibratedImageWrapperCallback listener,
            IntPtr clientData,
            VLSDK.ImageFormat format)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddCalibratedImageListener(
                this.handle,
                listener,
                clientData,
                (int) format);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveCalibratedImageListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] CalibratedImageWrapperCallback listener,
            IntPtr clientData,
            int format);

        /// <summary>
        ///  Unregisters a listener from calibrated image events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <param name="format">
        ///  The image format for which the listener has been registered.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="CalibratedImage"/>
        public bool RemoveCalibratedImageListener(
            CalibratedImageWrapperCallback listener,
            IntPtr clientData,
            VLSDK.ImageFormat format)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveCalibratedImageListener(
                this.handle,
                listener,
                clientData,
                (int) format);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddTrackingStateListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for tracking state events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an tracking state event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool AddTrackingStateListener(StringCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddTrackingStateListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveTrackingStateListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from tracking state events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool RemoveTrackingStateListener(StringCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveTrackingStateListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddPerformanceInfoListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for performance information events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if a performance info state event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool AddPerformanceInfoListener(StringCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddPerformanceInfoListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemovePerformanceInfoListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from performance info events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="Image"/>
        public bool RemovePerformanceInfoListener(StringCallback listener, IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemovePerformanceInfoListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddWorldFromAnchorTransformListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string anchorName,
            [MarshalAs(UnmanagedType.FunctionPtr)] SimilarityTransformWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for anchor to world transform events.
        /// </summary>
        /// <param name="anchorName">
        ///  Name of the anchor which the listener should be getting the similarity transform
        ///  from.
        /// </param>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an similarity transform event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="SimilarityTransform"/>
        public bool AddWorldFromAnchorTransformListener(
            string anchorName,
            SimilarityTransformWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddWorldFromAnchorTransformListener(
                this.handle,
                anchorName,
                listener,
                clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveWorldFromAnchorTransformListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string anchorName,
            [MarshalAs(UnmanagedType.FunctionPtr)] SimilarityTransformWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from anchor to world transform events.
        /// </summary>
        /// <param name="anchorName">
        ///  Name of the anchor from which the listener should be removed.
        /// </param>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="SimilarityTransform"/>
        public bool RemoveWorldFromAnchorTransformListener(
            string anchorName,
            SimilarityTransformWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveWorldFromAnchorTransformListener(
                this.handle,
                anchorName,
                listener,
                clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_AddWorldFromCameraTransformListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ExtrinsicDataWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Registers a listener for world to camera transform events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which will be notified during the event processing,
        ///  if an world to camera transform event occurred.
        /// </param>
        /// <param name="clientData">
        ///  The listener function will be called with the given pointer value as
        ///  parameter.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was registered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="ExtrinsicData"/>
        public bool AddWorldFromCameraTransformListener(
            ExtrinsicDataWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_AddWorldFromCameraTransformListener(this.handle, listener, clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_RemoveWorldFromCameraTransformListener(
            IntPtr worker,
            [MarshalAs(UnmanagedType.FunctionPtr)] ExtrinsicDataWrapperCallback listener,
            IntPtr clientData);

        /// <summary>
        ///  Unregisters a listener from world to camera transform events.
        /// </summary>
        /// <param name="listener">
        ///  Listener which should be unregistered.
        /// </param>
        /// <param name="clientData">
        ///  Pointer value used as parameter during the registration of the
        ///  listener.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the listener was unregistered successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="ExtrinsicData"/>
        public bool RemoveWorldFromCameraTransformListener(
            ExtrinsicDataWrapperCallback listener,
            IntPtr clientData)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_RemoveWorldFromCameraTransformListener(
                this.handle,
                listener,
                clientData);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_PollEvents(IntPtr worker);

        /// <summary>
        ///  Calls the registered listeners for the enqueued events.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Listeners aren't called immediately from the tracking thread in
        ///   order to avoid synchronization problems. Instead this method should
        ///   be called regularly from the main thread.
        ///  </para>
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the events where processed successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <seealso cref="AddImageListener"/>
        /// <seealso cref="AddExtrinsicDataListener"/>
        /// <seealso cref="AddIntrinsicDataListener"/>
        public bool PollEvents()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_PollEvents(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_WaitEvents(IntPtr worker, System.UInt32 timeout);

        /// <summary>
        ///  Waits for enqueued events and calls the registered listeners.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Listeners aren't called immediately from the tracking thread in
        ///   order to avoid synchronization problems. Instead this method should
        ///   be called regularly from the main thread.
        ///  </para>
        /// </remarks>
        /// <param name="timeout">
        ///  Number of milliseconds before stopping to wait. Under normal
        ///  circumstances this shouldn't happen, but in case something went wrong,
        ///  we do not want to wait indefinitely.
        /// </param>
        /// <returns>
        ///  <c>true</c>, on success or <c>false</c>, if there was an error while
        ///  waiting for events. <c>false</c> is also returned, if the tracking
        ///  is enabled, but the timeout elapsed without an event arriving.
        /// </returns>
        /// <seealso cref="AddImageListener"/>
        /// <seealso cref="AddExtrinsicDataListener"/>
        /// <seealso cref="AddIntrinsicDataListener"/>
        public bool WaitEvents(uint timeout)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            return vlWorker_WaitEvents(this.handle, timeout);
        }

        // [DllImport (VLSDK.dllName)]
        // private static extern bool vlWorker_Lock(IntPtr worker);
        // public bool Lock()
        // {
        //     if (this.disposed)
        //     {
        //         throw new ObjectDisposedException("VLWorker");
        //     }

        //     return vlWorker_Lock(this.handle);
        // }

        // [DllImport (VLSDK.dllName)]
        // private static extern bool vlWorker_Unlock(IntPtr worker);
        // public bool Unlock()
        // {
        //     if (this.disposed)
        //     {
        //         throw new ObjectDisposedException("VLWorker");
        //     }

        //     return vlWorker_Unlock(this.handle);
        // }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_PushJsonAndBinaryCommand(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string jsonString,
            IntPtr binaryData,
            UInt32 binaryDataSize,
            [MarshalAs(UnmanagedType.FunctionPtr)] JsonStringAndBinaryCallback callback,
            IntPtr clientData);

        /// <summary>
        ///  Enqueues a command for the tracking thread using a JSON string and
        ///  binary data - THIS FUNCTION IS CONSIDERED AS BETA AND MAY BE MATTER OF CHANGE.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   The command gets processed asynchronously by the tracking thread and
        ///   a callback will called once after the processing has finished.
        ///   Since the memory is pinned until the callback is called in order to prevent multiple
        ///   copies, the static FreeBinaryMemory() function should be called from the callback, in
        ///   order to free allocated memory, which has eventually being passed to the function.
        ///  </para>
        /// </remarks>
        /// <param name="cmd">
        ///  A command containing a JSON string describing the binary data, the binary data itself
        ///  and the size of the binary data.
        /// </param>
        /// <param name="callback">
        ///  Callback, which will be called inside <see cref="ProcessCallbacks"/>
        ///  after the command was processed.
        /// </param>
        /// <param name="clientData">
        ///  The callback function will be called with the given pointer value.
        /// </param>
        /// <returns>
        ///  <c>true</c>, if the command was enqueue successfully;
        ///  <c>false</c> otherwise (usually some JSON syntax error).
        /// </returns>
        public bool PushCommand(
            WorkerCommands.JsonAndBinaryCommandBase cmd,
            JsonStringAndBinaryCallback callback,
            IntPtr clientData)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLWorker");
            }

            LogHelper.LogDebug("Pushed JsonAndBinaryCommand: " + cmd.jsonString);
            return vlWorker_PushJsonAndBinaryCommand(
                this.handle,
                cmd.jsonString,
                cmd.binaryData,
                cmd.binaryDataSize,
                callback,
                clientData);
        }

        internal Task<AsyncBinaryCommand.LabeledBinaryData> PushCommandAsync(
            WorkerCommands.JsonAndBinaryCommandBase cmd)
        {
            return AsyncBinaryCommand.Execute(this, cmd);
        }

        internal async Task<T> PushCommandAsync<T>(WorkerCommands.JsonAndBinaryCommandBase cmd)
        {
            using (var result = await PushCommandAsync(cmd))
            {
                if (result.data != IntPtr.Zero)
                {
                    LogHelper.LogWarning(
                        "Using PushCommandAsync with JsonAndBinaryCommand ignores binary result.");
                }
                return JsonHelper.FromJson<T>(result.jsonDescription);
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_SetNodeImageSync(
            IntPtr worker,
            IntPtr image,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public bool SetNodeImageSync(Image image, string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("SetNodeImageSync cannot be executed asynchronous.");
                return false;
            }

            return vlWorker_SetNodeImageSync(this.handle, image.getHandle(), node, key);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetNodeImageSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public Image GetNodeImageSync(string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("GetNodeImageSync cannot be executed asynchronous.");
                return null;
            }

            return new Image(vlWorker_GetNodeImageSync(this.handle, node, key), true);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_TryGetNodeImageSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public Image TryGetNodeImageSync(string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("TryGetNodeImageSync cannot be executed asynchronous.");
                return null;
            }
            var imageHandle = vlWorker_TryGetNodeImageSync(this.handle, node, key);
            if (imageHandle == IntPtr.Zero)
            {
                return null;
            }
            return new Image(imageHandle, true);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_SetNodeIntrinsicDataSync(
            IntPtr worker,
            IntPtr intrinsicData,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public bool SetNodeIntrinsicDataSync(IntrinsicData intrinsicData, string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("SetNodeIntrinsicDataSync cannot be executed asynchronous.");
                return false;
            }

            return vlWorker_SetNodeIntrinsicDataSync(
                this.handle,
                intrinsicData.getHandle(),
                node,
                key);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetNodeIntrinsicDataSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public IntrinsicData GetNodeIntrinsicDataSync(string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("GetNodeIntrinsicDataSync cannot be executed asynchronous.");
                return null;
            }

            return new IntrinsicData(
                vlWorker_GetNodeIntrinsicDataSync(this.handle, node, key),
                true);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_SetNodeExtrinsicDataSync(
            IntPtr worker,
            IntPtr extrinsicData,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public bool SetNodeExtrinsicDataSync(ExtrinsicData extrinsicData, string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("SetNodeExtrinsicDataSync cannot be executed asynchronous.");
                return false;
            }

            return vlWorker_SetNodeExtrinsicDataSync(
                this.handle,
                extrinsicData.getHandle(),
                node,
                key);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetNodeExtrinsicDataSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public ExtrinsicData GetNodeExtrinsicDataSync(string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("GetNodeExtrinsicDataSync cannot be executed asynchronous.");
                return null;
            }

            return new ExtrinsicData(
                vlWorker_GetNodeExtrinsicDataSync(this.handle, node, key),
                true);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlWorker_SetNodeSimilarityTransformSync(
            IntPtr worker,
            IntPtr SimilarityTransform,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public bool SetNodeSimilarityTransformSync(
            SimilarityTransform similarityTransform,
            string node,
            string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning(
                    "SetNodeSimilarityTransformSync cannot be executed asynchronous.");
                return false;
            }

            return vlWorker_SetNodeSimilarityTransformSync(
                this.handle,
                similarityTransform.getHandle(),
                node,
                key);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetNodeSimilarityTransformSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string node,
            [MarshalAs(UnmanagedType.LPStr)] string key);

        public SimilarityTransform GetNodeSimilarityTransformSync(string node, string key)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning(
                    "GetNodeSimilarityTransformSync cannot be executed asynchronous.");
                return null;
            }

            return new SimilarityTransform(
                vlWorker_GetNodeSimilarityTransformSync(this.handle, node, key),
                true);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetNodeTrackingStateJsonSync(
            IntPtr worker,
            [MarshalAs(UnmanagedType.LPStr)] string node);

        public TrackingState GetNodeTrackingStateSync(string node)
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            if (!this.synchronous)
            {
                LogHelper.LogWarning("GetNodeTrackingStateSync cannot be executed asynchronous.");
                return null;
            }

            IntPtr trackingStateJsonStringPtr =
                vlWorker_GetNodeTrackingStateJsonSync(this.handle, node);
            string trackingStateJson = Marshal.PtrToStringAnsi(trackingStateJsonStringPtr);
            VLSDK.ReleaseMemory(trackingStateJsonStringPtr);

            return JsonHelper.FromJson<TrackingState>(trackingStateJson);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlWorker_GetDeviceInfo(IntPtr worker);

        /// <summary>
        ///  Retrieves the device info object from the Worker.
        /// </summary>
        /// <returns>
        ///  <c>DeviceInfo</c>, if the device info was acquired successfully;
        ///  <c>null</c> otherwise.
        /// </returns>
        public DeviceInfo GetDeviceInfo()
        {
            if (this.GetDisposed())
            {
                throw new ObjectDisposedException("VLWorker");
            }

            IntPtr deviceInfoJsonStringPtr = vlWorker_GetDeviceInfo(this.handle);
            if (deviceInfoJsonStringPtr == IntPtr.Zero)
            {
                return null;
            }

            string deviceInfoJson = Marshal.PtrToStringAnsi(deviceInfoJsonStringPtr);
            VLSDK.ReleaseMemory(deviceInfoJsonStringPtr);

            return JsonHelper.FromJson<DeviceInfo>(deviceInfoJson);
        }
    }
}
