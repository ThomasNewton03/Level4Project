using System;
using System.Linq;
using UnityEngine;
using Visometry.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class MeshHelper
    {
        /// <summary>
        /// True only if the combined bounding box of the meshes on the GameObject and all its
        /// children is strictly inside the specified camera's view frustum. 
        /// </summary>
        public static bool IsMeshEntirelyInsideCameraViewFrustum(
            GameObject gameObject,
            Camera camera)
        {
            ThrowIfGameObjectHasNoMeshesOrCameraIsNull(gameObject, camera);

            var rendererBounds = BoundsUtilities.GetRendererBounds(gameObject, true);

            var frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            return rendererBounds.Corners().All(
                corner => IsPointStrictlyInsideFrustum(frustum, corner));
        }

        /// <summary>
        /// True only if the point is inside the frustum and not any of the frustum planes.
        /// Assumes the frustum plane normals are oriented inwards.
        /// </summary>
        private static bool IsPointStrictlyInsideFrustum(Plane[] frustumPlanes, Vector3 point)
        {
            return frustumPlanes.All(plane => plane.GetDistanceToPoint(point) > float.Epsilon);
        }

        /// <summary>
        /// Calculates the combined bounding box of the meshes on the GameObject and all its children,
        /// then centers this bounding box in the specified camera's view frustum.
        /// The camera's near and far planes are moved outwards to fit the bounding box if it
        /// otherwise would be clipped.
        /// </summary>
        internal static void CenterMeshInCameraView(GameObject gameObject, Camera camera)
        {
            ThrowIfGameObjectHasNoMeshesOrCameraIsNull(gameObject, camera);
            CenterBoundingBoxInCameraView(
                gameObject,
                BoundsUtilities.GetRendererBounds(gameObject, true),
                camera);
        }

        internal static void CenterBoundingBoxInCameraView(
            GameObject gameObject,
            Bounds boundsInWorldCS,
            Camera camera)
        {
            var cameraTransform = camera.transform;
            var cameraAlignedBounds = boundsInWorldCS.Rotate(cameraTransform.rotation);
            var extents = cameraAlignedBounds.extents;

            // Calculate the distance to the object front face from this and the field of view (fov)
            var fovYHalf = camera.fieldOfView * Mathf.PI / 360f;
            var fovXHalf = fovYHalf * camera.aspect;
            var twoFy = 1 / Mathf.Tan(fovYHalf);
            var twoFx = 1 / Mathf.Tan(fovXHalf);

            // Extents are half the size of the bounding box - so the following is equivalent to `size * f`
            var distanceFrontX = extents.x * twoFx;
            var distanceFrontY = extents.y * twoFy;
            var distanceFront = Mathf.Max(distanceFrontX, distanceFrontY);

            // Calculate the distance to the object center
            var distance = distanceFront + extents.z;

            // Estimate good near and far values
            var near = distanceFront * 0.25f;
            var far = distanceFront + Mathf.Max(extents.x, extents.y, extents.z) * 3.0f;

#if UNITY_EDITOR
            Undo.RecordObject(gameObject.transform, "Adjusting the initPose of the trackingAnchor");
#endif

            // This is the position where the center finally has to be
            var targetCenterPositionInWorldCS =
                cameraTransform.TransformPoint(new Vector3(0, 0, distance));

            // Here it is right now in the trackingAnchor CS
            var centerInAnchorCS =
                gameObject.transform.InverseTransformPoint(boundsInWorldCS.center);

            // So in world coordinates, this is the following delta to the origin
            var deltaInWorldCS = gameObject.transform.rotation * centerInAnchorCS;

            // In order to get the center to the position where we want it to be, we put the TrackingAnchor origin here
            gameObject.transform.position = targetCenterPositionInWorldCS - deltaInWorldCS;

            if (camera.nearClipPlane > near)
            {
                const string adjustmentMessage =
                    "Adjusting the nearClipPlane of the SLAM camera to make the object completely visible";
#if UNITY_EDITOR
                Undo.RecordObject(camera, adjustmentMessage);
#endif
                LogHelper.LogWarning(adjustmentMessage, camera);
                camera.nearClipPlane = near;
            }
            if (camera.farClipPlane < far)
            {
                const string adjustmentMessage =
                    "Adjusting the farClipPlane of the SLAM camera to make the object completely visible.";
#if UNITY_EDITOR
                Undo.RecordObject(camera, adjustmentMessage);
#endif
                LogHelper.LogWarning(adjustmentMessage, camera);
                camera.farClipPlane = far;
            }
        }

        private static void ThrowIfGameObjectHasNoMeshesOrCameraIsNull(
            GameObject gameObject,
            Camera camera)
        {
            if (gameObject.GetComponentInChildren<MeshFilter>() == null)
            {
                throw new ArgumentException(
                    "The provided GameObject and its children have no meshes.");
            }
            if (!camera)
            {
                throw new ArgumentException("Camera may not be null.");
            }
        }
    }
}
