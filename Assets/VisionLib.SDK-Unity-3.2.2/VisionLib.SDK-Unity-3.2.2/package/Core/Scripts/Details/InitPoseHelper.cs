using System;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class InitPoseHelper
    {
        [Serializable]
        public class JsonInitPose : JsonHelper.IJsonParsable
        {
            public ModelTrackerCommands.InitPose initPose;

            public static JsonInitPose Parse(string jsonString)
            {
                var initPose = JsonUtility.FromJson<JsonInitPose>(jsonString);
                if (!initPose.IsValid() && jsonString.StartsWith("{"))
                {
                    initPose = new JsonInitPose
                    {
                        initPose =
                            JsonUtility.FromJson<ModelTrackerCommands.InitPose>(jsonString)
                    };
                }
                return initPose;
            }

            public bool IsValid()
            {
                return IsValidPose(this.initPose);
            }

            public string GetJsonName()
            {
                return "Init Pose";
            }

            public string GetWarning()
            {
                return "";
            }

            public Pose? ToPose()
            {
                return TRArraysToPose(this.initPose.t, this.initPose.r);
            }

            public override string ToString()
            {
                var pose = ToPose();
                return !pose.HasValue ? "" : ToString(pose.Value);
            }

            public static string ToString(Pose pose)
            {
                return $"position = {pose.position.ToString("0.0000")}\n" +
                       $"rotation = {pose.rotation.ToString("0.0000")}";
            }
        }

        public static bool IsValidPose(ModelTrackerCommands.InitPose initPose)
        {
            return IsVector3(initPose.t) && IsVector4(initPose.r);
        }

        public static Pose VLInitPoseToUnityWorldPose(Pose vlInitPose, Camera referenceCamera)
        {
            //openCV to Unity coordinate system conversion and transformation
            //from camera coordinate system to world coordinates
            var unityWorldInitPose = referenceCamera.transform.localToWorldMatrix *
                                     CameraHelper.flipY * vlInitPose.ToMatrix() *
                                     CameraHelper.flipX;

            return unityWorldInitPose.ToPose();
        }

        public static Pose UnityWorldPoseToVLInitPose(
            Pose unityWorldInitPose,
            Camera referenceCamera)
        {
            var vlInitPose = CameraHelper.flipY *
                             Matrix4x4.Inverse(referenceCamera.transform.localToWorldMatrix) *
                             unityWorldInitPose.ToMatrix() * CameraHelper.flipX;

            return new Pose(vlInitPose.GetPosition(), vlInitPose.rotation);
        }

        public static Pose VLTrackingResultToUnityWorldPose(ModelTransform vlTrackingResult)
        {
            var unityWorldMatrix =
                CameraHelper.flipX * vlTrackingResult.ToMatrix() * CameraHelper.flipX;
            return new Pose(
                unityWorldMatrix.GetColumn(3),
                CameraHelper.QuaternionFromMatrix(unityWorldMatrix));
        }

        private static bool IsValid(this JsonInitPose initPoseContainer)
        {
            return initPoseContainer != null && IsValidPose(initPoseContainer.initPose);
        }

        private static bool IsVector3(float[] vector)
        {
            return vector != null && vector.Length == 3;
        }

        private static bool IsVector4(float[] vector)
        {
            return vector != null && vector.Length == 4;
        }

        private static Pose TRArraysToPose(float[] t, float[] r)
        {
            if (!IsVector3(t))
            {
                throw new ArgumentException("\"t\" is not a 3 element vector.");
            }
            if (!IsVector4(r))
            {
                throw new ArgumentException("\"r\" is not a 4 element vector.");
            }

            return new Pose(new Vector3(t[0], t[1], t[2]), new Quaternion(r[0], r[1], r[2], r[3]));
        }
    }
}
