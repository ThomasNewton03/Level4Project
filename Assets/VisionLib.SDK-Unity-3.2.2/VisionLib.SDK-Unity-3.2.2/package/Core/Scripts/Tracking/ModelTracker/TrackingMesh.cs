using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    // @ingroup Core
    [AddComponentMenu("VisionLib/Core/Tracking Mesh")]
    [DisallowMultipleComponent]
    [HelpURL(DocumentationLink.trackingMesh)]
    public class TrackingMesh : TrackingObject
    {
        [SerializeField]
        // This parameter specifies whether the texture of the MeshFilter on this GameObject should
        // be used for tracking.
        public bool useTextureForTracking = true;

        protected override void CreateLoadedModelsHandle()
        {
            this.loadedModelHandle = new LoadedModelHandle(
                this,
                GetTrackingAnchor().GetAnchorName(),
                (anchorName) => ModelSerialization.AddMeshAsync(
                    anchorName,
                    this.transform,
                    this.useTextureForTracking));
        }

        public override Bounds GetBoundingBoxInModelCoordinates()
        {
            return BoundsUtilities.GetMeshBounds(this.gameObject);
        }
        
        public override Bounds GetBoundingBoxInAnchorCoordinates()
        {
            return BoundsUtilities.GetMeshBoundsInParentCoordinates(
                this.gameObject,
                GetTrackingAnchor()?.transform, false);
        }

#if UNITY_EDITOR
        public override List<SetupIssue> GetSceneIssues()
        {
            var issues = base.GetSceneIssues();

            var meshFilter = this.gameObject.GetComponent<MeshFilter>();
            var assetPath = AssetDatabase.GetAssetPath(meshFilter);
            try
            {
                meshFilter.CheckSerializability();
                if (!meshFilter.DoesUseFileScale())
                {
                    issues.Add(
                        new SetupIssue(
                            "Mesh does not use file scale",
                            "The mesh on the GameObject does not use the scale specified in the model file. " +
                            "This might create a size mismatch between the rendered model and the actual model. " +
                            "Enable this option!",
                            SetupIssue.IssueType.Warning,
                            this.gameObject,
                            new ISetupIssueSolution[]
                            {
                                new AssetRelatedAction(
                                    () => meshFilter.SetUsageOfFileScaleOnSharedMesh(true),
                                    "Use metric specified in model file.")
                            },
                            $"{assetPath}: Adjust file scale"));
                }
            }
            catch (MeshFilterExtensions.ModelNotReadableException e)
            {
                issues.Add(
                    new SetupIssue(
                        "TrackingMesh cannot be serialized",
                        e.Message,
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new ISetupIssueSolution[]
                        {
                            new AssetRelatedAction(
                                () => meshFilter.SetIsReadableOnSharedMesh(true),
                                "Allow vertices and indices to be accessed from script."),
                            new DestroyComponentAction(this)
                        },
                        $"{assetPath}: Enable Read/Write"));
            }
            catch (Exception e)
            {
                issues.Add(
                    new SetupIssue(
                        "TrackingMesh cannot find valid model",
                        e.Message,
                        SetupIssue.IssueType.Error,
                        this.gameObject,
                        new DestroyComponentAction(this)));
            }

            if (this.useTextureForTracking)
            {
                var meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    var materials = new List<Material>();
                    meshRenderer.GetSharedMaterials(materials);
                    issues.AddRange(
                        materials.Where(material => material && !material.IsSerializable()).Select(
                            material => new SetupIssue(
                                "Texture cannot be serialized",
                                "The Texture on the GameObject can't be serialized because " +
                                "'Read/Write Enabled' is not activated in the corresponding asset's import " +
                                "settings. Enable this option!",
                                SetupIssue.IssueType.Error,
                                this.gameObject,
                                new ISetupIssueSolution[]
                                {
                                    new AssetRelatedAction(
                                        () => material.SetIsReadableOnTextureAsset(
                                            true,
                                            this.gameObject),
                                        "Allow texture to be accessed from script."),
                                    new ReversibleAction(
                                        () => this.useTextureForTracking = false,
                                        this,
                                        "Deactivate texture for tracking."),
                                    new DestroyComponentAction(this)
                                },
                                $"{assetPath}: Enable Read/Write")));
                }
            }
            return issues;
        }
#endif
    }
}
