using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.API.Native
{
    /// <summary>
    /// Enum values describing how the video image acquired through VLSDK calls has
    /// to be rotated to match ScreenOrientation. The default render rotation for
    /// a given environment can be obtained by calling CameraHelper.GetRotationMatrix.
    /// </summary>
    /// @ingroup Native
    public enum RenderRotation
    {
        CCW0 = 0,
        CCW90 = 2,
        CCW180 = 1,
        CCW270 = 3
    }

    public static class RenderRotationHelper
    {
        /// <summary>
        ///  Transformation matrix with a 0 degree rotation around the z-axis
        ///  (identity matrix).
        /// </summary>
        public static readonly Matrix4x4 rotationZ0 = Matrix4x4.identity;
        /// <summary>
        ///  Transformation matrix with a 90 degree rotation around the z-axis.
        /// </summary>
        internal static readonly Matrix4x4 rotationZ90 = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.AngleAxis(90.0f, Vector3.forward),
            Vector3.one);
        /// <summary>
        ///  Transformation matrix with a 180 degree rotation around the z-axis.
        /// </summary>
        internal static readonly Matrix4x4 rotationZ180 = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.AngleAxis(180.0f, Vector3.forward),
            Vector3.one);
        /// <summary>
        ///  Transformation matrix with a 270 degree rotation around the z-axis.
        /// </summary>
        internal static readonly Matrix4x4 rotationZ270 = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.AngleAxis(270.0f, Vector3.forward),
            Vector3.one); 

        // Given a ScreenOrientation, returns an enum value describing how the
        // video image has to be rotated to match.
        public static RenderRotation GetRenderRotation(this ScreenOrientation orientation)
        {
#if ((UNITY_WSA_10_0 || UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
            // On UWP, Android and iOS, the default screen orientation is landscape.
            switch (orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    return RenderRotation.CCW0;
                case ScreenOrientation.PortraitUpsideDown:
                    return RenderRotation.CCW90;
                case ScreenOrientation.LandscapeRight:
                    return RenderRotation.CCW180;
                case ScreenOrientation.Portrait:
                    return RenderRotation.CCW270;
            }
#else
            // On the desktop the default screen orientation in Unity is portrait.
            switch (orientation)
            {
                case ScreenOrientation.Portrait:
                    return RenderRotation.CCW0;
                case ScreenOrientation.LandscapeLeft:
                    return RenderRotation.CCW90;
                case ScreenOrientation.PortraitUpsideDown:
                    return RenderRotation.CCW180;
                case ScreenOrientation.LandscapeRight:
                    return RenderRotation.CCW270;
            }
#endif
            throw new System.ArgumentException(
                $"Enum value \"{orientation}\" not supported",
                "orientation");
        }

        public static Matrix4x4 GetMatrixFromVLToUnity(this RenderRotation rotation)
        {
            switch (rotation)
            {
                case RenderRotation.CCW0:
                    return RenderRotationHelper.rotationZ0;
                case RenderRotation.CCW90:
                    return RenderRotationHelper.rotationZ90;
                case RenderRotation.CCW180:
                    return RenderRotationHelper.rotationZ180;
                case RenderRotation.CCW270:
                    return RenderRotationHelper.rotationZ270;
            }
            throw new System.ArgumentException("Enum value not in enum", "orientation");
        }

        public static Matrix4x4 GetMatrixFromUnityToVL(this RenderRotation rotation)
        {
            switch (rotation)
            {
                case RenderRotation.CCW0:
                    return RenderRotationHelper.rotationZ0;
                case RenderRotation.CCW90:
                    return RenderRotationHelper.rotationZ270;
                case RenderRotation.CCW180:
                    return RenderRotationHelper.rotationZ180;
                case RenderRotation.CCW270:
                    return RenderRotationHelper.rotationZ90;
            }
            throw new System.ArgumentException("Enum value not in enum", "orientation");
        }
    }
}
