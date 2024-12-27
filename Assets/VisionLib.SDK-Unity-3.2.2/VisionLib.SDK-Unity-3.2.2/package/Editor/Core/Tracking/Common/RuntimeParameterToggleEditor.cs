using System;
using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    /// \deprecated Instead of using the Runtime parameter, use the corresponding parameters in the TrackingAnchor.
    [Obsolete("Instead of using the Runtime parameter, use the corresponding parameters in the TrackingAnchor.")]
    [CustomEditor(typeof(RuntimeParameterToggle))]
    public class RuntimeParameterToggleEditor : Editor
    {
        private SerializedProperty runtimeParameterProp;
        private SerializedProperty buttonTextIfEnabledProp;
        private SerializedProperty buttonTextIfDisabledProp;


        private string ParameterPropertyNotSetMessage = "The target runtime parameter is not set.";

        private string ParameterPropertyIsNotBoolMessage(RuntimeParameter.ParameterType type)
        {
            return "The target runtime parameter is of type \"" + type.ToString() +
                   "\".  RuntimeParameterToggle only supports \"Bool\" runtime parameters.";
        }

        void OnEnable()
        {
            this.runtimeParameterProp =
                this.serializedObject.FindProperty("runtimeParameter");
            this.buttonTextIfEnabledProp =
                this.serializedObject.FindProperty("buttonTextIfEnabled");
            this.buttonTextIfDisabledProp =
                this.serializedObject.FindProperty("buttonTextIfDisabled");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(this.runtimeParameterProp);
            
            if (this.runtimeParameterProp.objectReferenceValue == null)
            {
                DisplayInfoError(this.ParameterPropertyNotSetMessage);
            }
            else
            {
                var runtimeParameter =
                    (RuntimeParameter) this.runtimeParameterProp.objectReferenceValue;
                if (runtimeParameter.parameterType != RuntimeParameter.ParameterType.Bool)
                {
                    DisplayInfoError(ParameterPropertyIsNotBoolMessage(runtimeParameter.parameterType));
                }
            }
            
            EditorGUILayout.PropertyField(this.buttonTextIfEnabledProp);
            EditorGUILayout.PropertyField(this.buttonTextIfDisabledProp);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DisplayInfoError(string errorMessage)
        {
            if (errorMessage != "")
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }
    }
}
