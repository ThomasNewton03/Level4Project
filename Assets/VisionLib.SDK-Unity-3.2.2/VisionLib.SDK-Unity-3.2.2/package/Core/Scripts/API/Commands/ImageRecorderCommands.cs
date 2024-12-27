using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the image recorder.
    /// </summary>
    /// @ingroup API
    public class ImageRecorderCommands
    {
        /// <summary>
        ///  Starts/Resumes recording
        /// </summary>
        public static async Task StartAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("start"));
        }

        /// <summary>
        ///  Pauses recording
        /// </summary>
        public static async Task PauseAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("stop"));
        }

        /// <summary>
        ///  Restarts recording.
        ///  According to `recordToNewDir` (set in vl-file) either starts recording to a new folder
        ///  (true) or overwrites the existing images (false).
        /// </summary>
        public static async Task ResetAsync(Worker worker)
        {
            await worker.PushCommandAsync(new WorkerCommands.CommandBase("reset"));
        }
    }
}
