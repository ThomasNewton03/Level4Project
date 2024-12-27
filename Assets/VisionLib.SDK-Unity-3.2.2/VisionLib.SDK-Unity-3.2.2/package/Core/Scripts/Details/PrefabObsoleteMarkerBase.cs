using System.Collections.Generic;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public abstract class PrefabObsoleteMarkerBase : MonoBehaviour, ISceneValidationCheck
    {
        [SerializeField]
        private List<LinkButtonContent> docsLinkDescriptions = new();

        [SerializeField]
        protected string obsoletePrefabName = null;
        [SerializeField]
        protected bool hasReplacement = true;
        [SerializeField]
        protected string replacementPrefabName = null;

        private void Awake()
        {
            LogHelper.LogWarning(GetWarningMessage(), this);
        }

        public string GetWarningMessage()
        {
            if (IsMissingNames())
            {
                return "Obsolete prefab usage.";
            }
            return $"The prefab \"{this.obsoletePrefabName}\" is obsolete. " + (this.hasReplacement
                ? $"Use {this.replacementPrefabName} instead."
                : "This prefab will be removed. Do not use it going forward.");
        }

        public bool IsMissingNames()
        {
            return string.IsNullOrEmpty(this.obsoletePrefabName) || (this.hasReplacement &&
                string.IsNullOrEmpty(this.obsoletePrefabName));
        }
#if UNITY_EDITOR
        public virtual List<SetupIssue> GetSceneIssues()
        {
            var message = "Usage of obsolete prefab" +
                          (!string.IsNullOrEmpty(this.obsoletePrefabName)
                              ? $" \"{this.obsoletePrefabName}\"."
                              : ".") +
                          (this.hasReplacement && !string.IsNullOrEmpty(this.replacementPrefabName)
                              ? $"\nReplace the prefab with \"{this.replacementPrefabName}\"."
                              : "\nRemove the prefab.");
            // The presence of this script itself is a setup issue
            return new List<SetupIssue>
            {
                new SetupIssue(
                    message,
                    "If in doubt about how to proceed, " +
                    "see the VisionLib Unity SDK changelogs for help or contact VisionLib support.",
                    SetupIssue.IssueType.Warning,
                    this.gameObject)
            };
        }
#endif
    }
}
