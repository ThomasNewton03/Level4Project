using UnityEngine;
using System;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public class ModelPropertyCache
    {
        public bool IsEnabled { get; private set; } = false;
        public bool IsOccluder { get; private set; } = false;
        public bool UseLines { get; private set; } = false;
        public ModelTransform RelativeTransform { get; private set; }
        
        public bool UpdateCache(ModelTransform relTrans, bool enabled, bool occluder, bool useLines)
        {
            var equalRelativeTransform = this.RelativeTransform.r == relTrans.r &&
                                         this.RelativeTransform.s == relTrans.s &&
                                         this.RelativeTransform.t == relTrans.t;
            if (equalRelativeTransform && this.IsEnabled == enabled &&
                this.IsOccluder == occluder && this.UseLines == useLines)
            {
                return false;
            }

            this.RelativeTransform = relTrans;
            this.IsEnabled = enabled;
            this.IsOccluder = occluder;
            this.UseLines = useLines;
            return true;
        }
    }
}
