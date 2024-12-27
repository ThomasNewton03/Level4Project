using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    ///  ExtrinsicData is a wrapper for an ExtrinsicData object.
    ///  ExtrinsicData objects represent the extrinsic camera parameters
    ///  (position and orientation).
    /// </summary>
    /// @ingroup Native
    public class ExtrinsicData : IDisposable
    {
        private IntPtr handle;
        private bool disposed = false;
        private bool owner;

        /// <summary>
        ///  Internal constructor of ExtrinsicData.
        /// </summary>
        /// <remarks>
        ///  This constructor is used internally by the VisionLib.
        /// </remarks>
        /// <param name="handle">
        ///  Handle to the native object.
        /// </param>
        /// <param name="owner">
        ///  <c>true</c>, if the ExtrinsicData is the owner of the native
        ///  object; <c>false</c>, otherwise.
        /// </param>
        public ExtrinsicData(IntPtr handle, bool owner)
        {
            this.handle = handle;
            this.owner = owner;
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlNew_ExtrinsicDataWrapper();

        public ExtrinsicData(Quaternion r, Vector3 t)
        {
            this.handle = vlNew_ExtrinsicDataWrapper();
            SetT(t);
            SetR(r);
            SetValid(true);
            this.owner = true;
        }

        ~ExtrinsicData()
        {
            // The finalizer was called implicitly from the garbage collector
            this.Dispose(false);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlExtrinsicDataWrapper_Clone(IntPtr extrinsicDataWrapper);

        /// <summary>
        ///  Creates a copy of this object and returns a Wrapper of it.
        /// </summary>
        /// <returns>
        ///  A wrapper of a copy of this object.
        /// </returns>
        public ExtrinsicData Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }
            return new ExtrinsicData(vlExtrinsicDataWrapper_Clone(this.handle), true);
        }

        public IntPtr getHandle()
        {
            return this.handle;
        }

        [DllImport(VLSDK.dllName)]
        private static extern void vlDelete_ExtrinsicDataWrapper(IntPtr extrinsicDataWrapper);

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
                vlDelete_ExtrinsicDataWrapper(this.handle);
            }
            this.handle = IntPtr.Zero;

            this.disposed = true;
        }

        /// <summary>
        ///  Explicitly releases references to unmanaged resources.
        /// </summary>
        /// <remarks>
        ///  Call <see cref="Dispose"/> when you are finished using the
        ///  <see cref="ExtrinsicData"/>. The  <see cref="Dispose"/> method
        ///  leaves the <see cref="ExtrinsicData"/> in an unusable state.
        ///  After calling <see cref="Dispose"/>, you must release all references to
        ///  the <see cref="ExtrinsicData"/> so the garbage collector can
        ///  reclaim the memory that the <see cref="ExtrinsicData"/> was
        ///  occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true); // Dispose was explicitly called by the user
            GC.SuppressFinalize(this);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_GetValid(IntPtr extrinsicDataWrapper);

        /// <summary>
        ///  Returns whether the current tracking pose is valid (the tracking was
        ///  successful).
        /// </summary>
        /// <returns>
        ///  <c>true</c>, if the current tracking pose is valid;
        ///  <c>false</c> otherwise.
        /// </returns>
        public bool GetValid()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            return vlExtrinsicDataWrapper_GetValid(this.handle);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_SetValid(
            IntPtr extrinsicDataWrapper,
            bool value);

        public void SetValid(bool value)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            if (!vlExtrinsicDataWrapper_SetValid(this.handle, value))
            {
                throw new InvalidOperationException("vlExtrinsicDataWrapper_SetValid");
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_GetModelViewMatrix(
            IntPtr extrinsicDataWrapper,
            IntPtr matrix,
            System.UInt32 matrixElementCount);

        /// <summary>
        ///  Returns the current camera pose as model-view matrix.
        /// </summary>
        /// <remarks>
        ///  The returned matrix assumes a right-handed coordinate system.
        ///  Throws an InvalidOperationExcception, if the ModelViewMatrix is invalid or can not be
        ///  retrieved.
        /// </remarks>
        public Matrix4x4 GetModelViewMatrix()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] matrix = new float[16];
            GCHandle matrixHandle = GCHandle.Alloc(matrix, GCHandleType.Pinned);
            try
            {
                if (!vlExtrinsicDataWrapper_GetModelViewMatrix(
                    this.handle,
                    matrixHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(matrix.Length)))
                {
                    throw new InvalidOperationException(
                        "vlExtrinsicDataWrapper_GetModelViewMatrix");
                }

                Matrix4x4 modelViewMatrix = new Matrix4x4();
                for (int i = 0; i < 16; ++i)
                {
                    modelViewMatrix[i % 4, i / 4] = matrix[i];
                }
                return modelViewMatrix;
            }
            finally
            {
                matrixHandle.Free();
            }
        }

        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_GetT(
            IntPtr extrinsicDataWrapper,
            IntPtr t,
            System.UInt32 elementCount);

        /// <summary>
        ///  Returns the translation \f$t\f$ of the ExtrinsicData.
        /// </summary>
        /// <remarks>
        ///  Notice, that \f$(R,t)\f$ represents the transformation of a
        ///  3D point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$
        ///  in camera coordinates: \f$P_c = RP_w + t\f$.
        ///  Throws an InvalidOperationExcception, if the Translation is invalid or
        ///  can not be set.
        /// </remarks>
        /// <returns>
        /// Translation of the ExtrinsicData
        /// </returns>
        public Vector3 GetT()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] t = new float[3];
            GCHandle vectorHandle = GCHandle.Alloc(t, GCHandleType.Pinned);
            try
            {
                if (!vlExtrinsicDataWrapper_GetT(
                    this.handle,
                    vectorHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(t.Length)))
                {
                    throw new InvalidOperationException("vlExtrinsicDataWrapper_GetT");
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
        private static extern bool vlExtrinsicDataWrapper_SetT(
            IntPtr extrinsicDataWrapper,
            IntPtr t,
            System.UInt32 elementCount);

        /// <summary>
        ///  Sets the translation \f$t\f$ of the ExtrinsicData.
        /// </summary>
        /// <remarks>
        ///  Notice, that \f$(R,t)\f$ represents the transformation of a
        ///  3D point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$
        ///  in camera coordinates: \f$P_c = RP_w + t\f$.
        ///  Throws an InvalidOperationExcception, if the Translation is invalid or can not be
        ///  changed.
        /// </remarks>
        public void SetT(Vector3 translation)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] t = new float[3];
            t[0] = translation.x;
            t[1] = translation.y;
            t[2] = translation.z;
            GCHandle vectorHandle = GCHandle.Alloc(t, GCHandleType.Pinned);
            try
            {
                if (!vlExtrinsicDataWrapper_SetT(
                    this.handle,
                    vectorHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(3)))
                {
                    throw new InvalidOperationException("vlExtrinsicDataWrapper_SetT");
                }
            }
            finally
            {
                vectorHandle.Free();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_GetR(
            IntPtr extrinsicDataWrapper,
            IntPtr r,
            System.UInt32 elementCount);

        /// <summary>
        ///  Returns the rotation \f$R\f$ of the ExtrinsicData.
        /// </summary>
        /// <remarks>
        ///  Notice, that \f$(R,t)\f$ represents the transformation of a
        ///  3D point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$
        ///  in camera coordinates: \f$P_c = RP_w + t\f$.
        ///  Throws an InvalidOperationExcception, if the Rotation is invalid or can not be
        ///  retrieved.
        /// </remarks>
        /// <returns>
        /// Rotation of the ExtrinsicData as Quaternion
        /// </returns>
        public Quaternion GetR()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] r = new float[4];
            GCHandle quaternionHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
            try
            {
                if (!vlExtrinsicDataWrapper_GetR(
                    this.handle,
                    quaternionHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(r.Length)))
                {
                    throw new InvalidOperationException("vlExtrinsicDataWrapper_GetR");
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
        private static extern bool vlExtrinsicDataWrapper_SetR(
            IntPtr extrinsicDataWrapper,
            IntPtr r,
            System.UInt32 elementCount);

        /// <summary>
        ///  Sets the rotation \f$R\f$ of the ExtrinsicData.
        /// </summary>
        /// <remarks>
        ///  Notice, that \f$(R,t)\f$ represents the transformation of a
        ///  3D point \f$P_w\f$ from world coordinates into a 3D point \f$P_c\f$
        ///  in camera coordinates: \f$P_c = RP_w + t\f$.
        ///  Throws an InvalidOperationExcception, if the Rotation is invalid or can not be
        ///  set.
        /// </remarks>
        public void SetR(Quaternion rotation)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] r = new float[4];
            r[0] = rotation.x;
            r[1] = rotation.y;
            r[2] = rotation.z;
            r[3] = rotation.w;

            GCHandle quaternionHandle = GCHandle.Alloc(r, GCHandleType.Pinned);
            try
            {
                if (!vlExtrinsicDataWrapper_SetR(
                    this.handle,
                    quaternionHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(r.Length)))
                {
                    throw new InvalidOperationException("vlExtrinsicDataWrapper_SetR");
                }
            }
            finally
            {
                quaternionHandle.Free();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_GetCamPosWorld(
            IntPtr extrinsicDataWrapper,
            IntPtr t,
            System.UInt32 elementCount);

        /// <summary>
        ///  Returns the position \f$P_{cam}\f$ of the camera in world coordinates.
        /// </summary>
        /// <remarks>
        ///  Internally the position \f$P_{cam}\f$ will be computed from the
        ///  transformation \f$(R,t)\f$ which transforms a 3D point from world
        ///  coordinates into camera coordinates (\f$P_{cam} = -R^{-1}t\f$).
        ///  Throws an InvalidOperationExcception, if the operation cannot be processed.
        /// </remarks>
        /// <returns>
        /// Position of the camera in world coordinates.
        /// </returns>
        public Vector3 GetCamPosWorld()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] pos = new float[3];
            GCHandle vectorHandle = GCHandle.Alloc(pos, GCHandleType.Pinned);
            try
            {
                if (!vlExtrinsicDataWrapper_GetCamPosWorld(
                    this.handle,
                    vectorHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(pos.Length)))
                {
                    throw new InvalidOperationException("vlExtrinsicDataWrapper_GetCamPosWorld");
                }

                return new Vector3(pos[0], pos[1], pos[2]);
            }
            finally
            {
                vectorHandle.Free();
            }
        }

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(VLSDK.dllName)]
        private static extern bool vlExtrinsicDataWrapper_SetCamPosWorld(
            IntPtr extrinsicDataWrapper,
            IntPtr t,
            System.UInt32 elementCount);

        /// <summary>
        ///  Sets the position \f$P_{cam}\f$ of the camera in world coordinates.
        /// </summary>
        /// <remarks>
        ///  Internally this will be stored as a transformation \f$(R,t)\f$ of a 3D
        ///  point from world coordinates into camera coordinates
        ///  (\f$t = -RP_{cam}\f$).
        ///  Throws an InvalidOperationExcception, if the operation cannot be processed.
        /// </remarks>
        public void SetCamPosWorld(Vector3 camPos)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            float[] pos = new float[3];
            pos[0] = camPos.x;
            pos[1] = camPos.y;
            pos[2] = camPos.z;
            GCHandle vectorHandle = GCHandle.Alloc(pos, GCHandleType.Pinned);
            try
            {
                if (vlExtrinsicDataWrapper_SetCamPosWorld(
                    this.handle,
                    vectorHandle.AddrOfPinnedObject(),
                    Convert.ToUInt32(pos.Length)))
                {
                    throw new InvalidOperationException("vlExtrinsicDataWrapper_SetCamPosWorld");
                }
            }
            finally
            {
                vectorHandle.Free();
            }
        }

        public void SetFromCamera(Camera camera)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLExtrinsicDataWrapper");
            }

            Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
            // Convert from left-handed to right-handed model-view matrix
            worldToCameraMatrix[0, 2] = -worldToCameraMatrix[0, 2];
            worldToCameraMatrix[1, 2] = -worldToCameraMatrix[1, 2];
            worldToCameraMatrix[2, 2] = -worldToCameraMatrix[2, 2];
            // Convert from OpenGL coordinates into VisionLib coordinates
            worldToCameraMatrix = CameraHelper.flipYZ * worldToCameraMatrix;

            SetT(worldToCameraMatrix.GetColumn(3));
            SetR(CameraHelper.QuaternionFromMatrix(worldToCameraMatrix));
        }
    }
}
