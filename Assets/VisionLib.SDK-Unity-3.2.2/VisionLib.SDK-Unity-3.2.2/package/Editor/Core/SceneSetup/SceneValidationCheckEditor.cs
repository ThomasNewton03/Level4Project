using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    public class SceneValidationCheckEditor : Editor
    {
        private ISceneValidationCheck setupIssues;

        private void OnEnable()
        {
            this.setupIssues = this.serializedObject.targetObject as ISceneValidationCheck;
        }

        public override void OnInspectorGUI()
        {
            SetupIssueEditorHelper.DrawErrorBox(this.setupIssues);
            this.setupIssues.GetSceneIssues().Draw();

            DrawDefaultInspector();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
