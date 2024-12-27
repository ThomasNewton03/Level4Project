using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class AnchorRuntimeParameters
    {
        [Serializable]
        public class CustomParameter
        {
            public string parameterName;
            public string parameterValue;
        }

        public DetectionThreshold detectionThreshold = new();
        public TrackingThreshold trackingThreshold = new();
        public ContourEdgeThreshold contourEdgeThreshold = new();
        public CreaseEdgeThreshold creaseEdgeThreshold = new();
        public ContrastThreshold contrastThreshold = new();
        public DetectionRadius detectionRadius = new();
        public TrackingRadius trackingRadius = new();
        public KeyFrameDistance keyFrameDistance = new();
        public PoseFilteringSmoothness poseFilteringSmoothness = new();
        public SensitivityForEdgesInTexture sensitivityForEdgesInTexture = new();
        public ShowLineModelRuntimeParameter showLineModel = new();
        public DisablePoseEstimation disablePoseEstimation = new();

        /// <summary>
        /// The customParameters allow you to set parameters, that are not usually provided in the
        /// TrackingAnchor.
        /// Note that these parameters will only be set once after the Anchor has been
        /// enabled. Additionally, they will not overwrite the provided parameters.
        /// </summary>
        [SerializeField]
        internal CustomParameter[] customParameters = Array.Empty<CustomParameter>();

        public Task<WorkerCommands.CommandWarnings> UpdateParametersInBackendAsync(
            IParameterHandler parameterHandler)
        {
            return WorkerCommands.AwaitAll(
                new[]
                {
                    UpdateCustomParameters(parameterHandler),
                    GetListOfAllParameters().UpdateInBackendAsync(parameterHandler)
                });
        }

        internal void SetParametersInternal(AnchorRuntimeParameters newParameters)
        {
            this.customParameters = newParameters.customParameters;
            this.detectionThreshold.SetValueInternal(newParameters.detectionThreshold);
            this.trackingThreshold.SetValueInternal(newParameters.trackingThreshold);
            this.contourEdgeThreshold.SetValueInternal(newParameters.contourEdgeThreshold);
            this.creaseEdgeThreshold.SetValueInternal(newParameters.creaseEdgeThreshold);
            this.contrastThreshold.SetValueInternal(newParameters.contrastThreshold);
            this.detectionRadius.SetValueInternal(newParameters.detectionRadius);
            this.trackingRadius.SetValueInternal(newParameters.trackingRadius);
            this.keyFrameDistance.SetValueInternal(newParameters.keyFrameDistance);
            this.poseFilteringSmoothness.SetValueInternal(newParameters.poseFilteringSmoothness);
            this.sensitivityForEdgesInTexture.SetValueInternal(
                newParameters.sensitivityForEdgesInTexture);
            this.showLineModel.SetValueInternal(newParameters.showLineModel);
            this.disablePoseEstimation.SetValueInternal(newParameters.disablePoseEstimation);
        }

        public async Task<WorkerCommands.CommandWarnings> SetParametersAsync(
            AnchorRuntimeParameters newParameters,
            TrackingAnchor anchor)
        {
            var warnings = await ResetCustomParameters(anchor);
            this.customParameters = newParameters.customParameters;
            SetParametersInternal(newParameters);
            var additionalWarnings = await UpdateParametersInBackendAsync(anchor);
            return warnings.Concat(additionalWarnings);
        }

        internal List<ITrackingParameter> GetListOfAllParameters()
        {
            return new List<ITrackingParameter>()
            {
                this.detectionThreshold,
                this.trackingThreshold,
                this.contourEdgeThreshold,
                this.creaseEdgeThreshold,
                this.contrastThreshold,
                this.detectionRadius,
                this.trackingRadius,
                this.keyFrameDistance,
                this.poseFilteringSmoothness,
                this.sensitivityForEdgesInTexture,
                this.showLineModel,
                this.disablePoseEstimation
            };
        }

        internal Task<WorkerCommands.CommandWarnings> ResetParametersToDefaultAsync(
            IParameterHandler parameterHandler)
        {
            var trackingAnchor = parameterHandler as TrackingAnchor;
            if (trackingAnchor == null)
            {
                LogHelper.LogWarning(
                    "Given IParameterSetter is no TrackingAnchor. Cannot reset parameters to default.");
                return Task.FromResult(WorkerCommands.NoWarnings());
            }
            return WorkerCommands.AwaitAll(
                new[]
                {
                    ResetCustomParameters(trackingAnchor),
                    GetListOfAllParameters().ResetToDefaultAsync(parameterHandler)
                });
        }

        /// <summary>
        /// Adds a new customParameter.
        /// Note that these parameters will only be set once after the Anchor has been
        /// enabled. Additionally, they will not overwrite the provided parameters.
        /// </summary>
        public void AddCustomParameter(CustomParameter parameter)
        {
            this.customParameters = this.customParameters.Concat(new[] {parameter}).ToArray();
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues(GameObject owner)
        {
            var sceneIssues = GetListOfAllParameters().GetSceneIssues(owner);
            if (this.customParameters == null)
            {
                return sceneIssues;
            }

            sceneIssues.AddRange(
                from standardParameter in GetListOfAllParameters()
                from customParameter in this.customParameters
                where customParameter.parameterName == standardParameter.GetNativeName()
                select new SetupIssue(
                    "Default parameter in customParameters",
                    $"The custom parameter {customParameter.parameterName} is already a default parameter. It will be overwritten by the default parameter.",
                    SetupIssue.IssueType.Warning,
                    owner,
                    new[]
                    {
                        new ReversibleAction(
                            () =>
                            {
                                var reducedParameters = this.customParameters.Where(
                                        parameter =>
                                            parameter.parameterName !=
                                            customParameter.parameterName)
                                    .ToArray();
                                this.customParameters = reducedParameters;
                            },
                            owner,
                            $"Remove {customParameter.parameterName} from customParameters")
                    }));

            var occurrences = new Dictionary<string, int>();
            foreach (var customParameter in this.customParameters)
            {
                if (occurrences.ContainsKey(customParameter.parameterName))
                {
                    occurrences[customParameter.parameterName] += 1;
                }
                else
                {
                    occurrences.Add(customParameter.parameterName, 1);
                }
            }
            sceneIssues.AddRange(
                from occurrence in occurrences
                where occurrence.Value > 1
                select new SetupIssue(
                    "Duplicate parameter in customParameters",
                    $"The custom parameter {occurrence.Key} is defined multiple times in customParameters.",
                    SetupIssue.IssueType.Warning,
                    owner,
                    new ReversibleAction(
                        () =>
                        {
                            var reducedParameters = this.customParameters
                                .Where(parameter => parameter.parameterName != occurrence.Key)
                                .Append(
                                    this.customParameters.First(
                                        parameter => parameter.parameterName == occurrence.Key))
                                .ToArray();
                            this.customParameters = reducedParameters;
                        },
                        owner,
                        $"Remove all but the first occurence of {occurrence.Key} from customParameters")));

            return sceneIssues;
        }
#endif

        private Task<WorkerCommands.CommandWarnings> UpdateCustomParameters(
            IParameterHandler parameterHandler)
        {
            return PerformOnCustomParameters(
                parameter => parameterHandler.SetParameterAsync(
                    parameter.parameterName,
                    parameter.parameterValue));
        }

        private async Task<WorkerCommands.CommandWarnings> ResetCustomParameters(
            TrackingAnchor anchor)
        {
            var warnings = await PerformOnCustomParameters(
                parameter => anchor.ResetParameterAsync(parameter.parameterName));
            this.customParameters = Array.Empty<CustomParameter>();
            return warnings;
        }

        private Task<WorkerCommands.CommandWarnings> PerformOnCustomParameters(
            Func<CustomParameter, Task<WorkerCommands.CommandWarnings>> func)
        {
            return WorkerCommands.AwaitAll(this.customParameters.Select(func));
        }
    }
}
