using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Base Editor Window containing the VisionLib Logo and Dialog buttons.
    /// </summary>
    public abstract class VisionLibDialogWindow : VisionLibWindow
    {
        private const string infoDeclineText = "Cancel";
        private const int buttonMinWidth = 80;
        private const float charHeightFactor = 0.35f;
        protected const float lineHeightFactor = 15f;
        protected const int tabbingSpace = 5;
        protected const float baseWindowHeight = 125f;
        protected const int windowWidth = 500;

        protected string dialogMessage = "";
        protected string infoAcceptText = "Continue";
        protected bool disableAcceptButton = false;
        protected bool dialogResult = false;

        protected abstract float CalculateWindowHeightFromContent();

        protected abstract void DrawAdditionalContent();

        protected virtual void DrawAdditionalOptions() {}

        protected override void DrawContent()
        {
            EditorGUILayout.LabelField(this.dialogMessage, EditorStyles.wordWrappedLabel);
            DrawAdditionalContent();
            GUILayout.FlexibleSpace();
            DrawDialogButtons();
        }

        protected void Resize()
        {
            var windowHeight = CalculateWindowHeightFromContent();
            this.minSize = new Vector2(VisionLibDialogWindow.windowWidth, windowHeight);
            this.maxSize = new Vector2(VisionLibDialogWindow.windowWidth, windowHeight);
        }

        protected float CalculateBaseWindow()
        {
            return VisionLibDialogWindow.baseWindowHeight +
                   this.dialogMessage.Length * VisionLibDialogWindow.charHeightFactor;
        }

        private void DrawAcceptField()
        {
            using (new EditorGUI.DisabledScope(this.disableAcceptButton))
            {
                if (GUILayout.Button(
                        this.infoAcceptText,
                        GUILayout.MinWidth(VisionLibDialogWindow.buttonMinWidth),
                        GUILayout.ExpandWidth(true)))
                {
                    this.dialogResult = true;
                    Close();
                }
            }
        }

        private void DrawCancelField()
        {
            if (GUILayout.Button(
                    VisionLibDialogWindow.infoDeclineText,
                    GUILayout.MinWidth(VisionLibDialogWindow.buttonMinWidth),
                    GUILayout.ExpandWidth(true)))
            {
                this.dialogResult = false;
                Close();
            }
        }

        private void DrawDialogButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(VisionLibDialogWindow.tabbingSpace);

                DrawAdditionalOptions();

                GUILayout.FlexibleSpace();
                DrawAcceptField();
                DrawCancelField();
                GUILayout.Space(VisionLibWindow.horizontalSpace);
            }
            GUILayout.Space(VisionLibWindow.verticalSpace);
        }
    }
}
