using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class CreaseEdgeThreshold : LogarithmicFloatRuntimeParameter
    {
        internal const string nativeName = "normalThreshold";

        private static readonly float defaultValue = 1000.0f;
        private static readonly float minValue = 0.001f;
        private static readonly float maxValue = 1000.0f;

        public CreaseEdgeThreshold()
            : base(CreaseEdgeThreshold.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Crease Edge Threshold";
        }

        public override string GetNativeName()
        {
            return CreaseEdgeThreshold.nativeName;
        }

        public override string GetDescription()
        {
            return
                "A line model edge detection parameter. The \"CreaseEdgeThreshold\" specifies the minimum surface normal difference between two neighboring pixels for recognition as an edge. In most cases, a high \"CreaseEdgeThreshold\" is recommended, since normal-based edge detection doesn't work reliably for many models. Despite this, tracking of some models benefits from a low threshold value.";
        }

        public override float GetDefaultValue()
        {
            return CreaseEdgeThreshold.defaultValue;
        }

        public override float GetMinValue()
        {
            return CreaseEdgeThreshold.minValue;
        }

        public override float GetMaxValue()
        {
            return CreaseEdgeThreshold.maxValue;
        }
    }
}
