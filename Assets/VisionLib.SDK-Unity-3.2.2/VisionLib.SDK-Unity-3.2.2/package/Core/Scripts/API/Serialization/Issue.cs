using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  This class contains information about an issue raised by the native VisionLib SDK.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class Issue
    {
        public enum IssueType
        {
            Notification = 0,
            Warning = 1,
            Error = 2
        }

        /// <summary> The name of the command causing this issue. </summary>
        public string commandName;

        /// <summary>The machine readable info on the error/warning.</summary>
        /// <remarks>
        ///  This might be matter of change - but can help to understand the issue
        ///  Do not rely on the string except that it holds special information like the device
        ///  name etc..
        /// </remarks>
        public string info;

        /// <summary>The english human readable message on the error/warning.</summary>
        /// <remarks>
        ///  This might be matter of change - but can help to understand the issue
        ///  Do not rely on the string.
        /// </remarks>
        public string message;

        /// <summary>The unique code for the error</summary>
        /// <remarks>
        ///  For current error codes see \ref tracking-init-issues
        /// </remarks>
        public VLIssueCode code;

        /// <summary>The error level</summary>
        public IssueType level;

        /// <summary>
        /// Origin of the issue. May be null if not mentioned.
        /// </summary>
        public MonoBehaviour caller = null;
    }
}
