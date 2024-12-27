using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(UEyeCameraExposureParameter))]
    public class UEyeCameraExposureParameterDrawer : UEyeCameraParameterDrawer
    {
        protected override void DrawIndentedSection(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("OnValueChanged"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("OnMinChanged"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("OnMaxChanged"));
        }
    }
}
