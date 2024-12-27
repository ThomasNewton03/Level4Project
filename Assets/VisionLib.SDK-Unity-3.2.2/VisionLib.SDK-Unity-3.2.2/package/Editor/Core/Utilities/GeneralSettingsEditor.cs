using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(GeneralSettings))]
    public class GeneralSettingsEditor : Editor
    {
        private SerializedProperty logLevelProperty;

        private GeneralSettings generalSettings;

        private void OnEnable()
        {
            this.logLevelProperty = this.serializedObject.FindProperty("logLevel");

            this.generalSettings = this.serializedObject.targetObject as GeneralSettings;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.logLevelProperty);
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                this.generalSettings.Update();
            }
        }
    }
}
