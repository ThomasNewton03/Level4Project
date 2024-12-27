using System.Globalization;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class FloatExtensions
    {
        public static string ToFourDecimalsWithPointInvariant(this float value)
        {
            return value.ToString("0.0000", CultureInfo.InvariantCulture);
        }
    }
}
