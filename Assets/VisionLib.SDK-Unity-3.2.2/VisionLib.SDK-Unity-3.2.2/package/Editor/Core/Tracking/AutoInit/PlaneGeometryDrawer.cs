using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(PlaneGeometry))]
    public class PlaneGeometryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text += " Properties";

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(property.FindPropertyRelative("length"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("width"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("step"));

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(LineGeometry))]
    public class LineGeometryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text += " Properties";

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(property.FindPropertyRelative("length"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("step"));

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
