using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(ImageSequenceParameters))]
    public class ImageSequenceParametersEditor : Editor
    {
        private static bool showEvents;

        private SerializedProperty firstIndexProperty;
        private SerializedProperty firstIndexEventProperty;
        private SerializedProperty currentIndexProperty;
        private SerializedProperty currentIndexEventProperty;
        private SerializedProperty lastIndexProperty;
        private SerializedProperty lastIndexEventProperty;
        private SerializedProperty maxIndexProperty;
        private SerializedProperty playBackSpeedProperty;

        private ImageSequenceParameters imageSequenceParameters;

        private void OnEnable()
        {
            this.firstIndexProperty = this.serializedObject.FindProperty("firstIndex");
            this.currentIndexProperty = this.serializedObject.FindProperty("currentIndex");
            this.lastIndexProperty = this.serializedObject.FindProperty("lastIndex");
            this.maxIndexProperty = this.serializedObject.FindProperty("maxIndex");

            this.playBackSpeedProperty = this.serializedObject.FindProperty("playBackSpeed");

            this.firstIndexEventProperty =
                this.serializedObject.FindProperty("onFirstIndexUpdated");
            this.currentIndexEventProperty =
                this.serializedObject.FindProperty("onCurrentIndexUpdated");
            this.lastIndexEventProperty = this.serializedObject.FindProperty("onLastIndexUpdated");

            this.imageSequenceParameters =
                this.serializedObject.targetObject as ImageSequenceParameters;
        }

        public override void OnInspectorGUI()
        {
            this.imageSequenceParameters.GetSceneIssues().Draw();

            DrawIndexSelection();
            DrawPlaybackSpeed();
            DrawUnityEvents();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawIndexSelection()
        {
            EditorGUI.BeginDisabledGroup(!TrackingManager.DoesTrackerExistAndIsRunning());

            if (DrawIntParameter(this.firstIndexProperty, 0, this.lastIndexProperty.intValue - 1))
            {
                this.imageSequenceParameters.SetFirstIndex(this.firstIndexProperty.intValue);
            }
            if (DrawIntParameter(
                    this.lastIndexProperty,
                    this.firstIndexProperty.intValue + 1,
                    this.maxIndexProperty.intValue))
            {
                this.imageSequenceParameters.SetLastIndex(this.lastIndexProperty.intValue);
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Reset Sequence Range",
                        "Resets the first and last index to the ends of the image sequence.")))
            {
                GUI.FocusControl(null);
                this.imageSequenceParameters.ResetSequenceRange();
            }

            var currentIndex = this.currentIndexProperty.intValue;
            var newIndex = (int) EditorGUILayout.Slider(
                "Image Sequence Index",
                currentIndex,
                this.firstIndexProperty.intValue,
                this.lastIndexProperty.intValue);
            EditorGUI.EndDisabledGroup();
            if (newIndex != currentIndex)
            {
                this.imageSequenceParameters.SetCurrentIndex(newIndex);
            }
        }

        private void DrawPlaybackSpeed()
        {
            var enabled = TrackingManager.DoesTrackerExistAndIsRunning();
            EditorGUI.BeginDisabledGroup(!enabled);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image Sequence Controls");
            GUILayout.FlexibleSpace();
            var newPlaybackType = MediaButton.DrawAll(
                this.imageSequenceParameters.PlayBackSpeed,
                enabled);
            if (newPlaybackType.HasValue)
            {
                this.imageSequenceParameters.PlayBackSpeed = newPlaybackType.Value;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawUnityEvents()
        {
            ImageSequenceParametersEditor.showEvents = EditorGUILayout.Foldout(
                ImageSequenceParametersEditor.showEvents,
                "Events",
                true);
            if (!ImageSequenceParametersEditor.showEvents)
            {
                return;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.firstIndexEventProperty);
            EditorGUILayout.PropertyField(this.currentIndexEventProperty);
            EditorGUILayout.PropertyField(this.lastIndexEventProperty);
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws IntField of the Property. Returns true, if the property has changed
        /// </summary>
        /// <param name="property"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns> Returns true, if the property has changed</returns>
        private static bool DrawIntParameter(
            SerializedProperty property,
            int minValue,
            int maxValue)
        {
            var oldValue = property.intValue;
            var newValue = EditorGUILayout.IntField(
                new GUIContent(property.displayName, property.tooltip),
                oldValue);
            if (newValue == oldValue || newValue < minValue || newValue > maxValue)
            {
                return false;
            }
            property.intValue = newValue;
            return oldValue != property.intValue;
        }
    }
}
