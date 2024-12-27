using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    public static class TrackingParameterNames
    {
        private const string trackingParameterPrefix = "tracker.parameters";
        public const string simulateExternalSLAM =
            TrackingParameterNames.trackingParameterPrefix + ".simulateExternalSLAM";
        public const string showLineModel =
            TrackingParameterNames.trackingParameterPrefix + ".showLineModel";
        public const string textureColorSensitivity =
            TrackingParameterNames.trackingParameterPrefix + ".textureColorSensitivity";
        public const string extendibleTracking =
            TrackingParameterNames.trackingParameterPrefix + ".extendibleTracking";
        public const string staticScene =
            TrackingParameterNames.trackingParameterPrefix + ".staticScene";
        public const string debugLevel =
            TrackingParameterNames.trackingParameterPrefix + ".debugLevel";
        public const string recordToNewDir =
            TrackingParameterNames.trackingParameterPrefix + ".recordToNewDir";
        public const string recordURIPrefix =
            TrackingParameterNames.trackingParameterPrefix + ".recordURIPrefix";
    }

    public static class InputParameterNames
    {
        private const int defaultInputIndex = 0;
        public static readonly string inputData =
            $"inputs[{InputParameterNames.defaultInputIndex}].data";
        public static readonly string inputType =
            $"inputs[{InputParameterNames.defaultInputIndex}].type";
        public static readonly string inputName =
            $"inputs[{InputParameterNames.defaultInputIndex}].name";
        /// \deprecated The simulateMobileRecording parameter should be replaced by SmartDownsamplingDisabled
        [Obsolete("The simulateMobileRecording parameter should be replaced by SmartDownsamplingDisabled")]
        public static readonly string simulateMobileRecording =
            InputParameterNames.inputData + ".simulateMobileRecording";
        public static readonly string smartDownsamplingDisabled =
            InputParameterNames.inputData + ".SmartDownsamplingDisabled";
        public static readonly string ignoreImageSequenceIntrinsics =
            InputParameterNames.inputData + ".ignoreImageSequenceIntrinsics";
    }

    public struct ImageSequenceQueryParameter
    {
        public string uri;
        public bool smartDownsamplingDisabled; // bools are initialized to false by default
        public bool ignoreImageSequenceIntrinsics; // bools are initialized to false by default
    }

    public static class QueryHelper
    {
        public enum DebugLevel
        {
            Off = 0,
            On = 1
        }

        public static string GenerateBooleanQuery(string parameterName, bool enable)
        {
            return parameterName + (enable ? "=true" : "=false");
        }

        public static string GenerateDebugLevelQueryParameter(DebugLevel debugLevel)
        {
            return TrackingParameterNames.debugLevel + "=" + (int) debugLevel;
        }

        public static List<string> GenerateImageSequenceQueryParameters(
            ImageSequenceQueryParameter parameter)
        {
            var queryParameters = GenerateImageSequenceInputQueryParameters(parameter.uri);
            if (parameter.smartDownsamplingDisabled)
            {
                queryParameters.Add(
                    GenerateBooleanQuery(InputParameterNames.smartDownsamplingDisabled, true));
            }
            if (parameter.ignoreImageSequenceIntrinsics)
            {
                queryParameters.Add(
                    GenerateBooleanQuery(InputParameterNames.ignoreImageSequenceIntrinsics, true));
            }
            return queryParameters;
        }

        public static List<string> GenerateImageInjectionQueryParameters()
        {
            return new List<string>
            {
                InputParameterNames.inputType + "=\"inject\"",
                InputParameterNames.inputName + "=\"inject0\""
            };
        }

        /// \deprecated GenerateImageSequenceQueryParameters with bool arguments is deprecated. Please use the ImageSequenceQueryParameter version instead. Note that smartDownsamplingDisabled is the inverse of simulateMobileRecording.
        [Obsolete("GenerateImageSequenceQueryParameters with bool arguments is deprecated. Please use the ImageSequenceQueryParameter version instead. Note that smartDownsamplingDisabled is the inverse of simulateMobileRecording.")]
        public static List<string> GenerateImageSequenceQueryParameters(
            string imageSequenceURI,
            bool simulateMobileRecording = false,
            bool ignoreImageSequenceIntrinsics = false)
        {
            return GenerateImageSequenceQueryParameters(
                new ImageSequenceQueryParameter
                {
                    uri = imageSequenceURI,
                    smartDownsamplingDisabled = !simulateMobileRecording,
                    ignoreImageSequenceIntrinsics = ignoreImageSequenceIntrinsics
                });
        }

        public static string GenerateTextureSensitivityQueryParameter(float sensitivityValue)
        {
            return TrackingParameterNames.textureColorSensitivity + "=" + sensitivityValue.ToString(
                "F2",
                CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static List<string> GenerateRecorderDirectoryQueryParameter(
            string temporaryImagePath)
        {
            var recordURIPrefix = string.Join("/", new string[] {temporaryImagePath, "Image_"});
            return new List<string>
            {
                GenerateBooleanQuery(TrackingParameterNames.recordToNewDir, false),
                TrackingParameterNames.recordURIPrefix + "=\"" + recordURIPrefix + "\""
            };
        }

        public static List<string> GenerateInputSourceQueryParameters(
            InputSourceSelection.InputSource source)
        {
            if (string.IsNullOrEmpty(source.deviceID))
            {
                Debug.LogWarning(
                    "Provided InputSource is missing its deviceID. " +
                    "Query parameter generation was skipped.");
                return new List<string>();
            }

            var sourceFormatSet = source.format != null;

            var queryParameters = new List<string>
            {
                sourceFormatSet
                    ? InputParameterNames.inputData + ".deviceID=\"" + source.deviceID + "\""
                    : "input.useDeviceID=\"" + source.deviceID + "\""
            };

            if (sourceFormatSet)
            {
                queryParameters.AddRange(GenerateCameraFormatQueryParameters(source.format));
            }

            return queryParameters;
        }

        public static string AppendQueryParametersToURI(string uri, List<string> queries)
        {
            return queries.Aggregate(uri, AppendQueryStringToURI);
        }

        public static string AppendQueryStringToURI(string uri, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return uri;
            }
            var separator = uri.Contains("?") ? "&" : "?";
            return uri + separator + query;
        }

        public static string RemoveEntireQueryString(string uri)
        {
            var queryStartIndex = uri.LastIndexOf('?');
            return queryStartIndex >= 0 ? uri.Substring(0, queryStartIndex) : uri;
        }

        public static string GetEntireQueryString(string uri)
        {
            var queryStartIndex = uri.LastIndexOf('?');
            return queryStartIndex >= 0 ? uri.Substring(queryStartIndex) : "";
        }

        public static Dictionary<string, string> GetAllQueryParametersAsMap(string uri)
        {
            var queryString = GetEntireQueryString(uri);

            if (queryString == "")
            {
                return new Dictionary<string, string>();
            }

            var queryParameters = queryString.Substring(1).Split(
                new char[] {'&'},
                StringSplitOptions.RemoveEmptyEntries).ToList();
            var queryParameterValuesMap = new Dictionary<string, string>();

            foreach (var queryParameter in queryParameters)
            {
                var values = queryParameter.Split(new char[] {'='}, 2);
                queryParameterValuesMap[values[0]] = values.Length == 2 ? values[1] : "";
            }

            return queryParameterValuesMap;
        }

        public static bool CustomInputSetInQueryString(string uri)
        {
            return GetAllQueryParametersAsMap(uri).Keys.Any(
                key => key == "input.useDeviceID" || key == "input.useImageSource" ||
                       key.StartsWith("inputs") || key.StartsWith("input.imageSources"));
        }

        public static bool CustomExtendibleTrackingValueSetInQueryString(string uri)
        {
            return GetAllQueryParametersAsMap(uri)
                .ContainsKey(TrackingParameterNames.extendibleTracking);
        }

        public static bool CustomStaticSceneValueSetInQueryString(string uri)
        {
            return GetAllQueryParametersAsMap(uri).ContainsKey(TrackingParameterNames.staticScene);
        }

        private static List<string> GenerateImageSequenceInputQueryParameters(
            string imageSequenceURI)
        {
            return new List<string>
            {
                InputParameterNames.inputType + "=\"imageSequence\"",
                InputParameterNames.inputData + ".uri=\"" + imageSequenceURI + "\""
            };
        }

        private static List<string> GenerateCameraFormatQueryParameters(
            DeviceInfo.Camera.Format cameraFormat)
        {
            // Calculate reasonable focal lengths, which can be used as initial
            // guess for a large range of cameras -> arithmetic mean of their
            // normalized values equals one
            double dw = cameraFormat.width;
            double dh = cameraFormat.height;
            var fNorm = 2.0 / (dw + dh);
            var fx = dh * fNorm;
            var fy = dw * fNorm;

            var queryParameters = new List<string>
            {
                InputParameterNames.inputData + ".calibration.width=" +
                cameraFormat.width.ToString(CultureInfo.InvariantCulture),
                InputParameterNames.inputData + ".calibration.height=" +
                cameraFormat.height.ToString(CultureInfo.InvariantCulture),
                InputParameterNames.inputData + ".calibration.fx=" + fx.ToString(
                    "R",
                    CultureInfo.InvariantCulture),
                InputParameterNames.inputData + ".calibration.fy=" + fy.ToString(
                    "R",
                    CultureInfo.InvariantCulture),
                InputParameterNames.inputData + ".calibration.cx=0.5",
                InputParameterNames.inputData + ".calibration.cy=0.5",
                InputParameterNames.inputType + "=\"camera\"",
                GenerateBooleanQuery(InputParameterNames.smartDownsamplingDisabled, true)
            };

            return queryParameters;
        }
    }
}
