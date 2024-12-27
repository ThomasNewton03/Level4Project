using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API.WorkSpace
{
    /// <summary>
    ///  Definition of a transformation with quaternion and translation
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class Transform
    {
        public float[] t;
        public float[] r;

        public Transform()
        {
            this.t = new float[3] {0, 0, 0};
            this.r = new float[4] {0, 0, 0, 1};
        }

        public Transform(Vector3 translation, Quaternion rotation)
        {
            this.t = new float[3] {translation.x, translation.y, translation.z};
            this.r = new float[4] {rotation.x, rotation.y, rotation.z, rotation.w};
        }

        public static Transform[] parseBinaryTransforms(IntPtr data, int count)
        {
            if (data == IntPtr.Zero)
            {
                return new WorkSpace.Transform[0];
            }

            float[] vector = new float[count * 7];
            Marshal.Copy(data, vector, 0, count * 7);
            VLSDK.ReleaseMemory(data);

            WorkSpace.Transform[] result = new WorkSpace.Transform[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = new WorkSpace.Transform(
                    new Vector3(vector[i * 7 + 0], vector[i * 7 + 1], vector[i * 7 + 2]),
                    new Quaternion(
                        vector[i * 7 + 3],
                        vector[i * 7 + 4],
                        vector[i * 7 + 5],
                        vector[i * 7 + 6]));
            }

            return result;
        }
    }

    /// <summary>
    ///  Base type of WorkSpace geometries
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class Geometry
    {
        /// <summary>
        ///  Parameters of WorkSpace geometries
        /// </summary>
        [Serializable]
        public class Parameters
        {
            public Transform transformation;

            public int planeSteps;
            public float planeLength;
            public float planeWidth;

            public int sphereSamples;
            public float sphereRadius;
            public float sphereThetaStart;
            public float sphereThetaLength;
            public float spherePhiStart;
            public float spherePhiLength;

            public float[] boundingBoxMin;
            public float[] boundingBoxMax;
        }

        public string type;
        public Parameters parameters;

        public Geometry(string typeName)
        {
            this.type = typeName;
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlSDKUtil_getCameraPositionsFromGeometry(
            [MarshalAs(UnmanagedType.LPStr)] string baseNodeJson,
            out int size);

        /// <summary>
        /// Function for obtaining all positions of a geometry definition directly from vlSDK.
        /// </summary>
        /// <returns>Array of Unity coordinates, which are described by the given Geometry</returns>
        public Vector3[] GetCameraPositions()
        {
            int positionsSize = 0;
            IntPtr positions = vlSDKUtil_getCameraPositionsFromGeometry(
                JsonHelper.ToJson(this),
                out positionsSize);

            if (positions == IntPtr.Zero)
            {
                return new Vector3[0];
            }

            float[] positionsVector = new float[positionsSize * 3];
            Marshal.Copy(positions, positionsVector, 0, positionsSize * 3);
            VLSDK.ReleaseMemory(positions);

            Vector3[] cameraPositions = new Vector3[positionsSize];

            for (int i = 0; i < positionsSize; i++)
            {
                cameraPositions[i] = CameraHelper.VLPoseToUnityPose(
                    new Vector3(
                        positionsVector[i * 3],
                        positionsVector[i * 3 + 1],
                        positionsVector[i * 3 + 2]));
            }

            return cameraPositions;
        }

        [System.Obsolete("WorkSpace.Geometry.Remap is obsolete.")]
        /// <summary>
        /// </summary>
        /// \deprecated WorkSpace.Geometry.Remap is obsolete.
        public static float Remap(
            float srcValue,
            float srcRangeMin,
            float srcRangeMax,
            float dstRangeMin,
            float dstRangeMax)
        {
            return MathHelper.Remap(srcValue, srcRangeMin, srcRangeMax, dstRangeMin, dstRangeMax);
        }
    }

    /// <summary>
    ///  WorkSpace Geometry describing a plane in x (width) and y (length) direction.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class Plane : Geometry
    {
        private static readonly string defaultTypeName = "plane";

        public Plane()
            : base(defaultTypeName)
        {
            this.parameters = new Parameters();
            this.parameters.transformation = new Transform();

            this.parameters.planeSteps = 1;
            this.parameters.planeLength = 0;
            this.parameters.planeWidth = 0;
        }

        public Plane(float length, float width, int steps, WorkSpace.Transform trans)
            : base(defaultTypeName)
        {
            this.parameters = new Parameters();

            this.parameters.planeLength = length;
            this.parameters.planeWidth = width;
            this.parameters.planeSteps = steps;

            this.parameters.transformation = trans;
        }
    }

    /// @ingroup WorkSpace
    public class BoundingBox : Geometry
    {
        private static readonly string defaultTypeName = "boundingBox";

        public BoundingBox()
            : this(new Bounds()) {}

        public BoundingBox(Vector3 min, Vector3 max)
            : base(defaultTypeName)
        {
            this.parameters = new Parameters();
            this.parameters.boundingBoxMin = new float[3] {-min.x, min.y, min.z};
            this.parameters.boundingBoxMax = new float[3] {-max.x, max.y, max.z};
        }

        public BoundingBox(Bounds bounds)
            : this(bounds.min, bounds.max) {}
    }

    /// @ingroup WorkSpace
    public class BaseSphere : Geometry
    {
        private static readonly string defaultTypeName = "baseSphere";

        public BaseSphere()
            : this(0, 360, 0, 180, 0.2f) {}

        public BaseSphere(
            float thetaStart,
            float thetaLength,
            float phiStart,
            float phiLength,
            float detailLevel)
            : base(defaultTypeName)
        {
            this.parameters = new Parameters();
            this.parameters.sphereThetaStart = thetaStart;
            this.parameters.sphereThetaLength = thetaLength;
            this.parameters.spherePhiStart = phiStart;
            this.parameters.spherePhiLength = phiLength;
            this.parameters.sphereSamples =
                Mathf.FloorToInt(MathHelper.Remap(detailLevel, 0f, 1f, 42f, 1281f));
        }
    }

    /// <summary>
    ///  WorkSpace Geometry describing a sphere.
    ///  Phi describes the polar angle in degree around the x axis.
    ///  Theta describes the azimuth angle around the z axis
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class Sphere : BaseSphere
    {
        private static readonly string defaultTypeName = "sphere";

        public Sphere()
            : base()
        {
            this.parameters.transformation = new Transform();
            this.parameters.sphereRadius = 1;
            this.type = defaultTypeName;
        }

        public Sphere(
            float radius,
            float thetaStart,
            float thetaLength,
            float phiStart,
            float phiLength,
            float detailLevel,
            WorkSpace.Transform trans)
            : base(thetaStart, thetaLength, phiStart, phiLength, detailLevel)
        {
            this.parameters.sphereRadius = radius;
            this.parameters.transformation = trans;
            this.type = defaultTypeName;
        }
    }

    /// <summary>
    ///  Definition of a single workspace.
    ///  It contains one origin and one destination geometry from which the possible poses will be
    ///  calculated. Additionally, the upVector and the rotation values define the possible view
    ///  angles of the camera.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class Definition
    {
        public enum Type
        {
            Simple,
            Advanced
        }

        public string type = "WorkSpaceDef";
        public Transform transformation;
        public float rollAngleRange;
        public float rollAngleStep;
        public float fieldOfView;
        public float[] upVector;

        [SerializeField]
        public Geometry origin;

        [SerializeField]
        public Geometry destination;

        public Definition(
            Transform trans,
            Vector3 upVector,
            float rollAngleRange,
            float rollAngleStep,
            float fieldOfView,
            Type type)
        {
            this.transformation = trans;
            this.rollAngleRange = rollAngleRange;
            this.rollAngleStep = rollAngleStep;
            this.fieldOfView = fieldOfView;
            this.type = ToString(type);
            this.upVector = new float[3] {upVector.x, upVector.y, upVector.z};
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlSDKUtil_getCameraTransformsFromWorkspaceDefinition(
            [MarshalAs(UnmanagedType.LPStr)] string workspaceJson,
            out int size);

        private string ToString(Type type)
        {
            switch (type)
            {
                case Type.Simple:
                    return "Simple";

                case Type.Advanced:
                    return "WorkSpaceDef";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Function for obtaining all poses of a Simple WorkSpace definition directly from vlSDK.
        /// </summary>
        /// <returns>
        /// Array of Workspace.Transform in the vlSDK coordinate system, which represent all
        /// camera poses described by this WorkSpace.Definition
        /// </returns>
        public Transform[] GetCameraTransforms()
        {
            int transformsSize = 0;
            IntPtr transforms = vlSDKUtil_getCameraTransformsFromWorkspaceDefinition(
                JsonHelper.ToJson(this),
                out transformsSize);

            return Transform.parseBinaryTransforms(transforms, transformsSize);
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlSDKUtil_getCameraPositionsFromWorkspaceDefinition(
            [MarshalAs(UnmanagedType.LPStr)] string baseNodeJson,
            out int size);

        /// <summary>
        /// Function for obtaining all positions of a geometry definition directly from vlSDK.
        /// </summary>
        /// <returns>Array of Unity coordinates, which are described by the given Geometry</returns>
        public Vector3[] GetCameraPositions()
        {
            int positionsSize = 0;
            IntPtr positions = vlSDKUtil_getCameraPositionsFromWorkspaceDefinition(
                JsonHelper.ToJson(this),
                out positionsSize);

            if (positions == IntPtr.Zero)
            {
                return new Vector3[0];
            }

            float[] positionsVector = new float[positionsSize * 3];
            Marshal.Copy(positions, positionsVector, 0, positionsSize * 3);
            VLSDK.ReleaseMemory(positions);

            Vector3[] cameraPositions = new Vector3[positionsSize];

            for (int i = 0; i < positionsSize; i++)
            {
                cameraPositions[i] = CameraHelper.VLPoseToUnityPose(
                    new Vector3(
                        positionsVector[i * 3],
                        positionsVector[i * 3 + 1],
                        positionsVector[i * 3 + 2]));
            }

            return cameraPositions;
        }

        [DllImport(VLSDK.dllName)]
        private static extern IntPtr vlSDKUtil_getOriginTransformFromSimpleWorkspaceDefinition(
            [MarshalAs(UnmanagedType.LPStr)] string workspaceJson,
            out int size);

        /// <summary>
        /// Function for obtaining all poses of a Simple WorkSpace definition directly from vlSDK.
        /// </summary>
        /// <returns>
        /// Array of Workspace.Transform in the vlSDK coordinate system, which represent all
        /// camera poses described by this WorkSpace.Definition
        /// </returns>
        public Matrix4x4 GetOriginTransform()
        {
            int transformsSize = 0;
            IntPtr transforms = vlSDKUtil_getOriginTransformFromSimpleWorkspaceDefinition(
                JsonHelper.ToJson(this),
                out transformsSize);

            var trans = Transform.parseBinaryTransforms(transforms, transformsSize)[0];

            Vector3 position = new Vector3(trans.t[0], trans.t[1], trans.t[2]);
            Quaternion rotation = new Quaternion(trans.r[0], trans.r[1], trans.r[2], trans.r[3]);

            var mt = new ModelTransform(rotation, position);
            return CameraHelper.flipX * mt.ToMatrix() * CameraHelper.flipX;
        }
    }

    /// <summary>
    /// WorkSpace Configuration for a scene.
    /// It contains all WorkSpaces of the Scene, which should be used for AutoInit.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class Configuration
    {
        public string type = "VisionLibWorkSpacesConfig";
        public int version = 1;

        public List<WorkSpace.Definition> workSpaces;

        public Configuration(List<WorkSpace.Definition> workSpaces)
        {
            this.workSpaces = workSpaces;
        }

        /// <summary>
        /// Writes this WorkSpace.Configuration into the specified file.
        /// </summary>
        /// <param name="fileName">Path of the file to write the data in.</param>
        /// <remarks>
        ///  <para>
        ///   It's possible to use vlSDK file schemes (e.g. local-storage-dir) here.
        ///  </para>
        /// </remarks>
        public void WriteToFile(string fileName)
        {
            VLSDK.Set(fileName, JsonHelper.ToJson(this));
        }
    }
}
