using System.Threading.Tasks;
using System;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the BarCode Reader.
    /// </summary>
    /// @ingroup API
    public static class BarCodeReaderCommands
    {
        [Serializable]
        public class BarCodeResult
        {
            public string value;
            public string format;
            public bool valid;
            public int framesSinceRecognition;

            public override string ToString()
            {
                return "{\"" + value + "\" " + format + " " + valid + " " + framesSinceRecognition +
                       "}";
            }
        }

        /// <summary>
        /// Receives the BarCodeResult of the BarCodeReader from the last frame.
        /// </summary>
        public static async Task<BarCodeResult> GetBarCodeResultAsync(Native.Worker worker)
        {
            return await worker.PushCommandAsync<BarCodeResult>(
                new WorkerCommands.CommandBase("getBarCodeResult"));
        }

        /// <summary>
        /// Sets the region of interest for the BarCodeReader in relative image coordinates. The
        /// BarCodeReader will only look into this region for possible codes.   
        /// </summary>
        internal static async Task SetRegionOfInterest(
            Native.Worker worker,
            BarCodeReader.RegionOfInterest regionOfInterest)
        {
            await worker.PushCommandAsync(new SetRegionOfInterestCmd(regionOfInterest));
        }

        [Serializable]
        private class SetRegionOfInterestCmd : WorkerCommands.CommandBase
        {
            public BarCodeReader.RegionOfInterest param = new BarCodeReader.RegionOfInterest();

            public SetRegionOfInterestCmd(BarCodeReader.RegionOfInterest regionOfInterest)
                : base("setROI")
            {
                this.param = regionOfInterest;
            }
        }
    }
}
