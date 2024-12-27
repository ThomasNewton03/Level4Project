using UnityEngine;
using UnityEngine.UI;
using Visometry.VisionLib.SDK.Core;

namespace Visometry.VisionLib.SDK.UI
{
    // @ingroup UI
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("VisionLib/UI/Extendible Tracking Toggle")]
    [HelpURL(DocumentationLink.APIReferenceURI.UI + "extendible_tracking_toggle.html")]
    public class ExtendibleTrackingToggle : MonoBehaviour
    {
        public TrackingConfiguration trackingConfiguration;
        private Toggle toggle;

        private void Awake()
        {
            this.toggle = GetComponent<Toggle>();
        }

        private void Update()
        {
            if (this.trackingConfiguration == null)
            {
                return;
            }

            this.toggle.isOn = this.trackingConfiguration.extendTrackingWithSLAM;
        }
    }
}
