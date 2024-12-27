using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Enables scene authors to analyze the quality of a texture as a VisionLib reference image.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "poster_analyzer.html")]
    [AddComponentMenu("VisionLib/Core/Poster Analyzer")]
    public class PosterAnalyzer : MonoBehaviour
    {
        /// <summary>
        /// Path of the license file relative to StreamingAssets, e.g. license.xml.
        /// </summary>
        [Tooltip("Path of the license file relative to StreamingAssets, e.g. license.xml.")]
        public LicenseFile licenseFile;
        /// <summary>
        /// Reference Image to analyze.
        /// </summary>
        [Tooltip("Reference image to analyze")]
        [HideInInspector]
        public Texture2D texture;

        public double GetPosterQuality()
        {
            if (this.texture == null)
            {
                return -1;
            }

            Worker worker = new Worker();
            worker.SetLicenseFilePath(licenseFile.path);
            Image vlImage = Image.CreateFromTexture(this.texture);

            return VLSDK.GetPosterQuality(vlImage);
        }
    }
}
