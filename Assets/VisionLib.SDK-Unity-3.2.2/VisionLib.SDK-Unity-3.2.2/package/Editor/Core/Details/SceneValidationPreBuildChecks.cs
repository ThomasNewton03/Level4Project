using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Performs a scene setup validation before each build.
    /// </summary>
    public class SceneValidationPreBuildChecks : PreBuildChecks
    {
        private const string dialogTitlePrefix = "VisionLib: Malconfigured Scene";
        private const string continueWithoutFixingProblemMessage = "Continue Anyway";
        private const string cancelBuildMessage = "Cancel build";

        /// <summary>
        /// Callback containing checks that are performed directly after a Build is initialized
        /// and before the build actually starts.
        /// </summary>
        public override void OnPreprocessBuild(BuildReport report)
        {
            var trackingConfiguration = Object.FindObjectOfType<TrackingConfiguration>();
            var logOnly = (trackingConfiguration != null) && trackingConfiguration.ignoreSetupIssues;

            var setupIssues = SceneValidator.ValidateScene();
            foreach (var setupIssue in setupIssues)
            {
                setupIssue.Log(SetupIssue.IssueType.Warning);
                if (logOnly)
                {
                    continue;
                }
                HandleSetupIssue(setupIssue);
            }
        }

        private static void HandleSetupIssue(SetupIssue setupIssue)
        {
            bool continueWithDeployment = EditorUtility.DisplayDialog(
                SceneValidationPreBuildChecks.dialogTitlePrefix + " – " + setupIssue.title,
                setupIssue.message,
                SceneValidationPreBuildChecks.continueWithoutFixingProblemMessage,
                SceneValidationPreBuildChecks.cancelBuildMessage);
            if (!continueWithDeployment)
            {
                Selection.objects = new Object[] {setupIssue.sourceObject};
                CancelBuild();
            }
        }
    }
}
