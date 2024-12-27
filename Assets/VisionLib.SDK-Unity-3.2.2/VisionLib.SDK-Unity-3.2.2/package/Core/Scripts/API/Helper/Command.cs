using AOT;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    internal class Command
    {   
        private readonly TaskCompletionSource<string> t = new();
        private GCHandle gcHandle;

        private Command(WorkerCommands.CommandBase cmd, Func<WorkerCommands.CommandBase, Worker.JsonStringCallback, IntPtr, bool> commandExecutor)
        {
            this.gcHandle = GCHandle.Alloc(this);
            if (!commandExecutor(cmd, Command.commandCallbackDelegate, GCHandle.ToIntPtr(this.gcHandle)))
            {
                throw new InvalidOperationException("Could not send command.");
            }
        }

        private Task<string> GetTask()
        {
            return this.t.Task;
        }

        private static Command GetInstance(IntPtr clientData)
        {
            return (Command) GCHandle.FromIntPtr(clientData).Target;
        }

        [MonoPInvokeCallback(typeof(Worker.JsonStringCallback))]
        private static void DispatchCallback(string errorJson, string resultJson, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).Callback(errorJson, resultJson);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static readonly Worker.JsonStringCallback commandCallbackDelegate =
            new Worker.JsonStringCallback(DispatchCallback);

        private void Callback(string errorJson, string resultJson)
        {
            try
            {
                if (errorJson != null)
                {
                    var error = JsonHelper.FromJson<WorkerCommands.CommandError>(errorJson);
                    if (error.IsCanceled())
                    {
                        LogHelper.LogDebug(
                            "'" + error.commandName +
                            "' has been canceled because the tracker has been stopped or destroyed.");
                        this.t.SetCanceled();
                    }
                    else
                    {
                        this.t.SetException(error);
                    }
                }
                else
                {
                    this.t.SetResult(resultJson);
                }
            }
            catch (Exception e)
            {
                this.t.TrySetException(e);
            }
            finally
            {
                this.gcHandle.Free();
            }
        }

        public static Task<string> ExecuteAsync(Worker worker, WorkerCommands.CommandBase cmd)
        {
            return new Command(cmd, worker.PushCommand).GetTask();
        }
        
        public static string ExecuteSync(Worker worker, WorkerCommands.CommandBase cmd)
        {
            try
            {

                var task = new Command(cmd, worker.ProcessCommand).GetTask();
                return task.Result;
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Count == 1)
                {
                    throw e.InnerExceptions[0];
                }
                throw;
            }
        }
    }
}
