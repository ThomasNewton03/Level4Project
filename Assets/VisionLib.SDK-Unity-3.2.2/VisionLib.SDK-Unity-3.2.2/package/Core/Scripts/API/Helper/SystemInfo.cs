using System;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  SystemInfo enables access to system information
    /// </summary>
    /// @ingroup API
    [Serializable]
    public static class SystemInfo
    {
        public static string GetVLSDKVersion()
        {
            return VLSDK.GetVersionString();
        }

        public static string GetDetailedVLSDKVersion()
        {
            return "v" + VLSDK.GetVersionString() + " (" + VLSDK.GetVersionTimestampString() +
                   ", " + VLSDK.GetVersionHashString() + ")";
        }

        public static string GetHostID()
        {
            return VLSDK.GetHostID();
        }
    }
}
