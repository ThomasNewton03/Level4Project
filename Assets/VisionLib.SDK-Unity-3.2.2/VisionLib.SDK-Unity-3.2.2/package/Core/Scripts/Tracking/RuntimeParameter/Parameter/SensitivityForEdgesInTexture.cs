using System;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class SensitivityForEdgesInTexture : FloatRuntimeParameter
    {
        internal const string nativeName = "textureColorSensitivity";

        private static readonly float defaultValue = 0.0f;
        private static readonly float minValue = 0.0f;
        private static readonly float maxValue = 1.0f;

        public SensitivityForEdgesInTexture()
            : base(
                SensitivityForEdgesInTexture.defaultValue) {}

        public override string GetDescriptiveName()
        {
            return "Texture Edge Sensitivity";
        }

        public override string GetNativeName()
        {
            return SensitivityForEdgesInTexture.nativeName;
        }

        public override string GetDescription()
        {
            return
                "Sensitivity for generating the line-model based on the models color texture. This only applies if your model provides texture data. Using a high value will extract many edges from the texture; using low values will extract very few edges from the texture. The value 0.0 means that no edges will be extracted from the texture at all.";
        }

        public override float GetDefaultValue()
        {
            return SensitivityForEdgesInTexture.defaultValue;
        }

        public override float GetMinValue()
        {
            return SensitivityForEdgesInTexture.minValue;
        }

        public override float GetMaxValue()
        {
            return SensitivityForEdgesInTexture.maxValue;
        }
    }
}
