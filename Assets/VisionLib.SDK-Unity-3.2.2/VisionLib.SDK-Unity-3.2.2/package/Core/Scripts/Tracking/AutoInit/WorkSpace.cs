using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This class contains shared properties and functionality of Advanced and Simple WorkSpaces.
    ///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
    /// </summary>
    /// @ingroup WorkSpace
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "work_space.html")]
    [AddComponentMenu("VisionLib/Core/AutoInit/WorkSpace")]
    public abstract class WorkSpace : MonoBehaviour, ISceneValidationCheck
    {
        [Tooltip(
            "Camera that is used to preview the AutoInit poses. " +
            "Attention: Transform of this camera will be changed by this feature!")]
        public Camera usedCamera;

        [FormerlySerializedAs("destinationGeometry")]
        [Tooltip(
            "Use any GameObject from the scene to set the destination to its center or use the destination child of WorkSpace")]
        public GameObject destinationObject;

        [Tooltip("Display dotted lines between all origin and destination points")]
        public bool displayViewDirection = true;

        [SerializeField]
        [Tooltip("The up-Vector of your 3D object")]
        protected Vector3 upVector = Vector3.up;

        [SerializeField]
        private int camSliderPosition;

        protected const float defaultRotationRange = 20.0f;
        protected const float defaultRotationStep = 20.0f;
        private const float defaultFieldOfView = 60.0f;

        private void Awake()
        {
            if (this.usedCamera == null)
            {
                this.usedCamera = CameraProvider.MainCamera;
            }
        }

        /// <summary>
        /// Gets the local positions related to the destination object.
        /// If the destinationObject is a Renderer, it will calculate the center of the model.
        /// </summary>
        /// <returns>Array of local points that represent the object geometry</returns>
        public Vector3[] GetDestinationVertices()
        {
            if (!this.destinationObject)
            {
                return new Vector3[] {};
            }

            // check if object has a geometry component
            WorkSpaceGeometry vlDestinationGeometry =
                this.destinationObject.GetComponent<WorkSpaceGeometry>();
            if (vlDestinationGeometry != null)
            {
                vlDestinationGeometry.GetGeometry().UpdateMesh();
                return vlDestinationGeometry.GetGeometry().currentMesh;
            }

            // if not, we use the renderer
            return new[] {GetCenter(this.destinationObject)};
        }

        private float GetFieldOfView()
        {
            if (!this.usedCamera)
            {
                return defaultFieldOfView;
            }

            float verticalFoV = this.usedCamera.fieldOfView;
            float horizontalFoV = Camera.VerticalToHorizontalFieldOfView(
                verticalFoV,
                this.usedCamera.aspect);
            float usedFieldOfView = Mathf.Min(horizontalFoV, verticalFoV);
            return (usedFieldOfView > 0) ? usedFieldOfView : defaultFieldOfView;
        }

        private float RoundToNearestMultiple(float number, float factor)
        {
            return (float) Math.Round(number / factor) * factor;
        }

        private float GetApproximateFieldOfView()
        {
            const float factor = 0.5f;
            float roundedFov = RoundToNearestMultiple(GetFieldOfView(), factor);
            return Math.Max(roundedFov, factor);
        }

        protected API.WorkSpace.Definition GetWorkSpaceDefinitionFromType(
            API.WorkSpace.Definition.Type type,
            bool useCameraRotation)
        {
            var vlUpVector = CameraHelper.UnityVectorToVLVector(this.upVector);
            if (vlUpVector == Vector3.zero)
            {
                vlUpVector = Vector3.up;
            }

            API.ModelTransform modelTransform = new API.ModelTransform(
                this.gameObject.transform,
                GetRootTransform());
            var workspaceTrafo = new API.WorkSpace.Transform(modelTransform.t, modelTransform.r);
            var currentWorkSpaceDef = new API.WorkSpace.Definition(
                workspaceTrafo,
                vlUpVector,
                useCameraRotation ? defaultRotationRange : 0.0f,
                defaultRotationStep,
                GetApproximateFieldOfView(),
                type);

            currentWorkSpaceDef.origin = GetSourceGeometryDefinition();
            currentWorkSpaceDef.destination = GetDestinationGeometryDefinition();

            return currentWorkSpaceDef;
        }

        public API.WorkSpace.Transform[] GetCameraTransforms()
        {
            bool useCameraRotation = false;
            return this.GetWorkSpaceDefinition(useCameraRotation).GetCameraTransforms();
        }

        public static Vector3 GetCenter(GameObject go)
        {
            return BoundsUtilities.GetMeshBoundsInParentCoordinates(go, go.transform, true).center;
        }

        public float GetOptimalCameraDistance(GameObject destinationForBounds)
        {
            float boundingBoxDiagonal = BoundsUtilities.GetMeshBoundsInParentCoordinates(
                destinationForBounds,
                destinationForBounds.transform,
                true).size.magnitude;
            return boundingBoxDiagonal * 0.5f /
                   Mathf.Sin(GetApproximateFieldOfView() * 0.5f / 180f * Mathf.PI);
        }

        protected static API.WorkSpace.Transform CreateVLTransformFromObject(
            GameObject sourceObject)
        {
            return CameraHelper.CreateVLTransform(sourceObject, false);
        }

        public abstract BaseGeometry GetSourceGeometry();

        protected abstract API.WorkSpace.Geometry GetDestinationGeometryDefinition();

        protected abstract API.WorkSpace.Geometry GetSourceGeometryDefinition();

        public Transform GetRootTransform()
        {
            var parentTrackingAnchor = GetComponentInParent<TrackingAnchor>();
            if (parentTrackingAnchor)
            {
                return parentTrackingAnchor.transform;
            }

            if (!this.destinationObject)
            {
                return null;
            }
            var trackingModel = this.destinationObject.GetComponentInChildren<TrackingObject>();
            return trackingModel != null ? trackingModel.GetRootTransform() : null;
        }

        /// <summary>
        /// Creates a WorkSpace.Definition from this WorkSpace.
        /// </summary>
        /// <returns>WorkSpace.Definition described by this class</returns>
        public abstract API.WorkSpace.Definition GetWorkSpaceDefinition(bool useCameraRotation);

        public abstract Vector3 GetCenter();

        /// <summary>
        /// Calculates the WorkSpace boundaries
        /// using the origin and destination bounds
        /// and the distance between them.
        /// </summary>
        /// <returns>WorkSpace size</returns>
        public abstract float GetSize();

        public abstract int GetVerticesCount();

#if UNITY_EDITOR
        private void MoveGameObject(Transform newParentTransform)
        {
            Undo.RegisterFullObjectHierarchyUndo(
                this.gameObject,
                "Move GameObject to RootTransform");
            this.transform.parent = newParentTransform;
        }

        protected abstract void CreateSourceObject();

        public List<SetupIssue> GetSceneIssues()
        {
            var parentTrackingAnchor = this.gameObject.GetComponentInParent<TrackingAnchor>();
            var trackingAnchorInScene = FindObjectsOfType<TrackingAnchor>();

            var issues = new List<SetupIssue>();
            if (trackingAnchorInScene.Length > 0)
            {
                // Only check for correct placement, if an Anchor is available
                if (GetRootTransform() == null)
                {
                    var moveToTrackingAnchorSolutions = trackingAnchorInScene.Select(
                            anchor => new ReversibleAction(
                                () => MoveGameObject(anchor.transform),
                                this.gameObject,
                                $"Make this WorkSpace child of {anchor.gameObject.name}."))
                        .Cast<ISetupIssueSolution>();

                    issues.Add(
                        new SetupIssue(
                            "Misplaced WorkSpace",
                            "To work correctly, a WorkSpace should be on a child GameObject of a TrackingAnchor.",
                            SetupIssue.IssueType.Error,
                            this.gameObject,
                            moveToTrackingAnchorSolutions.ToArray()));
                }
                else if (this.gameObject.GetComponent<TrackingAnchor>())
                {
                    issues.Add(
                        new SetupIssue(
                            "Misplaced WorkSpace",
                            "To work correctly, a WorkSpace must not be on the same GameObject as the corresponding TrackingAnchor.",
                            SetupIssue.IssueType.Error,
                            this.gameObject,
                            new DestroyComponentAction(this)));
                }
                else if (!parentTrackingAnchor)
                {
                    issues.Add(
                        new SetupIssue(
                            "Misplaced WorkSpace",
                            "To work correctly, a WorkSpace should be on a child GameObject of a TrackingAnchor.",
                            SetupIssue.IssueType.Warning,
                            this.gameObject,
                            new ReversibleAction(
                                () => MoveGameObject(GetRootTransform()),
                                this.gameObject,
                                "Move WorkSpace below TrackingAnchor.")));
                }
            }

            if (this.destinationObject == null)
            {
                var solutions = new List<ISetupIssueSolution>();
                if (parentTrackingAnchor)
                {
                    solutions.Add(
                        new ReversibleAction(
                            () => { this.destinationObject = parentTrackingAnchor.gameObject; },
                            this,
                            "Use TrackingAnchor as Destination for WorkSpace"));
                }
                issues.Add(
                    new SetupIssue(
                        "No Destination object",
                        "The WorkSpace requires a DestinationObject from which to correctly estimate potential initialization poses.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        solutions.ToArray()));
            }

            // To correctly estimate the source Geometry
            if (GetSourceGeometry() == null)
            {
                issues.Add(
                    new SetupIssue(
                        "No Source Object set",
                        "The WorkSpace requires a SourceObject from which to correctly estimate potential initialization poses.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new ReversibleAction(
                            CreateSourceObject,
                            this,
                            "Create sphere SourceObject for WorkSpace")));
            }

            // Only check for zero poses, if the WorkSpace is placed correctly
            if (issues.Count == 0 &&
                GetWorkSpaceDefinition(false)?.GetCameraTransforms()?.Length == 0)
            {
                issues.Add(
                    new SetupIssue(
                        "No poses were generated",
                        "Set a valid source and destination object.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject));
            }

            if (this.usedCamera == null)
            {
                var solutions = new List<ISetupIssueSolution>();
                if (CameraProvider.MainCamera)
                {
                    solutions.Add(
                        new ReversibleAction(
                            () => { this.usedCamera = CameraProvider.MainCamera; },
                            this,
                            "Use MainCamera in WorkSpace"));
                }

                issues.Add(
                    new SetupIssue(
                        "No Preview Camera",
                        "This camera is used to display all potential initialization poses in the GameView.",
                        SetupIssue.IssueType.Info,
                        this.gameObject,
                        solutions.ToArray()));
            }

            if (parentTrackingAnchor && GetRootTransform() != parentTrackingAnchor.transform)
            {
                issues.Add(
                    new SetupIssue(
                        "Incorrect RootTransform",
                        "The TrackingAnchor that uses this WorkSpace must be referenced as the RootTransform.",
                        SetupIssue.IssueType.Error,
                        this.gameObject));
            }

            return issues;
        }
#endif
    }
}
