using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details.Singleton;
#if NETFX_CORE
using System.Threading.Tasks;
#endif

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    public interface InputFrame : IDisposable
    {
        Frame Evaluate();
    }

    /// <summary>
    ///     A special type of <see cref="TrackingManager"/> for use in scenes where camera image
    ///     acquisition is not handled natively by the VisionLib, and camera images are
    ///     fed into the VisionLib via scripts instead (e.g. our ImageInjection
    ///     and AR Foundation examples).
    /// </summary>
    /// @ingroup Core
    [AddComponentMenu("VisionLib/Core/Synchronous Tracking Manager")]
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "synchronous_tracking_manager.html")]
    public class SynchronousTrackingManager : TrackingManager
    {
        private class DisposableTrackingResult : IDisposable
        {
            public readonly Frame frame;
            public readonly List<TrackingState> droppedPrecedingTrackingStates;
            private bool disposed = false;

            public DisposableTrackingResult(
                Frame frame,
                List<TrackingState> droppedPrecedingTrackingStates)
            {
                this.frame = frame;
                this.droppedPrecedingTrackingStates = droppedPrecedingTrackingStates;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }
                if (disposing)
                {
                    this.frame?.Dispose();
                }
                this.disposed = true;
            }
        }

        private class ThreadSafeResultStore : IDisposable
        {
            private readonly object lockObject = new object();

            private bool disposed = false;

            private Frame trackingResult;
            private readonly List<TrackingState> droppedTrackingStatesSinceLastPop =
                new List<TrackingState>();

            public void Push(Frame resultFrame)
            {
                lock (this.lockObject)
                {
                    if (this.trackingResult != null)
                    {
                        this.droppedTrackingStatesSinceLastPop.Add(this.trackingResult.trackingState);
                        this.trackingResult.Dispose();
                    }
                    this.trackingResult = resultFrame.Clone();
                }
            }

            public DisposableTrackingResult Pop()
            {
                lock (this.lockObject)
                {
                    var listCopy = new List<TrackingState>(this.droppedTrackingStatesSinceLastPop);
                    var trackingResultReference = this.trackingResult;

                    this.droppedTrackingStatesSinceLastPop.Clear();
                    this.trackingResult = null;

                    return new DisposableTrackingResult(
                        trackingResultReference,
                        listCopy);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }
                if (disposing)
                {
                    lock (this.lockObject)
                    {
                        this.trackingResult?.Dispose();
                    }
                }
                this.disposed = true;
            }

            public void Clear()
            {
                lock (this.lockObject)
                {
                    this.trackingResult?.Dispose();
                    this.trackingResult = null;
                    this.droppedTrackingStatesSinceLastPop.Clear();
                }
            }
        }

#if NETFX_CORE
        private class MyThread
        {
            private Task task;
            public MyThread(Action run)
            {
                this.task = new Task(run);
                this.task.Start();
            }
            public void Join()
            {
                this.task.Wait();
            }
        }
#else
        private class MyThread
        {
            private Thread thread;

            public MyThread(ThreadStart run)
            {
                this.thread = new Thread(run);
                this.thread.Start();
            }

            public void Join()
            {
                this.thread.Join();
            }
        }
#endif
        /// <summary>
        ///     Get a reference to the <see cref="SynchronousTrackingManager"/> in the scene.
        /// </summary>
        /// <remarks>
        ///     Usage:
        /// 
        ///     <c> var thisScenesSynchronousTrackingManager =
        ///         <see cref="SynchronousTrackingManager"/>.<see cref="Instance"/>;</c>
        ///
        ///     This raises a <see cref="WrongTypeException{SingletonType}"/> if the <see cref="TrackingManager"/>
        ///     in the current scene is not an <see cref="SynchronousTrackingManager"/>.
        /// </remarks>
        /// @exception WrongTypeException, NullSingletonException, DuplicateSingletonException
        public new static SynchronousTrackingManager Instance
        {
            get => TrackingManager.instance.As<SynchronousTrackingManager>();
        }

        /// <summary>
        /// Executes a command synchronously
        /// </summary>
        /// <param name="command"></param>
        /// <returns>result of the command</returns>
        public string Execute(WorkerCommands.CommandBase command)
        {
            return this.worker.ProcessCommand(command);
        }

        public T Execute<T>(WorkerCommands.CommandBase command)
        {
            return this.worker.ProcessCommand<T>(command);
        }

        public static void CatchCommandErrors(
            Func<WorkerCommands.CommandWarnings> task,
            MonoBehaviour caller = null)
        {
            try
            {
                TriggerWarnings(task(), caller);
            }
            catch (WorkerCommands.CommandError e)
            {
                var issue = e.GetIssue();
                issue.caller = caller;
                TriggerIssue(issue);
            }
            catch (TaskCanceledException) {}
        }

        public static void CatchCommandErrors(Action task, MonoBehaviour caller = null)
        {
            CatchCommandErrors(
                () =>
                {
                    task();
                    return WorkerCommands.NoWarnings();
                },
                caller);
        }

        private MyThread workerThread;

        private readonly FrameBuffer<InputFrame> frameToInject = new FrameBuffer<InputFrame>();
        private readonly ThreadSafeResultStore trackingResultStore = new ThreadSafeResultStore();

        private bool exitThread = true;

        protected override void Update()
        {
            base.Update();

            if (!this.trackerInitialized)
            {
                return;
            }

            using var trackingResult = this.trackingResultStore.Pop();
            foreach (var trackingState in trackingResult.droppedPrecedingTrackingStates)
            {
                EmitOnTrackingStatesWhenValid(trackingState);
            }
            if (trackingResult.frame == null)
            {
                return;
            }
            EmitEvents(trackingResult.frame);
        }

        protected override void OnDestroy()
        {
            StopTracking();
            this.frameToInject.Dispose();
            this.trackingResultStore.Dispose();
            base.OnDestroy();
        }

        public override void StartTracking(string path)
        {
            StopTracking();
            this.exitThread = false;
            base.StartTracking(path);
            StartWorkerThread();
        }

        public override void StopTracking()
        {
            if (this.workerThread != null)
            {
                StopWorkerThread();
                base.StopTracking();
            }
            this.frameToInject.Clear();
            this.trackingResultStore.Clear();
        }

        public bool IsReady()
        {
            return this.trackerInitialized;
        }

        public void Push(InputFrame frame)
        {
            this.frameToInject.Push(frame);
        }

        /// <summary>
        ///     Execute the tracking pipeline on the previously set <see cref="Frame"/>.
        /// </summary>
        private bool RunOnceSync()
        {
            return this.Worker.RunOnceSync();
        }

        /// <summary>
        ///     Execute the tracking pipeline on <see cref="frame"/> and return the result.
        /// </summary>
        private Frame TrackFrame(Frame frame)
        {
            lock (this.workerLock)
            {
                if (!this.Worker.IsRunning())
                {
                    return null;
                }

                InjectInputFrame(frame);

                RunOnceSync();

                if (!this.trackingRunning)
                {
                    return null;
                }
                var result = ExtractResultFrame();

                if (GetTrackerType() == "PosterTracker")
                {
                    result.image = frame.image;
                }
                result.timestamp = frame.timestamp;
                return result;
            }
        }

        private void InjectInputFrame(Frame frame)
        {
            this.Worker.SetNodeImageSync(frame.image, "inject0", "imageIn");
            if (frame.intrinsicData != null)
            {
                this.Worker.SetNodeIntrinsicDataSync(frame.intrinsicData, "inject0", "intrinsic");
            }
            this.Worker.SetNodeExtrinsicDataSync(frame.extrinsicData, "inject0", "extrinsic");
            CatchCommandErrors(
                () => Execute<WorkerCommands.CommandWarnings>(
                    new WorkerCommands.SetTimestampCmd(frame.timestamp)),
                this);
        }

        private Frame ExtractResultFrame()
        {
            Frame result = new Frame();

            foreach (var anchorName in TrackingManager.anchorObservableMap.GetAnchorNames())
            {
                var transform = this.Worker.GetNodeSimilarityTransformSync(
                    "",
                    "WorldFrom" + anchorName + "Transform");
                result.anchorTransforms[anchorName] = transform;
            }

            result.debugImage = this.Worker.TryGetNodeImageSync("", "DebugImage");
            result.intrinsicData =
                this.Worker.GetNodeIntrinsicDataSync("inject0", "intrinsicDisplay");

            if (GetTrackerType() == "PosterTracker")
            {
                result.anchorTransforms["TrackedObject"] =
                    this.Worker.GetNodeSimilarityTransformSync(
                        "",
                        "WorldFromTrackedObjectTransform");
                result.extrinsicData = this.Worker.GetNodeExtrinsicDataSync("", "extrinsic");
                result.cameraTransform =
                    this.Worker.GetNodeExtrinsicDataSync("", "worldFromCameraTransform");
            }
            else if (GetTrackerType() == "CameraCalibration")
            {
                result.extrinsicData = this.Worker.GetNodeExtrinsicDataSync("", "extrinsic");
                result.image = this.Worker.GetNodeImageSync("", "imageDisplay");
            }
            else
            {
                result.image = this.Worker.GetNodeImageSync("", "imageDisplay");
                result.extrinsicData = result.cameraTransform;
                result.cameraTransform =
                    this.Worker.GetNodeExtrinsicDataSync("", "worldFromCameraTransform");
            }

            result.trackingState = this.Worker.GetNodeTrackingStateSync("");
            return result;
        }

        protected override bool TryAddDebugImageListener()
        {
            return true;
        }

        protected override bool TryRemoveDebugImageListener()
        {
            return true;
        }

        protected override void CreateWorker()
        {
            this.worker = new Worker(true);
        }

        protected override void RegisterListeners() {}

        protected override void UnregisterListeners() {}

        protected override void RegisterTrackerListeners() {}

        protected override void UnregisterTrackerListeners() {}

        protected override void UpdateAnchorTransformListeners() {}

        private void StartWorkerThread()
        {
            this.workerThread = new MyThread(WorkerThreadRun);
        }

        private void StopWorkerThread()
        {
            this.exitThread = true;
            this.workerThread.Join();
            this.workerThread = null;
        }

        private void WorkerThreadRun()
        {
            while (!this.trackerInitialized && !this.exitThread)
            {
                this.RunOnceSync();
            }
            while (this.trackerInitialized && !this.exitThread)
            {
                WorkerThreadApply();
            }
        }

        private void WorkerThreadApply()
        {
            using (var frameToInject = this.frameToInject.Pop())
            {
                if (frameToInject == null)
                {
                    return;
                }
                try
                {
                    using (var frameToTrack = frameToInject.Evaluate())
                    {
                        if (frameToTrack == null)
                        {
                            return;
                        }
                        using (var newTrackingResult = this.TrackFrame(frameToTrack))
                        {
                            if (newTrackingResult == null)
                            {
                                return;
                            }
                            this.trackingResultStore.Push(newTrackingResult);
                        }
                    }
                }
                catch (InvalidOperationException e)
                {
                    LogHelper.LogException(e);
                }
            }
        }

        protected override bool ShouldShowMark()
        {
            return true;
        }

#if UNITY_EDITOR
        protected override List<SetupIssue> GetInputSourceIssues()
        {
            var configuration = FindObjectOfType<TrackingConfiguration>();
            if (!configuration || configuration.inputSource == TrackingConfiguration.InputSource.ImageInjection)
            {
                return SetupIssue.NoIssues();
            }
            return new List<SetupIssue>
            {
                new SetupIssue(
                    "SynchronousTrackingManager requires ImageInjection input source",
                    "To work correctly, the SynchronousTrackingManager requires a image source defined in Unity, providing images via the Push function. The images will be injected into VisionLib and therefore also require an ImageInjection input source. This can either be defined in the tracking configuration file or by setting the imageSource in the TrackingConfiguration to imageInjection.",
                    configuration.inputSource ==
                    TrackingConfiguration.InputSource.TrackingConfig
                        ? SetupIssue.IssueType.Info
                        : SetupIssue.IssueType.Error,
                    configuration.gameObject,
                    new ReversibleAction(
                        () =>
                        {
                            configuration.inputSource =
                                TrackingConfiguration.InputSource.ImageInjection;
                        },
                        configuration,
                        "Use ImageInjection as input source"))
            };
        }
#endif
    }
}
