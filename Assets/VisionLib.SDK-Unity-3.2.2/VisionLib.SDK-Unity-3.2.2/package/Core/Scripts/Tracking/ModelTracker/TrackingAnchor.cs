using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine.Events;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.Details.Singleton;
using MeshHelper = Visometry.VisionLib.SDK.Core.Details.MeshHelper;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// The <see cref="TrackingAnchor"/> represents a tracked Object (anchor).
    /// It handles rendered augmentations at runtime, switching between an initial pose guide during
    /// initialization and content to be augmented while the anchor in question is tracked.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.trackingAnchor)]
    [DisallowMultipleComponent]
    public class TrackingAnchor : MonoBehaviour, ISceneValidationCheck, IParameterHandler
    {
        private enum AnchorState
        {
            Unknown,
            Created,
            Enabled,
            Disabled,
            Enabling,
            Disabling
        }

        [SerializeField]
        private AugmentationHandler augmentationHandler = new AugmentationHandler();

        private ModelTransform? lastTrackingPose;

        [SerializeField]
        private InitPoseHandler initPoseHandler = new InitPoseHandler();

        [CanBeNull]
        private string currentState = null;

        [SerializeField]
        [CanBeNull]
        [Tooltip(
            "Unique identifier for this TrackingAnchor within VisionLib. Duplicate names in " +
            "a Tracking Setup are not allowed.")]
        private string anchorName = null;

        private AnchorState anchorState = AnchorState.Unknown;

        private readonly List<LoadedModelHandle> modelsBeingLoaded = new List<LoadedModelHandle>();

#if UNITY_EDITOR
        [Tooltip(
            "If true, the parameters will be propagated from play mode to edit mode. Otherwise " +
            "they will be discarded after leaving the play mode.")]
        public bool persistParametersFromPlayMode;
        [Tooltip(
            "If true, the init pose will be propagated from play mode to edit mode. Otherwise " +
            "they will be discarded after leaving the play mode.")]
        public bool persistInitPoseFromPlayMode;
#endif

        private const string defaultAnchorNamePrefix = "TrackedObject";
        private static int nextDefaultAnchorNamePostfix;

        [SerializeField]
        private Metric.Unit unit = Metric.Unit.m;

        [SerializeField]
        private TrackingAnchor parentAnchor = null;

        [SerializeField]
        private List<TrackingAnchor> childAnchors = new List<TrackingAnchor>();

        public string GetAnchorName()
        {
            return this.anchorName ??= CreateUniqueName();
        }

        public bool HasParentAnchor()
        {
            return this.parentAnchor != null;
        }

        public bool GetIsInitPoseInBackendInitialized()
        {
            return this.initPoseHandler != null && (!this.initPoseHandler.useInitPose ||
                                                    this.initPoseHandler
                                                        .isInitPoseInitializedInBackend);
        }

        public bool ShowInitPoseGuideWhileDisabled
        {
            get
            {
                return this.augmentationHandler.ShowInitPoseGuideWhileDisabled;
            }
            set
            {
                this.augmentationHandler.ShowInitPoseGuideWhileDisabled = value;
            }
        }

        /// <summary>
        /// Object for adjusting parameters of the TrackingAnchor.
        /// </summary>
        [SerializeReference]
        private AnchorRuntimeParameters parameters = null;

        /// <summary>
        /// List of WorkSpaces that are used to initialize the tracking target.
        /// </summary>
        [SerializeField]
        [Tooltip("List of WorkSpaces that are used to initialize the tracking target.")]
        public WorkSpace[] workSpaces = new WorkSpace[] {};

        public void AddWorkSpace(WorkSpace workSpace)
        {
            this.workSpaces = this.workSpaces.Concat(new[] {workSpace}).ToArray();
            if (Application.isPlaying)
            {
                SetWorkSpaceInBackend();
            }
        }

        public bool AnchorExists
        {
            get
            {
                return this.anchorState != AnchorState.Unknown;
            }
        }

        public bool IsAnchorEnabled
        {
            get
            {
                return this.anchorState == AnchorState.Enabled;
            }
        }

        public bool IsAnchorDisabled
        {
            get
            {
                return this.anchorState == AnchorState.Disabled;
            }
        }

        [SerializeField]
        [Tooltip(
            "Camera that contains the slam tracking result. This camera has to match for all " +
            "existing Tracking Anchors in the current Tracking Setup. In most scenarios this " +
            "is the main camera that is moved in AR.")]
        private Camera slamCamera;

        public Camera GetSLAMCamera()
        {
            if (!this.slamCamera)
            {
                this.slamCamera = CameraProvider.MainCamera;
            }
            return this.slamCamera;
        }

        public void SetSLAMCamera(Camera SLAMCamera)
        {
            this.slamCamera = SLAMCamera;
        }

        public bool UseInitPose
        {
            get
            {
                return this.initPoseHandler.useInitPose;
            }
            set
            {
                this.initPoseHandler.useInitPose = value;
            }
        }

        public void SetKeepUpright(bool value)
        {
            this.initPoseHandler.keepUpright = value;
        }

        public bool GetKeepUpright()
        {
            return this.initPoseHandler.keepUpright;
        }

        public void SetWorldUpVector(Vector3 worldUp)
        {
            this.initPoseHandler.worldUpVector = worldUp;
        }

        public void SetModelUpVector(Vector3 modelUp)
        {
            this.initPoseHandler.modelUpVector = modelUp;
        }

        public Vector3 GetWorldUpVector()
        {
            return this.initPoseHandler.worldUpVector;
        }

        public Vector3 GetModelUpVector()
        {
            return this.initPoseHandler.modelUpVector;
        }

        public void SetDetectionThreshold(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().detectionThreshold.SetValueAsync(newValue, this),
                this);
        }

        public void SetTrackingThreshold(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().trackingThreshold.SetValueAsync(newValue, this),
                this);
        }

        public void SetContourEdgeThreshold(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().contourEdgeThreshold.SetValueAsync(newValue, this),
                this);
        }

        public void SetCreaseEdgeThreshold(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().creaseEdgeThreshold.SetValueAsync(newValue, this),
                this);
        }

        public void SetContrastThreshold(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().contrastThreshold.SetValueAsync(newValue, this),
                this);
        }

        public void SetDetectionRadius(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().detectionRadius.SetValueAsync(newValue, this),
                this);
        }

        public void SetTrackingRadius(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().trackingRadius.SetValueAsync(newValue, this),
                this);
        }

        public void SetKeyFrameDistance(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().keyFrameDistance.SetValueAsync(newValue, this),
                this);
        }

        public void SetPoseFilteringSmoothness(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().poseFilteringSmoothness.SetValueAsync(newValue, this),
                this);
        }

        public void SetSensitivityForEdgesInTexture(float newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().sensitivityForEdgesInTexture
                    .SetValueAsync(newValue, this),
                this);
        }

        public void SetDisablePoseEstimation(bool newValue)
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().disablePoseEstimation.SetValueAsync(newValue, this),
                this);
        }

        public Task<WorkerCommands.CommandWarnings> SetShowLineModelAsync(bool newShowLineModel)
        {
            return GetAnchorRuntimeParameters().showLineModel.SetValueAsync(newShowLineModel, this);
        }

        public void SetShowLineModel(bool newShowLineModel)
        {
            TrackingManager.CatchCommandErrors(SetShowLineModelAsync(newShowLineModel), this);
        }

        public Task<WorkerCommands.CommandWarnings> SetShowLineModelAsync(
            ShowLineModel newShowLineModel)
        {
            return GetAnchorRuntimeParameters().showLineModel.SetValueAsync(newShowLineModel, this);
        }

        public void SetShowLineModel(ShowLineModel newShowLineModel)
        {
            TrackingManager.CatchCommandErrors(SetShowLineModelAsync(newShowLineModel), this);
        }

        public ModelTransform? GetLastTrackingPose()
        {
            return this.lastTrackingPose;
        }

        public AnchorRuntimeParameters GetAnchorRuntimeParameters()
        {
            this.parameters ??= new AnchorRuntimeParameters();
            return this.parameters;
        }

        /// <summary>
        ///  Event fired once after the tracking state changed to "tracked".
        /// </summary>
        public UnityEvent OnTracked = new UnityEvent();

        /// <summary>
        ///  Event fired once after the tracking state changed to "critical".
        /// </summary>
        public UnityEvent OnTrackingCritical = new UnityEvent();

        /// <summary>
        ///  Event fired once after the tracking state changed to "lost".
        /// </summary>
        public UnityEvent OnTrackingLost = new UnityEvent();

        /// <summary>
        ///  Event fired once after the anchor internal state changed to "enabled".
        /// </summary>
        public UnityEvent OnAnchorEnabled = new UnityEvent();

        /// <summary>
        ///  Event fired once after the anchor internal state changed to "disabled".
        /// </summary>
        public UnityEvent OnAnchorDisabled = new UnityEvent();

        /// <summary>
        ///  Event emitted after the anchor has been added to VisionLib.
        /// </summary>
        public event VLSDK.VoidDelegate OnAnchorAdded;

        /// <summary>
        ///  Event emitted after the anchor has been removed from VisionLib.
        /// </summary>
        public event VLSDK.VoidDelegate OnAnchorRemoved;

        public void Awake()
        {
            UpdateParentAnchor();

            this.initPoseHandler.Initialize(this);
            SetTransformAsInitPose();
            this.initPoseHandler.RegisterCallbacks();

            TrackingManager.OnTrackerStopped += HandleTrackerStopped;
            TrackingManager.OnTrackerInitialized += HandleTrackerInitialized;
            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                HandleTrackerInitialized();
            }
        }

        private void OnEnable()
        {
            // This additional update happens, because ARFoundation updates the transform of the
            // SLAM Camera very late.
            Application.onBeforeRender += UpdateInitPose;
            EnableAnchor();
            GetAnchorRuntimeParameters().GetListOfAllParameters().Broadcast();
        }

        private void Update()
        {
            this.initPoseHandler.UpdateInitPoseInBackend();
            UpdateInitPose();
        }

        private void OnTransformParentChanged()
        {
            UpdateParentAnchor();
        }

        private void OnDisable()
        {
            DisableAnchor();
            Application.onBeforeRender -= UpdateInitPose;
        }

        private void OnDestroy()
        {
            // Since in OnDestroy, the Component will no longer exist after any await call,
            // everything has to be cleaned up before the async command is called.
            HandleTrackerStopped();
            RemoveAnchorFromBackend();

            TrackingManager.OnTrackerInitialized -= HandleTrackerInitialized;
            TrackingManager.OnTrackerStopped -= HandleTrackerStopped;
            this.initPoseHandler.DeregisterCallbacks();
            if (this.parentAnchor != null)
            {
                this.parentAnchor.DeregisterChild(this);
            }
        }

        public void SetAnchorNameSafely(string newAnchorName)
        {
            if (this.AnchorExists || (TrackingManager.DoesTrackerExistAndIsInitialized() &&
                                      this.gameObject.activeInHierarchy))
            {
                throw new InvalidOperationException(
                    "Cannot change the name of an Anchor that exists in the backend.");
            }
            this.anchorName = newAnchorName;
        }

        /// <summary>
        ///     Variant of SetInitPose that accepts the raw pose from a VL configuration file
        ///     and first translates this into Unity world coordinates before applying it.
        /// </summary>
        public void SetVLInitPose(Pose initPose)
        {
            var unityWorldInitPose = InitPoseHelper.VLInitPoseToUnityWorldPose(
                initPose,
                GetSLAMCamera());
            SetInitPose(unityWorldInitPose);
        }

        /// <summary>
        ///     Variant of SetInitPose that receives a pose in camera coordinate system
        /// </summary>
        public void SetInitPoseInCameraCoordinateSystem(Pose initPose)
        {
            var worldInitPoseMatrix =
                GetSLAMCamera().transform.localToWorldMatrix * initPose.ToMatrix();
            SetInitPose(worldInitPoseMatrix.ToPose());
        }

        public Pose GetVLInitPose()
        {
            var vlInitPose = InitPoseHelper.UnityWorldPoseToVLInitPose(
                this.transform.ToPose(),
                GetSLAMCamera());
            return vlInitPose;
        }

        /// <summary>
        ///     Set the InitPose in the InitPoseHandler to the specified Pose and immediately
        ///     send it to the backend. This change will take with approximately a single
        ///     frame's delay.
        /// </summary>
        public void SetInitPose(Pose initPose)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this.transform, "Set init pose");
#endif
            this.transform.SetPose(initPose);
            SetTransformAsInitPose();
        }

        /// <summary>
        ///     Centers this Anchor's tracking geometry in the view of the SLAM Camera.
        ///     Also adjusts the camera's near and far planes to fit the tracking geometry if
        ///     necessary.
        /// </summary>
        public void CenterInitPoseInSlamCamera()
        {
            var bounds = GetTrackingGeometryBoundsInAnchorCoordinates();
            if (!bounds.HasValue)
            {
                LogHelper.LogWarning(
                    "Cannot Center TrackingAnchor in SLAM Camera, since it has no TrackingGeometry");
                return;
            }

            MeshHelper.CenterBoundingBoxInCameraView(
                this.gameObject,
                bounds.Value.Transform(this.transform.localToWorldMatrix),
                GetSLAMCamera());
            SetTransformAsInitPose();
        }

        /// <summary>
        ///     Set the InitPose in the InitPoseHandler to the <see cref="TrackingAnchor"/>'s
        ///     current transform and immediately send it to the backend. This change will take
        ///     with approximately a single frame's delay.
        /// </summary>
        public void SetTransformAsInitPose()
        {
            TrackingManager.CatchCommandErrors(SetTransformAsInitPoseAndWriteToBackendAsync());
        }

        public async Task<WorkerCommands.CommandWarnings>
            SetTransformAsInitPoseAndWriteToBackendAsync()
        {
            WorkerCommands.CommandWarnings warnings;
            if (HasParentAnchor())
            {
                warnings =
                    await this.initPoseHandler.SetRelativeInitPoseAndWriteToBackend(
                        GetRelativeInitPose());
            }
            else
            {
                warnings = await this.initPoseHandler.SetInitPoseAndWriteToBackend(this.transform);
            }
            UpdateInitPose();
            return warnings;
        }

        /// <summary>
        ///     Enable the <see cref="AugmentationHandler"/>'s InitPoseGuide. In order to not
        ///     interfere with the augmentation behaviour, this is a no-op whenever tracking is
        ///     running.
        /// </summary>
        public void EnableInitPoseGuideIfNotTracking()
        {
            if (!TrackingManager.DoesTrackerExistAndIsRunning())
            {
                SwitchToInitPoseGuide();
            }
        }

        /// <summary>
        ///     Disable any augmentation currently enabled by the <see cref="AugmentationHandler"/>.
        ///     In order to not interfere with the augmentation behaviour, this is a no-op
        ///     whenever tracking is running.
        /// </summary>
        public void DisableAnyAugmentationIfNotTracking()
        {
            if (!TrackingManager.DoesTrackerExistAndIsRunning())
            {
                this.augmentationHandler.OnTrackingStopped();
            }
        }

        /// <summary>
        ///  Reset the tracking.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void ResetSoft()
        {
            TrackingManager.CatchCommandErrors(ResetSoftAndNotifyAsync(), this);
        }

        /// <summary>
        ///  Reset the tracking and all keyframes.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void ResetHard()
        {
            TrackingManager.CatchCommandErrors(ResetHardAndNotifyAsync(), this);
        }

        public string[] GetModelHashes()
        {
            var trackingMeshes = GetComponentsInChildren<TrackingMesh>();
            return trackingMeshes.Select(
                trackingMesh => VLSDK.GetModelHash(
                    new SerializedModel(
                        ModelSerialization.CollectModelData(
                            trackingMesh.transform,
                            trackingMesh.useTextureForTracking)))).ToArray();
        }

        public IEnumerable<RenderedObject> GetRegisteredRenderedObjects()
        {
            return this.augmentationHandler.renderedObjects;
        }

        public void RegisterRenderedObject(RenderedObject renderedObject)
        {
            this.augmentationHandler.Register(renderedObject);
        }

        public void DeregisterRenderedObject(RenderedObject renderedObject)
        {
            this.augmentationHandler.Deregister(renderedObject);
        }

        public async Task ResetSoftAndNotifyAsync()
        {
            await MultiModelTrackerCommands.AnchorResetSoftAsync(
                TrackingManager.Instance.Worker,
                GetAnchorName());
            NotificationHelper.SendInfo("Reset initPose of Anchor " + GetAnchorName(), this);
        }

        public async Task ResetHardAndNotifyAsync()
        {
            await MultiModelTrackerCommands.AnchorResetHardAsync(
                TrackingManager.Instance.Worker,
                GetAnchorName());
            NotificationHelper.SendInfo("Reset Anchor " + GetAnchorName(), this);
        }

        public void RegisterLoadingModel(LoadedModelHandle loadedModel)
        {
            this.modelsBeingLoaded.Add(loadedModel);
        }

        public void FinalizeLoadingModel(LoadedModelHandle loadedModel)
        {
            if (!this.modelsBeingLoaded.Contains(loadedModel))
            {
                throw new ArgumentException(
                    "Cannot finalize loading for model. Has not been registered or already removed.");
            }
            this.modelsBeingLoaded.Remove(loadedModel);
            if (this.modelsBeingLoaded.Count == 0)
            {
                SetWorkSpaceInBackend();
            }
        }

        [System.Obsolete("ClearNullWorkspaces is obsolete. It should no longer be used.")]
        public void ClearNullWorkspaces()
        {
            this.workSpaces = this.workSpaces.Where(workspace => workspace != null).ToArray();
        }

        private TrackingAnchor FindParentAnchor()
        {
            if (this.transform.parent == null)
            {
                return null;
            }
            return this.transform.parent.GetComponentInParent<TrackingAnchor>();
        }

        public void UpdateParentAnchor()
        {
            var newParent = FindParentAnchor();
            if (newParent == this.parentAnchor && (newParent == null || newParent.childAnchors.Contains(this)))
            {
                return;
            }
            if (this.parentAnchor != null)
            {
                this.parentAnchor.DeregisterChild(this);
            }
            if (newParent != null)
            {
                newParent.RegisterChild(this);
                this.initPoseHandler.useInitPose = false;
            }
            this.parentAnchor = newParent;
            if (this.AnchorExists)
            {
                SetCurrentParentAnchorInBackend();
            }
        }

        public ModelTransform? GetCurrentInitPoseInWorldCoordinateSystem()
        {
            return this.initPoseHandler.GetInitPoseInWorldCoordinateSystem();
        }

        internal ModelTransform? GetLastTrackingResultOrInitPose()
        {
            if (this.lastTrackingPose.HasValue)
            {
                return new ModelTransform(
                    CameraHelper.flipX * this.lastTrackingPose.Value.ToMatrix() *
                    CameraHelper.flipX);
            }
            return GetCurrentInitPoseInWorldCoordinateSystem();
        }

        public ModelTransform? GetCurrentInitPoseInCameraCoordinateSystem()
        {
            return this.initPoseHandler.GetInitPoseInCameraCoordinateSystem();
        }

        public void UpdateInitPose()
        {
            var maybeRelativeInitPose = this.initPoseHandler.GetInitPoseInParentCoordinateSystem();
            if (HasParentAnchor() && maybeRelativeInitPose.HasValue)
            {
                var maybeParentPose = this.parentAnchor.GetLastTrackingResultOrInitPose();
                if (maybeParentPose.HasValue)
                {
                    this.augmentationHandler.SetInitPose(
                        maybeParentPose.Value * maybeRelativeInitPose.Value);
                }
                return;
            }
            var maybeInitPose = this.initPoseHandler.GetInitPoseInWorldCoordinateSystem();
            if (!maybeInitPose.HasValue)
            {
                return;
            }
            this.augmentationHandler.SetInitPose(maybeInitPose.Value);
        }

        public bool ActiveInBackend()
        {
            return this && this.IsAnchorEnabled;
        }

        public Task<WorkerCommands.CommandWarnings> SetParameterAsync(
            string parameterName,
            string parameterValue)
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return Task.FromResult(WorkerCommands.NoWarnings());
            }
            var attribute = new MultiModelTrackerCommands.AnchorAttribute(
                GetAnchorName(),
                parameterValue);
            return MultiModelTrackerCommands.AnchorSetAttributeAsync(
                TrackingManager.Instance.Worker,
                parameterName,
                new List<MultiModelTrackerCommands.AnchorAttribute>() {attribute});
        }

        public async Task<T> GetParameterAsync<T>(string parameterName)
        {
            return await MultiModelTrackerCommands.AnchorGetAttributeAsync<T>(
                TrackingManager.Instance.Worker,
                GetAnchorName(),
                parameterName);
        }

        public Task<WorkerCommands.CommandWarnings> ResetParameterAsync(string parameterName)
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return Task.FromResult(WorkerCommands.NoWarnings());
            }
            return MultiModelTrackerCommands.AnchorResetParameterAsync(
                TrackingManager.Instance.Worker,
                GetAnchorName(),
                parameterName);
        }

        public void SetParameter(string parameterName, string parameterValue)
        {
            TrackingManager.CatchCommandErrors(
                SetParameterAsync(parameterName, parameterValue),
                this);
        }

        public void ResetParametersToDefault()
        {
            TrackingManager.CatchCommandErrors(
                GetAnchorRuntimeParameters().ResetParametersToDefaultAsync(this),
                this);
        }

        /// <summary>
        /// Accepts the raw parameters section from a VL configuration file, transforms this into
        /// Unity parameters and applies them to this TrackingAnchor.
        /// Note that some parameters can only be applied on a global level and not inside
        /// the TrackingAnchor. 
        /// </summary>
        public Task<WorkerCommands.CommandWarnings> SetVLParametersAsync(string jsonParameterString)
        {
            return AnchorRuntimeParametersConverter.ParseParameters(jsonParameterString)
                .ApplyToAnchorAsync(this);
        }

        /// <summary>
        /// Accepts the raw parameters section from a VL configuration file, transforms this into
        /// Unity parameters and applies them to this TrackingAnchor.
        /// Note that some parameters can only be applied on a global level and not inside
        /// the TrackingAnchor.
        /// </summary>
        public void SetVLParameters(string jsonParameterString)
        {
            TrackingManager.CatchCommandErrors(SetVLParametersAsync(jsonParameterString), this);
        }

        public void SetMeshRenderersEnabledInSubtree(bool isEnabled)
        {
            TrackingObjectHelper.SetMeshRenderersEnabledInSubtree(this.gameObject, isEnabled);
        }

        public Bounds? GetTrackingGeometryBoundsInAnchorCoordinates()
        {
            return GetComponentsInChildren<TrackingObject>()
                .Where(trackingObject => trackingObject.enabled && !trackingObject.occluder)
                .Select(o => o.GetBoundingBoxInAnchorCoordinates()).Combine();
        }

        /// <summary>
        /// This function scales the TrackingGeometry so that the current units will be transformed
        /// to the new metric.
        /// </summary>
        /// <param name="newUnit"></param>
        public void SetMetric(Metric.Unit newUnit)
        {
            var scaleFactor = Metric.ScaleFactor(this.unit, newUnit);
            ScaleTrackingGeometry(scaleFactor);
            this.unit = newUnit;
        }

        public Metric.Unit GetMetric()
        {
            return this.unit;
        }

        public static string CreateUniqueName()
        {
            var allUsedAnchorNamesInScene = Object.FindObjectsOfType<TrackingAnchor>()
                .Select(reference => reference.anchorName).ToArray();
            while (allUsedAnchorNamesInScene.Contains(
                       TrackingAnchor.defaultAnchorNamePrefix +
                       TrackingAnchor.nextDefaultAnchorNamePostfix))
            {
                TrackingAnchor.nextDefaultAnchorNamePostfix++;
            }
            return TrackingAnchor.defaultAnchorNamePrefix +
                   TrackingAnchor.nextDefaultAnchorNamePostfix;
        }

        public bool IsTracking()
        {
            return !string.IsNullOrEmpty(this.currentState) && this.currentState != "lost";
        }

        /// <summary>
        /// This function scales the TrackingGeometry and by keeping the relative distances between
        /// the models.
        /// </summary>
        /// <param name="scaleFactor"></param>
        private void ScaleTrackingGeometry(float scaleFactor)
        {
            for (var childIndex = 0; childIndex < this.transform.childCount; childIndex++)
            {
                var child = this.transform.GetChild(childIndex);
#if UNITY_EDITOR
                Undo.RecordObject(child, "Scale Tracking Geometry");
#endif
                child.localPosition *= scaleFactor;
                child.localScale *= scaleFactor;
            }
        }

        private async Task<WorkerCommands.CommandWarnings> HandleTrackerInitializedAsync()
        {
            this.currentState = null;

            var warnings = await CreateAnchor();
            this.ThrowIfNotAlive();
            this.anchorState = AnchorState.Created;
            if (this.gameObject.activeSelf && this.enabled)
            {
                warnings.Concat(await EnableAnchorAsync());
            }
            else
            {
                await DisableAnchorAsync();
            }
            return warnings;
        }

        private void HandleTrackingStates(TrackingState state)
        {
            if (!this.IsAnchorEnabled || !IsParentAnchorTracking())
            {
                return;
            }
            try
            {
                var objectState = state.objects.First(obj => obj.name == GetAnchorName()).state;
                HandleTrackingState(objectState);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(
                    "No tracking state for anchor \"" + GetAnchorName() + "\" found.");
            }
        }

        private void HandleTrackingState(string stateString)
        {
            if (this.currentState != null && string.Equals(stateString, this.currentState))
            {
                return;
            }
            switch (stateString)
            {
                case "tracked":
                    HandleTracked();
                    break;
                case "critical":
                    HandleTrackingCritical();
                    break;
                case "lost":
                    HandleTrackingLost();
                    break;
                default:
                    throw new ArgumentException("Invalid Tracking State: \"" + stateString + "\"");
            }
            this.currentState = stateString;
        }

        private void OnModelTransform(SimilarityTransform worldFromModelTransform)
        {
            if (!worldFromModelTransform.GetValid())
            {
                return;
            }
            this.lastTrackingPose =
                CameraHelper.flipXY * new ModelTransform(worldFromModelTransform);
            this.augmentationHandler.OnModelTransform(this.lastTrackingPose.Value);
            PerformOnChildren(childAnchor => { childAnchor.UpdateInitPose(); });
        }

        private void SwitchToInactiveRenderer()
        {
            this.augmentationHandler.SwitchToAugmentationMode(
                AugmentationHandler.AugmentationMode.Inactive);
            PerformOnChildren(childAnchor => childAnchor.SwitchToInactiveRenderer());
        }

        private void SwitchToInitPoseGuide()
        {
            this.augmentationHandler.SwitchToAugmentationMode(
                AugmentationHandler.AugmentationMode.Initializing);
            PerformOnChildren(
                childAnchor =>
                {
                    childAnchor.currentState = null;
                    childAnchor.SwitchToInactiveRenderer();
                });
        }

        private void SwitchToAugmentation()
        {
            this.augmentationHandler.SwitchToAugmentationMode(
                AugmentationHandler.AugmentationMode.Tracking);
            PerformOnChildren(
                childAnchor =>
                {
                    if (!childAnchor.IsTracking())
                    {
                        childAnchor.SwitchToInitPoseGuide();
                    }
                });
        }

        private void PerformOnChildren(Action<TrackingAnchor> action)
        {
            this.childAnchors.RemoveAll(anchor => anchor == null);
            foreach (var child in this.childAnchors)
            {
                action(child);
            }
        }

        private void HandleTracked()
        {
            SwitchToAugmentation();
            this.OnTracked.Invoke();
        }

        private void HandleTrackingCritical()
        {
            SwitchToAugmentation();
            this.OnTrackingCritical.Invoke();
        }

        private void HandleTrackingLost()
        {
            this.lastTrackingPose = null;
            if (this.enabled && (this.initPoseHandler.useInitPose || IsParentAnchorTracking()))
            {
                SwitchToInitPoseGuide();
            }
            else
            {
                SwitchToInactiveRenderer();
            }
            this.OnTrackingLost.Invoke();
        }

        private async Task SetWorkSpaceInBackendAsync()
        {
            const bool useCameraRotation = true;
            var workSpaceDefinitions = this.workSpaces.Where(ws => ws != null && ws.isActiveAndEnabled)
                .Select(ws => ws.GetWorkSpaceDefinition(useCameraRotation)).ToList();
            if (workSpaceDefinitions.Count == 0)
            {
                LogHelper.LogDebug(
                    "The anchor " + GetAnchorName() + " has no workspace defined.",
                    this);
                return;
            }

            await MultiModelTrackerCommands.AnchorSetWorkSpaceAsync(
                TrackingManager.Instance.Worker,
                GetAnchorName(),
                new API.WorkSpace.Configuration(workSpaceDefinitions));
            NotificationHelper.SendInfo(
                "Added " + this.workSpaces.Length + " WorkSpace(s) for anchor " + GetAnchorName(),
                this);
        }

        private void SetWorkSpaceInBackend()
        {
            TrackingManager.CatchCommandErrors(SetWorkSpaceInBackendAsync(), this);
        }

        /// Also treats the case where an anchor with the same name already exists in the backend.
        private async Task<WorkerCommands.CommandWarnings> CreateAnchor()
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return WorkerCommands.NoWarnings();
            }
            bool anchorExistsInBackend = await MultiModelTrackerCommands.AnchorExistsAsync(
                TrackingManager.Instance.Worker,
                GetAnchorName());
            this.ThrowIfNotAlive();

            var warnings = WorkerCommands.NoWarnings();
            if (!anchorExistsInBackend)
            {
                // The anchor will be added disabled by default, so AddAnchorAsync will not fail
                // because of license limitations.
                warnings = await MultiModelTrackerCommands.AddAnchorAsync(
                    TrackingManager.Instance.Worker,
                    GetAnchorName(),
                    false);
                this.ThrowIfNotAlive();
                LogHelper.LogDebug("Anchor " + GetAnchorName() + " has been added.", this);
            }

            await SetCurrentParentAnchorInBackendAsync();
            this.ThrowIfNotAlive();

            this.modelsBeingLoaded.Clear();
            TrackingManager.Instance.ResetAnchorTransformListener(GetAnchorName());
            RegisterAnchorListeners();
            OnAnchorAdded?.Invoke();
            return warnings;
        }

        private async Task<WorkerCommands.CommandWarnings> EnableAnchorAsync()
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized() || !ShouldEnableAnchor())
            {
                return WorkerCommands.NoWarnings();
            }

            this.anchorState = AnchorState.Enabling;
            await MultiModelTrackerCommands.EnableAnchorAsync(
                TrackingManager.Instance.Worker,
                GetAnchorName());
            this.ThrowIfNotAlive();
            this.anchorState = AnchorState.Enabled;
            var warnings = await GetAnchorRuntimeParameters().UpdateParametersInBackendAsync(this);
            SwitchToInitPoseGuide();
            this.OnAnchorEnabled.Invoke();
            LogHelper.LogDebug("Enabled Anchor " + GetAnchorName(), this);
            return warnings;
        }

        private async Task DisableAnchorAsync()
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized() || !ShouldDisableAnchor())
            {
                return;
            }

            this.anchorState = AnchorState.Disabling;
            try
            {
                await MultiModelTrackerCommands.DisableAnchorAsync(
                    TrackingManager.Instance.Worker,
                    GetAnchorName());
            }
            catch (ObjectDisposedException) {}
            try
            {
                this.ThrowIfNotAlive();
                this.anchorState = AnchorState.Disabled;
                SwitchToInactiveRenderer();
                this.OnAnchorDisabled.Invoke();
                LogHelper.LogDebug("Disabled Anchor " + GetAnchorName(), this);
                ResetInternalTrackingState();
            }
            catch (MonoBehaviourExtensions.MonoBehaviourRemovedDuringTaskException) {}
        }

        private void ResetInternalTrackingState()
        {
            if (IsParentAnchorTracking())
            {
                HandleTrackingState("lost");
            }
            this.currentState = null;
        }

        private async Task RemoveAnchorFromBackendAsync()
        {
            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                await MultiModelTrackerCommands.RemoveAnchorAsync(
                    TrackingManager.Instance.Worker,
                    GetAnchorName());
            }
        }

        private void HandleAnchorDestroyed()
        {
            this.modelsBeingLoaded.Clear();
            OnAnchorRemoved?.Invoke();
            this.anchorState = AnchorState.Unknown;
            this.lastTrackingPose = null;
            this.augmentationHandler.OnTrackingStopped();
        }

        private void EnableAnchor()
        {
            TrackingManager.CatchCommandErrors(EnableAnchorAsync(), this);
        }

        private void RemoveAnchorFromBackend()
        {
            TrackingManager.CatchCommandErrors(RemoveAnchorFromBackendAsync(), this);
        }

        private void DisableAnchor()
        {
            TrackingManager.CatchCommandErrors(DisableAnchorAsync(), this);
        }

        private void HandleTrackerInitialized()
        {
            TrackingManager.CatchCommandErrors(HandleTrackerInitializedAsync(), this);
        }

        private void HandleTrackerStopped()
        {
            ResetInternalTrackingState();
            DeregisterAnchorListeners();
            HandleAnchorDestroyed();
        }

        private void RegisterAnchorListeners()
        {
            TrackingManager.OnTrackingStates += HandleTrackingStates;
            TrackingManager.AnchorTransform(GetAnchorName()).OnUpdate += OnModelTransform;
        }

        private void DeregisterAnchorListeners()
        {
            TrackingManager.AnchorTransform(GetAnchorName()).OnUpdate -= OnModelTransform;
            try
            {
                TrackingManager.Instance.ResetAnchorTransformListener(GetAnchorName());
            }
            catch (NullSingletonException) {}
            TrackingManager.OnTrackingStates -= HandleTrackingStates;
        }

        private bool ShouldEnableAnchor()
        {
            return this.anchorState is AnchorState.Disabled or AnchorState.Created
                or AnchorState.Disabling;
        }

        private bool ShouldDisableAnchor()
        {
            return this.anchorState is AnchorState.Enabled or AnchorState.Created
                or AnchorState.Enabling;
        }

        private void RegisterChild(TrackingAnchor childAnchor)
        {
            this.childAnchors.Add(childAnchor);
        }

        private void DeregisterChild(TrackingAnchor childAnchor)
        {
            if (this.childAnchors.Contains(childAnchor))
            {
                this.childAnchors.Remove(childAnchor);
            }
        }

        private async Task SetCurrentParentAnchorInBackendAsync()
        {
            if (HasParentAnchor())
            {
                await MultiModelTrackerCommands.SetAnchorParentAsync(
                    TrackingManager.Instance.Worker,
                    GetAnchorName(),
                    this.parentAnchor.GetAnchorName());
                this.ThrowIfNotAlive();
                LogHelper.LogDebug(
                    $"Set {this.parentAnchor.GetAnchorName()} as parent of {GetAnchorName()}",
                    this);
            }
            else
            {
                await MultiModelTrackerCommands.RemoveAnchorParentAsync(
                    TrackingManager.Instance.Worker,
                    GetAnchorName());
                this.ThrowIfNotAlive();
                LogHelper.LogDebug($"Removed parent from {GetAnchorName()}", this);
            }
        }

        private void SetCurrentParentAnchorInBackend()
        {
            TrackingManager.CatchCommandErrors(SetCurrentParentAnchorInBackendAsync(), this);
        }

        private bool IsParentAnchorTracking()
        {
            return this.parentAnchor == null || this.parentAnchor.IsTracking();
        }

        private ModelTransform GetRelativeInitPose()
        {
            if (!HasParentAnchor())
            {
                throw new ArgumentException("Called GetRelativeInitPose on a root anchor");
            }
            return new ModelTransform(
                TransformUtilities.GetRelativeTransform(
                    this.transform,
                    this.parentAnchor.transform));
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetGeneralIssues()
        {
            var issues = new List<SetupIssue>();
            if (this.workSpaces.Any(workspace => workspace == null))
            {
                issues.Add(
                    new SetupIssue(
                        "The TrackingAnchor " + GetAnchorName() + " contains empty WorkSpaces",
                        "This might create unexpected behaviour. Please set or remove the empty entries in the list of WorkSpaces.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject,
                        new ReversibleAction(
                            () =>
                            {
                                this.workSpaces = this.workSpaces
                                    .Where(workspace => workspace != null).ToArray();
                            },
                            this,
                            "Remove all empty entries in the list of WorkSpaces")));
            }

            if (TrackingAnchorHelper.GetDuplicateAnchorNamesInScene().Contains(GetAnchorName()))
            {
                issues.Add(
                    new SetupIssue(
                        "The anchorName " + GetAnchorName() +
                        " is used more than once in the scene",
                        "This will create unexpected behaviour. Change the anchorName of one of the TrackingAnchors.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new ReversibleAction(
                            () => { this.anchorName = CreateUniqueName(); },
                            this,
                            "Generate new TrackingAnchorName")));
            }

            if (!this.initPoseHandler.useInitPose && !HasParentAnchor())
            {
                if (this.workSpaces.Length == 0)
                {
                    issues.Add(
                        new SetupIssue(
                            "The TrackingAnchor uses no init pose but also no workspace is defined",
                            "There is no way to initialize the tracking. If you have predefined " +
                            "initialization data added to your tracking configuration, you can ignore " +
                            "this issue.",
                            SetupIssue.IssueType.Warning,
                            this.gameObject,
                            new ReversibleAction(
                                () => { this.initPoseHandler.useInitPose = true; },
                                this,
                                "Set \"useInitPose\" to \"true\"")));
                }

                var initPoseGuide = GetRegisteredRenderedObjects().FirstOrDefault(
                    renderedObject =>
                        renderedObject.renderMode != RenderedObject.RenderMode.WhenTracking);
                if (initPoseGuide != null)
                {
                    issues.Add(
                        new SetupIssue(
                            "The TrackingAnchor uses no init pose but has a registered init pose guide",
                            "The tracking will not start if the init pose guide is aligned with the physical object.",
                            SetupIssue.IssueType.Error,
                            this.gameObject,
                            new ISetupIssueSolution[2]
                            {
                                new ReversibleAction(
                                    () => this.initPoseHandler.useInitPose = true,
                                    this,
                                    "Set \"useInitPose\" to \"true\""),
                                new ReversibleAction(
                                    () => initPoseGuide.renderMode =
                                        RenderedObject.RenderMode.WhenTracking,
                                    initPoseGuide,
                                    "Set mode in RenderedObject to \"When Tracking\"")
                            }));
                }
            }

            if (this.slamCamera == null)
            {
                ISetupIssueSolution solution = CameraProvider.MainCamera != null
                    ? new ReversibleAction(
                        () => { this.slamCamera = CameraProvider.MainCamera; },
                        this,
                        "Set SLAM camera to CameraProvider.MainCamera")
                    : null;
                issues.Add(
                    new SetupIssue(
                        "The TrackingAnchor has to reference a valid SLAM camera",
                        "Without a valid SLAM camera, the InitPose cannot be set correctly.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        solution));
            }

            var allTrackingAnchors = UnityEngine.Object.FindObjectsOfType<TrackingAnchor>();
            var trackingAnchorsByCamera = allTrackingAnchors
                .GroupBy(trackingAnchor => trackingAnchor.GetSLAMCamera())
                .OrderByDescending(anchors => anchors.Count());
            if (trackingAnchorsByCamera.Count() > 1)
            {
                var cameraOfMostTrackingAnchors = trackingAnchorsByCamera.First().Key
                    ? trackingAnchorsByCamera.First().Key
                    : trackingAnchorsByCamera.ElementAt(1).Key;
                const string setupIssueMessage =
                    "Using different SLAMCameras simultaneously will lead to unexpected " +
                    "behaviour. Therefore you should use a single the SLAMCamera per scene.";
                issues.Add(
                    new SetupIssue(
                        "More than one SLAMCamera is used in the scene",
                        setupIssueMessage,
                        SetupIssue.IssueType.Warning,
                        this.gameObject,
                        new ReversibleAction(
                            () =>
                            {
                                foreach (var anchor in allTrackingAnchors)
                                {
                                    Undo.RecordObject(anchor, setupIssueMessage);
                                    anchor.SetSLAMCamera(cameraOfMostTrackingAnchors);
                                }
                            },
                            this,
                            "Set SLAMCamera in all TrackingAnchors to " +
                            cameraOfMostTrackingAnchors)));
            }

            return issues.Concat(TransformSetupIssueHelper.CheckForUnexpectedScale(this.gameObject))
                .ToList();
        }

        public List<SetupIssue> GetTrackingGeometryIssues(
            Bounds? trackingGeometryBoundsInAnchorCoordinates)
        {
            if (trackingGeometryBoundsInAnchorCoordinates.HasValue)
            {
                var bounds = trackingGeometryBoundsInAnchorCoordinates.Value;
                var minDimension = Math.Min(bounds.size.x, Math.Min(bounds.size.y, bounds.size.z));
                var maxDimension = Math.Max(bounds.size.x, Math.Max(bounds.size.y, bounds.size.z));
                var setupIssues = SetupIssue.NoIssues();

                if (minDimension < 0.01 || maxDimension > 50)
                {
                    var plausibleMetrics = Metric.GetPlausibleMetrics(
                        bounds.size,
                        this.unit,
                        0.01f,
                        50f);
                    ISetupIssueSolution[] solveErrorFunctions = plausibleMetrics.Select(
                        plausibleMetric => new ReversibleAction(
                            () => SetMetric(plausibleMetric),
                            this,
                            $"Change metric to {plausibleMetric.ToString()}")).ToArray();
                    var modelDimensions = bounds.GetDimensionString(this.unit);

                    setupIssues.Add(
                        new SetupIssue(
                            $"Model dimensions implausible ({modelDimensions})",
                            $"The physical size of the model is set to {modelDimensions}. There might be a problem with the model size. Adjust the metric of the model",
                            SetupIssue.IssueType.Warning,
                            this.gameObject,
                            solveErrorFunctions));
                }
                return setupIssues;
            }

            if (!this.HasMeshes())
            {
                return new List<SetupIssue>()
                {
                    new SetupIssue(
                        "No models found in the subtree",
                        "Add models for tracking.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject)
                };
            }

            return new List<SetupIssue>()
            {
                new SetupIssue(
                    "No TrackingMesh components found in the subtree",
                    "Without these components, the models will not be used for tracking. " +
                    "Automatically add a TrackingMesh on each child model or manually add " +
                    "TrackingMesh components to the desired models.",
                    SetupIssue.IssueType.Warning,
                    this.gameObject,
                    new ReversibleAction(
                        () =>
                        {
                            TrackingObjectHelper.AddTrackingMeshesInSubTree(this.gameObject);
                        },
                        this,
                        TrackingAnchorSettings.addTrackingMeshLabel))
            };
        }

        public List<SetupIssue> GetAugmentedContentIssues()
        {
            if (!this.gameObject.activeInHierarchy)
            {
                return new List<SetupIssue>
                {
                    new SetupIssue(
                        "GameObject is disabled.",
                        "Integrity checks for augmented content are disabled. Enable the GameObject to enable checks.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject)
                };
            }
            if (this.HasAugmentedContent())
            {
                return SetupIssue.NoIssues();
            }

            const string noRenderedObjectIssueTitle =
                "No RenderedObject is set that uses this TrackingAnchor.";

            if (!this.HasMeshes())
            {
                return new List<SetupIssue>()
                {
                    new SetupIssue(
                        noRenderedObjectIssueTitle,
                        "Manually add the target reference.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject)
                };
            }
            return new List<SetupIssue>()
            {
                new SetupIssue(
                    noRenderedObjectIssueTitle,
                    "Manually add the target reference or automatically create and reference the targets using the corresponding buttons.",
                    SetupIssue.IssueType.Warning,
                    this.gameObject,
                    new ISetupIssueSolution[4]
                    {
                        new ReversibleAction(
                            () =>
                            {
                                TrackingAnchorHelper.CreateRenderedObjectAndLinkToTrackingAnchor(
                                    RenderedObject.RenderMode.Always,
                                    this,
                                    this.gameObject);
                            },
                            this,
                            TrackingAnchorSettings.selfAugmentationLabel),
                        new ReversibleAction(
                            () =>
                            {
                                this.CloneAsRenderedObject(RenderedObject.RenderMode.WhenTracking);
                            },
                            this,
                            TrackingAnchorSettings.cloneAsAugmentationLabel),
                        new ReversibleAction(
                            () =>
                            {
                                var clonedParent = this.CloneAsRenderedObject(
                                    RenderedObject.RenderMode.WhenInitializing);
                                TrackingObjectHelper.SetMeshRendererMaterialsInSubtree(
                                    clonedParent,
                                    TrackingObjectHelper.LoadAsset<Material>(
                                        TrackingObjectHelper.LoadableAsset
                                            .SemiTransparentDefaultMaterial));
                            },
                            this,
                            TrackingAnchorSettings.cloneAsInitPoseGuideLabel),
                        new ReversibleAction(
                            () =>
                            {
                                this.CloneAsRenderedObject(
                                    RenderedObject.RenderMode.WhenInitializingOrTracking);
                            },
                            this,
                            TrackingAnchorSettings.cloneAsBothLabel)
                    })
            };
        }

        public List<SetupIssue> GetTrackingEventIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.OnTracked, this.OnTrackingCritical, this.OnTrackingLost},
                this.gameObject);
        }

        public List<SetupIssue> GetAnchorEventIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.OnAnchorEnabled, this.OnAnchorDisabled},
                this.gameObject);
        }

        public List<SetupIssue> GetSceneIssues()
        {
            return GetTrackingEventIssues().Concat(GetAnchorEventIssues())
                .Concat(GetAnchorRuntimeParameters().GetSceneIssues(this.gameObject))
                .Concat(GetGeneralIssues())
                .Concat(GetTrackingGeometryIssues(GetTrackingGeometryBoundsInAnchorCoordinates()))
                .Concat(GetAugmentedContentIssues()).ToList();
        }
#endif
    }
}
