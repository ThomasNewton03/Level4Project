using System.Threading.Tasks;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public class SingletonTaskExecutor
    {
        public delegate Task TaskProducer();

        public SingletonTaskExecutor(TaskProducer t, MonoBehaviour caller = null)
        {
            this.taskProducer = t;
            this.caller = caller;
        }

        private async Task TryExecuteAsync()
        {
            if (this.running)
            {
                return;
            }
            this.running = true;
            try
            {
                await this.taskProducer();
            }
            finally
            {
                this.running = false;
            }
        }

        /// <summary>
        /// Executes the given task if it isn't already running.
        /// After the current task is finished calling this function will execute the command again.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void TryExecute()
        {
            TrackingManager.CatchCommandErrors(TryExecuteAsync(), this.caller);
        }

        private MonoBehaviour caller;
        private TaskProducer taskProducer;
        private bool running = false;
    }
}
