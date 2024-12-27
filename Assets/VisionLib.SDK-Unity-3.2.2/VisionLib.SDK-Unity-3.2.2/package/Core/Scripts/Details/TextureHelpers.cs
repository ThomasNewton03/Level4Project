using System;
using System.Linq;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    ///     Provides helper functions for handling Unity's texture objects. 
    /// </summary>
    /// @ingroup Details
    public static class TextureHelpers
    {
        private static TextureFormat[] RGBAConversionSupportedFormats =
        {
            TextureFormat.RGBA32, TextureFormat.ARGB32, TextureFormat.BGRA32,
            TextureFormat.ETC_RGB4, TextureFormat.DXT5, TextureFormat.RGB24, TextureFormat.DXT1,
            TextureFormat.R8, TextureFormat.EAC_R, TextureFormat.BC4, TextureFormat.ASTC_6x6,
            TextureFormat.PVRTC_RGB4
        };

        public class UnsupportedTextureFormatException : Exception
        {
            public UnsupportedTextureFormatException(string message)
                : base(message) {}
        }

        /// <summary>
        ///     Creates an independent copy of the referenced texture (deep copy),
        ///     converted into the the RGBA32 texture format.
        /// </summary>
        /// <remarks>
        ///     Supported texture formats:
        ///     <para>
        ///         4-Channel: RGBA32, ARGB32, BGRA32, DXT5
        ///     </para>
        ///     <para>
        ///         3-Channel: RGB24, DXT1, ETC_RGB4, PVRTC_RGB4
        ///     </para>
        ///     <para>
        ///         Single-Channel: R8, BC4
        ///     </para>
        /// </remarks>
        /// <param name="texture"> Reference to the texture to be copied and converted.</param>
        /// @exception UnsupportedTextureFormatException
        public static Texture2D CreateRGBACopy(Texture2D texture)
        {
            ThrowIfIncorrectTextureFormat(
                texture.format,
                TextureHelpers.RGBAConversionSupportedFormats);
            return texture.format switch
            {
                TextureFormat.RGBA32 => CreateTexture2DCopy(texture),
                TextureFormat.ETC_RGB4 => ConvertRGBtoRGBA(texture),
                TextureFormat.PVRTC_RGB4 => ConvertRGBtoRGBA(texture),
                TextureFormat.ARGB32 => CreateTexture2DCopy(texture, TextureFormat.RGBA32),
                TextureFormat.BGRA32 => CreateTexture2DCopy(texture, TextureFormat.RGBA32),
                TextureFormat.R8 => ConvertGreyScaleToRGBA(texture),
                TextureFormat.EAC_R => ConvertGreyScaleToRGBA(texture),
                TextureFormat.RGB24 => ConvertRGBtoRGBA(texture),
                TextureFormat.DXT5 => CreateTexture2DCopy(texture, TextureFormat.RGBA32),
                TextureFormat.DXT1 => ConvertRGBtoRGBA(texture),
                TextureFormat.BC4 => ConvertGreyScaleToRGBA(texture),
                TextureFormat.ASTC_6x6 => ConvertRGBtoRGBA(texture),
                _ => null
            };
        }

        /// <summary>
        ///     Creates an independent copy of the referenced texture (deep copy), optionally
        ///     converted into target texture format. NOTE: This might fail if the input
        ///     and target texture format are not compatible.
        /// </summary>
        /// <param name="original">
        ///     Reference to the texture to be copied.
        /// </param>
        /// <param name="targetFormat">
        ///     Target texture format, default is the input texture's format.
        /// </param>
        private static Texture2D CreateTexture2DCopy(
            Texture2D original,
            TextureFormat? targetFormat = null)
        {
            var copy = new Texture2D(
                original.width,
                original.height,
                targetFormat ?? original.format,
                false);
            copy.SetPixels(original.GetPixels());
            copy.Apply();
            return copy;
        }

        /// <summary>
        ///     Creates an independent copy of the referenced greyscale texture (deep copy),
        ///     converted into the RGBA32 texture format.
        /// </summary>
        /// <param name="original">
        ///     Reference to the texture to be copied and converted.
        /// </param>
        /// <remarks>
        ///     Supported texture formats: R8, BC4
        /// </remarks>
        /// @exception UnsupportedTextureFormatException
        private static Texture2D ConvertGreyScaleToRGBA(Texture2D original)
        {
            ThrowIfIncorrectTextureFormat(
                original.format,
                new[] {TextureFormat.R8, TextureFormat.BC4, TextureFormat.EAC_R});
            return ConvertToRGBAWithDelegate(
                original,
                color => new Color(color.r, color.r, color.r, 1.0f));
        }

        /// <summary>
        ///     Creates an independent copy of the referenced RGB texture (deep copy),
        ///     converted into the RGBA32 texture format.
        /// </summary>
        /// <param name="original">
        ///     Reference to the texture to be copied and converted.
        /// </param>
        /// <remarks>
        ///     Supported texture formats: RGB24, DXT1, ETC_RGB4, ASTC_6x6, PVRTC_RGB4
        /// </remarks>
        /// @exception UnsupportedTextureFormatException
        private static Texture2D ConvertRGBtoRGBA(Texture2D original)
        {
            ThrowIfIncorrectTextureFormat(
                original.format,
                new[]
                {
                    TextureFormat.RGB24, TextureFormat.DXT1, TextureFormat.ETC_RGB4,
                    TextureFormat.ASTC_6x6, TextureFormat.PVRTC_RGB4
                });
            return ConvertToRGBAWithDelegate(
                original,
                color => new Color(color.r, color.g, color.b, 1.0f));
        }

        private static Texture2D ConvertToRGBAWithDelegate(
            Texture2D original,
            Func<Color, Color> convertColor)
        {
            var copy = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
            var colors = original.GetPixels();
            var modifiedColors = colors.Select(convertColor).ToArray();
            copy.SetPixels(modifiedColors);
            copy.Apply();
            return copy;
        }

        private static void ThrowIfIncorrectTextureFormat(
            TextureFormat actualFormat,
            TextureFormat[] expectedFormats)
        {
            if (!expectedFormats.Contains(actualFormat))
            {
                throw new UnsupportedTextureFormatException(
                    "Unsupported Image Format! Expected: " + string.Join(
                        " or ",
                        expectedFormats.Select(f => f.ToString())) + "; but was: " +
                    actualFormat.ToString());
            }
        }
    }
}
