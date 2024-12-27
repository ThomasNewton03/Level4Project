using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.UI
{
    // @ingroup UI
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("VisionLib/UI/Extend Toggle Events")]
    [HelpURL(DocumentationLink.APIReferenceURI.UI + "extend_toggle_events.html")]
    public class ExtendToggleEvents : MonoBehaviour, ISceneValidationCheck
    {
        public UnityEngine.Events.UnityEvent onValueChangedToFalse;

        void Start()
        {
            GetComponent<Toggle>().onValueChanged.AddListener(
                value =>
                {
                    if (!value)
                    {
                        onValueChangedToFalse.Invoke();
                    }
                });
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.onValueChangedToFalse},
                this.gameObject);
        }
#endif
    }
}
