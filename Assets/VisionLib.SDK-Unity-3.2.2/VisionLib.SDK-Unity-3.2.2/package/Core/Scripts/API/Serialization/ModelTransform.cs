using System;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    [System.Serializable]
    public struct ModelTransform
    {
        public Vector3 t;
        public Vector3 s;
        public Quaternion r;

        public bool IsSimilarTo(
            ModelTransform other,
            float maxAngle = 0.1f,
            float maxTranslation = 0.01f)
        {
            var deltaTranslation = (this.t - other.t).magnitude;
            (this.r * Quaternion.Inverse(other.r)).ToAngleAxis(out var deltaAngle, out _);
            return deltaTranslation < maxTranslation &&
                   (deltaAngle < maxAngle || deltaAngle > (360.0f - maxAngle));
        }

        public ModelTransform RotateAroundCenter(Vector3 center, Quaternion rotation)
        {
            //read as: set "center" as new origin (-center), then apply rotation, then reset
            //origin back to where it was initially (+center)
            var rotateAroundCenter = new ModelTransform(rotation, center) *
                                     new ModelTransform(Quaternion.identity, -center);
            return rotateAroundCenter * this;
        }

        public ModelTransform(Matrix4x4 m)
        {
            s = new Vector3(
                m.GetColumn(0).magnitude,
                m.GetColumn(1).magnitude,
                m.GetColumn(2).magnitude);
            m.SetColumn(0, m.GetColumn(0) / this.s.x);
            m.SetColumn(1, m.GetColumn(1) / this.s.y);
            m.SetColumn(2, m.GetColumn(2) / this.s.z);
            r = CameraHelper.QuaternionFromMatrix(m).normalized;
            t = m.GetColumn(3);
        }

        public ModelTransform(Transform unityTransform)
            : this(unityTransform.rotation, unityTransform.position, unityTransform.lossyScale) {}

        public static ModelTransform operator *(Matrix4x4 left, ModelTransform right)
        {
            return new ModelTransform(left * right.ToMatrix());
        }

        public static ModelTransform operator *(ModelTransform left, Matrix4x4 right)
        {
            return new ModelTransform(left.ToMatrix() * right);
        }

        public static ModelTransform operator *(ModelTransform left, ModelTransform right)
        {
            return left.ToMatrix() * right;
        }

        public ModelTransform(SimilarityTransform similarityTransform)
        {
            this.r = similarityTransform.GetR();
            this.r.Normalize();
            this.t = similarityTransform.GetT();
            this.s = Vector3.one * similarityTransform.GetS();
        }

        public ModelTransform(ExtrinsicData extrinsicData)
        {
            this.r = extrinsicData.GetR();
            this.r.Normalize();
            this.t = extrinsicData.GetT();
            this.s = Vector3.one;
        }

        public ModelTransform(Quaternion rIn, Vector3 tIn, Vector3? sIn = null)
        {
            this.r = rIn;
            this.t = tIn;
            this.s = sIn ?? Vector3.one;
        }

        public ModelTransform(Transform transform, Transform referenceTransform)
        {
            if (referenceTransform == null)
            {
                this.r = transform.rotation;
                this.t = transform.position;
                this.s = TransformUtilities.GetGlobalScale(transform);
            }
            else
            {
                this = new ModelTransform(TransformUtilities.GetRelativeTransform(transform, referenceTransform));
            }
            CameraHelper.ToVLInPlace(ref this.t, ref this.r);
        }

        public ModelTransform(ModelTrackerCommands.InitPose initPose)
        {
            this.r = new Quaternion(initPose.r[0], initPose.r[1], initPose.r[2], initPose.r[3]);
            this.t = new Vector3(initPose.t[0], initPose.t[1], initPose.t[2]);
            this.s = Vector3.one;

            this = CameraHelper.flipXY * this;
        }

        public static ModelTransform Identity()
        {
            var mt = new ModelTransform();
            mt.r = Quaternion.identity;
            mt.t = Vector3.zero;
            mt.s = Vector3.one;
            return mt;
        }

        public bool IsFarAway()
        {
            return this.t.x > 100000.0 || this.t.x < -100000.0 || this.t.y > 100000.0 ||
                   this.t.y < -100000.0 || this.t.z > 100000.0 || this.t.z < -100000.0;
        }

        public Matrix4x4 ToMatrix()
        {
            return Matrix4x4.TRS(t, r, s);
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            return ToMatrix().MultiplyVector(direction);
        }

        public Vector3 TransformPoint(Vector3 point)
        {
            return ToMatrix().MultiplyPoint(point);
        }

        public ModelTransform Inverse()
        {
            return new ModelTransform(ToMatrix().inverse);
        }

        public Pose ToPose()
        {
            return new Pose(this.t, this.r);
        }
    }
}
