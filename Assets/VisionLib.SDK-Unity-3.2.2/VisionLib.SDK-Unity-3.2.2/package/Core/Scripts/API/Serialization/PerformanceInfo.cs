using System;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  PerformanceInfo stores information about the tracking performance.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public struct PerformanceInfo
    {
        /// <summary>
        /// The tracking processing time in milliseconds. This excludes the sleep
        /// duration for achieving the target FPS.
        /// </summary>
        public int processingTime;
    }
}
