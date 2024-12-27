using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class PoseFilteringSmoothness : FloatRuntimeParameter
    {
        internal const string nativeName = "poseFilteringSmoothness";

        private static readonly float defaultValue = 0.0f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 1000.0f;

        public PoseFilteringSmoothness()
            : base(PoseFilteringSmoothness.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Pose Filtering Smoothness";
        }

        public override string GetNativeName()
        {
            return PoseFilteringSmoothness.nativeName;
        }

        public override string GetDescription()
        {
            return
                "This value defines the smoothness of the pose filtering. Lower values will smooth out pose changes over time at the cost of augmentations lagging behind the real object. Higher values reduce this lag but augmentation movements will be more sudden.";
        }

        public override float GetDefaultValue()
        {
            return PoseFilteringSmoothness.defaultValue;
        }

        public override float GetMinValue()
        {
            return PoseFilteringSmoothness.minValue;
        }

        public override float GetMaxValue()
        {
            return PoseFilteringSmoothness.maxValue;
        }
    }
}
