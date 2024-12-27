using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(UEyeCameraParameter))]
    public class UEyeCameraParameterDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            using var verticalScope = new EditorGUILayout.VerticalScope();
            var maxWidth = position.width - 35f;
            var bodyRect = GUILayoutUtility.GetRect(100f, maxWidth, 15, 20);

            property.isExpanded = EditorGUI.Foldout(bodyRect, property.isExpanded, label, true);
            if (!property.isExpanded)
            {
                return;
            }
            EditorGUI.indentLevel++;
            DrawIndentedSection(property);
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
        
        protected virtual void DrawIndentedSection(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("OnValueChanged"));
        }
    }
}
