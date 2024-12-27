#if UNITY_WSA_10_0 && VL_HL_XRPROVIDER_OPENXR
using AOT;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Microsoft.MixedReality.OpenXR;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Stores the Data relevant for localization on HoloLens and allows setting it inside the native VisionLib SDK
    /// </summary>
    /// @ingroup HoloLens
    public class HoloLensOpenXRCallbackHandle : HoloLensLocalizationHandle
    {
        private static readonly SortedDictionary<Guid, SpatialGraphNode> guidDictionary =
            new SortedDictionary<Guid, SpatialGraphNode>();

        private static Pose GetPose(Guid id, long qpcTime)
        {
            if (!HoloLensOpenXRCallbackHandle.guidDictionary.ContainsKey(id))
            {
                HoloLensOpenXRCallbackHandle.guidDictionary.Add(
                    id,
                    SpatialGraphNode.FromDynamicNodeId(id));
            }
            if (!HoloLensOpenXRCallbackHandle.guidDictionary.ContainsKey(id))
            {
                LogHelper.LogError("Guid not found in dictionary");
                return Pose.identity;
            }
            if (!HoloLensOpenXRCallbackHandle.guidDictionary[id].TryLocate(qpcTime, out var pose))
            {
                LogHelper.LogError("Could not locate object at timestamp " + qpcTime);
            }
            return pose;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void PoseExtractionCallbackDelegate(
            Guid guid,
            Int64 qpcTime,
            IntPtr extrinsicData);

        [MonoPInvokeCallback(typeof(PoseExtractionCallbackDelegate))]
        private static void PoseExtractionCallback(
            Guid guid,
            Int64 qpcTime,
            IntPtr extrinsicDataPtr)
        {
            try
            {
                if (extrinsicDataPtr == IntPtr.Zero)
                {
                    LogHelper.LogError("ExtrinsicDataPtr is null");
                    return;
                }
                var extrinsicData = new ExtrinsicData(extrinsicDataPtr, false);
                var cameraToWorld = GetPose(guid, qpcTime);

                var cameraFromWorldUnity = new ModelTransform(
                    cameraToWorld.rotation,
                    cameraToWorld.position).Inverse();
                var cameraFromWorldVL = new ModelTransform(
                    CameraHelper.flipZ * cameraFromWorldUnity.ToMatrix() * CameraHelper.flipY);

                extrinsicData.SetR(cameraFromWorldVL.r);
                extrinsicData.SetT(cameraFromWorldVL.t);
                extrinsicData.SetValid(true);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static readonly PoseExtractionCallbackDelegate commandCallbackDelegate =
            PoseExtractionCallback;

        public override Task SetLocalizationDataInVisionLibAsync(Worker worker)
        {
            return ModelTrackerCommands.SetOpenXRCallbackAsync(worker, this.FunctionPointer);
        }

        private IntPtr functionPointer = IntPtr.Zero;
        private IntPtr FunctionPointer
        {
            get
            {
                if (this.functionPointer == IntPtr.Zero)
                {
                    this.functionPointer = Marshal.GetFunctionPointerForDelegate(
                        HoloLensOpenXRCallbackHandle.commandCallbackDelegate);
                }
                return this.functionPointer;
            }
        }

        protected override void ReleaseNativeData()
        {
            this.functionPointer = IntPtr.Zero;
        }
    }
}

#endif
