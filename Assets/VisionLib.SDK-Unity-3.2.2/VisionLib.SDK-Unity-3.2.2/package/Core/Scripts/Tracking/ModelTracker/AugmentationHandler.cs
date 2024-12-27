using System;
using System.Collections.Generic;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class AugmentationHandler
    {
        public readonly HashSet<RenderedObject> renderedObjects;
        private AugmentationMode augmentationMode;
        [SerializeField]
        private bool showInitPoseGuideWhileDisabled = false;

        public enum AugmentationMode
        {
            Initializing,
            Tracking,
            Inactive
        }

        public AugmentationHandler()
        {
            this.renderedObjects = new HashSet<RenderedObject>();
            this.augmentationMode = AugmentationMode.Inactive;
        }

        public bool ShowInitPoseGuideWhileDisabled
        {
            get
            {
                return this.showInitPoseGuideWhileDisabled;
            }
            set
            {
                this.showInitPoseGuideWhileDisabled = value;
                PropagateAugmentationMode();
            }
        }

        public void OnTrackingStopped()
        {
            SwitchToAugmentationMode(AugmentationMode.Inactive);
        }

        private AugmentationMode GetAugmentationMode()
        {
            if (this.augmentationMode == AugmentationMode.Inactive &&
                this.ShowInitPoseGuideWhileDisabled)
            {
                return AugmentationMode.Initializing;
            }
            return this.augmentationMode;
        }

        public void Register(RenderedObject renderedObject)
        {
            this.renderedObjects.Add(renderedObject);
            renderedObject.SetRenderingState(GetAugmentationMode());
        }

        public void Deregister(RenderedObject renderedObject)
        {
            this.renderedObjects.Remove(renderedObject);
        }

        public void SwitchToAugmentationMode(AugmentationMode newAugmentationMode)
        {
            this.augmentationMode = newAugmentationMode;
            PropagateAugmentationMode();
        }

        private void PropagateAugmentationMode()
        {
            foreach (var renderedObject in this.renderedObjects)
            {
                renderedObject.SetRenderingState(GetAugmentationMode());
            }
        }

        public void OnModelTransform(ModelTransform unityWorldFromModel)
        {
            foreach (var renderedObject in this.renderedObjects)
            {
                renderedObject.SetTargetTransform(unityWorldFromModel);
            }
        }

        public void SetInitPose(ModelTransform initPoseInWorldCS)
        {
            if (this.augmentationMode == AugmentationMode.Tracking)
            {
                return;
            }
            // PositionUpdateDamper implicitly converts from VL to Unity CS
            var initMatVL = CameraHelper.flipX * initPoseInWorldCS.ToMatrix() * CameraHelper.flipX;
            OnModelTransform(new ModelTransform(initMatVL));
        }
    }
}
