using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.Details.Singleton;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Image = Visometry.VisionLib.SDK.Core.API.Native.Image;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///     The base class for all types of <see cref="TrackingManager"/>.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracking_manager.html")]
    public abstract class TrackingManager : MonoBehaviour, IParameterHandler, ISceneValidationCheck
    {
        public class WorkerNotFoundException : Exception
        {
            /// <summary>
            /// Exception that is thrown when the worker is tried to be accessed while it is null.
            /// This can happen when the worker is already destroyed or not created yet.
            /// </summary>
            public WorkerNotFoundException()
                : base(
                    "The Worker is accessed before it has been created or " +
                    "after it has been destroyed.") {}
        }

        public enum ImageStream
        {
            None,
            CameraImage,
            DebugImage,
            DepthImage
        }

        /// <summary>
        /// Object for adjusting tracker parameters.
        /// </summary>
        [SerializeReference]
        [HideInInspector]
        private TrackerRuntimeParameters parameters = null;

        protected static SingletonObjectReference<TrackingManager> instance =
            new SingletonObjectReference<TrackingManager>();

        public TrackerRuntimeParameters GetTrackerRuntimeParameters()
        {
            this.parameters ??= new TrackerRuntimeParameters();
            return this.parameters;
        }

        public void SetImageSourceEnabled(bool newValue)
        {
            CatchCommandErrors(
                GetTrackerRuntimeParameters().imageSourceEnabled.SetValueAsync(newValue, this),
                this);
        }

#if UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR)
        public void SetFieldOfView(string newValue)
        {
            CatchCommandErrors(
                GetTrackerRuntimeParameters().fieldOfView.SetValueAsync(newValue, this),
                this);
        }
#endif

        /// <summary>
        ///     Get a reference to the TrackingManager in the scene.
        /// </summary>
        /// <remarks>
        ///     Usage:
        /// 
        ///     <c> var thisScenesTrackingManager =
        ///         <see cref="TrackingManager"/>.<see cref="Instance"/>;</c>
        /// </remarks>
        public static TrackingManager Instance
        {
            get => TrackingManager.instance.Instance;
        }

        /// <summary>
        /// File uris will be treated as relative to this directory.
        /// Affected files include the tracking configuration, the license file
        /// and the calibration file.
        /// </summary>
        /// <remarks>
        /// The base directory is "streaming-assets-dir:VisionLib", since loading files from the
        /// <c>/StreamingAssets/</c> directory works on all platforms.
        /// </remarks>
        private static readonly string baseDir = "streaming-assets-dir:VisionLib";

        /// <summary>
        /// Path to the license file.
        /// </summary>
        [Tooltip("Path of the license file, e.g. 'streaming-assets-dir:VisionLib/license.xml.")]
        public LicenseFile licenseFile;

        /// <summary>
        ///  Target number of frames per second for the tracking thread.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   The tracking will run as fast as possible, if the value is zero or
        ///   less.
        ///  </para>
        ///  <para>
        ///   Higher values will result in a smoother tracking experience, but the
        ///   battery will be drained faster.
        ///  </para>
        /// </remarks>
        [Tooltip("Target number of frames per second for tracking.")]
        public int targetFPS = 30;
        private int lastTargetFPS = -1;

        /// <summary>
        ///  Whether to wait for tracking events.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   If <c>true</c>, the Update member function will wait until there is
        ///   at least one tracking event. This will limit the speed of the Unity
        ///   update cycler to the speed of the tracking, but the tracking will feel
        ///   more smooth, because the camera image will be shown with less delay.
        ///  </para>
        ///  <para>
        ///   If <c>false</c>, the speed of the tracking and the Unity update cycle
        ///   are largely separate. Due to the out of sync update rates, the camera
        ///   might be shown with a slight delay.
        ///  </para>
        /// </remarks>
        [Tooltip("Update only if there is at least one tracking event.")]
        public bool waitForEvents = false;

        /// <summary>
        ///  VisionLib log level.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Available log levels:
        ///   * 0: Mute
        ///   * 1: Fatal
        ///   * 2: Warning
        ///   * 3: Notice
        ///   * 4: Info
        ///   * 5: Debug
        ///  </para>
        ///  <para>
        ///   Log level N will disable all log messages with a level > N.
        ///  </para>
        /// </remarks>
        [Tooltip("Log Types")]
        public VLSDK.LogLevel logLevel = VLSDK.LogLevel.Warning;
        private VLSDK.LogLevel lastLogLevel;

        private BufferedLogger logger = null;

        public string calibrationDataBaseURI;

        protected Worker worker = null;
        protected object workerLock = new object();

        /// <summary>
        ///     Get a reference to the  <see cref="Worker"/> owned by a
        ///     <see cref="TrackingManager"/>.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///     Usage for direct access to the Worker in the current scene via the
        ///     <see cref="TrackingManager"/>'s static <see cref="Instance"/> property:
        /// 
        ///     <c> var thisScenesWorker =
        ///         <see cref="TrackingManager"/>.<see cref="Instance"/>.<see cref="Worker"/>;</c>
        ///  </para>
        ///  <para>
        ///     Usage for access via a reference to a specific <see cref="TrackingManager"/>:
        /// 
        ///     <c> var worker = someTrackingManager.<see cref="Worker"/>;</c>
        ///  </para>
        /// </remarks>
        public Worker Worker
        {
            get
            {
                if (this.worker == null)
                {
                    throw new WorkerNotFoundException();
                }
                return this.worker;
            }
        }

        protected bool trackingRunning = false;

        internal static AnchorObservableMap anchorObservableMap = new AnchorObservableMap();

        private NotificationAdapter notificationAdapter = new NotificationAdapter();

        private bool previousTrackerInitialized = false;
        protected bool trackerInitialized = false;

        protected bool worldFromCameraTransformListenerRegistered = false;

        private bool warnedAboutMissingDebugImage = false;
        private bool registeredDebugImageListener = false;
        private QueryHelper.DebugLevel? currentDebugLevel;

        private string trackerType = "";
        private string deviceType = "";

        private static readonly int supportedMajorVersion = 3;
        private static readonly int minimalMinorVersion = 0;

        protected Dictionary<string, IDisposable> anchorTransformListeners =
            new Dictionary<string, IDisposable>();

        private Dictionary<ImageStream, ImageStreamTexture> streamDictionary =
            new Dictionary<ImageStream, ImageStreamTexture>();

        private float timeUntilNextMark = 20.0f;
        private Text markingText;
        private IEnumerator lcnsRoutine;
        private LicenseInformation licenseInfo;
        private bool showMarking = false;

        protected virtual void Awake()
        {
            TrackingManager.instance.Instance = this;
            try
            {
                if (!IsMajorVersionSupported())
                {
                    LogHelper.LogError(
                        "This version of the VisionLib SDK for Unity may not work with " +
                        "the provided native VisionLib SDK version (" + VLSDK.GetVersionString() +
                        ").\n" + "Only major version " + supportedMajorVersion + " is supported.");
                }
                else if (!IsMinorVersionSupported())
                {
                    LogHelper.LogWarning(
                        "This version of the VisionLib SDK for Unity may not work with" +
                        " the provided native VisionLib SDK version (" + VLSDK.GetVersionString() +
                        ").\n" + "The following versions are supported: " + supportedMajorVersion +
                        "." + minimalMinorVersion + ".0 and higher");
                }
            }
            catch (InvalidOperationException)
            {
                LogHelper.LogWarning("Failed to get version strings");
            }
            TrackingAnchorHelper.MergeRootTrackingAnchorAndSlamCameraPosesOnce();
            Initialize();
        }

        protected virtual void OnEnable()
        {
            Initialize();

            if (this.trackerInitialized)
            {
                ResumeTracking();
            }

            this.notificationAdapter.ActivateNotifications();
            OnTrackerInitialized += InitializeLcns;
        }

        private void Start()
        {
            LogHelper.LogInfo(
                "VisionLib version: v" + VLSDK.GetVersionString() + " (" +
                VLSDK.GetVersionTimestampString() + ", " + VLSDK.GetVersionHashString() + ")");
        }

        protected virtual void Update()
        {
            // Log level changed?
            if (this.lastLogLevel != this.logLevel)
            {
                this.logger.SetLogLevel(this.logLevel);
                this.lastLogLevel = this.logLevel;
            }

            // Target FPS changed?
            if (this.lastTargetFPS != this.targetFPS)
            {
                SetFPS(this.targetFPS);
            }

            // Anchor Listeners changed?
            UpdateAnchorTransformListeners();

            ProcessCallbacks();

            this.logger.FlushLogBuffer();
        }

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_ANDROID)
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseTrackingInternal();
            }
            else if (this.trackingRunning)
            {
                ResumeTracking();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                PauseTrackingInternal();
            }
            else if (this.trackingRunning)
            {
                ResumeTracking();
            }
        }
#endif

        protected virtual void OnDisable()
        {
            OnTrackerInitialized -= InitializeLcns;

            this.previousTrackerInitialized = this.trackerInitialized;

            // Check for this.enabled will check whether the GameObject is about to be destroyed
            if (this.trackerInitialized && !this.enabled)
            {
                PauseTracking();
            }

            this.notificationAdapter.DeactivateNotifications();

            this.trackerInitialized = false;
        }

        protected virtual void OnDestroy()
        {
            UnregisterListeners();

            try
            {
                lock (this.workerLock)
                {
                    // Release the worker reference (this is necessary, because it
                    // references native resources)
                    this.Worker.Dispose();
                    this.worker = null;

                    // Release the log listener, because we will add a new one during the
                    // next call to Awake
                    this.logger.Dispose();
                    this.logger = null;
                }
            }
            catch (WorkerNotFoundException) {}
        }

        [Obsolete("Use the static function TrackingManager.DoesTrackerExistAndIsRunning instead.")]
        /// \deprecated Use the static function TrackingManager.DoesTrackerExistAndIsRunning instead.
        public bool GetTrackingRunning()
        {
            return this.trackingRunning;
        }

        /// <summary>
        ///  Returns true if the tracker is running.
        ///  If no instance of the tracking manager exists (yet / anymore) this will
        //// also return false and not throw an exception.
        /// </summary>
        public static bool DoesTrackerExistAndIsRunning()
        {
            try
            {
                return TrackingManager.Instance.trackingRunning;
            }
            catch (NullSingletonException)
            {
                return false;
            }
            catch (DuplicateSingletonException)
            {
                return false;
            }
        }

        /// <summary>
        /// This variable stores the state of the trackerInitialized variable before it is Disabled.
        /// </summary>
        /// <remarks>
        /// Some MonoBehaviours request the trackerInitialized state in their OnEnable function. If
        /// OnEnable is called for several GameObjects, the execution order can not be influenced.
        /// This is especially the case when recompiling during play mode. For this reason, we store
        /// the trackerInitialized state in this variable and set trackerInitialized to false in
        /// OnDisable. In OnEnable, we reset the value of trackerInitialized back to the original
        /// value. This way, any MonoBehaviour re-enabled before the TrackingManager will assume the
        /// TrackingManager as uninitialized.
        /// At the moment, if the scripts are recompiled, TrackingManager will be uninitialized
        /// afterwards.
        /// </remarks>
        /// \deprecated Use the static function TrackingManager.DoesTrackerExistAndIsInitialized instead.
        [Obsolete("Use the static function TrackingManager.DoesTrackerExistAndIsInitialized instead.")]
        public bool GetTrackerInitialized()
        {
            return this.trackerInitialized;
        }

        /// <summary>
        ///  Returns true if the tracker is initialized.
        ///  If no instance of the tracking manager exists (yet / anymore) this will
        //// also return false and not throw an exception.
        /// </summary>
        public static bool DoesTrackerExistAndIsInitialized()
        {
            try
            {
                return TrackingManager.Instance.trackerInitialized;
            }
            catch (NullSingletonException)
            {
                return false;
            }
            catch (DuplicateSingletonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the type of the loaded tracking pipeline.
        /// Works for tracking configurations loaded from a vl-file or vl-string.
        /// </summary>
        /// <returns>loaded tracker type</returns>
        public string GetTrackerType()
        {
            if (!this.trackerInitialized)
            {
                return "";
            }
            return this.trackerType;
        }

        /// <summary>
        /// Returns the type of the loaded device pipeline.
        /// Works for tracking configurations loaded from a vl-file or vl-string.
        /// </summary>
        /// <returns>loaded device type</returns>
        public string GetDeviceType()
        {
            if (!this.trackerInitialized)
            {
                return "";
            }
            return this.deviceType;
        }

        /// <summary>
        ///  Event which will be emitted once after calling the StartTracking
        ///  function.
        /// </summary>
        public static event VLSDK.VoidDelegate OnTrackerInitializing;

        /// <summary>
        ///  Event which will be emitted after the tracking configuration was
        ///  loaded.
        /// </summary>
        public static event VLSDK.VoidDelegate OnTrackerInitialized;

        /// <summary>
        ///  Event which will be emitted after the tracking was stopped or
        ///  the initialization of the tracker has failed.
        /// </summary>
        public static event VLSDK.VoidDelegate OnTrackerStopped;

        /// <summary>
        ///  Event which will be emitted once after the tracking was stopped or
        ///  paused and is now running again.
        /// </summary>
        public static event VLSDK.VoidDelegate OnTrackerRunning;

        /// <summary>
        ///  Event which will be emitted after the tracking was paused.
        /// </summary>
        public static event VLSDK.VoidDelegate OnTrackerPaused;

        /// <summary>
        ///  Event which will be emitted after a soft reset was executed.
        /// </summary>
        /// \deprecated The `OnTrackerResetSoft` event is obsolete. If you want to reset the tracking and perform some code afterwards, use the `ResetSoft` function returning a task.
        [System.Obsolete(
            "The `OnTrackerResetSoft` event is obsolete. If you want to reset the tracking" +
            " and perform some code afterwards," +
            " use the `ResetSoft` function returning a task.")]
        public static event VLSDK.VoidDelegate OnTrackerResetSoft;

        /// <summary>
        ///  Event which will be emitted after a hard reset was executed.
        /// </summary>
        /// \deprecated The `OnTrackerResetHard` event is obsolete. If you want to reset the tracking and perform some code afterwards, use the `ResetHard` function returning a task.
        [System.Obsolete(
            "The `OnTrackerResetHard` event is obsolete. If you want to reset the tracking" +
            " and perform some code afterwards," +
            " use the `ResetHard` function returning a task.")]
        public static event VLSDK.VoidDelegate OnTrackerResetHard;

        /// <summary>
        ///  Delegate for <see cref="OnTrackingStates"/> events.
        /// </summary>
        /// <param name="state">
        ///  <see cref="TrackingState"/> with information about the currently
        ///  tracked objects.
        /// </param>
        public delegate void TrackingStatesAction(TrackingState state);

        /// <summary>
        ///  Event with the current tracking state of all tracked objects. This
        ///  Event will be emitted for each tracking frame.
        /// </summary>
        public static event TrackingStatesAction OnTrackingStates;

        /// <summary>
        ///  Delegate for <see cref="OnPerformanceInfo"/> events.
        /// </summary>
        /// <param name="state">
        ///  <see cref="PerformanceInfo"/> with information about the performance.
        /// </param>
        public delegate void PerformanceInfoAction(PerformanceInfo state);

        /// <summary>
        ///  Event with the current tracking performance. This Event will be
        ///  emitted for each tracking frame.
        /// </summary>
        public static event PerformanceInfoAction OnPerformanceInfo;

        /// <summary>
        /// Delegate for <see cref="OnImage"/> events.
        /// </summary>
        /// <param name="image">
        ///  <see cref="API.Native.Image"/>.
        /// </param>
        public delegate void ImageAction(Image image);

        /// <summary>
        ///  Event with the current tracking image. This Event will be
        ///  emitted for each tracking frame.
        /// </summary>
        public static event ImageAction OnImage;

        /// <summary>
        ///  Event with the current debug image. This Event will be
        ///  emitted for each tracking frame, if debugLevel is at least 1
        /// </summary>
        public static event ImageAction OnDebugImage;

        /// <summary>
        /// Delegate for <see cref="OnExtrinsicData"/> events.
        /// </summary>
        /// <param name="extrinsicData">
        /// <see cref="ExtrinsicData"/>.
        /// </param>
        public delegate void ExtrinsicDataAction(ExtrinsicData extrinsicData);

        /// <summary>
        ///  Event with the current extrinsic data. This Event will be
        ///  emitted for each tracking frame.
        /// </summary>
        public static event ExtrinsicDataAction OnExtrinsicData;

        /// <summary>
        ///  Event with the current extrinsic data slam. This Event will be
        ///  emitted for each tracking frame.
        /// </summary>
        public static event ExtrinsicDataAction OnCameraTransform;

        /// <summary>
        /// Delegate for <see cref="OnIntrinsicData"/> events.
        /// </summary>
        /// <param name="intrinsicData">
        /// <see cref="IntrinsicData"/>.
        /// </param>
        public delegate void IntrinsicDataAction(IntrinsicData intrinsicData);

        /// <summary>
        ///  Event with the current intrinsic data. This Event will be
        ///  emitted for each tracking frame.
        /// </summary>
        public static event IntrinsicDataAction OnIntrinsicData;

        /// <summary>
        /// Delegate for <see cref="OnCalibratedImage"/> events.
        /// </summary>
        /// <param name="calibratedImage">
        /// <see cref="CalibratedImage"/>.
        /// </param>
        public delegate void CalibratedImageAction(CalibratedImage calibratedImage);

        /// <summary>
        ///  Event with the current calibrated depth image. This Event will be
        ///  emitted for each tracking frame.
        /// </summary>
        public static event CalibratedImageAction OnCalibratedDepthImage;

        /// <summary>
        ///  Delegate for <see cref="OnIssueTriggered"/> events.
        /// </summary>
        /// <param name="issue">
        /// <see cref="Issue"/>
        /// </param>
        public delegate void IssueTriggeredAction(Issue issue);

        /// <summary>
        ///  Event which will be emitted if an Issue was triggered
        /// </summary>
        public static event IssueTriggeredAction OnIssueTriggered;

        /// <summary>
        ///  Event which will be emitted once after the worker
        ///  has been created.
        /// </summary>
        public static event VLSDK.VoidDelegate OnWorkerCreated;

        public static EventWrapper<SimilarityTransform> AnchorTransform(string anchorName)
        {
            return anchorObservableMap.GetOrCreate(anchorName);
        }

        /// <summary>
        ///  Returns the owned Worker object.
        /// </summary>
        /// <returns>
        ///  Worker object or null, if the Worker wasn't initialized yet.
        /// </returns>
        /// \deprecated TrackingManager.GetWorker() is obsolete and slated for removal in the next major release. Use TrackingManager.Worker instead.
        [System.Obsolete(
            "TrackingManager.GetWorker() is obsolete and slated for removal in the" +
            " next major release. Use TrackingManager.Worker instead.")]
        public Worker GetWorker()
        {
            return this.Worker;
        }

        /// <summary>
        /// Check if the Worker has already been created. If you depend on the Worker, you can
        /// register to the OnWorkerCreated event, if this function returns false.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if the Worker can be used, <c>false</c> otherwise.
        /// </returns>
        public bool HasWorker()
        {
            return this.worker != null;
        }

        /// <summary>
        /// Adds the camera calibration DataBase using the URI. It will not be loaded at this point
        /// but only the possibility to add it will be checked. The loading of the actual database
        /// happens when starting the tracking pipe!
        ///
        /// </summary>
        /// <returns><c>true</c>, if camera calibration DB was added, <c>false</c>
        /// otherwise.</returns> <param name="uri">URI pointing to the camera calibration to be
        /// merged.</param>
        public bool AddCameraCalibrationDB(string uri)
        {
            return this.Worker.AddCameraCalibrationDB(MergeWithBaseDirIfRelative(uri));
        }

        /// <summary>
        ///  Start the tracking using a vl-file.
        /// </summary>
        /// <remarks>
        ///  The type of the tracker will be derived from the vl-file.
        /// </remarks>
        public virtual void StartTracking(string filename)
        {
            StopTracking();
            ApplyLicenseFilePath();

            if (this.calibrationDataBaseURI != "")
            {
                AddCameraCalibrationDB(this.calibrationDataBaseURI);
            }

            LogHelper.LogDebug("Tracker initializing");
            OnTrackerInitializing?.Invoke();

            this.Worker.Start();

            string trackingFile = MergeWithBaseDirIfRelative(filename);
            StartTracker(WorkerCommands.CreateTrackerAsync(this.worker, trackingFile));
        }

        /// <summary>
        ///  Start the tracking using a tracking configuration as string.
        /// </summary>
        /// <param name="trackingConfig">Tracking configuration as string</param>
        /// <param name="projectDir">Directory</param>
        /// <param name="overrideParameter"></param>
        public void StartTrackingFromString(
            string trackingConfig,
            string projectDir,
            string overrideParameter = null)
        {
            StopTracking();
            ApplyLicenseFilePath();

            if (this.calibrationDataBaseURI != "")
            {
                AddCameraCalibrationDB(this.calibrationDataBaseURI);
            }

            OnTrackerInitializing?.Invoke();

            if (String.IsNullOrEmpty(projectDir))
            {
                LogHelper.LogWarning(
                    "The project directory has not been set." +
                    " File references will be searched in " + baseDir);
                projectDir = baseDir;
            }

            this.Worker.Start();

            string basePathFileName = PathHelper.CombinePaths(projectDir, "FakeFileName.vl");
            if (!String.IsNullOrEmpty(overrideParameter) && !overrideParameter.StartsWith("?"))
            {
                basePathFileName += "?";
            }
            basePathFileName += overrideParameter;

            StartTracker(
                WorkerCommands.CreateTrackerAsync(this.worker, trackingConfig, basePathFileName));
        }

        /// <summary>
        ///  Stop the tracking (releases all tracking resources).
        /// </summary>
        public virtual void StopTracking()
        {
            var wasInitialized = this.trackerInitialized;
            this.Worker.Stop();
            this.trackingRunning = false;
            this.trackerInitialized = false;
            this.showMarking = false;
            DeinitializeImageStreams();
            UnregisterTrackerListeners();
            if (wasInitialized)
            {
                OnTrackerStopped?.Invoke();
                NotificationHelper.SendInfo("Tracker stopped");
            }
        }

        /// <summary>
        ///  Pause the tracking.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void PauseTracking()
        {
            CatchCommandErrors(PauseTrackingAsync(), this);
        }

        /// <summary>
        ///  Pause the tracking.
        /// </summary>
        public async Task PauseTrackingAsync()
        {
            this.trackingRunning = false;
            await PauseTrackingInternalAsync();
        }

        /// <summary>
        ///  Resume the tracking.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void ResumeTracking()
        {
            CatchCommandErrors(ResumeTrackingAsync(), this);
        }

        /// <summary>
        ///  Resume the tracking.
        /// </summary>
        public async Task ResumeTrackingAsync()
        {
            await RunTrackerAsync();
            NotificationHelper.SendInfo("Tracker resumed");
        }

        /// <summary>
        ///  Sets target number of frames per second for the tracking thread.
        /// </summary>
        public async Task SetFPSAsync(int newFPS)
        {
            if (trackerInitialized)
            {
                this.targetFPS = newFPS;
                this.lastTargetFPS = newFPS;
                await WorkerCommands.SetTargetFPSAsync(this.worker, newFPS);
                LogHelper.LogDebug("Set FPS to " + newFPS);
            }
        }

        /// <summary>
        /// Sets the frame rate of the tracking algorithm.
        /// This might limit the performance consumed by VisionLib.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="newFPS">Number of frames per second</param>
        public void SetFPS(int newFPS)
        {
            CatchCommandErrors(SetFPSAsync(newFPS), this);
        }

        /// \deprecated Do not use this function. It has only been introduced to support previous behaviour and will be removed in the future.
        [System.Obsolete(
            "Do not use this function. It has only been introduced to support previous" +
            " behaviour and will be removed in the future.")]
        public static void InvokeOnTrackerResetSoft()
        {
#pragma warning disable CS0618 // OnTrackerResetSoft is obsolete
            OnTrackerResetSoft?.Invoke();
#pragma warning restore CS0618 // OnTrackerResetSoft is obsolete
        }

        /// <summary>
        ///  Reset the tracking.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// \deprecated The `void ResetTrackingHard()` function of TrackingManager is obsolete. Use the reset functions of the active Tracker instead (e.g. on the TrackingAnchor).
        [System.Obsolete(
            "The `void ResetTrackingHard()` function of TrackingManager is obsolete." +
            " Use the reset functions of the active Tracker instead (e.g. on the TrackingAnchor).")]
        public void ResetTrackingSoft()
        {
            CatchCommandErrors(ResetTrackingSoftAsync(), this);
        }

        /// \deprecated Do not use this function. It has only been introduced to support previous behaviour and will be removed in the future.
        [System.Obsolete(
            "Do not use this function. It has only been introduced to support previous" +
            " behaviour and will be removed in the future.")]
        public static void InvokeOnTrackerResetHard()
        {
#pragma warning disable CS0618 // OnTrackerResetHard is obsolete
            OnTrackerResetHard?.Invoke();
#pragma warning restore CS0618 // OnTrackerResetHard is obsolete
        }

        /// <summary>
        ///  Reset the tracking and all key frames.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// \deprecated The `void ResetTrackingHard()` function of TrackingManager is obsolete. Use the reset functions of the active Tracker instead (e.g. on the TrackingAnchor).
        [System.Obsolete(
            "The `void ResetTrackingHard()` function of TrackingManager is obsolete." +
            " Use the reset functions of the active Tracker instead (e.g. on the TrackingAnchor).")]
        public void ResetTrackingHard()
        {
            CatchCommandErrors(ResetTrackingHardAsync(), this);
        }

        /// <summary>
        ///  Set <see cref="waitForEvents"/> to the given value.
        /// </summary>
        /// <remarks>
        ///  See <see cref="waitForEvents"/> for further information.
        /// </remarks>
        public void SetWaitForEvents(bool wait)
        {
            this.waitForEvents = wait;
        }

        /// <summary>
        /// Returns the device info, when the worker object has been initialized.
        /// You can call this function in order to get useful system information before starting the
        /// tracking pipe You might use this structure for retrieving the available cameras in the
        /// system.
        /// </summary>
        /// <returns>The device info object or null.</returns>
        public DeviceInfo GetDeviceInfo()
        {
            return this.Worker.GetDeviceInfo();
        }

        /// <summary>
        /// Returns the type of the loaded tracking pipeline.
        /// Works for tracking configurations loaded from a vl-file or vl-string.
        /// </summary>
        /// <param name="trackerType">loaded tracker type</param>
        /// <returns>returns true on success; false otherwise.</returns>
        /// \deprecated The `bool GetTrackerType(out string trackerType)` function is obsolete. Use `string GetTrackerType()` instead.
        [System.Obsolete(
            "The `bool GetTrackerType(out string trackerType)` function is obsolete." +
            " Use `string GetTrackerType()` instead.")]
        public bool GetTrackerType(out string trackerType)
        {
            trackerType = GetTrackerType();
            return trackerType != "";
        }

        public LicenseInformation GetLicenseInformation()
        {
            return this.Worker.GetLicenseInformation();
        }

        public static void EmitEvents(Frame frame)
        {
            if (frame.cameraTransform != null)
            {
                EmitOnCameraTransform(frame.cameraTransform);
            }
            if (frame.extrinsicData != null)
            {
                EmitOnExtrinsicData(frame.extrinsicData);
            }
            EmitOnIntrinsicData(frame.intrinsicData);
            EmitOnImage(frame.image);
            if (frame.debugImage != null)
            {
                EmitOnDebugImage(frame.debugImage);
            }
            EmitOnCalibratedDepthImageWhenValid(frame.calibratedDepthImage);
            EmitOnTrackingStatesWhenValid(frame.trackingState);

            anchorObservableMap.NotifyAll(frame.anchorTransforms);
        }

        private Task<WorkerCommands.CommandWarnings> SetDebugLevel(
            QueryHelper.DebugLevel debugLevel)
        {
            if (this.currentDebugLevel == debugLevel)
            {
                return Task.FromResult(WorkerCommands.NoWarnings());
            }

            this.currentDebugLevel = debugLevel;
            return WorkerCommands.SetAttributeAsync(
                TrackingManager.Instance.Worker,
                "debugLevel",
                ((int) debugLevel).ToString());
        }

        private async Task<WorkerCommands.CommandWarnings> ActivateDebugImageAsync()
        {
            var warnings = await SetDebugLevel(QueryHelper.DebugLevel.On);
            if (!this.registeredDebugImageListener && TryAddDebugImageListener())
            {
                this.registeredDebugImageListener = true;
            }
            return warnings;
        }

        private async Task<WorkerCommands.CommandWarnings> DeactivateDebugImageAsync()
        {
            var warnings = await SetDebugLevel(QueryHelper.DebugLevel.Off);
            if (this.registeredDebugImageListener && TryRemoveDebugImageListener())
            {
                this.registeredDebugImageListener = false;
            }
            return warnings;
        }

        public Texture2D GetStreamTexture(ImageStream streamType)
        {
            if (TrackerSupportsDebugImage())
            {
                CatchCommandErrors(
                    streamType == ImageStream.DebugImage
                        ? ActivateDebugImageAsync()
                        : DeactivateDebugImageAsync(),
                    this);
            }
            else if (streamType == ImageStream.DebugImage && !this.warnedAboutMissingDebugImage)
            {
                NotificationHelper.SendWarning(
                    GetTrackerType() + " does currently not support the use of debug images.");
                this.warnedAboutMissingDebugImage = true;
            }

            return this.streamDictionary.TryGetValue(streamType, out var stream)
                ? stream.GetTexture()
                : Texture2D.blackTexture;
        }

        /// <summary>
        /// This function should be used when awaiting a Task created anywhere within the
        /// vlUnitySDK. It will await the Task while treating all VisionLib specific errors which
        /// might arise from calling a command. Using this function also preserves the call stack,
        /// so you will still be able to identify the function which causes the logged error.
        /// </summary>
        /// <param name="task">Task which should be awaited.</param>
        /// <param name="caller">
        /// MonoBehaviour which should be referenced, when selecting error message in the log.
        /// </param>
        public static async void CatchCommandErrors(Task task, MonoBehaviour caller = null)
        {
            try
            {
                await task;
            }
            catch (WorkerCommands.CommandError e)
            {
                var issue = e.GetIssue();
                issue.caller = caller;
                TriggerIssue(issue);
            }
            catch (MonoBehaviourExtensions.MonoBehaviourDisabledDuringTaskException e)
            {
                NotificationHelper.SendWarning(e.Message, e.monoBehaviour);
            }
            catch (MonoBehaviourExtensions.MonoBehaviourRemovedDuringTaskException e)
            {
                NotificationHelper.SendWarning(e.Message, caller);
            }
            catch (TaskCanceledException) {}
        }

        /// <summary>
        /// This function should be used when awaiting a Task created anywhere within the
        /// vlUnitySDK. It will await the Task while treating all VisionLib specific errors which
        /// might arise from calling a command. Using this function also preserves the call stack,
        /// so you will still be able to identify the function which causes the logged error.
        /// </summary>
        /// <param name="task">Task which should be awaited.</param>
        /// <param name="caller">
        /// MonoBehaviour which should be referenced, when selecting error message in the log.
        /// </param>
        public static async void CatchCommandErrors(
            Task<WorkerCommands.CommandWarnings> task,
            MonoBehaviour caller = null)
        {
            try
            {
                var warnings = await task;
                TriggerWarnings(warnings, caller);
            }
            catch (WorkerCommands.CommandError e)
            {
                var issue = e.GetIssue();
                issue.caller = caller;
                TriggerIssue(issue);
            }
            catch (MonoBehaviourExtensions.MonoBehaviourDisabledDuringTaskException e)
            {
                NotificationHelper.SendWarning(e.Message, e.monoBehaviour);
            }
            catch (MonoBehaviourExtensions.MonoBehaviourRemovedDuringTaskException e)
            {
                NotificationHelper.SendWarning(e.Message, caller);
            }
            catch (TaskCanceledException) {}
        }

        protected static void TriggerWarnings(
            WorkerCommands.CommandWarnings warnings,
            MonoBehaviour caller = null)
        {
            if (warnings.warnings == null)
            {
                return;
            }
            foreach (var warning in warnings.warnings)
            {
                warning.caller = caller;
                warning.level = Issue.IssueType.Warning;
                TriggerIssue(warning);
            }
        }

        protected abstract void RegisterTrackerListeners();

        protected abstract void UnregisterTrackerListeners();

        protected abstract void RegisterListeners();

        protected abstract void UnregisterListeners();

        protected abstract void CreateWorker();

        protected abstract void UpdateAnchorTransformListeners();

        public void ResetAnchorTransformListener(string anchorName)
        {
            if (!this.anchorTransformListeners.ContainsKey(anchorName))
            {
                return;
            }
            this.anchorTransformListeners[anchorName].Dispose();
            this.anchorTransformListeners.Remove(anchorName);
        }

        protected void UnregisterAllAnchorTransformListeners()
        {
            foreach (var listenerKV in this.anchorTransformListeners)
            {
                listenerKV.Value.Dispose();
            }
            this.anchorTransformListeners.Clear();
        }

        protected static void EmitOnImage(Image image)
        {
            OnImage?.Invoke(image);
        }

        protected static void EmitOnDebugImage(Image debugImage)
        {
            OnDebugImage?.Invoke(debugImage);
        }

        protected static void EmitOnCalibratedDepthImageWhenValid(CalibratedImage calibratedImage)
        {
            if (calibratedImage != null)
            {
                OnCalibratedDepthImage?.Invoke(calibratedImage);
            }
        }

        protected static void EmitOnIntrinsicData(IntrinsicData intrinsicData)
        {
            OnIntrinsicData?.Invoke(intrinsicData);
        }

        protected static void EmitOnExtrinsicData(ExtrinsicData extrinsicData)
        {
            OnExtrinsicData?.Invoke(extrinsicData);
        }

        protected static void EmitOnCameraTransform(ExtrinsicData extrinsicData)
        {
            OnCameraTransform?.Invoke(extrinsicData);
        }

        protected static void EmitOnTrackingStatesWhenValid(TrackingState state)
        {
            if (state != null && state.objects != null)
            {
                OnTrackingStates?.Invoke(state);
            }
        }

        protected static void EmitOnPerformanceInfo(PerformanceInfo performanceInfo)
        {
            OnPerformanceInfo?.Invoke(performanceInfo);
        }

        protected void CreateLogger()
        {
            // Unity 2017 with Mono .NET 4.6 as scripting runtime version can't
            // properly handle callbacks from external threads. Until this is
            // fixed, we need to buffer the log messages and fetch them from the
            // main thread inside the update function.
            this.logger = new BufferedLogger(this.logLevel);

            this.lastLogLevel = this.logLevel;
        }

        protected static void TriggerIssue(Issue issue)
        {
            OnIssueTriggered?.Invoke(issue);
        }

        private string MergeWithBaseDirIfRelative(string uri)
        {
            if (!PathHelper.IsAbsolutePath(uri))
            {
                // Avoid breaking changes, especially for users that had specified
                // the license relative to StreamingAssets, e.g. "VisionLib/myLicense.xml"
                string pathWithoutVLPrefix = uri;
                if (uri.StartsWith("VisionLib/"))
                {
                    pathWithoutVLPrefix = uri.Substring("VisionLib/".Length);
                }

                string mergedUri = PathHelper.CombinePaths(baseDir, pathWithoutVLPrefix);
                LogHelper.LogWarning(
                    "Loading files relative to a base directory is deprecated." +
                    "\nUse an absolute path or an URI scheme instead, e.g. '" + mergedUri + "'.",
                    this);
                return mergedUri;
            }
            return uri;
        }

        /// <summary>
        /// Sets the path of the license file.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if a valid license file could be found;
        ///  <c>false</c> otherwise.</returns>
        private void ApplyLicenseFilePath()
        {
            if (this.licenseFile.path.Length > 0 && this.licenseFile.content.Length > 0)
            {
                LogHelper.LogWarning(
                    "Both license file and license data are specified. Using file: " +
                    this.licenseFile.path);
            }

            if (Regex.IsMatch(licenseFile.path, "\\*\\*.*\\*\\*"))
            {
                this.Worker.SetLicenseFilePath(this.licenseFile.path);
                return;
            }

            if (this.licenseFile.path.Length > 0)
            {
                this.Worker.SetLicenseFilePath(MergeWithBaseDirIfRelative(licenseFile.path));
            }
            else if (this.licenseFile.content.Length > 0)
            {
                this.Worker.SetLicenseFileData(this.licenseFile.content);
            }
            else
            {
                this.Worker.SetLicenseFilePath("");
            }
        }

        private async Task<WorkerCommands.CommandWarnings> StartTrackerAsync(
            Task<TrackerInfo> createTrackerTask)
        {
            try
            {
                var trackerInfo = await createTrackerTask;
                this.ThrowIfNotAliveAndEnabled();
                this.trackerType = trackerInfo.trackerType;
                this.deviceType = trackerInfo.deviceType;
                this.trackerInitialized = true;

                NotificationHelper.SendInfo("Tracker initialized");

                LogHelper.LogDebug("Tracker Type: " + this.trackerType);
                LogHelper.LogDebug("Device Type: " + this.deviceType);

                OnTrackerInitialized?.Invoke();

                await GetTrackerRuntimeParameters().UpdateParametersInBackendAsync(this);
                await SetFPSAsync(this.targetFPS);
                InitializeImageStreams();
                RegisterTrackerListeners();
                await RunTrackerAsync();
                return new WorkerCommands.CommandWarnings {warnings = trackerInfo.warnings};
            }
            catch (WorkerCommands.CommandError e)
            {
                this.trackerInitialized = false;
                var initIssue = e.GetIssue();
                initIssue.caller = this;
                TriggerIssue(initIssue);
                StopTracking();
                OnTrackerStopped?.Invoke();
            }
            catch (TaskCanceledException) {}
            return WorkerCommands.NoWarnings();
        }

        /// <summary>
        /// Starts the given tracker, after it was created.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="createTrackerTask">Task for creating the tracker.</param>
        private void StartTracker(Task<TrackerInfo> createTrackerTask)
        {
            CatchCommandErrors(StartTrackerAsync(createTrackerTask), this);
        }

        private async Task RunTrackerAsync()
        {
            await WorkerCommands.RunTrackingAsync(this.worker);
            this.ThrowIfNotAliveAndEnabled();
            this.trackingRunning = true;
            LogHelper.LogDebug("Tracker running");
            OnTrackerRunning?.Invoke();
        }

        private void RunTracker()
        {
            CatchCommandErrors(RunTrackerAsync(), this);
        }

        /// <summary>
        ///  Pause the tracking internal.
        ///  Does not modify the `trackingRunning` variable.
        /// </summary>
        private async Task PauseTrackingInternalAsync()
        {
            await WorkerCommands.PauseTrackingAsync(this.worker);
            OnTrackerPaused?.Invoke();
            NotificationHelper.SendInfo("Tracker paused");
        }

        /// <summary>
        /// Pause the tracking.
        /// Does not modify the `trackingRunning` variable.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        protected void PauseTrackingInternal()
        {
            CatchCommandErrors(PauseTrackingInternalAsync(), this);
        }

        protected async Task ResetTrackingSoftAsync()
        {
            await ModelTrackerCommands.ResetSoftAsync(this.worker);
#pragma warning disable CS0618 // OnTrackerResetSoft is obsolete
            InvokeOnTrackerResetSoft();
#pragma warning restore CS0618 // OnTrackerResetSoft is obsolete
            NotificationHelper.SendInfo("Tracker reset init pose");
        }

        protected async Task ResetTrackingHardAsync()
        {
            await ModelTrackerCommands.ResetHardAsync(this.worker);
#pragma warning disable CS0618 // OnTrackerResetHard is obsolete
            InvokeOnTrackerResetHard();
#pragma warning restore CS0618 // OnTrackerResetHard is obsolete
            NotificationHelper.SendInfo("Tracker reset");
        }

        protected abstract bool TryAddDebugImageListener();
        protected abstract bool TryRemoveDebugImageListener();

        private void ProcessCallbacks()
        {
            this.Worker.ProcessCallbacks();
            if (this.waitForEvents)
            {
                this.Worker.WaitEvents(1000);
            }
            else
            {
                this.Worker.PollEvents();
            }
        }

        private bool IsMajorVersionSupported()
        {
            return VLSDK.GetMajorVersion() == supportedMajorVersion;
        }

        private bool IsMinorVersionSupported()
        {
            return IsMajorVersionSupported() && VLSDK.GetMinorVersion() >= minimalMinorVersion;
        }

        private bool TrackerSupportsDebugImage()
        {
            var tracker = GetTrackerType();
            return tracker == "ModelTracker" || tracker == "PosterTracker" ||
                   tracker == "MultiModelTracker" || tracker == "CubeTracker";
        }

        private void InitializeImageStreams()
        {
            DeinitializeImageStreams();
            this.streamDictionary.Add(ImageStream.CameraImage, new CameraImageStreamTexture());
            if (TrackerSupportsDebugImage())
            {
                this.streamDictionary.Add(ImageStream.DebugImage, new DebugImageStreamTexture());
            }
            this.streamDictionary.Add(ImageStream.DepthImage, new DepthImageStreamTexture());
        }

        private void DeinitializeImageStreams()
        {
            this.warnedAboutMissingDebugImage = false;
            this.registeredDebugImageListener = false;
            foreach (var stream in this.streamDictionary)
            {
                stream.Value.DeInit();
            }
            this.streamDictionary.Clear();
            Resources.UnloadUnusedAssets();
        }

        protected virtual void Initialize()
        {
            if (this.worker != null)
            {
                this.trackerInitialized = this.previousTrackerInitialized;
                return;
            }

            GetTrackerRuntimeParameters();
            DeinitializeImageStreams();
            UnregisterAllAnchorTransformListeners();
            this.worldFromCameraTransformListenerRegistered = false;
            this.trackerInitialized = false;
            this.previousTrackerInitialized = false;
            this.trackingRunning = false;

            CreateLogger();
            CreateWorker();
            RegisterListeners();

            // fire the event at the end of the Awake() function
            OnWorkerCreated?.Invoke();
        }

        protected abstract bool ShouldShowMark();

        private Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("VLRuntimeCanvas");
            // If a VL scene is loaded additively to another scene, the background object
            // would appear in the first scene instead of the VL scene.
            // To fix that, the object needs to be moved to the VL scene here.
            SceneManager.MoveGameObjectToScene(canvasObject, this.gameObject.scene);
            var canvas = canvasObject.AddComponentUndoable<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.current;
            canvas.planeDistance = 0.6f;
            var scaler = canvasObject.AddComponentUndoable<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800, 600);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;

            return canvas;
        }

        private IEnumerator ShowMarking()
        {
            yield return new WaitForSecondsRealtime(10.0f);

            const float fadeTime = 3.0f;

            if (this.markingText == null)
            {
                var newGameObject = new GameObject("VisionLib " + this.licenseInfo.name);
                this.markingText = newGameObject.AddComponentUndoable<Text>();
                this.markingText.text = "VisionLib " + this.licenseInfo.GetLabel() +
                                        "\nexpires on " + this.licenseInfo.expirationDate + ". \n" +
                                        this.licenseInfo.customerContact + "\n" +
                                        this.licenseInfo.customerName;
                this.markingText.color = Color.clear;
#if UNITY_2022
                var newFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
                var newFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
                this.markingText.font = newFont;
                this.markingText.horizontalOverflow = HorizontalWrapMode.Overflow;
                newGameObject.transform.SetParent(CreateCanvas().transform, false);
            }

            while (this.showMarking)
            {
                yield return new WaitForSecondsRealtime(this.timeUntilNextMark);

                for (var t = 0.01f; t < fadeTime; t += Time.deltaTime)
                {
                    this.markingText.color = Color.Lerp(
                        Color.clear,
                        Color.white,
                        Mathf.Min(1, t / fadeTime));
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(5.0f);

                for (var t = 0.01f; t < fadeTime; t += Time.deltaTime)
                {
                    this.markingText.color = Color.Lerp(
                        Color.white,
                        Color.clear,
                        Mathf.Min(1, t / fadeTime));
                    yield return null;
                }
            }
        }

        private void TriggerLcnsRoutine()
        {
            if (this.lcnsRoutine != null)
            {
                StopCoroutine(this.lcnsRoutine);
            }

            this.lcnsRoutine = ShowMarking();
            StartCoroutine(this.lcnsRoutine);
        }

        private void InitializeLcns()
        {
            if (!ShouldShowMark())
            {
                return;
            }

            this.licenseInfo = GetLicenseInformation();
            if (this.licenseInfo.watermark)
            {
                if (this.licenseInfo.name == "trialLicense")
                {
                    this.timeUntilNextMark = 8.0f;
                }
                this.showMarking = true;
                TriggerLcnsRoutine();
            }
        }

        public bool ActiveInBackend()
        {
            return DoesTrackerExistAndIsInitialized();
        }

        public Task<WorkerCommands.CommandWarnings> SetParameterAsync(
            string parameterName,
            string parameterValue)
        {
            return WorkerCommands.SetAttributeAsync(
                TrackingManager.Instance.Worker,
                parameterName,
                parameterValue);
        }

        public async Task<T> GetParameterAsync<T>(string parameterName)
        {
            return await WorkerCommands.GetAttributeAsync<T>(
                TrackingManager.Instance.Worker,
                parameterName);
        }

#if UNITY_EDITOR
        protected abstract List<SetupIssue> GetInputSourceIssues();

        public virtual List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new UnityEventBase[]
                {
#if UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR)
                    GetTrackerRuntimeParameters().fieldOfView.onValueChanged,
#endif
                    GetTrackerRuntimeParameters().imageSourceEnabled.onValueChanged
                },
                this.gameObject).Concat(GetInputSourceIssues()).ToList();
            ;
        }
#endif
    }
}
