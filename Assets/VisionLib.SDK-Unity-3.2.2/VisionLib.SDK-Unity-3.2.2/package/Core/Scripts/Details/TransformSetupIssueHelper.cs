using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class TransformSetupIssueHelper
    {
#if UNITY_EDITOR
        public static List<SetupIssue> CheckForUnexpectedScale(GameObject gameObject)
        {
            if (gameObject.transform.lossyScale != Vector3.one ||
                gameObject.transform.localScale != Vector3.one)
            {
                return new List<SetupIssue>
                {
                    new(
                        $"{gameObject.name} cannot have scale",
                        $"The scale of {gameObject.name} will not be considered in the tracking " +
                        "Therefore its scale and the scale of all its parents have to be 1 in all dimensions.",
                        SetupIssue.IssueType.Error,
                        gameObject)
                };
            }
            return SetupIssue.NoIssues();
        }
#endif
    }
}
