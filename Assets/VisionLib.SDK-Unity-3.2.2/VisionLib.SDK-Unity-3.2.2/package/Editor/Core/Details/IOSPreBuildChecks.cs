#if UNITY_IOS
using System.Globalization;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each Build with target platform iOS.
    /// </summary>
    public class IOSPreBuildChecks : PreBuildChecks
    {
        private static readonly float minimumIosVersion = 12.1f;

        /// <summary>
        /// Callback containing checks that are performed directly after a build is initialized
        /// and before the build actually starts.
        /// </summary>
        public override void OnPreprocessBuild(BuildReport report)
        {
            Check(
                !IsCameraUsageDescriptionEmpty(),
                () => {},
                "No Camera Usage Description set",
                "VisionLib requires access to the Camera. Set the iOS usage description" +
                " under 'Project Settings>Player>iOS' to let the user know why you are using the Camera in your app.",
                "Continue Build");
            
            CheckOptional(
                !IsMinimumTargetVersionDifferent(),
                () =>
                {
                    PlayerSettings.iOS.targetOSVersionString =
                        IOSPreBuildChecks.minimumIosVersion.ToString(CultureInfo.InvariantCulture);
                },
                "Target minimum iOS Version is lower than VisionLib's iOS version.",
                "Project: " + PlayerSettings.iOS.targetOSVersionString + "\n" + "VisionLib: " +
                IOSPreBuildChecks.minimumIosVersion + "\n\n" +
                "Adjust the version under 'Project Settings>Player>iOS' to use the VisionLib iOS Version or higher.",
                $"Set minimum iOSVersion to {IOSPreBuildChecks.minimumIosVersion}");
        }

        private static bool IsCameraUsageDescriptionEmpty()
        {
            return string.IsNullOrEmpty(PlayerSettings.iOS.cameraUsageDescription);
        }

        private static bool IsMinimumTargetVersionDifferent()
        {
            return float.Parse(
                PlayerSettings.iOS.targetOSVersionString,
                CultureInfo.InvariantCulture.NumberFormat) < IOSPreBuildChecks.minimumIosVersion;
        }
    }
}
#endif // UNITY_IOS
