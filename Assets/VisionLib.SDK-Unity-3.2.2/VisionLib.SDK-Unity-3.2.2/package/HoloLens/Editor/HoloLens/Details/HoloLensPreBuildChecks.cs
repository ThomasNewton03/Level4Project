#if UNITY_WSA
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Visometry.VisionLib.SDK.HoloLens.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each Build with target platform UWP.
    /// </summary>
    public class HoloLensPreBuildChecks : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Callback containing checks that are performed directly after a Build is initialized
        /// and before the build actually starts.
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
#if !(UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR))
            if (!EditorUtility.DisplayDialog(
                    "VisionLib: No XRPlugin set",
                    "Deploying to HoloLens requires you to specify an XRPlugin. " +
                    "Please follow the corresponding tutorial page for deploying on HoloLens in " +
                    "the documentation.",
                    "Continue without XRPlugin",
                    "Cancel build"))
            {
                throw new BuildFailedException("Build was canceled by the user.");
            }
#endif
        }
    }
}
#endif
