using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public class TrackingObjectEditor : Editor
    {
        protected TrackingObject trackingObject;
        protected SerializedProperty occluderProperty;
        protected SerializedProperty useLinesProperty;
        protected SerializedProperty rootTransformProperty;

        private const string occluderTooltip =
            "Tell VisionLib whether to treat this model as a tracking occluder.";
        private const string useLinesTooltip =
            "Use manual lines defined in this model for tracking.";
        private const string rootTransformTooltip =
            "The transform relative to the Root Transform will be considered as the transform of this mesh inside the model.";

        protected void OnEnable()
        {
            this.occluderProperty = this.serializedObject.FindProperty("occluder");
            this.useLinesProperty = this.serializedObject.FindProperty("useLines");
            this.rootTransformProperty = this.serializedObject.FindProperty("rootTransform");
            this.trackingObject = this.serializedObject.targetObject as TrackingObject;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            DrawTrackingObjectProperties();
        }

        protected void DrawTrackingObjectProperties()
        {
            DrawOccluderPropertyField();
            DrawUseLinesPropertyField();
            DrawRootTransformField();
        }

        protected void DrawTrackingObjectDimensions()
        {
            EditorGUILayout.LabelField(
                "Tracking geometry dimensions (red x green x blue): ",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField(
                this.trackingObject.GetModelDimensionsString(),
                EditorStyles.wordWrappedLabel);
            GUI.enabled = true;
        }

        private void DrawOccluderPropertyField()
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(
                    this.occluderProperty,
                    GUIHelper.GenerateGUIContentWithIcon(
                        GUIHelper.Icons.OccluderEnabledIcon,
                        TrackingObjectEditor.occluderTooltip,
                        "Occluder"));
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawUseLinesPropertyField()
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(
                    this.useLinesProperty,
                    new GUIContent("Use Lines", TrackingObjectEditor.useLinesTooltip));
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawRootTransformField()
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(
                    this.rootTransformProperty,
                    new GUIContent("Root Transform", TrackingObjectEditor.rootTransformTooltip));
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public static class TrackingObjectEditorExtensions
    {
        private const string modelTrackingContextMenuPath = "GameObject/VisionLib/Model Tracking/";
        private const string useForTrackingCommand = "Enable Meshes in Tracker";
        private const string disregardForTrackingCommand = "Disable Meshes in Tracker";
        private const string setOccluderCommand = "Set Meshes as Occluder";
        private const string unsetOccluderCommand = "Unset Meshes as Occluder";
        private const string addTrackingObjectsCommand = "Auto-Add TrackingMesh Components";

        [MenuItem(
            TrackingObjectEditorExtensions.modelTrackingContextMenuPath +
            TrackingObjectEditorExtensions.addTrackingObjectsCommand)]
        private static void AddTrackingObjectsInSubTree(MenuCommand command)
        {
            try
            {
                var targetGameObject = command.context as GameObject;
                Undo.RegisterFullObjectHierarchyUndo(
                    targetGameObject,
                    "Add TrackingMeshes on all children of " + targetGameObject);
                TrackingObjectHelper.AddTrackingMeshesInSubTree(targetGameObject);
            }
            catch (TrackingObjectHelper.InvalidTargetException e)
            {
                LogHelper.LogWarning(
                    e.Message + " Ignoring command \"" +
                    TrackingObjectEditorExtensions.addTrackingObjectsCommand + "\".");
            }
        }

        [MenuItem(
            TrackingObjectEditorExtensions.modelTrackingContextMenuPath +
            TrackingObjectEditorExtensions.useForTrackingCommand)]
        private static void EnableTrackingObjectsInSubTree(MenuCommand command)
        {
            try
            {
                var targetGameObject = command.context as GameObject;
                Undo.RegisterFullObjectHierarchyUndo(
                    targetGameObject,
                    "Enable TrackingMeshes on all children of " + targetGameObject);
                TrackingObjectHelper.SetTrackingActiveValueInSubTree(targetGameObject, true);
            }
            catch (TrackingObjectHelper.InvalidTargetException e)
            {
                LogHelper.LogWarning(
                    e.Message + " Ignoring command \"" +
                    TrackingObjectEditorExtensions.useForTrackingCommand + "\".");
            }
        }

        [MenuItem(
            TrackingObjectEditorExtensions.modelTrackingContextMenuPath +
            TrackingObjectEditorExtensions.disregardForTrackingCommand)]
        private static void DisableTrackingObjectsInSubTree(MenuCommand command)
        {
            try
            {
                var targetGameObject = command.context as GameObject;
                Undo.RegisterFullObjectHierarchyUndo(
                    targetGameObject,
                    "Disable TrackingMeshes on all children of " + targetGameObject);
                TrackingObjectHelper.SetTrackingActiveValueInSubTree(targetGameObject, false);
            }
            catch (TrackingObjectHelper.InvalidTargetException e)
            {
                LogHelper.LogWarning(
                    e.Message + " Ignoring command \"" +
                    TrackingObjectEditorExtensions.disregardForTrackingCommand + "\".");
            }
        }

        [MenuItem(
            TrackingObjectEditorExtensions.modelTrackingContextMenuPath +
            TrackingObjectEditorExtensions.setOccluderCommand)]
        private static void EnableOccluderInSubTree(MenuCommand command)
        {
            try
            {
                var targetGameObject = command.context as GameObject;
                Undo.RegisterFullObjectHierarchyUndo(
                    targetGameObject,
                    "Enable occluder on TrackingMeshes on all children of " + targetGameObject);
                TrackingObjectHelper.SetOccluderValueInSubTree(targetGameObject, true);
            }
            catch (TrackingObjectHelper.InvalidTargetException e)
            {
                LogHelper.LogWarning(
                    e.Message + " Ignoring command \"" +
                    TrackingObjectEditorExtensions.setOccluderCommand + "\".");
            }
        }

        [MenuItem(
            TrackingObjectEditorExtensions.modelTrackingContextMenuPath +
            TrackingObjectEditorExtensions.unsetOccluderCommand)]
        private static void DisableOccluderInSubTree(MenuCommand command)
        {
            try
            {
                var targetGameObject = command.context as GameObject;
                Undo.RegisterFullObjectHierarchyUndo(
                    targetGameObject,
                    "Disable occluder on TrackingMeshes on all children of " + targetGameObject);
                TrackingObjectHelper.SetOccluderValueInSubTree(targetGameObject, false);
            }
            catch (TrackingObjectHelper.InvalidTargetException e)
            {
                LogHelper.LogWarning(
                    e.Message + " Ignoring command \"" +
                    TrackingObjectEditorExtensions.unsetOccluderCommand + "\".");
            }
        }
    }
}
