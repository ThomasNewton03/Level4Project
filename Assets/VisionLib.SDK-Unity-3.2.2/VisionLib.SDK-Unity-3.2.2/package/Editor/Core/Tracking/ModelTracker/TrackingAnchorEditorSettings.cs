using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public static class TrackingAnchorEditorSettings
    {
        public enum TrackingMeshButtonMode
        {
            AddTrackingMeshes,
            RemoveTrackingMeshes
        }

        public struct TrackingMeshButtonParameters
        {
            public ButtonParameters buttonParameters;
            public string failureMessagePrefix;
        }

        public static readonly ButtonParameters setSelfAsAugmentationButtonParameters =
            new ButtonParameters
            {
                label = TrackingAnchorSettings.selfAugmentationLabel,
                labelTooltip = TrackingAnchorSettings.selfAugmentationTooltip,
                buttonIcon = GUIHelper.Icons.LinkIcon
            };

        public static readonly ButtonParameters cloneAsInitPoseGuideButtonParameters =
            new ButtonParameters
            {
                label = TrackingAnchorSettings.cloneAsInitPoseGuideLabel,
                labelTooltip = TrackingAnchorSettings.cloneAsInitPoseGuideTooltip,
                buttonIcon = GUIHelper.Icons.PlusIcon
            };

        public static readonly ButtonParameters cloneAsAugmentationButtonParameters =
            new ButtonParameters
            {
                label = TrackingAnchorSettings.cloneAsAugmentationLabel,
                labelTooltip = TrackingAnchorSettings.cloneAsAugmentationTooltip,
                buttonIcon = GUIHelper.Icons.PlusIcon
            };

        public static readonly ButtonParameters cloneAsBothButtonParameters = new ButtonParameters
        {
            label = TrackingAnchorSettings.cloneAsBothLabel,
            labelTooltip = TrackingAnchorSettings.cloneAsBothTooltip,
            buttonIcon = GUIHelper.Icons.PlusIcon
        };

        public static readonly ButtonParameters copyModelHashButtonParameters = new ButtonParameters
        {
            label = TrackingAnchorSettings.copyModelHashLabel,
            labelTooltip = TrackingAnchorSettings.copyToClipboardTooltip,
            buttonIcon = GUIHelper.Icons.DuplicateIcon
        };

        public static readonly ButtonParameters addInteractionButtonParameters =
            new ButtonParameters
            {
                label = TrackingAnchorSettings.addInteractionLabel,
                labelTooltip = TrackingAnchorSettings.addInteractionTooltip,
                buttonIcon = GUIHelper.Icons.PlusIcon
            };

        public static readonly ButtonParameters removeInteractionButtonParameters =
            new ButtonParameters
            {
                label = TrackingAnchorSettings.removeInteractionLabel,
                labelTooltip = TrackingAnchorSettings.removeInteractionTooltip,
                buttonIcon = GUIHelper.Icons.MinusIcon
            };

        public static readonly TrackingMeshButtonParameters addTrackingMeshButtonParameters =
            new TrackingMeshButtonParameters
            {
                buttonParameters = new ButtonParameters
                {
                    label = TrackingAnchorSettings.addTrackingMeshLabel,
                    labelTooltip = TrackingAnchorSettings.addTrackingMeshTooltip,
                    buttonIcon = GUIHelper.Icons.PlusIcon
                },
                failureMessagePrefix = "Adding TrackingMeshes failed: \""
            };

        public static readonly TrackingMeshButtonParameters removeTrackingMeshButtonParameters =
            new TrackingMeshButtonParameters
            {
                buttonParameters = new ButtonParameters
                {
                    label = TrackingAnchorSettings.removeTrackingMeshLabel,
                    labelTooltip = TrackingAnchorSettings.removeTrackingMeshTooltip,
                    buttonIcon = GUIHelper.Icons.MinusIcon
                },
                failureMessagePrefix = "Removing TrackingMeshes failed: \""
            };

        public static readonly ButtonParameters copyLegacyVLInitPoseButtonParameters = new ButtonParameters
        {
            label = TrackingAnchorSettings.copyLegacyVLInitPoseLabel,
            labelTooltip = TrackingAnchorSettings.copyToClipboardTooltip,
            buttonIcon = GUIHelper.Icons.DuplicateIcon
        };
    }
}
