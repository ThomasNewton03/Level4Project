using System;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// The CameraImageStream propagates the camera image to its texture
    /// </summary>
    /// @ingroup Core
    public class CameraImageStreamTexture : ImageStreamTexture
    {
        public CameraImageStreamTexture()
            : base()
        {
            TrackingManager.OnImage += this.OnVLImage;
        }

        override public void DeInit()
        {
            TrackingManager.OnImage -= this.OnVLImage;
        }
    }
}
