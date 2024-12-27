using UnityEditor;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomEditor(typeof(TrackingConfiguration))]
    public class TrackingConfigurationEditor : Editor
    {
        private SerializedProperty configurationFileReferenceProperty;
        private SerializedProperty licenseFileReferenceProperty;
        private SerializedProperty calibrationFileReferenceProperty;
        private SerializedProperty autoStartTrackingProperty;
        private SerializedProperty extendTrackingWithSLAMProperty;
        private SerializedProperty staticSceneProperty;
        private SerializedProperty inputSourceProperty;
        private SerializedProperty imageSequenceURIProperty;
        private SerializedProperty showOnMobileProperty;

        private SerializedProperty configurationPathProperty;
        private SerializedProperty licensePathProperty;

        private SerializedProperty legacyPathProperty;

        private SerializedProperty ignoreSetupIssuesProperty;

        private string extendTrackingInfoMessage = "";
        private string staticSceneInfoMessage = "";
        private string inputSourceInfoMessage = "";

        private bool customExtendedTrackingValueSetViaQueryString = false;
        private bool customStaticSceneValueSetViaQueryString = false;
        private bool customInputSetViaQueryString = false;

        private void OnEnable()
        {
            this.configurationFileReferenceProperty =
                serializedObject.FindProperty("configurationFileReference");
            this.licenseFileReferenceProperty =
                serializedObject.FindProperty("licenseFileReference");
            this.calibrationFileReferenceProperty =
                serializedObject.FindProperty("calibrationFileReference");
            this.autoStartTrackingProperty = serializedObject.FindProperty("autoStartTracking");
            this.extendTrackingWithSLAMProperty =
                serializedObject.FindProperty("extendTrackingWithSLAM");
            this.staticSceneProperty = serializedObject.FindProperty("staticScene");
            this.inputSourceProperty = serializedObject.FindProperty("inputSource");
            this.imageSequenceURIProperty = serializedObject.FindProperty("imageSequenceURI");
            this.showOnMobileProperty = serializedObject.FindProperty("showOnMobileDevices");

            this.configurationPathProperty =
                this.configurationFileReferenceProperty.FindPropertyRelative("uri");
            this.licensePathProperty =
                this.licenseFileReferenceProperty.FindPropertyRelative("uri");

            this.legacyPathProperty = serializedObject.FindProperty("path");

            this.ignoreSetupIssuesProperty = serializedObject.FindProperty("ignoreSetupIssues");

            serializedObject.Update();

            if (this.legacyPathProperty.stringValue != "")
            {
                SetConfigurationUriFromLegacyPathProperty();
            }

            OverwriteParametersWithConfigurationQuery();
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(this.ignoreSetupIssuesProperty);
            
            SetupIssueEditorHelper.DrawSceneValidation();

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(this.configurationFileReferenceProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                OverwriteParametersWithConfigurationQuery();
            }

            EditorGUILayout.PropertyField(this.licenseFileReferenceProperty);

            if (this.licensePathProperty.stringValue == "")
            {
                DisplayInfoHelpbox(
                    "No license file set. Using the license, which is specified in the TrackingManager.");
            }

            EditorGUILayout.PropertyField(this.calibrationFileReferenceProperty);

            EditorGUILayout.PropertyField(this.autoStartTrackingProperty);

            using (new EditorGUI.DisabledScope(this.customExtendedTrackingValueSetViaQueryString))
            {
                EditorGUILayout.PropertyField(this.extendTrackingWithSLAMProperty);
                if (this.extendTrackingWithSLAMProperty.boolValue &&
                    !this.customExtendedTrackingValueSetViaQueryString)
                {
                    EditorGUI.indentLevel++;
                    using (new EditorGUI.DisabledScope(
                               this.customStaticSceneValueSetViaQueryString))
                    {
                        EditorGUILayout.PropertyField(this.staticSceneProperty);
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    this.staticSceneProperty.boolValue = false;
                }
            }

            DisplayInfoHelpbox(this.extendTrackingInfoMessage);
            DisplayInfoHelpbox(this.staticSceneInfoMessage);

            DrawInputSourceProperties();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInputSourceProperties()
        {
            using (new EditorGUI.DisabledScope(this.customInputSetViaQueryString))
            {
                EditorGUILayout.PropertyField(this.inputSourceProperty);

                var inputSource = (TrackingConfiguration.InputSource) this.inputSourceProperty
                    .enumValueIndex;

                switch (inputSource)
                {
                    case TrackingConfiguration.InputSource.InputSelection:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(this.showOnMobileProperty);
                        EditorGUI.indentLevel--;
                        break;
                    case TrackingConfiguration.InputSource.ImageSequence:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(this.imageSequenceURIProperty);
                        EditorGUI.indentLevel--;
                        break;
                }
            }

            DisplayInfoHelpbox(this.inputSourceInfoMessage);
        }

        private void DisplayInfoHelpbox(string infoMessage)
        {
            if (infoMessage != "")
            {
                EditorGUILayout.HelpBox(infoMessage, MessageType.Info);
            }
        }

        private void OverwriteParametersWithConfigurationQuery()
        {
            this.extendTrackingInfoMessage = "";
            this.inputSourceInfoMessage = "";
            this.staticSceneInfoMessage = "";

            this.customInputSetViaQueryString = false;
            this.customExtendedTrackingValueSetViaQueryString = false;
            this.customStaticSceneValueSetViaQueryString = false;

            if (QueryHelper.CustomExtendibleTrackingValueSetInQueryString(
                    this.configurationPathProperty.stringValue))
            {
                this.extendTrackingInfoMessage =
                    "Overwriting the Extend Tracking parameter with a custom value from the query string.";
                this.customExtendedTrackingValueSetViaQueryString = true;
            }

            if (QueryHelper.CustomStaticSceneValueSetInQueryString(
                    this.configurationPathProperty.stringValue))
            {
                this.staticSceneInfoMessage =
                    "Overwriting the staticScene parameter with a custom value from the query string.";
                this.customStaticSceneValueSetViaQueryString = true;
            }

            if (QueryHelper.CustomInputSetInQueryString(this.configurationPathProperty.stringValue))
            {
                this.inputSourceInfoMessage =
                    "Disabling the input source selection: A custom input is set via query string.";
                this.customInputSetViaQueryString = true;
            }
        }

        private void SetConfigurationUriFromLegacyPathProperty()
        {
            this.configurationPathProperty.stringValue = PathHelper.CombinePaths(
                "streaming-assets-dir:VisionLib",
                this.legacyPathProperty.stringValue);

            this.legacyPathProperty.stringValue = "";
        }
    }
}
