using System;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Re-parents this Transform under the new parent transform. Records an undo if run in
        /// the unity editor.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="newParent">New parent Transform.</param>
        /// <param name="worldPositionStays">If true, the parent-relative position, scale and
        /// rotation are modified such that the object keeps the same world space position,
        /// rotation and scale as before.</param>
        public static void SetParentWithUndo(
            this Transform transform,
            Transform newParent,
            bool worldPositionStays = true)
        {
#if UNITY_EDITOR
            Undo.SetTransformParent(
                transform,
                newParent,
                worldPositionStays,
                $"Make {transform.name} child of {newParent.name}.");
#else
            transform.SetParent(newParent, worldPositionStays);
#endif
        }

        /// <summary>
        /// Resets the target transform's pose to identity.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        public static void ResetPose(this Transform transform)
        {
            transform.SetPose(Pose.identity);
        }

        /// <summary>
        /// Resets the transform's local pose to identity.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        public static void ResetLocalPose(this Transform transform)
        {
            transform.SetLocalPose(Pose.identity);
        }

        /// <summary>
        /// Returns the Transform's position and rotation as a pose.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        public static Pose ToPose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }

        /// <summary>
        /// Sets the Transform's position and rotation to the values given in the pose.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="pose">The new world pose to set.</param>
        public static void SetPose(this Transform transform, Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        /// <summary>
        /// Sets the Transform's position and rotation to the values given in the ModelTransform.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="modelTransform">The new world pose to set.</param>
        public static void SetPose(this Transform transform, ModelTransform modelTransform)
        {
            transform.SetPositionAndRotation(modelTransform.t, modelTransform.r);
        }

        /// <summary>
        /// Sets the Transform's local position and rotation to the values given in the pose.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="pose">The new local pose to set.</param>
        public static void SetLocalPose(this Transform transform, Pose pose)
        {
            transform.SetLocalPositionAndRotation(pose.position, pose.rotation);
        }

        /// <summary>
        /// Sets the Transform's local position and rotation to the values given in the ModelTransform.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="modelTransform">The new local pose to set.</param>
        public static void SetLocalPose(this Transform transform, ModelTransform modelTransform)
        {
            transform.SetLocalPositionAndRotation(modelTransform.t, modelTransform.r);
        }

        /// <summary>
        /// Checks whether the Transform's world pose is identity.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        public static bool IsWorldPoseIdentity(this Transform transform)
        {
            return transform.position == Vector3.zero && transform.rotation == Quaternion.identity;
        }

        /// <summary>
        /// Checks whether the Transform's local pose is identity.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        public static bool IsLocalPoseIdentity(this Transform transform)
        {
            return transform.localPosition == Vector3.zero &&
                   transform.localRotation == Quaternion.identity;
        }

        /// <summary>
        /// Transforms a rotation from local space to world space.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="rotation">The rotation to transform from the target's local space into world
        /// space.</param>
        public static Quaternion TransformRotation(this Transform transform, Quaternion rotation)
        {
            return transform.rotation * rotation;
        }

        /// <summary>
        ///   Transforms a rotation from world space to local space.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="rotation">The rotation to transform into the target's local space.</param>
        public static Quaternion InverseTransformRotation(
            this Transform transform,
            Quaternion rotation)
        {
            return Quaternion.Inverse(transform.rotation) * rotation;
        }

        /// <summary>
        /// Transforms a pose (position and rotation) from local space to world space.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="pose">The pose to transform from the target's local space into world
        /// space.</param>
        public static Pose TransformPose(this Transform transform, Pose pose)
        {
            return new Pose(
                transform.TransformPoint(pose.position),
                transform.TransformRotation(pose.rotation));
        }

        /// <summary>
        /// Transforms a pose (position and rotation) from world space to local space.
        /// </summary>
        /// <param name="transform">The target transform.</param>
        /// <param name="pose">The pose to transform into the target's local space.</param>
        public static Pose InverseTransformPose(this Transform transform, Pose pose)
        {
            return new Pose(
                transform.InverseTransformPoint(pose.position),
                transform.InverseTransformRotation(pose.rotation));
        }

        public static void RotateAround(
            this Transform transform,
            Vector3 center,
            Quaternion rotation)
        {
            var modelTransform = new ModelTransform(transform);
            var rotateAroundCenter = new ModelTransform(rotation, center) *
                                     new ModelTransform(Quaternion.identity, -center);
            transform.SetPose(rotateAroundCenter * modelTransform);
        }
    }
}
