using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    [CustomEditor(typeof(HoloLensInstructionPanel))]
    public class HoloLensInstructionPanelEditor : Editor
    {
        private HoloLensInstructionPanel holoLensInstructionPanel;

        private void OnEnable()
        {
            this.holoLensInstructionPanel =
                this.serializedObject.targetObject as HoloLensInstructionPanel;
        }

        public override void OnInspectorGUI()
        {
            this.holoLensInstructionPanel.GetSceneIssues().Draw();

            DrawDefaultInspector();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
