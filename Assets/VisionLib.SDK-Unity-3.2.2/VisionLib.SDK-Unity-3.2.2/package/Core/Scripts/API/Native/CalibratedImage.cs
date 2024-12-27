using System;
using System.Runtime.InteropServices;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  The CalibratedImage is a wrapper for a CalibratedImage object. It is a container
    ///  for an Image, IntrinsicData and ExtrinsicData such that the
    ///  intrinsic data belongs to the image and the extrinsic data describes the transformation
    ///  from the device to the image.
    /// </summary>
    /// @ingroup Native
    public class CalibratedImage : IDisposable
    {
        private IntPtr handle;
        private bool disposed = false;
        private bool owner;

        /// <summary>
        ///  Internal constructor of CalibratedImage.
        /// </summary>
        /// <remarks>
        ///  This constructor is used internally by the VisionLib.
        /// </remarks>
        /// <param name="handle">
        ///  Handle to the native object.
        /// </param>
        /// <param name="owner">
        ///  <c>true</c>, if the CalibratedImage is the owner of the native object;
        ///  <c>false</c>, otherwise.
        /// </param>
        public CalibratedImage(IntPtr handle, bool owner)
        {
            this.handle = handle;
            this.owner = owner;
        }

        ~CalibratedImage()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlCalibratedImageWrapper_Clone(IntPtr calibratedImageWrapper);

        /// <summary>
        ///  Creates a copy of this object and returns a Wrapper of it.
        /// </summary>
        /// <returns>
        ///  A wrapper of a copy of this object.
        /// </returns>
        public CalibratedImage Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLCalibratedImageWrapper");
            }
            return new CalibratedImage(vlCalibratedImageWrapper_Clone(this.handle), true);
        }

        public IntPtr getHandle()
        {
            return this.handle;
        }

        [DllImport(VLSDK.dllName)]
        private static extern void vlDelete_CalibratedImageWrapper(IntPtr calibratedImageWrapper);

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (this.owner)
            {
                vlDelete_CalibratedImageWrapper(this.handle);
            }
            this.handle = IntPtr.Zero;

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="CalibratedImage"/>. The <see cref="Dispose"/> method leaves
        ///  the <see cref="CalibratedImage"/> in an unusable state. After calling
        ///  <see cref="Dispose"/>, you must release all references to the
        ///  <see cref="CalibratedImage"/> so the garbage collector can reclaim the
        ///  memory that the <see cref="CalibratedImage"/> was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlCalibratedImageWrapper_GetImage(
            IntPtr calibratedImageWrapper);

        /// <summary>
        ///  Returns the image of the calibrated image.
        /// </summary>
        /// <returns>
        ///  <see cref="Image"/> of the image.
        /// </returns>
        public Image GetImage()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLCalibratedImageWrapper");
            }

            return new Image(vlCalibratedImageWrapper_GetImage(this.handle), false);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlCalibratedImageWrapper_GetIntrinsicData(
            IntPtr calibratedImageWrapper);

        /// <summary>
        ///  Returns the intrinsic data regarding the image.
        /// </summary>
        /// <returns>
        ///  <see cref="IntrinsicData"/> of the image.
        /// </returns>
        public IntrinsicData GetIntrinsicData()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLCalibratedImageWrapper");
            }

            return new IntrinsicData(vlCalibratedImageWrapper_GetIntrinsicData(this.handle), true);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlCalibratedImageWrapper_GetImageFromDeviceTransform(
            IntPtr calibratedImageWrapper);

        /// <summary>
        ///  Returns the extrinsic data for the transformation from device space to
        ///  image space.
        /// </summary>
        /// <returns>
        ///  <see cref="ExtrinsicData"\> for transformation from device to image space.
        /// </returns>
        public ExtrinsicData GetImageFromDeviceTransform()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLCalibratedImageWrapper");
            }

            return new ExtrinsicData(
                vlCalibratedImageWrapper_GetImageFromDeviceTransform(this.handle),
                true);
        }
    }
}
