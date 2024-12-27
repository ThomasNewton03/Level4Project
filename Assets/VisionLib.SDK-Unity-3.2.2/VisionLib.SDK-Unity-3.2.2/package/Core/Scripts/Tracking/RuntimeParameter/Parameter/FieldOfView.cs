using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class FieldOfView : StringRuntimeParameter
    {
        internal const string nativeName = "fieldOfView";
        
        private static readonly string defaultValue = "wide";
        
        public FieldOfView()
            : base(FieldOfView.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Field of View";
        }
        
        public override string GetNativeName()
        {
            return FieldOfView.nativeName;
        }

        public override string GetDescription()
        {
            return
                "Adjusts camera field of view: Use \"wide\" for larger objects and \"narrow\" for small objects. Only works on HoloLens.";
        }

        public override string GetDefaultValue()
        {
            return FieldOfView.defaultValue;
        }
    }
}
