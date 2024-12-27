using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(LogarithmicFloatRuntimeParameter), true)]
    public class LogarithmicFloatRuntimeParameterDrawer : FloatRuntimeParameterDrawer
    {
        protected override void DrawSlider(
            Rect sliderRect,
            float minValue,
            float maxValue,
            SerializedProperty valueProperty)
        {
            EditorGUI.BeginChangeCheck();
            var logMin = Mathf.Log10(minValue);
            var logMax = Mathf.Log10(maxValue);
            var logValue = Mathf.Log10(valueProperty.floatValue);
            var newLogValue = GUI.HorizontalSlider(sliderRect, logValue, logMin, logMax);
            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.floatValue = Mathf.Pow(10, newLogValue);
            }
        }
    }
}
