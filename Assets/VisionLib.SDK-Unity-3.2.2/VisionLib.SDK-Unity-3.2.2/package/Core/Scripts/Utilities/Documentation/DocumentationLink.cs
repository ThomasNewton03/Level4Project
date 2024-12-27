using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    public static class DocumentationLink
    {
        private const string baseVisionLibURI = "https://docs.visionlib.com";
        private const string version = "3.2.2";
        public const string documentationBaseURI =
            DocumentationLink.baseVisionLibURI + "/v" + DocumentationLink.version;
        private const string docsifyBaseURI = DocumentationLink.documentationBaseURI + "/#/";

        // Tracking essentials
        public const string trackingEssentials = DocumentationLink.docsifyBaseURI +
                                                 "Using_VisionLib/Tracking_Essentials/README";
        public const string basicTrackingParameter = DocumentationLink.docsifyBaseURI +
                                                     "Using_VisionLib/Understanding_Tracking/OptionalTrackingParameters?id=basic-tracking-parameters";
        public const string understandingTrackingParameters = DocumentationLink.docsifyBaseURI +
                                                              "Using_VisionLib/Understanding_Tracking/OptionalTrackingParameters";
        public const string textureColorSensitivityParameter = DocumentationLink.docsifyBaseURI +
                                                              "Using_VisionLib/Understanding_Tracking/OptionalTrackingParameters?id=texture-color-sensitivity";
        public const string imageRecorder = DocumentationLink.docsifyBaseURI +
                                            "Using_VisionLib/Tracking_Essentials/ImageSequences";
        public const string cameraCalibration = DocumentationLink.docsifyBaseURI +
                                                "Using_VisionLib/Tracking_Essentials/CameraCalibration";
        public const string uEyeCameras = DocumentationLink.docsifyBaseURI +
                                          "Using_VisionLib/Tracking_Essentials/Advanced_VisionLib_Features/UEyeCameras";

        // Unity tutorials
        public const string quickStart = DocumentationLink.docsifyBaseURI +
                                         "Using_VisionLib/Working_With_Unity/QuickStart";
        public const string modelTrackingSetup = DocumentationLink.docsifyBaseURI +
                                                 "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/Using_the_Model_Tracking_Setup_Scene/README";
        public const string posterTracking = DocumentationLink.docsifyBaseURI +
                                             "Using_VisionLib/Working_With_Unity/PosterTracker";
        public const string modelInjection = DocumentationLink.docsifyBaseURI +
                                             "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/ModelInjection";
        public const string autoInit = DocumentationLink.docsifyBaseURI +
                                       "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/AutoInitalization";
        public const string multiModel = DocumentationLink.docsifyBaseURI +
                                         "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/ModelInjection?id=adding-tracking-anchors-to-a-scene";
        public const string arFoundation = DocumentationLink.docsifyBaseURI +
                                           "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/ARFoundation";
        public const string magicLeap = DocumentationLink.docsifyBaseURI +
                                        "Using_VisionLib/Working_With_Unity/MagicLeap";
        public const string urp = DocumentationLink.docsifyBaseURI +
                                  "Using_VisionLib/Working_With_Unity/URP_Support";
        public const string occluders = DocumentationLink.docsifyBaseURI +
                                        "Using_VisionLib/Tracking_Essentials/Occluder";
        public const string differentAugmentation = DocumentationLink.docsifyBaseURI +
                                                    "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/DifferentAugmentationAndInitPoseGuide";
        public const string addTrackingDuringRuntime = DocumentationLink.docsifyBaseURI +
                                                       "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/AddTrackingDuringRuntime";
        public const string nestedTracking = DocumentationLink.docsifyBaseURI +
                                             "Using_VisionLib/Tracking_Essentials/Advanced_VisionLib_Features/Nested_Tracking/Readme";
        public const string holoLensCommands = DocumentationLink.docsifyBaseURI +
                                               "Using_VisionLib/Working_With_Unity/VisionLib_Tracking_on_HoloLens/HoloLensCommands";
        public const string modelPartsTracking = DocumentationLink.docsifyBaseURI +
                                                 "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/ModelPartsTrackingUnity";

        // Components
        public const string trackingAnchor = DocumentationLink.docsifyBaseURI +
                                             "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/ModelInjection?id=tracking-anchor";
        public const string trackingMesh = DocumentationLink.docsifyBaseURI +
                                           "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/ModelPartsTrackingUnity?id=tracking-mesh";
        public const string renderedObject = DocumentationLink.docsifyBaseURI +
                                             "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/DifferentAugmentationAndInitPoseGuide?id=rendered-object";
        public const string initPoseInteraction = DocumentationLink.docsifyBaseURI +
                                                  "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/SettingInitPose?id=init-pose-interaction";
        public const string simpleWorkSpace = DocumentationLink.docsifyBaseURI +
                                              "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/AutoInitalization?id=setting-up-a-simpleworkspace";
        public const string advancedWorkSpace = DocumentationLink.docsifyBaseURI +
                                                "Using_VisionLib/Working_With_Unity/Model_Tracking_in_Unity/AutoInitalization?id=setting-up-an-advancedworkspace";

        // Tracking Configuration
        public const string initPoseJson = DocumentationLink.docsifyBaseURI +
                                           "Detailed_Reference/Configuration_File_Reference/ModelTracker?id=initpose";
        public const string modelTrackerConfig = DocumentationLink.docsifyBaseURI +
                                                 "Detailed_Reference/Configuration_File_Reference/ModelTracker";

        // API Reference
        public static class APIReferenceURI
        {
            private const string separator = "_1_1_";
            private const string baseURI = DocumentationLink.documentationBaseURI +
                                           "/api_reference/" + "class_visometry" +
                                           APIReferenceURI.separator + "vision_lib" +
                                           APIReferenceURI.separator + "s_d_k" +
                                           APIReferenceURI.separator;
            public const string Core = APIReferenceURI.baseURI + "core" + APIReferenceURI.separator;
            public const string UI = APIReferenceURI.baseURI + "u_i" + APIReferenceURI.separator;
            public const string Examples =
                APIReferenceURI.baseURI + "examples" + APIReferenceURI.separator;
            public const string HoloLens =
                APIReferenceURI.baseURI + "holo_lens" + APIReferenceURI.separator;
            public const string MagicLeap =
                APIReferenceURI.baseURI + "magic_leap" + APIReferenceURI.separator;
            public const string ARFoundation = APIReferenceURI.baseURI + "a_r_foundation" +
                                               APIReferenceURI.separator;
        }

#if UNITY_EDITOR
        [MenuItem("VisionLib/Documentation", false, 1001)]
#endif
        public static void OpenVisionLibDocumentation()
        {
            Application.OpenURL(DocumentationLink.docsifyBaseURI);
        }
    }
}
