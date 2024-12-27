using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    [CustomEditor(typeof(KeyboardInput))]
    public class KeyboardInputEditor : Editor
    {
        private KeyboardInput keyboardInput;

        private void OnEnable()
        {
            this.keyboardInput =
                this.serializedObject.targetObject as KeyboardInput;
        }

        public override void OnInspectorGUI()
        {
            this.keyboardInput.GetSceneIssues().Draw();

            DrawDefaultInspector();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
