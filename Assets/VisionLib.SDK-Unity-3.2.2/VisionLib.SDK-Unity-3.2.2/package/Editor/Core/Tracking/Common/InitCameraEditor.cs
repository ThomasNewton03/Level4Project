using UnityEngine;
using UnityEditor;
using System;

namespace Visometry.VisionLib.SDK.Core
{
    /// \deprecated EditorScript of Obsolete component InitCamera
    [Obsolete("EditorScript of Obsolete component InitCamera")]
    [CustomEditor(typeof(InitCamera))]
    [CanEditMultipleObjects]
    public class InitCameraEditor : Editor
    {
        private string initPoseLabel = "\"initPose\":";
        private string initPoseString = "";
        private string initPoseError = "";

        private GUIContent content = new GUIContent();
        private Vector2 scrollPos = new Vector2();

        private InitCamera initCamera;

        void Reset()
        {
            this.initCamera = target as InitCamera;
        }

        private void OnEnable()
        {
            this.initCamera = target as InitCamera;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button(
                    "Align with view",
                    GUILayout.Height(25)))
            {
                initCamera.AlignWithView();
            }

            // Only show the VisionLib initPose, if the InitCamera is selected in hierarchy
            if (!PrefabUtility.IsPartOfPrefabInstance(this.targets[0]))
            {
                return;
            }

            // Only show the VisionLib initPose, if one object is selected
            if (this.targets.Length == 1)
            {
                UpdateInitPoseString();

                if (this.initPoseError == null)
                {
                    ReadOnlyTextField(
                        this.initPoseLabel,
                        this.initPoseString,
                        EditorStyles.helpBox);
                }
                else
                {
                    EditorGUILayout.HelpBox(this.initPoseError, MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "\"initPose\" preview does not work with multi-editing.",
                    MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateInitPoseString()
        {
            InitCamera initCamera = (InitCamera) this.targets[0];

            try
            {
                this.initPoseString = initCamera.GetInitPoseAsString();
                this.initPoseError = null;
            }
            catch (NullReferenceException e)
            {
                this.initPoseError = e.Message;
            }
        }

        private void ReadOnlyTextField(string labelText, string text, GUIStyle style)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(labelText, GUILayout.Width(EditorGUIUtility.labelWidth));

                this.scrollPos = EditorGUILayout.BeginScrollView(
                    this.scrollPos,
                    GUILayout.ExpandHeight(false));
                {
                    // Explicitly set the size of the SelectableLabel. Otherwise
                    // the ScrollView doesn't work correctly.
                    this.content.text = text;
                    Vector2 size = style.CalcSize(this.content);
                    EditorGUILayout.SelectableLabel(
                        text,
                        style,
                        GUILayout.ExpandWidth(true),
                        GUILayout.MinWidth(size.x),
                        GUILayout.Height(size.y));
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
