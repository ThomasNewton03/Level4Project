using System.IO;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.Details
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    internal static class TrackingAnchorEditorPersistence
    {
        private static readonly string foldoutPersistentPath = Path.Combine(
            Application.temporaryCachePath,
            "TrackingAnchorEditor_Foldout_persistence.json");

        static TrackingAnchorEditorPersistence()
        {
            EditorApplication.playModeStateChanged += PersistAnchorEditorFoldouts;
        }

        private static void PersistAnchorEditorFoldouts(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    RestoreEditorFoldoutValues();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    StoreEditorFoldoutValues();
                    break;
            }
        }

        private static void RestoreEditorFoldoutValues()
        {
            var result = VLSDK.Get(TrackingAnchorEditorPersistence.foldoutPersistentPath);
            TrackingAnchorEditor.state = JsonUtility.FromJson<TrackingAnchorEditor.FoldoutValues>(result);
        }

        private static void StoreEditorFoldoutValues()
        {
            var content = JsonUtility.ToJson(TrackingAnchorEditor.state);
            VLSDK.Set(TrackingAnchorEditorPersistence.foldoutPersistentPath, content);
        }
    }
#endif
}
