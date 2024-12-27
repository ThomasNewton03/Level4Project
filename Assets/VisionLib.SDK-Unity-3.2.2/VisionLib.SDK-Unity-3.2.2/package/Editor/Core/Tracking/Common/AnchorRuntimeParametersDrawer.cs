using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(AnchorRuntimeParameters))]
    public class AnchorRuntimeParametersDrawer : PropertyDrawer
    {
        private static bool showAdvancedParameters;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUILayout.PropertyField(property.FindPropertyRelative("detectionThreshold"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("trackingThreshold"));

            AnchorRuntimeParametersDrawer.showAdvancedParameters = EditorGUILayout.Foldout(
                AnchorRuntimeParametersDrawer.showAdvancedParameters,
                new GUIContent("Advanced"),
                true);
            if (AnchorRuntimeParametersDrawer.showAdvancedParameters)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("showLineModel"));

                EditorGUILayout.PropertyField(
                    property.FindPropertyRelative("contourEdgeThreshold"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("creaseEdgeThreshold"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("contrastThreshold"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("detectionRadius"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("trackingRadius"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("keyFrameDistance"));
                EditorGUILayout.PropertyField(
                    property.FindPropertyRelative("poseFilteringSmoothness"));
                EditorGUILayout.PropertyField(
                    property.FindPropertyRelative("sensitivityForEdgesInTexture"));
                EditorGUILayout.PropertyField(
                    property.FindPropertyRelative("disablePoseEstimation"));

                EditorGUI.BeginDisabledGroup(TrackingManager.DoesTrackerExistAndIsInitialized());
                EditorGUILayout.PropertyField(property.FindPropertyRelative("customParameters"));
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
