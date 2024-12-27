#if UNITY_EDITOR

using System.Linq;
using UnityEngine.XR.Management;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;

#if VL_ARKIT_XRPLUGIN
using UnityEngine.XR.ARKit;
#endif

#if VL_ARCORE_XRPLUGIN
using UnityEditor.XR.ARCore;
using UnityEngine.Rendering;
#endif

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each build for ARFoundation.
    /// </summary>
    public class ARFoundationPreBuildChecks : PreBuildChecks
    {
#if VL_ARCORE_XRPLUGIN
        private static readonly GraphicsDeviceType[] ARCoreSupportedGraphicsAPIs = new[]
        {
            GraphicsDeviceType.OpenGLES3
        };
#endif

#if UNITY_IOS
        private const string XRProviderName = "ARKit";
#if VL_ARKIT_XRPLUGIN
        private const string XRProviderTypeName = "UnityEngine.XR.ARKit.ARKitLoader";
        private const BuildTargetGroup platformTargetGroup = BuildTargetGroup.iOS;

        private static bool IsPlatformXRPluginEnabled(XRGeneralSettings platformXRSettings)
        {
            return platformXRSettings && platformXRSettings.Manager &&
                   platformXRSettings.Manager.activeLoaders.Any(loader => loader is ARKitLoader);
        }
#endif
#endif

        /// <summary>
        /// Callback containing checks that are performed directly after a build is initialized
        /// and before the build actually starts.
        /// </summary>
        public override void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_IOS
            CheckPlatformXRPluginEnabled();
#elif VL_ARCORE_XRPLUGIN
            CheckGraphicsAPISettings();
#else
            return;
#endif
        }

#if UNITY_IOS
        private static void CheckPlatformXRPluginEnabled()
        {
#if !VL_XR_MANAGEMENT
            Check(
                false,
                delegate {},
                "XR Plug-in Management not installed",
                "The XR Plug-in Management is broken or missing. Please install XR Plug-in Management in Unity via Project Settings > XR Plug-in Management > Install XR Plug-in Management or Window > Package Manager > Unity Registry > XR Plug-in Management to use VisionLib with ARFoundation on iOS.",
                "Continue anyway");
#else
            EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(
                XRGeneralSettings.k_SettingsKey,
                out var buildTargetSettings);
            var settingsFound = buildTargetSettings != null;

            Check(
                settingsFound,
                delegate {},
                "Broken XR Environment",
                "Unable to load the XR environment settings. " +
                "This might indicate that the XR Plugin Management is broken or missing.",
                "Continue anyway");

            if (!settingsFound)
            {
                return;
            }

#if VL_ARKIT_XRPLUGIN
            var platformXRSettings =
                buildTargetSettings.SettingsForBuildTarget(
                    ARFoundationPreBuildChecks.platformTargetGroup);
            Check(
                IsPlatformXRPluginEnabled(platformXRSettings),
                () => XRPackageMetadataStore.AssignLoader(
                    platformXRSettings.Manager,
                    ARFoundationPreBuildChecks.XRProviderTypeName,
                    ARFoundationPreBuildChecks.platformTargetGroup),
                $"{ARFoundationPreBuildChecks.XRProviderName} XR Plug-In Missing",
                $"The {ARFoundationPreBuildChecks.XRProviderName} XR Plug-In is not " +
                "enabled. Please enable this setting.",
                $"Enable {ARFoundationPreBuildChecks.XRProviderName} XR Plug-In");
#else
            Check(
                false,
                delegate {},
                $"{ARFoundationPreBuildChecks.XRProviderName} XR Plug-In Missing",
                $"The {ARFoundationPreBuildChecks.XRProviderName} XR Plug-In is not " +
                $"installed. Please install the {ARFoundationPreBuildChecks.XRProviderName} plugin " + 
                "via Window > Package Manager > Unity Registry > ARKit XR Plugin or " + 
                "Project Settings > XR Plug-in Management > ARKit to use VisionLib with ARFoundation on iOS.",
                "Continue anyway");
#endif
#endif
        }
#endif

#if VL_ARCORE_XRPLUGIN
        private static void CheckGraphicsAPISettings()
        {
            var usingAutoGraphicsAPI =
                PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);

            var vulkanIsNotPreferredAPI = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)[0] !=
                                          GraphicsDeviceType.Vulkan;
            Check(
                vulkanIsNotPreferredAPI,
                () =>
                {
                    if (usingAutoGraphicsAPI)
                    {
                        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                    }
                    PlayerSettings.SetGraphicsAPIs(
                        BuildTarget.Android,
                        ARFoundationPreBuildChecks.ARCoreSupportedGraphicsAPIs);
                },
                "Using Vulkan Graphics API",
                "Vulkan is set as the preferred graphics API. " +
                "This will cause your build to fail since ARCore only supports OpenGL ES. " +
                "The assistant can set the enabled graphics API to OpenGL ES 3.0 for you." +
                (usingAutoGraphicsAPI
                    ? "\n\nThis will disable the 'Auto Graphics API' setting."
                    : ""),
                "Use Only OpenGL ES 3.0 ");
        }
#endif
    }
}
#endif
