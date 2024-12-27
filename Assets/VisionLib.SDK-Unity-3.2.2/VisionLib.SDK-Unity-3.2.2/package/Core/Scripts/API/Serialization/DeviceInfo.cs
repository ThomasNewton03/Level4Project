using System;
using System.Linq;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  DeviceInfo stores information about the system and available cameras.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class DeviceInfo
    {
        /// <summary>
        ///  TrackingIssue stores an issue when tracking or when initialized
        /// </summary>
        [Serializable]
        public class Camera
        {
            /// <summary>
            ///  Format stores available formats of the camera device
            /// </summary>
            [Serializable]
            public class Format
            {
                /// <summary>
                ///  number of pixels in horizontal direction
                /// </summary>
                /// <remarks>
                ///  default is 640 (VGA)
                /// </remarks>
                public int width;

                /// <summary>
                ///  number of pixels in vertical direction
                /// </summary>
                /// <remarks>
                ///  default is 480 (VGA)
                /// </remarks>
                public int height;

                /// <summary>
                ///  compression format (e.g. YUY2)
                /// </summary>
                public string compression;

                public override string ToString()
                {
                    return this.width + "x" + this.height + "x" + this.compression;
                }
            }

            /// <summary>Refers to a specific camera of the visionLib SDK.</summary>
            /// <remarks>
            ///  You should use it for direct referring to a camera in some cases.
            /// </remarks>
            public string deviceID;

            /// <summary>Refers to an internal reference used for identifying the camera in the
            /// system.</summary> <remarks>
            ///  This is only for internal use.
            /// </remarks>
            public string internalID;

            /// <summary>A human readable information of the device itself.</summary>
            /// <remarks>
            ///  You might use this in order to present this to the user of your software.
            /// </remarks>
            public string cameraName;

            /// <summary>A hint for describing the position of the camera.</summary>
            /// <remarks>
            ///  If available, the value can be "front", "back", "unknown".
            /// </remarks>
            public string position;

            /// <summary>A hint for describing the type of the camera.</summary>
            /// <remarks>
            ///  If available, the value can be
            /// "BuiltInWideAngleCamera", "BuiltInTelephotoCamera", "BuiltInDualCamera",
            /// "BuiltInUltraWideCamera", "BuiltInDualWideCamera", "BuiltInTripleCamera" or
            /// "External"
            public string deviceType;

            /// <summary>A hint for describing the best resolution used for the camera.</summary>
            /// <remarks>
            ///  This might be empty, depending on the OS.
            /// </remarks>
            public string prefRes;

            /// <summary>
            ///  Array holding the available formats: image resolution and compression type.
            /// </summary>
            public Format[] availableFormats;

            /// <summary>States whether the camera supports the acquisition of depth data.</summary>
            public bool supportsDepthData;

            /// <summary>States whether the camera supports the acquisition of smoothed depth
            /// data.</summary>
            public bool supportsSmoothedDepthData;

            public override string ToString()
            {
                var deviceInfoString = this.cameraName + "\n\ndeviceID\n    " + this.deviceID;

                if (this.availableFormats.Length > 0)
                {
                    deviceInfoString += "\n\navailable formats\n" + String.Join(
                        "\n",
                        this.availableFormats.Select(format => "    " + format.ToString()));
                }

                return deviceInfoString;
            }
        }

        /// <summary>
        ///  Array holding the available camera devices.
        /// </summary>
        public Camera[] availableCameras;
    }
}
