using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Editor script modifying and displaying relevant WorkSpace values.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WorkSpace))]
    public abstract class WorkSpaceEditor : Editor
    {
        protected WorkSpace workSpace;
        protected const int maxLinesToDraw = 600;

        protected API.WorkSpace.Transform[] poses = new API.WorkSpace.Transform[0];
        private TransformCache posesTransform;

        private SerializedProperty destinationObjectProperty;
        private SerializedProperty upVectorProperty;
        private SerializedProperty camSliderPositionProperty;
        private SerializedProperty displayViewDirectionProperty;
        private SerializedProperty usedCameraProperty;

        private void Initialize()
        {
            InitializeSerializedProperties();
            this.workSpace = target as WorkSpace;
            this.posesTransform = new TransformCache();
            UpdatePoses();
            this.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void Reset()
        {
            Initialize();
        }

        protected virtual void OnEnable()
        {
            Initialize();
            Undo.undoRedoPerformed += UpdatePoses;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= UpdatePoses;
        }

        private void InitializeSerializedProperties()
        {
            this.destinationObjectProperty = serializedObject.FindProperty("destinationObject");
            this.upVectorProperty = serializedObject.FindProperty("upVector");
            this.camSliderPositionProperty = serializedObject.FindProperty("camSliderPosition");
            this.displayViewDirectionProperty =
                serializedObject.FindProperty("displayViewDirection");
            this.usedCameraProperty = serializedObject.FindProperty("usedCamera");
        }

        /// <summary>
        /// Update the poses for the WorkSpace by taking the poses from vlSDK.
        /// Reset camSliderPosition if it's greater than the number of poses.
        /// </summary>
        protected void UpdatePoses()
        {
            if (this.workSpace.GetSourceGeometry() == null ||
                this.workSpace.destinationObject == null)
            {
                return;
            }

            UpdateMeshes();

            if (this.camSliderPositionProperty.intValue > this.poses.Length)
            {
                this.camSliderPositionProperty.intValue = 0;
            }
        }

        protected virtual void OnSceneGUI()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;

            if (Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "FrameSelected")
                {
                    Event.current.Use();
                    sceneView.LookAt(
                        this.workSpace.GetCenter(),
                        sceneView.rotation,
                        this.workSpace.GetSize());
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (!this.workSpace || this.poses == null)
            {
                Reset();
            }

            EditorGUI.BeginChangeCheck();
            this.workSpace.GetSceneIssues().Draw();
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePoses();
            }

            DisplayInspectorParamaters();
        }

        private void DisplayInspectorParamaters()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", CustomEditorStyles.boldWithWrappedLabel);

            EditorGUILayout.PropertyField(this.usedCameraProperty);
            DrawUpVector();
            DrawDestinationObject();
            DisplaySource();
            DisplayHelpBox();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", CustomEditorStyles.boldWithWrappedLabel);

            DisplayViewDirections();
            DisplayPreviewInCamera();

            // apply if any parameter has been changed
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                UpdatePoses();
            }
        }

        private void DrawUpVector()
        {
            EditorGUILayout.PropertyField(this.upVectorProperty);

            if (this.upVectorProperty.vector3Value == Vector3.zero)
            {
                EditorGUILayout.HelpBox(
                    "Up Vector can not be (0, 0, 0). Set a different up vector.",
                    MessageType.Error);
            }
        }

        private void DrawDestinationObject()
        {
            EditorGUILayout.PropertyField(this.destinationObjectProperty);
        }

        protected abstract void DisplaySource();

        private void DisplayViewDirections()
        {
            bool isNumberOfLinesHigherThanMax =
                this.workSpace.GetVerticesCount() > WorkSpaceEditor.maxLinesToDraw;
            string toolTip = isNumberOfLinesHigherThanMax
                ? this.displayViewDirectionProperty.tooltip
                : "Lines will not be displayed if there are too many poses";

            using (new EditorGUI.DisabledScope(
                       !this.workSpace.destinationObject || isNumberOfLinesHigherThanMax))
            {
                EditorGUILayout.PropertyField(
                    this.displayViewDirectionProperty,
                    new GUIContent(this.displayViewDirectionProperty.displayName, toolTip));
            }
        }

        private void DisplayPreviewInCamera()
        {
            if (EditorApplication.isPlaying || this.poses.Length < 1 ||
                this.usedCameraProperty.objectReferenceValue == null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.IntSlider(
                        new GUIContent(
                            "Switch through Poses: ",
                            "Can only preview poses when the used camera is set"),
                        0,
                        0,
                        1);
                }
                return;
            }

            EditorGUILayout.BeginVertical();
            if (this.poses.Length == 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Preview Pose in Camera: ");
                if (GUILayout.Button("Preview Pose"))
                {
                    SetPreviewCamera(
                        this.camSliderPositionProperty.intValue,
                        this.usedCameraProperty.objectReferenceValue as Camera);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                this.camSliderPositionProperty.intValue = EditorGUILayout.IntSlider(
                    new GUIContent(
                        "Switch through Poses: ",
                        "Use this slider to switch the camera preview through your set poses"),
                    this.camSliderPositionProperty.intValue,
                    0,
                    this.poses.Length - 1);

                if (EditorGUI.EndChangeCheck())
                {
                    SetPreviewCamera(
                        this.camSliderPositionProperty.intValue,
                        this.usedCameraProperty.objectReferenceValue as Camera);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DisplayHelpBox()
        {
            EditorGUILayout.Space();

            if (this.poses.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    this.poses.Length == 1
                        ? "Will generate one pose."
                        : "Will generate around " + this.poses.Length + " Poses" +
                          (this.poses.Length > 3000
                              ? " Having more than 3.000 poses may lead to performance issues."
                              : ""),
                    this.poses.Length > 3000 ? MessageType.Warning : MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Will generate 0 Poses. Set a valid source and destination object.",
                    MessageType.Error);
            }
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Set preview camera to one of the poses regarding the camStepPosition.
        /// </summary>
        /// <param name="camStepPos">camera step index</param>
        private void SetPreviewCamera(int camStepPos, Camera previewCamera)
        {
            if (this.poses.Length <= camStepPos || previewCamera == null)
            {
                return;
            }

            previewCamera.enabled = true;

            if (this.posesTransform.UpdateTransform(new ModelTransform(this.workSpace.transform)))
            {
                UpdatePoses();
            }

            var r = this.poses[camStepPos].r;
            var t = this.poses[camStepPos].t;
            Quaternion orientation;
            Vector3 position;

            CameraHelper.VLPoseToCamera(
                new Vector3(t[0], t[1], t[2]),
                new Quaternion(r[0], r[1], r[2], r[3]),
                out position,
                out orientation);

            var rootTransform = this.workSpace.GetRootTransform();
            if (rootTransform)
            {
                var localTransform = rootTransform.localToWorldMatrix *
                                     new ModelTransform(orientation, position);
                orientation = localTransform.r;
                position = localTransform.t;
            }
            previewCamera.transform.position = position;
            previewCamera.transform.rotation = orientation;
        }

        /// <summary>
        /// Draw source and destination points (PaintVertices).
        /// Draw Lines (PaintLines) if that option is enabled in WorkSpace.
        /// </summary>
        protected static void DrawGizmos(WorkSpace workSpace, Matrix4x4 sourceTransform)
        {
            Vector3[] sourceVertices = workSpace.GetSourceGeometry().currentMesh;
            Vector3[] destinationVertices = workSpace.GetDestinationVertices();

            WorkSpaceGeometryEditor.PaintVertices(sourceVertices, Color.white, sourceTransform);

            if (workSpace.destinationObject != null)
            {
                Handles.Label(
                    workSpace.destinationObject.transform.TransformPoint(destinationVertices[0]),
                    "Target");
                WorkSpaceGeometryEditor.PaintVertices(
                    destinationVertices,
                    Color.cyan,
                    workSpace.destinationObject.transform.localToWorldMatrix);

                if (workSpace.displayViewDirection &&
                    workSpace.GetVerticesCount() < WorkSpaceEditor.maxLinesToDraw)
                {
                    PaintLines(
                        sourceVertices,
                        destinationVertices,
                        sourceTransform,
                        workSpace.destinationObject.transform);
                }
            }

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Draw Lines from all source vertices to all destination vertices
        /// as long as the number of lines to draw is smaller than maxLinesToDraw.
        /// </summary>
        private static void PaintLines(
            Vector3[] sourceVertices,
            Vector3[] destinationVertices,
            Matrix4x4 sourceTransform,
            Transform destinationTransform)
        {
            foreach (Vector3 sourcePos in sourceVertices)
            {
                foreach (Vector3 targetPos in destinationVertices)
                {
                    Vector3 sourcePosition = sourceTransform.MultiplyPoint(sourcePos);
                    Handles.DrawDottedLine(
                        sourcePosition,
                        destinationTransform.TransformPoint(targetPos),
                        7f);
                }
            }
        }

        protected abstract void UpdateMeshes();
    }
}
