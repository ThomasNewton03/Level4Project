using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Stores data that represents sections of a sphere limited by a horizontal and vertical angle range.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public abstract class SphereGeometry : BaseGeometry
    {
        [Range(0f, 1f)]
        [Tooltip("Effects the point count of the whole sphere")]
        public float detailLevel = 0.1f;

        [Range(0f, 180f)]
        [Tooltip("Vertical starting angle")]
        public float thetaStart = 0f;
        [Range(0f, 180f)]
        [Tooltip("Vertical sweep angle size")]
        public float thetaLength = 90f;
        [Range(0f, 360f)]
        [Tooltip("Horizontal starting angle")]
        public float phiStart = 0f;
        [Range(0f, 360f)]
        [Tooltip("Horizontal sweep angle size")]
        public float phiLength = 360f;
    }

    /// <summary>
    /// SphereGeometry without a radius parameter. It will inherit the size from "currentMesh" instead.
    ///
    /// Can be used with the SimpleWorkSpace only.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class SimpleSphereGeometry : SphereGeometry
    {
        public override API.WorkSpace.Geometry CreateGeometry(API.WorkSpace.Transform trans)
        {
            return new API.WorkSpace.BaseSphere(
                this.thetaStart,
                this.thetaLength,
                this.phiStart,
                this.phiLength,
                this.detailLevel);
        }

        private Bounds GetBaseSphereBounds()
        {
            if (this.currentMesh.Length == 0)
            {
                return new Bounds();
            }

            Bounds boundingBox = new Bounds();
            boundingBox.center = this.currentMesh[0];
            boundingBox.extents = Vector3.zero;
            foreach (Vector3 vertex in currentMesh)
            {
                boundingBox.Encapsulate(vertex);
            }
            return boundingBox;
        }

        public override float GetGeometrySize()
        {
            return GetBaseSphereBounds().size.magnitude / 2f;
        }
    }

    /// <summary>
    /// SphereGeomgetry in which one can control the radius via parameters.
    ///
    /// Can be used with the AdvancedWorkSpace only.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class AdvancedSphereGeometry : SphereGeometry
    {
        public float radius = 1;

        public override API.WorkSpace.Geometry CreateGeometry(API.WorkSpace.Transform trans)
        {
            return new API.WorkSpace.Sphere(
                this.radius,
                this.thetaStart,
                this.thetaLength,
                this.phiStart,
                this.phiLength,
                this.detailLevel,
                trans);
        }

        public override float GetGeometrySize()
        {
            return this.radius * 1.2f;
        }
    }
}
