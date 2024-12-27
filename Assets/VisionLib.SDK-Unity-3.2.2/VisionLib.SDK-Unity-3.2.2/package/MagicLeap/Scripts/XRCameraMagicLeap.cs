using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.MagicLeap
{
    // @ingroup MagicLeap
    [HelpURL(DocumentationLink.APIReferenceURI.MagicLeap + "x_r_camera_magic_leap.html")]
    public class XRCameraMagicLeap : MonoBehaviour
    {
        [SerializeField] [Tooltip("The width of the image that will be passed into Vision Lib")]
        private int width = 640;

        [SerializeField] [Tooltip("The height of the image that will be passed into Vision Lib")]
        private int height = 480;

        [Tooltip(
            "The frame rate of the video capture. Note, 60fps is not support on resolutions greater than 1440x1080")]
        [SerializeField]
        private MLCamera.CaptureFrameRate frameRate = MLCamera.CaptureFrameRate._30FPS;

        private MLCamera cvCamera;
        private Coroutine setUpCameraCoroutine;

        private enum State
        {
            Initial,
            PermissionGranted,
            Available,
            Connected,
            Capturing
        };

        private static State GetLowerState(State a, State b)
        {
            return (State)Math.Min((int)a, (int)b);
        }

        private static State GetHigherState(State a, State b)
        {
            return (State)Math.Max((int)a, (int)b);
        }

        private State state = State.Initial;

        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        private void Awake()
        {
            this.permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            this.permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            this.permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
        }

        private void OnDestroy()
        {
            if (this.cvCamera != null)
            {
                this.cvCamera.OnRawVideoFrameAvailable -= PushFrame;
            }

            this.permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
            this.permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            this.permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        }

        private void OnDisable()
        {
            if (this.state >= State.Connected)
            {
                this.cvCamera.Disconnect();
            }

            this.state = GetLowerState(this.state, State.PermissionGranted);
            this.cvCamera = null;
        }

        void Start()
        {
            MLPermissions.RequestPermission(MLPermission.Camera, this.permissionCallbacks);
        }

        private void OnEnable()
        {
            SetUpCamera();
        }

        private void StopVideoCapture()
        {
            if (this.setUpCameraCoroutine != null)
            {
                StopCoroutine(this.setUpCameraCoroutine);
            }

            if (this.state == State.Capturing)
            {
                this.cvCamera.CaptureVideoStop();
            }

            this.state = GetLowerState(this.state, State.Connected);
        }

        private void SetUpCamera()
        {
            if (this.state >= State.PermissionGranted && this.setUpCameraCoroutine == null)
            {
                this.setUpCameraCoroutine = StartCoroutine(SetUpCameraAsync());
            }
        }

        private IEnumerator SetUpCameraAsync()
        {
            // Assumes, that the permission is already granted.
            while (this.state < State.Available)
            {
                try
                {
                    MLCamera.GetDeviceAvailabilityStatus(
                        MLCamera.Identifier.CV,
                        out bool cameraDeviceAvailable);
                    if (cameraDeviceAvailable)
                    {
                        this.state = State.Available;
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.SendError(
                        $"Exception from MLCamera.GetDeviceAvailabilityStatus: {e}",
                        this);
                }

                yield return new WaitForSeconds(1.0f);
            }

            if (this.state < State.Connected)
            {
                ConnectCamera();
            }

            while (this.state < State.Connected)
            {
                yield return new WaitForSeconds(1.0f);
            }

            if (this.state < State.Capturing)
            {
                StartVideoCapture();
            }

            this.setUpCameraCoroutine = null;
        }

        private async void ConnectCamera()
        {
            MLCamera.ConnectContext connectContext = MLCamera.ConnectContext.Create();
            connectContext.CamId = MLCamera.Identifier.CV;
            connectContext.Flags = MLCamera.ConnectFlag.CamOnly;
            connectContext.EnableVideoStabilization = false;
            this.cvCamera = await MLCamera.CreateAndConnectAsync(connectContext);
            if (this.cvCamera != null)
            {
                this.state = State.Connected;
            }
        }

        private async void StartVideoCapture()
        {
            if (this.cvCamera == null)
            {
                NotificationHelper.SendError("Unable to properly Connect MLCamera", this);
                return;
            }

            //Get all of the supported streams for the connected camera
            var streamCapabilities = MLCamera.GetImageStreamCapabilitiesForCamera(
                this.cvCamera,
                MLCamera.CaptureType.Video);

            if (streamCapabilities == null || streamCapabilities.Length == 0)
            {
                NotificationHelper.SendError("No stream caps received", this);
                return;
            }

            // Select the stream that best matches the width and height
            MLCamera.TryGetBestFitStreamCapabilityFromCollection(
                streamCapabilities,
                this.width,
                this.height,
                MLCamera.CaptureType.Video,
                out MLCamera.StreamCapability selectedCapability);

            NotificationHelper.SendInfo(
                $"Streaming in {selectedCapability.Width}x{selectedCapability.Height}",
                this);

            // Prepare the camera capture with the selected stream settings
            MLCamera.CaptureConfig captureConfig = new MLCamera.CaptureConfig();
            captureConfig.CaptureFrameRate = this.frameRate;
            captureConfig.StreamConfigs = new MLCamera.CaptureStreamConfig[1];
            captureConfig.StreamConfigs[0] = MLCamera.CaptureStreamConfig.Create(
                selectedCapability,
                MLCamera.OutputFormat.RGBA_8888);
            MLResult result = this.cvCamera.PrepareCapture(captureConfig, out MLCamera.Metadata _);
            if (!result.IsOk)
            {
                NotificationHelper.SendError($"Error in PrepareCapture: {result}", this);
                return;
            }

            await this.cvCamera.PreCaptureAEAWBAsync();
            result = await this.cvCamera.CaptureVideoStartAsync();

            if (!result.IsOk)
            {
                NotificationHelper.SendError($"Image capture failed. Reason: {result}", this);
                return;
            }

            this.state = State.Capturing;
            this.cvCamera.OnRawVideoFrameAvailable += PushFrame;
        }

        private void PushFrame(
            MLCamera.CameraOutput output,
            MLCamera.ResultExtras extras,
            MLCamera.Metadata metadataHandle)
        {
            Frame frame = new Frame();

            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return;
            }

            int width = (int)(output.Planes[0].Stride / output.Planes[0].BytesPerPixel);
            frame.image = Image.CreateFromBuffer(
                output.Planes[0].Data,
                width,
                (int)output.Planes[0].Height);
            frame.intrinsicData = ToVlIntrinsic(extras.Intrinsics);
            frame.extrinsicData = GetExtrinsicsAtTime(extras.VCamTimestamp);
            frame.timestamp = 0.000001 * extras.VCamTimestamp;
            try
            {
                SynchronousTrackingManager.Instance.Push(frame);
            }
            catch (NullSingletonException e)
            {
                LogHelper.LogException(e);
            }
        }

        ExtrinsicData GetExtrinsicsAtTime(MLTime vcamTimestampUs)
        {
#if UNITY_ANDROID
            MLResult result = MLCVCamera.GetFramePose(vcamTimestampUs, out var outMatrix);
            if (result.IsOk)
            {
                return CameraMatrixToExtrinsicData(outMatrix);
            }

            return null;
#else
        return null;
#endif
        }

        private static ExtrinsicData CameraMatrixToExtrinsicData(Matrix4x4 cameraMatrix)
        {
            Quaternion r = Quaternion.identity;
            Vector4 t = Vector4.zero;
            var matrix_CameraFromUnityWorld = Matrix4x4.Inverse(
                Matrix4x4.TRS(
                    cameraMatrix.GetPosition(),
                    cameraMatrix.rotation,
                    new Vector3(1, 1, -1)));
            var matrix_CameraFromWorld = matrix_CameraFromUnityWorld * CameraHelper.flipXY;
            CameraHelper.WorldToCameraMatrixToVLPose(matrix_CameraFromWorld, out t, out r);
            return new ExtrinsicData(r, t);
        }

        private static IntrinsicData ToVlIntrinsic(
            MLCamera.IntrinsicCalibrationParameters? cameraIntrinsicParameters)
        {
            if (cameraIntrinsicParameters == null)
            {
                return null;
            }

            var cameraParameters = cameraIntrinsicParameters.Value;
            return new IntrinsicData(
                (int)cameraParameters.Width,
                (int)cameraParameters.Height,
                cameraParameters.FocalLength.x / cameraParameters.Width,
                cameraParameters.FocalLength.y / cameraParameters.Height,
                cameraParameters.PrincipalPoint.x / cameraParameters.Width,
                cameraParameters.PrincipalPoint.y / cameraParameters.Height,
                0);
        }

        private void OnPermissionDenied(string permission)
        {
            var result = MLResult.Create(MLResult.Code.PermissionDenied);
            NotificationHelper.SendError(
                $"CameraIntrinsicTest failed to get requested permissions, disabling script. Reason: {result}",
                this);
            this.enabled = false;
        }

        private void OnPermissionGranted(string permission)
        {
            NotificationHelper.SendInfo($"Succeeded in requesting all permissions.", this);
            this.state = GetHigherState(this.state, State.PermissionGranted);
            SetUpCamera();
        }
    }
}