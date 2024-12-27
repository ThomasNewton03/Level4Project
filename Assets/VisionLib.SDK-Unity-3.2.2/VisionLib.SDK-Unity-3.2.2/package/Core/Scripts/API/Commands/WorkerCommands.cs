using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the Worker.
    /// </summary>
    /// @ingroup API
    public static class WorkerCommands
    {
        [Serializable]
        public class CommandBase
        {
            public string name;

            public CommandBase(string name)
            {
                this.name = name;
            }
        }

        public class JsonAndBinaryCommandBase
        {
            [Serializable]
            public class DescriptionBase
            {
                public string name;

                public DescriptionBase(string name)
                {
                    this.name = name;
                }
            }

            public string jsonString;
            public IntPtr binaryData;
            public UInt32 binaryDataSize;

            public JsonAndBinaryCommandBase(
                DescriptionBase commandDescription,
                IntPtr binaryData,
                UInt32 binaryDataSize)
            {
                this.jsonString = JsonHelper.ToJson(commandDescription);
                this.binaryData = binaryData;
                this.binaryDataSize = binaryDataSize;
            }
        }

        /// <summary>
        /// Warnings arising from a command
        /// </summary>
        [Serializable]
        public struct CommandWarnings
        {
            public Issue[] warnings;
        }

        public static CommandWarnings NoWarnings()
        {
            return new CommandWarnings();
        }

        public static CommandWarnings Concat(
            this CommandWarnings first,
            CommandWarnings additionalWarnings)
        {
            var allWarnings = first.warnings ?? Array.Empty<Issue>();
            if (additionalWarnings.warnings != null)
            {
                allWarnings = allWarnings.Concat(additionalWarnings.warnings).ToArray();
            }

            return new CommandWarnings {warnings = allWarnings};
        }

        public static async Task<CommandWarnings> AwaitAll(
            IEnumerable<Task<CommandWarnings>> tasksWithWarnings)
        {
            var results = await Task.WhenAll(tasksWithWarnings);
            var allWarnings = new List<Issue>();
            foreach (var result in results)
            {
                if (result.warnings != null)
                {
                    allWarnings.AddRange(result.warnings);
                }
            }
            return new CommandWarnings {warnings = allWarnings.ToArray()};
        }

        /// <summary>
        ///  Creates a tracker from a vl-file.
        /// </summary>
        public static async Task<TrackerInfo> CreateTrackerAsync(Worker worker, string trackingFile)
        {
            return await worker.PushCommandAsync<TrackerInfo>(new CreateTrackerCmd(trackingFile));
        }

        /// <summary>
        ///  Creates a tracker from a vl-string.
        /// </summary>
        public static async Task<TrackerInfo> CreateTrackerAsync(
            Worker worker,
            string trackingConfiguration,
            string fakeFileName)
        {
            return await worker.PushCommandAsync<TrackerInfo>(
                new CreateTrackerFromStringCmd(trackingConfiguration, fakeFileName));
        }

        /// <summary>
        ///  Sets the target number of frames per seconds of the tracking thread.
        /// </summary>
        public static async Task SetTargetFPSAsync(Worker worker, int targetFPS)
        {
            await worker.PushCommandAsync(new SetTargetFpsCmd(targetFPS));
        }

        /// <summary>
        ///  Gets the current value of a certain attribute.
        /// </summary>
        public static async Task<GetAttributeResult> GetAttributeAsync(
            Worker worker,
            string attributeName)
        {
            return await worker.PushCommandAsync<GetAttributeResult>(
                new GetAttributeCmd(attributeName));
        }

        public static async Task<T> GetAttributeAsync<T>(Worker worker, string attributeName)
        {
            var getAttributeResult = await GetAttributeAsync(worker, attributeName);
            return JsonHelper.ParseJsonValueFromBackendAs<T>(getAttributeResult.value);
        }

        /// <summary>
        ///  Sets the value of a certain attribute.
        /// </summary>
        public static async Task<CommandWarnings> SetAttributeAsync(
            Worker worker,
            string attributeName,
            string attributeValue)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetAttributeCmd(attributeName, attributeValue));
        }

        /// <summary>
        ///  Starts the tracking.
        /// </summary>
        public static async Task RunTrackingAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("runTracking"));
        }

        /// <summary>
        ///  Stops the tracking.
        /// </summary>
        public static async Task PauseTrackingAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("pauseTracking"));
        }

        /// <summary>
        ///  Set the device orientation.
        /// </summary>
        public static async Task SetDeviceOrientationAsync(Worker worker, int mode)
        {
            await worker.PushCommandAsync(new SetDeviceOrientationCmd(mode));
        }

        [Serializable]
        private class CreateTrackerCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string uri;
            }

            public Param param = new Param();

            public CreateTrackerCmd(string trackingFile)
                : base("createTracker")
            {
                this.param.uri = trackingFile;
            }
        }

        [Serializable]
        private class CreateTrackerFromStringCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string str;
                public string fakeFilename;
            }

            public Param param = new Param();

            public CreateTrackerFromStringCmd(string trackingConfiguration, string fakeFileName)
                : base("createTrackerFromString")
            {
                this.param.str = trackingConfiguration;
                this.param.fakeFilename = fakeFileName;
            }
        }

        [Serializable]
        private class SetTargetFpsCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public int targetFPS;
            }

            public Param param = new Param();

            public SetTargetFpsCmd(int fps)
                : base("setTargetFPS")
            {
                this.param.targetFPS = fps;
            }
        }

        [Serializable]
        private class GetAttributeCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string att;
            }

            public Param param = new Param();

            public GetAttributeCmd(string attributeName)
                : base("getAttribute")
            {
                this.param.att = attributeName;
            }
        }

        [Serializable]
        private class SetAttributeCmd : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string att;
                public string val;
            }

            public Param param = new Param();

            public SetAttributeCmd(string attributeName, string attributeValue)
                : base("setAttribute")
            {
                this.param.att = attributeName;
                this.param.val = attributeValue;
            }
        }

        [Serializable]
        private class SetDeviceOrientationCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public int mode;
            }

            public Param param = new Param();

            public SetDeviceOrientationCmd(int mode)
                : base("setDeviceOrientation")
            {
                this.param.mode = mode;
            }
        }

        [Serializable]
        internal class SetTimestampCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public double timestamp;
            }

            public Param param = new Param();

            public SetTimestampCmd(double timestamp)
                : base("setTimestamp")
            {
                this.param.timestamp = timestamp;
            }
        }

        // Return types

        /// <summary>
        ///  Error returned from Worker.JsonStringCallback.
        /// </summary>
        [Serializable]
        public class CommandError : Exception
        {
            public VLIssueCode errorCode;
            public string commandName;
            public string info;
            public string message;

            public bool IsCanceled()
            {
                return this.errorCode == VLIssueCode.ERROR_COMMAND_CANCELED;
            }

            public Issue GetIssue()
            {
                return new Issue
                {
                    commandName = this.commandName,
                    code = this.errorCode,
                    info = this.info,
                    message = this.message,
                    level = Issue.IssueType.Error
                };
            }

            public override string Message
            {
                get
                {
                    return $"{base.Message}\n{IssuesToEventsAdapter.GetIssueMessage(GetIssue())}";
                }
            }
        }

        /// <summary>
        ///  Result of GetAttributeCmd.
        /// </summary>
        [Serializable]
        public struct GetAttributeResult
        {
            public string value;
        }
    }
}
