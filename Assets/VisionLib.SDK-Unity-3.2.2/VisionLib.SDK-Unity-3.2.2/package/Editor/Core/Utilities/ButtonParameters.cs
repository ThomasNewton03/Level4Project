using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public class ButtonParameters
    {
        private static readonly GUIStyle defaultGUIStyle =  new(EditorStyles.miniButton)
        {
            fixedWidth = 35, fixedHeight = 20, alignment = TextAnchor.MiddleCenter
        };

        public string label;
        public string labelTooltip;
        public GUIHelper.Icons labelIcon = GUIHelper.Icons.None;
        [CanBeNull]
        public string buttonText;
        [CanBeNull]
        public string buttonTooltip;
        public GUIHelper.Icons buttonIcon = GUIHelper.Icons.None;
        public GUIStyle guiStyle = ButtonParameters.defaultGUIStyle;

        public ButtonParameters() {}

        public ButtonParameters(ButtonParameters template)
        {
            this.label = template.label;
            this.labelTooltip = template.labelTooltip;
            this.labelIcon = template.labelIcon;
            this.buttonText = template.buttonText;
            this.buttonIcon = template.buttonIcon;
            this.buttonTooltip = template.buttonTooltip;
            this.guiStyle = template.guiStyle;
        }

        public static bool ButtonWasClicked(ButtonParameters parameters)
        {
            using (new GUILayout.HorizontalScope())
            {
                var labelContent = GUIHelper.GenerateGUIContentWithIcon(
                    parameters.labelIcon,
                    parameters.labelTooltip,
                    parameters.label);

                EditorGUILayout.LabelField(labelContent, EditorStyles.wordWrappedLabel);

                var actualButtonTooltip = string.IsNullOrEmpty(parameters.buttonTooltip)
                    ? parameters.labelTooltip
                    : parameters.buttonTooltip;

                return GUILayout.Button(
                    GUIHelper.GenerateGUIContentWithIcon(
                        parameters.buttonIcon,
                        actualButtonTooltip,
                        " " + parameters.buttonText),
                    parameters.guiStyle);
            }
        }

        public static void DrawLinkButton(string label, string tooltip, string url)
        {
            if (LinkButtonClicked(label, tooltip))
            {
                Application.OpenURL(url);
            }
        }

        public static void DrawLinkButton(LinkButtonContent linkButtonDescription)
        {
            DrawLinkButton(
                linkButtonDescription.linkButtonLabel,
                linkButtonDescription.linkButtonTooltip,
                linkButtonDescription.linkURL);
        }

        private static bool LinkButtonClicked(string label, string tooltip)
        {
#if UNITY_2021_3_OR_NEWER
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15);
                return EditorGUILayout.LinkButton(new GUIContent(label, tooltip));
            }
#else
            var buttonParameters = new ButtonParameters()
            {
                buttonIcon = GUIHelper.Icons.WebIcon,
                label = label,
                labelTooltip = tooltip
            };
            return ButtonWasClicked(buttonParameters);
#endif
        }
    }
}
