using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class PoseExtensions
    {
        public static Matrix4x4 ToMatrix(this Pose pose)
        {
            return Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
        }

        public static bool IsApproximatelyEqualTo(this Pose pose, Pose otherPose)
        {
            return pose.position.Equals(otherPose.position) &&
                   pose.rotation.IsApproximatelyEqualTo(otherPose.rotation);
        }

        public static bool IsApproximatelyEqualTo(
            this Quaternion rotation,
            Quaternion otherRotation)
        {
            return Mathf.Abs(Quaternion.Dot(rotation, otherRotation)) > (1.0f - 10e-6f);
        }
    }
}
