#if UNITY_IOS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEditor.Callbacks;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// VL Xcode project mod, modifies the Xcode project for iOS builds to include
    /// the following changes:
    /// 1. Add frameworks: Accelerate, Metal and GLKit
    /// 2. Disable Bitcode option
    /// 3. Add Libraries: libxml2, libz
    /// </summary>
    public class XcodeBuildPostProcessor
    {
        [PostProcessBuildAttribute(0)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject proj = new PBXProject();
                proj.ReadFromFile(projPath);

                string target = proj.GetUnityMainTargetGuid();

                string frameworkGUID = proj.FindFileGuidByProjectPath(
                    "Frameworks/com.visometry.visionlib.sdk/Plugins/iOS/vlSDK.framework");

                proj.AddFileToEmbedFrameworks(target, frameworkGUID);
                proj.AddBuildProperty(
                    target,
                    "LD_RUNPATH_SEARCH_PATHS",
                    "@executable_path/Frameworks");

                // Add Frameworks
                proj.AddFrameworkToProject(target, "Accelerate.framework", false);
                proj.AddFrameworkToProject(target, "Metal.framework", false);
                proj.AddFrameworkToProject(target, "GLKit.framework", false);
                proj.AddFrameworkToProject(target, "ARKit.framework", true);

                // Disable use of Bitcode
                proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

                // New in 2019.3 - having a separate UnityFramework
                string fwTarget = proj.GetUnityFrameworkTargetGuid();
                proj.SetBuildProperty(fwTarget, "ENABLE_BITCODE", "NO");
                proj.UpdateBuildProperty(
                    fwTarget,
                    "ARCHS",
                    new string[] {},
                    new string[] {"armv7", "armv7s", "armv7"});
                proj.UpdateBuildProperty(
                    fwTarget,
                    "VALID_ARCHS",
                    new string[] {"arm64"},
                    new string[] {"armv7", "armv7s"});

                // Only generate 64bit variants, so set Valid Archs to only "arm64"
                // proj.SetBuildProperty(target, "ARCHS", "arm64");
                // proj.SetBuildProperty(target, "VALID_ARCHS", "arm64");
                proj.UpdateBuildProperty(
                    target,
                    "ARCHS",
                    new string[] {},
                    new string[] {"armv7", "armv7s", "armv7"});
                proj.UpdateBuildProperty(
                    target,
                    "VALID_ARCHS",
                    new string[] {"arm64"},
                    new string[] {"armv7", "armv7s"});

                proj.WriteToFile(projPath);

                // Since iOS 10 it's necessary to add a reason for accessing the
                // camera to Info.plist. Newer version of Unity allow to set the
                // usage description inside the editor. For older Versions of
                // Unity, we add a default value automatically.

                // Get plist
                string plistPath = path + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                // Get root
                PlistElementDict rootDict = plist.root;

                // Set usage description, if not set already
                string cameraUsageDescriptionKey = "NSCameraUsageDescription";
                string cameraUsageDescriptionValue = "Augmented Reality";
                PlistElementString cameraUsageDescriptionEl =
                    (PlistElementString) rootDict[cameraUsageDescriptionKey];
                if (cameraUsageDescriptionEl == null)
                {
                    rootDict.SetString(cameraUsageDescriptionKey, cameraUsageDescriptionValue);
                    File.WriteAllText(plistPath, plist.WriteToString());
                }
                else if (String.IsNullOrEmpty(cameraUsageDescriptionEl.value))
                {
                    cameraUsageDescriptionEl.value = cameraUsageDescriptionValue;
                    File.WriteAllText(plistPath, plist.WriteToString());
                }
            }
        }
    }
}
#endif // UNITY_IOS
