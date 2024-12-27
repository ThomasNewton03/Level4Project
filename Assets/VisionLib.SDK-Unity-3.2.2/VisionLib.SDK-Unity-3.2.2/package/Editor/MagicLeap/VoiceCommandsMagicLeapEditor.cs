using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.MagicLeap
{
    [CustomEditor(typeof(VoiceCommandsMagicLeap))]
    public class VoiceCommandsMagicLeapEditor : Editor
    {
        private VoiceCommandsMagicLeap voiceCommands;

        private void OnEnable()
        {
            this.voiceCommands =
                this.serializedObject.targetObject as VoiceCommandsMagicLeap;
        }

        public override void OnInspectorGUI()
        {
            this.voiceCommands.GetSceneIssues().Draw();

            DrawDefaultInspector();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
