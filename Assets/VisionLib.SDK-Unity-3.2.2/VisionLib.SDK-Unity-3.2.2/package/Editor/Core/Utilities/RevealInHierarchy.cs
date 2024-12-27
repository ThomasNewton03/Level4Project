using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public static class RevealInHierarchy
    {
        private static readonly ButtonParameters defaultButtonParameters = new ButtonParameters()
        {
            label = "Highlight in hierarchy",
            labelTooltip = "Reveal game object in hierarchy.",
            buttonIcon = GUIHelper.Icons.SearchIcon,
        };

        public static void DrawButton(
            string customLabel,
            GameObject revealableObject,
            string labelToolTip = null)
        {
            var buttonParameters =
                new ButtonParameters(RevealInHierarchy.defaultButtonParameters)
                {
                    label = customLabel,
                    labelTooltip = string.IsNullOrEmpty(labelToolTip)
                        ? RevealInHierarchy.defaultButtonParameters.labelTooltip
                        : labelToolTip
                };

            if (ButtonParameters.ButtonWasClicked(buttonParameters))
            {
                EditorGUIUtility.PingObject(revealableObject);
            }
        }
    }
}
