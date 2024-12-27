#if UNITY_STANDALONE_OSX
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each Build with target platform macOS.
    /// </summary>
    public class MacOSPreBuildChecks : PreBuildChecks
    {
        /// <summary>
        /// Callback containing checks that are performed directly after a build is initialized
        /// and before the build actually starts.
        /// </summary>
        public override void OnPreprocessBuild(BuildReport report)
        {
            Check(
                !IsCameraUsageDescriptionEmpty(),
                () => {},
                "iOS No Camera Usage Description set",
                "VisionLib requires access to the Camera. Set the macOS usage description" +
                " under 'Project Settings>Player>Mac' to let the user know why you are using the Camera in your app.",
                "Continue Build");
        }

        private static bool IsCameraUsageDescriptionEmpty()
        {
            return string.IsNullOrEmpty(PlayerSettings.macOS.cameraUsageDescription);
        }
    }
}
#endif // UNITY_STANDALONE_OSX
