using UnityEngine;
using System;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    public abstract class ImageStreamTexture
    {
        private Texture2D texture = null;
        private byte[] textureData;

        private static TextureFormat ImageFormatToTextureFormat(VLSDK.ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case VLSDK.ImageFormat.Grey:
                    return TextureFormat.Alpha8;
                case VLSDK.ImageFormat.RGB:
                    return TextureFormat.RGB24;
                case VLSDK.ImageFormat.RGBA:
                    return TextureFormat.RGBA32;
                case VLSDK.ImageFormat.Depth:
                    return TextureFormat.RFloat;
            }

            throw new ArgumentException("Unsupported image format");
        }

        protected void OnVLImage(Image image)
        {
            // Use camera image as texture
            int imageWidth = image.GetWidth();
            int imageHeight = image.GetHeight();
            if (imageWidth > 0 && imageHeight > 0)
            {
                int imageByteSize = imageWidth * imageHeight * image.GetBytesPerPixel();
                if (this.textureData == null || this.textureData.Length != imageByteSize)
                {
                    this.textureData = new byte[imageByteSize];
                }

                // Copy the image into a byte array
                if (image.CopyToBuffer(this.textureData))
                {
                    // Generate a texture from the byte array
                    VLSDK.ImageFormat imageFormat = image.GetFormat();
                    TextureFormat textureFormat = ImageFormatToTextureFormat(imageFormat);
                    if (!this.texture || this.texture.width != imageWidth ||
                        this.texture.height != imageHeight || this.texture.format != textureFormat)
                    {
                        this.texture = new Texture2D(imageWidth, imageHeight, textureFormat, false);
                    }

                    this.texture.LoadRawTextureData(this.textureData);
                    this.texture.Apply();
                }
            }
        }

        public ImageStreamTexture()
        {
            this.textureData = new byte[1];
        }

        public Texture2D GetTexture()
        {
            return this.texture;
        }

        abstract public void DeInit();
    }
}
