using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView
{
    /// <summary>
    /// This class corresponds to a visible row in the actual tree. It contains a
    /// <see cref="TrackingAnchorTreeElement"/> from which the data is acquired. If a
    /// <see cref="TrackingAnchorTreeElement"/> should not be displayed, no
    /// <see cref="TrackingAnchorTreeViewItem"/> will be created.
    /// </summary>
    public class TrackingAnchorTreeViewItem : TreeViewItem
    {
        public struct DrawingRects
        {
            public Rect trackingObjectToggle;
            public Rect trackingObjectLabel;
            public Rect enabledToggle;
            public Rect meshRenderersEnabledToggle;
            public Rect isOccluderToggle;
            public Rect searchButtonToggle;
            public Rect combineMeshesButton;

            public DrawingRects(
                Rect trackingObjectToggle,
                Rect trackingObjectLabel,
                Rect enabledToggle,
                Rect meshRenderersEnabledToggle,
                Rect isOccluderToggle,
                Rect searchButtonToggle,
                Rect combineMeshesButton)
            {
                this.trackingObjectToggle = trackingObjectToggle;
                this.trackingObjectLabel = trackingObjectLabel;
                this.enabledToggle = enabledToggle;
                this.meshRenderersEnabledToggle = meshRenderersEnabledToggle;
                this.isOccluderToggle = isOccluderToggle;
                this.searchButtonToggle = searchButtonToggle;
                this.combineMeshesButton = combineMeshesButton;
            }
        }

        public TrackingAnchorTreeElement Data { get; }

        public TrackingAnchorTreeViewItem(TrackingAnchorTreeElement data, int depth)
            : base(data.elementID, depth, data.elementName)
        {
            this.Data = data;
        }

        public void DrawAsRow(DrawingRects rects)
        {
            var gameObject = GetGameObject(this.id);
            if (!gameObject)
            {
                return;
            }

            var includeInactiveChildren = !IsActiveRow(gameObject);

            this.Data.isTrackingObject = TriStateHelper.IsTrackingObject(
                gameObject,
                includeInactiveChildren);
            this.Data.isTrackingObjectEnabled =
                TriStateHelper.IsTrackingObjectEnabled(gameObject, includeInactiveChildren);
            this.Data.isOccluder = TriStateHelper.IsOccluder(gameObject, includeInactiveChildren);
            this.Data.isMeshRendererEnabled =
                TriStateHelper.IsMeshRendererEnabled(gameObject, includeInactiveChildren);

            EditorGUI.BeginDisabledGroup(includeInactiveChildren);
            DrawTrackingObject(
                rects.trackingObjectToggle,
                rects.trackingObjectLabel,
                this.Data.isTrackingObject,
                this.displayName,
                gameObject);
            EditorGUI.BeginDisabledGroup(this.Data.isTrackingObject == TriState.False);
            DrawTrackingObjectEnabledCheckBox(
                rects.enabledToggle,
                this.Data.isTrackingObjectEnabled,
                gameObject);
            EditorGUI.EndDisabledGroup();
            DrawMeshRendererEnabledCheckBox(
                rects.meshRenderersEnabledToggle,
                this.Data.isMeshRendererEnabled,
                gameObject);
            EditorGUI.BeginDisabledGroup(this.Data.isTrackingObject == TriState.False);
            DrawOccluderEnabledCheckBox(rects.isOccluderToggle, this.Data.isOccluder, gameObject);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            DrawSearchGameObjectInHierarchyButton(rects.searchButtonToggle, gameObject);

            bool canBeMerged = CanBeMerged(gameObject);
            EditorGUI.BeginDisabledGroup(!canBeMerged);
            DrawCombineMeshesButton(rects.combineMeshesButton, gameObject);
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawTrackingObject(
            Rect toggleRect,
            Rect labelRect,
            TriState state,
            string label,
            GameObject gameObject)
        {
            // If toggle is visible
            if (toggleRect.xMax < labelRect.xMax)
            {
                if (TriStateToggle.DrawTriStateToggle(toggleRect, state))
                {
                    switch (state)
                    {
                        case TriState.False:
                            TrackingObjectHelper.AddTrackingMeshesInSubTree(gameObject);
                            break;
                        case TriState.True:
                            TrackingObjectHelper.RemoveTrackingMeshesInSubTree(gameObject);
                            break;
                    }
                }
            }
            EditorGUI.LabelField(labelRect, label);
        }

        private static void DrawTrackingObjectEnabledCheckBox(
            Rect cellRect,
            TriState state,
            GameObject gameObject)
        {
            DrawAndHandleTriStateCheckbox(
                cellRect,
                state,
                boolValue =>
                    TrackingObjectHelper.SetTrackingActiveValueInSubTree(gameObject, boolValue));
        }

        private static void DrawMeshRendererEnabledCheckBox(
            Rect cellRect,
            TriState state,
            GameObject gameObject)
        {
            DrawAndHandleTriStateCheckbox(
                cellRect,
                state,
                boolValue =>
                    TrackingObjectHelper.SetMeshRenderersEnabledInSubtree(gameObject, boolValue));
        }

        private static void DrawOccluderEnabledCheckBox(
            Rect cellRect,
            TriState state,
            GameObject gameObject)
        {
            DrawAndHandleTriStateCheckbox(
                cellRect,
                state,
                boolValue => TrackingObjectHelper.SetOccluderValueInSubTree(gameObject, boolValue));
        }

        private static void DrawAndHandleTriStateCheckbox(
            Rect cellRect,
            TriState state,
            Action<bool> stateChangeReaction)
        {
            if (TriStateToggle.DrawTriStateToggle(cellRect, state))
            {
                switch (state)
                {
                    case TriState.False:
                        stateChangeReaction(true);
                        break;
                    case TriState.True:
                    case TriState.Mixed:
                        stateChangeReaction(false);
                        break;
                }
            }
        }

        private static void DrawSearchGameObjectInHierarchyButton(
            Rect cellRect,
            GameObject gameObject)
        {
            if (GUI.Button(
                    cellRect,
                    GUIHelper.GenerateGUIContentWithIcon(
                        GUIHelper.Icons.SearchIcon,
                        "Reveal GameObject in the Hierarchy")))
            {
                EditorGUIUtility.PingObject(gameObject);
            }
        }

        private static void DrawCombineMeshesButton(Rect cellRect, GameObject gameObject)
        {
            if (GUI.Button(
                    cellRect,
                    GUIHelper.GenerateGUIContentWithIcon(
                        GUIHelper.Icons.CombineMeshesIcon,
                        "Combine Meshes")))
            {
                TrackingAnchorHelper.MergeAllMeshFilterIntoNewGameObject(gameObject);
            }
        }

        private static bool IsActiveRow(GameObject gameObject)
        {
            var noMeshFilterSelfOrInChildren =
                gameObject.GetComponentsInChildren<MeshFilter>().Length == 0;

            return gameObject.activeInHierarchy && !noMeshFilterSelfOrInChildren;
        }

        private bool CanBeMerged(GameObject gameObject)
        {
            var hasMeshFilterInChildren =
                gameObject.GetComponentsInChildren<MeshFilter>().Length > 0;
            var isRoot = this.parent is {parent: null};
            return this.hasChildren && hasMeshFilterInChildren && !isRoot;
        }

        private static GameObject GetGameObject(int instanceID)
        {
            return (GameObject) EditorUtility.InstanceIDToObject(instanceID);
        }
    }
}
