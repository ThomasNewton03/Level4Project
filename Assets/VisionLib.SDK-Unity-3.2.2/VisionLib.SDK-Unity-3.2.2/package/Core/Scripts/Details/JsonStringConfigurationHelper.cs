using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing helper methods to create json strings
    /// representing VisionLib tracking configuration files.
    /// </summary>
    public static class JsonStringConfigurationHelper
    {
        public static string CreateTrackingConfigurationString(
            string trackerType,
            string parametersJson)
        {
            return CreateTrackingConfigurationStringFromSections(
                CreateTrackerSection(trackerType, parametersJson));
        }

        public static string CreateTrackingConfigurationStringWithHoloLensInputSection(
            string trackerType,
            string parametersJson,
            string fieldOfView)
        {
            return CreateTrackingConfigurationStringFromSections(
                CreateTrackerSection(trackerType, parametersJson),
                CreateHoloLensInputSection(fieldOfView));
        }

        public static string CreateTrackingConfigurationStringWithImageSequence(
            string trackerType,
            string parametersJson,
            string imageSequenceURI)
        {
            return CreateTrackingConfigurationStringFromSections(
                CreateTrackerSection(trackerType, parametersJson),
                CreateImageSequenceInputSection(imageSequenceURI));
        }

        private static string CreateTrackingConfigurationStringFromSections(
            string trackerSection,
            string inputSection = null)
        {
            return "{" + "\"$schema\": \"" + GetSchemaURI() + "\"," +
                   "\"type\": \"VisionLibTrackerConfig\"," + "\"version\": 1," + trackerSection +
                   (string.IsNullOrEmpty(inputSection) ? "" : "," + inputSection) + "}";
        }

        private static string CreateTrackerSection(string trackerType, string parametersJson)
        {
            var parametersSection = trackerType == "posterTracker"
                ? CreatePosterTrackerParametersSection(parametersJson)
                : CreateParametersSection(parametersJson);

            return "\"tracker\":" + "{" + "\"type\": \"" + trackerType + "\"," + "\"version\": 1," +
                   parametersSection + "}";
        }

        private static string CreateParametersSection(string parametersJson)
        {
            if (string.IsNullOrEmpty(parametersJson))
            {
                return "\"parameters\": {}";
            }
            return "\"parameters\":" + parametersJson;
        }

        private static string CreatePosterTrackerParametersSection(string parametersJson)
        {
            if (string.IsNullOrEmpty(parametersJson))
            {
                return "\"parameters\": {" + "\"imageURI\": \"\"" + "}";
            }
            return "\"parameters\":" + parametersJson;
        }

        private static string CreateImageSequenceInputSection(string uri)
        {
            var dataSection = "\"data\": {" + "\"uri\": \"" + uri + "\"}";
            return CreateInputSection("imageSequence", dataSection);
        }

        private static string CreateHoloLensInputSection(string fieldOfView)
        {
            var dataSection = "\"data\": { \"undistort\": true," +
                              "\"useColor\": false," + "\"fieldOfView\": \"" + fieldOfView + "\"}";
            return CreateInputSection("camera", dataSection);
        }

        private static string CreateInputSection(string type, string dataSection)
        {
            const string imageSourceName = "myImageSource";
            return "\"input\":" + "{" + "\"useImageSource\": \"" + imageSourceName + "\"," +
                   "\"imageSources\": [{" + "\"name\": \"" + imageSourceName + "\"," +
                   "\"type\": \"" + type + "\"," + dataSection + "}]" + "}";
        }

        private static string GetSchemaURI()
        {
            return DocumentationLink.documentationBaseURI + "/vl.schema.json";
        }
    }
}
