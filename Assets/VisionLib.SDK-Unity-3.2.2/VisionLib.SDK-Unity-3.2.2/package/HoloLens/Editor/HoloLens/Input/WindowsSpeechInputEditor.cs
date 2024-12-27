using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    [CustomEditor(typeof(WindowsSpeechInput))]
    public class WindowsSpeechInputEditor : Editor
    {
        private WindowsSpeechInput windowsSpeechInput;

        private void OnEnable()
        {
            this.windowsSpeechInput =
                this.serializedObject.targetObject as WindowsSpeechInput;
        }

        public override void OnInspectorGUI()
        {
            this.windowsSpeechInput.GetSceneIssues().Draw();

            DrawDefaultInspector();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
