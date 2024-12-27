using System;
using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// GUI class to display a toggle switch for bool and TriState values
    /// </summary>
    public static class ToggleSwitch
    {
        private const string onButtonTextureName = "ButtonToggle_On_Icon";
        private const string offButtonTextureName = "ButtonToggle_Off_Icon";
        private const string mixedButtonTextureName = "ButtonToggle_Intermediate_Icon";

        private static readonly Texture2D onButtonTexture =
            LoadButtonIcon(ToggleSwitch.onButtonTextureName);
        private static readonly Texture2D offButtonTexture =
            LoadButtonIcon(ToggleSwitch.offButtonTextureName);
        private static readonly Texture2D intermediateButtonTexture =
            LoadButtonIcon(ToggleSwitch.mixedButtonTextureName);

        private static readonly GUIStyle toggleSwitchStyle = new(GUIStyle.none)
        {
            fixedWidth = 40f,
            fixedHeight = 20f,
            padding = new RectOffset(0, 0, 0, 0),
            hover = new GUIStyleState()
            {
                background = LoadButtonIcon("ButtonToggle_BackgroundHover_Icon")
            },
            normal = new GUIStyleState()
            {
                background = LoadButtonIcon("ButtonToggle_Background_Icon")
            }
        };

        public static bool DrawToggleSwitch(Rect rect, bool toggleState, string labelText)
        {
            return DrawToggleSwitchButton(rect, GetButtonIcon(toggleState), labelText);
        }

        public static bool DrawToggleSwitch(Rect rect, TriState toggleState, string labelText)
        {
            return DrawToggleSwitchButton(rect, GetButtonIcon(toggleState), labelText);
        }

        private static bool DrawToggleSwitchButton(
            Rect rect,
            Texture2D buttonTexture,
            string labelText)
        {
            return GUI.Button(
                new Rect(new Vector2(rect.position.x, rect.position.y), rect.size),
                new GUIContent(buttonTexture, labelText),
                ToggleSwitch.toggleSwitchStyle);
        }

        private static Texture2D GetButtonIcon(bool toggleState)
        {
            return toggleState ? ToggleSwitch.onButtonTexture : ToggleSwitch.offButtonTexture;
        }

        private static Texture2D GetButtonIcon(TriState toggleState)
        {
            return toggleState switch
            {
                TriState.False => ToggleSwitch.offButtonTexture,
                TriState.True => ToggleSwitch.onButtonTexture,
                TriState.Mixed => ToggleSwitch.intermediateButtonTexture,
                _ => throw new ArgumentOutOfRangeException(nameof(toggleState), toggleState, null)
            };
        }

        private static Texture2D LoadButtonIcon(string buttonTextureName)
        {
            if (EditorGUIUtility.isProSkin)
            {
                return Resources.Load($"d_{buttonTextureName}", typeof(Texture2D)) as Texture2D;
            }
            return Resources.Load(buttonTextureName, typeof(Texture2D)) as Texture2D;
        }
    }
}
