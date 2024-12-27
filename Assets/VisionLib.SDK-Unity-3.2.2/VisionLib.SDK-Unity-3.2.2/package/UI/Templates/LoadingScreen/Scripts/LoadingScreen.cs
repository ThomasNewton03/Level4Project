using UnityEngine;
using Visometry.VisionLib.SDK.Core;

namespace Visometry.VisionLib.SDK.UI
{
    /// <summary>
    ///  Implements a loading screen.
    /// </summary>
    // @ingroup UI
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("VisionLib/UI/Loading Screen")]
    [HelpURL(DocumentationLink.APIReferenceURI.UI + "loading_screen.html")]
    public class LoadingScreen : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Animator animator;
        private int loadingHash = Animator.StringToHash("loading");

        public void Show()
        {
            this.rectTransform.SetAsLastSibling();
            this.animator.SetBool(loadingHash, true);
        }

        public void Hide()
        {
            this.animator.SetBool(loadingHash, false);
        }

        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
            this.animator = this.GetComponent<Animator>();
            this.animator.SetBool(loadingHash, false);
        }

        private void Start()
        {
            TrackingManager.OnTrackerInitializing += Show;
            TrackingManager.OnTrackerInitialized += Hide;

            // It's possible to stop the tracking, before the 'initialized' event
            // was emitted. Therefore we also listen to the 'stop' event.
            TrackingManager.OnTrackerStopped += Hide;
        }

        private void OnDestroy()
        {
            TrackingManager.OnTrackerInitializing -= Show;
            TrackingManager.OnTrackerInitialized -= Hide;
            TrackingManager.OnTrackerStopped -= Hide;
        }
    }
}
