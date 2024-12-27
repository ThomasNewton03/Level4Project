using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Editor script modifying and displaying relevant WorkSpaceGeometry values.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WorkSpaceGeometry))]
    public class WorkSpaceGeometryEditor : Editor
    {
        private WorkSpaceGeometry geometry;

        private SphereHandle sphereHandle = new SphereHandle(true);
        private static float wireSphereSize = 0.125f;

        private SerializedProperty shapeProperty;
        private SerializedProperty sphereProperty;
        private SerializedProperty planeProperty;
        private SerializedProperty lineProperty;

        private void Reset()
        {
            this.geometry = this.target as WorkSpaceGeometry;
            UpdateGeometryMesh();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UpdateGeometryMesh;

            this.shapeProperty = serializedObject.FindProperty("shape");
            this.sphereProperty = serializedObject.FindProperty("sphere");
            this.planeProperty = serializedObject.FindProperty("plane");
            this.lineProperty = serializedObject.FindProperty("line");
        }

        private void OnDisable()
        {
            if (this.serializedObject.targetObject)
            {
                this.serializedObject.ApplyModifiedProperties();
            }
            Undo.undoRedoPerformed -= UpdateGeometryMesh;
        }

        private void OnSceneGUI()
        {
            if (this.geometry == null)
            {
                return;
            }

            SceneView sceneView = SceneView.currentDrawingSceneView;

            if (Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "FrameSelected")
                {
                    Event.current.Use();
                    sceneView.LookAt(
                        this.geometry.transform.position,
                        sceneView.rotation,
                        this.geometry.GetGeometry().GetGeometrySize());
                }
            }
            if (this.geometry.shape == WorkSpaceGeometry.Shape.Sphere)
            {
                this.sphereHandle.Draw(
                    ref this.geometry.sphere.phiStart,
                    ref this.geometry.sphere.phiLength,
                    ref this.geometry.sphere.thetaStart,
                    ref this.geometry.sphere.thetaLength,
                    ref this.geometry.sphere.radius,
                    this.geometry.transform.localToWorldMatrix);
                this.geometry.GetGeometry().UpdateMesh();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!this.geometry)
            {
                Reset();
            }

            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(geometry, "VLGeometry");

            DisplayGeometryShapeDropDown();
            DisplayGeometryValues();

            if (EditorGUI.EndChangeCheck())
            {
                UpdateGeometryMesh();
                EditorUtility.SetDirty(this.geometry);
                if (!EditorApplication.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                        this.geometry.gameObject.scene);
                }
            }
        }

        private void DisplayGeometryValues()
        {
            EditorGUILayout.BeginVertical();

            WorkSpaceGeometry.Shape shape =
                (WorkSpaceGeometry.Shape) this.shapeProperty.enumValueIndex;

            switch (shape)
            {
                case WorkSpaceGeometry.Shape.Sphere:
                    EditorGUILayout.PropertyField(this.sphereProperty);
                    break;
                case WorkSpaceGeometry.Shape.Plane:
                    EditorGUILayout.PropertyField(this.planeProperty);
                    break;
                case WorkSpaceGeometry.Shape.Line:
                    EditorGUILayout.PropertyField(this.lineProperty);
                    break;
                case WorkSpaceGeometry.Shape.Point:
                    break;
                default:
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void DisplayGeometryShapeDropDown()
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(this.shapeProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdateGeometryMesh();
            }
        }

        /// <summary>
        /// Updates the stored positions of the geometry.
        /// </summary>
        private void UpdateGeometryMesh()
        {
            serializedObject.ApplyModifiedProperties();
            this.geometry.GetGeometry().UpdateMesh();
        }

        /// <summary>
        /// Draw vertices if a geometry creator object is selected.
        /// Draw them white, if it's the Origin, blue if it's the Destination.
        /// </summary>
        [DrawGizmo(GizmoType.Pickable | GizmoType.Selected)]
        public static void DrawSingleSelectedGeometryGizmos(
            WorkSpaceGeometry geometry,
            GizmoType gizmoType)
        {
            Color paintColor = Selection.activeObject.name == "Origin" ? Color.white :
                Selection.activeObject.name == "Destination" ? Color.cyan : Color.grey;

            PaintVertices(geometry.GetGeometry().currentMesh, paintColor, geometry.transform.localToWorldMatrix);
        }

        /// <summary>
        /// Draw wiresphere at given coordinates.
        /// </summary>
        /// <param name="vertices">Points to be drawn</param>
        /// <param name="paintColor">Gizmo color</param>
        /// <param name="trans">Transform applied to the points before drawing</param>
        public static void PaintVertices(
            Vector3[] vertices,
            Color paintColor,
            Matrix4x4 trans)
        {
            if (vertices == null)
            {
                return;
            }

            Gizmos.color = paintColor;

            foreach (Vector3 vertPos in vertices)
            {
                Vector3 targetPosition = trans.MultiplyPoint(vertPos);

                Gizmos.DrawWireSphere(
                    targetPosition,
                    HandleUtility.GetHandleSize(targetPosition) *
                    WorkSpaceGeometryEditor.wireSphereSize);
            }
        }
    }
}
