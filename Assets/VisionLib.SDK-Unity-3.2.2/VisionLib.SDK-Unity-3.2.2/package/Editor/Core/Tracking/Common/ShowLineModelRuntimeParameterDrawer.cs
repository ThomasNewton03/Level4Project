using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(ShowLineModelRuntimeParameter))]
    public class ShowLineModelRuntimeParameterDrawer : DynamicTrackingParameterDrawer<ShowLineModel>
    {
        private bool showLineModelEnabled;

        protected override void DrawParameterSetterField(
            Rect contentRect,
            DynamicTrackingParameter<ShowLineModel> parameter,
            SerializedProperty valueProperty)
        {
            var enabledState = parameter.GetValue().IsEnabled();
            if (!ToggleSwitch.DrawToggleSwitch(contentRect, enabledState, ""))
            {
                return;
            }
            
            switch (enabledState)
            {
                case TriState.False:
                case TriState.Mixed:
                    this.showLineModelEnabled = true;
                    break;
                case TriState.True:
                    this.showLineModelEnabled = false;
                    break;
            }
            valueProperty.FindPropertyRelative("enabled.tracked").boolValue =
                this.showLineModelEnabled;
            valueProperty.FindPropertyRelative("enabled.critical").boolValue =
                this.showLineModelEnabled;
            valueProperty.FindPropertyRelative("enabled.lost").boolValue =
                this.showLineModelEnabled;
        }

        protected override string ToDisplayString(DynamicTrackingParameter<ShowLineModel> parameter)
        {
            return parameter.GetValue().enabled.GetSharedValueForAllStates()
                .GetValueOrDefault(true).ToInvariantLowerCaseString();
        }

        protected override void DrawIndentedSection(SerializedProperty property)
        {
            var usingPerCorrespondency = DrawToggleUsePerCorrespondencyButton(property);
            DrawLineModelStates(property, usingPerCorrespondency);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("onBoolValueChanged"));
            EditorGUILayout.PropertyField(ExtractEventProperty(property));
        }

        private static bool DrawToggleUsePerCorrespondencyButton(SerializedProperty property)
        {
            var perCorrespondencyProperty = ExtractValueProperty(property)
                .FindPropertyRelative("color.perCorrespondency");
            if (perCorrespondencyProperty.boolValue)
            {
                if (ButtonParameters.ButtonWasClicked(
                        new ButtonParameters()
                        {
                            label =
                                "Using color per correspondency. Switch to color by tracking state.",
                            labelTooltip =
                                "The line model color depends on the tracking quality for each " +
                                "individual correspondency. Pressing this button switches to " +
                                "drawing the line model in a uniform color depending on the " +
                                "tracking state (tracked/critical/lost).",
                            buttonText = "↔",
                            buttonIcon = GUIHelper.Icons.None
                        }))
                {
                    perCorrespondencyProperty.boolValue = false;
                }
            }
            else
            {
                if (ButtonParameters.ButtonWasClicked(
                        new ButtonParameters()
                        {
                            label =
                                "Using color by tracking state. Switch to color per correspondency.",
                            labelTooltip =
                                "The line model is uniformly color depending on the tracking " +
                                "state (tracked/critical/lost). Pressing this button switches to " +
                                "coloring each individual correspondency depending on its " +
                                "tracking quality.",
                            buttonText = "↔",
                            buttonIcon = GUIHelper.Icons.None
                        }))
                {
                    perCorrespondencyProperty.boolValue = true;
                }
            }
            return perCorrespondencyProperty.boolValue;
        }

        private static void DrawLineModelStates(SerializedProperty property, bool perCorrespondency)
        {
            if (perCorrespondency)
            {
                var showLineModelProperty = ExtractValueProperty(property);
                var newLineWidth = showLineModelProperty.FindPropertyRelative("lineWidth.lost")
                    .intValue;
                newLineWidth = EditorGUILayout.IntSlider(
                    new GUIContent("Line Width"),
                    newLineWidth,
                    1,
                    100);
                showLineModelProperty.FindPropertyRelative("lineWidth.tracked").intValue =
                    newLineWidth;
                showLineModelProperty.FindPropertyRelative("lineWidth.critical").intValue =
                    newLineWidth;
                showLineModelProperty.FindPropertyRelative("lineWidth.lost").intValue =
                    newLineWidth;
            }
            else
            {
                DrawLineModelState("tracked", property);
                DrawLineModelState("critical", property);
                DrawLineModelState("lost", property);
            }
        }

        private static void DrawLineModelState(string name, SerializedProperty property)
        {
            var showLineModelProperty = ExtractValueProperty(property);
            var lineModelEnabledInCurrentStateProperty =
                showLineModelProperty.FindPropertyRelative("enabled." + name);
            var lineWidthInCurrentStateProperty =
                showLineModelProperty.FindPropertyRelative("lineWidth." + name);

            using var horizontalScope = new EditorGUILayout.HorizontalScope();

            lineModelEnabledInCurrentStateProperty.boolValue = EditorGUILayout.Toggle(
                new GUIContent(name),
                lineModelEnabledInCurrentStateProperty.boolValue);

            EditorGUI.BeginDisabledGroup(!lineModelEnabledInCurrentStateProperty.boolValue);
            {
                using var verticalScope = new EditorGUILayout.VerticalScope();
                var colorInCurrentStateProperty =
                    showLineModelProperty.FindPropertyRelative("color." + name);
                colorInCurrentStateProperty.colorValue =
                    EditorGUILayout.ColorField(colorInCurrentStateProperty.colorValue);
                lineWidthInCurrentStateProperty.intValue = EditorGUILayout.IntSlider(
                    new GUIContent("Line Width"),
                    lineWidthInCurrentStateProperty.intValue,
                    1,
                    100);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
