using UnityEngine;
using UnityEngine.UI;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  The DebugImage can be used to visualize debug images using the
    ///  Unity GUI system.
    /// </summary>
    /// <remarks>
    ///  Use the VLDebugImage prefab. It will ensure the correct object
    ///  hierarchy.
    /// </remarks>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "debug_image.html")]
    [AddComponentMenu("VisionLib/Core/Debug Image")]
    [RequireComponent(typeof(ImageStreamFilter))]
    public class DebugImage : MonoBehaviour
    {
        /// <summary>
        ///  Target object with the RawImage component for displaying the debug
        ///  image.
        /// </summary>
        [Tooltip("Target object with the RawImage component for displaying the debug image")]
        public GameObject imageObject;

        public bool DebugImageEnabled
        {
            get
            {
                return this.rawImage.enabled;
            }
            set
            {
                this.rawImage.enabled = value;
            }
        }

        private RectTransform imageRectTransform;
        private AspectRatioFitter imageAspectRatioFitter;
        private RawImage rawImage;

        private ImageStreamFilter imageStreamFilter = null;

        private RenderRotation renderRotation = RenderRotation.CCW0;

        // Rotations for different screen orientations
        private static readonly Quaternion rot0 = Quaternion.AngleAxis(0.0f, Vector3.forward);
        private static readonly Quaternion rot90 = Quaternion.AngleAxis(90.0f, Vector3.forward);
        private static readonly Quaternion rot180 = Quaternion.AngleAxis(180.0f, Vector3.forward);
        private static readonly Quaternion rot270 = Quaternion.AngleAxis(270.0f, Vector3.forward);

        /// <summary>
        /// Turns the debug image on or off
        /// Triggered with voice command
        /// </summary>
        public void ToggleDebugImageVisibility()
        {
            this.DebugImageEnabled = !this.DebugImageEnabled;
        }

        [System.Obsolete(
            "ShowDebugImage is obsolete, use the `DebugImageEnabled` property instead.")]
        /// \deprecated ShowDebugImage is obsolete, use the `DebugImageEnabled` property instead.
        public void ShowDebugImage(bool show)
        {
            this.DebugImageEnabled = show;
        }

        private void Update()
        {
            Texture2D imageStreamTexture = this.imageStreamFilter.GetTexture();
            if (imageStreamTexture != null)
            {
                this.rawImage.texture = imageStreamTexture;
            }
            else
            {
                this.rawImage.texture = Texture2D.blackTexture;
            }

            this.UpdateImageSize();
        }

        private void UpdateImageSize()
        {
            if (!this.imageRectTransform)
            {
                return;
            }

            if (this.renderRotation == RenderRotation.CCW0)
            {
                this.imageRectTransform.localRotation = rot0;
            }
            else if (this.renderRotation == RenderRotation.CCW180)
            {
                this.imageRectTransform.localRotation = rot180;
            }
            else if (this.renderRotation == RenderRotation.CCW90)
            {
                this.imageRectTransform.localRotation = rot90;
            }
            else if (this.renderRotation == RenderRotation.CCW270)
            {
                this.imageRectTransform.localRotation = rot270;
            }

            if (!this.rawImage || !this.imageAspectRatioFitter)
            {
                return;
            }

            this.imageAspectRatioFitter.aspectRatio =
                (float) this.rawImage.texture.width / this.rawImage.texture.height;
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            this.renderRotation = orientation.GetRenderRotation();
            this.UpdateImageSize();
        }

        private void Awake()
        {
            if (this.imageObject != null)
            {
                this.imageRectTransform = this.imageObject.GetComponentInChildren<RectTransform>();
                this.imageAspectRatioFitter =
                    this.imageObject.GetComponentInChildren<AspectRatioFitter>();
                this.rawImage = this.imageObject.GetComponentInChildren<RawImage>();
                this.rawImage.uvRect = new Rect(0.0f, 0.0f, 1.0f, -1.0f);
                this.rawImage.texture = Texture2D.blackTexture;
            }
            else
            {
                LogHelper.LogError("'Image Object' must be set", this);
            }

            this.imageStreamFilter = GetComponent<ImageStreamFilter>();
        }

        private void OnEnable()
        {
            OnOrientationChange(ScreenOrientationObserver.GetScreenOrientation());
            ScreenOrientationObserver.OnOrientationChange += OnOrientationChange;
        }

        private void OnDisable()
        {
            ScreenOrientationObserver.OnOrientationChange -= OnOrientationChange;
        }
    }
}
