using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class DetectionThreshold : FloatRuntimeParameter
    {
        internal const string nativeName = "minInitQuality";

        private static readonly float defaultValue = 0.65f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 1.0f;

        public DetectionThreshold()
            : base(
                DetectionThreshold.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Detection Threshold";
        }

        public override string GetNativeName()
        {
            return DetectionThreshold.nativeName;
        }

        public override string GetDescription()
        {
            return
                "This is a quality threshold for validating the tracking result during the initialization phase. Tracking only snaps on to an object if the \"DetectionThreshold\" is met or exceeded. The correct value strongly depends on your scenario. If the line model matches the real object perfectly and there is no occlusion, a high \"DetectionThreshold\" is recommended. Because the line-model usually doesn't perfectly match the real object, a lower \"DetectionThreshold\" value performs better in most cases. If the value is too low, however, the algorithm might start tracking random objects. Our experience shows that the \"DetectionThreshold\" should be set higher than the \"TrackingThreshold\" since the algorithm will struggle to recover from a bad initialization.";
        }

        public override float GetDefaultValue()
        {
            return DetectionThreshold.defaultValue;
        }
        
        public override float GetMinValue()
        {
            return DetectionThreshold.minValue;
        }

        public override float GetMaxValue()
        {
            return DetectionThreshold.maxValue;
        }
    }
}
