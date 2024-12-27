using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(InitPoseInteraction))]
    public class InitPoseInteractionEditor : Editor
    {
        private InitPoseInteraction initPoseInteraction;
        private SerializedProperty disableDuringTracking;

        private void OnEnable()
        {
            this.initPoseInteraction = this.serializedObject.targetObject as InitPoseInteraction;
            this.disableDuringTracking =
                this.serializedObject.FindProperty("disableDuringTracking");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            SetupIssueEditorHelper.DrawErrorBox(this.initPoseInteraction);

            EditorGUILayout.PropertyField(this.disableDuringTracking);
            
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
