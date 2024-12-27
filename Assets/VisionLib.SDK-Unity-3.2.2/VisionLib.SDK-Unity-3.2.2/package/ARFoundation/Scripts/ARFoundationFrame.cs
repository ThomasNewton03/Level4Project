using UnityEngine;
using System;
using UnityEngine.XR.ARSubsystems;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.ARFoundation
{
    /**
     *  @ingroup ARFoundation
     */
    internal class ARFoundationFrame : InputFrame
    {
        public XRCpuImage? image = null;

        public Vector2Int targetSize = Vector2Int.zero;
        public IntrinsicData intrinsicData = null;
        public ExtrinsicData extrinsicData = null;

        private bool disposed = false;
        private MirroringDataStore mirror;

        internal ARFoundationFrame(MirroringDataStore mirror)
        {
            this.mirror = mirror;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.image != null)
                {
                    this.image.Value.Dispose();
                }
                if (this.intrinsicData != null)
                {
                    this.intrinsicData.Dispose();
                }
            }
            this.disposed = true;
        }

        unsafe public Frame Evaluate()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Frame");
            }
            if (this.image == null || !this.image.Value.valid)
            {
                return null;
            }

            Frame result = new Frame();
            // result now owns this.intrinsicData
            result.intrinsicData = this.intrinsicData;
            this.intrinsicData = null;
            // result now owns this.extrinsicData
            result.extrinsicData = this.extrinsicData;
            result.timestamp = this.image.Value.timestamp;
            this.extrinsicData = null;

            var format = TextureFormat.RGBA32;

            var conversionParams = new XRCpuImage.ConversionParams(
                this.image.Value, format, XRCpuImage.Transformation.MirrorX);

            conversionParams.outputDimensions = this.targetSize;
            this.mirror.Allocate(this.targetSize.x, this.targetSize.y);
            this.image.Value.Convert(conversionParams, this.mirror.input, this.mirror.GetSize());
            result.image = Image.CreateFromBuffer(
                this.mirror.MirrorData(),
                this.targetSize.x,
                this.targetSize.y);
            Dispose();
            return result;
        }
    }
}
