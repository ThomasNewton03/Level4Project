namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class BoolExtensions
    {
        public static string ToInvariantLowerCaseString(this bool value)
        {
            return value.ToString().ToLowerInvariant();
        }
    }
}
