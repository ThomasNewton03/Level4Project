using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class InitPoseHandler
    {
        [SerializeField]
        [Tooltip(
            "If enabled, this GameObject's transform is kept upright w.r.t. the world" +
            "\"Up\" vector.")]
        public bool keepUpright;

        [SerializeField]
        [Tooltip("The model \"Up\" vector is kept aligned with this vector.")]
        public Vector3 worldUpVector = Vector3.up;

        [SerializeField]
        [Tooltip("The tracking model's \"Up\" direction in the world coordinate system.")]
        public Vector3 modelUpVector = Vector3.up;

        [Tooltip(
            "When enabled the defined initialization pose will be used for Tracking. " +
            "If this is disabled, you should provide other ways of initializing (e.g. AutoInit " +
            "with WorkSpaces or InitData templates).")]
        public bool useInitPose = true;
        public bool isInitPoseInitializedInBackend;
        private TrackingAnchor trackingAnchor;
        private TransformCache initPoseInBackend;
        private TransformCache relativeInitPoseInBackend;
        private Matrix4x4 renderRotationMatrixFromUnityToVL;
        private ModelTransform? initPoseInCameraCoordinateSystem;
        private ModelTransform? relativeInitPose;
        private bool currentlyUpdatingInitPose;

        private bool IsAnchorEnabledAndTrackingRunning()
        {
            return this.trackingAnchor && this.trackingAnchor.IsAnchorEnabled &&
                   TrackingManager.DoesTrackerExistAndIsRunning();
        }

        public InitPoseHandler()
        {
            this.initPoseInBackend = new TransformCache();
            this.relativeInitPoseInBackend = new TransformCache();
            this.renderRotationMatrixFromUnityToVL = RenderRotationHelper.rotationZ0;
        }

        public void Initialize(TrackingAnchor trackingAnchor)
        {
            this.trackingAnchor = trackingAnchor;
            this.isInitPoseInitializedInBackend = false;
        }

        public void UpdateInitPoseInBackend()
        {
            TrackingManager.CatchCommandErrors(
                WriteInitPoseToBackendIfDifferent(),
                this.trackingAnchor);
        }

        public void RegisterCallbacks()
        {
            OnOrientationChange(ScreenOrientationObserver.GetScreenOrientation());
            ScreenOrientationObserver.OnOrientationChange += OnOrientationChange;
            TrackingManager.OnTrackerInitialized += HandleTrackerInitialized;
        }

        public void DeregisterCallbacks()
        {
            TrackingManager.OnTrackerInitialized -= HandleTrackerInitialized;
            ScreenOrientationObserver.OnOrientationChange -= OnOrientationChange;
        }

        public Task<WorkerCommands.CommandWarnings> SetRelativeInitPoseAndWriteToBackend(
            ModelTransform initPose)
        {
            this.relativeInitPose = initPose;
            return WriteInitPoseToBackendIfDifferent();
        }

        public Task<WorkerCommands.CommandWarnings> SetInitPoseAndWriteToBackend(
            Transform transformWithInitPose)
        {
            this.initPoseInCameraCoordinateSystem =
                ConvertTransformToInitPoseInCameraCoordinateSystem(transformWithInitPose);
            return WriteInitPoseToBackendIfDifferent();
        }

        public ModelTransform? GetInitPoseInCameraCoordinateSystem()
        {
            if (!this.initPoseInCameraCoordinateSystem.HasValue)
            {
                return null;
            }

            if (!this.keepUpright)
            {
                return this.initPoseInCameraCoordinateSystem;
            }

            if (!GetSlamCamera())
            {
                ThrowForMissingSlamCamera();
            }

            return RotateToMatchWorldUpVector(this.initPoseInCameraCoordinateSystem.Value);
        }

        public ModelTransform? GetInitPoseInCameraCoordinateSystemRotated()
        {
            return this.renderRotationMatrixFromUnityToVL * GetInitPoseInCameraCoordinateSystem();
        }

        public ModelTransform? GetInitPoseInWorldCoordinateSystem()
        {
            return ConvertInitPoseIntoWorldCoordinateSystem(GetInitPoseInCameraCoordinateSystem());
        }

        public ModelTransform? GetInitPoseInParentCoordinateSystem()
        {
            return this.relativeInitPose;
        }

        public ModelTransform? ConvertTransformToInitPoseInWorldCoordinateSystem(
            Transform transform)
        {
            return ConvertInitPoseIntoWorldCoordinateSystem(
                ConvertTransformToInitPoseInCameraCoordinateSystem(transform));
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            this.initPoseInBackend.Invalidate();
            var renderRotation = orientation.GetRenderRotation();
            this.renderRotationMatrixFromUnityToVL = renderRotation.GetMatrixFromUnityToVL();
        }

        private void HandleTrackerInitialized()
        {
            this.initPoseInBackend.Invalidate();
            this.relativeInitPoseInBackend.Invalidate();
        }

        private bool IsBackendOutOfSync()
        {
            if (!this.trackingAnchor.ActiveInBackend())
            {
                return false;
            }
            if (this.relativeInitPose.HasValue)
            {
                return this.relativeInitPoseInBackend.UpdateTransform(relativeInitPose.Value);
            }
            var maybeInitPose = GetInitPoseInCameraCoordinateSystemRotated();
            if (maybeInitPose.HasValue)
            {
                return this.initPoseInBackend.UpdateTransform(maybeInitPose.Value) ||
                       IsOpticalSeeThroughDevice();
            }
            return false;
        }

        private Task<WorkerCommands.CommandWarnings> WriteInitPoseToBackendIfDifferent()
        {
            if (!IsAnchorEnabledAndTrackingRunning() || this.currentlyUpdatingInitPose)
            {
                return Task.FromResult(WorkerCommands.NoWarnings());
            }
            List<Task<WorkerCommands.CommandWarnings>> tasks = new();
            if (this.relativeInitPoseInBackend.IsValid() && !this.trackingAnchor.HasParentAnchor())
            {
                // If we do not have a parent anchor, we remove the relativeInitPose from the backend
                tasks.Add(RemoveRelativeInitPoseFromBackend());
            }

            if (this.initPoseInBackend.IsValid() &&
                (!this.useInitPose || this.trackingAnchor.HasParentAnchor()))
            {
                // If we have a parent anchor or do not want to use any initPose, we remove the initPose from the backend
                tasks.Add(RemoveInitPoseFromBackend());
            }

            if ((this.useInitPose || this.trackingAnchor.HasParentAnchor()) && IsBackendOutOfSync())
            {
                tasks.Add(WriteInitPoseToBackend());
            }
            return BlockUpdatesWhileAwaitingResult(WorkerCommands.AwaitAll(tasks));
        }

        private Task<WorkerCommands.CommandWarnings> RemoveInitPoseFromBackend()
        {
            this.initPoseInBackend.Invalidate();
            return MultiModelTrackerCommands.DisableInitPoseAsync(
                TrackingManager.Instance.Worker,
                this.trackingAnchor.GetAnchorName());
        }

        private Task<WorkerCommands.CommandWarnings> RemoveRelativeInitPoseFromBackend()
        {
            this.relativeInitPoseInBackend.Invalidate();
            return MultiModelTrackerCommands.DisableRelativeInitPoseAsync(
                TrackingManager.Instance.Worker,
                this.trackingAnchor.GetAnchorName());
        }

        private async Task<WorkerCommands.CommandWarnings> WriteRelativeInitPoseToBackend(
            ModelTransform initPose)
        {
            var param = CameraHelper.MatrixToRelativeInitPose(initPose.ToMatrix());
            var warnings = await MultiModelTrackerCommands.SetRelativeInitPoseAsync(
                TrackingManager.Instance.Worker,
                this.trackingAnchor.GetAnchorName(),
                param);
            this.trackingAnchor.ThrowIfNotAlive();
            this.isInitPoseInitializedInBackend = true;
            return warnings;
        }

        private async Task<WorkerCommands.CommandWarnings> WriteInitPoseToBackend(
            ModelTransform initPose)
        {
            var param = CameraHelper.MatrixToGlobalObjectPose(initPose.ToMatrix());
            var warnings = await MultiModelTrackerCommands.SetInitPoseAsync(
                TrackingManager.Instance.Worker,
                this.trackingAnchor.GetAnchorName(),
                param);
            this.trackingAnchor.ThrowIfNotAlive();
            this.isInitPoseInitializedInBackend = true;
            return warnings;
        }

        private async Task<WorkerCommands.CommandWarnings> WriteGlobalObjectPoseToBackend(
            ModelTransform globalObjectPose)
        {
            var param = CameraHelper.MatrixToGlobalObjectPose(globalObjectPose.ToMatrix());
            var warnings = await MultiModelTrackerCommands.SetGlobalObjectPoseAsync(
                TrackingManager.Instance.Worker,
                this.trackingAnchor.GetAnchorName(),
                param);
            this.trackingAnchor.ThrowIfNotAlive();
            this.isInitPoseInitializedInBackend = true;
            return warnings;
        }

        private bool IsOpticalSeeThroughDevice()
        {
#if VL_WITH_OPTICALSEETHROUGH || VL_WITH_MAGICLEAP || (UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR))
            return true;
#else
            return false;
#endif
        }

        private async Task<WorkerCommands.CommandWarnings> WriteInitPoseToBackend()
        {
            if (this.relativeInitPose.HasValue)
            {
                return await WriteRelativeInitPoseToBackend(relativeInitPose.Value);
            }

            if (IsOpticalSeeThroughDevice())
            {
                var writeGlobalObjectPose = GetInitPoseInWorldCoordinateSystem();
                if (writeGlobalObjectPose.HasValue)
                {
                    return await WriteGlobalObjectPoseToBackend(writeGlobalObjectPose.Value);
                }
            }
            else
            {
                var maybeInitPose = GetInitPoseInCameraCoordinateSystemRotated();
                if (maybeInitPose.HasValue)
                {
                    return await WriteInitPoseToBackend(maybeInitPose.Value);
                }
            }
            return WorkerCommands.NoWarnings();
        }

        private async Task<WorkerCommands.CommandWarnings> BlockUpdatesWhileAwaitingResult(
            Task<WorkerCommands.CommandWarnings> task)
        {
            this.currentlyUpdatingInitPose = true;
            try
            {
                return await task;
            }
            finally
            {
                this.currentlyUpdatingInitPose = false;
            }
        }

        private ModelTransform? ConvertTransformToInitPoseInCameraCoordinateSystem(
            Transform transform)
        {
            var slamCamera = GetSlamCamera();
            if (!slamCamera)
            {
                return null;
            }
            var transformToCameraMatrix =
                slamCamera.transform.worldToLocalMatrix * transform.localToWorldMatrix;
            return new ModelTransform(transformToCameraMatrix);
        }

        private ModelTransform? ConvertInitPoseIntoWorldCoordinateSystem(
            ModelTransform? maybeInitPose)
        {
            var slamCamera = GetSlamCamera();
            if (!maybeInitPose.HasValue || !slamCamera)
            {
                return null;
            }
            return slamCamera.transform.localToWorldMatrix * maybeInitPose;
        }

        [CanBeNull]
        private Camera GetSlamCamera()
        {
            return !this.trackingAnchor ? null : this.trackingAnchor.GetSLAMCamera();
        }

        private Vector3? TransformVectorFromWorldToCameraSpace(Vector3 worldVector)
        {
            return GetSlamCamera()?.transform.InverseTransformVector(worldVector);
        }

        private ModelTransform RotateToMatchWorldUpVector(ModelTransform poseInCameraCoordinates)
        {
            var desiredUpVectorIfSlamCameraSet =
                TransformVectorFromWorldToCameraSpace(this.worldUpVector);
            if (!desiredUpVectorIfSlamCameraSet.HasValue)
            {
                ThrowForMissingSlamCamera();
            }
            var desiredUpVector = desiredUpVectorIfSlamCameraSet.Value;
            var actualUpVector = poseInCameraCoordinates.TransformDirection(this.modelUpVector);
            var go = this.trackingAnchor.gameObject;
            var modelCenter = poseInCameraCoordinates.TransformPoint(
                BoundsUtilities.GetMeshBoundsInParentCoordinates(go, go.transform, true).center);

            if (Vector3.Angle(desiredUpVector, actualUpVector) < 10e-8)
            {
                return poseInCameraCoordinates;
            }

            var uprightRotation = Quaternion.FromToRotation(actualUpVector, desiredUpVector);

            return poseInCameraCoordinates.RotateAroundCenter(modelCenter, uprightRotation);
        }

        private static void ThrowForMissingSlamCamera()
        {
            throw new InvalidOperationException(
                "SLAM camera not set. This operation depends on the SLAM camera's transform.");
        }
    }
}
