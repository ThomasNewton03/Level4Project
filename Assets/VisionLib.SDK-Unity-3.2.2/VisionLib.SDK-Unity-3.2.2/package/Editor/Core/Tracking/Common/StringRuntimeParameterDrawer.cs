using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(StringRuntimeParameter), true)]
    public class StringRuntimeParameterDrawer : DynamicTrackingParameterDrawer<string>
    {
        protected override void DrawParameterSetterField(
            Rect contentRect,
            DynamicTrackingParameter<string> parameter,
            SerializedProperty valueProperty)
        {
            EditorGUI.PropertyField(
                contentRect,
                valueProperty,
                new GUIContent("", parameter.GetDescription()));
        }
    }
}
