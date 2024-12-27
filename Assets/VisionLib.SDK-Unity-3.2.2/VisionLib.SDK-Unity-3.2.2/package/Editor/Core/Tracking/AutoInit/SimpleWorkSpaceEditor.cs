using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Editor script modifying and displaying relevant WorkSpace values.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SimpleWorkSpace))]
    public class SimpleWorkSpaceEditor : WorkSpaceEditor
    {
        private Transform transform;
        private bool showConstraints = false;
        private SphereHandle sphereHandle = new SphereHandle(false);

        private SerializedProperty sourceGeometryProperty;

        protected override void Reset()
        {
            base.Reset();
            this.transform = base.workSpace.transform;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Disable Transform component of the WorkSpace
            if (this.transform)
            {
                this.transform.hideFlags = HideFlags.NotEditable;
            }
            Tools.hidden = true;

            InitializeSerializedProperties();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Re-enable Transform component of the WorkSpace
            if (this.transform)
            {
                this.transform.hideFlags = HideFlags.None;
            }
            Tools.hidden = false;
        }

        protected override void OnSceneGUI()
        {
            if (workSpace.destinationObject == null)
            {
                return;
            }
            base.OnSceneGUI();

            SimpleSphereGeometry sphere = (SimpleSphereGeometry) base.workSpace.GetSourceGeometry();

            var radius = (target as SimpleWorkSpace).GetSimpleSphereRadius();
            if (radius == 0)
            {
                radius = 0.1f;
            }
            var originTransform = workSpace.GetRootTransform().localToWorldMatrix *
                                  base.workSpace.GetWorkSpaceDefinition(false).GetOriginTransform();

            this.sphereHandle.Draw(
                ref sphere.phiStart,
                ref sphere.phiLength,
                ref sphere.thetaStart,
                ref sphere.thetaLength,
                ref radius,
                originTransform);

            UpdateMeshes();
        }

        private void InitializeSerializedProperties()
        {
            this.sourceGeometryProperty = serializedObject.FindProperty("sourceGeometry");
        }

        protected override void DisplaySource()
        {
            this.showConstraints = EditorGUILayout.Foldout(
                this.showConstraints,
                "Sphere Constraints");
            if (this.showConstraints)
            {
                EditorGUILayout.PropertyField(this.sourceGeometryProperty);
            }
            this.serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateMeshes()
        {
            var useCameraRotation = false;
            this.poses = base.workSpace.GetWorkSpaceDefinition(useCameraRotation)
                .GetCameraTransforms();

            var cameraPositions = base.workSpace.GetWorkSpaceDefinition(useCameraRotation)
                .GetCameraPositions();
            base.workSpace.GetSourceGeometry().SetMesh(cameraPositions);
        }

        [DrawGizmo(GizmoType.Pickable | GizmoType.Selected)]
        private static void DrawGizmos(SimpleWorkSpace workSpace, GizmoType gizmoType)
        {
            if (workSpace.destinationObject == null)
            {
                return;
            }
            var originTransform = workSpace.GetRootTransform().localToWorldMatrix;
            WorkSpaceEditor.DrawGizmos(workSpace, originTransform);
        }
    }
}
