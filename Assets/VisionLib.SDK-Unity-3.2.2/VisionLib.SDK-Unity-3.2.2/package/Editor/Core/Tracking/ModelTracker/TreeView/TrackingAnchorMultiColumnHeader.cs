using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView
{
    public class TrackingAnchorMultiColumnHeader : MultiColumnHeader
    {
        const float DefaultToggleColumnWidth = 50f;
        const float DefaultButtonColumnWidth = 30f;
        public TrackingAnchorMultiColumnHeader()
            : base(CreateDefaultTrackingAnchorHeaderState())
        {
            this.canSort = false;
            this.height = DefaultGUI.defaultHeight;
            ResizeToFit();
        }

        private static MultiColumnHeaderState CreateDefaultTrackingAnchorHeaderState()
        {
            var columns = new[]
            {
                GenerateGameObjectNameHeader(), GenerateTrackingObjectEnabledHeader(),
                GenerateMeshRendererEnabledHeader(), GenerateOccluderHeader(),
                GenerateSearchGameObjectsHeader(), GenerateCombineMeshesHeader()
            };
            return new MultiColumnHeaderState(columns);
        }

        private static MultiColumnHeaderState.Column GenerateGameObjectNameHeader()
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.TrackingObjectIcon,
                    "TrackingObject presence on and under each GameObject." +
                    " Only GameObjects with Models are considered. " +
                    "Those without models are shown but greyed out.",
                    " Tracking Object"),
                contextMenuText = "Type",
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = 300,
                minWidth = 300,
                maxWidth = 600,
                autoResize = false,
                allowToggleVisibility = true,
                canSort = false
            };
        }

        private static MultiColumnHeaderState.Column GenerateTrackingObjectEnabledHeader()
        {
            return CreateToggleColumn(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.EnabledForTrackingIcon,
                    "Cumulated TrackingObject enabled state for each GameObject." +
                    " Shows whether the geometry on and beneath the object is currently being" +
                    " used by VisionLib for tracking."));
        }

        private static MultiColumnHeaderState.Column GenerateMeshRendererEnabledHeader()
        {
            return CreateToggleColumn(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.MeshRendererEnabledIcon,
                    "Cumulated MeshRenderer enabled state for each GameObject." +
                    " Shows whether the meshes on and beneath the object are currently" +
                    " rendered in the game view by Unity." +
                    " This is a visualization setting that does not affect tracking."));
        }

        private static MultiColumnHeaderState.Column GenerateOccluderHeader()
        {
            return CreateToggleColumn(
                GUIHelper.GenerateGUIContentWithIcon(
                    GUIHelper.Icons.OccluderEnabledIcon,
                    "Cumulated TrackingObject occluder state for each GameObject." +
                    " Shows whether the geometry on and beneath the object is currently" +
                    " treated as a tracking occluder."));
        }

        private static MultiColumnHeaderState.Column GenerateSearchGameObjectsHeader()
        {
            return CreateButtonColumn();
        }

        private static MultiColumnHeaderState.Column GenerateCombineMeshesHeader()
        {
            return CreateButtonColumn();
        }
        
        private static MultiColumnHeaderState.Column CreateToggleColumn(GUIContent guiContent)
        {
            return CreateColumn(guiContent, DefaultToggleColumnWidth);
        }

         private static MultiColumnHeaderState.Column CreateButtonColumn()
        {
            return CreateColumn(new GUIContent(), DefaultButtonColumnWidth);
        }

        private static MultiColumnHeaderState.Column CreateColumn(
            GUIContent guiContent,
            float width)
        {
            
            return new MultiColumnHeaderState.Column
            {
                headerContent = guiContent,
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                autoResize = false,
                allowToggleVisibility = true,
                contextMenuText = guiContent.text,
                canSort = false,
                width = width,
                minWidth = width,
                maxWidth = width
            };
        }


    }
}
