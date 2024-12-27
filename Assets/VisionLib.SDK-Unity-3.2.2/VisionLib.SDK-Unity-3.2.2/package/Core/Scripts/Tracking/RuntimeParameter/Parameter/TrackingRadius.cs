using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class TrackingRadius : FloatRuntimeParameter
    {
        internal const string nativeName = "lineSearchLengthTrackingRelative";

        private static readonly float defaultValue = 0.03125f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 1.0f;

        public TrackingRadius()
            : base(TrackingRadius.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Tracking Radius";
        }

        public override string GetNativeName()
        {
            return TrackingRadius.nativeName;
        }

        public override string GetDescription()
        {
            return "Same as \"DetectionRadius\", but used during the tracking phase.";
        }

        public override float GetDefaultValue()
        {
            return TrackingRadius.defaultValue;
        }

        public override float GetMinValue()
        {
            return TrackingRadius.minValue;
        }

        public override float GetMaxValue()
        {
            return TrackingRadius.maxValue;
        }
    }
}
