using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView
{
    public enum FilterState
    {
        Any,
        True,
        False
    }

    public static class FilterStateExtensions
    {
        public static bool IsEquivalentTo(this FilterState filterState, TriState triState)
        {
            return filterState switch
            {
                FilterState.Any => true,
                FilterState.True => triState != TriState.False,
                FilterState.False => triState != TriState.True,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(filterState),
                    filterState,
                    "Using invalid filter state")
            };
        }
        
        public static string GenerateFilterButtonText(this FilterState filter, string filterString)
        {
            return filter switch
            {
                FilterState.True => filterString + " Yes",
                FilterState.False => filterString + " No",
                _ => filterString + " Any"
            };
        }
        
    }

    /// <summary>
    /// Tri-State to filter for GameObjects with attached <see cref="TrackingObject"/>. 
    /// </summary>
    public struct TreeViewFilter
    {
        public FilterState isTrackingObject;
        public FilterState isTrackingObjectEnabled;
        public FilterState isMeshRendererEnabled;
        public FilterState isOccluder;

        public static FilterState NextFilterState(FilterState state)
        {
            if (state == FilterState.False)
            {
                return FilterState.Any;
            }
            return state + 1;
        }

        public void Reset()
        {
            this.isTrackingObject = FilterState.Any;
            this.isTrackingObjectEnabled = FilterState.Any;
            this.isMeshRendererEnabled = FilterState.Any;
            this.isOccluder = FilterState.Any;
        }

        public void Filter(TrackingAnchorTreeViewItem root, IList<TreeViewItem> rows)
        {
            if ((this.isTrackingObject != FilterState.Any ||
                 this.isTrackingObjectEnabled != FilterState.Any ||
                 this.isMeshRendererEnabled != FilterState.Any ||
                 this.isOccluder != FilterState.Any) && root != null)
            {
                FilterTree(root, rows);
            }
        }

        private void FilterTree(TrackingAnchorTreeViewItem root, IList<TreeViewItem> rows)
        {
            if (!Matches(root.Data))
            {
                rows.Remove(root);
            }

            if (root.children == null)
            {
                return;
            }
            foreach (var treeElement in root.children.Cast<TrackingAnchorTreeViewItem>()
                         .Where(treeElement => treeElement != null))
            {
                FilterTree(treeElement, rows);
            }
        }

        private bool Matches(TrackingAnchorTreeElement element)
        {
            return this.isTrackingObject.IsEquivalentTo(element.isTrackingObject) &&
                   this.isTrackingObjectEnabled.IsEquivalentTo(element.isTrackingObjectEnabled) &&
                   this.isMeshRendererEnabled.IsEquivalentTo(element.isMeshRendererEnabled) &&
                   this.isOccluder.IsEquivalentTo(element.isOccluder);
        }
    }
}
