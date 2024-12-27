using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    internal static class CustomEditorStyles
    {
        public static readonly GUIStyle boldWithWrappedLabel =
            new (EditorStyles.boldLabel) {wordWrap = true};
    }
}
