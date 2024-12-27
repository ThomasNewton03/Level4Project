using UnityEngine;
using UnityEditor;
using System;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(TrackingCamera))]
    public class TrackingCameraEditor : Editor
    {
        private TrackingCamera trackingCamera;
        private SerializedProperty coordinateSystemAdjustmentProperty;
        private SerializedProperty backgroundLayerProperty;
        
        private void OnEnable()
        {
            this.coordinateSystemAdjustmentProperty = this.serializedObject.FindProperty("coordinateSystemAdjustment");
            this.backgroundLayerProperty = this.serializedObject.FindProperty("backgroundLayer");
            this.trackingCamera = this.serializedObject.targetObject as TrackingCamera;
        }

        public override void OnInspectorGUI()
        {
            this.trackingCamera.GetSceneIssues().Draw();

            EditorGUILayout.PropertyField(this.coordinateSystemAdjustmentProperty);
            EditorGUILayout.PropertyField(this.backgroundLayerProperty);
            
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
