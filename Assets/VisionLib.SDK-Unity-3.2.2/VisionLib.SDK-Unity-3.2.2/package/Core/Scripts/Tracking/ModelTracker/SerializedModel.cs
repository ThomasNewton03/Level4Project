using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Rendering;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///     Fixed-size byte buffer. Easily append data from arrays of
    ///     different data types until the buffer is full. 
    /// </summary>
    /// <member name="data">
    ///     Read-only access to the data in the buffer.
    /// </member>
    /// @ingroup Core
    public class BinaryDataBuffer
    {
        public readonly byte[] data;
        private readonly int bufferSize;
        private int offset;

        /// <param name="bufferSizeInBytes">
        ///     Size of the data buffer. this can not be changed later.
        /// </param>
        public BinaryDataBuffer(int bufferSizeInBytes)
        {
            this.data = new byte[bufferSizeInBytes];
            this.offset = 0;
            this.bufferSize = bufferSizeInBytes;
        }

        /// <summary>
        ///     Append data of the specified type the the data already contained in the buffer.
        ///     Throws an exception if the provided data exceed the free space in the buffer. 
        /// </summary>
        /// <param name="src">
        ///     Source array.
        /// </param>
        /// <param name="datumSizeInBytes">
        ///     Size of a single array element in bytes.  
        /// </param>
        /// @exception IndexOutOfRangeException
        public void Append<DataType>(DataType[] src, int datumSizeInBytes) where DataType : struct
        {
            var offsetIncrement = datumSizeInBytes * src.Length;
            if (this.offset + offsetIncrement > this.bufferSize)
            {
                throw new IndexOutOfRangeException(
                    "BinaryDataBuffer: Data to append is longer" +
                    " than the free space in the buffer.");
            }
            Buffer.BlockCopy(src, 0, this.data, this.offset, offsetIncrement);
            this.offset += offsetIncrement;
        }
    }

    /// <summary>
    ///     Contains the concatenated data from a set of <see cref="SerializedModel"/>s that were
    ///     concatenated.
    /// </summary>
    /// <member name="data">
    ///     The buffer containing the raw concatenated data.
    /// </member>
    /// <member name="dataDescriptors">
    ///     Each descriptor in this list represents a model contained in <see cref="data"/>.
    ///     The descriptor's <see cref="BinaryDataDescriptor"/> specifies where the model's data
    ///     is located inside <see cref="data"/>. 
    /// </member>
    /// @ingroup Core
    public struct SerializedModels
    {
        [NotNull]
        public byte[] data;
        [NotNull]
        public List<ModelDataDescriptor> dataDescriptors;
        
        public static SerializedModels Concatenate(IEnumerable<SerializedModel> models)
        {
            var totalModelDataSizeInBytes = models.Sum(m => m.DataSizeInBytes);
            var dataBuffer = new BinaryDataBuffer(totalModelDataSizeInBytes);
            var modelDescriptors = new List<ModelDataDescriptor>();
            var byteOffset = 0;
            foreach (var model in models)
            {
                dataBuffer.Append(model.BinaryData, sizeof(byte));
                var descriptor = model.serializedModelDataDescriptor;
                descriptor.shape.binaryOffset = byteOffset;
                modelDescriptors.Add(descriptor);
                byteOffset += model.DataSizeInBytes;
            }
            return new SerializedModels
            {
                data = dataBuffer.data, dataDescriptors = modelDescriptors
            };
        }
    }

    /// <summary>
    ///     Serializes all data pertaining to a single model as required for streaming to VisionLib.
    ///     This includes the mesh geometry itself, the mesh's texture coordinates (UVs) and
    ///     the texture. The latter two are only included if both are present.
    /// </summary>
    /// <member name="BinaryData">
    ///     Single byte array containing the serialized model data.
    ///     Data Structure (in this order):
    ///     - vertexCount * 3 float: vertices
    ///     - triangleIndexCount UInt32: face indices
    ///     - normalCount * 3 float: normals
    ///     - uvCount * 2 float: uvs (vertex texture coordinates)
    ///     - textureWidth * textureHeight * 4 float: raw texture data
    ///       in RGBA format (r-g-b-a-r-g-b-a-... etc.)
    /// </member>
    /// <member name="SerializedModelDataDescriptor">
    ///     The <see cref="ModelDataDescriptor"/> needed to properly interpret
    ///     the <see cref="BinaryData"/>.
    /// </member>
    /// <member name="DataSizeInBytes">
    ///     Size of the serialized data.
    /// </member>
    /// exception NotInitializedException
    /// @ingroup Core
    public class SerializedModel
    {
        public class NotInitializedException : Exception
        {
            public NotInitializedException(string message)
                : base(message) {}
        }

        public byte[] BinaryData
        {
            get => this.serializedModelDataBuffer.data;
        }

        public int DataSizeInBytes
        {
            get
            {
                return this.serializedModelDataDescriptor.shape.DataSizeInBytes;
            }
        }
        
        public readonly ModelDataDescriptor serializedModelDataDescriptor;
        private readonly BinaryDataBuffer serializedModelDataBuffer;

        private struct SerializationResult
        {
            public BinaryDataDescriptor descriptor;
            public BinaryDataBuffer dataBuffer;
        }

        /// <summary>
        ///     Constructor automatically extracts the raw texture data from compatible textures
        ///     in RGBA format.
        ///     (Supported texture formats see <see cref="TextureHelpers.CreateRGBACopy"/>.) 
        /// </summary>
        /// <param name="parentGameObject">
        ///     GameObject to which the mesh and texture belong.
        ///     (E.g. the parent GameObject)
        /// </param>
        /// <param name="modelID">
        ///     Unique ID to assign to the Mesh. If the ID is not unique and another model with the
        ///     same ID is loaded for tracking, VisionLib will refuse any attempts to load this
        ///     model for tracking. 
        /// </param>
        /// <param name="mesh">
        ///     A mesh.
        /// </param>
        /// <param name="texture">
        ///     Reference to the texture belonging to the mesh.
        /// </param>
        public SerializedModel(
            GameObject parentGameObject,
            string modelID,
            Mesh mesh,
            Texture2D texture)
        {
            var serializationResult = Serialize(mesh, texture, parentGameObject);
            this.serializedModelDataBuffer = serializationResult.dataBuffer;
            this.serializedModelDataDescriptor = new ModelDataDescriptor
            {
                name = modelID, shape = serializationResult.descriptor
            };
        }

        public SerializedModel(ModelSerialization.ModelDataSet modelDataSet)
            : this(
                modelDataSet.transform.gameObject,
                modelDataSet.transform.GetInstanceID().ToString(),
                modelDataSet.mesh,
                modelDataSet.texture)
        {}

        private static IEnumerable<(SubMeshDescriptor subMesh, int idx)> EnumerateSubMeshes(
            Mesh mesh,
            MeshTopology topology)
        {
            for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; subMeshIdx++)
            {
                var subMesh = mesh.GetSubMesh(subMeshIdx);
                if (subMesh.topology == topology)
                    yield return (subMesh, subMeshIdx);
            }
        }

        private static SerializationResult Serialize(
            Mesh mesh,
            Texture2D texture,
            GameObject parentGameObject)
        {
            var dataDescriptor = new BinaryDataDescriptor
            {
                binaryOffset = 0,
                vertexCount = mesh.vertexCount,
                triangleIndexCount = 0,
                lineIndexCount = 0,
                normalCount = mesh.normals.Length,
                uvCount = 0,
                textureHeight = 0,
                textureWidth = 0
            };

            var tmpUVs = new List<Vector2>();
            mesh.GetUVs(0, tmpUVs);
            dataDescriptor.uvCount = tmpUVs.Count;

            var tmpTextureData = Array.Empty<byte>();

            var loadingTextureDataSucceeded = false;
            if (texture)
            {
                if (dataDescriptor.uvCount > 0)
                {
                    try
                    {
                        dataDescriptor.textureWidth = texture.width;
                        dataDescriptor.textureHeight = texture.height;
                        tmpTextureData = ExtractRawDataInRGBAFormat(texture);
                        loadingTextureDataSucceeded = true;
                    }
                    catch (TextureHelpers.UnsupportedTextureFormatException e)
                    {
                        LogHelper.LogWarning(
                            "Texture could not be converted to RGBA. " +
                            "Continuing without texture data.\n" +
                            "(Conversion failed with message: \"" + e.Message + "\")",
                            parentGameObject);
                    }
                }
                else
                {
                    LogHelper.LogWarning(
                        "The model was added without its texture since no corresponding" +
                        " UV map could be found.",
                        parentGameObject);
                }
            }

            if (!loadingTextureDataSucceeded)
            {
                dataDescriptor.textureWidth = 0;
                dataDescriptor.textureHeight = 0;
                dataDescriptor.uvCount = 0;
            }

            Func<Vector3, float[]> flipXAndConvertToFloatArray =
                vec => new float[] {-vec.x, vec.y, vec.z};
            foreach (var (subMesh, _) in EnumerateSubMeshes(mesh, MeshTopology.Lines))
            {
                dataDescriptor.lineIndexCount += subMesh.indexCount;
            }
            foreach (var (subMesh, _) in EnumerateSubMeshes(mesh, MeshTopology.Triangles))
            {
                dataDescriptor.triangleIndexCount += subMesh.indexCount;
            }

            var dataBuffer = new BinaryDataBuffer(dataDescriptor.DataSizeInBytes);
            dataBuffer.Append(
                mesh.vertices.SelectMany(flipXAndConvertToFloatArray).ToArray(),
                sizeof(float));

            foreach (var (_, idx) in EnumerateSubMeshes(mesh, MeshTopology.Triangles))
            {
                dataBuffer.Append(mesh.GetIndices(idx), sizeof(UInt32));
            }

            dataBuffer.Append(
                mesh.normals.SelectMany(flipXAndConvertToFloatArray).ToArray(),
                sizeof(float));

            foreach (var (_, idx) in EnumerateSubMeshes(mesh, MeshTopology.Lines))
            {
                dataBuffer.Append(mesh.GetIndices(idx), sizeof(UInt32));
            }

            //not inverted to avoid skipping potential future data additions below
            if (dataDescriptor.textureWidth > 0 && dataDescriptor.textureHeight > 0)
            {
                dataBuffer.Append(
                    tmpUVs.SelectMany(uv => new float[] {uv.x, 1 - uv.y}).ToArray(),
                    sizeof(float));
                dataBuffer.Append(tmpTextureData, sizeof(byte));
            }

            return new SerializationResult {descriptor = dataDescriptor, dataBuffer = dataBuffer};
        }

        private static byte[] ExtractRawDataInRGBAFormat(Texture2D texture)
        {
            var tmpTexture = TextureHelpers.CreateRGBACopy(texture);
            var data = tmpTexture.GetRawTextureData();
            UnityEngine.Object.DestroyImmediate(tmpTexture);
            return data;
        }
    }
}
