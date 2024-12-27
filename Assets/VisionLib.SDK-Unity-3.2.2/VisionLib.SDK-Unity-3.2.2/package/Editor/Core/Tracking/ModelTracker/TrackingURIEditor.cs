using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(TrackingURI), true)]
    public class TrackingURIEditor : TrackingObjectEditor
    {
        private SerializedProperty URIProperty;
        private TrackingURI trackingURI;

        private const string URITooltip =
            "URI of the model file to be loaded and used for tracking.";

        private const string errorInstructions = "This will cause an exception " +
                                                 "in play mode. Set a valid URI " +
                                                 "or remove this component.";

        private new void OnEnable()
        {
            base.OnEnable();
            this.URIProperty = this.serializedObject.FindProperty("modelFileURI");
            this.trackingURI = this.serializedObject.targetObject as TrackingURI;
        }

        public override void OnInspectorGUI()
        {
            this.trackingURI.GetSceneIssues().Draw();
            
            this.serializedObject.Update();

            DrawURIPropertyField();

            DrawTrackingObjectProperties();
            
            DrawTrackingObjectDimensions();
        }

        private void DrawURIPropertyField()
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(
                    this.URIProperty,
                    GUIHelper.GenerateGUIContentWithIcon(
                        GUIHelper.Icons.ModelIcon,
                        TrackingURIEditor.URITooltip,
                        "Model File URI"));
            }
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
