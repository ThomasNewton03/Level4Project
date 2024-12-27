using System;
using System.Collections.Generic;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    [System.Serializable]
    public struct BinaryDataDescriptor
    {
        public int binaryOffset;
        public int vertexCount;
        public int triangleIndexCount;
        public int lineIndexCount;
        public int normalCount;
        public int colorCount;
        public int uvCount;
        public int textureHeight;
        public int textureWidth;

        public int DataSizeInBytes
        {
            get
            {
                return this.vertexCount * 3 * sizeof(float) +
                       this.triangleIndexCount * sizeof(UInt32) +
                       this.lineIndexCount * sizeof(UInt32) +
                       this.normalCount * 3 * sizeof(float) +
                       this.colorCount * 4 * sizeof(float) +
                       this.uvCount * 2 * sizeof(float) +
                       this.textureHeight * this.textureWidth * 4 * sizeof(byte);
            }
        }
    }

    /// @ingroup API
    [System.Serializable]
    public class ModelDataDescriptor
    {
        public string name;
        public bool enabled = true;
        public bool occluder = false;
        public bool useLines = false;
        public ModelTransform transform = ModelTransform.Identity();
        public BinaryDataDescriptor shape;

        public ModelDataDescriptor Clone()
        {
            return new ModelDataDescriptor
            {
                name = this.name,
                enabled = this.enabled,
                occluder = this.occluder,
                useLines = this.useLines,
                transform = this.transform,
                shape = this.shape
            };
        }
    }

    /// @ingroup API
    [System.Serializable]
    public class ModelDataDescriptorList
    {
        public List<ModelDataDescriptor> models = new List<ModelDataDescriptor>();
    }
}
