using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core
{
    internal class AngleRangeHandle
    {
        private ArcHandle angleEndHandle = new ArcHandle();
        private ArcHandle angleBeginHandle = new ArcHandle();

        private static Matrix4x4 ToMat4(Quaternion quat)
        {
            return Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);
        }

        public AngleRangeHandle(Color color, bool withRadius)
        {
            if (withRadius)
            {
                angleBeginHandle.SetColorWithRadiusHandle(color, 0.1f);
                angleEndHandle.SetColorWithoutRadiusHandle(color, 0.1f);
            }
            else
            {
                angleBeginHandle.SetColorWithoutRadiusHandle(color, 0.1f);
                angleEndHandle.SetColorWithoutRadiusHandle(color, 0.1f);
            }

            angleBeginHandle.angleHandleSizeFunction = (Vector3 v) =>
                HandleUtility.GetHandleSize(v) * 0.12f;
            angleEndHandle.angleHandleSizeFunction = (Vector3 v) =>
                HandleUtility.GetHandleSize(v) * 0.12f;
            angleBeginHandle.radiusHandleSizeFunction = (Vector3 v) =>
                HandleUtility.GetHandleSize(v) * 0.05f;
            angleEndHandle.radiusHandleSizeFunction = (Vector3 v) =>
                HandleUtility.GetHandleSize(v) * 0.05f;
        }

        public void Draw(
            ref float angleBegin,
            ref float angleLength,
            ref float radius,
            Matrix4x4 handleMatrix)
        {
            var angleEnd = angleBegin + angleLength;
            var angleOffset = (angleBegin + angleEnd) / 2;

            angleEndHandle.angle = angleEnd - angleOffset;
            angleBeginHandle.angle = angleBegin - angleOffset;
            angleEndHandle.radius = radius;
            angleBeginHandle.radius = radius;

            using (new Handles.DrawingScope(
                handleMatrix * ToMat4(Quaternion.AngleAxis(angleOffset, Vector3.up))))
            {
                angleEndHandle.DrawHandle();
                angleBeginHandle.DrawHandle();
            }

            angleBegin = Math.Max(0.0f, Math.Min(angleBeginHandle.angle + angleOffset, 360.0f));
            angleEnd = angleEndHandle.angle + angleOffset;
            angleLength = Math.Max(0.0f, Math.Min(angleEnd - angleBegin, 360.0f));

            if (radius != angleEndHandle.radius)
            {
                radius = angleEndHandle.radius;
            }
            else if (radius != angleBeginHandle.radius)
            {
                radius = angleBeginHandle.radius;
            }
        }
    }
}
