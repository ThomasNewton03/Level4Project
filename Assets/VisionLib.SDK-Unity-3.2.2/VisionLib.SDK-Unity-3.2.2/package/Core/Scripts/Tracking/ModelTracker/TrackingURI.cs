using System;
using System.Collections.Generic;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Visometry.VisionLib.SDK.Core
{
    // @ingroup Core
    [AddComponentMenu("VisionLib/Core/Tracking URI")]
    [DisallowMultipleComponent]
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracking_uri.html")]
    public class TrackingURI : TrackingObject
    {
        private const string invalidURIprefix = "Invalid model URI: ";

        public class URINotSetException : Exception
        {
            public URINotSetException(string message)
                : base(message) {}
        }

        public class MalformedURIException : Exception
        {
            public MalformedURIException(string message)
                : base(message) {}
        }

        [SerializeField]
        private string modelFileURI;

        private Bounds boundsInModelDimensions;
        private string boundsModelFileURI;

        protected override void CreateLoadedModelsHandle()
        {
            ThrowIfURIEmptyOrMalformed(this.modelFileURI);
            this.loadedModelHandle = new LoadedModelHandle(
                this,
                GetTrackingAnchor().GetAnchorName(),
                (anchorName) => ModelSerialization.AddURIAsync(
                    anchorName,
                    this.transform,
                    this.modelFileURI));
        }

        public string GetModelFileURI()
        {
            return this.modelFileURI;
        }

        public static TrackingURI AddTrackingURI(GameObject target, string URI)
        {
            ThrowIfURIEmptyOrMalformed(URI);
            var targetWasActive = target.activeSelf;
            target.SetActive(false);
            var trackingURI = target.AddComponentUndoable<TrackingURI>();
#if UNITY_EDITOR
            Undo.RecordObject(trackingURI, "Set up tracking URI");
#endif
            trackingURI.modelFileURI = URI;
            target.SetActive(targetWasActive);
            return trackingURI;
        }

        public override Bounds GetBoundingBoxInModelCoordinates()
        {
            try
            {
                if (this.boundsModelFileURI != this.modelFileURI)
                {
                    this.boundsInModelDimensions = VLSDK.GetModelBoundingBox(this.modelFileURI);
                    this.boundsModelFileURI = this.modelFileURI;
                }
                return this.boundsInModelDimensions;
            }
            catch (ArgumentException)
            {
                return new Bounds();
            }
        }

        public override Bounds GetBoundingBoxInAnchorCoordinates()
        {
            var boundsInModelCoordinates = GetBoundingBoxInModelCoordinates();
            return boundsInModelCoordinates.extents == Vector3.zero
                ? boundsInModelCoordinates
                : boundsInModelCoordinates.Transform(
                    TransformUtilities.GetRelativeTransform(
                        this.transform,
                        GetTrackingAnchor()?.transform));
        }

        private static void ThrowIfURIEmptyOrMalformed(string URI)
        {
            if (string.IsNullOrEmpty(URI))
            {
                throw new URINotSetException(TrackingURI.invalidURIprefix + "No URI provided.");
            }
            if (!PathHelper.IsAbsoluteURI(URI))
            {
                throw new MalformedURIException(
                    TrackingURI.invalidURIprefix +
                    $"Provided string '{URI}' is not an absolute URI.");
            }
        }

#if UNITY_EDITOR
        public override List<SetupIssue> GetSceneIssues()
        {
            var setupIssues = base.GetSceneIssues();
            
            if (!ModelFileURIPointsToLoadableFile(
                    this.modelFileURI,
                    out var issueTitle,
                    out var issueMessage))
            {
                setupIssues.Add(
                    new(
                        issueTitle,
                        issueMessage,
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new ISetupIssueSolution[2]
                        {
                            new DestroyComponentAction(this),
                            new ReversibleAction(
                                () =>
                                {
                                    this.modelFileURI =
                                        PathHelper.SubstituteStreamingAssetsPathWithSchema(
                                            EditorUtility.OpenFilePanel(
                                                "Select a model file",
                                                Application.dataPath,
                                                ""));
                                },
                                this,
                                "Select a model file.")
                        }));
            }

            return setupIssues;
        }

        private static bool ModelFileURIPointsToLoadableFile(
            string modelFileURI,
            out string invalidURITitle,
            out string invalidURIMessage)
        {
            if (string.IsNullOrEmpty(modelFileURI))
            {
                invalidURITitle = "ModelFileURI is empty";
                invalidURIMessage = "Cannot use the TrackingURI with an empty URI";
                return false;
            }
            if (!PathHelper.IsAbsoluteURI(modelFileURI))
            {
                invalidURITitle = "Invalid model URI";
                invalidURIMessage =
                    $"Provided modelFileURI \"{modelFileURI}\" is not an absolute URI.";
                return false;
            }
            if (!VLSDK.FileExists(modelFileURI))
            {
                invalidURITitle = "Could not resolve modelFileURI";
                invalidURIMessage =
                    $"No data could be loaded from \"{modelFileURI}\". Provide a correct URI or remove the TrackingURI.";
                return false;
            }
            invalidURITitle = null;
            invalidURIMessage = null;
            return true;
        }
#endif
    }
}
