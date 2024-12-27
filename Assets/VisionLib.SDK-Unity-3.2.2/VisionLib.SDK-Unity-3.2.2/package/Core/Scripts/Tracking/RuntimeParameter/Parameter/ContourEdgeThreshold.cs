using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class ContourEdgeThreshold : LogarithmicFloatRuntimeParameter
    {
        internal const string nativeName = "laplaceThreshold";

        private static readonly float defaultValue = 5.0f;
        private static readonly float minValue = 0.001f;
        private static readonly float maxValue = 100000.0f;

        public ContourEdgeThreshold()
            : base(ContourEdgeThreshold.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Contour Edge Threshold";
        }

        public override string GetNativeName()
        {
            return ContourEdgeThreshold.nativeName;
        }

        public override string GetDescription()
        {
            return
                "A line model edge detection parameter. The \"ContourEdgeThreshold\" is the minimum depth difference (in mm) between two neighboring pixels for the step to be recognized as an edge and added to the line model.";
        }

        public override float GetDefaultValue()
        {
            return ContourEdgeThreshold.defaultValue;
        }

        public override float GetMinValue()
        {
            return ContourEdgeThreshold.minValue;
        }

        public override float GetMaxValue()
        {
            return ContourEdgeThreshold.maxValue;
        }
    }
}
