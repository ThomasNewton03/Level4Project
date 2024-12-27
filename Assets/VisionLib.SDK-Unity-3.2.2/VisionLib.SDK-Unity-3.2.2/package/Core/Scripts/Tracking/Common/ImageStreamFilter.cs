using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "image_stream_filter.html")]
    [AddComponentMenu("VisionLib/Core/Image Stream Filter")]
    public class ImageStreamFilter : MonoBehaviour
    {
        /// <summary>
        /// Image stream of the image to display.
        /// </summary>
        [Tooltip("Image stream of the image to display")]
        public TrackingManager.ImageStream imageStream = TrackingManager.ImageStream.CameraImage;

        public void UseDebugImageStream()
        {
            this.imageStream = TrackingManager.ImageStream.DebugImage;
        }

        public void UseCameraImageStream()
        {
            this.imageStream = TrackingManager.ImageStream.CameraImage;
        }

        public void UseDepthImageStream()
        {
            this.imageStream = TrackingManager.ImageStream.DepthImage;
        }

        public void UseNoImageStream()
        {
            this.imageStream = TrackingManager.ImageStream.None;
        }

        public Texture2D GetTexture()
        {
            return TrackingManager.DoesTrackerExistAndIsInitialized()
                ? TrackingManager.Instance.GetStreamTexture(this.imageStream)
                : Texture2D.blackTexture;
        }
    }
}
