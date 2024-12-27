using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class ImageSourceEnabled : BoolRuntimeParameter
    {
        internal const string nativeName = "imageSource.enabled";
        
        private static readonly bool defaultValue = true;
        
        public ImageSourceEnabled()
            : base(ImageSourceEnabled.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Image Source enabled";
        }

        public override string GetNativeName()
        {
            return ImageSourceEnabled.nativeName;
        }

        public override string GetDescription()
        {
            return
                "Pause (false) or resume (true) image stream in the backend. Pausing causes repeated tracking on the same frame indefinitely.";
        }

        public override bool GetDefaultValue()
        {
            return ImageSourceEnabled.defaultValue;
        }
    }
}
