using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Visometry.VisionLib.SDK.ARFoundation
{
    /**
     *  @ingroup ARFoundation
     */
    internal class MirroringDataStore : IDisposable
    {
        public IntPtr input = IntPtr.Zero;
        public IntPtr output = IntPtr.Zero;
        public int width = 0;
        public int height = 0;

        private bool disposed = false;

        public unsafe IntPtr MirrorData()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLMirroringDataStore");
            }
            int bytesPerLine = this.width * 4;
            for (int i = 0; i < this.height; i++)
            {
                var source = IntPtr.Add(this.input, (this.height - i - 1) * bytesPerLine)
                    .ToPointer();
                var destination = IntPtr.Add(this.output, i * bytesPerLine).ToPointer();
                Buffer.MemoryCopy(source, destination, bytesPerLine, bytesPerLine);
            }
            return this.output;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Allocate(int newWidth, int newHeight)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("VLMirroringDataStore");
            }
            if (this.output == IntPtr.Zero || this.input == IntPtr.Zero || this.width != newWidth ||
                this.height != newHeight)
            {
                this.width = newWidth;
                this.height = newHeight;
                Marshal.FreeHGlobal(this.input);
                Marshal.FreeHGlobal(this.output);
                this.input = Marshal.AllocHGlobal(GetSize());
                this.output = Marshal.AllocHGlobal(GetSize());
            }
        }

        public int GetSize()
        {
            return this.width * this.height * 4;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                Marshal.FreeHGlobal(this.input);
                Marshal.FreeHGlobal(this.output);
            }
            this.disposed = true;
        }
    }
}
