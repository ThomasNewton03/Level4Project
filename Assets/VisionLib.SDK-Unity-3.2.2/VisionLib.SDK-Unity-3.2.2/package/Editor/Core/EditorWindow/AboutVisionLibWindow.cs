using UnityEngine;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public class AboutVisionLibWindow : VisionLibWindow
    {
        [MenuItem("VisionLib/About", false, 1009)]
        private static void CreateWindow()
        {
            var window = (AboutVisionLibWindow) GetWindow(
                typeof(AboutVisionLibWindow),
                true,
                "About VisionLib");

            window.maxSize = new Vector2(450f, 115f);
            window.minSize = new Vector2(450f, 115f);
            window.Show();
        }

        protected override void DrawContent()
        {
            DrawCopyableInformationField(
                "SDK Version",
                API.SystemInfo.GetVLSDKVersion(),
                API.SystemInfo.GetDetailedVLSDKVersion());
            DrawCopyableInformationField("Host ID", API.SystemInfo.GetHostID());
        }

        private static void DrawCopyableInformationField(
            string fieldLabel,
            string fieldContent,
            string contentToCopy = null)
        {
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(fieldLabel, GUILayout.Width(120));
                EditorGUILayout.LabelField(fieldContent, GUILayout.Width(270));

                if (GUILayout.Button(
                        EditorGUIUtility.IconContent("TreeEditor.Duplicate", "| Copy to clipboard"),
                        GUILayout.Width(30)))
                {
                    if (contentToCopy == null)
                    {
                        contentToCopy = fieldContent;
                    }

                    GUIUtility.systemCopyBuffer = contentToCopy;
                    LogHelper.LogInfo(
                        "Copied " + fieldLabel + " \"" + contentToCopy + "\" to clipboard");
                }
            }
        }
    }
}
