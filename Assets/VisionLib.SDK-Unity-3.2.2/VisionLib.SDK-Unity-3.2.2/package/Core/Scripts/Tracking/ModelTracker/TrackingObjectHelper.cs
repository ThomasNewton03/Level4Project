using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public static class TrackingObjectHelper
    {
        public class InvalidTargetException : Exception
        {
            public InvalidTargetException(string message)
                : base(message) {}
        }

        private static ObjectType[] GetComponentsInChildrenWithValidation<ObjectType>(
            GameObject targetGameObject)
        {
            if (!targetGameObject)
            {
                throw new InvalidTargetException("No GameObject provided.");
            }
            if (!targetGameObject.activeInHierarchy)
            {
                throw new InvalidTargetException("Inactive GameObject provided.");
            }
            var objects = targetGameObject.GetComponentsInChildren<ObjectType>();

            if (objects.Length < 1)
            {
                throw new InvalidTargetException(
                    "No objects of type " + typeof(ObjectType).Name +
                    " found in the hierarchy underneath the provided GameObject.");
            }
            return objects;
        }

        public static void AddTrackingMeshesInSubTree(GameObject gameObject)
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(
                gameObject,
                "Add TrackingMeshes on all children of " + gameObject);
#endif

            var meshFilters = GetComponentsInChildrenWithValidation<MeshFilter>(gameObject);

            foreach (var meshFilter in meshFilters)
            {
                var go = meshFilter.transform.gameObject;
                if (go.activeInHierarchy && !go.GetComponent<TrackingObject>())
                {
                    go.AddComponentUndoable<TrackingMesh>();
                }
            }
        }

        public static void RemoveTrackingMeshesInSubTree(GameObject gameObject)
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(
                gameObject,
                "Remove TrackingMeshes of all children of " + gameObject);
#endif

            var trackingObjects = GetComponentsInChildrenWithValidation<TrackingObject>(gameObject);

            foreach (var trackingObject in trackingObjects)
            {
                trackingObject.Destroy();
            }
        }

        public static void SetTrackingActiveValueInSubTree(GameObject gameObject, bool isActive)
        {
            SetBoolValueOnAllComponentsInSubtree<TrackingObject>(
                gameObject,
                isActive,
                (trackingObject, boolValue) => trackingObject.enabled = boolValue);
        }

        public static void SetMeshRenderersEnabledInSubtree(GameObject gameObject, bool isEnabled)
        {
            SetBoolValueOnAllComponentsInSubtree<MeshRenderer>(
                gameObject,
                isEnabled,
                (meshRenderer, boolValue) => meshRenderer.enabled = boolValue);
        }

        public static void SetOccluderValueInSubTree(GameObject gameObject, bool isOccluder)
        {
            SetBoolValueOnAllComponentsInSubtree<TrackingObject>(
                gameObject,
                isOccluder,
                (trackingObject, boolValue) => trackingObject.occluder = boolValue);
        }

        public static void SetMeshRendererMaterialsInSubtree(
            GameObject gameObject,
            Material material)
        {
            if (!material)
            {
                return;
            }

#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Set MeshRenderer materials in subtree");
#endif

            foreach (var meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = material;
            }
        }

        private static void SetBoolValueOnAllComponentsInSubtree<ComponentType>(
            GameObject gameObject,
            bool value,
            Action<ComponentType, bool> setValueAction) where ComponentType : Component
        {
#if UNITY_EDITOR
            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Set bool value on all components in subtree");
#endif
            if (gameObject == null || !gameObject.activeInHierarchy)
            {
                return;
            }
            var components = gameObject.GetComponentsInChildren<ComponentType>();
            foreach (var component in components)
            {
                if (component.transform.gameObject.activeInHierarchy)
                {
                    setValueAction(component, value);
                }
            }
        }

#if UNITY_EDITOR
        public enum LoadableAsset
        {
            SemiTransparentDefaultMaterial
        }

        private static string GetGUIDFromLoadableAsset(LoadableAsset loadableAsset)
        {
            return loadableAsset switch
            {
                LoadableAsset.SemiTransparentDefaultMaterial => "890f8a6c20da28fd3844a3a152141570",
                _ => throw new ArgumentException("Attempted to get GUID of invalid GUIDProxy.")
            };
        }

        public static TAssetType LoadAsset<TAssetType>(LoadableAsset loadableAsset) where TAssetType : class
        {
            var objectGUID = GetGUIDFromLoadableAsset(loadableAsset);
            var assetPath = AssetDatabase.GUIDToAssetPath(objectGUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new NullReferenceException(
                    "Can't find object for " + loadableAsset + " with GUID " + objectGUID +
                    ". The file may have been removed or the meta file has been modified.");
            }
            var loadedAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TAssetType)) as TAssetType;
            if (loadedAsset == null)
            {
                throw new ArgumentException(
                    "Asset " + loadableAsset + " isn't of type " + typeof(TAssetType));
            }
            return loadedAsset;
        }
#endif
    }
}
