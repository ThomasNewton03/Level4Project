using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView
{
    /// <summary>
    /// Adds a TreeView of the GameObject hierarchy below the given <see cref="TrackingAnchor"/>.
    /// This class encapsulates creation and layout code of all needed TreeView UI controls.
    /// </summary>
    [Serializable]
    public class TrackingAnchorTreeEditor : Editor
    {
        private GUIStyle filterButtonStyle;
        private const float treeViewHeight = 300f;

        private TrackingAnchorTreeView treeView;

        private TrackingAnchor trackingAnchor;
        public TrackingAnchor TrackingAnchor
        {
            get => this.trackingAnchor;
            set => this.trackingAnchor = value;
        }
        
        public override void OnInspectorGUI()
        {
            this.filterButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 20,
                alignment = TextAnchor.MiddleCenter,
                stretchHeight = true,
            };
            CreateTreeView();
            DrawToolBar();
            DrawTreeView();
        }

        public void ResetTreeView()
        {
            this.treeView = null;
            CreateTreeView();
        }

        private void CreateTreeView()
        {
            if (this.treeView != null)
            {
                return;
            }

            this.treeView = new TrackingAnchorTreeView(
                new TreeViewState(),
                new TrackingAnchorMultiColumnHeader(),
                this.trackingAnchor.transform);
        }

        private void DrawToolBar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Filter:", GUILayout.Width(50));
                DrawTrackingMeshExistsButton();
                DrawTrackingObjectEnabledButton();
                DrawMeshRendererEnabledButton();
                DrawOccluderEnabledButton();
                DrawResetFilterButton();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Tree:", GUILayout.Width(50));
                DrawExpandTreeButton();
                DrawCollapseTreeButton();
            }
        }

        private void DrawCollapseTreeButton()
        {
            if (GUILayout.Button("Collapse All", this.filterButtonStyle))
            {
                this.treeView.CollapseAll();
            }
        }

        private void DrawExpandTreeButton()
        {
            if (GUILayout.Button("Expand All", this.filterButtonStyle))
            {
                this.treeView.ExpandAll();
            }
        }

        private void DrawTrackingMeshExistsButton()
        {
            this.treeView.treeViewFilter.isTrackingObject = DrawFilterButton(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.TrackingObjectIcon,
                    "Filter by TrackingObject presence.",
                    this.treeView.treeViewFilter.isTrackingObject.GenerateFilterButtonText("")),
                this.treeView.treeViewFilter.isTrackingObject);
        }

        private void DrawTrackingObjectEnabledButton()
        {
            this.treeView.treeViewFilter.isTrackingObjectEnabled = DrawFilterButton(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.EnabledForTrackingIcon,
                    "Filter by TrackingObject enabled state.",
                    this.treeView.treeViewFilter.isTrackingObjectEnabled.GenerateFilterButtonText(
                        "")),
                this.treeView.treeViewFilter.isTrackingObjectEnabled);
        }

        private void DrawMeshRendererEnabledButton()
        {
            this.treeView.treeViewFilter.isMeshRendererEnabled = DrawFilterButton(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.MeshRendererEnabledIcon,
                    "Filter by MeshRenderer enabled state.",
                    this.treeView.treeViewFilter.isMeshRendererEnabled
                        .GenerateFilterButtonText("")),
                this.treeView.treeViewFilter.isMeshRendererEnabled);
        }

        private void DrawOccluderEnabledButton()
        {
            this.treeView.treeViewFilter.isOccluder = DrawFilterButton(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.OccluderEnabledIcon,
                    "Filter by TrackingObject occluder state.",
                    this.treeView.treeViewFilter.isOccluder.GenerateFilterButtonText("")),
                this.treeView.treeViewFilter.isOccluder);
        }

        private FilterState DrawFilterButton(GUIContent buttonContent, FilterState filter)
        {
            if (GUILayout.Button(buttonContent, this.filterButtonStyle))
            {
                var newFilter = TreeViewFilter.NextFilterState(filter);
                this.treeView.ExpandAll();
                return newFilter;
            }
            return filter;
        }

        private void DrawResetFilterButton()
        {
            if (GUILayout.Button(new GUIContent("Reset"), this.filterButtonStyle))
            {
                this.treeView.treeViewFilter.Reset();
                this.treeView.ExpandAll();
            }
        }

        private void DrawTreeView()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var maxWidth = EditorGUIUtility.currentViewWidth - 35f;
                this.treeView.OnGUI(
                    GUILayoutUtility.GetRect(
                        100f,
                        maxWidth,
                        TrackingAnchorTreeEditor.treeViewHeight,
                        TrackingAnchorTreeEditor.treeViewHeight));
            }
        }
    }
}
