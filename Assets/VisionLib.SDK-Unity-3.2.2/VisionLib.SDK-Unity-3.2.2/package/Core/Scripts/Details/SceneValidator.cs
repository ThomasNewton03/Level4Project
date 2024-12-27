using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class SceneValidator
    {
        /// <summary>
        /// Returns a list of all <see cref="SetupIssue"/>s found in the scene.
        /// </summary>
        public static List<SetupIssue> ValidateScene()
        {
            var componentsWithValidationCheck = Object.FindObjectsOfType<MonoBehaviour>()
                .OfType<ISceneValidationCheck>();
            return AggregateSceneIssues(componentsWithValidationCheck);
        }

        public static List<SetupIssue> AggregateSceneIssues(
            IEnumerable<ISceneValidationCheck> componentsWithValidationCheck)
        {
            var issues = new List<SetupIssue>();
            foreach (var checkableComponent in componentsWithValidationCheck)
            {
                issues.AddRange(checkableComponent.GetSceneIssues());
            }
            return issues.Distinct(new SetupIssue.EqualityComparer()).ToList();
        }

        static SceneValidator()
        {
            EditorApplication.playModeStateChanged += AddNotificationsOnPlayModeEntered;
        }

        private static void AddNotificationsOnPlayModeEntered(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            var trackingConfiguration = Object.FindObjectOfType<TrackingConfiguration>();
            var logOnly = (trackingConfiguration != null) && trackingConfiguration.ignoreSetupIssues;
            
            var componentsWithValidationCheck = Object.FindObjectsOfType<MonoBehaviour>()
                .OfType<ISceneValidationCheck>();
            foreach (var checkableComponent in componentsWithValidationCheck)
            {
                foreach (var issue in checkableComponent.GetSceneIssues())
                {
                    if (logOnly)
                    {
                        issue.Log();
                    }
                    else
                    {
                        issue.Notify();
                    }
                }
            }
        }
    }
#endif
}
