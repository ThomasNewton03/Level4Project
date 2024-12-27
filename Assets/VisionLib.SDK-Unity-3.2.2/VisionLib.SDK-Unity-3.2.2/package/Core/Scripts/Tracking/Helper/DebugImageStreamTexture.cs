using System;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// The DebugImageStream propagates the debug image to its texture
    /// </summary>
    /// @ingroup Core
    public class DebugImageStreamTexture : ImageStreamTexture
    {
        public DebugImageStreamTexture()
            : base()
        {
            TrackingManager.OnDebugImage += this.OnVLImage;
        }

        override public void DeInit()
        {
            TrackingManager.OnDebugImage -= this.OnVLImage;
        }
    }
}
