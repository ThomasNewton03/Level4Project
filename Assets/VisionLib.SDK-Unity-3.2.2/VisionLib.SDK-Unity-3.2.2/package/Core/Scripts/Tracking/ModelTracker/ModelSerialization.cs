using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Provides functionality to serialize trees of existing meshes in the scene for use in model tracking.
    /// </summary>
    /// @ingroup Core
    public static class ModelSerialization
    {
        /// <summary>
        ///     Representation of a model before serialization
        /// </summary>
        /// <member name="transform">
        ///     Source transform on which this model is located.
        /// </member>
        /// <member name="mesh">
        ///     The model's mesh.
        /// </member>
        /// <member name="texture">
        ///     The model's texture if available. (Null if not available.)
        /// </member>
        public class ModelDataSet
        {
            public Transform transform;
            public Mesh mesh;
            [CanBeNull]
            public Texture2D texture;
        }

        public class LoadedModelDescription
        {
            /// <summary>
            /// The name of the model added to the tracking system.
            /// </summary>
            public string name;

            /// <summary>
            /// Model hash of the data added to the tracking system. These strings can
            /// be used to acquire new license features for these models.
            /// </summary>
            public string licenseFeature;

            /// <summary>
            /// Warnings which occurred during deserialization of the model.
            /// </summary>
            public Issue[] warnings;
        }

        /// <summary>
        ///     Serializes all Meshes on the `origin` transform or below and sends them to
        ///     VisionLib for use as tracking models in the current tracker.
        /// </
        /// <param name="anchorName">
        ///     Name of the anchor to which the model should be added.
        /// </param>
        /// <param name="origin">
        ///     Transform from which to start the serialization.
        /// </param>
        /// <param name="useTexture">
        ///     Texture of mesh should also be added.
        /// </param>
        /// <returns>
        ///     Name, license features and unity transform for each serialized model.
        /// </returns>
        /// @exception ModelNotFoundException, ModelNotReadableException
        public static async Task<List<ModelDeserializationResult>> AddMeshesAsync(
            string anchorName,
            Transform origin,
            bool useTexture)
        {
            var serializedModels =
                SerializeModels(CollectModelDataSetsInChildren(origin, useTexture));
            var loadingResult = await SendModelDataToVisionLib(anchorName, serializedModels);
            return loadingResult.addedModels.ToList();
        }

        /// <summary>
        ///     Serializes the mesh directly on the `origin` transform sends the result to
        ///     VisionLib for use as tracking model in the current tracker.
        /// </summary>
        /// <param name="origin">
        ///     Transform on which search for a mesh to serialize
        /// </param>
        /// <param name="useTexture">
        ///     Texture of mesh should also be added.
        /// </param>
        /// <returns>
        ///     Name, license features and unity transform for the serialized model.
        /// </returns>
        /// /// @exception ModelNotFoundException, ModelNotReadableException
        public static async Task<LoadedModelDescription> AddMeshAsync(
            string anchorName,
            Transform origin,
            bool useTexture)
        {
            var serializedModels = SerializeModels(
                new List<ModelDataSet> {CollectModelData(origin, useTexture)});
            var loadingResult = await SendModelDataToVisionLib(anchorName, serializedModels);
            return new LoadedModelDescription
            {
                name = loadingResult.addedModels[0].name,
                licenseFeature = loadingResult.addedModels[0].licenseFeature,
                warnings = loadingResult.warnings
            };
        }

        /// <summary>
        ///     Forwards the URI of a model for use as tracking model in the current tracker.
        ///     VisionLib takes care of loading the model. 
        /// </summary>
        /// <param name="anchorName">
        ///     Name of anchor to which the model should be added.
        /// </param>
        /// <param name="origin">
        ///     Transform to which the serialized model should be linked.
        /// </param>
        /// <param name="URI">
        ///     URI of the model file to be serialized. 
        /// </param>
        /// <param name="enabled">
        ///     Optionally specify whether the model should be enabled in the
        ///     tracker from the start. Default is disabled.
        /// </param>
        /// <param name="occluder">
        ///     Optionally specify whether the model should be marked as an occluder in the
        ///     tracker from the start. Default is "not occluder".
        /// </param>
        /// <param name="useLines">
        ///     Optionally specify whether manual lines from this model should be used from the start.
        ///     Default is to exclude manual lines.
        /// </param>
        /// <returns>
        ///     Name, license features and unity transform for the serialized model.
        /// </returns>
        public static async Task<LoadedModelDescription> AddURIAsync(
            string anchorName,
            Transform origin,
            string URI,
            bool enabled = false,
            bool occluder = false,
            bool useLines = false)
        {
            var uniqueUnityModelID = origin.transform.GetInstanceID().ToString();
            ModelProperties modelProperties = new ModelProperties(
                uniqueUnityModelID,
                URI,
                enabled,
                occluder,
                useLines);

            var addModelWarnings = await MultiModelTrackerCommands.AnchorAddModelAsync(
                TrackingManager.Instance.Worker,
                anchorName,
                modelProperties);
            NotificationHelper.SendInfo("Added Model " + modelProperties.name);
            return new LoadedModelDescription
            {
                name = uniqueUnityModelID, warnings = addModelWarnings.warnings
            };
        }

        ///  <summary>
        ///      Updates the provided model to the new state (transform, enabled, occluder).
        ///      Meshes will not be altered by calling this function.
        ///  </summary>
        ///  <param name="anchorName">
        ///     Name of the anchor to which the model should be added.
        /// </param>
        ///  <param name="modelName">
        ///      Name of the model to update
        ///  </param>
        ///  <param name="transform">
        ///      Transform to set.
        ///  </param>
        ///  <param name="enabled">
        ///      Set to true if the meshes should be enabled.
        /// </param>
        ///  <param name="occluder">
        ///      Set to true if the meshes should be used as occluders.
        ///      Has no effect on disabled models.
        ///  </param>
        ///  <param name="useLines">
        ///      Set to true if the manual lines from this model should be used.
        ///      Has no effect on disabled models.
        /// </param>
        ///  <returns>
        ///      A void Task that finishes, once the command was processed.
        ///  </returns>
        public static async Task<WorkerCommands.CommandWarnings> UpdateModelPropertiesAsync(
            string anchorName,
            string modelName,
            ModelTransform transform,
            bool enabled,
            bool occluder,
            bool useLines)
        {
            var descriptors = new ModelDataDescriptorList
            {
                models =
                {
                    new ModelDataDescriptor
                    {
                        name = modelName,
                        enabled = enabled,
                        occluder = occluder,
                        transform = transform,
                        useLines = useLines
                    }
                }
            };
            try
            {
                return await MultiModelTrackerCommands.AnchorSetModelPropertiesAsync(
                    TrackingManager.Instance.Worker,
                    anchorName,
                    descriptors);
            }
            catch (NullSingletonException) {}
            return WorkerCommands.NoWarnings();
        }

        /// <summary>
        ///     Serializes a set of models and initializes a map of <see cref="LoadedModel"/>s
        ///     that will be populated with <see cref="LoadedModel.licenseFeature"/> after the
        ///     serialized models are sent to VisionLib.
        /// </summary>
        /// <param name="targets">
        ///     <see cref="ModelDataSet"/>s for the models of interest.
        /// </param>
        /// <returns>
        ///     A <see cref="SerializedModels"/> containing the serialized models and the
        ///     pre-initialized  <see cref="LoadedModel"/>
        /// </returns>
        public static SerializedModels SerializeModels(List<ModelDataSet> targets)
        {
            return SerializedModels.Concatenate(targets.Select(t => new SerializedModel(t)));
        }

        public static ModelDataSet CollectModelData(Transform node, bool useTexture)
        {
            var filter = node.GetComponent<MeshFilter>();
            filter.CheckSerializability();
            var meshRenderer = node.GetComponent<MeshRenderer>();
            Texture texture = null;
            if (useTexture && meshRenderer && meshRenderer.sharedMaterial &&
                meshRenderer.sharedMaterial.HasMainTexture())
            {
                texture = meshRenderer.sharedMaterial.mainTexture;
            }
            if (texture && !(texture is Texture2D))
            {
                LogHelper.LogWarning(
                    "Texture found in the material on \"" + node.name +
                    "\" was ignored because it is of unsupported type: \"" + texture.GetType() +
                    "\".\n",
                    node.gameObject);
                texture = null;
            }
            return new ModelDataSet
            {
                transform = node, mesh = filter.sharedMesh, texture = (Texture2D) texture
            };
        }

        private static bool ShouldInspectSubTree(Transform node)
        {
            return node.gameObject.activeInHierarchy && !node.GetComponent<TrackingObject>();
        }

        private static List<ModelDataSet> CollectModelDataSetsInChildren(
            Transform origin,
            bool useTexture)
        {
            var result = new List<ModelDataSet>();
            SceneTraversal.Traverse(
                origin,
                node => { result.Add(CollectModelData(node, useTexture)); },
                ShouldInspectSubTree);
            return result;
        }

        private static async Task<ModelDeserializationResultList> SendModelDataToVisionLib(
            string anchorName,
            SerializedModels serializedModels)
        {
            var binaryDataHandle = GCHandle.Alloc(serializedModels.data, GCHandleType.Pinned);
            var data = binaryDataHandle.AddrOfPinnedObject();
            var dataLength = Convert.ToUInt32(serializedModels.data.Length);

            try
            {
                var modelDescriptorList =
                    new ModelDataDescriptorList {models = serializedModels.dataDescriptors};
                return await MultiModelTrackerCommands.AnchorAddModelDataAsync(
                    TrackingManager.Instance.Worker,
                    anchorName,
                    modelDescriptorList,
                    data,
                    dataLength);
            }
            finally
            {
                binaryDataHandle.Free();
            }
        }
    }
}
