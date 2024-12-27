using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Class providing functions to use the internal memory of uEye cameras.
    ///  You can use them to save or load your own configurations. Additionally,
    ///  provides the possibility to configure your uEye camera with the IDS camera
    ///  manager and load your settings in VisionLib.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "u_eye_camera_parameters.html")]
    [AddComponentMenu("VisionLib/Core/UEye Camera Parameters")]
    public class UEyeCameraParameters : MonoBehaviour, ISceneValidationCheck
    {
        [SerializeField]
        private UEyeCameraExposureParameter exposure = new UEyeCameraExposureParameter();
        [SerializeField]
        private UEyeCameraParameter gain = new UEyeCameraParameter("imageSource.gain");
        [SerializeField]
        private UEyeCameraParameter blackLevel = new UEyeCameraParameter("imageSource.blackLevel");
        [SerializeField]
        private UEyeCameraParameter gamma = new UEyeCameraParameter("imageSource.gamma");
        private readonly UEyeCameraColorModeParameter colorMode = new UEyeCameraColorModeParameter();

        public void OnEnable()
        {
            TrackingManager.OnTrackerInitialized += ReadFromBackend;
            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                ReadFromBackend();
            }
        }
        
        public void OnDisable()
        {
            TrackingManager.OnTrackerInitialized -= ReadFromBackend;
        }

        public void SetExposure(float exposureValue)
        {
            TrackingManager.CatchCommandErrors(this.exposure.SetAsync(exposureValue), this);
        }

        public void SetGain(float gainValue)
        {
            TrackingManager.CatchCommandErrors(this.gain.SetAsync(gainValue), this);
        }

        public void SetBlackLevel(float blackLevelValue)
        {
            TrackingManager.CatchCommandErrors(this.blackLevel.SetAsync(blackLevelValue), this);
        }

        public void SetGamma(float gammaValue)
        {
            TrackingManager.CatchCommandErrors(this.gamma.SetAsync(gammaValue), this);
        }

        public void SetColorModeBGR()
        {
            TrackingManager.CatchCommandErrors(
                this.colorMode.SetAsync(UEyeCameraColorModeParameter.ColorMode.BGR),
                this);
        }

        public void SetColorModeMonochrome()
        {
            TrackingManager.CatchCommandErrors(
                this.colorMode.SetAsync(UEyeCameraColorModeParameter.ColorMode.Monochrome),
                this);
        }

        public void SetColorModeBayerFormat()
        {
            TrackingManager.CatchCommandErrors(
                this.colorMode.SetAsync(UEyeCameraColorModeParameter.ColorMode.BayerFormat),
                this);
        }

        public async Task LoadParametersFromEEPROMAsync()
        {
            await UEyeCameraCommands.LoadParametersFromEEPROMAsync(TrackingManager.Instance.Worker);
            await ReadFromBackendAsync();
            NotificationHelper.SendInfo("Loaded UEye parameters from EEPROM");
        }

        /// <summary>
        ///  Load uEye camera settings form internal memory.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void LoadParametersFromEEPROM()
        {
            TrackingManager.CatchCommandErrors(LoadParametersFromEEPROMAsync(), this);
        }

        public async Task SaveParametersToEEPROMAsync()
        {
            await UEyeCameraCommands.SaveParametersToEEPROMAsync(TrackingManager.Instance.Worker);
            NotificationHelper.SendInfo("Saved UEye parameters to EEPROM");
        }

        /// <summary>
        ///  Save uEye camera settings to internal memory.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void SaveParametersToEEPROM()
        {
            TrackingManager.CatchCommandErrors(SaveParametersToEEPROMAsync(), this);
        }

        public async Task LoadParametersFromFileAsync(string fileName)
        {
            await UEyeCameraCommands.LoadParametersFromFileAsync(
                TrackingManager.Instance.Worker,
                fileName);
            await ReadFromBackendAsync();
        }
        
        public void LoadParametersFromFile(string fileName)
        {
            TrackingManager.CatchCommandErrors(LoadParametersFromFileAsync(fileName));
        }
        
        public async Task SaveParametersToFileAsync(string fileName)
        {
            await UEyeCameraCommands.SaveParametersToFileAsync(
                TrackingManager.Instance.Worker,
                fileName);
        }
        
        public void SaveParametersToFile(string fileName)
        {
            TrackingManager.CatchCommandErrors(SaveParametersToFileAsync(fileName));
        }

        public async Task ResetParametersToDefaultAsync()
        {
            await UEyeCameraCommands.ResetParametersToDefaultAsync(TrackingManager.Instance.Worker);
            await ReadFromBackendAsync();
            NotificationHelper.SendInfo("Reset UEye parameters to default values");
        }

        /// <summary>
        ///  Reset uEye camera parameters to default.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void ResetParametersToDefault()
        {
            TrackingManager.CatchCommandErrors(ResetParametersToDefaultAsync(), this);
        }

        private async Task ReadFromBackendAsync()
        {
            await Task.WhenAll(
                this.exposure.InitializeValueFromBackend(),
                this.gain.InitializeValueFromBackend(),
                this.blackLevel.InitializeValueFromBackend(),
                this.gamma.InitializeValueFromBackend());
        }

        private void ReadFromBackend()
        {
            TrackingManager.CatchCommandErrors(ReadFromBackendAsync(), this);
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return this.exposure.CheckOnValueChangedForBrokenListeners()
                .Concat(this.gain.CheckOnValueChangedForBrokenListeners())
                .Concat(this.blackLevel.CheckOnValueChangedForBrokenListeners())
                .Concat(this.gamma.CheckOnValueChangedForBrokenListeners()).ToList();
        }
#endif
    }
}
