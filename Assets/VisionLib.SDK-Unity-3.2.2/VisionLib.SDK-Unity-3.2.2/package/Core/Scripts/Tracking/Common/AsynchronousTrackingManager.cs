using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///     The default type of <see cref="TrackingManager"/> for use in scenes where camera image
    ///     acquisition is handled natively by the VisionLib.
    /// </summary>
    /// <remarks>
    ///     In scenes based on image injection (such as our ImageInjection and AR Foundation
    ///     examples), use a <see cref="SynchronousTrackingManager"/> instead.
    /// </remarks>
    /// @ingroup Core
    [AddComponentMenu("VisionLib/Core/Asynchronous Tracking Manager")]
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "asynchronous_tracking_manager.html")]
    public class AsynchronousTrackingManager : TrackingManager
    {
        /// <summary>
        ///     Get a reference to the <see cref="AsynchronousTrackingManager"/> in the scene.
        /// </summary>
        /// <remarks>
        ///     Usage:
        /// 
        ///     <c> var thisScenesAsynchronousTrackingManager =
        ///         <see cref="AsynchronousTrackingManager"/>.<see cref="Instance"/>;</c>
        ///
        ///     This raises a <see cref="WrongTypeException{SingletonType}"/> if the <see cref="TrackingManager"/>
        ///     in the current scene is not an <see cref="AsynchronousTrackingManager"/>.
        /// </remarks>
        /// @exception WrongTypeException, NullSingletonException, DuplicateSingletonException
        public new static AsynchronousTrackingManager Instance
        {
            get => TrackingManager.instance.As<AsynchronousTrackingManager>();
        }

        protected override void Awake()
        {
            AllocateGCHandle();
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DeallocateGCHandle();
        }

        protected override void CreateWorker()
        {
            this.worker = new Worker(false);
        }

        protected override void RegisterListeners()
        {
            if (!this.Worker.AddImageListener(
                    dispatchImageCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to add image listener");
            }
            if (!this.Worker.AddExtrinsicDataListener(
                    dispatchExtrinsicDataCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to add extrinsic data listener");
            }
            if (!this.Worker.AddIntrinsicDataListener(
                    dispatchIntrinsicDataCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to add intrinsic data listener");
            }
            if (!this.Worker.AddCalibratedImageListener(
                    dispatchCalibratedDepthImageCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle),
                    VLSDK.ImageFormat.Depth))
            {
                LogHelper.LogWarning("Failed to add depth image listener");
            }
            if (!this.Worker.AddTrackingStateListener(
                    dispatchTrackingStateCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to add tracking state listener");
            }
            if (!this.Worker.AddPerformanceInfoListener(
                    dispatchPerformanceInfoCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to add performance info listener");
            }
        }

        protected override void UnregisterListeners()
        {
            // Explicitly remove the listeners, so we know if everything went well
            if (!this.Worker.RemovePerformanceInfoListener(
                    dispatchPerformanceInfoCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to remove performance info listener");
            }
            if (!this.Worker.RemoveTrackingStateListener(
                    dispatchTrackingStateCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to remove tracking state listener");
            }
            if (!this.Worker.RemoveCalibratedImageListener(
                    dispatchCalibratedDepthImageCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle),
                    VLSDK.ImageFormat.Depth))
            {
                LogHelper.LogWarning("Failed to remove depth frame listener");
            }
            if (!this.Worker.RemoveIntrinsicDataListener(
                    dispatchIntrinsicDataCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to remove intrinsic data listener");
            }
            if (!this.Worker.RemoveExtrinsicDataListener(
                    dispatchExtrinsicDataCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to remove extrinsic data listener");
            }
            if (!this.Worker.RemoveImageListener(
                    dispatchImageCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to remove image listener");
            }
        }

        protected override void RegisterTrackerListeners()
        {
            this.worldFromCameraTransformListenerRegistered =
                this.Worker.AddWorldFromCameraTransformListener(
                    dispatchCameraTransformCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle));
        }

        protected override void UnregisterTrackerListeners()
        {
            UnregisterAllAnchorTransformListeners();
            if (this.worldFromCameraTransformListenerRegistered &&
                !this.Worker.RemoveWorldFromCameraTransformListener(
                    dispatchCameraTransformCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                LogHelper.LogWarning("Failed to remove extrinsic data slam listener");
            }
            this.worldFromCameraTransformListenerRegistered = false;
        }

        protected override void UpdateAnchorTransformListeners()
        {
            if (!this.trackerInitialized)
            {
                return;
            }
            anchorObservableMap.SynchronizeHandler(this.anchorTransformListeners, this.Worker);
        }

        protected override bool TryAddDebugImageListener()
        {
            return this.Worker.AddDebugImageListener(
                dispatchDebugImageCallbackDelegate,
                GCHandle.ToIntPtr(this.gcHandle));
        }

        protected override bool TryRemoveDebugImageListener()
        {
            return this.Worker.RemoveDebugImageListener(
                dispatchDebugImageCallbackDelegate,
                GCHandle.ToIntPtr(this.gcHandle));
        }

        private GCHandle gcHandle;

        private void AllocateGCHandle()
        {
            // Get a handle to the current object and make sure, that the object
            // doesn't get deleted by the garbage collector. We then use this
            // handle as client data for the native callbacks. This allows us to
            // retrieve the current address of the actual object during the
            // callback execution. GCHandleType.Pinned is not necessary, because we
            // are accessing the address only through the handle object, which gets
            // stored in a global handle table.
            this.gcHandle = GCHandle.Alloc(this);
        }

        private void DeallocateGCHandle()
        {
            this.gcHandle.Free();
        }

        private static AsynchronousTrackingManager GetInstance(IntPtr clientData)
        {
            return (AsynchronousTrackingManager) GCHandle.FromIntPtr(clientData).Target;
        }

        [MonoPInvokeCallback(typeof(Worker.ImageWrapperCallback))]
        protected static void DispatchImageCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).ImageHandler(handle);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.ImageWrapperCallback dispatchImageCallbackDelegate =
            new Worker.ImageWrapperCallback(DispatchImageCallback);

        [MonoPInvokeCallback(typeof(Worker.ImageWrapperCallback))]
        protected static void DispatchDebugImageCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).DebugImageHandler(handle);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.ImageWrapperCallback dispatchDebugImageCallbackDelegate =
            new Worker.ImageWrapperCallback(DispatchDebugImageCallback);

        [MonoPInvokeCallback(typeof(Worker.ExtrinsicDataWrapperCallback))]
        protected static void DispatchExtrinsicDataCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).ExtrinsicDataHandler(handle);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.ExtrinsicDataWrapperCallback dispatchExtrinsicDataCallbackDelegate =
            new Worker.ExtrinsicDataWrapperCallback(DispatchExtrinsicDataCallback);

        [MonoPInvokeCallback(typeof(Worker.ExtrinsicDataWrapperCallback))]
        protected static void DispatchCameraTransformCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).CameraTransformHandler(handle);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.ExtrinsicDataWrapperCallback dispatchCameraTransformCallbackDelegate =
            new Worker.ExtrinsicDataWrapperCallback(DispatchCameraTransformCallback);

        [MonoPInvokeCallback(typeof(Worker.IntrinsicDataWrapperCallback))]
        protected static void DispatchIntrinsicDataCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).IntrinsicDataHandler(handle);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.IntrinsicDataWrapperCallback dispatchIntrinsicDataCallbackDelegate =
            new Worker.IntrinsicDataWrapperCallback(DispatchIntrinsicDataCallback);

        [MonoPInvokeCallback(typeof(Worker.CalibratedImageWrapperCallback))]
        protected static void DispatchCalibratedDepthImageCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).CalibratedDepthImageHandler(handle);
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.CalibratedImageWrapperCallback
            dispatchCalibratedDepthImageCallbackDelegate =
                new Worker.CalibratedImageWrapperCallback(DispatchCalibratedDepthImageCallback);

        [MonoPInvokeCallback(typeof(Worker.StringCallback))]
        protected static void DispatchTrackingStateCallback(
            string trackingStateJson,
            IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).TrackingStateHandler(trackingStateJson);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.StringCallback dispatchTrackingStateCallbackDelegate =
            new Worker.StringCallback(DispatchTrackingStateCallback);

        [MonoPInvokeCallback(typeof(Worker.StringCallback))]
        private static void DispatchPerformanceInfoCallback(
            string performanceInfoJson,
            IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).PerformanceInfoHandler(performanceInfoJson);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.StringCallback dispatchPerformanceInfoCallbackDelegate =
            new Worker.StringCallback(DispatchPerformanceInfoCallback);

        private void ImageHandler(IntPtr handle)
        {
            using (Image image = new Image(handle, false))
            {
                EmitOnImage(image);
            }
        }

        private void DebugImageHandler(IntPtr handle)
        {
            using (Image debugImage = new Image(handle, false))
            {
                EmitOnDebugImage(debugImage);
            }
        }

        private void ExtrinsicDataHandler(IntPtr handle)
        {
            using (ExtrinsicData extrinsicData = new ExtrinsicData(handle, false))
            {
                EmitOnExtrinsicData(extrinsicData);
            }
        }

        private void CameraTransformHandler(IntPtr handle)
        {
            using (ExtrinsicData extrinsicData = new ExtrinsicData(handle, false))
            {
                EmitOnCameraTransform(extrinsicData);
            }
        }

        private void IntrinsicDataHandler(IntPtr handle)
        {
            using (IntrinsicData intrinsicData = new IntrinsicData(handle, false))
            {
                EmitOnIntrinsicData(intrinsicData);
            }
        }

        private void CalibratedDepthImageHandler(IntPtr handle)
        {
            using (CalibratedImage calibratedImage = new CalibratedImage(handle, false))
            {
                EmitOnCalibratedDepthImageWhenValid(calibratedImage);
            }
        }

        private void TrackingStateHandler(string trackingStateJson)
        {
            TrackingState state = JsonHelper.FromJson<TrackingState>(trackingStateJson);
            if (state != null)
            {
                EmitOnTrackingStatesWhenValid(state);
            }
        }

        private void PerformanceInfoHandler(string performanceInfoJson)
        {
            PerformanceInfo performanceInfo =
                JsonHelper.FromJson<PerformanceInfo>(performanceInfoJson);
            EmitOnPerformanceInfo(performanceInfo);
        }

        protected override bool ShouldShowMark()
        {
#if UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR)
            return true;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        protected override List<SetupIssue> GetInputSourceIssues() 
        {
            var configuration = FindObjectOfType<TrackingConfiguration>();
            if (configuration && configuration.inputSource == TrackingConfiguration.InputSource.ImageInjection)
            {
                return new List<SetupIssue>
                {
                    new SetupIssue(
                        "AsynchronousTrackingManager does not support ImageInjection",
                        "The AsynchronousTrackingManager acquires the (camera) image in its own tracking thread. Therefore the input source ImageInjection is not supported.",
                        SetupIssue.IssueType.Error,
                        configuration.gameObject,
                        new ISetupIssueSolution[]
                        {
                            new ReversibleAction(
                                () =>
                                {
                                    configuration.inputSource = TrackingConfiguration.InputSource
                                        .TrackingConfig;
                                },
                                configuration,
                                "Use default input source"),
                            new ReversibleAction(
                                () =>
                                {
                                    configuration.inputSource = TrackingConfiguration.InputSource
                                        .InputSelection;
                                },
                                configuration,
                                "Allow camera selection"),
                            new ReversibleAction(
                                () =>
                                {
                                    configuration.inputSource = TrackingConfiguration.InputSource
                                        .ImageSequence;
                                    Selection.activeGameObject = configuration.gameObject;
                                },
                                configuration,
                                "Use an image sequence as input source"),
                        })
                };
            }
            return SetupIssue.NoIssues();
        }
#endif
    }
}
