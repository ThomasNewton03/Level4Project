using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /**
     *  @ingroup HoloLens
     */
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "holo_lens_instruction_panel.html")]
    [AddComponentMenu("VisionLib/HoloLens/Instruction Panel")]
    public class HoloLensInstructionPanel : MonoBehaviour, ISceneValidationCheck
    {
        public bool hideAfterTrackerInitialized = false;
        [FormerlySerializedAs("hideAutomatically")]
        public bool hideAfterTime = false;
        [OnlyShowIf("hideAfterTime", true)]
        public float hideAfterSeconds = 15f;
        public UnityEvent OnPanelHide = new UnityEvent();

        private IEnumerator hidePanelCoroutine;

        private void OnEnable()
        {
            if (this.hideAfterTrackerInitialized)
            {
                TrackingManager.OnTrackerInitialized += HidePanel;
            }
        }

        private void OnDisable()
        {
            if (this.hideAfterTrackerInitialized)
            {
                TrackingManager.OnTrackerInitialized -= HidePanel;
            }
        }

        private void Start()
        {
            if (this.hideAfterTime)
            {
                HidePanelAfterTime(this.hideAfterSeconds);
            }
        }

        private void HidePanelAfterTime(float seconds)
        {
            if (this.hidePanelCoroutine != null)
            {
                StopCoroutine(this.hidePanelCoroutine);
            }

            this.hidePanelCoroutine = HideAfterSeconds(seconds);
            StartCoroutine(this.hidePanelCoroutine);
        }

        private IEnumerator HideAfterSeconds(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            HidePanel();
        }

        private void HidePanel()
        {
            this.gameObject.SetActive(false);
            OnPanelHide?.Invoke();
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.OnPanelHide},
                this.gameObject);
        }
#endif
    }
}
