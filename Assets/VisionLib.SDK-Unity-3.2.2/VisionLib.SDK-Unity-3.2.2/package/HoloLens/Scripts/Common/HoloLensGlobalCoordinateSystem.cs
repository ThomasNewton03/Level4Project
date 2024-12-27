using UnityEngine;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Synchronizes the Global Coordinate System between Unity and the native VisionLib SDK
    /// </summary>
    /// <remarks>
    ///  Right now this behaviour is also included in the HoloLensTracker.
    /// </remarks>
    /// @ingroup HoloLens
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "holo_lens_global_coordinate_system.html")]
    [AddComponentMenu("VisionLib/HoloLens/HoloLens Global Coordinate System")]
    public class HoloLensGlobalCoordinateSystem : MonoBehaviour
    {
        private HoloLensLocalizationHandle coordinateSystem = null;

        /// \deprecated The staticCoordinateSystem should not be used anymore
        [System.Obsolete("The staticCoordinateSystem should not be used anymore")]
        private static HoloLensLocalizationHandle staticCoordinateSystem = null;

        /// \deprecated The static function SetGlobalCoordinateSystemInVisionLibAsync is obsolete. Please use the HoloLensLocalizationHandle instead
        [System.Obsolete("The static function SetGlobalCoordinateSystemInVisionLibAsync is obsolete. Please use the HoloLensLocalizationHandle instead")]
        public static async Task SetGlobalCoordinateSystemInVisionLibAsync(Worker worker)
        {
            if (staticCoordinateSystem == null)
            {
                staticCoordinateSystem = HoloLensLocalizationHandle.CreateLocalizationHandle();
            }

            await staticCoordinateSystem.SetLocalizationDataInVisionLibAsync(worker);
        }

        /// \deprecated The static function SetGlobalCoordinateSystemInVisionLib is obsolete. Please use the HoloLensLocalizationHandle instead
        [System.Obsolete("The static function SetGlobalCoordinateSystemInVisionLib is obsolete. Please use the HoloLensLocalizationHandle instead")]
        public static void SetGlobalCoordinateSystemInVisionLib(Worker worker, MonoBehaviour caller)
        {
            if (staticCoordinateSystem == null)
            {
                staticCoordinateSystem = HoloLensLocalizationHandle.CreateLocalizationHandle();
            }

            staticCoordinateSystem.SetLocalizationDataInVisionLib(worker, caller);
        }

        private void Awake()
        {
            if (this.coordinateSystem == null)
            {
                this.coordinateSystem = HoloLensLocalizationHandle.CreateLocalizationHandle();
            }
        }

        private void OnDestroy()
        {
            this.coordinateSystem?.Dispose();
        }

        private void OnTrackerInitialized()
        {
#if !UNITY_EDITOR
            this.coordinateSystem.SetLocalizationDataInVisionLib(
                TrackingManager.Instance.Worker,
                this);
#else
            Debug.LogWarning(
                "Executing a HoloLens scene in Editor might not work as expected. No LocalizationData could be set in VisionLib.");
#endif
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
        }
    }
}
