using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public abstract class StringRuntimeParameter : DynamicTrackingParameter<string>
    {
        protected StringRuntimeParameter(string defaultValue)
            : base(defaultValue) {}

        protected override string GetValueString()
        {
            return this.value;
        }

        protected override string GetIndependentValueCopy()
        {
            return string.Copy(this.value);
        }
    }
}
