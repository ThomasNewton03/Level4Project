using System;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public class PositionUpdateDamper
    {
        private Vector3 position = Vector3.zero;
        private Vector3 positionVelocity = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        private Vector3 scale = Vector3.one;
        private Vector3 scaleVelocity = Vector3.zero;

        private bool valid = false;
        private bool propagated = false;

        public void Invalidate()
        {
            this.valid = false;
            this.propagated = false;
        }

        public void SetData(ModelTransform mt)
        {
            CameraHelper.SetUnityRotationTranslationTo(out this.rotation, out this.position, mt);
            this.scale = mt.s;
            this.valid = true;
        }

        public void SetData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.scale = scale;
            this.rotation = rotation;
            this.position = position;
            this.valid = true;
        }

        private static Vector3 ToLocalScale(Vector3 globalScale, Transform transform)
        {
            var parentScale = transform.parent?.lossyScale ?? Vector3.one;
            return new Vector3(
                globalScale.x / parentScale.x,
                globalScale.y / parentScale.y,
                globalScale.z / parentScale.z);
        }
        
        public void Slerp(float smoothTime, GameObject go)
        {
            if (!this.valid)
            {
                return;
            }
            if (!this.propagated)
            {
                go.transform.position = this.position;
                go.transform.rotation = this.rotation;
                go.transform.localScale = ToLocalScale(this.scale, go.transform);
                this.propagated = true;
            }

            go.transform.position = Vector3.SmoothDamp(
                go.transform.position,
                this.position,
                ref this.positionVelocity,
                smoothTime);

            if (smoothTime > 0)
            {
                float elapsedTime = Mathf.Min(Time.deltaTime, smoothTime);
                go.transform.rotation = Quaternion.Slerp(
                    go.transform.rotation,
                    this.rotation,
                    0.5f * (elapsedTime / smoothTime));
            }
            else
            {
                go.transform.rotation = this.rotation;
            }

            go.transform.localScale = Vector3.SmoothDamp(
                go.transform.localScale,
                ToLocalScale(this.scale, go.transform),
                ref this.scaleVelocity,
                smoothTime);
        }
    }
}
