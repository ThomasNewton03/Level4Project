using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///     An <see cref="GameObjectPoseInteraction"/> will directly manipulate the pose of the
    ///     <see cref="GameObject"/> it is attached to according to mouse/touch inputs.
    /// 
    ///     Pose changes are understood relative to the <see cref="Camera"/> referenced 
    ///     <see cref="GameObjectPoseInteraction"/>'s <see cref="gameViewCamera"/>.
    ///
    ///     Zooming moves the object closer/further away from the camera. Panning moves the object
    ///     horizontally and laterally. Tilting rotates the object in place.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "game_object_pose_interaction.html")]
    [DisallowMultipleComponent]
    public class GameObjectPoseInteraction : MonoBehaviour, ISceneValidationCheck,
        IObjectPoseInteractionProvider
    {
        /// <summary>
        /// Specifies when the user interaction should be processed
        /// </summary>
        private enum InteractionType
        {
            Always,
            WhenGameObjectSelected
        }

        [SerializeField]
        [Tooltip("Reference to the camera that renders the game view during user interaction.")]
        public Camera gameViewCamera;

        [Tooltip("Specifies when this component should process the user interaction")]
        [SerializeField]
        private InteractionType interactionType = InteractionType.Always;

        [SerializeField]
        [Tooltip("Rotation speed while dragging.")]
        private float dragRotationSpeed = 1f;

        [Tooltip("How much the drag speed is slowed down per update.")]
        [SerializeField]
        private float dragRotationSpeedDampening = 10f;

        [Tooltip("Speed at which drag rotation will stop completely.")]
        [SerializeField]
        private float dragRotationSpeedThreshold = 0.1f;

        [Tooltip("Determines how much the camera will be zoomed on each Zoom.")]
        [SerializeField]
        private float zoomStep = 0.05f;

        [Tooltip("Determines how much scrolling needs to occur, for it being detected as scroll.")]
        [SerializeField]
        private float scrollThreshold = 0.1f;

        [Tooltip("Determines how much the camera will be panned on each 3 finger movement.")]
        [SerializeField]
        private float panFactor = 1f;

        // <summary>
        // Set this value to overwrite the pivot point with a point
        // given in this objects coordinate system
        //
        // If set to null, the center of the bounding box of all meshes in this object is used.
        // </summary>
        public Vector3? pivotPointOverwrite = null;

        /// <summary>
        /// Last set original pose. Allows undoing all interactions.
        /// </summary>
        private Pose originalPoseInCameraSpace;
        private Pose currentPoseInCameraSpace;

        private const float mouseVsTouchFactor = 4f;

        private Vector2 lastPositionTouch0, lastPositionTouch1;
        private Vector2 rotationVelocity = Vector2.zero;

        private bool isTransformBeingChanged;
        private bool pointerOverGameObject;

        private BoxCollider interactionCollider;

        /// <summary>
        /// The previous frame's pan input location on the screen in pixels - if available.
        /// (0,0) = Bottom left; (screenWidth, screenHeight) = top right.
        /// Is null while no interaction is underway and before the first pan input in a given
        /// interaction.
        /// </summary>
        private Vector2? previousScreenSpacePanPoint;

        public event IObjectPoseInteractionProvider.VoidDelegate InteractionStarted;
        public event IObjectPoseInteractionProvider.VoidDelegate TargetTransformChanged;
        public event IObjectPoseInteractionProvider.VoidDelegate InteractionEnded;

        private void Awake()
        {
            SaveCurrentPoseAsOriginalPose();
        }

        private void OnDisable()
        {
            RemoveCollider();
        }

        private void LateUpdate()
        {
            AdjustCollider();
            if (this.isTransformBeingChanged)
            {
                RestoreTransformInCameraSpace();
            }
            HandleUserInput();
            StoreTransformInCameraSpace();
        }

        private void RestoreTransformInCameraSpace()
        {
            var currentPoseInWorldSpace = GetGameViewCamera().transform
                .TransformPose(this.currentPoseInCameraSpace);
            this.transform.SetPose(currentPoseInWorldSpace);
        }

        private void StoreTransformInCameraSpace()
        {
            this.currentPoseInCameraSpace = GetGameViewCamera().transform
                .InverseTransformPose(this.transform.ToPose());
        }

        /// <summary>
        /// Resets the <see cref="GameObject"/>'s transform to the last set OriginalPose.
        /// </summary>
        public void ResetToOriginalPose()
        {
            SetToPose(GetGameViewCamera().transform.TransformPose(this.originalPoseInCameraSpace));
        }

        /// <summary>
        /// Sets the OriginalPose to the specified value.
        /// </summary>
        public void SetOriginalPose(Pose originalPose)
        {
            this.originalPoseInCameraSpace =
                GetGameViewCamera().transform.InverseTransformPose(originalPose);
        }

        /// <summary>
        /// Records the GameObject's current pose as the new OriginalPose.
        /// </summary>
        public void SaveCurrentPoseAsOriginalPose()
        {
            SetOriginalPose(this.gameObject.transform.ToPose());
        }

        /// <summary>
        /// Sets the given pose in the <see cref="GameObject"/>'s transform.
        /// Expects
        /// </summary>
        public void SetToPose(Pose pose)
        {
            if (this.isTransformBeingChanged)
            {
                return;
            }
            InteractionStarted?.Invoke();
            this.gameObject.transform.SetPose(pose);
            TargetTransformChanged?.Invoke();
            InteractionEnded?.Invoke();
        }

        public void SetInteractionCamera(Camera interactionCamera)
        {
            this.gameViewCamera = interactionCamera;
        }

        public Camera GetInteractionCamera()
        {
            return this.gameViewCamera;
        }

        public void SetEnabled(bool newEnabledState)
        {
            this.enabled = newEnabledState;
        }

        /// \deprecated RegisterListener should be replaced by directly registering to the corresponding events
        [Obsolete(
            "RegisterListener should be replaced by directly registering to the corresponding events")]
        public void RegisterListener(IObjectPoseInteractionEventListener listener)
        {
            InteractionStarted += listener.OnInteractionStarted;
            TargetTransformChanged += listener.OnTransformChanged;
            InteractionEnded += listener.OnInteractionEnded;
        }

        /// \deprecated DeregisterListener should be replaced by directly deregistering from the corresponding events
        [Obsolete(
            "DeregisterListener should be replaced by directly deregistering from the corresponding events")]
        public void DeregisterListener(IObjectPoseInteractionEventListener listener)
        {
            InteractionEnded += listener.OnInteractionEnded;
            TargetTransformChanged += listener.OnTransformChanged;
            InteractionStarted += listener.OnInteractionStarted;
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            var setupIssues = new List<SetupIssue>();
            if (FindObjectOfType<EventSystem>() == null)
            {
                setupIssues.Add(
                    new SetupIssue(
                        "No EventSystem is present in the scene",
                        "Without an EventSystem, pose interactions will not receive any inputs and therefore will not work.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new CreateGameObjectSolution(
                            "Create new EventSystem",
                            "EventSystem",
                            typeof(EventSystem),
                            typeof(StandaloneInputModule))));
            }
            if (this.enabled && DeployingToPlatformWithoutMouse() &&
                this.interactionType == InteractionType.WhenGameObjectSelected)
            {
                var go = this.gameObject;
                setupIssues.Add(
                    new SetupIssue(
                        "Interaction Type \"When Game Object Selected\" is not supported on mobile platforms.",
                        "The interaction type \"When Game Object Selected\" is currently" +
                        " not supported on platforms without mouse interaction. The interaction will do nothing" +
                        " if deployed like this.",
                        SetupIssue.IssueType.Warning,
                        go,
                        new ISetupIssueSolution[]
                        {
                            new ReversibleAction(
                                () => this.interactionType = InteractionType.Always,
                                this,
                                "Change the interaction type to always. Touch inputs will" +
                                " modify the init poses of all anchors simultaneously."),
                            new ReversibleAction(
                                () => TrackingAnchorHelper.RemoveInitPoseInteraction(go),
                                go,
                                "Remove all interaction components.")
                        }));
            }
            return setupIssues;
        }

        private static bool DeployingToPlatformWithoutMouse()
        {
#if UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR)
            return true;
#else
            var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            return activeBuildTarget == BuildTarget.Android || activeBuildTarget == BuildTarget.iOS;
#endif
        }
#endif

        public Bounds GetObjectBounds()
        {
            return BoundsUtilities.GetRendererBounds(this.gameObject, true);
        }

        private Camera GetGameViewCamera()
        {
            if (this.gameViewCamera == null)
            {
                this.gameViewCamera = CameraProvider.MainCamera;
            }
            return this.gameViewCamera;
        }

        private void AdjustCollider()
        {
            switch (this.interactionType)
            {
                case InteractionType.Always:
                {
                    if (this.interactionCollider != null)
                    {
                        Destroy(this.interactionCollider);
                        this.interactionCollider = null;
                    }
                    return;
                }
                case InteractionType.WhenGameObjectSelected:
                {
                    if (this.interactionCollider != null)
                    {
                        return;
                    }
                    this.interactionCollider = this.gameObject.AddComponentUndoable<BoxCollider>();
                    var meshBounds = BoundsUtilities.GetRendererBounds(this.gameObject, true);
                    this.interactionCollider.size = meshBounds.size;
                    this.interactionCollider.center =
                        this.transform.InverseTransformPoint(meshBounds.center);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveCollider()
        {
            Destroy(this.interactionCollider);
            this.interactionCollider = null;
        }

#if !UNITY_IOS && !UNITY_ANDROID && !(UNITY_WSA_10_0 && (VL_HL_XRPROVIDER_OPENXR || VL_HL_XRPROVIDER_WINDOWSMR))
        private void OnMouseEnter()
        {
            this.pointerOverGameObject = true;
        }

        private void OnMouseExit()
        {
            this.pointerOverGameObject = false;
        }
#endif
        private void StartUserInteraction()
        {
            this.isTransformBeingChanged = true;
            InteractionStarted?.Invoke();
        }

        private void EndUserInteraction()
        {
            InteractionEnded?.Invoke();
            this.isTransformBeingChanged = false;
            ResetTemporaryInteractionVariables();
        }

        private void ResetTemporaryInteractionVariables()
        {
            this.previousScreenSpacePanPoint = null;
        }

        private void HandleUserInput()
        {
            if (ReceivingUserInput())
            {
                UpdateTransformBasedOnInput();
            }
            else
            {
                if (HasInteractionJustStopped())
                {
                    EndUserInteraction();
                }
                // User stopped interacting but rotation not yet finished
                if (this.rotationVelocity != Vector2.zero)
                {
                    DampenRotationVelocity();
                    ClampRotationVelocity();
                }
            }

            if (this.rotationVelocity != Vector2.zero)
            {
                Rotate();
            }
        }

        private bool HasInteractionJustStopped()
        {
            return (this.rotationVelocity == Vector2.zero) && this.isTransformBeingChanged;
        }

        private void UpdateTransformBasedOnInput()
        {
            if (!this.isTransformBeingChanged)
            {
                if (this.interactionType == InteractionType.WhenGameObjectSelected &&
                    !this.pointerOverGameObject)
                {
                    return;
                }
                StartUserInteraction();
            }

            if (Input.touchCount > 0)
            {
                UpdateTransformBasedOnTouchInput();
            }
            else
            {
                UpdateTransformBasedOnMouseAndScrollWheelInput();
            }
        }

        private void UpdateTransformBasedOnMouseAndScrollWheelInput()
        {
            if (IsLeftMouseButtonPressed())
            {
                MouseRotateUpdate();
            }
            else if (IsScrollWheelMoved())
            {
                MouseZoomUpdate();
            }
            else if (WasRightMouseButtonPressed())
            {
                if (WasRightMouseButtonJustPressed())
                {
                    this.previousScreenSpacePanPoint = Input.mousePosition;
                }
                else
                {
                    MousePanUpdate();
                }
            }
        }

        private void UpdateTransformBasedOnTouchInput()
        {
            switch (Input.touchCount)
            {
                case 1:
                    TouchRotateUpdate();
                    break;
                case 2:
                    TouchZoomUpdate();
                    break;
                case 3:
                    TouchPanUpdate();
                    break;
            }
        }

        private bool ReceivingUserInput()
        {
            return (ReceivingTouchInput() && !IsAnyFingerOverUI()) ||
                   (ReceivingMouseInput() && !IsMouseOverUI());
        }

        private static bool ReceivingTouchInput()
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended;
        }

        private bool ReceivingMouseInput()
        {
            if (Input.touchCount > 0)
            {
                return false;
            }

            return IsLeftMouseButtonPressed() || WasRightMouseButtonPressed() ||
                   IsScrollWheelMoved();
        }

        private bool IsLeftMouseButtonPressed()
        {
            return Input.GetMouseButton(0);
        }

        private bool IsScrollWheelMoved()
        {
            return Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) >= this.scrollThreshold;
        }

        private bool WasRightMouseButtonPressed()
        {
            return Input.GetMouseButton(1);
        }

        private bool WasRightMouseButtonJustPressed()
        {
            return Input.GetMouseButtonDown(1);
        }

        /// <summary>
        /// Checks if any touch input is made on an UI element.
        /// </summary>
        /// <remarks>
        /// Note: EventSystem.current.IsPointerOverGameObject(touch.fingerId)
        /// does return false when touch.phase == TouchPhase.Ended, even if the position
        /// of the touch is still over the UI.
        /// So only call this after checking the touch phase accordingly.
        /// </remarks>
        private static bool IsAnyFingerOverUI()
        {
            return Input.touches.Any(
                touch => EventSystem.current.IsPointerOverGameObject(touch.fingerId));
        }

        private static bool IsMouseOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Rotates the target <see cref="GameObject"/> around its center point.
        /// </summary>
        private void Rotate()
        {
            var rotationVector = this.rotationVelocity;
            var cameraTransform = GetGameViewCamera().transform;
            RotateAroundAxis(
                rotationVector.y,
                rotationVector.x,
                cameraTransform.right,
                -cameraTransform.up);
        }

        private void RotateAroundAxis(
            float xRotation,
            float yRotation,
            Vector3 xRotationAxis, //rightVector
            Vector3 yRotationAxis) //upVector
        {
            var center = GetPivotPoint();
            this.gameObject.transform.RotateAround(center, xRotationAxis, xRotation);
            this.gameObject.transform.RotateAround(center, yRotationAxis, yRotation);
            TargetTransformChanged?.Invoke();
        }

        private Vector3 GetCameraPositionInWorld()
        {
            return GetGameViewCamera().transform.position;
        }

        private void MouseRotateUpdate()
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            this.rotationVelocity = mouseMovement * (this.dragRotationSpeed *
                                                     GameObjectPoseInteraction.mouseVsTouchFactor);
        }

        private void TouchRotateUpdate()
        {
            var touchInput = Input.touches[0].deltaPosition;
            touchInput.Scale(
                new Vector2(
                    1f / Screen.currentResolution.width,
                    1f / Screen.currentResolution.height));
            this.rotationVelocity = touchInput * (180f * this.dragRotationSpeed);
        }

        /// <summary>
        /// Slows down the rotation over time.
        /// </summary>
        private void DampenRotationVelocity()
        {
            this.rotationVelocity *=
                (float) Math.Exp(-this.dragRotationSpeedDampening * Time.deltaTime);
        }

        /// <summary>
        /// Checks if the velocity is above the dragRotationSpeedThreshold.
        /// This prevents endless rotation, when the rotation speed slows down.
        /// </summary>
        private void ClampRotationVelocity()
        {
            if (Vector3.Magnitude(this.rotationVelocity) < this.dragRotationSpeedThreshold)
            {
                this.rotationVelocity = Vector2.zero;
            }
        }

        private void MouseZoomUpdate()
        {
            var zoomFactor = -Input.GetAxis("Mouse ScrollWheel") * this.zoomStep;
            Zoom(zoomFactor);
        }

        private Vector2 ScaleInScreenSpace(Vector2 x, float scale)
        {
            var cam = GetGameViewCamera();
            var center = new Vector2(cam.pixelWidth, cam.pixelHeight) / 2.0f;
            return (x - center) * scale + center;
        }

        private void TouchZoomUpdate()
        {
            if (Input.GetTouch(0).phase != TouchPhase.Began &&
                Input.GetTouch(1).phase != TouchPhase.Began)
            {
                var previousTouchDistance = Vector2.Distance(
                    this.lastPositionTouch0,
                    this.lastPositionTouch1);
                var touchDistance = Vector2.Distance(
                    Input.GetTouch(0).position,
                    Input.GetTouch(1).position);
                var distanceFactor = touchDistance / previousTouchDistance;
                var zoomFactor = 1.0f / distanceFactor - 1.0f;
                Zoom(zoomFactor);

                var newMeanPoint = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
                var oldMeanPoint = (this.lastPositionTouch0 + this.lastPositionTouch1) / 2.0f;
                var oldPointAfterScaling = ScaleInScreenSpace(oldMeanPoint, distanceFactor);
                Pan(oldPointAfterScaling, newMeanPoint);

                // Rotate around view axis
                var newDiffVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
                var oldDiffVetor = this.lastPositionTouch1 - this.lastPositionTouch0;
                var angle = Vector2.SignedAngle(oldDiffVetor, newDiffVector);
                this.gameObject.transform.RotateAround(
                    GetGameViewCamera().transform.position,
                    ScreenToWorldPoint(newMeanPoint) - GetGameViewCamera().transform.position,
                    angle);
            }
            this.lastPositionTouch0 = Input.GetTouch(0).position;
            this.lastPositionTouch1 = Input.GetTouch(1).position;
        }

        private void MousePanUpdate()
        {
            Pan(Input.mousePosition);
        }

        private void TouchPanUpdate()
        {
            var meanPoint = (Input.GetTouch(0).position + Input.GetTouch(1).position +
                             Input.GetTouch(2).position) * (1.0f / 3.0f);

            if (Input.GetTouch(0).phase == TouchPhase.Began ||
                Input.GetTouch(1).phase == TouchPhase.Began ||
                Input.GetTouch(2).phase == TouchPhase.Began)
            {
                this.previousScreenSpacePanPoint = meanPoint;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved &&
                     Input.GetTouch(1).phase == TouchPhase.Moved &&
                     Input.GetTouch(2).phase == TouchPhase.Moved)
            {
                Pan(meanPoint);
            }
        }

        private void Zoom(float zoomFactor)
        {
            if (zoomFactor == 0f)
            {
                return;
            }
            var shiftFactor = zoomFactor * GetClampedInteractionPlaneZ();
            this.gameObject.transform.position +=
                GetGameViewCamera().transform.forward * shiftFactor;
            TargetTransformChanged?.Invoke();
        }

        private void Pan(Vector2 currentPanPoint)
        {
            this.previousScreenSpacePanPoint ??= currentPanPoint;
            Pan(this.previousScreenSpacePanPoint.Value, currentPanPoint);
            this.previousScreenSpacePanPoint = currentPanPoint;
        }

        private void Pan(Vector2 previousPoint, Vector2 currentPoint)
        {
            var currentPointWorldSpace = ScreenToWorldPoint(currentPoint);
            var previousPointWorldSpace = ScreenToWorldPoint(previousPoint);
            var worldSpaceTranslation = (currentPointWorldSpace - previousPointWorldSpace) *
                                        this.panFactor;
            this.gameObject.transform.position += worldSpaceTranslation;
            TargetTransformChanged?.Invoke();
        }

        private float GetClampedInteractionPlaneZ()
        {
            var zValue = GetGameViewCamera().transform.InverseTransformPoint(GetPivotPoint()).z;
            // Make sure, that the interaction plane is at least 50 cm in front of us
            return Math.Max(0.5f, zValue);
        }

        private Vector3 ScreenToWorldPoint(Vector2 screenPoint)
        {
            return GetGameViewCamera().ScreenToWorldPoint(
                new Vector3(screenPoint.x, screenPoint.y, GetClampedInteractionPlaneZ()));
        }

        private Vector3 GetPivotPoint()
        {
            if (this.pivotPointOverwrite != null)
            {
                return this.transform.TransformPoint(this.pivotPointOverwrite.Value);
            }
            return GetObjectBounds().center;
        }
    }
}
