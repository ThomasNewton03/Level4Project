using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  Image is a wrapper for an Image object.
    /// </summary>
    /// @ingroup Native
    public class Image : IDisposable
    {
        private IntPtr handle;
        private bool disposed = false;
        private bool owner;

        /// <summary>
        ///  Internal constructor of the Image.
        /// </summary>
        /// <remarks>
        ///  This constructor is used internally by the VisionLib.
        /// </remarks>
        /// <param name="handle">
        ///  Handle to the native object.
        /// </param>
        /// <param name="owner">
        ///  <c>true</c>, if the Image is the owner of the native object;
        ///  <c>false</c>, otherwise.
        /// </param>
        public Image(IntPtr handle, bool owner)
        {
            this.handle = handle;
            this.owner = owner;
        }

        ~Image()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlImageWrapper_Clone(IntPtr imageWrapper);

        /// <summary>
        ///  Creates a copy of this object and returns a Wrapper of it.
        /// </summary>
        /// <returns>
        ///  A wrapper of a copy of this object.
        /// </returns>
        public Image Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }
            return new Image(vlImageWrapper_Clone(this.handle), true);
        }

        public IntPtr getHandle()
        {
            return this.handle;
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlNew_ImageWrapper(int format);

        [DllImport(VLSDK.dllName)]
        private static extern void vlDelete_ImageWrapper(IntPtr imageWrapper);

        private void Dispose(bool disposing)
        {
            // Prevent multiple calls to Dispose
            if (this.disposed)
            {
                return;
            }

            // Was dispose called explicitly by the user?
            if (disposing)
            {
                // Dispose managed resources (those that implement IDisposable)
            }

            // Clean up unmanaged resources
            if (this.owner)
            {
                vlDelete_ImageWrapper(this.handle);
            }
            this.handle = IntPtr.Zero;

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="Image"/>. The <see cref="Dispose"/> method leaves
        ///  the <see cref="Image"/> in an unusable state. After calling
        ///  <see cref="Dispose"/>, you must release all references to the
        ///  <see cref="Image"/> so the garbage collector can reclaim the
        ///  memory that the <see cref="Image"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.UInt32 vlImageWrapper_GetFormat(IntPtr imageWrapper);

        /// <summary>
        ///  Returns an enumeration with the internal type of the image.
        /// </summary>
        /// <returns>
        ///  <see cref="VLSDK.ImageFormat"/> enumeration with the internal type
        ///  of the image.
        /// </returns>
        public VLSDK.ImageFormat GetFormat()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }

            return (VLSDK.ImageFormat) vlImageWrapper_GetFormat(this.handle);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.UInt32 vlImageWrapper_GetBytesPerPixel(IntPtr imageWrapper);

        /// <summary>
        ///  Returns the number of bytes per pixel.
        /// </summary>
        /// <returns>
        ///  The number of bytes per pixel.
        /// </returns>
        public int GetBytesPerPixel()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }

            return Convert.ToInt32(vlImageWrapper_GetBytesPerPixel(this.handle));
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.UInt32 vlImageWrapper_GetWidth(IntPtr imageWrapper);

        /// <summary>
        ///  Returns the width of the image.
        /// </summary>
        /// <returns>
        ///  The width in pixels.
        /// </returns>
        public int GetWidth()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }

            return Convert.ToInt32(vlImageWrapper_GetWidth(this.handle));
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.UInt32 vlImageWrapper_GetHeight(IntPtr imageWrapper);

        /// <summary>
        ///  Returns the height of the image.
        /// </summary>
        /// <returns>
        ///  The height in pixels.
        /// </returns>
        public int GetHeight()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }

            return Convert.ToInt32(vlImageWrapper_GetHeight(this.handle));
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlImageWrapper_CopyToBuffer(
            IntPtr imageWrapper,
            IntPtr buffer,
            System.UInt32 bufferSize);

        /// <summary>
        ///  Copies the VisionLib image into the given byte array.
        /// </summary>
        /// <remarks>
        ///  Make sure, that the byte array is large enough for storing the
        ///  whole image date (width * height * bytesPerPixel). The number of bytes
        ///  per pixel an be acquired using the <see cref="GetBytesPerPixel"/>
        ///  function.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the data was copied to the byte array successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <param name="buffer">
        ///  Byte array for storing the raw image data.
        /// </param>
        public bool CopyToBuffer(byte[] buffer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }

            bool result = false;
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                result = vlImageWrapper_CopyToBuffer(
                    this.handle,
                    bufferHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(buffer.Length));
            }
            finally
            {
                bufferHandle.Free();
            }

            return result;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlImageWrapper_CopyFromBuffer(
            IntPtr imageWrapper,
            IntPtr buffer,
            System.UInt32 width,
            System.UInt32 height);

        public bool CopyFromBuffer(IntPtr buffer, int width, int height)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLImageWrapper");
            }

            if (this.handle == IntPtr.Zero)
            {
                return false;
            }

            return vlImageWrapper_CopyFromBuffer(
                this.handle,
                buffer,
                Convert.ToUInt32(width),
                Convert.ToUInt32(height));
        }

        /// <summary>
        ///  Copies the given byte array into the VisionLib image.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   The VisionLib image will be resized according to the width and height
        ///   parameter.
        ///  </para>
        ///  <para>
        ///   Make sure, that the data stored in the byte array has the same
        ///   format as the image. The image format can be acquired using the
        ///   <see cref="GetFormat"/> function.
        ///  </para>
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the data was copied into the image successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <param name="buffer">Byte array with the image data.</param>
        /// <param name="width">New width of the image.</param>
        /// <param name="height">New height of the image.</param>
        public bool CopyFromBuffer(byte[] buffer, int width, int height)
        {
            if (buffer.Length != width * height * GetBytesPerPixel())
            {
                throw new ArgumentException(
                    "Buffer size does not match image size. Width: " + width + ", Height: " +
                    height + ", BytesPerPixel: " + GetBytesPerPixel() + ", BufferSize: " +
                    buffer.Length);
            }

            bool result = false;
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                result = CopyFromBuffer(bufferHandle.AddrOfPinnedObject(), width, height);
            }
            finally
            {
                bufferHandle.Free();
            }

            return result;
        }

        public static Image CreateFromBuffer(IntPtr buffer, int width, int height)
        {
            var result = new Image(vlNew_ImageWrapper((int) VLSDK.ImageFormat.RGBA), true);
            if (!result.CopyFromBuffer(buffer, width, height))
            {
                LogHelper.LogError("No valid license with the image injection feature found.");
            }
            return result;
        }

        public static Image CreateFromBuffer(byte[] buffer, int width, int height)
        {
            var result = new Image(vlNew_ImageWrapper((int) VLSDK.ImageFormat.RGBA), true);
            if (!result.CopyFromBuffer(buffer, width, height))
            {
                LogHelper.LogError("No valid license with the image injection feature found.");
            }
            return result;
        }

        private static void TransformUnityImageToVLImage(
            Color32[] unityImage,
            ref byte[] vlData,
            int width,
            int height)
        {
            if (vlData == null || vlData.Length != unityImage.Length * 4)
            {
                vlData = new byte[unityImage.Length * 4];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int unityIndex = y * width + x;
                    int vlSDKIndex = 4 * ((height - 1 - y) * width + x);

                    vlData[vlSDKIndex] = unityImage[unityIndex].r;
                    vlData[vlSDKIndex + 1] = unityImage[unityIndex].g;
                    vlData[vlSDKIndex + 2] = unityImage[unityIndex].b;
                    vlData[vlSDKIndex + 3] = unityImage[unityIndex].a;
                }
            }
        }

        public static Image CreateFromTexture(Texture2D texture, ref byte[] byteBuffer)
        {
            TransformUnityImageToVLImage(
                texture.GetPixels32(),
                ref byteBuffer,
                texture.width,
                texture.height);
            return CreateFromBuffer(byteBuffer, texture.width, texture.height);
        }

        public static Image CreateFromTexture(Texture2D texture)
        {
            byte[] tempBuffer = null;
            return CreateFromTexture(texture, ref tempBuffer);
        }

        public static Image CreateFromTexture(WebCamTexture texture, ref byte[] byteBuffer)
        {
            TransformUnityImageToVLImage(
                texture.GetPixels32(),
                ref byteBuffer,
                texture.width,
                texture.height);
            return CreateFromBuffer(byteBuffer, texture.width, texture.height);
        }

        public static Image CreateFromTexture(WebCamTexture texture)
        {
            byte[] tempBuffer = null;
            return CreateFromTexture(texture, ref tempBuffer);
        }
    }
}
