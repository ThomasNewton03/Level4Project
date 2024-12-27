using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(TrackingManager), true)]
    [CanEditMultipleObjects]
    public class TrackingManagerEditor : Editor
    {
        private static bool showTrackerParameters;
        private TrackingManager trackingManager;
        private bool showSceneOverview = false;
        private List<TrackingAnchorInfoBox> trackingAnchorInfoBoxes =
            new List<TrackingAnchorInfoBox>();

        private SerializedProperty trackerRuntimeParametersProperty;

        private void Reset()
        {
            this.trackingManager = this.target as TrackingManager;
            if (this.trackingManager)
            {
                this.trackingAnchorInfoBoxes = AggregateTrackingAnchorInfoBoxes();
                this.trackingManager.GetTrackerRuntimeParameters();
            }
        }

        private void OnEnable()
        {
            Reset();
            this.trackerRuntimeParametersProperty =
                this.serializedObject.FindProperty("parameters");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this.trackingManager.GetSceneIssues().Draw();
            DrawDefaultInspector();
            DrawParametersSection();
            DrawSceneInformation();
            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawParametersSection()
        {
            TrackingManagerEditor.showTrackerParameters = EditorGUILayout.Foldout(
                TrackingManagerEditor.showTrackerParameters,
                "Tracker Parameters",
                true);

            if (!TrackingManagerEditor.showTrackerParameters)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(this.trackerRuntimeParametersProperty);

            if (this.serializedObject.ApplyModifiedProperties())
            {
                var trackerRuntimeParameters =
                    (TrackerRuntimeParameters) this.trackerRuntimeParametersProperty
                        .managedReferenceValue;
                TrackingManager.CatchCommandErrors(
                    trackerRuntimeParameters.UpdateParametersInBackendAsync(this.trackingManager),
                    this.trackingManager);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSceneInformation()
        {
            if (this.trackingAnchorInfoBoxes.Count == 0)
            {
                return;
            }

            this.showSceneOverview = EditorGUILayout.Foldout(
                this.showSceneOverview,
                new GUIContent("Scene Overview", ""),
                true);
            if (!this.showSceneOverview)
            {
                return;
            }

            EditorGUI.indentLevel++;
            GUILayout.BeginVertical();
            foreach (var infoBox in this.trackingAnchorInfoBoxes)
            {
                infoBox.Draw();
            }
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private static List<TrackingAnchorInfoBox> AggregateTrackingAnchorInfoBoxes()
        {
            var trackingAnchors = FindObjectsOfType<TrackingAnchor>();
            return trackingAnchors
                .Select(trackingAnchor => new TrackingAnchorInfoBox(trackingAnchor)).ToList();
        }
    }
}
