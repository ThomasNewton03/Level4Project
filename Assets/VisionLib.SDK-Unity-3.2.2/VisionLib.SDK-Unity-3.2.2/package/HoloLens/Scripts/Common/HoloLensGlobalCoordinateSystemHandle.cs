#if UNITY_WSA_10_0 && VL_HL_XRPROVIDER_WINDOWSMR

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using UnityEngine.XR.WindowsMR;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Stores the Global Coordinate System and allows setting it inside the native VisionLib SDK
    /// </summary>
    /// @ingroup HoloLens
    public class HoloLensGlobalCoordinateSystemHandle : HoloLensLocalizationHandle
    {
        public override Task SetLocalizationDataInVisionLibAsync(Worker worker)
        {
            return ModelTrackerCommands.SetGlobalCoordinateSystemAsync(
                worker,
                this.GlobalCoordinateSystem);
        }

        private IntPtr globalCoordinateSystem = IntPtr.Zero;
        private IntPtr GlobalCoordinateSystem
        {
            get
            {
                if (this.globalCoordinateSystem != IntPtr.Zero)
                {
                    return this.globalCoordinateSystem;
                }

                this.globalCoordinateSystem = WindowsMREnvironment.OriginSpatialCoordinateSystem;
                if (this.globalCoordinateSystem == IntPtr.Zero)
                {
                    NotificationHelper.SendError("Failed to retrieve spatial coordinate system");
                }

                return this.globalCoordinateSystem;
            }
        }

        protected override void ReleaseNativeData()
        {
            if (this.globalCoordinateSystem == IntPtr.Zero)
            {
                return;
            }
            Marshal.Release(this.globalCoordinateSystem);
            this.globalCoordinateSystem = IntPtr.Zero;
        }
    }
}
#endif