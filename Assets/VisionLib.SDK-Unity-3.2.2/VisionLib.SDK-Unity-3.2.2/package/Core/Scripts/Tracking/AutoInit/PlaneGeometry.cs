using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Stores data that represents equidistant samplings of a section of a plane.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class PlaneGeometry : BaseGeometry
    {
        public float length = 1f;
        public float width = 1f;

        [Tooltip("Point count of one line of the plane")]
        public int step = 3;

        public override API.WorkSpace.Geometry CreateGeometry(API.WorkSpace.Transform trans)
        {
            return new API.WorkSpace.Plane(this.length, this.width, this.step, trans);
        }

        public override float GetGeometrySize()
        {
            if (this.length == 0f && this.width == 0f)
            {
                return 0.1f;
            }

            return this.length > this.width ? this.length : this.width;
        }
    }

    /// <summary>
    /// Stores data that represents equidistant samplings of a section of a line.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class LineGeometry : BaseGeometry
    {
        public float length = 1f;

        [Tooltip("Point count of the line")]
        public int step = 3;

        private API.WorkSpace.Geometry CreatePlane(API.WorkSpace.Transform trans)
        {
            return new API.WorkSpace.Plane(this.length, 0f, this.step, trans);
        }

        public override API.WorkSpace.Geometry CreateGeometry(API.WorkSpace.Transform trans)
        {
            return CreatePlane(trans);
        }

        public override float GetGeometrySize()
        {
            return this.length > 0 ? this.length : 0.1f;
        }
    }

    /// <summary>
    /// Stores data of a single point.
    /// 
    /// Since only local data is stored in BaseGeometry classes, this does not have any fields.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public class PointGeometry : BaseGeometry
    {
        private API.WorkSpace.Geometry CreatePlane(API.WorkSpace.Transform trans)
        {
            return new API.WorkSpace.Plane(0f, 0f, 1, trans);
        }

        public override API.WorkSpace.Geometry CreateGeometry(API.WorkSpace.Transform trans)
        {
            return CreatePlane(trans);
        }

        public override float GetGeometrySize()
        {
            return 0.1f;
        }
    }
}
