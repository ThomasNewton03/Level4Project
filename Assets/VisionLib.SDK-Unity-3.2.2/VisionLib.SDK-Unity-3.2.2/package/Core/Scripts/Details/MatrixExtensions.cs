using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class MatrixExtensions
    {
        public static Pose ToPose(this Matrix4x4 m)
        {
            return new Pose(m.GetPosition(), m.rotation);
        }
    }
}
