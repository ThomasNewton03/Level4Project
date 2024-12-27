using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Base class for all geometry classes that represent source or destinations of workspaces.
    ///
    /// Those classes actually store the data, allow editing and create the serialized representation
    /// of the data when the workspaces need to be sent to the native VisionLib SDK.
    /// </summary>
    /// @ingroup WorkSpace
    [Serializable]
    public abstract class BaseGeometry
    {
        [NonSerialized]
        public Vector3[] currentMesh = new Vector3[0];

        /// <summary>
        /// Updates the stored positions of the geometry by the values taken from vlSDK.
        /// </summary>
        public void UpdateMesh()
        {
            API.WorkSpace.Geometry geometry = CreateGeometry(new API.WorkSpace.Transform());
            SetMesh(geometry.GetCameraPositions());
        }

        public void SetMesh(Vector3[] points)
        {
            this.currentMesh = points;
        }

        /// <summary>
        /// Creates a corresponding WorkSpace.Geometry, which can be used in the vlSDK.
        /// </summary>
        /// <param name="trans">Transform, which will be applied to the Geometry</param>
        /// <returns></returns>
        public abstract API.WorkSpace.Geometry CreateGeometry(API.WorkSpace.Transform trans);

        /// <summary>
        /// Calculates the biggest boundary value of the geometry
        /// </summary>
        /// <returns>Highest boundary size of the geometry</returns>
        public abstract float GetGeometrySize();
    }
}
