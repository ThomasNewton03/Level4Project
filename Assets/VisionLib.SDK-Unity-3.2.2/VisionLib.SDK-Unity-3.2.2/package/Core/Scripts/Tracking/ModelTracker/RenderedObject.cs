using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Visometry.VisionLib.SDK.Core
{
    /// @ingroup Core
    [HelpURL(DocumentationLink.renderedObject)]
    [Serializable]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class RenderedObject : MonoBehaviour, ISceneValidationCheck
    {
        public enum RenderMode
        {
            WhenTracking,
            WhenInitializing,
            Always,
            Never,
            WhenInitializingOrTracking
        }

        public static bool Matches(
            RenderMode renderMode,
            AugmentationHandler.AugmentationMode augmentationMode)
        {
            return renderMode switch
            {
                RenderMode.WhenTracking => augmentationMode is AugmentationHandler.AugmentationMode
                    .Tracking,
                RenderMode.WhenInitializing => augmentationMode is AugmentationHandler
                    .AugmentationMode.Initializing,
                RenderMode.Always => true,
                RenderMode.Never => false,
                RenderMode.WhenInitializingOrTracking => augmentationMode is AugmentationHandler
                    .AugmentationMode.Initializing or AugmentationHandler.AugmentationMode.Tracking,
                _ => throw new ArgumentOutOfRangeException(nameof(renderMode), renderMode, null)
            };
        }

        [SerializeField]
        private TrackingAnchor trackingAnchor;
        [SerializeField]
        public RenderMode renderMode;
        [SerializeField]
        [Tooltip(
            "Interpolation time to apply updates to the transform from tracking. Lower values lead to more " +
            "immediate updates while larger values lead to smoother movement. Smoothing only applies to pose updates during tracking - not those during initialization.")]
        public float smoothTime = 0.0f;
        [SerializeField]
        private Renderer[] renderersInTarget = Array.Empty<Renderer>();
        private List<Renderer> disabledByThis = new List<Renderer>();

        private PositionUpdateDamper interpolationTarget;
        private AugmentationHandler.AugmentationMode lastAugmentationMode = AugmentationHandler.AugmentationMode.Inactive;

        private const string childOfTrackingAnchorMessage =
            "RenderedObjects cannot be children of TrackingAnchors.";

        public void RegisterWithTrackingAnchor()
        {
            if (this.trackingAnchor)
            {
                this.trackingAnchor.RegisterRenderedObject(this);
            }
        }

        public void DeregisterWithTrackingAnchor()
        {
            if (this.trackingAnchor)
            {
                this.trackingAnchor.DeregisterRenderedObject(this);
            }
        }

        public void SetTrackingAnchor(TrackingAnchor newTrackingAnchor)
        {
            DeregisterWithTrackingAnchor();
            this.trackingAnchor = newTrackingAnchor;
            RegisterWithTrackingAnchor();
        }

        private void Initialize()
        {
            this.interpolationTarget = new PositionUpdateDamper();
            if (this.trackingAnchor == null)
            {
                this.trackingAnchor = this.gameObject.GetComponent<TrackingAnchor>();
            }
            RegisterWithTrackingAnchor();
            UpdateRendererList();
        }

        public void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged += UpdateRendererList;
#endif
            Initialize();
            CheckLegalPlacement();
        }

        public void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= UpdateRendererList;
#endif
            DeregisterWithTrackingAnchor();
        }

        public void OnDestroy()
        {
            DeregisterWithTrackingAnchor();
        }

        public void SetRenderMode(RenderMode newRenderMode)
        {
            this.renderMode = newRenderMode;
            UpdateRendering();
        }

        public void ShowAlways()
        {
            SetRenderMode(RenderMode.Always);
        }

        public void ShowNever()
        {
            SetRenderMode(RenderMode.Never);
        }

        public void ShowWhenInitializing()
        {
            SetRenderMode(RenderMode.WhenInitializing);
        }

        public void ShowWhenInitializingOrTracking()
        {
            SetRenderMode(RenderMode.WhenInitializingOrTracking);
        }

        public void ShowWhenTracking()
        {
            SetRenderMode(RenderMode.WhenTracking);
        }

        public void SetRenderingState(AugmentationHandler.AugmentationMode newAugmentationMode)
        {
            this.lastAugmentationMode = newAugmentationMode;
            UpdateRendering();
        }

        public void SetMeshRenderersEnabledInSubtree(bool isEnabled)
        {
            TrackingObjectHelper.SetMeshRenderersEnabledInSubtree(this.gameObject, isEnabled);
        }

        private bool ShouldRender()
        {
            return Matches(this.renderMode, this.lastAugmentationMode);
        }

        public void UpdateRendering()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (this.renderersInTarget.Length == 0)
            {
                UpdateRendererList();
            }
            if (ShouldRender())
            {
                ReenableRenderer();
            }
            else
            {
                DisableRenderer();
            }
        }

        public void UpdateRendererList()
        {
            var renderersInThisRenderedObject = new List<Renderer>();
            SceneTraversal.Traverse(
                this.gameObject.transform,
                node => { renderersInThisRenderedObject.AddRange(node.GetComponents<Renderer>()); },
                node =>
                {
                    var renderedObjectOnNode = node.GetComponent<RenderedObject>();
                    return renderedObjectOnNode == null || renderedObjectOnNode == this;
                });
            this.renderersInTarget = renderersInThisRenderedObject.ToArray();
        }

        public void Update()
        {
            if (!this.trackingAnchor.IsReferenceValidAndEnabled())
            {
                return;
            }
            this.interpolationTarget.Slerp(this.smoothTime, this.gameObject);
        }

        public void SetTargetTransform(ModelTransform worldFromModel)
        {
            if (this.lastAugmentationMode is AugmentationHandler.AugmentationMode.Initializing
                or AugmentationHandler.AugmentationMode.Inactive)
            {
                SetTargetTransformImmediately(worldFromModel);
            }
            else
            {
                SetTargetTransformInInterpolationTarget(worldFromModel);
            }
        }

        private void SetTargetTransformImmediately(ModelTransform worldFromModel)
        {
            this.interpolationTarget.Invalidate();
            this.interpolationTarget.SetData(worldFromModel);
            this.interpolationTarget.Slerp(this.smoothTime, this.gameObject);
        }

        private void SetTargetTransformInInterpolationTarget(ModelTransform worldFromModel)
        {
            this.interpolationTarget.SetData(worldFromModel);
            if (this.smoothTime < Mathf.Epsilon)
            {
                Update();
            }
        }

        private bool IsLegallyPlaced()
        {
            var trackingAnchorOnParent = this.gameObject.GetComponentInParent<TrackingAnchor>();
            return !trackingAnchorOnParent || (trackingAnchorOnParent.transform == this.transform &&
                                               trackingAnchorOnParent == this.trackingAnchor);
        }

        private void CheckLegalPlacement()
        {
            if (!IsLegallyPlaced())
            {
                LogHelper.LogError(RenderedObject.childOfTrackingAnchorMessage, this);
            }
        }

        private void DisableRenderer()
        {
            var enabledRenderers = this.renderersInTarget.WhereAlive().Where(mr => mr.enabled);
            foreach (var renderer in enabledRenderers)
            {
                renderer.enabled = false;
                this.disabledByThis.Add(renderer);
            }
        }

        private void ReenableRenderer()
        {
            foreach (var renderer in this.disabledByThis.WhereAlive())
            {
                renderer.enabled = true;
            }
            this.disabledByThis.Clear();
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            var issues = new List<SetupIssue>();
            if (!this.trackingAnchor)
            {
                var solutions = new List<ISetupIssueSolution>() {new DestroyComponentAction(this)};

                var trackingAnchorOnGameObject = this.gameObject.GetComponent<TrackingAnchor>();
                if (trackingAnchorOnGameObject)
                {
                    solutions.Add(
                        new ReversibleAction(
                            () => { SetTrackingAnchor(trackingAnchorOnGameObject); },
                            this,
                            "Use TrackingAnchor on the same GameObject."));
                }

                issues.Add(
                    new SetupIssue(
                        "No TrackingAnchor set for RenderedObject",
                        "Every RenderedObject must be registered to a TrackingAnchor. Otherwise it will not work as expected.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        solutions.ToArray()));
            }
            if (this.trackingAnchor && !IsLegallyPlaced())
            {
                issues.Add(
                    new SetupIssue(
                        RenderedObject.childOfTrackingAnchorMessage,
                        "Everything below a TrackingAnchor will be considered part of the tracking geometry. " +
                        "Changes in the hierarchy will affect the tracking directly. " +
                        "A RenderedObject below the TrackingAnchor will therefore introduce a feedback loop.",
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new ISetupIssueSolution[] {new DestroyComponentAction(this)}));
            }
            if (this.gameObject.GetComponentsInChildren<Renderer>().Length < 1)
            {
                issues.Add(
                    new SetupIssue(
                        "RenderedObject has no renderers.",
                        "When a RenderedObject has no renderers, nothing will be shown",
                        SetupIssue.IssueType.Warning,
                        this.gameObject,
                        new ISetupIssueSolution[] {new DestroyComponentAction(this)}));
            }
            if (this.gameObject.GetComponentsInChildren<TrackingAnchor>().Any(
                    anchor => anchor.transform != this.transform && anchor == this.trackingAnchor))
            {
                issues.Add(
                    new SetupIssue(
                        "RenderedObject is parent of its TrackingAnchor.",
                        "When a RenderedObject is a parent of its TrackingAnchor, it will transform the TrackingAnchor in unhandled ways. This will lead to unexpected results.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject,
                        new ISetupIssueSolution[] {new DestroyComponentAction(this)}));
            }
            return issues.Concat(TransformSetupIssueHelper.CheckForUnexpectedScale(this.gameObject))
                .ToList();
        }
#endif
    }
}
