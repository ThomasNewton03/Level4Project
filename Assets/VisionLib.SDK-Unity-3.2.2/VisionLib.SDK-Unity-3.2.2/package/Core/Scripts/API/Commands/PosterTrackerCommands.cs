using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API.Native;
using static Visometry.VisionLib.SDK.Core.API.WorkerCommands;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the poster tracker.
    /// </summary>
    /// @ingroup API
    public static class PosterTrackerCommands
    {
        /// <summary>
        ///  Resets the tracking and all keyframes.
        /// </summary>
        public static async Task ResetHardAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("resetHard"));
        }
    }
}
