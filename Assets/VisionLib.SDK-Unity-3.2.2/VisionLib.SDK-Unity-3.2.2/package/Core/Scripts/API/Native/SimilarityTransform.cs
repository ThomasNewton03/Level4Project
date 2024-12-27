using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  The SimilarityTransform is a wrapper for a SimilarityTransform
    ///  object. SimilarityTransform objects represent a transformation
    ///  (position, orientation and scaling).
    /// </summary>
    /// @ingroup Native
    public class SimilarityTransform : IDisposable
    {
        private IntPtr handle;
        private bool disposed = false;
        private bool owner;

        /// <summary>
        ///  Internal constructor of SimilarityTransform.
        /// </summary>
        /// <remarks>
        ///  This constructor is used internally by the VisionLib.
        /// </remarks>
        /// <param name="handle">
        ///  Handle to the native object.
        /// </param>
        /// <param name="owner">
        ///  <c>true</c>, if the SimilarityTransform is the owner of the native
        ///  object; <c>false</c>, otherwise.
        /// </param>
        public SimilarityTransform(IntPtr handle, bool owner)
        {
            this.handle = handle;
            this.owner = owner;
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlNew_SimilarityTransformWrapper();

        public SimilarityTransform(Quaternion r, Vector3 t, float s)
        {
            this.handle = vlNew_SimilarityTransformWrapper();
            SetT(t);
            SetR(r);
            SetS(s);
            SetValid(true);
            this.owner = true;
        }

        ~SimilarityTransform()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlSimilarityTransformWrapper_Clone(
            IntPtr similarityTransformWrapper);

        /// <summary>
        ///  Creates a copy of this object and returns a Wrapper of it.
        /// </summary>
        /// <returns>
        ///  A wrapper of a copy of this object.
        /// </returns>
        public SimilarityTransform Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }
            return new SimilarityTransform(vlSimilarityTransformWrapper_Clone(this.handle), true);
        }

        public IntPtr getHandle()
        {
            return this.handle;
        }

        [DllImport(VLSDK.dllName)]
        private static extern void vlDelete_SimilarityTransformWrapper(
            IntPtr similarityTransformWrapper);

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
                vlDelete_SimilarityTransformWrapper(this.handle);
            }
            this.handle = IntPtr.Zero;

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>
        ///  Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="SimilarityTransform"/>. The  <see cref="Dispose"/> method
        ///  leaves the <see cref="SimilarityTransform"/> in an unusable state.
        ///  After calling <see cref="Dispose"/>, you must release all references to
        ///  the <see cref="SimilarityTransform"/> so the garbage collector can
        ///  reclaim the memory that the <see cref="SimilarityTransform"/> was
        ///  occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_GetValid(
            IntPtr similarityTransformWrapper);

        /// <summary>
        ///  Returns whether the SimilarityTransform is valid.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if the SimilarityTransform is valid;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool GetValid()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            return vlSimilarityTransformWrapper_GetValid(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_SetValid(
            IntPtr similarityTransformWrapper,
            bool value);

        public void SetValid(bool value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            if (!vlSimilarityTransformWrapper_SetValid(this.handle, value))
            {
                throw new InvalidOperationException("vlSimilarityTransformWrapper_SetValid");
            }
        }

        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_GetT(
            IntPtr similarityTransformWrapper,
            IntPtr t,
            System.UInt32 elementCount);

        /// <summary>
        ///  Returns the translation \f$t\f$ of the given SimilarityTransform.
        /// </summary>
        /// <remarks>
        /// Notice, that \f$(s,R,t)\f$ represents the transformation of a 3D
        /// point as follows: \f$y = s* Rx + t\f$.
        /// </remarks>
        /// <returns>
        /// Translation of the SimilarityTransform
        /// </returns>
        public Vector3 GetT()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            float[] t = new float[3];
            GCHandle vectorHandle = GCHandle.Alloc(t, GCHandleType.Pinned);
            try
            {
                if (!vlSimilarityTransformWrapper_GetT(
                    this.handle,
                    vectorHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(t.Length)))
                {
                    throw new InvalidOperationException("vlSimilarityTransformWrapper_GetT");
                }

                return new Vector3(t[0], t[1], t[2]);
            }
            finally
            {
                vectorHandle.Free();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_SetT(
            IntPtr similarityTransformWrapper,
            IntPtr t,
            System.UInt32 elementCount);

        /// <summary>
        ///  Sets the translation \f$t\f$ of the given SimilarityTransform.
        /// </summary>
        /// <remarks>
        /// Notice, that \f$(s,R,t)\f$ represents the transformation of a 3D
        /// point as follows: \f$y = s* Rx + t\f$.
        /// </remarks>
        public void SetT(Vector3 translation)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            float[] t = new float[3];
            t[0] = translation.x;
            t[1] = translation.y;
            t[2] = translation.z;
            GCHandle vectorHandle = GCHandle.Alloc(t, GCHandleType.Pinned);
            try
            {
                if (!vlSimilarityTransformWrapper_SetT(
                    this.handle,
                    vectorHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(t.Length)))
                {
                    throw new InvalidOperationException("vlSimilarityTransformWrapper_SetT");
                }
            }
            finally
            {
                vectorHandle.Free();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_GetR(
            IntPtr similarityTransformWrapper,
            IntPtr r,
            System.UInt32 elementCount);

        /// <summary>
        ///  Returns the rotation \f$R\f$ of the given SimilarityTransform.
        /// </summary>
        /// <remarks>
        /// Notice, that \f$(s,R,t)\f$ represents the transformation of a 3D
        /// point as follows: \f$y = s* Rx + t\f$.
        /// </remarks>
        /// <returns>
        ///  Rotation of the SimilarityTransform as Quaternion
        /// </returns>
        public Quaternion GetR()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            float[] r = new float[4];
            GCHandle quaternionHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
            try
            {
                if (!vlSimilarityTransformWrapper_GetR(
                    this.handle,
                    quaternionHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(r.Length)))
                {
                    throw new InvalidOperationException("vlSimilarityTransformWrapper_GetR");
                }

                return new Quaternion(r[0], r[1], r[2], r[3]);
            }
            finally
            {
                quaternionHandle.Free();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_SetR(
            IntPtr similarityTransformWrapper,
            IntPtr r,
            System.UInt32 elementCount);

        /// <summary>
        ///  Sets the rotation \f$R\f$ of the given SimilarityTransform.
        /// </summary>
        /// <remarks>
        /// Notice, that \f$(s,R,t)\f$ represents the transformation of a 3D
        /// point as follows: \f$y = s* Rx + t\f$.
        /// </remarks>
        public void SetR(Quaternion rotation)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            float[] r = new float[4];
            r[0] = rotation.x;
            r[1] = rotation.y;
            r[2] = rotation.z;
            r[3] = rotation.w;
            GCHandle quaternionHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
            try
            {
                if (!vlSimilarityTransformWrapper_SetR(
                    this.handle,
                    quaternionHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(r.Length)))
                {
                    throw new InvalidOperationException("vlSimilarityTransformWrapper_SetR");
                }
            }
            finally
            {
                quaternionHandle.Free();
            }
        }

        [DllImport(VLSDK.dllName)]
        private static extern float vlSimilarityTransformWrapper_GetS(
            IntPtr similarityTransformWrapper);

        /// <summary>
        ///  Returns the scale factor \f$s\f$ of the given SimilarityTransform.
        /// </summary>
        /// <remarks>
        /// Notice, that \f$(s,R,t)\f$ represents the transformation of a 3D
        /// point as follows: \f$y = s* Rx + t\f$.
        /// </remarks>
        /// <returns>
        ///  Float which contains the scale factor.
        /// </returns>
        public float GetS()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            float s = vlSimilarityTransformWrapper_GetS(this.handle);
            if (s < 0)
            {
                throw new InvalidOperationException("vlSimilarityTransformWrapper_GetS");
            }
            return s;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlSimilarityTransformWrapper_SetS(
            IntPtr similarityTransformWrapper,
            float s);

        /// <summary>
        ///  Sets the scale factor \f$s\f$ of the given SimilarityTransform.
        /// </summary>
        /// <remarks>
        /// Notice, that \f$(s,R,t)\f$ represents the transformation of a 3D
        /// point as follows: \f$y = s* Rx + t\f$.
        /// </remarks>
        /// <param name="s">
        ///  Float which contains the scale factor.
        /// </param>
        public void SetS(float s)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLSimilarityTransformWrapper");
            }

            if (!vlSimilarityTransformWrapper_SetS(this.handle, s))
            {
                throw new InvalidOperationException("vlSimilarityTransformWrapper_SetS");
            }
        }
    }
}
