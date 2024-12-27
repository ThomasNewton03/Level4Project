using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class ContrastThreshold : FloatRuntimeParameter
    {
        internal const string nativeName = "lineGradientThreshold";

        private static readonly float defaultValue = 40.0f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 765.0f;

        public ContrastThreshold()
            : base(ContrastThreshold.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Contrast Threshold";
        }

        public override string GetNativeName()
        {
            return ContrastThreshold.nativeName;
        }

        public override string GetDescription()
        {
            return
                "Threshold for edge candidates in the image. High values will only consider pixels with high contrast as candidates. Low values will also consider other pixels. This is a trade-off. If there are too many candidates the algorithm might choose the wrong pixels. If there are not enough candidates the line-model might not stick to the object in the image.";
        }

        public override float GetDefaultValue()
        {
            return ContrastThreshold.defaultValue;
        }

        public override float GetMinValue()
        {
            return ContrastThreshold.minValue;
        }

        public override float GetMaxValue()
        {
            return ContrastThreshold.maxValue;
        }
    }
}
