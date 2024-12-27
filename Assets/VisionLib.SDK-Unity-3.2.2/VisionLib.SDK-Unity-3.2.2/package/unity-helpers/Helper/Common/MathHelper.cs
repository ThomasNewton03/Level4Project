namespace Visometry.Helpers
{
    public static class MathHelper
    {
        public static float Remap(
            float srcValue,
            float srcRangeMin,
            float srcRangeMax,
            float dstRangeMin,
            float dstRangeMax)
        {
            return dstRangeMin + (srcValue - srcRangeMin) / (srcRangeMax - srcRangeMin) *
                (dstRangeMax - dstRangeMin);
        }
    }
}
