using AOT;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    internal class AsyncBinaryCommand
    {
        internal class LabeledBinaryData : IDisposable
        {
            private bool disposed = false;
            public string jsonDescription = "";
            public IntPtr data = IntPtr.Zero;
            public UInt32 dataSize = 0;

            ~LabeledBinaryData()
            {
                // The finalizer was called implicitly from the garbage collector
                this.Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                // Prevent multiple calls to Dispose
                if (this.disposed)
                {
                    return;
                }
                VLSDK.ReleaseMemory(this.data);
                this.disposed = true;
            }

            public void Dispose()
            {
                Dispose(true); // Dispose was explicitly called by the user
                GC.SuppressFinalize(this);
            }

            public LabeledBinaryData(string jsonDescription, IntPtr data, UInt32 dataSize)
            {
                this.jsonDescription = jsonDescription;
                this.data = data;
                this.dataSize = dataSize;
            }
        }

        private TaskCompletionSource<LabeledBinaryData> t =
            new TaskCompletionSource<LabeledBinaryData>();
        private GCHandle gcHandle;

        private AsyncBinaryCommand(Worker worker, WorkerCommands.JsonAndBinaryCommandBase cmd)
        {
            this.gcHandle = GCHandle.Alloc(this);
            if (!worker.PushCommand(
                cmd,
                binaryCommandCallbackDelegate,
                GCHandle.ToIntPtr(this.gcHandle)))
            {
                throw new InvalidOperationException("Could not send command.");
            }
        }

        private Task<LabeledBinaryData> GetTask()
        {
            return t.Task;
        }

        private static AsyncBinaryCommand GetInstance(IntPtr clientData)
        {
            return (AsyncBinaryCommand) GCHandle.FromIntPtr(clientData).Target;
        }

        [MonoPInvokeCallback(typeof(Worker.JsonStringAndBinaryCallback))]
        private static void DispatchBinaryCallback(
            string errorJson,
            string resultJson,
            IntPtr data,
            System.UInt32 dataSize,
            IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).BinaryCallback(
                    errorJson,
                    new LabeledBinaryData(resultJson, data, dataSize));
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static Worker.JsonStringAndBinaryCallback binaryCommandCallbackDelegate =
            new Worker.JsonStringAndBinaryCallback(DispatchBinaryCallback);

        private void BinaryCallback(string errorJson, LabeledBinaryData result)
        {
            try
            {
                if (errorJson != null)
                {
                    var error = JsonHelper.FromJson<WorkerCommands.CommandError>(errorJson);
                    if (error.IsCanceled())
                    {
                        t.SetCanceled();
                    }
                    else
                    {
                        t.SetException(error);
                    }
                }
                else
                {
                    t.SetResult(result);
                }
            }
            catch (Exception e)
            {
                if (e != null)
                {
                    t.TrySetException(e);
                }
            }
            finally
            {
                this.gcHandle.Free();
            }
        }

        public static Task<LabeledBinaryData> Execute(
            Worker worker,
            WorkerCommands.JsonAndBinaryCommandBase cmd)
        {
            return (new AsyncBinaryCommand(worker, cmd)).GetTask();
        }
    }
}
