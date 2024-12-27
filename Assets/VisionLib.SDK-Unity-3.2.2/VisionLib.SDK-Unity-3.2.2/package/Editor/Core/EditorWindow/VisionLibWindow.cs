using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Base Editor Window containing the VisionLib Logo.
    /// </summary>
    public abstract class VisionLibWindow : EditorWindow
    {
        private static Texture2D vlLogo;
        protected const int verticalSpace = 10;
        protected const int horizontalSpace = 10;

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(VisionLibWindow.horizontalSpace);
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawVisionLibLogo();
                    DrawContent();
                }
                GUILayout.Space(VisionLibWindow.horizontalSpace);
            }
        }

        protected abstract void DrawContent();

        private static void DrawVisionLibLogo()
        {
            if (!VisionLibWindow.vlLogo)
            {
                VisionLibWindow.vlLogo = LoadVLLogo();
            }
            var iconStyle = new GUIStyle() {margin = new RectOffset(20, 0, 0, 0)};
            GUILayout.Space(VisionLibWindow.verticalSpace);
            GUILayout.Label(VisionLibWindow.vlLogo, iconStyle);
            GUILayout.Space(VisionLibWindow.verticalSpace);
        }

        private static Texture2D LoadVLLogo()
        {
            if (EditorGUIUtility.isProSkin)
            {
                return Resources.Load("VLLogo_170x36_DarkTheme", typeof(Texture2D)) as Texture2D;
            }
            return Resources.Load("VLLogo_170x36_LightTheme", typeof(Texture2D)) as Texture2D;
        }
    }
}
