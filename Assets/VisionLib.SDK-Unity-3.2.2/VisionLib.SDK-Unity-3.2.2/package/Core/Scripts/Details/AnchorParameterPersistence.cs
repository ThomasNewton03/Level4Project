using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    internal static class AnchorParameterPersistence
    {
        [Serializable]
        private class PersistentObject<TObjectType>
        {
            [SerializeField]
            public bool persist;
            [SerializeField]
            public bool hasValue;
            [SerializeField]
            public TObjectType objectValue;
        }

        [Serializable]
        private class PersistentData
        {
            [SerializeField]
            public PersistentObject<AnchorRuntimeParameters> parameter;
            [SerializeField]
            public PersistentObject<Pose> initPose;
        }

        static AnchorParameterPersistence()
        {
            EditorApplication.playModeStateChanged += PersistAnchorParameters;
        }

        private static void PersistAnchorParameters(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    DoOnEachAnchorWithPersistentParameters(SaveParameters);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    DoOnEachAnchorWithPersistentParameters(LoadParameters);
                    break;
            }
        }

        private static void DoOnEachAnchorWithPersistentParameters(
            Action<TrackingAnchor, string> handleData)
        {
            var trackingAnchors = Object.FindObjectsOfType<TrackingAnchor>();
            foreach (var anchor in trackingAnchors)
            {
                var fullPath = Path.Combine(
                    Application.temporaryCachePath,
                    SceneManager.GetActiveScene().name + "_" + anchor.GetAnchorName() +
                    "_parameters.json");

                handleData(anchor, fullPath);
            }
        }

        private static void SaveParameters(TrackingAnchor anchor, string uri)
        {
            var currentInitPose = anchor.GetCurrentInitPoseInCameraCoordinateSystem()?.ToPose();
            var persistentData = new PersistentData
            {
                parameter =
                    new PersistentObject<AnchorRuntimeParameters>
                    {
                        persist = anchor.persistParametersFromPlayMode,
                        hasValue = true,
                        objectValue = anchor.GetAnchorRuntimeParameters()
                    },
                initPose = new PersistentObject<Pose>
                {
                    persist = anchor.persistInitPoseFromPlayMode,
                    hasValue = currentInitPose.HasValue,
                    objectValue = currentInitPose ?? new Pose()
                }
            };
            var content = JsonUtility.ToJson(
                persistentData);
            VLSDK.Set(uri, content);
            LogHelper.LogDebug($"Wrote parameters to {uri}", anchor);
        }

        private static void LoadParameters(TrackingAnchor anchor, string uri)
        {
            TrackingManager.CatchCommandErrors(LoadParametersAsync(anchor, uri), anchor);
        }

        private static Task<WorkerCommands.CommandWarnings> LoadParametersAsync(
            TrackingAnchor anchor,
            string uri)
        {
            var result = VLSDK.Get(uri);
            var deserializedData = JsonUtility.FromJson<PersistentData>(result);
            HandleInitPose(anchor, deserializedData.initPose);
            return HandleParameter(anchor, deserializedData.parameter);
        }

        private static void HandleInitPose(TrackingAnchor anchor, PersistentObject<Pose> initPose)
        {
            anchor.persistInitPoseFromPlayMode = initPose.persist;
            if (!RequireObjectUpdate(initPose, anchor.transform.ToPose()))
            {
                return;
            }
#if UNITY_EDITOR
            Undo.RecordObject(anchor, "Load InitPose");
            Undo.RecordObject(anchor.transform, "Load InitPose");
#endif
            anchor.SetInitPoseInCameraCoordinateSystem(initPose.objectValue);
            LogHelper.LogDebug($"Updated initPose in EditMode", anchor);
        }

        private static async Task<WorkerCommands.CommandWarnings> HandleParameter(
            TrackingAnchor anchor,
            PersistentObject<AnchorRuntimeParameters> parameterWithBool)
        {
            anchor.persistParametersFromPlayMode = parameterWithBool.persist;
            if (!RequireObjectUpdate(parameterWithBool, anchor.GetAnchorRuntimeParameters()))
            {
                return WorkerCommands.NoWarnings();
            }
#if UNITY_EDITOR
            Undo.RecordObject(anchor, "Load Parameters");
#endif
            var warnings = await anchor.GetAnchorRuntimeParameters()
                .SetParametersAsync(parameterWithBool.objectValue, anchor);
            LogHelper.LogDebug($"Updated parameters in EditMode", anchor);
            return warnings;
        }

        private static bool RequireObjectUpdate<TObjectType>(
            PersistentObject<TObjectType> persistentObject,
            TObjectType currentObject)
        {
            if (!persistentObject.persist || !persistentObject.hasValue)
            {
                return false;
            }
            var newParameters = persistentObject.objectValue;
            if (newParameters == null)
            {
                return false;
            }
            return JsonUtility.ToJson(newParameters) != JsonUtility.ToJson(currentObject);
        }
    }
#endif
}
