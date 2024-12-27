using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class DetectionRadius : FloatRuntimeParameter
    {
        internal const string nativeName = "lineSearchLengthInitRelative";

        private static readonly float defaultValue = 0.05f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 1.0f;

        public DetectionRadius()
            : base(DetectionRadius.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Detection Radius";
        }

        public override string GetNativeName()
        {
            return DetectionRadius.nativeName;
        }

        public override string GetDescription()
        {
            return
                "Length of the search lines when looking for the object in the camera images during initialization. Longer search lines allow tracking to snap on to the object more easily, i.e. with less alignemnt between the line model and the real object in the image. This comes at increased risk of misaligned initialization.";
        }

        public override float GetDefaultValue()
        {
            return DetectionRadius.defaultValue;
        }

        public override float GetMinValue()
        {
            return DetectionRadius.minValue;
        }

        public override float GetMaxValue()
        {
            return DetectionRadius.maxValue;
        }
    }
}
