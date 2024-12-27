using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Editor script modifying and displaying relevant WorkSpace values.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AdvancedWorkSpace))]
    public class AdvancedWorkSpaceEditor : WorkSpaceEditor
    {
        private SerializedProperty sourceObjectProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeSerializedProperties();
        }

        private void InitializeSerializedProperties()
        {
            this.sourceObjectProperty = serializedObject.FindProperty("sourceObject");
        }

        protected override void DisplaySource()
        {
            TrackingObject trackingObject = null;
            if (base.workSpace.destinationObject)
            {
                trackingObject = base.workSpace.destinationObject.GetComponent<TrackingObject>();
            }
            using (new EditorGUI.DisabledScope(
                this.sourceObjectProperty.objectReferenceValue != null))
            {
                EditorGUILayout.PropertyField(this.sourceObjectProperty);
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateMeshes()
        {
            var useCameraRotation = false;
            base.poses = base.workSpace.GetWorkSpaceDefinition(useCameraRotation)
                .GetCameraTransforms();
            base.workSpace.GetSourceGeometry().UpdateMesh();
        }

        [DrawGizmo(GizmoType.Pickable | GizmoType.Selected)]
        private static void DrawGizmos(AdvancedWorkSpace workSpace, GizmoType gizmoType)
        {
            if (workSpace.sourceObject == null)
            {
                return;
            }

            WorkSpaceEditor.DrawGizmos(
                workSpace,
                workSpace.sourceObject.transform.localToWorldMatrix);
        }
    }
}
