using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public class JsonInputField<TTargetType> where TTargetType : class, JsonHelper.IJsonParsable
    {
        private string textAreaString;

        private readonly string title;
        private readonly string parseResultTooltip;
        private readonly Func<string, TTargetType> parseFunction;
        private readonly string documentationURL;
        private readonly Action<TTargetType> applyFunction;

        public JsonInputField(
            string title,
            string parseResultTooltip,
            Func<string, TTargetType> parseFunction,
            string documentationURL,
            Action<TTargetType> applyFunction)
        {
            this.title = title;
            this.parseResultTooltip = parseResultTooltip;
            this.parseFunction = parseFunction;
            this.documentationURL = documentationURL;
            this.applyFunction = applyFunction;
        }

        public void Draw()
        {
            try
            {
                var result = DrawJsonEntryField();
                DrawAcceptanceField(result);
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox(e.Message,  MessageType.Error);
            }
        }

        private TTargetType DrawJsonEntryField()
        {
            EditorGUILayout.LabelField(this.title, EditorStyles.wordWrappedLabel);
            this.textAreaString = EditorGUILayout.TextArea(this.textAreaString);
            if (string.IsNullOrEmpty(this.textAreaString))
            {
                return null;
            }

            var jsonString = JsonHelper.ConditionJson(this.textAreaString);

            var result = this.parseFunction(jsonString);
            if (result.IsValid())
            {
                EditorGUILayout.HelpBox(
                    new GUIContent(
                        $"Parsed {result.GetJsonName()}\n" + "------------\n" + result,
                        this.parseResultTooltip));
            }
            else
            {
                EditorGUILayout.HelpBox("Invalid data format!\nFor the correct format, see the corresponding VisionLib documentation.", MessageType.Warning);
                ButtonParameters.DrawLinkButton("Open Documentation", "", this.documentationURL);
            }

            var warning = result.GetWarning();
            if (!string.IsNullOrEmpty(warning))
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            return result;
        }
        
        private void DrawAcceptanceField(TTargetType result)
        {
            if (result == null || !result.IsValid())
            {
                return;
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15);
                if (GUILayout.Button($"Apply {result.GetJsonName()} from JSON"))
                {
                    this.applyFunction(result);
                    // As long as the TextArea has focus, we can not change its content.
                    GUI.FocusControl(null);
                    this.textAreaString = null;
                }
            }
        }
    }
}
