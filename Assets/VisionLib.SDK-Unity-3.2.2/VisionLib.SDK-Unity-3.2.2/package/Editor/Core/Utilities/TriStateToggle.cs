using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Tri-State Toggle extension of the default Unity Toggle to also display a 'Indeterminate'
    /// state within the toggle box.
    /// </summary>
    public static class TriStateToggle
    {
        public static bool DrawTriStateToggle(Rect rect, TriState toggleState)
        {
            EditorGUI.BeginChangeCheck();
            switch (toggleState)
            {
                case TriState.False:
                    EditorGUI.Toggle(rect, false);
                    break;
                case TriState.True:
                    EditorGUI.Toggle(rect, true);
                    break;
                case TriState.Mixed:
                    EditorGUI.showMixedValue = true;
                    EditorGUI.Toggle(rect, true);
                    EditorGUI.showMixedValue = false;
                    break;
            }
            return EditorGUI.EndChangeCheck();
        }
    }
}
