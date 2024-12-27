using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class DisablePoseEstimation : BoolRuntimeParameter
    {
        internal const string nativeName = "disablePoseEstimation";
        
        private static readonly bool defaultValue = false;

        public DisablePoseEstimation()
            : base(DisablePoseEstimation.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Disable Pose Estimation";
        }

        public override string GetNativeName()
        {
            return DisablePoseEstimation.nativeName;
        }

        public override string GetDescription()
        {
            return
                "Deactivates frame to frame tracking. Instead the tracking result will only be calculated at the given init pose. The tracking result then states, whether the object could be detected at the init pose.";
        }

        public override bool GetDefaultValue()
        {
            return DisablePoseEstimation.defaultValue;
        }
    }
}
