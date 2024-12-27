using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class KeyFrameDistance : LogarithmicFloatRuntimeParameter
    {
        internal const string nativeName = "keyFrameDistance";

        private static readonly float defaultValue = 100.0f;
        private static readonly float minValue = 0.001f;
        private static readonly float maxValue = 100000.0f;

        public KeyFrameDistance()
            : base(KeyFrameDistance.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Keyframe Distance";
        }

        public override string GetNativeName()
        {
            return KeyFrameDistance.nativeName;
        }

        public override string GetDescription()
        {
            return
                "The minimum distance between keyframes in mm. Line models are not created every frame. Instead, a new line model is generated on each keyframe. Higher keyframe distances therefore reduce computational load at the cost of reduced tracking precision. Vice versa, small keyframe distances improve tracking accuracy but cause more computational load and thus reduce the overall frame rate.";
        }

        public override float GetDefaultValue()
        {
            return KeyFrameDistance.defaultValue;
        }

        public override float GetMinValue()
        {
            return KeyFrameDistance.minValue;
        }

        public override float GetMaxValue()
        {
            return KeyFrameDistance.maxValue;
        }
    }
}
