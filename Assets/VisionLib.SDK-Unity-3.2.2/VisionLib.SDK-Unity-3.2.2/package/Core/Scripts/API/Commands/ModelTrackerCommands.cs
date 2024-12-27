using System;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using static Visometry.VisionLib.SDK.Core.API.WorkerCommands;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the model tracker.
    /// </summary>
    /// @ingroup API
    public class ModelTrackerCommands
    {
        /// <summary>
        ///  Result of GetInitPoseCmd.
        /// </summary>
        [Serializable]
        public struct InitPose
        {
            public float[] t;
            public float[] r;

            public InitPose(Vector3 t, Quaternion r)
            {
                this.t = new float[3] {t.x, t.y, t.z};
                this.r = new float[4] {r.x, r.y, r.z, r.w};
            }

            public InitPose(ModelTransform mt)
                : this(mt.t, mt.r) {}

            public InitPose(Camera camera, RenderRotation renderRotation)
            {
                Vector4 t;
                Quaternion r;
                var worldToCameraMatrix = renderRotation.GetMatrixFromUnityToVL() *
                                          camera.worldToCameraMatrix;
                CameraHelper.WorldToCameraMatrixToVLPose(worldToCameraMatrix, out t, out r);
                this.t = new float[3] {t.x, t.y, t.z};
                this.r = new float[4] {r.x, r.y, r.z, r.w};
            }
        }

        [System.Obsolete(
            "ModelProperty is obsolete. Use the specific SetModelProperty function instead.")]
        /// <summary>
        /// Used to specify the model property which should be targeted by a command.
        /// </summary>
        /// \deprecated ModelProperty is obsolete. Use the specific SetModelProperty function instead.
        public enum ModelProperty
        {
            Enabled,
            Occluder
        }

        /// <summary>
        ///  Resets the tracking.
        /// </summary>
        public static async Task ResetSoftAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("resetSoft"));
        }

        /// <summary>
        ///  Resets the tracking and all keyframes.
        /// </summary>
        public static async Task ResetHardAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("resetHard"));
        }

        /// <summary>
        ///  Get the initial pose.
        /// </summary>
        public static async Task<InitPose?> GetInitPoseAsync(Worker worker)
        {
            var stringResult = await worker.PushCommandAsync(new CommandBase("getInitPose"));
            return JsonHelper.FromNullableJson<InitPose>(stringResult);
        }

        /// <summary>
        ///  Set the initial pose.
        /// </summary>
        public static async Task<CommandWarnings> SetInitPoseAsync(Worker worker, InitPose initPose)
        {
            return await worker.PushCommandAsync<CommandWarnings>(new SetInitPoseCmd(initPose));
        }

        /// <summary>
        ///  Sets the relative init pose.
        /// </summary>
        public static async Task<CommandWarnings> SetRelativeInitPoseAsync(
            Worker worker,
            InitPose initPose)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetRelativeInitPoseCmd(initPose));
        }

        /// <summary>
        ///  Set the global pose of the object to track.
        /// </summary>
        public static async Task<CommandWarnings> SetGlobalObjectPoseAsync(
            Worker worker,
            InitPose objectPose)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetGlobalObjectPoseCmd(objectPose));
        }

        /// <summary>
        /// Get the current state of all model tracker parameters that
        /// do not have their default value.
        /// </summary>
        public static async Task<string> GetNonDefaultAttributesAsync(Worker worker)
        {
            return await worker.PushCommandAsync(new CommandBase("getNonDefaultAttributes"));
        }

        /// <summary>
        ///  Write init data to default location (filePrefix == null) or custom location.
        /// </summary>
        public static async Task WriteInitDataAsync(Worker worker, string filePrefix = null)
        {
            if (filePrefix == null)
            {
                await worker.PushCommandAsync(new CommandBase("writeInitData"));
                return;
            }
            await worker.PushCommandAsync(new WriteInitDataWithPrefixCmd(filePrefix));
        }

        /// <summary>
        ///  Read init data from custom location with custom file name.
        /// </summary>
        public static async Task ReadInitDataAsync(Worker worker, string filePrefix)
        {
            await worker.PushCommandAsync(new ReadInitDataWithPrefixCmd(filePrefix));
        }

        /// <summary>
        ///  Reset Offline init data
        /// </summary>
        public static async Task ResetInitDataAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("resetInitData"));
        }

        public static async Task<CommandWarnings> AddModelAsync(
            Worker worker,
            ModelProperties properties)
        {
            return await worker.PushCommandAsync<CommandWarnings>(new AddModelCmd(properties));
        }

        public static async Task<ModelPropertiesStructure> GetModelPropertiesAsync(Worker worker)
        {
            return await worker.PushCommandAsync<ModelPropertiesStructure>(
                new CommandBase("getModelProperties"));
        }

        public static async Task RemoveModelAsync(Worker worker, string modelName)
        {
            await worker.PushCommandAsync(new RemoveModelCmd(modelName));
        }

        [System.Obsolete(
            "SetModelPropertyAsync is obsolete. Use the specific SetModelProperty function instead.")]
        /// \deprecated SetModelPropertyAsync is obsolete. Use the specific SetModelProperty function instead.
        public static async Task<CommandWarnings> SetModelPropertyAsync(
            Worker worker,
            ModelProperty property,
            string name,
            bool value)
        {
            switch (property)
            {
                case ModelProperty.Enabled:
                {
                    return await SetModelPropertyEnabledAsync(worker, name, value);
                }
                case ModelProperty.Occluder:
                {
                    return await SetModelPropertyOccluderAsync(worker, name, value);
                }
            }
            throw new ArgumentException("ModelProperty has to be either 'Enabled' or 'Occluder'");
        }

        public static async Task<CommandWarnings> SetModelPropertyEnabledAsync(
            Worker worker,
            string name,
            bool value)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetModelPropertyEnabledCmd(name, value));
        }

        public static async Task<CommandWarnings> SetModelPropertyOccluderAsync(
            Worker worker,
            string name,
            bool value)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetModelPropertyOccluderCmd(name, value));
        }

        public static async Task<CommandWarnings> SetModelPropertyURIAsync(
            Worker worker,
            string name,
            string uri)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetModelPropertyURICmd(name, uri));
        }

        public static async Task<CommandWarnings> SetMultipleModelPropertiesAsync(
            Worker worker,
            ModelDataDescriptorList models)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new SetMultipleModelPropertiesCmd(models));
        }

        public static async Task Set1DRotationConstraintAsync(
            Worker worker,
            Vector3 worldUpVector,
            Vector3 modelUpVector,
            Vector3 modelCenter)
        {
            await worker.PushCommandAsync(
                new Set1DRotationConstraintCommand(worldUpVector, modelUpVector, modelCenter));
        }

        public static async Task DisableConstraintAsync(Worker worker)
        {
            await worker.PushCommandAsync(new CommandBase("disableConstraints"));
        }

        /// <summary>
        ///  Set the WorkSpaces for AutoInit.
        /// </summary>
        public static async Task SetWorkSpacesAsync(Worker worker, WorkSpace.Configuration config)
        {
            await worker.PushCommandAsync(new SetWorkSpacesCmd(config));
        }

        public static async Task<ModelDeserializationResultList> AddModelDataAsync(
            Worker worker,
            ModelDataDescriptorList models,
            IntPtr data,
            UInt32 dataSize)
        {
            return await worker.PushCommandAsync<ModelDeserializationResultList>(
                new JsonAndBinaryCommandBase(new AddModelDataCmd(models), data, dataSize));
        }

        /// <summary>
        ///  Set the global coordinate system of Unity to be used in the vlSDK
        /// </summary>
        public static async Task SetGlobalCoordinateSystemAsync(
            Worker worker,
            IntPtr nativeISpatialCoordinateSystemPtr)
        {
            if (nativeISpatialCoordinateSystemPtr == IntPtr.Zero)
            {
                LogHelper.LogError("Can not set the global coordinate system to a nullptr.");
                return;
            }
            await worker.PushCommandAsync(
                new JsonAndBinaryCommandBase(
                    new JsonAndBinaryCommandBase.DescriptionBase("setGlobalCoordinateSystem"),
                    nativeISpatialCoordinateSystemPtr,
                    0));
        }

        /// <summary>
        ///  Set the extrinsic extraction callback function for OpenXR support on HoloLens
        /// </summary>
        public static async Task SetOpenXRCallbackAsync(
            Worker worker,
            IntPtr extractExtrinsicDataCallback)
        {
            await worker.PushCommandAsync(
                new JsonAndBinaryCommandBase(
                    new JsonAndBinaryCommandBase.DescriptionBase("setOpenXRCallback"),
                    extractExtrinsicDataCallback,
                    0));
        }

        /// <summary>
        /// Resets the given parameter to its default value.
        /// </summary>
        public static async Task<CommandWarnings> ResetParameterAsync(
            Worker worker,
            string parameterName)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new ResetParameterCmd(parameterName));
        }

        [Serializable]
        protected class SetInitPoseCmd : CommandBase
        {
            public InitPose param;

            public SetInitPoseCmd(InitPose initPose)
                : base("setInitPose")
            {
                this.param = initPose;
            }
        }

        [Serializable]
        protected class SetRelativeInitPoseCmd : CommandBase
        {
            public InitPose param;

            public SetRelativeInitPoseCmd(InitPose initPose)
                : base("setRelativeInitPose")
            {
                this.param = initPose;
            }
        }

        [Serializable]
        protected class AddModelCmd : CommandBase
        {
            public ModelProperties param;

            public AddModelCmd(ModelProperties properties)
                : base("addModel")
            {
                this.param = properties;
            }
        }

        [Serializable]
        protected class WriteInitDataWithPrefixCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string uri;
            }

            public Param param = new Param();

            public WriteInitDataWithPrefixCmd(string filePrefix)
                : base("writeInitData")
            {
                this.param.uri = filePrefix;
            }
        }

        [Serializable]
        protected class ReadInitDataWithPrefixCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string uri;
            }

            public Param param = new Param();

            public ReadInitDataWithPrefixCmd(string filePrefix)
                : base("readInitData")
            {
                this.param.uri = filePrefix;
            }
        }

        [Serializable]
        protected class RemoveModelCmd : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string modelName;
            }

            public Param param = new Param();

            public RemoveModelCmd(string modelName)
                : base("removeModel")
            {
                this.param.modelName = modelName;
            }
        }

        [Serializable]
        protected class SetWorkSpacesCmd : CommandBase
        {
            public WorkSpace.Configuration param;

            public SetWorkSpacesCmd(WorkSpace.Configuration config)
                : base("setWorkSpaces")
            {
                this.param = config;
            }
        }

        [Serializable]
        protected class SetMultipleModelPropertiesCmd : CommandBase
        {
            public ModelDataDescriptorList param = new ModelDataDescriptorList();

            public SetMultipleModelPropertiesCmd(ModelDataDescriptorList models)
                : base("setMultipleModelProperties")
            {
                this.param = models;
            }
        }

        [Serializable]
        protected class SetModelPropertyEnabledCmd : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string name;
                public bool enabled;
            }

            public Param param;

            public SetModelPropertyEnabledCmd(string name, bool enable)
                : base("setModelProperties")
            {
                this.param.name = name;
                this.param.enabled = enable;
            }
        }

        [Serializable]
        protected class SetModelPropertyOccluderCmd : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string name;
                public bool occluder;
            }

            public Param param;

            public SetModelPropertyOccluderCmd(string name, bool occluder)
                : base("setModelProperties")
            {
                this.param.name = name;
                this.param.occluder = occluder;
            }
        }

        [Serializable]
        protected class SetModelPropertyIgnoreForZoomingCmd : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string name;
                public bool ignoreForZooming;
            }

            public Param param;

            public SetModelPropertyIgnoreForZoomingCmd(string name, bool ignoreForZooming)
                : base("setModelProperties")
            {
                this.param.name = name;
                this.param.ignoreForZooming = ignoreForZooming;
            }
        }

        [Serializable]
        protected class SetModelPropertyURICmd : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string name;
                public string uri;
            }

            public Param param;

            public SetModelPropertyURICmd(string name, string uri)
                : base("setModelProperties")
            {
                this.param.name = name;
                this.param.uri = uri;
            }
        }

        [Serializable]
        protected class SetGlobalObjectPoseCmd : CommandBase
        {
            public InitPose param;

            public SetGlobalObjectPoseCmd(InitPose param)
                : base("setGlobalObjectPose")
            {
                this.param = param;
            }
        }

        [Serializable]
        public class Set1DRotationConstraintCommand : CommandBase
        {
            [Serializable]
            public class Parameters
            {
                public Vector3 up_world;
                public Vector3 up_model;
                public Vector3 center_model;
            }

            public Parameters param = new Parameters();

            public Set1DRotationConstraintCommand(
                Vector3 upWorld,
                Vector3 upModel,
                Vector3 centerModel)
                : base("set1DRotationConstraint")
            {
                this.param.up_world = upWorld;
                this.param.up_model = upModel;
                this.param.center_model = centerModel;
            }
        }

        [Serializable]
        protected class AddModelDataCmd : JsonAndBinaryCommandBase.DescriptionBase
        {
            public ModelDataDescriptorList param = new ModelDataDescriptorList();

            public AddModelDataCmd(ModelDataDescriptorList models)
                : base("addModelData")
            {
                this.param = models;
            }
        }

        [Serializable]
        protected class ResetParameterCmd : CommandBase
        {
            [Serializable]
            public class Parameters
            {
                public string parameterName;
            }

            public Parameters param = new Parameters();

            public ResetParameterCmd(string parameterName)
                : base("resetParameter")
            {
                this.param.parameterName = parameterName;
            }
        }
    }
}
