using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Helper functions for setting up basic VisionLib tracking.
    /// </summary>
    /// @ingroup Core
    public static class TrackingSetupHelper
    {
        public const string gameObjectContextMenuPath = "GameObject/VisionLib/";
        private const string setupVisionLibTracking = "Set Up VisionLib Tracking";
        private const string setupVisionLibTrackingFromFile =
            "Set Up VisionLib Tracking from Tracking Configuration File";
        private const string useForTracking = "Use for Tracking";

        private const string vlTrackingPrefab =
            "Packages/com.visometry.visionlib.sdk/Core/Prefabs/Common/VLTracking.prefab";

        // The components in this list will not be considered when evaluating if a Prefab already
        // exists in the current scene.
        private static readonly Type[] componentBlackList = {typeof(Transform)};

        /// <summary>
        /// Adds all necessary components/prefabs for VisionLib tracking.
        /// If the prefabs already exist, nothing will be done.
        /// If all components of the prefab are already present in the scene, nothing will be done.
        /// </summary>
        [MenuItem(
            TrackingSetupHelper.gameObjectContextMenuPath +
            TrackingSetupHelper.setupVisionLibTracking,
            false,
            1)]
        [MenuItem("VisionLib/" + TrackingSetupHelper.setupVisionLibTracking + "", false, 1)]
        public static void SetupVisionLibTracking()
        {
            SetupVisionLibTrackingInternal();
        }

        private static bool SetupVisionLibTrackingInternal()
        {
            if (!RequiresTrackingSetup())
            {
                return true;
            }
            try
            {
                if (InfoDialogWindow.OpenSetupTrackingDialog())
                {
                    CreateDefaultModelTrackerConfigIfMissing();
                    AddPrefabIfMissing(TrackingSetupHelper.vlTrackingPrefab);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                Undo.PerformUndo();
                throw;
            }
        }

        /// <summary>
        /// Validation function for SetupVisionLibTracking. Checks whether VisionLib Tracking is already present.
        /// </summary>
        /// <returns></returns>
        [MenuItem(
            TrackingSetupHelper.gameObjectContextMenuPath +
            TrackingSetupHelper.setupVisionLibTracking,
            true,
            1)]
        [MenuItem("VisionLib/" + TrackingSetupHelper.setupVisionLibTracking + "", true, 1)]
        public static bool RequiresTrackingSetup()
        {
            var prefabInScene = GetPrefabInScene(TrackingSetupHelper.vlTrackingPrefab);
            if (prefabInScene.Length > 0)
            {
                return false;
            }
            return PrefabComponentsExistsInScene(TrackingSetupHelper.vlTrackingPrefab) !=
                   PrefabExistence.Complete;
        }

        /// <summary>
        /// Adds all necessary components/prefabs for VisionLib tracking.
        /// If the prefabs already exist, nothing will be done.
        /// If all components of the prefab are already present in the scene, nothing will be done.
        /// </summary>
        [MenuItem(
            TrackingSetupHelper.gameObjectContextMenuPath +
            TrackingSetupHelper.setupVisionLibTrackingFromFile,
            false,
            2)]
        [MenuItem("VisionLib/" + TrackingSetupHelper.setupVisionLibTrackingFromFile + "", false, 2)]
        public static void SetupVisionLibTrackingFromFile()
        {
            try
            {
                var trackingConfig = TrackingSetupFromFileWindow.OpenSetupTrackingDialog();
                if (trackingConfig == null)
                {
                    return;
                }

                if (!SetupVisionLibTrackingInternal())
                {
                    LogHelper.LogWarning($"VisionLib Setup has been canceled.");
                }
                trackingConfig.Apply();
            }
            catch (Exception)
            {
                Undo.PerformUndo();
                throw;
            }
        }

        /// <summary>
        /// Validation function for SetupVisionLibTrackingFromFile. Checks whether a TrackingAnchor
        /// is already present in the current scene
        /// </summary>
        /// <returns></returns>
        [MenuItem(
            TrackingSetupHelper.gameObjectContextMenuPath +
            TrackingSetupHelper.setupVisionLibTrackingFromFile,
            true,
            2)]
        [MenuItem("VisionLib/" + TrackingSetupHelper.setupVisionLibTrackingFromFile + "", true, 2)]
        public static bool ValidateTrackingSetupFromFile()
        {
            return !Object.FindObjectOfType<TrackingAnchor>();
        }

        /// <summary>
        /// Enable using the selected GameObject for tracking by adding a TrackingAnchor and any
        /// other necessary VisionLib components (if not already present). 
        /// This function only works if the selected GameObject has a `MeshFilter` and isn't already
        /// part of a TrackingAnchor.
        /// See also <see cref="ValidateUseForTracking"/>.
        /// </summary>
        [MenuItem(
            TrackingSetupHelper.gameObjectContextMenuPath + TrackingSetupHelper.useForTracking,
            false,
            3)]
        public static void UseForTracking()
        {
            var gameObject = Selection.activeTransform.gameObject;

            if (!SetupVisionLibTrackingInternal())
            {
                LogHelper.LogWarning(
                    $"VisionLib Setup has been canceled. Cannot use {gameObject} for tracking.",
                    gameObject);
                return;
            }
            if (!InfoDialogWindow.OpenUseForTrackingDialog(gameObject.name))
            {
                return;
            }

            LogHelper.LogInfo($"Use {gameObject} for tracking.", gameObject);

            GameObject parentGameObject = null;
            if (gameObject.transform.parent != null)
            {
                parentGameObject = gameObject.transform.parent.gameObject;
                Undo.RegisterFullObjectHierarchyUndo(
                    parentGameObject,
                    "Inserting TrackingAnchor between the selected GameObject and its parent");
            }

            var trackingAnchorGameObject = AddAnchorWithSelfAugmentation(parentGameObject);
            var trackingAnchor = trackingAnchorGameObject.GetComponent<TrackingAnchor>();
            Undo.SetTransformParent(
                gameObject.transform,
                trackingAnchorGameObject.transform,
                $"Make {gameObject} child of TrackingAnchor.");
            TrackingObjectHelper.AddTrackingMeshesInSubTree(trackingAnchorGameObject);
            trackingAnchor.CenterInitPoseInSlamCamera();
        }

        /// <summary>
        /// Validation function for UseForTracking. Checks whether the selected GameObject can
        /// be used for tracking.
        /// </summary>
        /// <returns></returns>
        [MenuItem(
            TrackingSetupHelper.gameObjectContextMenuPath + TrackingSetupHelper.useForTracking,
            true,
            2)]
        public static bool ValidateUseForTracking()
        {
            if (Selection.activeTransform == null)
            {
                return false;
            }
            var gameObject = Selection.activeTransform.gameObject;
            if (gameObject.GetComponentInParent<TrackingAnchor>())
            {
                return false;
            }
            if (gameObject.GetComponentInChildren<TrackingAnchor>())
            {
                return false;
            }
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            return meshFilters.Length > 0;
        }

        /// <summary>
        /// If the prefab already exists, it will return the prefab root. Otherwise, it will
        /// instantiate a new instance of the prefab or return a best matching GameObject (depending
        /// on the current scene setup.  
        /// </summary>
        /// <param name="prefabAssetPath">Path of the requested prefab.</param>
        /// <param name="rootGameObject">New Parent GameObject of the new Prefab.</param>
        /// <returns>Root GameObject of the corresponding Prefab (or best matching instance).
        /// </returns>
        private static void AddPrefabIfMissing(
            string prefabAssetPath,
            GameObject rootGameObject = null)
        {
            switch (GetPrefabInScene(prefabAssetPath).Length)
            {
                case > 1:
                    throw new Exception(
                        $"There are multiple instances of prefab {prefabAssetPath}. Setup will be aborted.");
                case 1:
                    LogHelper.LogInfo($"Prefab {prefabAssetPath} already exists.");
                    return;
            }
            switch (PrefabComponentsExistsInScene(prefabAssetPath))
            {
                case PrefabExistence.None:
                    InstantiatePrefab(prefabAssetPath, rootGameObject);
                    return;
                case PrefabExistence.Complete:
                    return;
                case PrefabExistence.Partly:
                    var prefabComponents = GetComponentsOfPrefab(prefabAssetPath);
                    var description =
                        $"Some components of '{prefabAssetPath.Split(Path.AltDirectorySeparatorChar).Last()}' are already present in " +
                        "the scene.\n" +
                        "To avoid breaking your scene, this function will not attempt " +
                        "to merge components. Your options:\n" +
                        " A. Remove the duplicate components (see warnings above), then retry. \n" +
                        " B. Add the missing components (see warnings above) yourself.\n";
                    LogPrefabComponentOccurrences(prefabComponents);
                    throw new Exception("Setup was aborted (Expand message for instructions)\n" + description);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum PrefabExistence
        {
            None,
            Partly,
            Complete
        }

        private static GameObject[] GetPrefabInScene(string prefabAssetPath)
        {
            var assetReference = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
            return PrefabUtility.FindAllInstancesOfPrefab(assetReference);
        }

        private static Component[] GetComponentsOfPrefab(string prefabAssetPath)
        {
            var assetReference = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
            return assetReference.GetComponentsInChildren(typeof(Component)).Where(
                component =>
                {
                    return TrackingSetupHelper.componentBlackList.All(
                        blackListElement => component.GetType() != blackListElement);
                }).ToArray();
        }

        private static PrefabExistence PrefabComponentsExistsInScene(string prefabAssetPath)
        {
            var prefabComponents = GetComponentsOfPrefab(prefabAssetPath);
            if (prefabComponents.Length == 0)
            {
                throw new Exception(
                    $"The given prefab {prefabAssetPath} does not have any Component which isn't blacklisted.");
            }

            var prefabComponentsInScene = prefabComponents
                .Select(component => Object.FindObjectsOfType(component.GetType()))
                .Count(componentsInScene => componentsInScene.Length > 0);

            if (prefabComponentsInScene == 0)
            {
                return PrefabExistence.None;
            }
            if (prefabComponentsInScene == prefabComponents.Count())
            {
                return PrefabExistence.Complete;
            }
            return PrefabExistence.Partly;
        }

        /// <summary>
        /// Instantiates the given prefab as a child of the rootGameObject.
        /// </summary>
        /// <param name="prefabAssetPath">Asset reference of the prefab</param>
        /// <param name="rootGameObject">New Parent GameObject of the new Prefab.</param>
        /// <returns>Root GameObject of the new Prefab.</returns>
        /// <exception cref="Exception">Throws an exception if the prefab could not be instantiated.
        /// </exception>
        private static GameObject InstantiatePrefab(
            string prefabAssetPath,
            GameObject rootGameObject = null)
        {
            var assetReference = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
            var parent = rootGameObject != null ? rootGameObject.transform : null;
            var prefab = PrefabUtility.InstantiatePrefab(assetReference, parent) as GameObject;
            Undo.RegisterCreatedObjectUndo(prefab, $"Instantiate prefab {assetReference}");
            if (prefab == null)
            {
                throw new Exception($"Couldn't Instantiate prefab {assetReference}.");
            }
            return prefab;
        }

        /// <summary>
        /// For all components, in the IEnumerable, this will either log if the component is missing
        /// from the scene or log all occurrences of this component.
        /// </summary>
        /// <param name="prefabComponents">
        /// IEnumerable of Components which should be checked.
        /// </param>
        private static void LogPrefabComponentOccurrences(IEnumerable<Component> prefabComponents)
        {
            foreach (var component in prefabComponents)
            {
                var componentsInScene = Object.FindObjectsOfType(component.GetType());
                if (componentsInScene.Length == 0)
                {
                    LogHelper.LogWarning(
                        $"[Missing] Prefab component {component.GetType()} is missing in the scene.");
                    continue;
                }
                foreach (var sceneComponent in componentsInScene)
                {
                    LogHelper.LogWarning(
                        $"[Duplicate] Prefab component {component.GetType()} is present in the scene.",
                        sceneComponent);
                }
            }
        }

        private static GameObject AddAnchorWithSelfAugmentation(GameObject parent)
        {
            var gameObject = new GameObject("VLTrackingAnchor");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create TrackingAnchor");
            if (parent)
            {
                gameObject.transform.SetParent(parent.transform);
            }
            var anchor = Undo.AddComponent<TrackingAnchor>(gameObject);
            SetPracticalDefaultTrackingParameters(anchor);
            TrackingAnchorHelper.CreateRenderedObjectAndLinkToTrackingAnchor(
                RenderedObject.RenderMode.Always,
                anchor,
                gameObject);
            return gameObject;
        }

        private static void SetPracticalDefaultTrackingParameters(TrackingAnchor anchor)
        {
            anchor.SetContourEdgeThreshold(1f);
            anchor.SetCreaseEdgeThreshold(0.3f);
            anchor.SetKeyFrameDistance(10f);
        }

        private static void CreateDefaultModelTrackerConfigIfMissing()
        {
            const string relativeAssetPath =
                "VisionLib/Examples/ModelTracking/DefaultTrackingConfiguration.vl";
            const string projectAssetPath = "Assets/StreamingAssets/" + relativeAssetPath;
            const string defaultConfigPath = "streaming-assets-dir:" + relativeAssetPath;
            const string defaultConfigMetaPath = defaultConfigPath + ".meta";

            if (VLSDK.FileExists(defaultConfigPath))
            {
                return;
            }
            
            VLSDK.Set(
                defaultConfigPath,
                JsonStringConfigurationHelper.CreateTrackingConfigurationString(
                    "multiModelTracker",
                    "{\n\"useColor\": true,\n\"metric\": \"m\"\n}"));
            if (!VLSDK.FileExists(defaultConfigMetaPath))
            {
                VLSDK.Set(
                    defaultConfigMetaPath,
                    "fileFormatVersion: 2\nguid: d5bc94f113a000344a4f852fc25f045e\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n");
            }
            AssetDatabase.ImportAsset(projectAssetPath);
        }
    }
}
