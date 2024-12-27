using System;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API.Native;
using static Visometry.VisionLib.SDK.Core.API.WorkerCommands;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the UEye camera.
    /// </summary>
    /// @ingroup API
    public class UEyeCameraCommands
    {
        /// <summary>
        ///  Load uEye camera settings from internal memory.
        /// </summary>
        public static async Task LoadParametersFromEEPROMAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("imageSource.loadParametersFromEEPROM"));
        }

        /// <summary>
        ///  Save uEye camera settings to internal memory.
        /// </summary>
        public static async Task SaveParametersToEEPROMAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("imageSource.saveParametersToEEPROM"));
        }

        /// <summary>
        ///  Load uEye camera settings from the given file path.
        /// </summary>
        public static async Task LoadParametersFromFileAsync(Worker worker, string path)
        {
            await worker.PushCommandAsync(new LoadParametersFromFileCommand(path));
        }

        /// <summary>
        ///  Save uEye camera settings to the given file path.
        /// </summary>
        public static async Task SaveParametersToFileAsync(Worker worker, string path)
        {
            await worker.PushCommandAsync(new SaveParametersToFileCommand(path));
        }

        /// <summary>
        ///  Reset uEye camera parameters to default.
        /// </summary>
        public static async Task ResetParametersToDefaultAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("imageSource.resetParametersToDefault"));
        }

        [Serializable]
        private class SaveParametersToFileCommand : WorkerCommands.CommandBase
        {
            [Serializable]
            public class Param
            {
                public string uri;
            }

            public Param param = new Param();

            public SaveParametersToFileCommand(string uri)
                : base("imageSource.saveParametersToFile")
            {
                this.param.uri = uri;
            }
        }

        [Serializable]
        private class LoadParametersFromFileCommand : WorkerCommands.CommandBase
        {
            [Serializable]
            public class Param
            {
                public string uri;
            }

            public Param param = new Param();

            public LoadParametersFromFileCommand(string uri)
                : base("imageSource.loadParametersFromFile")
            {
                this.param.uri = uri;
            }
        }
    }
}
