using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.ARFoundation
{
    /**
     *  @ingroup ARFoundation
     */
    [AddComponentMenu("VisionLib/AR Foundation/XR Camera")]
    [HelpURL(DocumentationLink.APIReferenceURI.ARFoundation + "x_r_camera.html")]
    public class XRCamera : MonoBehaviour
    {
        /// <summary>
        ///  The ARCameraManager which will produce frame events. Will be set on the AR Camera by
        ///  default.
        /// </summary>
        [Tooltip(
            "The ARCameraManager which will produce frame events. Will be set on the AR Camera by default.")]
        public ARCameraManager cameraManager;

        /// <summary>
        ///  The GameObject from which to read the camera to world transform from.
        /// </summary>
        [Tooltip("The GameObject from which to read the camera to world transform from.")]
        [DisplayName("AR Camera")]
        public Camera arCamera;

        /// <summary>
        ///  Maximum frequency to forward events from the arCameraManager to the worker.
        /// </summary>
        /// <remarks>
        ///  Ignores all events that happen less then 1/maxTrackingFPS after a processed event.
        /// </remarks>
        [Tooltip("Maximum frequency to forward events from the arCameraManager to the worker.")]
        public double maxTrackingFPS = 15.0;

        private MirroringDataStore mirror = new MirroringDataStore();
        private DateTime lastFrameTimestamp = System.DateTime.Now;
        private Matrix4x4 renderRotationMatrixFromUnityToVL = RenderRotationHelper.rotationZ0;

        private void OnEnable()
        {
            Application.onBeforeRender += SampleFrame;
            OnOrientationChange(ScreenOrientationObserver.GetScreenOrientation());
            ScreenOrientationObserver.OnOrientationChange += OnOrientationChange;
        }

        private void OnDisable()
        {
            ScreenOrientationObserver.OnOrientationChange -= OnOrientationChange;
            Application.onBeforeRender -= SampleFrame;
        }

        private bool ShouldTrackCurrentFrame()
        {
            DateTime now = System.DateTime.Now;
            if (now.Subtract(this.lastFrameTimestamp).TotalMilliseconds <
                1000.0 / this.maxTrackingFPS)
            {
                return false;
            }
            this.lastFrameTimestamp = now;
            return true;
        }

        [CanBeNull]
        private ARFoundationFrame GetFrame()
        {
            var frame = new ARFoundationFrame(this.mirror);
            XRCameraIntrinsics intrinsics;
            if (!this.cameraManager.TryGetIntrinsics(out intrinsics))
            {
                frame.Dispose();
                return null;
            }

            XRCpuImage image;
            if (!this.cameraManager.TryAcquireLatestCpuImage(out image))
            {
                frame.Dispose();
                return null;
            }

            frame.image = image;
            frame.targetSize = GetTrackingImageSize(image.width, image.height);
            frame.intrinsicData = ToVLIntrinsic(intrinsics, frame.targetSize);
            frame.extrinsicData = GetCurrentExtrinsicData();
            return frame;
        }

        private void SampleFrame()
        {
            if (!SynchronousTrackingManager.Instance.IsReady() || !this.ShouldTrackCurrentFrame())
            {
                return;
            }

            var frame = GetFrame();
            if (frame == null)
            {
                return;
            }
            
#if UNITY_EDITOR
            try
            {
                SynchronousTrackingManager.Instance.Push(frame.Evaluate());
            }
            catch (InvalidOperationException e)
            {
                frame.Dispose();
                LogHelper.LogException(e);
            }
#else
            SynchronousTrackingManager.Instance.Push(frame);
#endif
        }

        private static Vector2Int GetTrackingImageSize(int inputImageWidth, int inputImageHeight)
        {
            const int targetWidth = 640;
            int w = Math.Min(targetWidth, inputImageWidth);
            int h = inputImageHeight * w / inputImageWidth;
            return new Vector2Int(w, h);
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            var renderRotation = orientation.GetRenderRotation();
            this.renderRotationMatrixFromUnityToVL =
                renderRotation.GetMatrixFromUnityToVL();
        }

        private static IntrinsicData ToVLIntrinsic(XRCameraIntrinsics intr, Vector2Int resolution)
        {
            return new IntrinsicData(
                resolution.x,
                resolution.y,
                intr.focalLength.x / intr.resolution.x,
                intr.focalLength.y / intr.resolution.y,
                intr.principalPoint.x / intr.resolution.x,
                intr.principalPoint.y / intr.resolution.y,
                0);
        }

        private void OnDestroy()
        {
            this.mirror.Dispose();
        }

        private ExtrinsicData GetCurrentExtrinsicData()
        {
            Quaternion r;
            Vector4 t;
            var worldToCameraMatrix = this.renderRotationMatrixFromUnityToVL * this.arCamera.worldToCameraMatrix;
            CameraHelper.WorldToCameraMatrixToVLPose(worldToCameraMatrix, out t, out r);
            var cameraFromUnityWorldTransform = new ModelTransform(r, t);
            var unityWorldFromVisionLibWorld = CameraHelper.flipXY;
            var cameraFromVisionLibWorld = cameraFromUnityWorldTransform * unityWorldFromVisionLibWorld;
            return new ExtrinsicData(cameraFromVisionLibWorld.r, cameraFromVisionLibWorld.t);
        }
    }
}
