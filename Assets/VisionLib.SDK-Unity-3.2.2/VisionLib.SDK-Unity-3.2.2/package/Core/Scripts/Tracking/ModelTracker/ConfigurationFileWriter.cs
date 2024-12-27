using System;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public static class ConfigurationFileWriter
    {
        public class TrackerOrWorkerMissingException : Exception
        {
            public TrackerOrWorkerMissingException(string message)
                : base(message) {}
        }

        /// <summary>
        /// Writes the current tracker configuration to a file at the specified URI.
        /// </summary>
        /// <param name="fileURI"> An URI including file name and extension (typically ".vl") for
        /// tracking configuration files. 
        /// Preferably use VisionLib Schemes or Unity's built-in directory aliases.
        /// You may also use an absolute path. Relative paths are discouraged.
        /// </param>
        /// @exception ArgumentNullException, TrackerOrWorkerMissingException, NotSupportedException
        public static async Task SaveCurrentConfigurationAsync(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                throw new TrackerOrWorkerMissingException(
                    "Cannot save current configuration: Tracker is not initialized.");
            }

            var worker = TrackingManager.Instance.Worker;
            if (worker == null)
            {
                throw new TrackerOrWorkerMissingException(
                    "Cannot save current configuration: Worker does not exist.");
            }

            try
            {
                var configurationString =
                    await CreateConfigurationForCurrentTracker(TrackingManager.Instance, worker);
                WriteToFile(JsonHelper.FormatJson(configurationString), uri);
                NotificationHelper.SendInfo(
                    "Saved current configuration to " + VLSDK.GetPhysicalPath(uri));
            }
            catch (NotSupportedException e)
            {
                throw new NotSupportedException(
                    "SaveCurrentConfiguration does not support current tracker type: " + e.Message);
            }
        }

        public static async Task SaveCurrentConfigurationAsyncAndLogExceptions(
            string uri,
            UnityEngine.Object errorContext = null)
        {
            try
            {
                await SaveCurrentConfigurationAsync(uri);
            }
            catch (ArgumentNullException)
            {
                NotificationHelper.SendError(
                    "You must specify a valid file URI. " + "You may use VisionLib Schemes: \n " +
                    "E.g., on most platforms, use " + "`local-storage-dir:VisionLib/MyConfig.vl`." +
                    "For HoloLens, use " + "`capture-dir:VisionLib/MyConfig.vl` instead.",
                    errorContext);
            }
            catch (TrackerOrWorkerMissingException e)
            {
                NotificationHelper.SendError(e.Message, errorContext);
            }
            catch (NotSupportedException e)
            {
                NotificationHelper.SendError(e.Message, errorContext);
            }
            catch (AggregateException ee)
            {
                foreach (var e in ee.InnerExceptions)
                {
                    NotificationHelper.SendError(e.Message);
                }
            }
        }

        private static async Task<string> CreateConfigurationForCurrentTracker(
            TrackingManager trackingManager,
            Worker worker)
        {
            var trackerType = trackingManager.GetTrackerType();

            if (trackerType == "ModelTracker" || trackerType == "MultiModelTracker")
            {
                var parametersJson =
                    await ModelTrackerCommands.GetNonDefaultAttributesAsync(worker);
                return JsonStringConfigurationHelper.CreateTrackingConfigurationString(
                    ToCamelCase(trackerType),
                    parametersJson);
            }
            if (trackerType == "HoloLensModelTracker")
            {
                var parametersJson =
                    await ModelTrackerCommands.GetNonDefaultAttributesAsync(worker);
                var fieldOfView = await GetFieldOfViewValue(worker);
                return JsonStringConfigurationHelper
                    .CreateTrackingConfigurationStringWithHoloLensInputSection(
                        ToCamelCase(trackerType),
                        parametersJson,
                        fieldOfView);
            }

            throw new NotSupportedException(trackerType);
        }

        private static async Task<string> GetFieldOfViewValue(Worker worker)
        {
            var result = await WorkerCommands.GetAttributeAsync(worker, "fieldOfView");
            return result.value;
        }

        private static void WriteToFile(string content, string uri)
        {
            VLSDK.Set(uri, content);
        }

        private static string ToCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
    }
}
