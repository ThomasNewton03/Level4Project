using System;
using System.Collections.Generic;

namespace Visometry.VisionLib.SDK.Core
{
    internal class FrameBuffer<FrameType> : IDisposable where FrameType : class, IDisposable
    {
        private List<FrameType> frames = new List<FrameType>();
        private const int maxSize = 2;
        private bool disposed = false;
        private object framesLock = new object();

        public void Push(FrameType frame)
        {
            var frameToDispose = PushAndGetOverFlow(frame);
            if (frameToDispose != null)
            {
                frameToDispose.Dispose();
            }
        }

        public FrameType PushAndGetOverFlow(FrameType frame)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("FrameBuffer");
            }
            FrameType overflow = null;
            lock (this.framesLock)
            {
                if (this.frames.Count == maxSize)
                {
                    overflow = this.frames[0];
                    this.frames.RemoveAt(0);
                }
                this.frames.Add(frame);
            }
            return overflow;
        }

        public FrameType Pop()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("FrameBuffer");
            }
            FrameType fetchedFrame = null;
            lock (this.framesLock)
            {
                if (this.frames.Count > 0)
                {
                    fetchedFrame = this.frames[0];
                    this.frames.RemoveAt(0);
                }
            }
            return fetchedFrame;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            lock (this.framesLock)
            {
                foreach (var frame in this.frames)
                {
                    frame.Dispose();
                }
                this.frames.Clear();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }
            if (disposing)
            {
                Clear();
            }
            this.disposed = true;
        }
    }
}

