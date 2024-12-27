using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(LicenseFile))]
    public class LicenseFileDrawer : PropertyDrawer
    {
        private static readonly string[] filter = {"XML files", "xml", "All files", "*"};

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            SerializedProperty pathProp = property.FindPropertyRelative("path");

            // Add the property label
            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            Rect buttonRect = new Rect(
                fieldRect.xMax - 25.0f,
                fieldRect.yMin,
                25.0f,
                fieldRect.height);
            Rect textRect = new Rect(
                fieldRect.xMin,
                fieldRect.yMin,
                fieldRect.width - buttonRect.width - 5,
                fieldRect.height);

            // Add a text field for the license file path
            EditorGUI.PropertyField(textRect, pathProp, GUIContent.none);

            // Add a button for selecting the license file using a file dialog
            if (GUI.Button(buttonRect, "..."))
            {
                GUI.FocusControl(null);
                string absolutePath = EditorUtility.OpenFilePanelWithFilters(
                    "License file",
                    Application.streamingAssetsPath,
                    filter);
                if (absolutePath.Length > 0)
                {
                    // The file should reside inside the StreamingAssets directory
                    if (absolutePath.StartsWith(Application.streamingAssetsPath + "/"))
                    {
                        // Store the path relative to the StreamingAssets directory
                        string relativePath =
                            absolutePath.Substring(Application.streamingAssetsPath.Length + 1);
                        pathProp.stringValue = "streaming-assets-dir:" + relativePath;
                    }
                    else
                    {
                        // Store the absolute path and output a warning
                        pathProp.stringValue = absolutePath;
                        LogHelper.LogWarning(
                            "The license file must reside inside the 'StreamingAssets' directory");
                    }
                }
            }

            // Make it possible to drop the license file into the text field
            Event e = Event.current;
            if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
            {
                if (DragAndDrop.paths.Length == 1 && textRect.Contains(e.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (e.type == EventType.DragPerform)
                    {
                        // The given path is relative to the project directory
                        const string relativeStreamingAssetsPath = "Assets/StreamingAssets/";
                        string projectRelativePath = DragAndDrop.paths[0];

                        // The file should reside inside the StreamingAssets directory
                        if (projectRelativePath.StartsWith(relativeStreamingAssetsPath))
                        {
                            string relativePath =
                                projectRelativePath.Substring(relativeStreamingAssetsPath.Length);
                            pathProp.stringValue = "streaming-assets-dir:" + relativePath;
                        }
                        // Store the absolute path and output a warning
                        else
                        {
                            string absolutePath = PathHelper.CombinePaths(
                                Application.dataPath,
                                projectRelativePath.Substring("Assets/".Length));
                            pathProp.stringValue = absolutePath;
                            LogHelper.LogWarning(
                                "The license file must reside inside the 'StreamingAssets' directory");
                        }
                        DragAndDrop.AcceptDrag();
                        e.Use();
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
