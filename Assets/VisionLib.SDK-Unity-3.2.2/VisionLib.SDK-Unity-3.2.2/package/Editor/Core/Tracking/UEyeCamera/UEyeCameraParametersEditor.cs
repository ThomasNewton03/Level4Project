using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(UEyeCameraParameters), true)]
    [CanEditMultipleObjects]
    public class UEyeCameraParametersEditor : Editor
    {
        private SerializedProperty exposureProperty;
        private SerializedProperty gainProperty;
        private SerializedProperty blackLevelProperty;
        private SerializedProperty gammaProperty;

        private UEyeCameraParameters uEyeCameraParameters;
        
        private void OnEnable()
        {
            this.exposureProperty = this.serializedObject.FindProperty("exposure");
            this.gainProperty = this.serializedObject.FindProperty("gain");
            this.blackLevelProperty = this.serializedObject.FindProperty("blackLevel");
            this.gammaProperty = this.serializedObject.FindProperty("gamma");
            
            this.uEyeCameraParameters = this.serializedObject.targetObject as UEyeCameraParameters;
        }

        public override void OnInspectorGUI()
        {
            if (!this.uEyeCameraParameters)
            {
                return;
            }

            this.uEyeCameraParameters.GetSceneIssues().Draw();

            DrawParametersSection();
        }

        private void DrawParametersSection()
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.exposureProperty);
            EditorGUILayout.PropertyField(this.gainProperty);
            EditorGUILayout.PropertyField(this.blackLevelProperty);
            EditorGUILayout.PropertyField(this.gammaProperty);
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }

            
            EditorGUI.indentLevel--;
        }

    }
}
