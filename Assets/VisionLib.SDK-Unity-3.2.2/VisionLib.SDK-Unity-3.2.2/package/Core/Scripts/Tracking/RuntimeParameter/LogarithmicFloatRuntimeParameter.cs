using System;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public abstract class LogarithmicFloatRuntimeParameter : FloatRuntimeParameter
    {
        protected LogarithmicFloatRuntimeParameter(float defaultValue)
            : base(defaultValue) {}
    }
}
