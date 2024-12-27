using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    public class TrackingAnchorInfoBox
    {
        private readonly TrackingAnchor trackingAnchor;
        private bool showAugmentationSettings;

        public TrackingAnchorInfoBox(TrackingAnchor anchor)
        {
            this.trackingAnchor = anchor;
        }

        public void Draw()
        {
            GUILayout.BeginVertical("", "HelpBox");

            RevealInHierarchy.DrawButton(GetAnchorName(), this.trackingAnchor.gameObject);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(
                "Number of MeshRenderers: " + GetNumberOfTrackingObjects(),
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.ObjectField(
                new GUIContent("SLAM Camera", ""),
                GetSLAMCamera(),
                typeof(Object),
                false);

            this.showAugmentationSettings = EditorGUILayout.Foldout(
                this.showAugmentationSettings,
                new GUIContent(
                    "Augmented Content",
                    "Augmentation and InitPoseGuide for the given TrackingAnchor"),
                true);
            if (this.showAugmentationSettings)
            {
                RenderedObjectEditorHelper.DrawRenderedObjectsSection(GetRenderedObjects());
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private string GetAnchorName()
        {
            return this.trackingAnchor.GetAnchorName();
        }

        private IEnumerable<RenderedObject> GetRenderedObjects()
        {
            return this.trackingAnchor.GetRegisteredRenderedObjects();
        }

        private int GetNumberOfTrackingObjects()
        {
            return this.trackingAnchor.GetComponentsInChildren<TrackingObject>().Length;
        }

        private Camera GetSLAMCamera()
        {
            return this.trackingAnchor.GetSLAMCamera();
        }
    }
}
