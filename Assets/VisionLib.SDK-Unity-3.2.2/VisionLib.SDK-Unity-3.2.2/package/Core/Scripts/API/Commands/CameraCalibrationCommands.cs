using System;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands to communicate with a camera calibration pipeline.
    /// </summary>
    /// @ingroup API
    public static class CameraCalibrationCommands
    {
        /// <summary>
        /// Starts the collection of frames.
        /// </summary>
        public static async Task RunAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("run"));
        }

        public static async Task CancelAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("cancel"));
        }

        public static async Task ResetAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("reset"));
        }

        public static async Task PauseAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("pause"));
        }

        public static async Task OptimizeAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("optimize"));
        }

        public static async Task<CameraCalibrationResult> GetResultsAsync(Worker worker)
        {
            var result =
                await worker.PushCommandAsync(new WorkerCommands.CommandBase("getResults"));
            return JsonHelper.FromJson<CameraCalibrationResult>(result);
        }

        public static async Task WriteCameraCalibrationAsync(
            Worker worker,
            string uri,
            CameraCalibrationResult calibration)
        {
            await worker.PushCommandAsync<CameraCalibrationResult>(
                new WriteCameraCalibrationCmd(uri, calibration));
        }

        [Serializable]
        private class WriteCameraCalibrationCmd : WorkerCommands.CommandBase
        {
            [Serializable]
            public class Param
            {
                public string uri;
                public CameraCalibrationResult calibration;
            }

            public Param param = new Param();

            public WriteCameraCalibrationCmd(string uri, CameraCalibrationResult calibration)
                : base("write")
            {
                this.param.uri = uri;
                this.param.calibration = calibration;
            }
        }
    }
}
