using System;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public abstract class FloatRuntimeParameter : DynamicTrackingParameter<float>
    {
        protected FloatRuntimeParameter(float defaultValue)
            : base(defaultValue) {}

        protected override string GetValueString()
        {
            return this.value.ToFourDecimalsWithPointInvariant();
        }

        public abstract float GetMinValue();

        public abstract float GetMaxValue();
    }
}
