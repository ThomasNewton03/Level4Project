using System;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    /// CameraCalibrationResult stores the result of a calibration process.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class CameraCalibrationResult
    {
        /// <summary>
        ///  CameraIntrinsics stores the cameras intrinsic parameter and quality
        ///  values of the calibration process.
        /// </summary>
        [Serializable]
        public class CameraIntrinsics
        {
            public int width;
            public int height;

            public float fx;
            public float fy;
            public float cx;
            public float cy;

            public float k1;
            public float k2;
            public float k3;
            public float k4;
            public float k5;
            public float s;

            public float calibrationError;
            public string quality;
        }

        public string type;
        public int version;

        public float timestamp;
        public string organization;

        public string deviceID;
        public string cameraName;

        public bool calibrated;

        public CameraIntrinsics intrinsics;
        public CameraIntrinsics intrinsicsDist;

        public string[] alternativeDeviceIDs;
    }
}
