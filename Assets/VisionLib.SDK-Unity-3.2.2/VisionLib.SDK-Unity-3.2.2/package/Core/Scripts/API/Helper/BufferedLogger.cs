using System;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    public class BufferedLogger : IDisposable
    {
        private Logger logger;
        private bool disposed;

        public BufferedLogger(VLSDK.LogLevel logLevel = VLSDK.LogLevel.Warning)
        {
            this.logger = new Logger();
            this.logger.EnableLogBuffer();
            this.logger.SetLogLevel(logLevel);
        }

        public void SetLogLevel(VLSDK.LogLevel logLevel)
        {
            this.logger.SetLogLevel(logLevel);
        }

        public void FlushLogBuffer()
        {
            this.logger.FlushLogBuffer();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.logger != null)
                {
                    this.logger.FlushLogBuffer();
                    this.logger.Dispose();
                    this.logger = null;
                }
            }
            this.disposed = true;
        }

        ~BufferedLogger()
        {
            Dispose(false);
        }
    }
}
