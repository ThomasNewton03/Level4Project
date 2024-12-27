using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    public class EventWrapper<T>
    {
        public delegate void TAction(T item);
        public event TAction OnUpdate;

        public bool IsUsed()
        {
            return OnUpdate != null;
        }

        public void Notify(T value)
        {
            if (IsUsed())
            {
                OnUpdate(value);
            }
        }
    }

    /// @ingroup API
    internal class AnchorHandler : IDisposable
    {
        private EventWrapper<SimilarityTransform> observable;
        private string anchorName;
        private Worker worker;

        private readonly GCHandle gcHandle;
        private bool disposed = false;

        public AnchorHandler(
            Worker worker,
            string anchorName,
            EventWrapper<SimilarityTransform> observable)
        {
            this.worker = worker;
            this.anchorName = anchorName;
            this.observable = observable;
            this.gcHandle = GCHandle.Alloc(this);
            if (this.worker == null || !this.worker.AddWorldFromAnchorTransformListener(
                    this.anchorName,
                    AnchorHandler.dispatchSimilarityTransformCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle)))
            {
                throw new InvalidOperationException("AddWorldFromAnchorTransformListener");
            }
        }

        ~AnchorHandler()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (!this.worker.GetDisposed())
            {
                this.worker.RemoveWorldFromAnchorTransformListener(
                    this.anchorName,
                    AnchorHandler.dispatchSimilarityTransformCallbackDelegate,
                    GCHandle.ToIntPtr(this.gcHandle));
            }

            if (disposing)
            {
                // Dispose managed resources (those that implement IDisposable)
            }

            this.gcHandle.Free();
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [MonoPInvokeCallback(typeof(Worker.SimilarityTransformWrapperCallback))]
        private static void DispatchSimilarityTransformCallback(IntPtr handle, IntPtr clientData)
        {
            try
            {
                GetInstance(clientData).SimilarityTransformHandler(handle);
            }
            catch (Exception e) // Catch all exceptions, because this is a callback
                // invoked from native code
            {
                LogHelper.LogException(e);
            }
        }

        private static readonly Worker.SimilarityTransformWrapperCallback
            dispatchSimilarityTransformCallbackDelegate =
                new Worker.SimilarityTransformWrapperCallback(DispatchSimilarityTransformCallback);

        private void SimilarityTransformHandler(IntPtr handle)
        {
            var similarityTransform = new SimilarityTransform(handle, false);
            this.observable.Notify(similarityTransform);
            similarityTransform.Dispose();
        }

        private static AnchorHandler GetInstance(IntPtr clientData)
        {
            return (AnchorHandler) GCHandle.FromIntPtr(clientData).Target;
        }
    }
}
