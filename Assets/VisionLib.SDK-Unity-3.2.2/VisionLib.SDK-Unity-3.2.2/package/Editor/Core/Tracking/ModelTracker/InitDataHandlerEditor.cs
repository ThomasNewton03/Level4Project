using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(InitDataHandler))]
    public class InitDataHandlerEditor : Editor
    {
        private InitDataHandler initDataHandler;
        private SerializedProperty loadInitDataOnTrackerStart;
        private SerializedProperty initDataURI;

        private void OnEnable()
        {
            this.initDataHandler = this.serializedObject.targetObject as InitDataHandler;
            this.loadInitDataOnTrackerStart =
                this.serializedObject.FindProperty("loadInitDataOnTrackerStart");
            this.initDataURI = this.serializedObject.FindProperty("initDataURI");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            SetupIssueEditorHelper.DrawErrorBox(this.initDataHandler);

            EditorGUILayout.PropertyField(this.loadInitDataOnTrackerStart);
            EditorGUILayout.PropertyField(this.initDataURI);

            if (this.serializedObject.targetObject != null)
            {
                this.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
