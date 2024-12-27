#if UNITY_WSA
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each Build with target platform UWP.
    /// </summary>
    public class UWPPreBuildChecks : PreBuildChecks
    {
        /// <summary>
        /// Callback containing checks that are performed directly after a Build is initialized
        /// and before the build actually starts.
        /// </summary>
        public override void OnPreprocessBuild(BuildReport report)
        {
            CheckRequiredCapability(PlayerSettings.WSACapability.WebCam);
            CheckRequiredCapability(PlayerSettings.WSACapability.Microphone);
            CheckRequiredCapability(PlayerSettings.WSACapability.VideosLibrary);
        }

        private static void CheckRequiredCapability(PlayerSettings.WSACapability capability)
        {
            CheckOptional(
                PlayerSettings.WSA.GetCapability(capability),
                () => PlayerSettings.WSA.SetCapability(capability, true),
                "Required capability '" + capability + "' is missing",
                "VisionLib requires the capability '" + capability + "' to work correctly.\n" +
                "Would you like to enable it?",
                "Enable " + capability + " and continue");
        }
    }
}
#endif
