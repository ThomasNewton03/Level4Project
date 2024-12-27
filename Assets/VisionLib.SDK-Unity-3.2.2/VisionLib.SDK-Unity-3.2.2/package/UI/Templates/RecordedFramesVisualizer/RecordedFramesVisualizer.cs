using UnityEngine;
using UnityEngine.UI;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Examples
{
    /**
     *  @ingroup Examples
     */
    [AddComponentMenu("VisionLib/Examples/Recorded Frames Visualizer")]
    [HelpURL(DocumentationLink.APIReferenceURI.Examples + "recorded_frames_visualizer.html")]
    public class RecordedFramesVisualizer : MonoBehaviour
    {
        public Text textComponent;
        private string state = "Stopped";
        private int numberOfFrames = 0;

        void Update()
        {
            if (this.textComponent != null)
            {
                this.textComponent.text = this.state + " @Frame (" + this.numberOfFrames + ")";
            }
        }

        void StoreTrackingStates(TrackingState state)
        {
            if (state.objects.Length > 0)
            {
                var obj = state.objects[0];
                this.numberOfFrames = obj._NumberOfTemplates;
                this.state = obj.state;
            }
        }

        void OnEnable()
        {
            TrackingManager.OnTrackingStates += StoreTrackingStates;
        }

        void OnDisable()
        {
            TrackingManager.OnTrackingStates -= StoreTrackingStates;
        }

        public void OnStop()
        {
            this.state = "Stopped";
        }
    }
}
