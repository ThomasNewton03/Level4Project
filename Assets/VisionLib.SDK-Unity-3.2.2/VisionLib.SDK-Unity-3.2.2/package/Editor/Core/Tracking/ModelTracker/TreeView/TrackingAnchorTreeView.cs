using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView
{
    /// <summary>
    /// IMGUI based control to display the GameObject hierarchy of a given <see cref="TrackingAnchor"/> as a
    /// tree within an editor window (e.g. the Inspector). The UI elements added in this class
    /// modify the <see cref="TrackingMesh"/> of child GameObjects.
    /// </summary>
    public class TrackingAnchorTreeView : TreeView
    {
        private TrackingAnchorTreeElement rootElement;
        private readonly Transform treeRootTransform;
        private const float rowHeights = 20f;
        private const float toggleWidth = 28f;
        public TreeViewFilter treeViewFilter;

        public TrackingAnchorTreeView(
            TreeViewState state,
            MultiColumnHeader multiColumnHeader,
            Transform treeRootTransform)
            : base(state, multiColumnHeader)
        {
            if (treeRootTransform == null)
            {
                throw new ArgumentNullException(
                    nameof(treeRootTransform),
                    "Input treeRootTransform is null. Ensure input is a non-null list.");
            }
            this.treeRootTransform = treeRootTransform;

            this.rootElement = TrackingAnchorTreeElement.GenerateTree(treeRootTransform);
            this.rowHeight = TrackingAnchorTreeView.rowHeights;
            this.columnIndexForTreeFoldouts = 0;
            this.showAlternatingRowBackgrounds = false;
            this.showBorder = true;
            this.customFoldoutYOffset =
                (TrackingAnchorTreeView.rowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            this.extraSpaceBeforeIconAndLabel = 20f;

            Reload();
            ExpandAll();
        }

        protected override TreeViewItem BuildRoot()
        {
            const int depthForHiddenRoot = -1;
            return new TrackingAnchorTreeViewItem(this.rootElement, depthForHiddenRoot);
        }

        protected override IList<int> GetAncestors(int id)
        {
            var parents = new List<int>();
            var element = this.rootElement.Find(id);
            if (element == null)
            {
                return parents;
            }
            while (element.elementParent != null)
            {
                parents.Add(element.elementParent.elementID);
                element = element.elementParent;
            }
            return parents;
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            var searchRoot = this.rootElement.Find(id);
            if (searchRoot == null)
            {
                return new List<int>();
            }

            var descendants = new Stack<TrackingAnchorTreeElement>();
            descendants.Push(searchRoot);

            var descendantsWithChildren = new List<int>();
            while (descendants.Count > 0)
            {
                var current = descendants.Pop();
                if (current.HasChildren)
                {
                    descendantsWithChildren.Add(current.elementID);
                    foreach (var child in current.elementChildren)
                    {
                        descendants.Push(child);
                    }
                }
            }
            return descendantsWithChildren;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (this.rootElement == null)
            {
                Debug.LogError("Tree model root is null. Model isn't correctly initialized.");
                return new List<TreeViewItem>();
            }

            var rows = CreateListOfAllExpandedChildren(this.rootElement);

            // We still need to set up the child parent information for the rows since this 
            // information is used by the TreeView internal logic (navigation, dragging etc)
            SetupParentsAndChildrenFromDepths(root, rows);

            this.treeViewFilter.Filter((TrackingAnchorTreeViewItem) this.rootItem, rows);
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TrackingAnchorTreeViewItem) args.item;

            var trackingObjectCellRect = args.GetCellRect(0);
            var trackingObjectToggleRect = CalculateToggleBoxRect(args.GetCellRect(0));
            trackingObjectToggleRect.x += GetContentIndent(item);
            trackingObjectCellRect.xMin = trackingObjectToggleRect.xMax;

            EditorGUI.BeginChangeCheck();
            item.DrawAsRow(
                new TrackingAnchorTreeViewItem.DrawingRects(
                    trackingObjectToggleRect,
                    trackingObjectCellRect,
                    CalculateToggleBoxRect(args.GetCellRect(1)),
                    CalculateToggleBoxRect(args.GetCellRect(2)),
                    CalculateToggleBoxRect(args.GetCellRect(3)),
                    args.GetCellRect(4),
                    args.GetCellRect(5)
                    ));

            if (EditorGUI.EndChangeCheck())
            {
                this.rootElement = TrackingAnchorTreeElement.GenerateTree(this.treeRootTransform);
                Reload();
            }
        }
        
        private Rect CalculateToggleBoxRect(Rect cellRect)
        {
            var toggleRect = cellRect;
            toggleRect.width = TrackingAnchorTreeView.toggleWidth;
            return toggleRect;
        }
        private IList<TreeViewItem> CreateListOfAllExpandedChildren(
            TrackingAnchorTreeElement rootNode)
        {
            var rows = new List<TreeViewItem>();
            if (rootNode.HasChildren)
            {
                AddChildrenRecursive(rootNode, 0, rows);
            }
            return rows;
        }

        private void AddChildrenRecursive(
            TrackingAnchorTreeElement parent,
            int depth,
            IList<TreeViewItem> newRows)
        {
            foreach (var child in parent.elementChildren)
            {
                var item = new TrackingAnchorTreeViewItem(child, depth);
                newRows.Add(item);

                if (!child.HasChildren)
                {
                    continue;
                }
                if (IsExpanded(child.elementID))
                {
                    AddChildrenRecursive(child, depth + 1, newRows);
                }
                else
                {
                    item.children = CreateChildListForCollapsedParent();
                }
            }
        }
    }
}
