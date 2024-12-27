using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "u_eye_plugin_loader.html")]
    [AddComponentMenu("VisionLib/Core/UEye Plugin Loader")]
    public class UEyePluginLoader : MonoBehaviour
    {
        void Start()
        {
            if (!TrackingManager.Instance.Worker.LoadPlugin("VideoUEye"))
            {
                LogHelper.LogError(
                    "Failed to load uEye plugin. If you do not want to use the UEye plugin, please deactivate the UEyePluginLoader component.",
                    this);
            }
        }
    }
}
