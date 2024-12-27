using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class AnchorRuntimeParametersConverter : JsonHelper.IJsonParsable
    {
        // Ignored parameters
        private Dictionary<string, string> ignoredParameters = new Dictionary<string, string>();

        // Global parameters
        private bool? extendibleTracking;
        private bool? staticScene;

        // Anchor parameters
        private Pose? initPose;
        private Metric.Unit? metric = null;
        private readonly AnchorRuntimeParameters trackingParameters = new();

        // ModelURIs
        public List<ModelProperties> modelURIs = new();

        // Internal project-dir path for resolving modelURIs
        private string projectDirPath = "";

        public static AnchorRuntimeParametersConverter ParseParameters(string json)
        {
            return ParseParameters(json, "");
        }

        public static AnchorRuntimeParametersConverter ParseParameters(
            string json,
            string projectDirPath)
        {
            var jObject = JObject.Parse(json);
            return jObject.TryGetValue("parameters", out var parametersToken)
                ? ParseParameters((JObject) parametersToken, projectDirPath)
                : ParseParameters(jObject, projectDirPath);
        }

        public AnchorRuntimeParametersConverter Clone()
        {
            var clone = new AnchorRuntimeParametersConverter();
            clone.initPose = this.initPose;
            clone.metric = this.metric;
            clone.trackingParameters.SetParametersInternal(this.trackingParameters);
            clone.projectDirPath = this.projectDirPath;
            return clone;
        }

        public static AnchorRuntimeParametersConverter ParseParameters(
            JObject jObject,
            string projectDirPath,
            AnchorRuntimeParametersConverter globalParameters = null)
        {
            var parseResult = globalParameters != null
                ? globalParameters.Clone()
                : new AnchorRuntimeParametersConverter();
            if (!string.IsNullOrEmpty(projectDirPath))
            {
                parseResult.projectDirPath = projectDirPath;
            }

            foreach (var property in jObject.Properties())
            {
                try
                {
                    parseResult.ParseParameterProperty(property);
                }
                catch (Exception e)
                {
                    parseResult.ignoredParameters.Add(property.Name, e.Message);
                }
            }
            return parseResult;
        }

        public static List<ModelProperties> ParseModels(JArray modelsArray, string projectDirPath)
        {
            return modelsArray.Select(
                modelDefinition => new ModelProperties(
                    modelDefinition.Value<string>("name") ?? "default",
                    PathHelper.SubstituteStreamingAssetsPathWithSchema(
                        PathHelper.SubstituteProjectDirPath(
                            modelDefinition.Value<string>("uri"),
                            projectDirPath)) ??
                    throw new ArgumentException("'uri' must not be null"),
                    modelDefinition.Value<bool?>("enabled") ?? true,
                    modelDefinition.Value<bool?>("occluder") ?? false,
                    modelDefinition.Value<bool?>("useLines") ?? false)).ToList();
        }

        public Task<WorkerCommands.CommandWarnings> ApplyToAnchorAsync(TrackingAnchor anchor)
        {
#if UNITY_EDITOR
            Undo.RecordObject(anchor, "Set new parameters");
#endif

            SetSpecialParameters(anchor);

            foreach (var model in this.modelURIs)
            {
                AddTrackingURI(anchor, model);
            }

            return anchor.GetAnchorRuntimeParameters()
                .SetParametersAsync(this.trackingParameters, anchor);
        }

        private void ParseParameterProperty(JProperty property)
        {
            switch (property.Name)
            {
                case "initPose":
                    this.initPose = InitPoseHelper.JsonInitPose.Parse(
                        JsonHelper.ConditionJson(property.ToString())).ToPose();
                    break;
                case "extendibleTracking":
                    this.extendibleTracking = property.Value.Value<bool>();
                    break;
                case "staticScene":
                    this.staticScene = property.Value.Value<bool>();
                    break;
                case "parameters":
                    this.ignoredParameters.Add(
                        property.Name,
                        "Cannot use a parameters section inside another parameters section");
                    break;
                case "anchors":
                case "debugLevel":
                case "synchronous":
                case "useColor":
                    this.ignoredParameters.Add(
                        property.Name,
                        "This parameter cannot be applied on anchor level");
                    break;
                case "modelURI":
                    var modelURI = property.Value.Value<string>();
                    if (!string.IsNullOrEmpty(this.projectDirPath))
                    {
                        modelURI = PathHelper.SubstituteStreamingAssetsPathWithSchema(
                            PathHelper.SubstituteProjectDirPath(modelURI, this.projectDirPath));
                    }
                    this.modelURIs.Add(
                        new ModelProperties("default", modelURI, true, false, false));
                    break;
                case "models":
                    this.modelURIs.AddRange(
                        ParseModels((JArray) property.Value, this.projectDirPath));
                    break;
                case "metric":
                    this.metric = Metric.Parse(property.Value.ToString());
                    break;
                case ContourEdgeThreshold.nativeName:
                    this.trackingParameters.contourEdgeThreshold.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case ContrastThreshold.nativeName:
                    this.trackingParameters.contrastThreshold.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case CreaseEdgeThreshold.nativeName:
                    this.trackingParameters.creaseEdgeThreshold.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case DetectionRadius.nativeName:
                    this.trackingParameters.detectionRadius.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case DetectionThreshold.nativeName:
                    this.trackingParameters.detectionThreshold.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case DisablePoseEstimation.nativeName:
                    this.trackingParameters.disablePoseEstimation.SetValueInternal(
                        property.Value.Value<bool>());
                    break;
                case KeyFrameDistance.nativeName:
                    this.trackingParameters.keyFrameDistance.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case PoseFilteringSmoothness.nativeName:
                    this.trackingParameters.poseFilteringSmoothness.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case SensitivityForEdgesInTexture.nativeName:
                    this.trackingParameters.sensitivityForEdgesInTexture.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case ShowLineModelRuntimeParameter.nativeName:
                    this.trackingParameters.showLineModel.SetValueInternal(
                        property.Value.ToShowLineModel());
                    break;
                case TrackingRadius.nativeName:
                    this.trackingParameters.trackingRadius.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                case TrackingThreshold.nativeName:
                    this.trackingParameters.trackingThreshold.SetValueInternal(
                        property.Value.Value<float>());
                    break;
                default:
                    this.trackingParameters.AddCustomParameter(
                        new AnchorRuntimeParameters.CustomParameter
                        {
                            parameterName = property.Name,
                            parameterValue = property.Value.ToString()
                        });
                    break;
            }
        }

        private void AddTrackingURI(TrackingAnchor anchor, ModelProperties model)
        {
            var matchingURIs = anchor.GetComponentsInChildren<TrackingURI>()
                .Where(trackingUri => trackingUri.GetModelFileURI() == model.uri).ToArray();
            if (matchingURIs.Any())
            {
                LogHelper.LogWarning(
                    $"No TrackingURI to {model.uri} added, since trackingAnchor '{anchor.GetAnchorName()}' already contains such a TrackingURI",
                    matchingURIs.First());
                return;
            }

            LogHelper.LogInfo($"Add TrackingURI {model.name}");
            var modelGameObject = new GameObject(model.name);
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(modelGameObject, $"Create {model.name} GameObject");
            Undo.RecordObject(modelGameObject, "Modify model GameObject");
            Undo.SetTransformParent(
                modelGameObject.transform,
                anchor.transform,
                "Re-parent model game object under anchor");
#else
            modelGameObject.transform.SetParent(anchor.transform);
#endif
            
            var trackingURI = TrackingURI.AddTrackingURI(modelGameObject, model.uri);

            trackingURI.enabled = model.enabled;
            trackingURI.SetOccluder(model.occluder);
            trackingURI.UseLines(model.useLines);
        }

        private void SetSpecialParameters(TrackingAnchor anchor)
        {
            // Init Pose
            if (this.initPose.HasValue)
            {
                anchor.SetVLInitPose(this.initPose.Value);
            }

            // Metric
            if (this.metric.HasValue)
            {
                anchor.SetMetric(this.metric.Value);
            }

            var trackingConfigurations = Object.FindObjectsOfType<TrackingConfiguration>();
            // Extendible Tracking
            if (this.extendibleTracking.HasValue)
            {
                foreach (var config in trackingConfigurations)
                {
                    config.extendTrackingWithSLAM = this.extendibleTracking.Value;
                }
            }

            // Static Scene
            if (this.staticScene.HasValue)
            {
                foreach (var config in trackingConfigurations)
                {
                    config.staticScene = this.staticScene.Value;
                }
            }
        }

        public bool IsValid()
        {
            return true;
        }

        public string GetJsonName()
        {
            return "Parameters";
        }

        public string GetWarning()
        {
            var noProjectDirDefinedString = "";
            if (string.IsNullOrEmpty(this.projectDirPath) && this.modelURIs.Count > 0)
            {
                noProjectDirDefinedString = "Empty project-dir path\n" +
                                            "Models are added but no project-dir substitution string was set.\n" +
                                            "These paths might point to a false location.\n\n";
            }

            var ignoredParametersString = "";
            if (this.ignoredParameters.Count > 0)
            {
                ignoredParametersString = this.ignoredParameters.Aggregate(
                    "Ignored Parameters\n",
                    (current, keyValuePair) =>
                        current + keyValuePair.Key + ": " + keyValuePair.Value + "\n");
            }
            return noProjectDirDefinedString + ignoredParametersString;
        }

        public override string ToString()
        {
            var globalParameterString = this.extendibleTracking == null
                ? ""
                : " - ExtendibleTracking: " + this.extendibleTracking.Value + "\n";
            globalParameterString += this.staticScene == null
                ? ""
                : " - StaticScene: " + this.staticScene.Value + "\n";
            if (!string.IsNullOrEmpty(globalParameterString))
            {
                globalParameterString = "The following global parameters will be set:\n" +
                                        globalParameterString;
            }

            var modelsString = this.modelURIs.Count == 0
                ? ""
                : this.modelURIs.Aggregate(
                    "GameObjects with TrackingURI Component will be created for the following models:\n",
                    (current, modelProperties) =>
                        current + $" - {modelProperties.name} ({modelProperties.uri})\n");

            var trackingParameterStringArray = new List<string>
            {
                this.metric == null ? "" : $"metric: {this.metric.ToString()}",
                this.initPose == null
                    ? ""
                    : $"initPose: {InitPoseHelper.JsonInitPose.ToString(this.initPose.Value)}",
                this.trackingParameters.detectionThreshold.GetJsonStringIfUsed(),
                this.trackingParameters.trackingThreshold.GetJsonStringIfUsed(),
                this.trackingParameters.contourEdgeThreshold.GetJsonStringIfUsed(),
                this.trackingParameters.creaseEdgeThreshold.GetJsonStringIfUsed(),
                this.trackingParameters.contrastThreshold.GetJsonStringIfUsed(),
                this.trackingParameters.detectionRadius.GetJsonStringIfUsed(),
                this.trackingParameters.trackingRadius.GetJsonStringIfUsed(),
                this.trackingParameters.keyFrameDistance.GetJsonStringIfUsed(),
                this.trackingParameters.poseFilteringSmoothness.GetJsonStringIfUsed(),
                this.trackingParameters.sensitivityForEdgesInTexture.GetJsonStringIfUsed(),
                this.trackingParameters.showLineModel.GetJsonStringIfUsed(),
                this.trackingParameters.disablePoseEstimation.GetJsonStringIfUsed()
            };
            trackingParameterStringArray.AddRange(
                this.trackingParameters.customParameters.Select(
                    parameter =>
                        $"{parameter.parameterName}: {parameter.parameterValue} (Custom Parameter)"));

            var trackingParameterString = string.Join(
                "\n",
                trackingParameterStringArray.Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => $" - {s}"));
            if (!string.IsNullOrEmpty(trackingParameterString))
            {
                trackingParameterString = "The following parameters will be set:\n" +
                                          trackingParameterString + "\n";
            }

            return string.Join(
                "\n",
                new[] {globalParameterString, modelsString, trackingParameterString}.Where(
                    s => !string.IsNullOrEmpty(s)));
        }
    }
}
