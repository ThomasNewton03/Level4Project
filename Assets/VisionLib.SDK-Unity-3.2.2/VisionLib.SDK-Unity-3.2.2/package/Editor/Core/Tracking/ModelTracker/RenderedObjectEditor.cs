using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(RenderedObject))]
    public class RenderedObjectEditor : Editor
    {
        private SerializedProperty trackingAnchorProperty;
        private SerializedProperty smoothTimeProperty;
        private SerializedProperty renderModeProperty;
        private RenderedObject renderedObject;

        public void OnEnable()
        {
            this.trackingAnchorProperty =
                this.serializedObject.FindProperty("trackingAnchor");
            this.smoothTimeProperty = this.serializedObject.FindProperty("smoothTime");
            this.renderModeProperty = this.serializedObject.FindProperty("renderMode");
            this.renderedObject = this.serializedObject.targetObject as RenderedObject;
        }

        public override void OnInspectorGUI()
        {
            SetupIssueEditorHelper.DrawErrorBox(this.renderedObject);

            if (this.renderedObject == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(this.trackingAnchorProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.renderedObject.DeregisterWithTrackingAnchor();
                this.serializedObject.ApplyModifiedProperties();
                this.renderedObject.RegisterWithTrackingAnchor();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                this.renderModeProperty,
                new GUIContent(
                    "Visible",
                    "Couples the visibility of Meshes on this GameObject and its children to the current tracking state. " +
                    "Manually disabled MeshRenderers are never enabled."));
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                this.renderedObject.UpdateRendering();
            }
            using (new EditorGUI.DisabledGroupScope(
                       this.renderedObject.renderMode == RenderedObject.RenderMode.Never ||
                       this.renderedObject.renderMode ==
                       RenderedObject.RenderMode.WhenInitializing))
            {
                EditorGUILayout.PropertyField(this.smoothTimeProperty);
            }
            
            RenderedObjectEditorHelper.DrawEnableMeshRenderersButton(
                this.renderedObject.gameObject);
            
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
