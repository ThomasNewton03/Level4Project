using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(SphereGeometry), true)]
    public class SphereGeometryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            DrawSphereLabel(position, label);

            EditorGUI.indentLevel++;
            DrawSphereProperties(property);
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public static void DrawSphereLabel(Rect position, GUIContent label)
        {
            label.text += " Properties";
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        }

        public static void DrawSphereProperties(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("detailLevel"));

            EditorGUILayout.PropertyField(
                property.FindPropertyRelative("thetaStart"),
                new GUIContent("Polar Start"));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative("thetaLength"),
                new GUIContent("Polar Length"));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative("phiStart"),
                new GUIContent("Azimuth Start"));
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative("phiLength"),
                new GUIContent("Azimuth Length"));
        }
    }

    [CustomPropertyDrawer(typeof(AdvancedSphereGeometry))]
    public class AdvancedSphereGeometryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SphereGeometryDrawer.DrawSphereLabel(position, label);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
            SphereGeometryDrawer.DrawSphereProperties(property);
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
