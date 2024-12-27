using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    public class TrackingAnchorSettings
    {
        public static string selfAugmentationLabel = "Use this tracking anchor as content";
        public static string selfAugmentationTooltip =
            "Directly use the tracking anchor's child models as augmentation.";

        public static string cloneAsInitPoseGuideLabel =
            "Create copy of content as init pose guide";
        public static string cloneAsInitPoseGuideTooltip =
            "Automatically copy all child models into new init pose guide target GameObject and " +
            "reference it as augmented content.";

        public static string cloneAsAugmentationLabel = "Create copy of content as augmentation";
        public static string cloneAsAugmentationTooltip =
            "Automatically copy all child models into new augmentation target GameObject and " +
            "reference it as augmented content.";

        public static string cloneAsBothLabel =
            "Create copy of content as augmentation and init pose guide";
        public static string cloneAsBothTooltip =
            "Automatically copy all child models into new target GameObject and reference it as " +
            "augmented content. It will be used as both augmentation and init pose guide.";

        public static string copyModelHashLabel = "Copy Model License Features";
        public static string copyToClipboardTooltip = "Copy to clipboard.";

        public static string addInteractionLabel = "Add Init Pose Interaction";
        public static string addInteractionTooltip =
            "Add and set up manipulation of the init pose for this tracking anchor via touch/mouse inputs.";

        public static string removeInteractionLabel = "Remove Init Pose Interaction";
        public static string removeInteractionTooltip =
            "Remove direct input manipulation of the init pose from this tracking anchor.";

        public static string addTrackingMeshLabel = "Add TrackingMesh components";
        public static string addTrackingMeshTooltip =
            "Automatically add TrackingMesh components to all child models. Existing " +
            "TrackingMeshes remain unchanged.";

        public static string removeTrackingMeshLabel =
            "Remove TrackingMesh components";
        public static string removeTrackingMeshTooltip =
            "Automatically remove TrackingMesh components from all child models.";

        public static string copyLegacyVLInitPoseLabel = "Copy Init Pose";
    }
}
