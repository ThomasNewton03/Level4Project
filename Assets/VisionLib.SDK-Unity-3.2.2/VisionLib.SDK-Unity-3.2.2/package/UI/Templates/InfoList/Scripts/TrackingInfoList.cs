using UnityEngine;
using UnityEngine.UI;
using Visometry.DesignSystem;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.UI
{
    /// <summary>
    /// The TrackingInfoList logs tracking information to the scene UI.
    /// </summary>
    // @ingroup UI
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("VisionLib/UI/Tracking Info List")]
    [HelpURL(DocumentationLink.APIReferenceURI.UI + "tracking_info_list.html")]
    public class TrackingInfoList : UIDisplay
    {
        private Text text;
        private Image image;

        private string trackingStates = "";
        private string performanceInfo = "";

        private void Awake()
        {
            this.text = GetComponentInChildren<Text>(true);
            this.image = GetComponent<Image>();
            this.image.enabled = this.enabled;
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackingStates += StoreTrackingStates;
            TrackingManager.OnPerformanceInfo += StorePerformanceInfo;

            this.image.enabled = true;
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackingStates -= StoreTrackingStates;
            TrackingManager.OnPerformanceInfo -= StorePerformanceInfo;

            this.image.enabled = false;
            this.text.text = "";
        }

        private void Update()
        {
            DrawTrackingInfoOnTextField();
        }

        public static TrackingInfoList Instantiate()
        {
            return Instantiate(Resources.Load<TrackingInfoList>("VLTrackingInfoList"));
        }

        /// <summary>
        /// Sets text of trackingInfo TextComponent
        /// to display the trackingStates and performanceInfo.
        /// </summary>
        private void DrawTrackingInfoOnTextField()
        {
            this.text.text = this.performanceInfo + "\n\n" + this.trackingStates;
        }

        void StorePerformanceInfo(PerformanceInfo info)
        {
            this.performanceInfo = "ProcessingTime: " + info.processingTime;
        }

        void StoreTrackingStates(TrackingState state)
        {
            this.trackingStates = state.ToDisplayString();
        }
    }
}
