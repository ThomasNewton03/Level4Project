using System;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public abstract class BoolRuntimeParameter : DynamicTrackingParameter<bool>
    {
        protected BoolRuntimeParameter(bool defaultValue)
            : base(defaultValue) {}

        protected override string GetValueString()
        {
            return this.value.ToInvariantLowerCaseString();
        }
    }
}
