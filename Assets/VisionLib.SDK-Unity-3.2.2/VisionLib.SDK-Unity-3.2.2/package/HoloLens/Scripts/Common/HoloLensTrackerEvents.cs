using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    /// Helper class to call frequent tracking events on HoloLens
    /// while trying to find the necessary components automatically.
    /// A `ComponentNotFoundException` will be thrown if a required component is not found in the scene.
    /// </summary>
    /// @ingroup HoloLens
    public static class HoloLensTrackerEvents
    {
        public class ComponentNotFoundException : Exception
        {
            public ComponentNotFoundException(string message)
                : base(message) {}
        }

        public static bool IsDebugImageActive
        {
            get
            {
                return FindComponent<DebugImage>().DebugImageEnabled;
            }
        }

        public static void StartTracking(TrackingConfiguration trackingConfiguration)
        {
            if (trackingConfiguration == null)
            {
                LogHelper.LogError(
                    "Can not start tracking because no `TrackingConfiguration` is set.");
                return;
            }
            trackingConfiguration.StartTracking();
        }

        public static void StopTracking()
        {
            TrackingManager.Instance.StopTracking();
        }

        public static void PauseTracking()
        {
            TrackingManager.Instance.PauseTracking();
        }

        public static void ResumeTracking()
        {
            TrackingManager.Instance.ResumeTracking();
        }

        public static void ResetTrackingSoft()
        {
            foreach (var trackingAnchor in UnityEngine.Object.FindObjectsOfType<TrackingAnchor>())
            {
                trackingAnchor.ResetSoft();
            }
        }

        public static void ResetTrackingHard()
        {
            foreach (var trackingAnchor in UnityEngine.Object.FindObjectsOfType<TrackingAnchor>())
            {
                trackingAnchor.ResetHard();
            }
            try
            {
                UnityEngine.Object.FindObjectOfType<PosterTracker>()?.ResetTrackingHard();
            }
            catch (ComponentNotFoundException)
            {
            }
        }

        public static void ShowDebugImage()
        {
            FindComponent<DebugImage>().DebugImageEnabled = true;
            FindComponent<TrackingAnchor>().SetShowLineModel(true);
        }

        public static void HideDebugImage()
        {
            FindComponent<DebugImage>().DebugImageEnabled = false;
            FindComponent<TrackingAnchor>().SetShowLineModel(false);
        }

        public static void ReadInitData(string initDataPath)
        {
            var initDataHandler = FindComponent<InitDataHandler>();
            if (!string.IsNullOrEmpty(initDataPath) && initDataHandler.initDataURI != initDataPath)
            {
                LogHelper.LogWarning(
                    $"The specified initDataPath differs from the one set in the InitDataHandler." +
                    "The URI set in the InitDataHandler will be used:\n" +
                    $"Specified: {initDataPath}\n" +
                    $"InitDataHandler: {initDataHandler.initDataURI}",
                    initDataHandler);
            }
            initDataHandler.ReadInitData();
        }

        public static void WriteInitData(string initDataPath)
        {
            var initDataHandler = FindComponent<InitDataHandler>();
            if (!string.IsNullOrEmpty(initDataPath) && initDataHandler.initDataURI != initDataPath)
            {
                LogHelper.LogWarning(
                    $"The specified initDataPath differs from the one set in the InitDataHandler." +
                    "The URI set in the InitDataHandler will be used:\n" +
                    $"Specified: {initDataPath}\n" + 
                    $"InitDataHandler: {initDataHandler.initDataURI}",
                    initDataHandler);
            }
            initDataHandler.WriteInitData();
        }

        public static void SetWideFieldOfView()
        {
#if UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR)
            TrackingManager.Instance.SetFieldOfView("wide");
#else
            throw new Exception(
                "No HoloLens XR Provider package installed. Please either install com.microsoft.mixedreality.openxr or com.unity.xr.windowsmr");
#endif
        }

        public static void SetNarrowFieldOfView()
        {
#if UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR)
            TrackingManager.Instance.SetFieldOfView("narrow");
#else
            throw new Exception(
                "No HoloLens XR Provider package installed. Please either install com.microsoft.mixedreality.openxr or com.unity.xr.windowsmr");
#endif
        }

        public static void EnablePlaneConstrainMode()
        {
            FindComponent<PlaneConstrainedMode>().enabled = true;
        }

        public static void DisablePlaneConstrainMode()
        {
            FindComponent<PlaneConstrainedMode>().enabled = false;
        }

        public static void PauseRecording()
        {
            FindComponent<ImageRecorder>().PauseRecording();
        }

        public static void ResumeCapturing()
        {
            FindComponent<ImageRecorder>().ResumeRecording();
        }

        private static T FindComponent<T>() where T : Component
        {
            T component = UnityEngine.Object.FindObjectOfType<T>();

            if (component == null)
            {
                throw new ComponentNotFoundException(typeof(T).ToString());
            }
            return component;
        }
    }
}
