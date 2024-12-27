using UnityEngine;
using System;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    internal class SphereHandle
    {
        private AngleRangeHandle angleRangePhi;
        private AngleRangeHandle angleRangeTheta;

        private Matrix4x4 GetRotMat(float angle, Vector3 axis)
        {
            return Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one);
        }

        public SphereHandle(bool withRadius)
        {
            this.angleRangePhi = new AngleRangeHandle(Color.red, withRadius);
            this.angleRangeTheta = new AngleRangeHandle(Color.green, false);
        }

        public void Draw(
            ref float phiStart,
            ref float phiLength,
            ref float thetaStart,
            ref float thetaLength,
            ref float radius,
            Matrix4x4 handleMatrix)
        {
            var handleMatrixPhi = GetRotMat(90, Vector3.up) * CameraHelper.flipYZ;
            angleRangePhi.Draw(
                ref phiStart,
                ref phiLength,
                ref radius,
                handleMatrix * handleMatrixPhi);

            var handleMatrixTheta = GetRotMat(-phiStart - phiLength / 2 + 90, Vector3.up) *
                                    GetRotMat(90, Vector3.left) * GetRotMat(90, Vector3.forward);
            angleRangeTheta.Draw(
                ref thetaStart,
                ref thetaLength,
                ref radius,
                handleMatrix * handleMatrixTheta);
            if (thetaStart + thetaLength > 180)
            {
                thetaLength = 180.0f - thetaStart;
            }
            thetaStart = Math.Min(180.0f, thetaStart);
            radius = Math.Max(0.0f, radius);
        }
    }
}
