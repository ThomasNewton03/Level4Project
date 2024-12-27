using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(FilePathReferenceFieldAttribute))]
    public class FilePathReferenceDrawer : PropertyDrawer
    {
        private class CachedObjectReferenceDatabase
        {
            private Dictionary<string, Object> uriObjectMap = new Dictionary<string, Object>();

            public Object LoadObjectFromURI(string uri)
            {
                Object loadedObject;
                if (!this.uriObjectMap.TryGetValue(uri, out loadedObject))
                {
                    loadedObject = AssetDatabase.LoadAssetAtPath(uri, typeof(Object));
                    this.uriObjectMap.Add(uri, loadedObject);
                }
                return loadedObject;
            }
        }

        private CachedObjectReferenceDatabase cachedObjectReferenceDatabase =
            new CachedObjectReferenceDatabase();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var uriProperty = property.FindPropertyRelative("uri");
            var fieldTypeProperty = property.FindPropertyRelative("fieldType");
            var filePathAttribute = this.attribute as FilePathReferenceFieldAttribute;

            DrawPropertyField(position, label, uriProperty, fieldTypeProperty, filePathAttribute);

            EditorGUI.EndProperty();
        }

        private void DrawPropertyField(
            Rect fieldRect,
            GUIContent label,
            SerializedProperty uriProperty,
            SerializedProperty fieldTypeProperty,
            FilePathReferenceFieldAttribute filePathAttribute)
        {
            var dropDownRect = new Rect(
                fieldRect.xMax - 60.0f,
                fieldRect.yMin,
                60.0f,
                fieldRect.height);
            var fileReferenceRect = new Rect(
                fieldRect.xMin,
                fieldRect.yMin,
                fieldRect.width - dropDownRect.width - 5,
                fieldRect.height);

            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                if ((TrackingConfiguration.FilePathReference.FieldType) fieldTypeProperty
                        .enumValueIndex == TrackingConfiguration.FilePathReference.FieldType.Object)
                {
                    DrawObjectField(fileReferenceRect, label, uriProperty, filePathAttribute);
                }
                else
                {
                    DrawStringField(fileReferenceRect, label, uriProperty, filePathAttribute);
                }
                DrawFieldTypeDropdown(dropDownRect, fieldTypeProperty);
            }

            DrawHelpBox(uriProperty.stringValue, filePathAttribute);
        }

        private void DrawObjectField(
            Rect fileRect,
            GUIContent label,
            SerializedProperty uriProperty,
            FilePathReferenceFieldAttribute filePathAttribute)
        {
            Object newObjectReference;

            EditorGUI.BeginChangeCheck();
            {
                label.text = filePathAttribute.displayLabel + " File";
                label.tooltip = "Drag your " + filePathAttribute.displayLabel + " (" +
                                filePathAttribute.fileEnding + "-file) here.";

                newObjectReference = EditorGUI.ObjectField(
                    fileRect,
                    label,
                    GetObjectFromURI(uriProperty.stringValue),
                    typeof(Object),
                    false);
            }
            if (EditorGUI.EndChangeCheck())
            {
                uriProperty.stringValue = GetURIFromObject(newObjectReference);
            }
        }

        private static void DrawStringField(
            Rect fileRect,
            GUIContent label,
            SerializedProperty uriProperty,
            FilePathReferenceFieldAttribute filePathAttribute)
        {
            label.text = filePathAttribute.displayLabel + " File";
            label.tooltip = "URI of the used " + filePathAttribute.displayLabel + " (" +
                            filePathAttribute.fileEnding + "-file).";

            uriProperty.stringValue = EditorGUI.TextField(fileRect, label, uriProperty.stringValue);
        }

        private static void DrawFieldTypeDropdown(
            Rect dropDownRect,
            SerializedProperty fieldTypeProperty)
        {
            EditorGUI.PropertyField(dropDownRect, fieldTypeProperty, new GUIContent());
        }

        private void DrawHelpBox(string uri, FilePathReferenceFieldAttribute filePathAttribute)
        {
            if (uri == "")
            {
                if (filePathAttribute.mandatory)
                {
                    EditorGUILayout.HelpBox(
                        "The " + filePathAttribute.displayLabel + " must not be empty.",
                        MessageType.Error);
                }
                return;
            }

            if (!filePathAttribute.allowProjectDir &&
                (uri.StartsWith("project_dir:") || uri.StartsWith("project-dir:")))
            {
                EditorGUILayout.HelpBox(
                    "The " + filePathAttribute.displayLabel +
                    " can not refer to the project-dir scheme.",
                    MessageType.Error);
                return;
            }

            if (!PathHelper.IsAbsolutePath(uri) && !uri.StartsWith("Assets/"))
            {
                EditorGUILayout.HelpBox(
                    "Relative paths are not supported." +
                    "\nUse an absolute path or an URI scheme instead, e.g '" +
                    "streaming-assets-dir:" + uri + "'.",
                    MessageType.Error);
                return;
            }

            var pathWithoutQuery = QueryHelper.RemoveEntireQueryString(uri);

            if (!pathWithoutQuery.EndsWith(filePathAttribute.fileEnding))
            {
                EditorGUILayout.HelpBox(
                    "The " + filePathAttribute.displayLabel + " should have the file extension '" +
                    filePathAttribute.fileEnding + "'.",
                    MessageType.Warning);
            }

            var unifiedPath = EditorPathUtility.ResolveStreamingAssetsScheme(uri);

            if (!unifiedPath.StartsWith("Assets/"))
            {
                if (unifiedPath.StartsWith("project_dir:") ||
                    unifiedPath.StartsWith("project-dir:"))
                {
                    EditorGUILayout.HelpBox(
                        "Can not display file references that contain the project-dir scheme.",
                        MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "The given path is not referencing an object inside the Unity project (" +
                        unifiedPath + ").",
                        MessageType.Warning);
                }
            }
            else
            {
                if (GetObjectFromURI(uri) == null)
                {
                    EditorGUILayout.HelpBox(
                        "Could not find an object at the given path (" + unifiedPath + ").",
                        MessageType.Warning);
                }
                else if (!unifiedPath.StartsWith("Assets/StreamingAssets/"))
                {
                    EditorGUILayout.HelpBox(
                        "The given file is not lying inside the StreamingAssets folder." +
                        "\nMove it there to ensure it can be loaded by VisionLib.",
                        MessageType.Warning);
                }
            }

            var queryMap = QueryHelper.GetAllQueryParametersAsMap(uri);
            if (queryMap.Count <= 0)
            {
                return;
            }
            var fileURIMessage = "File URI: " + QueryHelper.RemoveEntireQueryString(uri) + "\n\n";

            var queryString = string.Join(
                "\n",
                queryMap.Select(x => "    " + x.Key + " = " + x.Value));
            var queryMessage = "Query Parameters:\n" + queryString;

            EditorGUILayout.HelpBox(fileURIMessage + queryMessage, MessageType.Info);
        }

        private Object GetObjectFromURI(string path)
        {
            string unifiedPath =
                EditorPathUtility.ResolveStreamingAssetsScheme(
                    QueryHelper.RemoveEntireQueryString(path));

            return this.cachedObjectReferenceDatabase.LoadObjectFromURI(unifiedPath);
        }

        private static string GetURIFromObject(Object objectReference)
        {
            string objectPath = AssetDatabase.GetAssetPath(objectReference);
            return EditorPathUtility.ReplaceStreamingAssetsPrefixWithScheme(objectPath);
        }
    }
}
