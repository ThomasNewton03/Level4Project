using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public class TrackingSetupFromFileWindow : VisionLibDialogWindow
    {
        private string vlFilePath;

        private TrackingConfigurationFileConverter trackingConfiguration;
        private string fileContent = "";
        private string parseError = "";

        private Vector2 configScrollPosition;
        private Vector2 parseResultScrollPosition;

        public static TrackingConfigurationFileConverter OpenSetupTrackingDialog()
        {
            return OpenDialog("Setup Tracking from Tracking Configuration File");
        }

        private static TrackingConfigurationFileConverter OpenDialog(string windowTitle)
        {
            var window = CreateInstance<TrackingSetupFromFileWindow>();

            window.titleContent.text = windowTitle;
            window.infoAcceptText = "Setup Tracking";
            window.dialogMessage =
                "This operation will create TrackingAnchors and set the parameters according to a given tracking configuration file. " +
                "For every model defined in the corresponding anchor section, a TrackingURI Component will be added as a child of the TrackingAnchor.";
            window.disableAcceptButton = true;
            window.Resize();

#if UNITY_EDITOR_OSX
            // This is necessary since closing the file selection will also continue execution here.
            // This check returns `false`, if `Close` has been called.
            while (window != null)
#endif
            {
                window.ShowModalUtility();  
            }

            var dialogResult = window.dialogResult;
            var trackingConfig = window.trackingConfiguration;
            window.Destroy();
            return !dialogResult ? null : trackingConfig;
        }

        private void ResetContent()
        {
            this.parseError = "";
            this.fileContent = "";
            this.trackingConfiguration = null;
        }

        protected override float CalculateWindowHeightFromContent()
        {
            return CalculateBaseWindow() + Math.Min(
                this.parseError.NewlineCount() * VisionLibDialogWindow.lineHeightFactor +
                this.fileContent.NewlineCount() * VisionLibDialogWindow.lineHeightFactor +
                (this.trackingConfiguration == null ? 0.0f : 100.0f),
                VisionLibDialogWindow.baseWindowHeight * 5);
        }

        protected override void DrawAdditionalContent()
        {
            DrawTrackingConfigurationSelection();
            DrawTrackingConfiguration();
        }
        
        private void DrawTrackingConfigurationSelection()
        {
            if (ButtonParameters.ButtonWasClicked(
                    new ButtonParameters
                    {
                        label = "Open Tracking Configuration File",
                        labelTooltip =
                            "Set Up VisionLib Tracking from Tracking Configuration File",
                        buttonIcon = GUIHelper.Icons.SearchIcon
                    }))
            {
                ResetContent();
                this.vlFilePath = EditorUtility.OpenFilePanel(
                    "Select a tracking configuration file",
                    Application.streamingAssetsPath,
                    "vl");

                if (string.IsNullOrEmpty(this.vlFilePath))
                {
                    return;
                }
                var reader = new StreamReader(this.vlFilePath);
                try
                {
                    this.fileContent = reader.ReadToEnd();
                    this.trackingConfiguration = TrackingConfigurationFileConverter.Parse(
                        this.fileContent,
                        Path.GetDirectoryName(this.vlFilePath));
                }
                catch (Exception e)
                {
                    this.parseError = e.Message;
                }
                finally
                {
                    this.disableAcceptButton = this.trackingConfiguration == null;
                    Resize();
                    reader.Close();
                }
            }

            if (!string.IsNullOrEmpty(this.vlFilePath))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        new GUIContent("Tracking Configuration file"),
                        GUILayout.MaxWidth(150));
                    EditorGUILayout.LabelField(
                        new GUIContent(this.vlFilePath),
                        EditorStyles.wordWrappedLabel);
                }
            }
        }

        private void DrawTrackingConfiguration()
        {
            if (string.IsNullOrEmpty(this.vlFilePath))
            {
                return;
            }
            this.configScrollPosition = EditorGUILayout.BeginScrollView(this.configScrollPosition);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(this.fileContent);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(this.parseError))
            {
                EditorGUILayout.HelpBox(this.parseError, MessageType.Error);
            }

            if (this.trackingConfiguration != null)
            {
                this.parseResultScrollPosition =
                    EditorGUILayout.BeginScrollView(this.parseResultScrollPosition);
                EditorGUILayout.HelpBox(
                    this.trackingConfiguration.ToString(),
                    MessageType.None,
                    true);
                EditorGUILayout.EndScrollView();

                var parseWarning = this.trackingConfiguration.GetWarning();
                if (!string.IsNullOrEmpty(parseWarning))
                {
                    EditorGUILayout.HelpBox(parseWarning, MessageType.Warning);
                }
                GUILayout.Space(VisionLibWindow.verticalSpace);
            }
        }
    }
}
