using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class TrackingConfigurationFileConverter : JsonHelper.IJsonParsable
    {
        private struct TrackingAnchorDescription
        {
            public string anchorName;
            public AnchorRuntimeParametersConverter parameter;
        }

        private List<TrackingAnchorDescription> anchors = new List<TrackingAnchorDescription>();

        // Ignored properties
        private Dictionary<string, string> ignoredSections = new Dictionary<string, string>();

        public static TrackingConfigurationFileConverter Parse(string json, string projectDirPath)
        {
            var jObject = JObject.Parse(json);
            var parseResult = new TrackingConfigurationFileConverter();

            foreach (var property in jObject.Properties())
            {
                switch (property.Name)
                {
                    // Unnecessary parameter
                    case "$schema":
                    case "type":
                    case "version":
                        break;
                    // Unsupported sections
                    case "trackers":
                    case "inputs":
                    case "input":
                        parseResult.ignoredSections.Add(
                            property.Name,
                            "Section is currently not supported");
                        break;
                    // Tracker section
                    case "tracker":
                        parseResult.ParseTrackerSection(property.Value as JObject, projectDirPath);
                        break;
                    // Additional parameter on base level
                    default:
                        parseResult.ignoredSections.Add(
                            property.Name,
                            "No handling for this parameter is implemented");
                        break;
                }
            }
            if (parseResult.anchors.Count == 0)
            {
                throw new DataException(
                    "The provided tracking configuration file does not contain a valid 'ModelTracker' or 'MultiModelTracker' configuration.");
            }
            return parseResult;
        }

        private void ParseTrackerSection(JObject trackerSection, string projectDirPath)
        {
            var type = trackerSection["type"].Value<string>();

            switch (type)
            {
                case "modelTracker":
                    var trackerName = trackerSection["name"] != null
                        ? trackerSection["name"].Value<string>()
                        : "TrackedObject";

                    ParseModelTrackerSection(
                        trackerName,
                        trackerSection["parameters"] as JObject,
                        projectDirPath);
                    break;
                case "multiModelTracker":
                    ParseMultiModelTrackerSection(
                        trackerSection["parameters"] as JObject,
                        projectDirPath);
                    break;
                default:
                    this.ignoredSections.Add(
                        "tracker",
                        $"Tracker type {type} is currently not supported");
                    return;
            }
        }

        private void ParseModelTrackerSection(
            string name,
            JObject parametersJson,
            string projectDirPath)
        {
            try
            {
                var parameters =
                    AnchorRuntimeParametersConverter.ParseParameters(
                        parametersJson,
                        projectDirPath);
                this.anchors.Add(
                    new TrackingAnchorDescription() {anchorName = name, parameter = parameters});
            }
            catch (Exception e)
            {
                this.ignoredSections.Add("tracker.parameters", e.Message);
            }
        }

        private void ParseMultiModelTrackerSection(JObject parametersJson, string projectDirPath)
        {
            var globalParameters =
                AnchorRuntimeParametersConverter.ParseParameters(parametersJson, projectDirPath);

            var anchorsJson = parametersJson.GetValue("anchors");
            if (anchorsJson == null)
            {
                throw new Exception(
                    "Multi Model Tracker configuration does not have an \"anchors\" section.");
            }

            foreach (var anchor in anchorsJson)
            {
                var trackerName = anchor["name"] != null
                    ? anchor["name"].Value<string>()
                    : "TrackedObject";

                // Parse Parameter
                var parameters = globalParameters.Clone();
                try
                {
                    if (anchor["parameters"] != null)
                    {
                        parameters = AnchorRuntimeParametersConverter.ParseParameters(
                            anchor["parameters"] as JObject,
                            projectDirPath,
                            globalParameters);
                    }
                }
                catch (Exception e)
                {
                    this.ignoredSections.Add("tracker.anchors.name", e.Message);
                }

                // Parse Models
                try
                {
                    var models = anchor["models"];
                    if (models != null)
                    {
                        parameters.modelURIs.AddRange(
                            AnchorRuntimeParametersConverter.ParseModels(
                                models as JArray,
                                projectDirPath));
                    }
                }
                catch (Exception e)
                {
                    this.ignoredSections.Add("tracker.anchors.models", e.Message);
                }

                this.anchors.Add(
                    new TrackingAnchorDescription()
                    {
                        anchorName = trackerName, parameter = parameters
                    });
            }
        }

        public new string ToString()
        {
            return string.Join(
                "------------------------------------------------------------------------\n",
                this.anchors.Select(
                    anchor =>
                    {
                        var anchorString =
                            $"The TrackingAnchor ({anchor.anchorName}) will be created\n";
                        if (anchor.parameter != null)
                        {
                            anchorString += anchor.parameter;
                        }
                        return anchorString;
                    }));
        }

        public Task<WorkerCommands.CommandWarnings> Apply()
        {
            return WorkerCommands.AwaitAll(
                this.anchors.Select(
                    anchor =>
                    {
                        var trackingAnchor = Object.FindObjectsOfType<TrackingAnchor>()
                            .FirstOrDefault(
                                trackingAnchor => trackingAnchor.name == anchor.anchorName);
                        if (trackingAnchor == null)
                        {
                            var trackingAnchorGameObject = new GameObject(anchor.anchorName);
                            Undo.RegisterCreatedObjectUndo(
                                trackingAnchorGameObject,
                                $"Create {anchor.anchorName}");
                            trackingAnchor =
                                Undo.AddComponent<TrackingAnchor>(trackingAnchorGameObject);
                            Undo.AddComponent<RenderedObject>(trackingAnchorGameObject);

                        }
                        return anchor.parameter.ApplyToAnchorAsync(trackingAnchor);
                    }));
        }

        public bool IsValid()
        {
            return this.anchors.Count > 0;
        }

        public string GetJsonName()
        {
            return "Tracking Configuration File";
        }

        public string GetWarning()
        {
            var ignoredSectionsString = this.ignoredSections.Count == 0
                ? ""
                : this.ignoredSections.Aggregate(
                    "Ignored Sections\n",
                    (current, keyValuePair) =>
                        current + keyValuePair.Key + ": " + keyValuePair.Value + "\n");

            var anchorWarningString = string.Join(
                "------------\n",
                from anchor in this.anchors
                let warnings = anchor.parameter.GetWarning()
                where !string.IsNullOrEmpty(warnings)
                select anchor.anchorName + "\n" + warnings);

            return string.Join(
                "------------\n",
                new[] {ignoredSectionsString, anchorWarningString}.Where(
                    s => !string.IsNullOrEmpty(s)));
        }
    }
}
