using System;
using UnityEngine;

namespace Visometry.Helpers
{
    public static class TransformUtilities
    {
        /// <summary>
        /// Multiplies all scales of all hierarchy levels
        /// </summary>
        public static Vector3 GetGlobalScale(Transform transform)
        {
            if (!transform.parent)
            {
                return transform.localScale;
            }

            var combinedScale = GetGlobalScale(transform.parent);
            combinedScale.x *= transform.localScale.x;
            combinedScale.y *= transform.localScale.y;
            combinedScale.z *= transform.localScale.z;
            return combinedScale;
        }

        public static Matrix4x4? GetRelativeTransformAncestor(Transform source, Transform ancestor)
        {
            Matrix4x4 result = Matrix4x4.identity;
            while (source != null)
            {
                if (source == ancestor)
                {
                    return result;
                }
                result = Matrix4x4.TRS(source.localPosition, source.localRotation, source.localScale) * result;
                source = source.parent;
            }
            return null;
        }
        
        public static Matrix4x4 GetRelativeTransformNoAncestor(Transform source, Transform target)
        {
            return target.worldToLocalMatrix * source.localToWorldMatrix;
        }

        /// <summary>
        /// Calculates the matrix that transforms from the coordinate system of source into
        /// the coordinate system of target.
        /// If no target is given, it transforms in the world coordinate system instead.
        /// </summary>
        public static Matrix4x4 GetRelativeTransform(
            Transform source,
            Transform target)
        {
            if (target == null)
            {
                return source.localToWorldMatrix;
            }
            var relativeTransform = GetRelativeTransformAncestor(source, target);
            if (relativeTransform.HasValue)
            {
                return relativeTransform.Value;
            }
            return GetRelativeTransformNoAncestor(source, target);
        }
    }
}
