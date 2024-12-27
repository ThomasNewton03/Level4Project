using System;
using UnityEngine;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Stores the Data relevant for localization on HoloLens and allows setting it inside the native VisionLib SDK
    /// </summary>
    /// @ingroup HoloLens
    public abstract class HoloLensLocalizationHandle : IDisposable
    {
        public static HoloLensLocalizationHandle CreateLocalizationHandle()
        {
#if UNITY_WSA_10_0
#if VL_HL_XRPROVIDER_OPENXR
            return new HoloLensOpenXRCallbackHandle();
#elif VL_HL_XRPROVIDER_WINDOWSMR
            return new HoloLensGlobalCoordinateSystemHandle();
#else
            throw new Exception("No HoloLens XR Provider package installed. Please either install com.microsoft.mixedreality.openxr or com.unity.xr.windowsmr");
#endif
#else 
            throw new Exception("Calling CreateLocalizationHandle is currently only implemented on HoloLens");
#endif
        }

        public abstract Task SetLocalizationDataInVisionLibAsync(Worker worker);

        public void SetLocalizationDataInVisionLib(Worker worker, MonoBehaviour caller)
        {
#if !UNITY_EDITOR
            TrackingManager.CatchCommandErrors(SetLocalizationDataInVisionLibAsync(worker), caller);
#else
            Debug.LogWarning(
                "Executing a HoloLens scene in Editor might not work as expected. No LocalizationData could be set in VisionLib.", caller);
#endif
        }

        protected abstract void ReleaseNativeData();

        private bool disposed = false;

        ~HoloLensLocalizationHandle()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            ReleaseNativeData();
        }

        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }
    }
}
