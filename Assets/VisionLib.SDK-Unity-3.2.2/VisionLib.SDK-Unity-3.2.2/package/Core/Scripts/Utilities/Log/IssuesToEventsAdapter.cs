using System.Linq;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Provides events that send the type, log and details each time
    /// an issue from the VisionLib SDK occurred.
    /// </summary>
    /// @ingroup Core
    public class IssuesToEventsAdapter
    {
        private const string defaultString = "XXX";

        private static string[] SplitValues(
            string stringToSplit,
            string separator,
            int expectedValues)
        {
            var separatedStrings = stringToSplit.Split(
                new string[] {separator},
                System.StringSplitOptions.None);

            var result = new string[expectedValues];
            for (int i = 0; i < expectedValues; i++)
            {
                if (i < separatedStrings.Length)
                {
                    result[i] = separatedStrings[i];
                }
                else
                {
                    result[i] = defaultString;
                }
            }
            return result;
        }

        /// <summary>
        ///  Delegate for <see cref="OnIssue"/> events.
        /// </summary>
        public delegate void IssueAction(
            Issue.IssueType issueType,
            string issueLog,
            string issueDetails,
            MonoBehaviour caller);

        /// <summary>
        ///  Event which will send the type, log and details after an issue occurred.
        /// </summary>
        public event IssueAction OnIssue;

        public void RegisterToVLIssues()
        {
            TrackingManager.OnIssueTriggered += EmitIssuesEvent;
        }

        public void UnregisterFromVLIssues()
        {
            TrackingManager.OnIssueTriggered -= EmitIssuesEvent;
        }

        private void EmitIssuesEvent(Issue issue)
        {
            OnIssue?.Invoke(
                GetIssueType(issue),
                GetIssueMessage(issue),
                GetIssueDetails(issue),
                issue.caller);
        }

        private string GetIssueDetails(Issue issue)
        {
            var result = "Details:\nIssue " + (int) issue.code;
            if (issue.message != "")
            {
                result += "\n" + issue.message;
            }
            return result;
        }

        private static string SplitFileNameAndParameters(string fileNameAndParameters)
        {
            var fileNameParametersSplit = fileNameAndParameters.Split(
                new string[] {"?"},
                System.StringSplitOptions.None);

            if (fileNameParametersSplit.Length <= 1)
            {
                return "\"" + fileNameAndParameters + "\"";
            }

            var parameters = fileNameParametersSplit[1].Split(
                new string[] {"&"},
                System.StringSplitOptions.None);

            var returnMessage = "\"" + fileNameParametersSplit[0] + "\"\n\nWith parameters:";

            return parameters.Aggregate(
                returnMessage,
                (current, parameter) => current + ("\n\"" + parameter + "\""));
        }

        public static Issue.IssueType GetIssueType(Issue issue)
        {
            switch (issue.code)
            {
                case VLIssueCode.WARNING_CALIBRATION_DEVICE_ID_OVERWRITTEN_ON_LOAD:
                    return Issue.IssueType.Notification;
                default:
                    return issue.level;
            }
        }

        public static string GetIssueMessage(Issue issue)
        {
            switch (issue.code)
            {
                case VLIssueCode.INTERNAL_ERROR:
                    return "Internal error occurred";

                case VLIssueCode.WARNING_CALIBRATION_MISSING_FOR_DEVICE:
                    return "No calibration available for device:\n\n\"" + issue.info + "\"";

                case VLIssueCode.ERROR_NO_CAMERA_CONNECTED:
                    return "No camera found";

                case VLIssueCode.ERROR_NO_CAMERA_ACCESS:
                    return "No camera access possible: Camera may be removed, " +
                           "used by another process, or no camera access is granted";

                case VLIssueCode.ERROR_CAMERA_IMAGE_AQUISITION_FAILED:
                    return "Unable to acquire an image from the camera: Check the hardware" +
                           " connection to the camera and the currently set parameters.";

                case VLIssueCode.WARNING_CALIBRATION_DB_LOAD_FAILED:
                    return "Unable to load camera calibration database:\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_CALIBRATION_DB_INVALID:
                    return "Unable to parse camera calibration database:\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_CALIBRATION_DB_LOAD_ERROR:
                    return "Failed to add camera calibration database:\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_CALIBRATION_DEVICE_ID_OVERWRITTEN_ON_LOAD:
                    return "Camera calibration loading overwrote existing camera " +
                           "calibration for device:\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_CALIBRATION_DEVICE_ID_OVERWRITTEN_BY_ALTERNATIVE_ID:
                {
                    var deviceIDs = SplitValues(issue.info, "-", 2);
                    return $"Overwriting camera calibration \"{deviceIDs[0]}\" " +
                           $"by alternative ID \"{deviceIDs[1]}\"";
                }
                case VLIssueCode.DEPRECATION_WARNING:
                {
                    var deprecatedParameters = SplitValues(issue.info, "-", 2);
                    return $"Used deprecated parameter \"{deprecatedParameters[0]}\"; " +
                           $"use \"{deprecatedParameters[1]}\" instead";
                }
                case VLIssueCode.ERROR_JSON_FILE_INVALID_PARAMETER:
                {
                    var parameterInformation = SplitValues(issue.info, " :: ", 3);
                    return $"Encountered invalid parameter while loading json file " +
                           $"\"{parameterInformation[0]}\": Parameter " +
                           $"\"{parameterInformation[1]}\"  must have structure " +
                           $"\"{parameterInformation[2]}\"";
                }
                case VLIssueCode.ERROR_FILE_SYNTAX_ERROR:
                    return "Failed to parse file because of syntax error:\n\n" + issue.info;
                case VLIssueCode.ERROR_FILE_WRITING_FAILED:
                    return "Failed to write into file:\n\n" +
                           SplitFileNameAndParameters(issue.info);
                case VLIssueCode.WARNING_PERMISSION_NOT_SET:
                    return "Permission has not been set:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_FILE_READING_FAILED:
                    return "Failed to load from file:\n\n" + SplitFileNameAndParameters(issue.info);
                case VLIssueCode.ERROR_FILE_INVALID:
                    return "File is not valid:\n\n" + SplitFileNameAndParameters(issue.info);
                case VLIssueCode.ERROR_FILE_FORMAT_NOT_ALLOWED:
                    return "Can not load file with extension \"" + issue.info +
                           "\". For example only \".json\" and \".vl\" " +
                           "files are allowed for tracking configuration.";
                case VLIssueCode.ERROR_LICENSE_INVALID:
                    return "License file is not valid:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_LICENSE_EXPIRED:
                    return "License expired on " + issue.info;
                case VLIssueCode.ERROR_LICENSE_EXCEEDS_RUNS:
                    return "License runs exceeded; application has been run " + issue.info +
                           " times, but only 5 runs are allowed";
                case VLIssueCode.WARNING_LICENSE_MODEL_BOUND_FEATURE_INVALD:
                    return "Unlicensed model found; register your model hash at " +
                           "visionlib.com:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_LICENSE_INVALID_HOST_ID:
                    return "Unlicensed hostID found; register your hostID at " +
                           "visionlib.com:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_LICENSE_NOT_SET:
                    return "No license file has been set";
                case VLIssueCode.ERROR_LICENSE_INVALID_PLATFORM:
                    return "License can not be used on Platform " + issue.info;
                case VLIssueCode.WARNING_LICENSE_USING_UNREGISTERED_MODELS:
                    return
                        $"Models are used which are not registered in the license{(string.IsNullOrEmpty(issue.info) ? "" : $": {issue.info}")}";
                case VLIssueCode.ERROR_LICENSE_FILE_NOT_FOUND:
                    return "License file not found:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_LICENSE_INVALID_PROGRAM_VERSION:
                    return "License only allows versions of VisionLib built before " + issue.info;
                case VLIssueCode.ERROR_LICENSE_INVALID_SEAT:
                    return "License is bound to a software protection dongle and does not work " +
                           "with the current seat";
                case VLIssueCode.ERROR_LICENSE_INVALID_FEATURE:
                    return "The application uses a feature which is not covered by the current " +
                           "license:\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_LICENSE_EXPIRING_SOON:
                    return "License will expire in " + issue.info + " days";
                case VLIssueCode.ERROR_LICENSE_INVALID_BUNDLE_ID:
                    return "Unlicensed bundleID found; Register your bundleID at " +
                           "visionlib.com:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_LICENSE_EXCEEDED_ALLOWED_NUMBER_OF_ANCHORS:
                    return "Number of allowed tracking anchors exceeded. Your license allows the " +
                           "simultaneous usage of " + issue.info + " anchors";
                case VLIssueCode.ERROR_UNSUPPORTED_SCHEME:
                {
                    var schemeInfo = SplitValues(issue.info, " :: ", 2);
                    return $"Unsupported file path scheme: \"{schemeInfo[0]}\".";
                }
                case VLIssueCode.ERROR_FEATURE_NOT_SUPPORTED:
                    return "\"" + issue.info + "\" is not supported on your device.";
                case VLIssueCode.ERROR_MODEL_LOAD_FAILED:
                    return "Failed to load model:\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_MODEL_DECODE_FAILED:
                    return "Failed to decode model\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_IMPLAUSIBLE_METRIC:
                    return "The metric of you model is implausible. Check the " +
                           "`metric` parameter. The bounding box dimensions are: " + issue.info;
                case VLIssueCode.ERROR_DUPLICATE_MODEL_NAME:
                    return "The modelName has already been used or is used twice:\n\n\"" +
                           issue.info + "\"";
                case VLIssueCode.ERROR_DUPLICATE_MODEL_ID:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 2);
                    return $"The modelID generated by \"{infoParts[0]}\" and " +
                           $" \"{infoParts[1]}\" are identical." + " Change the name of one model.";
                }
                case VLIssueCode.ERROR_POSTER_LOAD_FAILED:
                    return "Failed to find poster image:\n\n\"" + issue.info + "\"";
                case VLIssueCode.WARNING_POSTER_QUALITY_CRITICAL:
                    return "Poster quality is only " + issue.info + "; use a different poster";
                case VLIssueCode.ERROR_GRAPH_SETUP_FAILED_UNKNOWN_ERROR:
                    return "The setup of the graph failed for an unknown reason";
                case VLIssueCode.ERROR_GRAPH_NODE_NOT_FOUND:
                    return "Could not find the node with the name " + issue.info;
                case VLIssueCode.ERROR_GRAPH_INVALID_DATA_PATH:
                    return "The data path doesn't comply with the expected pattern " +
                           "\"nodeName.dataName\":\n\n\"" + issue.info + "\"";
                case VLIssueCode.ERROR_GRAPH_INPUT_NOT_FOUND:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 3);
                    return $"Could not find the input \"{infoParts[1]}\" of the node " +
                           $"\"{infoParts[0]}\"\n\nPossible values: {infoParts[2]}";
                }
                case VLIssueCode.ERROR_GRAPH_OUTPUT_NOT_FOUND:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 3);
                    return $"Could not find the output \"{infoParts[1]}\" of the node " +
                           $"\"{infoParts[0]}\"\n\nPossible values: {infoParts[2]}";
                }
                case VLIssueCode.ERROR_GRAPH_HAS_CYCLES:
                    return "There is a cycle in the graph of the tracking configuration," +
                           " so no order of execution could be determined";
                case VLIssueCode.ERROR_GRAPH_TRACKERS_EMPTY:
                    return "There was no tracker defined inside the tracking configuration";
                case VLIssueCode.ERROR_GRAPH_DUPLICATE_DEVICE_NAME:
                    return "The name \"" + issue.info + "\" has been used for two or more devices";
                case VLIssueCode.ERROR_GRAPH_DUPLICATE_TRACKER_NAME:
                    return "The name \"" + issue.info + "\" has been used for two or more trackers";
                case VLIssueCode.ERROR_DUPLICATE_ANCHOR_NAME:
                    return "The name \"" + issue.info + "\" has been used for two or more anchors";
                case VLIssueCode.ERROR_ANCHOR_NAME_NOT_FOUND:
                    return "No anchor with name \"" + issue.info + "\" has been found";
                case VLIssueCode.ERROR_DLL_NOT_LOADED:
                    return "A required dll is not loaded: \"" + issue.info + "\"";
                case VLIssueCode.WARNING_DLL_LOAD_FAILED:
                    return "Loading of a required dll failed: \"" + issue.info + "\"";
                case VLIssueCode.WARNING_DLL_NOT_FOUND:
                    return "Could not find a required dll: \"" + issue.info + "\"";
                case VLIssueCode.WARNING_DLL_VERSION_DIFFERENT:
                    return "The version of a required dll is not compatible: \"" + issue.info +
                           "\"";
                case VLIssueCode.ERROR_COMMAND_CANCELED:
                    return "The command \"" + issue.commandName + "\" aborted";
                case VLIssueCode.ERROR_COMMAND_NOT_SUPPORTED:
                    var extendedMessage = issue.caller == null
                        ? ""
                        : "\nYou may want to deactivate the " + issue.caller.name +
                          " or remove it from the scene.";
                    return $"The command \"{issue.commandName}\" is not supported by the current " +
                           $"pipeline ({issue.info}).\n{extendedMessage}";
                case VLIssueCode.ERROR_COMMAND_INTERNAL_PROBLEM:
                    return "The command \"" + issue.commandName +
                           "\" could not be executed due to internal problems " +
                           "in VisionLib.SDK.Native: " + issue.info;
                case VLIssueCode.ERROR_COMMAND_INVALID_PARAMETER:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 2);
                    return $"The command \"{issue.commandName}\" could not be executed because " +
                           $"parameter \"{infoParts[0]}\" did not fit the required structure: " +
                           $"{infoParts[1]}";
                }
                case VLIssueCode.ERROR_COMMAND_PARAMETER_VALUE_NOT_SUPPORTED:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 3);
                    return $"The command \"{issue.commandName}\" could not be executed because " +
                           $"\"{infoParts[0]}\" = \"{infoParts[1]}\" is not supported in the " +
                           $"current pipeline ({infoParts[2]})";
                }
                case VLIssueCode.WARNING_PARAMETER_CONSTANT_VALUE:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 3);
                    var fixValueMessage = (infoParts[1] == "")
                        ? "It has a fixed value"
                        : $"It has the fix value \"{infoParts[1]}\"";
                    var reason = (infoParts[2] == "")
                        ? "because a requirement isn't met"
                        : $"because a requirement isn't met: {infoParts[2]}";
                    return
                        $"The parameter \"{infoParts[0]}\" could not be set. {fixValueMessage}, {reason}. For more details see the log message.";
                }
                case VLIssueCode.ERROR_COMMAND_CURRENTLY_NOT_POSSIBLE:
                {
                    return $"The command \"{issue.commandName}\" could not be executed because \"" +
                           issue.message + "\"";
                }
                case VLIssueCode.ERROR_MODEL_NOT_FOUND:
                    return $"The model with the name \"{issue.info}\" does not exist.";
                case VLIssueCode.ERROR_ANCHOR_IS_DESCENDANT:
                {
                    var infoParts = SplitValues(issue.info, " :: ", 2);
                    return $"Anchor \"{infoParts[0]}\" is a descendant of \"{infoParts[1]}\"";
                }
                default:
                    return $"Unspecific issue when executing \"{issue.commandName}\": " +
                           $"{issue.message}\nError Code {issue.code}\nInfo: {issue.info}";
            }
        }
    }
}
