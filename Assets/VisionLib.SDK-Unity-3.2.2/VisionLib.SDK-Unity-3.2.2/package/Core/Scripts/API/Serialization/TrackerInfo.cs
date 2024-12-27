using System;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  TrackerInfo stores the information returned after creating a tracker
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class TrackerInfo
    {
        /// <summary>
        /// Loaded tracker type
        /// </summary>
        public string trackerType;

        /// <summary>
        /// Loaded device type
        /// </summary>
        public string deviceType;

        /// <summary>
        /// Warnings arising from the current tracking configuration
        /// </summary>
        public Issue[] warnings;
    }
}
