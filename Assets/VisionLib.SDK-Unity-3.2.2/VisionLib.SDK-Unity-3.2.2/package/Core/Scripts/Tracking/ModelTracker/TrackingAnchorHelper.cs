using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    public static class TrackingAnchorHelper
    {
        /// <summary>
        /// Option to filter according to the occluder status of a TrackingMesh
        /// </summary>
        public enum OccluderFilter
        {
            NoFilter,
            IsOccluder,
            IsNoOccluder
        }

        /// <summary>
        /// Option to filter according to the whether the TrackingMesh is active.
        /// </summary>
        public enum ActivityFilter
        {
            NoFilter,
            IsActive,
            IsDeactivated
        }

        /// <summary>
        /// Collects the TrackingMeshes from all child nodes below the given root node. 
        /// </summary>
        /// <param name="rootNode">Root GameObject from where to search all child nodes.</param>
        /// <returns>All TrackingMeshes from all child nodes.</returns>
        public static TrackingMesh[] GetTrackingMeshes(GameObject rootNode)
        {
            return rootNode.GetComponentsInChildren<TrackingMesh>();
        }

        /// <summary>
        /// Collects all TrackingMeshes from all child nodes below the given root node matching the
        /// specified conditions.
        /// </summary>
        /// <param name="rootNode">Root GameObject from where to search all child nodes.</param>
        /// <param name="active">Specifies if the returned TrackingMeshes should be active,
        /// inactive, or if the activity status of the TrackingMesh shouldn't be considered.
        /// </param>
        /// <param name="occluder">Specifies if the returned TrackingMeshes should be occluder,
        /// objects used for tracking, or if the occluder status of the TrackingMesh shouldn't be
        /// considered.</param>
        /// <returns>Filtered TrackingMeshes matching the specified conditions.</returns>
        public static TrackingMesh[] GetFilteredTrackingMeshes(
            GameObject rootNode,
            ActivityFilter active,
            OccluderFilter occluder)
        {
            return GetTrackingMeshes(rootNode).Where(
                    trackingMesh =>
                        Matches(trackingMesh, active) && Matches(trackingMesh, occluder)) as
                TrackingMesh[];
        }

        /// <summary>
        /// Collects all TrackingMeshes which are active occluders from all child nodes below the
        /// given root node. 
        /// </summary>
        /// <param name="rootNode">Root GameObject from where to search all child nodes.</param>
        /// <returns>Active TrackingMeshes from any child nodes, which are occluders.</returns>
        public static TrackingMesh[] GetActiveOccluders(GameObject rootNode)
        {
            return GetFilteredTrackingMeshes(
                rootNode,
                ActivityFilter.IsActive,
                OccluderFilter.IsOccluder);
        }

        /// <summary>
        /// Collects all TrackingMeshes which are active and not used as occluders from all child
        /// nodes below the given root node. 
        /// </summary>
        /// <param name="rootNode">Root GameObject from where to search all child nodes.</param>
        /// <returns>Active TrackingMeshes from any child nodes, which are used for tracking.
        /// </returns>
        public static TrackingMesh[] GetActiveTrackingMeshesUsedForTracking(GameObject rootNode)
        {
            return GetFilteredTrackingMeshes(
                rootNode,
                ActivityFilter.IsActive,
                OccluderFilter.IsNoOccluder);
        }

        public static bool IsReferenceValidAndEnabled(this TrackingAnchor anchor)
        {
            return anchor && anchor.isActiveAndEnabled;
        }

        /// \deprecated TrackingAnchorHelper.CreateUniqueName() is obsolete and will be removed in the future. Use TrackingAnchor.CreateUniqueName() instead
        [Obsolete("TrackingAnchorHelper.CreateUniqueName() is obsolete and will be removed in the future. Use TrackingAnchor.CreateUniqueName() instead")]
        public static string CreateUniqueName()
        {
            return TrackingAnchor.CreateUniqueName();
        }

        public static InitPoseInteraction AddInitPoseInteraction(this TrackingAnchor trackingAnchor)
        {
            var trackingAnchorGO = trackingAnchor.gameObject;
            var existingInteraction = trackingAnchorGO.GetComponent<InitPoseInteraction>();
            if (existingInteraction)
            {
                LogHelper.LogWarning(
                    "Skipped adding InitPoseInteraction since it is already present.");
                return existingInteraction;
            }
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(
                trackingAnchorGO,
                "Set gameViewCamera in GameObjectPoseInteraction to SLAMCamera of TrackingAnchor");
#endif
            var gameObjectInteraction =
                trackingAnchorGO.AddComponentUndoable<GameObjectPoseInteraction>();
            gameObjectInteraction.SetInteractionCamera(trackingAnchor.GetSLAMCamera());
            return trackingAnchorGO.AddComponentUndoable<InitPoseInteraction>();
        }

        public static void RemoveInitPoseInteraction(this TrackingAnchor trackingAnchor)
        {
            RemoveInitPoseInteraction(trackingAnchor.gameObject);
        }

        public static void RemoveInitPoseInteraction(GameObject gameObject)
        {
            gameObject.DestroyComponent<InitPoseInteraction>();
            gameObject.DestroyComponent<GameObjectPoseInteraction>();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Merge all mesh filter into new GameObject")]
        public static void MergeAllMeshFilterIntoNewGameObject(MenuCommand menuCommand)
        {
            var gameObject = (GameObject) menuCommand.context;
            Undo.RegisterFullObjectHierarchyUndo(gameObject, "Combine Meshes");
            MergeAllMeshFilterIntoNewGameObject(gameObject);
        }

        [MenuItem("GameObject/Merge all mesh filter into new GameObject", true)]
        public static bool ValidateMergeAllMeshFilterIntoNewGameObject()
        {
            if (Selection.activeTransform == null)
            {
                return false;
            }
            var gameObject = Selection.activeTransform.gameObject;
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            return meshFilters.Length > 0;
        }
#endif

        public static void MergeAllMeshFilterIntoNewGameObject(GameObject gameObject)
        {
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length == 0)
            {
                return;
            }

#if UNITY_EDITOR
            UnpackIfPrefab(gameObject);
            CheckForUnconsideredMeshes(meshFilters);
#endif

            CombineMeshFilter(gameObject, "", false);
            CombineMeshFilter(gameObject, "_occluder", true);

            gameObject.SetActive(false);
            gameObject.Destroy();
        }

        public static void MergeRootTrackingAnchorAndSlamCameraPosesOnce()
        {
            var trackingAnchors = Object.FindObjectsOfType<TrackingAnchor>(true);
            var trackinRootAnchors = trackingAnchors.Where(anchor => !anchor.HasParentAnchor());

            var groupsBySlamCamera =
                trackinRootAnchors.GroupBy(trackingAnchor => trackingAnchor.GetSLAMCamera());

            var wasAtLeastOneSlamCameraTransformModified = false;

            foreach (var group in groupsBySlamCamera)
            {
                var slamCamera = group.Key;
                if (!slamCamera || slamCamera.transform.IsWorldPoseIdentity())
                {
                    continue;
                }

                foreach (var trackingAnchor in group)
                {
                    CombineTransformationsInB(slamCamera.transform, trackingAnchor.transform);
                }
                ResetTransform(slamCamera.transform);
                wasAtLeastOneSlamCameraTransformModified = true;
            }

            if (wasAtLeastOneSlamCameraTransformModified)
            {
                LogHelper.LogInfo(
                    "Some SLAM camera's transforms weren't identity. The " +
                    " transforms of all linked TrackingAnchors were corrected for this.\n" +
                    "(A TrackingAnchor's effective InitPose is the transformation " +
                    "between the SLAM camera and the TrackingAnchor itself.");
            }
        }

        private static void CombineMeshFilter(
            GameObject gameObject,
            string namePostFix,
            bool isOccluder)
        {
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            var inverseBaseTransform = gameObject.transform.worldToLocalMatrix;

            var instances = meshFilters.Where(
                filter =>
                {
                    var trackingMesh = filter.gameObject.GetComponent<TrackingMesh>();
                    return trackingMesh && trackingMesh.enabled &&
                           trackingMesh.occluder == isOccluder;
                }).ToArray();
            var meshRendererEnabled = instances.All(
                filter =>
                {
                    var meshRenderer = filter.gameObject.GetComponent<MeshRenderer>();
                    return meshRenderer.enabled;
                });
            var combineInstances = instances.Select(
                filter => new CombineInstance
                {
                    mesh = filter.sharedMesh,
                    transform = inverseBaseTransform * filter.transform.localToWorldMatrix
                }).ToArray();

            if (combineInstances.Length == 0)
            {
                return;
            }

            var sharedMaterial = meshFilters.First().gameObject.GetComponent<MeshRenderer>()
                .sharedMaterial;

            var newMeshGameObject = CreateCombinedTrackingMesh(
                combineInstances,
                sharedMaterial,
                meshRendererEnabled,
                isOccluder);
            newMeshGameObject.name = gameObject.name + namePostFix;
            newMeshGameObject.transform.parent = gameObject.transform.parent;
            newMeshGameObject.transform.localPosition = gameObject.transform.localPosition;
            newMeshGameObject.transform.localRotation = gameObject.transform.localRotation;
            newMeshGameObject.transform.localScale = gameObject.transform.localScale;
        }

        private static GameObject CreateCombinedTrackingMesh(
            CombineInstance[] combineInstances,
            Material combinedMaterial,
            bool meshRendererEnabled,
            bool isOccluder)
        {
            var newMeshGameObject = new GameObject();
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newMeshGameObject, "Combine MeshFilter");
            Undo.RegisterCompleteObjectUndo(
                newMeshGameObject,
                "Modifications to the newly created GameObject");
#endif

            CreateMeshFilter(newMeshGameObject, combineInstances);
            CreateMeshRenderer(newMeshGameObject, combinedMaterial, meshRendererEnabled);
            CreateTrackingMesh(newMeshGameObject, isOccluder);

            newMeshGameObject.SetActive(true);

            return newMeshGameObject;
        }

        private static void CreateMeshFilter(
            GameObject gameObject,
            CombineInstance[] combineInstances)
        {
            var meshFilter = gameObject.AddComponentUndoable<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshFilter.sharedMesh.CombineMeshes(combineInstances);
        }

        private static void CreateMeshRenderer(
            GameObject gameObject,
            Material combinedMaterial,
            bool meshRendererEnabled)
        {
            var meshRenderer = gameObject.AddComponentUndoable<MeshRenderer>();
            meshRenderer.sharedMaterial = combinedMaterial;
            meshRenderer.enabled = meshRendererEnabled;
        }

        private static void CreateTrackingMesh(GameObject gameObject, bool isOccluder)
        {
            var trackingMesh = gameObject.AddComponentUndoable<TrackingMesh>();
            trackingMesh.occluder = isOccluder;
        }

#if UNITY_EDITOR
        private static void UnpackIfPrefab(GameObject gameObject)
        {
            var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            if (prefabRoot == null)
            {
                return;
            }
            var unpackPrefab = EditorUtility.DisplayDialog(
                "Prefab needs to be unpacked",
                "To combine the corresponding MeshFilter, the corresponding prefab has to be unpacked.",
                "Unpack prefab",
                "Stop combining MeshFilter");
            if (!unpackPrefab)
            {
                throw new ArgumentException(
                    "Merging of MeshFilters has been canceled because unpacking of prefab was aborted");
            }
            PrefabUtility.UnpackPrefabInstance(
                prefabRoot,
                PrefabUnpackMode.Completely,
                InteractionMode.UserAction);
        }
#endif

#if UNITY_EDITOR
        private static void CheckForUnconsideredMeshes(MeshFilter[] meshFilters)
        {
            var unaddedMeshes = meshFilters
                .Select(filter => filter.gameObject.GetComponent<TrackingMesh>()).Count(
                    trackingMesh => !trackingMesh || !trackingMesh.enabled);
            if (unaddedMeshes <= 0)
            {
                return;
            }
            var continueExecution = EditorUtility.DisplayDialog(
                "Some meshes are ignored",
                unaddedMeshes +
                " MeshFilter will not be added to the combined MeshFilter because they do not have a 'TrackingMesh' component or it is disabled. They will be deleted.",
                "Delete ignored MeshFilter",
                "Stop combining MeshFilter");
            if (!continueExecution)
            {
                throw new ArgumentException(
                    "Merging of MeshFilters has been canceled because " + unaddedMeshes +
                    " MeshFilter haven't been added to the combined MeshFilter.");
            }
        }
#endif

        public static string[] GetDuplicateAnchorNamesInScene()
        {
            return Object.FindObjectsOfType<TrackingAnchor>()
                .GroupBy(trackingAnchor => trackingAnchor.GetAnchorName())
                .Where(element => element.Count() > 1).Select(element => element.Key).ToArray();
        }

        public static void CreateRenderedObjectAndLinkToTrackingAnchor(
            RenderedObject.RenderMode renderMode,
            TrackingAnchor trackingAnchor,
            GameObject gameObject)
        {
            var renderedObject = gameObject.AddComponentUndoable<RenderedObject>();
            renderedObject.renderMode = renderMode;
            renderedObject.SetTrackingAnchor(trackingAnchor);
        }

        public static GameObject CloneAsRenderedObject(
            this TrackingAnchor trackingAnchor,
            RenderedObject.RenderMode renderMode)
        {
            var cloneParent = new GameObject(
                renderMode + "(" + trackingAnchor.gameObject.name + " clone)");
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(
                cloneParent,
                "Clone children of TrackingAnchor as RenderedObject");
            Undo.RegisterCompleteObjectUndo(
                cloneParent,
                "Modifications to the newly created RenderedObject");
#endif

            var trackingAnchorTransform = trackingAnchor.transform;

            cloneParent.transform.position = trackingAnchorTransform.position;
            cloneParent.transform.rotation = trackingAnchorTransform.rotation;

            CloneChildrenAndStripAnythingButMeshes(trackingAnchorTransform, cloneParent.transform);

            CreateRenderedObjectAndLinkToTrackingAnchor(renderMode, trackingAnchor, cloneParent);

            return cloneParent;
        }

        private static void CloneChildrenAndStripAnythingButMeshes(
            Transform source,
            Transform destination)
        {
            var sourceGameObject = source.gameObject;
            var sourceGameObjectActiveState = sourceGameObject.activeSelf;
            destination.gameObject.SetActive(false);
            foreach (Transform child in source)
            {
                CloneAndStripAnythingButMeshes(child, destination);
            }
            destination.gameObject.SetActive(sourceGameObjectActiveState);
        }

        private static void CloneAndStripAnythingButMeshes(Transform source, Transform destination)
        {
            Object.Instantiate(source.gameObject, destination, true);

            RemoveAllChildAnchors(destination);
            var componentsToDestroy = destination.gameObject.GetComponentsInChildren<Component>()
                .Where(IsNotMeshOrTransform);
            foreach (var component in componentsToDestroy)
            {
                component.Destroy();
            }
            RemoveEmptyGameObjectsInChildren(destination);
        }

        private static bool IsNotMeshOrTransform(Component component)
        {
            return component is not (MeshFilter or MeshRenderer or Transform);
        }

        private static void RemoveAllChildAnchors(Transform transform)
        {
            foreach (Transform child in transform)
            {
                var childGameObject = child.gameObject;
                var trackingAnchor = childGameObject.GetComponent<TrackingAnchor>();
                if (trackingAnchor != null)
                {
                    childGameObject.Destroy();
                    continue;
                }
                RemoveAllChildAnchors(child);
            }
        }

        /// <summary>
        /// Removes all empty GameObjects without children
        /// </summary>
        /// <param name="transform"></param>
        private static void RemoveEmptyGameObjectsInChildren(Transform transform)
        {
            foreach (Transform child in transform)
            {
                RemoveEmptyGameObjectsInChildren(child);
            }
            if (transform.childCount == 0 && transform.GetComponents<Component>().Length <= 1)
            {
                transform.gameObject.Destroy();
            }
        }

        public static bool HasAugmentedContent(this TrackingAnchor trackingAnchor)
        {
            return trackingAnchor.GetRegisteredRenderedObjects().Any();
        }

        public static bool HasMeshes(this TrackingAnchor trackingAnchor)
        {
            return trackingAnchor.HasComponent<MeshFilter>();
        }

        public static bool HasRenderers(this TrackingAnchor trackingAnchor)
        {
            return trackingAnchor.HasComponent<Renderer>();
        }

        public static bool HasTrackingObjects(this TrackingAnchor trackingAnchor)
        {
            return trackingAnchor.HasComponent<TrackingObject>();
        }

        private static bool HasComponent<T>(this TrackingAnchor trackingAnchor)
        {
            return trackingAnchor.gameObject.GetComponentInChildren<T>() != null;
        }

        public static bool MisconfiguredTrackingGeometry(this TrackingAnchor trackingAnchor)
        {
            return !trackingAnchor.HasMeshes() || !trackingAnchor.HasTrackingObjects();
        }

        //B -> A^(-1) * B
        private static void CombineTransformationsInB(Transform a, Transform b)
        {
            var inverseARotation = Quaternion.Inverse(a.rotation);
            b.rotation = inverseARotation * b.rotation;
            b.position = inverseARotation * (b.position - a.position);
        }

        //T -> I
        private static void ResetTransform(Transform t)
        {
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
        }

        private static bool Matches(TrackingMesh trackingMesh, OccluderFilter occluder)
        {
            return occluder switch
            {
                OccluderFilter.NoFilter => true,
                OccluderFilter.IsOccluder => trackingMesh.occluder,
                OccluderFilter.IsNoOccluder => !trackingMesh.occluder,
                _ => throw new ArgumentOutOfRangeException(nameof(occluder), occluder, null)
            };
        }

        private static bool Matches(TrackingMesh trackingMesh, ActivityFilter active)
        {
            return active switch
            {
                ActivityFilter.NoFilter => true,
                ActivityFilter.IsActive => trackingMesh.enabled,
                ActivityFilter.IsDeactivated => !trackingMesh.enabled,
                _ => throw new ArgumentOutOfRangeException(nameof(active), active, null)
            };
        }
    }
}
