using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    public class UEyeCameraColorModeParameter
    {
        public enum ColorMode
        {
            BGRA = 0,
            BGR = 1,
            Monochrome = 6,
            BayerFormat = 11,
            RGBA = 128,
            RGB = 129
        }

        public async Task<WorkerCommands.CommandWarnings> SetAsync(ColorMode colorMode)
        {
            return await UEyeCameraParameter.SetAttributeAsync(
                "imageSource.colorMode",
                ((int) colorMode).ToString());
        }
    }
}
