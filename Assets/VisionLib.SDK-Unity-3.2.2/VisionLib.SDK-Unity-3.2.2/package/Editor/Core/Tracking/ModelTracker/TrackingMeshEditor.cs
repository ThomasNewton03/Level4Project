using System;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(TrackingMesh), true)]
    public class TrackingMeshEditor : TrackingObjectEditor
    {
        private TrackingMesh trackingMesh;
        private SerializedProperty useTextureProperty;

        private const string useTextureTooltip = "Use the texture of the model for tracking.";

        private new void OnEnable()
        {
            base.OnEnable();

            this.trackingMesh = this.serializedObject.targetObject as TrackingMesh;
            this.useTextureProperty = this.serializedObject.FindProperty("useTextureForTracking");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            SetupIssueEditorHelper.DrawErrorBox(this.trackingMesh);

            DrawTrackingObjectProperties();

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(
                    this.useTextureProperty,
                    new GUIContent("Use Texture", TrackingMeshEditor.useTextureTooltip));
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }

            DrawTrackingObjectDimensions();
        }
    }
}
