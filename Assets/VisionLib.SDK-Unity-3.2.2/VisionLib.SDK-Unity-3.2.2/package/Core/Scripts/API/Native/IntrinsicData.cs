using System;
using System.Runtime.InteropServices;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  The IntrinsicData is a wrapper for an IntrinsicData object.
    ///  IntrinsicData objects represent the intrinsic camera parameters
    ///  (focal length, principal point, skew and distortion parameters).
    /// </summary>
    /// @ingroup Native
    public class IntrinsicData : IDisposable
    {
        private IntPtr handle;
        private bool disposed = false;
        private bool owner;

        /// <summary>
        ///  Internal constructor of IntrinsicData.
        /// </summary>
        /// <remarks>
        ///  This constructor is used internally by the VisionLib.
        /// </remarks>
        /// <param name="handle">
        ///  Handle to the native object.
        /// </param>
        /// <param name="owner">
        ///  <c>true</c>, if the IntrinsicData is the owner of the native
        ///  object; <c>false</c>, otherwise.
        /// </param>
        public IntrinsicData(IntPtr handle, bool owner)
        {
            this.handle = handle;
            this.owner = owner;
        }

        public IntrinsicData(
            int width,
            int height,
            double fxNorm,
            double fyNorm,
            double cxNorm,
            double cyNorm,
            double skewNorm)
        {
            this.handle = vlNew_IntrinsicDataWrapper(
                width, height, fxNorm, fyNorm, cxNorm, cyNorm, skewNorm);
            this.owner = true;
        }

        ~IntrinsicData()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlIntrinsicDataWrapper_Clone(IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Creates a copy of this object and returns a Wrapper of it.
        /// </summary>
        /// <returns>
        ///  A wrapper of a copy of this object.
        /// </returns>
        public IntrinsicData Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }
            return new IntrinsicData(vlIntrinsicDataWrapper_Clone(this.handle), true);
        }

        public IntPtr getHandle()
        {
            return this.handle;
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlNew_IntrinsicDataWrapper(
            int width,
            int height,
            double fxNorm,
            double fyNorm,
            double cxNorm,
            double cyNorm,
            double skewNorm);

        [DllImport(VLSDK.dllName)]
        private static extern void vlDelete_IntrinsicDataWrapper(
            IntPtr intrinsicDataPerspectiveBaseWrapper);

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
                vlDelete_IntrinsicDataWrapper(this.handle);
            }
            this.handle = IntPtr.Zero;

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>
        ///  Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="IntrinsicData"/>. The <see cref="Dispose"/> method
        ///  leaves the <see cref="IntrinsicData"/> in an unusable state.
        ///  After calling <see cref="Dispose"/>, you must release all references
        ///  to the <see cref="IntrinsicData"/> so the garbage collector
        ///  can reclaim the memory that the <see cref="IntrinsicData"/>
        ///  was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.UInt32 vlIntrinsicDataWrapper_GetWidth(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the width of the intrinsic camera calibration.
        /// </summary>
        /// <returns>
        ///  The width in pixels.
        /// </returns>
        public int GetWidth()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return Convert.ToInt32(vlIntrinsicDataWrapper_GetWidth(this.handle));
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.UInt32 vlIntrinsicDataWrapper_GetHeight(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the height of the intrinsic camera calibration.
        /// </summary>
        /// <returns>
        ///  The height in pixels.
        /// </returns>
        public int GetHeight()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return Convert.ToInt32(vlIntrinsicDataWrapper_GetHeight(this.handle));
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.Double vlIntrinsicDataWrapper_GetFxNorm(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the normalized focal length of the intrinsic camera
        ///  calibration in x direction.
        /// </summary>
        /// <remarks>
        ///  The focal length in x direction was normalized through a division by
        ///  the width of the camera calibration.
        /// </remarks>
        /// <returns>
        ///  Normalized focal length in x direction.
        /// </returns>
        public double GetFxNorm()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetFxNorm(this.handle);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.Double vlIntrinsicDataWrapper_GetFyNorm(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the normalized focal length of the intrinsic camera
        ///  calibration in y direction.
        /// </summary>
        /// <remarks>
        ///  The focal length in y direction was normalized through a division by
        ///  the height of the camera calibration.
        /// </remarks>
        /// <returns>
        ///  Normalized focal length in y direction.
        /// </returns>
        public double GetFyNorm()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetFyNorm(this.handle);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.Double vlIntrinsicDataWrapper_GetSkewNorm(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the normalized skew of the intrinsic camera calibration.
        /// </summary>
        /// <remarks>
        ///  The skew was normalized through a division by the width of the
        ///  camera calibration.
        /// </remarks>
        /// <returns>
        ///  Normalized skew.
        /// </returns>
        public double GetSkewNorm()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetSkewNorm(this.handle);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.Double vlIntrinsicDataWrapper_GetCxNorm(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the normalized x-component of the principal point.
        /// </summary>
        /// <remarks>
        ///  The x-component was normalized through a division by the width of the
        ///  camera calibration.
        /// </remarks>
        /// <returns>
        ///  Normalized x-component of the principal point.
        /// </returns>
        public double GetCxNorm()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetCxNorm(this.handle);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.Double vlIntrinsicDataWrapper_GetCyNorm(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the normalized y-component of the principal point.
        /// </summary>
        /// <remarks>
        ///  The y-component was normalized through a division by the height of the
        ///  camera calibration.
        /// </remarks>
        /// <returns>
        ///  Normalized y-component of the principal point.
        /// </returns>
        public double GetCyNorm()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetCyNorm(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool
            vlIntrinsicDataWrapper_GetCalibrated(IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns whether the intrinsic parameters are valid.
        /// </summary>
        /// <remarks>
        /// A intrinsic camera calibration used for tracking should always be valid.
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the intrinsic calibration is valid;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool GetCalibrated()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetCalibrated(this.handle);
        }

        [DllImport(VLSDK.dllName)]
        private static extern System.Double vlIntrinsicDataWrapper_GetCalibrationError(
            IntPtr intrinsicDataWrapper);

        /// <summary>
        ///  Returns the calibration error.
        /// </summary>
        /// <remarks>
        ///  The re-projection error in pixel. This is interesting for evaluating
        ///  the quality of a camera calibration.
        /// </remarks>
        /// <returns>
        ///  NormalizedThe re-projection error in pixel.
        /// </returns>
        public double GetCalibrationError()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            return vlIntrinsicDataWrapper_GetCalibrationError(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlIntrinsicDataWrapper_GetDistortionParameters(
            IntPtr intrinsicDataWrapper,
            IntPtr k,
            System.UInt32 elementCount);

        /// <summary>
        ///  Retrieves the radial and tangential distortion parameters.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, on success; <c>false</c> otherwise.
        /// </returns>
        /// <param name="k">
        ///  Double array with 5 elements for storing the distortion parameters.
        /// </param>
        public bool GetDistortionParameters(double[] k)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            bool result = false;
            GCHandle arrayHandle = GCHandle.Alloc(k, GCHandleType.Pinned);
            try
            {
                result = vlIntrinsicDataWrapper_GetDistortionParameters(
                    this.handle,
                    arrayHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(k.Length));
            }
            finally
            {
                arrayHandle.Free();
            }

            return result;
        }

        public bool GetRadialDistortion(double[] k)
        {
            LogHelper.LogWarning(
                "The GetRadialDistortion() is deprecated, use GetDistortionParameters() instead.");
            return GetDistortionParameters(k);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlIntrinsicDataWrapper_SetDistortionParameters(
            IntPtr extrinsicDataWrapper,
            IntPtr k,
            System.UInt32 elementCount);

        /// <summary>
        ///  Sets the radial and tangential distortion parameters.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, on success; <c>false</c> otherwise.
        /// </returns>
        /// <param name="t">
        ///  Double array with 5 elements, which contains the distortion
        ///  parameters.
        /// </param>
        public bool SetDistortionParameters(double[] k)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            bool result = false;
            GCHandle arrayHandle = GCHandle.Alloc(k, GCHandleType.Pinned);
            try
            {
                result = vlIntrinsicDataWrapper_SetDistortionParameters(
                    this.handle,
                    arrayHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(k.Length));
            }
            finally
            {
                arrayHandle.Free();
            }

            return result;
        }

        public bool SetRadialDistortion(double[] k)
        {
            LogHelper.LogWarning(
                "The SetRadialDistortion() is deprecated, use SetDistortionParameters() instead.");
            return SetDistortionParameters(k);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlIntrinsicDataWrapper_GetProjectionMatrix(
            IntPtr intrinsicDataPerspectiveBaseWrapper,
            float nearFact,
            float farFact,
            System.UInt32 screenWidth,
            System.UInt32 screenHeight,
            System.UInt32 screenOrientation,
            System.UInt32 mode,
            IntPtr matrix,
            System.UInt32 matrixElementCount);

        /// <summary>
        ///  Computed the projection matrix from the intrinsic camera parameters.
        /// </summary>
        /// <remarks>
        ///  The returned matrix is stored in the following order
        ///  (column-major order):
        ///  \f[
        ///   \begin{bmatrix}
        ///    0 & 4 &  8 & 12\\
        ///    1 & 5 &  9 & 13\\
        ///    2 & 6 & 10 & 14\\
        ///    3 & 7 & 11 & 15\\
        ///   \end{bmatrix}
        ///  \f]
        /// </remarks>
        /// <returns>
        ///  <c>true</c>, if the projection matrix was gotten successfully;
        ///  <c>false</c> otherwise.
        /// </returns>
        /// <param name="nearFact">
        ///  Value for the near clipping plane.
        /// </param>
        /// <param name="farFact">
        ///  Value for the far clipping plane.
        /// </param>
        /// <param name="screenWidth">
        ///  Width of the screen.
        /// </param>
        /// <param name="screenHeight">
        ///  Height of the screen.
        /// </param>
        /// <param name="renderRotation">
        ///  How the rendering is rotated relative to the orientation of the images
        ///  received from the VisionLib. E.g., if the rendering happens in
        ///  landscape-left mode and the images are also in landscape-left mode,
        ///  then RenderRotation.CCW0 should be used. If the rendering happens in
        ///  portrait mode, but the images are in landscape-left mode, then
        ///  RenderRotation.CCW270 should be used. A default rotation for a given
        ///  ScreenRotation can be obtained from CameraHelper.GetRenderRotation.
        /// </param>
        /// <param name="mode">
        ///  The mode defines how to handle mismatching aspect ratios. Right now
        ///  the mode value is ignored, but later we will support different modes
        ///  like 'cover' (scale the projection surface up until it covers the
        ///  whole screen) and 'contain' (scale the projection surface down until
        ///  it is completely contained inside the screen).
        /// </param>
        /// <param name="matrix">
        ///  Float array with 16 elements for storing the projection matrix.
        /// </param>
        public bool GetProjectionMatrix(
            float nearFact,
            float farFact,
            int screenWidth,
            int screenHeight,
            RenderRotation renderRotation,
            int mode,
            float[] matrix)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLIntrinsicDataWrapper");
            }

            bool result = false;
            GCHandle matrixHandle = GCHandle.Alloc(matrix, GCHandleType.Pinned);
            try
            {
                uint orientation = Convert.ToUInt32(renderRotation);

                result = vlIntrinsicDataWrapper_GetProjectionMatrix(
                    this.handle,
                    nearFact,
                    farFact,
                    Convert.ToUInt32(screenWidth),
                    Convert.ToUInt32(screenHeight),
                    orientation,
                    Convert.ToUInt32(mode),
                    matrixHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(matrix.Length));
            }
            finally
            {
                matrixHandle.Free();
            }

            return result;
        }
    }
}
