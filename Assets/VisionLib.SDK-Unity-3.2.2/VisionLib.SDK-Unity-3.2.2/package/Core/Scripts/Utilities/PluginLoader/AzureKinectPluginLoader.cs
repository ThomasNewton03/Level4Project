using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "azure_kinect_plugin_loader.html")]
    [AddComponentMenu("VisionLib/Core/Azure Kinect Plugin Loader")]
    public class AzureKinectPluginLoader : MonoBehaviour
    {
        void Start()
        {
            if (!TrackingManager.Instance.Worker.LoadPlugin("VideoAzureKinect"))
            {
                LogHelper.LogError("Failed to load Azure Kinect plugin");
            }
        }
    }
}
