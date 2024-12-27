using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(FloatRuntimeParameter), true)]
    public class FloatRuntimeParameterDrawer : DynamicTrackingParameterDrawer<float>
    {
        private const float floatFieldWidth = 78;
        private const float minSliderWidth = 50;
        private const float rightBorder = 5;

        protected override void DrawParameterSetterField(
            Rect contentRect,
            DynamicTrackingParameter<float> parameter,
            SerializedProperty valueProperty)
        {
            var floatRuntimeParameter = (FloatRuntimeParameter) parameter;

            var minValue = floatRuntimeParameter.GetMinValue();
            var maxValue = floatRuntimeParameter.GetMaxValue();

            var indentation = EditorGUI.indentLevel *
                              DynamicTrackingParameterDrawer<float>.indentValue;
            var sliderWidth = contentRect.width - FloatRuntimeParameterDrawer.floatFieldWidth -
                              FloatRuntimeParameterDrawer.rightBorder;

            var sliderRect = new Rect(
                contentRect.x,
                contentRect.y,
                sliderWidth,
                contentRect.height);

            if (sliderRect.width >= FloatRuntimeParameterDrawer.minSliderWidth)
            {
                DrawSlider(sliderRect, minValue, maxValue, valueProperty);

                var floatFieldRect = new Rect(
                    contentRect.x + contentRect.width -
                    FloatRuntimeParameterDrawer.floatFieldWidth - indentation,
                    contentRect.y,
                    FloatRuntimeParameterDrawer.floatFieldWidth + indentation,
                    contentRect.height);
                DrawFloatField(floatFieldRect, minValue, maxValue, valueProperty);
            }
            else
            {
                var floatFieldRect = new Rect(
                    contentRect.x - indentation,
                    contentRect.y,
                    contentRect.width + indentation,
                    contentRect.height);
                DrawFloatField(floatFieldRect, minValue, maxValue, valueProperty);
            }
        }

        protected virtual void DrawSlider(
            Rect contentRect,
            float minValue,
            float maxValue,
            SerializedProperty valueProperty)
        {
            EditorGUI.BeginChangeCheck();
            var newLogValue = GUI.HorizontalSlider(
                contentRect,
                valueProperty.floatValue,
                minValue,
                maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.floatValue = newLogValue;
            }
        }

        private static void DrawFloatField(
            Rect floatFieldRect,
            float minValue,
            float maxValue,
            SerializedProperty valueProperty)
        {
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.FloatField(floatFieldRect, valueProperty.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                // Clamp the value to ensure it's between min and max values
                valueProperty.floatValue = Mathf.Clamp(newValue, minValue, maxValue);
            }
        }
    }
}
