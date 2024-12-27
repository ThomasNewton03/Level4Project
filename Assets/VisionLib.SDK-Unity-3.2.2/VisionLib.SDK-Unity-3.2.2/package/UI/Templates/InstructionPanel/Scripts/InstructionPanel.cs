using System;
using UnityEngine;
using Visometry.VisionLib.SDK.Core;

namespace Visometry.VisionLib.SDK.Examples
{
    /**
     *  @ingroup Examples
     */
    [AddComponentMenu("VisionLib/Examples/Instruction Panel")]
    [HelpURL(DocumentationLink.APIReferenceURI.Examples + "instruction_panel.html")]
    internal class InstructionPanel : MonoBehaviour
    {
        private const string salesMail = "mailto:sales@visometry.com";

        private void Awake()
        {
            TrackingManager.OnTrackerInitialized += HidePanel;
            TrackingManager.OnTrackerStopped += ShowPanel;
        }

        private void OnDestroy()
        {
            TrackingManager.OnTrackerInitialized -= HidePanel;
            TrackingManager.OnTrackerStopped -= ShowPanel;
        }

        private void HidePanel()
        {
            this.gameObject.SetActive(false);
        }

        private void ShowPanel()
        {
            this.gameObject.SetActive(true);
        }

        public void OpenSalesMail()
        {
            Application.OpenURL(InstructionPanel.salesMail);
        }

        public void OpenDocumentation()
        {
            DocumentationLink.OpenVisionLibDocumentation();
        }

        public void OpenModelTrackerConfig()
        {
            Application.OpenURL(DocumentationLink.modelTrackerConfig);
        }

        public void OpenTrackingEssentials()
        {
            Application.OpenURL(DocumentationLink.trackingEssentials);
        }

        public void OpenUnderstandingTrackingParameters()
        {
            Application.OpenURL(DocumentationLink.understandingTrackingParameters);
        }

        public void OpenImageRecorder()
        {
            Application.OpenURL(DocumentationLink.imageRecorder);
        }

        public void OpenCameraCalibration()
        {
            Application.OpenURL(DocumentationLink.cameraCalibration);
        }

        public void OpenQuickStart()
        {
            Application.OpenURL(DocumentationLink.quickStart);
        }

        public void OpenModelTrackingSetup()
        {
            Application.OpenURL(DocumentationLink.modelTrackingSetup);
        }

        public void OpenPosterTracking()
        {
            Application.OpenURL(DocumentationLink.posterTracking);
        }

        public void OpenModelInjection()
        {
            Application.OpenURL(DocumentationLink.modelInjection);
        }

        public void OpenAutoInit()
        {
            Application.OpenURL(DocumentationLink.autoInit);
        }

        public void OpenMultiModel()
        {
            Application.OpenURL(DocumentationLink.multiModel);
        }

        public void OpenARFoundation()
        {
            Application.OpenURL(DocumentationLink.arFoundation);
        }

        public void OpenMagicLeap()
        {
            Application.OpenURL(DocumentationLink.magicLeap);
        }

        public void OpenUEye()
        {
            Application.OpenURL(DocumentationLink.uEyeCameras);
        }

        public void OpenURP()
        {
            Application.OpenURL(DocumentationLink.urp);
        }

        public void OpenOccluders()
        {
            Application.OpenURL(DocumentationLink.occluders);
        }

        public void OpenDifferentAugmentation()
        {
            Application.OpenURL(DocumentationLink.differentAugmentation);
        }

        public void OpenAddTrackingDuringRuntime()
        {
            Application.OpenURL(DocumentationLink.addTrackingDuringRuntime);
        }

        public void OpenNestedTracking()
        {
            Application.OpenURL(DocumentationLink.nestedTracking);
        }

        public void OpenModelPartsTracking()
        {
            Application.OpenURL(DocumentationLink.modelPartsTracking);
        }

        public void OpenTextureColorSensitivityParameter()
        {
            Application.OpenURL(DocumentationLink.textureColorSensitivityParameter);
        }
    }
}
