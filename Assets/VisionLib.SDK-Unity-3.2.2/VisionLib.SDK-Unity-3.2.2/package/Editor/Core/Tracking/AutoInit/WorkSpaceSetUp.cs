using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @brief Adds menu entries to allow adding SimpleWorkSpace
     * scene.
     *  @ingroup WorkSpace
     */
    public static class WorkSpaceSetUp
    {
        public static void AddSimpleWorkSpace(GameObject destination)
        {
            var workSpaceGO = new GameObject("VLSimpleWorkSpace");
            var simpleWorkSpace = workSpaceGO.AddComponentUndoable<SimpleWorkSpace>();
            simpleWorkSpace.destinationObject = destination;
            ConnectUsedCamera(simpleWorkSpace);
            ConnectToScene(simpleWorkSpace, destination);
            AddToTrackingAnchor(simpleWorkSpace);
            Finalize(workSpaceGO);
        }

        private static void ConnectUsedCamera(WorkSpace workSpace)
        {
            workSpace.usedCamera = CameraProvider.MainCamera;
        }

        private static void ConnectToScene(WorkSpace workSpace, GameObject destination)
        {
            if (!destination)
            {
                return;
            }

            var isTrackingAnchor = (bool) destination.GetComponent<TrackingAnchor>();
            if (isTrackingAnchor)
            {
                GameObjectUtility.SetParentAndAlign(
                    workSpace.gameObject,
                    destination.transform.gameObject);
                return;
            }

            // Ensure it gets re-parented if this was a context click (otherwise do nothing)
            if (destination.transform.parent)
            {
                GameObjectUtility.SetParentAndAlign(
                    workSpace.gameObject,
                    destination.transform.parent.gameObject);
            }
        }

        private static void AddToTrackingAnchor(WorkSpace workSpace)
        {
            var parentTrackingObject = workSpace.gameObject.GetComponentInParent<TrackingAnchor>();
            if (!parentTrackingObject)
            {
                return;
            }
            parentTrackingObject.AddWorkSpace(workSpace);
            EditorUtility.SetDirty(parentTrackingObject);
            LogHelper.LogInfo(
                "Added the new WorkSpace to the parent TrackingAnchor.",
                parentTrackingObject);
        }

        private static void AddGeometryChildren(
            AdvancedWorkSpace advancedWorkSpace,
            GameObject destination)
        {
            advancedWorkSpace.sourceObject = WorkSpaceHelper.CreateSphereGeometryObject(
                "Origin",
                advancedWorkSpace.gameObject,
                advancedWorkSpace.GetOptimalCameraDistance(destination));
            // The Destination Workspace is only created but not used
            WorkSpaceHelper.CreatePointGeometryObject("Destination", advancedWorkSpace.gameObject);
            advancedWorkSpace.destinationObject = destination;
        }

        private static void Finalize(GameObject workSpaceGO)
        {
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(workSpaceGO, "Create " + workSpaceGO.name);
            Selection.activeObject = workSpaceGO;
        }

        [MenuItem("GameObject/VisionLib/AutoInit/Create Simple WorkSpace", false, 10)]
        private static void AddSimpleWorkSpace(MenuCommand menuCommand)
        {
            var destination = menuCommand.context as GameObject;

            var workSpaceGO = new GameObject("VLSimpleWorkSpace");
            var simpleWorkSpace = workSpaceGO.AddComponentUndoable<SimpleWorkSpace>();
            simpleWorkSpace.destinationObject = destination;
            ConnectUsedCamera(simpleWorkSpace);
            ConnectToScene(simpleWorkSpace, destination);
            AddToTrackingAnchor(simpleWorkSpace);
            Finalize(workSpaceGO);
        }

        [MenuItem("GameObject/VisionLib/AutoInit/Create Advanced WorkSpace", false, 10)]
        private static void AddAdvancedWorkSpace(MenuCommand menuCommand)
        {
            var destination = menuCommand.context as GameObject;

            var workSpaceGO = new GameObject("VLAdvancedWorkSpace");
            var advancedWorkSpace = workSpaceGO.AddComponentUndoable<AdvancedWorkSpace>();
            ConnectUsedCamera(advancedWorkSpace);
            ConnectToScene(advancedWorkSpace, destination);
            AddToTrackingAnchor(advancedWorkSpace);
            AddGeometryChildren(advancedWorkSpace, destination);

            var relativeTransform = TransformUtilities.GetRelativeTransform(
                destination.transform,
                advancedWorkSpace.GetRootTransform());
            // Center of the axis-aligned bounding box of the combined sub-meshes. Given in the destination object's local coordinates.
            var center = BoundsUtilities
                .GetMeshBoundsInParentCoordinates(destination, destination.transform, true).center;
            workSpaceGO.transform.localPosition = relativeTransform.MultiplyPoint(center);
            workSpaceGO.transform.localRotation = relativeTransform.rotation;
            Finalize(workSpaceGO);
        }
    }
}
