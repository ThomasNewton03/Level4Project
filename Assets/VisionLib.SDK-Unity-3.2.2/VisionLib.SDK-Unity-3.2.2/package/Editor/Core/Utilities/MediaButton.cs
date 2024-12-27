using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// GUI class to display the media buttons
    /// </summary>
    public static class MediaButton
    {
        private class Config
        {
            public readonly string text;
            public readonly Texture2D iconLight;
            public readonly Texture2D iconDark;

            public Config(string text, string textureName)
            {
                this.text = text;
                this.iconLight = Resources.Load(textureName, typeof(Texture2D)) as Texture2D;
                this.iconDark = Resources.Load("d_" + textureName, typeof(Texture2D)) as Texture2D;
            }
        }

        private static readonly Dictionary<ImageSequenceParameters.PlayBack, Config> configs = new()
        {
            {
                ImageSequenceParameters.PlayBack.FastReverse,
                new Config("Reverse (5x)", "ScrubBackwardsFaster_Icon")
            },
            {
                ImageSequenceParameters.PlayBack.Reverse,
                new Config("Reverse (1x)", "ScrubBackwards_Icon")
            },
            {ImageSequenceParameters.PlayBack.Pause, new Config("Pause", "Pause_Icon")},
            {
                ImageSequenceParameters.PlayBack.Normal,
                new Config("Forwards (1x)", "ScrubForwards_Icon")
            },
            {
                ImageSequenceParameters.PlayBack.FastForward,
                new Config("Forwards (5x)", "ScrubForwardsFaster_Icon")
            }
        };

        private const string hoverBackgroundIcon = "MediaButton_Background_Hover_Icon";
        private const string normalBackgroundIcon = "MediaButton_Background_Icon";

        private static Texture2D LoadButtonIcon(string buttonTextureName)
        {
            var path = EditorGUIUtility.isProSkin ? $"d_{buttonTextureName}" : buttonTextureName;
            return Resources.Load(path, typeof(Texture2D)) as Texture2D;
        }

        private static readonly GUIStyle mediaButtonStyle = new(GUIStyle.none)
        {
            fixedWidth = 24f,
            fixedHeight = 24f,
            padding = new RectOffset(3, 3, 3, 3),
            hover =
                new GUIStyleState()
                {
                    background = LoadButtonIcon(MediaButton.hoverBackgroundIcon)
                },
            normal = new GUIStyleState()
            {
                background = LoadButtonIcon(MediaButton.normalBackgroundIcon)
            }
        };

        private static bool DrawMediaButton(bool isActive, Config config)
        {
            var lastBackgroundColor = GUI.backgroundColor;
            if (isActive)
            {
                GUI.backgroundColor = new Color(0.172f, 0.349f, 0.549f);
            }
            var icon = isActive || EditorGUIUtility.isProSkin ? config.iconDark : config.iconLight;
            var result = GUILayout.Button(
                new GUIContent(icon, config.text),
                MediaButton.mediaButtonStyle,
                GUILayout.Width(32f));
            GUI.backgroundColor = lastBackgroundColor;
            return result;
        }

        public static ImageSequenceParameters.PlayBack? DrawAll(
            ImageSequenceParameters.PlayBack currentState,
            bool enabled)
        {
            ImageSequenceParameters.PlayBack? newState = null;
            foreach (var (state, config) in MediaButton.configs)
            {
                var active = enabled && state == currentState;
                if (DrawMediaButton(active, config))
                {
                    newState = state;
                }
            }
            return enabled ? newState : null;
        }
    }
}
