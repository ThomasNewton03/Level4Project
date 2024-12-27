using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(GameObjectPoseInteraction), true)]
    public class GameObjectPoseInteractionEditor : Editor
    {
        private bool showInteractionSettings;

        private SerializedProperty gameViewCameraProperty;
        private SerializedProperty interactionTypeProperty;
        private SerializedProperty dragRotationSpeedProperty;
        private SerializedProperty dragRotationSpeedDampeningProperty;
        private SerializedProperty dragRotationSpeedThresholdProperty;
        private SerializedProperty zoomStepProperty;
        private SerializedProperty scrollThresholdProperty;
        private SerializedProperty panFactorProperty;

        private GameObjectPoseInteraction gameObjectPoseInteraction;

        private void OnEnable()
        {
            this.gameViewCameraProperty = this.serializedObject.FindProperty("gameViewCamera");
            this.interactionTypeProperty = this.serializedObject.FindProperty("interactionType");
            this.dragRotationSpeedProperty =
                this.serializedObject.FindProperty("dragRotationSpeed");
            this.dragRotationSpeedDampeningProperty =
                this.serializedObject.FindProperty("dragRotationSpeedDampening");
            this.dragRotationSpeedThresholdProperty =
                this.serializedObject.FindProperty("dragRotationSpeedThreshold");
            this.zoomStepProperty = this.serializedObject.FindProperty("zoomStep");
            this.scrollThresholdProperty = this.serializedObject.FindProperty("scrollThreshold");
            this.panFactorProperty = this.serializedObject.FindProperty("panFactor");

            this.gameObjectPoseInteraction =
                this.serializedObject.targetObject as GameObjectPoseInteraction;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            SetupIssueEditorHelper.DrawErrorBox(this.gameObjectPoseInteraction);

            DrawCameraSettingsSection();
            DrawInteractionTypeSection();
            DrawOriginalPoseSection();
            this.showInteractionSettings = EditorGUILayout.Foldout(
                this.showInteractionSettings,
                "Interaction Tuning Settings");
            if (this.showInteractionSettings)
            {
                DrawInteractionSettingsSection();
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawOriginalPoseSection()
        {
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            if (ButtonParameters.ButtonWasClicked(
                    new ButtonParameters()
                    {
                        label = "Record original pose",
                        labelTooltip =
                            "Set the GameObject's current pose as the original pose.",
                        buttonIcon = GUIHelper.Icons.ImportIcon
                    }))
            {
                this.gameObjectPoseInteraction.SaveCurrentPoseAsOriginalPose();
                this.serializedObject.ApplyModifiedProperties();
            }

            if (ButtonParameters.ButtonWasClicked(
                    new ButtonParameters()
                    {
                        label = "Reset original pose",
                        labelTooltip =
                            "Reset the GameObject's pose to the last recorded original pose.",
                        buttonIcon = GUIHelper.Icons.RefreshIcon
                    }))
            {
                this.gameObjectPoseInteraction.ResetToOriginalPose();
                this.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.EndDisabledGroup();
        }

        private void DrawCameraSettingsSection()
        {
            EditorGUILayout.PropertyField(this.gameViewCameraProperty);
        }

        private void DrawInteractionTypeSection()
        {
            EditorGUILayout.PropertyField(this.interactionTypeProperty);
        }

        private void DrawInteractionSettingsSection()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Rotation Settings", CustomEditorStyles.boldWithWrappedLabel);
            EditorGUILayout.PropertyField(this.dragRotationSpeedProperty);
            EditorGUILayout.PropertyField(this.dragRotationSpeedDampeningProperty);
            EditorGUILayout.PropertyField(this.dragRotationSpeedThresholdProperty);

            EditorGUILayout.LabelField("Zoom Settings", CustomEditorStyles.boldWithWrappedLabel);
            EditorGUILayout.PropertyField(this.zoomStepProperty);
            EditorGUILayout.PropertyField(this.scrollThresholdProperty);
            EditorGUILayout.PropertyField(this.dragRotationSpeedThresholdProperty);

            EditorGUILayout.LabelField("Pan Settings", CustomEditorStyles.boldWithWrappedLabel);
            EditorGUILayout.PropertyField(this.panFactorProperty);
            EditorGUI.indentLevel--;
        }
    }
}
