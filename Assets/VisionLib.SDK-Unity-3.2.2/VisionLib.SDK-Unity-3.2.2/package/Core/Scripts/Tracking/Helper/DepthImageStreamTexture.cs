using System;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// The VLDepthImageStream propagates the depth image to its texture
    /// </summary>
    /// @ingroup Core
    public class DepthImageStreamTexture : ImageStreamTexture
    {
        public DepthImageStreamTexture()
            : base()
        {
            TrackingManager.OnCalibratedDepthImage += this.OnCalibratedDepthImage;
        }

        override public void DeInit()
        {
            TrackingManager.OnCalibratedDepthImage -= this.OnCalibratedDepthImage;
        }

        private void OnCalibratedDepthImage(CalibratedImage calibratedDepthImage)
        {
            Image depthImage = calibratedDepthImage.GetImage();
            this.OnVLImage(depthImage);
            depthImage.Dispose();
        }
    }
}
