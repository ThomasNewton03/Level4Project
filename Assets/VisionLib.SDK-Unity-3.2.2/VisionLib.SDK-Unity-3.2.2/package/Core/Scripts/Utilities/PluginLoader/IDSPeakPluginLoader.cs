using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "i_d_s_peak_plugin_loader.html")]
    [AddComponentMenu("VisionLib/Core/IDS Peak Plugin Loader")]
    public class IDSPeakPluginLoader : MonoBehaviour
    {
        void Start()
        {
            if (!TrackingManager.Instance.Worker.LoadPlugin("VideoIDSPeak"))
            {
                LogHelper.LogError(
                    "Failed to load IDS Peak plugin. If you do not want to use the IDS Peak plugin, please deactivate the IDSPeakPluginLoader component.",
                    this);
            }
        }
    }
}
