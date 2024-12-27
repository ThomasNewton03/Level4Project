#if UNITY_ANDROID
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if VL_ARCORE_XRPLUGIN
using UnityEditor.XR.ARCore;
using UnityEngine.XR.ARCore;
#endif
#if VL_WITH_MAGICLEAP
using UnityEngine.XR.MagicLeap;
#endif

#if VL_ARFOUNDATION
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using System.Linq;
#endif

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Class containing all checks to be performed before each Build with target platform Android.
    /// </summary>
    public class AndroidPreBuildChecks : PreBuildChecks
    {
#if VL_ARCORE_XRPLUGIN
        private const string XRProviderName = "ARCore";
        private const string XRProviderTypeName = "UnityEngine.XR.ARCore.ARCoreLoader";
#elif VL_WITH_MAGICLEAP
        private const string XRProviderName = "MagicLeap";
        private const string XRProviderTypeName = "UnityEngine.XR.MagicLeap.MagicLeapLoader";
#endif

        private const string googlePrivacyDisclaimerPrefKey =
            "VisionLib.DontShowGooglePrivacyDisclaimer";
        private const BuildTargetGroup platformTargetGroup = BuildTargetGroup.Android;

        /// <summary>
        /// Callback containing checks that are performed directly after a Build is initialized
        /// and before the build actually starts.
        /// </summary>
        public override void OnPreprocessBuild(BuildReport report)
        {
            CheckForMinimalAPILevel(AndroidSdkVersions.AndroidApiLevel24);
#if !VL_WITH_MAGICLEAP
            CheckForArm64();
#endif
            CheckPlatformXRPluginEnabled();
#if VL_ARCORE_XRPLUGIN
            CheckARCoreSettingsAreOptional();
            ShowPrivacyDisclaimer();
#endif
        }

        private static void CheckForMinimalAPILevel(AndroidSdkVersions requiredVersion)
        {
            Check(
                PlayerSettings.Android.minSdkVersion >= requiredVersion,
                () => { PlayerSettings.Android.minSdkVersion = requiredVersion; },
                "Minimum API Level too low",
                $"VisionLib requires at least an minimum API level of {requiredVersion}. " +
                "You are currently using " + PlayerSettings.Android.minSdkVersion + "\n\n" +
                $"Would you like to increase the minimum API level to {requiredVersion}?",
                "Increase minimum API level");
        }

        private static void CheckForArm64()
        {
            CheckOptional(
                !Uses32BitWithout64Bit_ARM(),
                () =>
                {
                    EnableIL2CPP();
                    Enable64Bit_ARM();
                },
                "Arm 64 bit Disabled",
                "We strongly recommend enabling IL2CPP and selecting a 64 Bit target architecture in the Player " +
                "settings for Android. \n" +
                "VisionLib uses ARCore internally when deployed to android devices." +
                " ARCore is only available in our 64 bit builds. " +
                "If you continue the build, the resulting application will not use ARCore, " +
                "falling back to 'legacyCameraMode' instead. External SLAM features and " +
                "real-time calibration data will be unavailable." +
                "\n\nWould you like to enable IL2CPP and 64 Bit build?",
                "Enable 64 bit and continue");
        }

        private static bool Uses32BitWithout64Bit_ARM()
        {
            // Unity may say that 64 bit is enabled if IL2CPP is disabled, but the setting will not be effective.
            return !UsesIL2CPP() && Is32BitWithout64Bit_ARM();
        }

        private static bool UsesIL2CPP()
        {
            return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) ==
                   ScriptingImplementation.IL2CPP;
        }

        private static bool Is32BitWithout64Bit_ARM()
        {
            var arm32BitEnabled =
                (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) !=
                AndroidArchitecture.None;
            var arm64BitEnabled =
                (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) !=
                AndroidArchitecture.None;
            return arm32BitEnabled && !arm64BitEnabled;
        }

        private static void EnableIL2CPP()
        {
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP);
        }

        private static void Enable64Bit_ARM()
        {
            PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARM64;
        }

#if VL_ARFOUNDATION && (VL_ARCORE_XRPLUGIN || VL_WITH_MAGICLEAP)
        private static bool IsPlatformXRPluginEnabled(XRGeneralSettings platformXRSettings)
        {
            if (platformXRSettings.Manager == null || platformXRSettings.Manager.activeLoaders == null)
            {
                return false;
            }
#if VL_ARCORE_XRPLUGIN
            return platformXRSettings.Manager.activeLoaders.Any(loader => loader is ARCoreLoader);
#elif VL_WITH_MAGICLEAP
            return platformXRSettings.Manager.activeLoaders.Any(loader => loader is MagicLeapLoader);
#else
#error Unhandled XR Plugin!
#endif
        }
#endif

        private static void CheckPlatformXRPluginEnabled()
        {
            if (Uses32BitWithout64Bit_ARM())
            {
                // 32 bit does not link against ARCore, so we can skip this check.
                return;
            }
#if !VL_XR_MANAGEMENT
            Check(
                false,
                delegate {},
                "XR Plug-in Management not installed",
                "The XR Plug-in Management is broken or missing. Please install XR Plug-in Management in Unity via Project Settings > XR Plug-in Management > Install XR Plug-in Management or Window > Package Manager > Unity Registry > XR Plug-in Management to use VisionLib on Android.",
                "Continue anyway");
#elif !VL_ARCORE_XRPLUGIN && !VL_WITH_MAGICLEAP
            Check(
                false,
                delegate {},
                "No Plug-in Provider is installed",
                "Please install a XR Plug-in Provider (i.e. ARCore or MagicLeap) to use VisionLib on Android. To do so, go to Player Settings > XR Plug-in Management and install the desired provider.",
                "Continue anyway");
#elif VL_ARFOUNDATION
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

            var platformXRSettings =
                buildTargetSettings.SettingsForBuildTarget(
                    AndroidPreBuildChecks.platformTargetGroup);
            Check(
                IsPlatformXRPluginEnabled(platformXRSettings),
                () => XRPackageMetadataStore.AssignLoader(
                    platformXRSettings.Manager,
                    AndroidPreBuildChecks.XRProviderTypeName,
                    AndroidPreBuildChecks.platformTargetGroup),
                $"{AndroidPreBuildChecks.XRProviderName} XR Plug-In Missing",
                $"The {AndroidPreBuildChecks.XRProviderName} XR Plug-In is not " +
                "Enabled. Please enable this setting.",
                $"Enable {AndroidPreBuildChecks.XRProviderName} XR Plug-In");

#endif
        }

#if VL_ARCORE_XRPLUGIN
        private static void CheckARCoreSettingsAreOptional()
        {
            var arCoreSettings = ARCoreSettings.currentSettings;
            Check(
                arCoreSettings.requirement == ARCoreSettings.Requirement.Optional,
                () => arCoreSettings.requirement = ARCoreSettings.Requirement.Optional,
                "ARCore is required",
                $"Using VisionLib requires the ARCore to be optional.",
                "Set ARCore Required to Optional");
            Check(
                arCoreSettings.depth == ARCoreSettings.Requirement.Optional,
                () => arCoreSettings.depth = ARCoreSettings.Requirement.Optional,
                "ARCore Depth is required",
                $"Using ARCore with VisionLib requires the Depth feature of ARCore to be optional.",
                "Set Depth to Optional");
        }
#endif

        private static void ShowPrivacyDisclaimer()
        {
            if (PlayerPrefs.HasKey(AndroidPreBuildChecks.googlePrivacyDisclaimerPrefKey) &&
                PlayerPrefs.GetInt(AndroidPreBuildChecks.googlePrivacyDisclaimerPrefKey) == 1)
            {
                return;
            }

            switch (EditorUtility.DisplayDialogComplex(
                        "Google Privacy Disclaimer",
                        "The deployed application runs on Google Play Services for AR (ARCore), which is provided by Google LLC and governed by the Google Privacy Policy.",
                        "Confirm",
                        "Cancel build",
                        "Confirm and do not show again"))
            {
                case 0: // Ok
                    break;
                case 1: // Cancel
                    CancelBuild();
                    break;
                case 2: // Alternative (Confirm and do not show again)
                    PlayerPrefs.SetInt(AndroidPreBuildChecks.googlePrivacyDisclaimerPrefKey, 1);
                    break;
            }
        }
    }
}
#endif // UNITY_ANDROID
