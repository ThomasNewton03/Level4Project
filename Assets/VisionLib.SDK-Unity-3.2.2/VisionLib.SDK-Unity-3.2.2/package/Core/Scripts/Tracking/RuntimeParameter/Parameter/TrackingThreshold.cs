using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class TrackingThreshold : FloatRuntimeParameter
    {
        internal const string nativeName = "minTrackingQuality";

        private static readonly float defaultValue = 0.55f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 1.0f;

        public TrackingThreshold()
            : base(TrackingThreshold.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Tracking Threshold";
        }

        public override string GetNativeName()
        {
            return TrackingThreshold.nativeName;
        }

        public override string GetDescription()
        {
            return
                "This is a quality threshold for validating the tracking result after the initialization phase. This value decides the tracking status for a given frame. The correct value strongly depends on your scenario. If the line model matches the real object perfectly and there is no occlusion, a high \"TrackingThreshold\" is recommended. Because the line-model usually doesn't perfectly match the real object, a lower \"TrackingThreshold\" value performs better in most cases. Our experience shows that the \"DetectionThreshold\" should be set higher than the \"TrackingThreshold\" since the algorithm will struggle to recover from a bad initialization.";
        }

        public override float GetDefaultValue()
        {
            return TrackingThreshold.defaultValue;
        }

        public override float GetMinValue()
        {
            return TrackingThreshold.minValue;
        }

        public override float GetMaxValue()
        {
            return TrackingThreshold.maxValue;
        }
    }
}
