using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public static class DynamicTrackingParameterSetHelper
    {
        public static void Broadcast(this List<ITrackingParameter> parameters)
        {
            parameters.ForEach(parameter => parameter.EmitOnValueChangedWithCurrentValue());
        }

        public static async Task<WorkerCommands.CommandWarnings> ResetToDefaultAsync(
            this List<ITrackingParameter> parameters,
            IParameterHandler parameterHandler)
        {
            return await WorkerCommands.AwaitAll(
                parameters.Select(parameter => parameter.ResetAsync(parameterHandler)));
        }

        public static async Task<WorkerCommands.CommandWarnings> UpdateInBackendAsync(
            this List<ITrackingParameter> parameters,
            IParameterHandler parameterHandler)
        {
            return await WorkerCommands.AwaitAll(
                parameters.Select(parameter => parameter.UpdateInBackendAsync(parameterHandler)));
        }

#if UNITY_EDITOR
        public static List<SetupIssue> GetSceneIssues(
            this List<ITrackingParameter> parameters,
            GameObject owner)
        {
            return parameters.Aggregate(
                new List<SetupIssue>(),
                (setupIssues, parameter) =>
                {
                    setupIssues.AddRange(parameter.CheckOnValueChangedForBrokenListeners(owner));
                    return setupIssues;
                });
        }
#endif
    }
}
