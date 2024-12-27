using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;
using static Visometry.VisionLib.SDK.Core.API.WorkerCommands;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  Commands for communicating with the multi-model tracker.
    /// </summary>
    /// @ingroup API
    public class MultiModelTrackerCommands : ModelTrackerCommands
    {
        [Serializable]
        public class AnchorAttribute
        {
            public string anchor;
            public string value;

            public AnchorAttribute(string anchorName, string value)
            {
                this.value = value;
                this.anchor = anchorName;
            }
        }

        /// <summary>
        ///  Adds a new anchor to the tracker
        /// </summary>
        public static async Task<CommandWarnings> AddAnchorAsync(
            Worker worker,
            string anchorName,
            bool enabled)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AddAnchorCommand(anchorName, enabled));
        }

        /// <summary>
        ///  Sets the parent of an anchor
        /// </summary>
        public static async Task SetAnchorParentAsync(
            Worker worker,
            string anchorName,
            string anchorParentName)
        {
            await worker.PushCommandAsync(new SetAnchorParentCommand(anchorName, anchorParentName));
        }

        /// <summary>
        ///  Removes the parent of an anchor
        /// </summary>
        public static async Task RemoveAnchorParentAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(
                new MultiModelTrackerCommand("removeAnchorParent", anchorName));
        }

        /// <summary>
        ///  Returns true if the given anchor exists; returns false otherwise
        /// </summary>
        public static async Task<bool> AnchorExistsAsync(Worker worker, string anchorName)
        {
            var anchorExistsResult = await worker.PushCommandAsync<BoolValueResult>(
                new MultiModelTrackerCommand("anchorExists", anchorName));
            return anchorExistsResult.value;
        }

        /// <summary>
        ///  Removes an existing anchor from the tracker
        /// </summary>
        public static async Task RemoveAnchorAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(new MultiModelTrackerCommand("removeAnchor", anchorName));
        }

        /// <summary>
        ///  Enables an anchor
        /// </summary>
        public static async Task EnableAnchorAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(new MultiModelTrackerCommand("enableAnchor", anchorName));
        }

        /// <summary>
        ///  Returns true if the given anchor is enabled; returns false otherwise
        /// </summary>
        public static async Task<bool> AnchorEnabledAsync(Worker worker, string anchorName)
        {
            var anchorExistsResult = await worker.PushCommandAsync<BoolValueResult>(
                new MultiModelTrackerCommand("anchorEnabled", anchorName));
            return anchorExistsResult.value;
        }

        /// <summary>
        ///  Disables an anchor
        /// </summary>
        public static async Task DisableAnchorAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(
                new MultiModelTrackerCommand("disableAnchor", anchorName));
        }

        /// <summary>
        ///  Performs a Soft Reset to a specific anchor
        /// </summary>
        public static async Task AnchorResetSoftAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(new AnchorCommand(anchorName, "resetSoft"));
        }

        /// <summary>
        ///  Performs a Hard Reset to a specific anchor
        /// </summary>
        public static async Task AnchorResetHardAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(new AnchorCommand(anchorName, "resetHard"));
        }

        public static async Task<InitPose?> AnchorGetInitPoseAsync(Worker worker, string anchorName)
        {
            var stringResult =
                await worker.PushCommandAsync(new AnchorCommand(anchorName, "getInitPose"));
            return JsonHelper.FromNullableJson<InitPose>(stringResult);
        }

        /// <summary>
        ///  Performs a SetWorkSpaceCommand to a specific anchor
        /// </summary>
        public static async Task AnchorSetWorkSpaceAsync(
            Worker worker,
            string anchorName,
            WorkSpace.Configuration config)
        {
            await worker.PushCommandAsync(new AnchorSetWorkSpacesCommand(anchorName, config));
        }

        public static async Task<CommandWarnings> AnchorAddModelAsync(
            Worker worker,
            string anchorName,
            ModelProperties modelProperties)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorAddModelCommand(anchorName, modelProperties));
        }

        public static async Task AnchorRemoveModelAsync(
            Worker worker,
            string anchorName,
            string modelName)
        {
            await worker.PushCommandAsync(new AnchorRemoveModelCommand(anchorName, modelName));
        }

        [System.Obsolete(
            "AnchorSetModelPropertyAsync is obsolete. Use the specific AnchorSetModelPropertyAsync function instead.")]
        /// \deprecated AnchorSetModelPropertyAsync is obsolete. Use the specific AnchorSetModelPropertyAsync function instead.
        public static async Task<CommandWarnings> AnchorSetModelPropertyAsync(
            Worker worker,
            string anchorName,
            ModelProperty property,
            string name,
            bool value)
        {
            switch (property)
            {
                case ModelProperty.Enabled:
                {
                    return await AnchorSetModelPropertyEnabledAsync(
                        worker,
                        anchorName,
                        name,
                        value);
                }
                case ModelProperty.Occluder:
                {
                    return await AnchorSetModelPropertyOccluderAsync(
                        worker,
                        anchorName,
                        name,
                        value);
                }
            }
            throw new ArgumentException("ModelProperty has to be either 'Enabled' or 'Occluder'");
        }

        public static async Task<CommandWarnings> AnchorSetModelPropertyEnabledAsync(
            Worker worker,
            string anchorName,
            string name,
            bool value)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetModelPropertyEnabledCommand(anchorName, name, value));
        }

        public static async Task<CommandWarnings> AnchorSetModelPropertyOccluderAsync(
            Worker worker,
            string anchorName,
            string name,
            bool value)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetModelPropertyOccluderCommand(anchorName, name, value));
        }

        public static async Task<CommandWarnings> AnchorSetModelPropertyIgnoreForZoomingAsync(
            Worker worker,
            string anchorName,
            string name,
            bool value)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetModelPropertyIgnoreForZoomingCommand(anchorName, name, value));
        }

        public static async Task<CommandWarnings> AnchorSetModelPropertyURIAsync(
            Worker worker,
            string anchorName,
            string name,
            string uri)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetModelPropertyURICommand(anchorName, name, uri));
        }

        public static async Task<CommandWarnings> AnchorSetModelPropertiesAsync(
            Worker worker,
            string anchorName,
            ModelDataDescriptorList models)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetModelPropertiesCommand(anchorName, models));
        }

        /// <summary>
        ///  Sets an attribute of a specific anchor.
        /// </summary>
        public static async Task<CommandWarnings> AnchorSetAttributeAsync(
            Worker worker,
            string attributeName,
            List<AnchorAttribute> anchorValueList)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetAttributeCommand(attributeName, anchorValueList));
        }

        /// <summary>
        ///  Gets the current attribute value on a specific anchor.
        /// </summary>
        public static async Task<T> AnchorGetAttributeAsync<T>(
            Worker worker,
            string anchorName,
            string attributeName)
        {
            var resultString = await worker.PushCommandAsync(
                new GetAttributeSeparatelyCommand(attributeName));
            var jObjects = JArray.Parse(resultString);

            var anchorAttribute = jObjects.Children()
                .Select(childToken => childToken.ToObject<AnchorAttribute>())
                .First(attribute => attribute.anchor == anchorName);

            return JsonHelper.ParseJsonValueFromBackendAs<T>(anchorAttribute.value);
        }

        /// <summary>
        /// Resets the parameter of a specific anchor to its default value
        /// </summary>
        public static async Task<CommandWarnings> AnchorResetParameterAsync(
            Worker worker,
            string anchorName,
            string parameterName)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorResetParameterCommand(anchorName, parameterName));
        }

        public static async Task<ModelDeserializationResultList> AnchorAddModelDataAsync(
            Worker worker,
            string anchorName,
            ModelDataDescriptorList models,
            IntPtr data,
            UInt32 dataSize)
        {
            return await worker.PushCommandAsync<ModelDeserializationResultList>(
                new JsonAndBinaryCommandBase(
                    new AnchorAddModelDataCommand(anchorName, new AddModelDataCmd(models)),
                    data,
                    dataSize));
        }

        /// <summary>
        ///  Sets the init pose of the given anchor
        /// </summary>
        public static async Task<CommandWarnings> SetInitPoseAsync(
            Worker worker,
            string anchorName,
            InitPose initPose)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetInitPoseCommand(anchorName, initPose));
        }

        /// <summary>
        ///  Sets the global object pose of the given anchor
        /// </summary>
        public static async Task<CommandWarnings> SetGlobalObjectPoseAsync(
            Worker worker,
            string anchorName,
            InitPose globalObjectPose)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetGlobalObjectPoseCommand(anchorName, globalObjectPose));
        }

        /// <summary>
        ///  Sets the relative init pose of the given anchor
        /// </summary>
        public static async Task<CommandWarnings> SetRelativeInitPoseAsync(
            Worker worker,
            string anchorName,
            InitPose initPose)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorSetRelativeInitPoseCommand(anchorName, initPose));
        }

        /// <summary>
        ///  Disables the use of the relative init pose for the anchor.
        /// </summary>
        public static async Task<CommandWarnings> DisableRelativeInitPoseAsync(
            Worker worker,
            string anchorName)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorCommand(anchorName, "setRelativeInitPose"));
        }

        /// <summary>
        ///  Disables the use of an init pose for the anchor.
        /// </summary>
        public static async Task<CommandWarnings> DisableInitPoseAsync(
            Worker worker,
            string anchorName)
        {
            return await worker.PushCommandAsync<CommandWarnings>(
                new AnchorCommand(anchorName, "setInitPose"));
        }

        public static async Task Set1DRotationConstraintAsync(
            Worker worker,
            string anchorName,
            Vector3 worldUpVector,
            Vector3 modelUpVector,
            Vector3 modelCenter)
        {
            await worker.PushCommandAsync(
                new AnchorSet1DRotationConstraintCommand(
                    anchorName,
                    worldUpVector,
                    modelUpVector,
                    modelCenter));
        }

        public static async Task DisableConstraintAsync(Worker worker, string anchorName)
        {
            await worker.PushCommandAsync(new AnchorCommand(anchorName, "disableConstraints"));
        }

        [Serializable]
        private class MultiModelTrackerCommand : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string anchorName;
            }

            public Param param = new Param();

            public MultiModelTrackerCommand(string commandName, string anchorName)
                : base(commandName)
            {
                this.param.anchorName = anchorName;
            }
        }

        [Serializable]
        private class AddAnchorCommand : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string name;
                public bool enabled;
            }

            public Param param = new Param();

            public AddAnchorCommand(string anchorName, bool enabled)
                : base("addAnchor")
            {
                this.param.name = anchorName;
                this.param.enabled = enabled;
            }
        }

        [Serializable]
        private class SetAnchorParentCommand : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string anchorName;
                public string parentAnchorName;
            }

            public Param param = new Param();

            public SetAnchorParentCommand(string anchorName, string parentName)
                : base("setAnchorParent")
            {
                this.param.anchorName = anchorName;
                this.param.parentAnchorName = parentName;
            }
        }

        [Serializable]
        protected class AnchorCommand : CommandBase
        {
            [Serializable]
            public class Param
            {
                public string anchorName;
                public CommandBase content;
            }

            public Param param = new Param();

            public AnchorCommand(string anchorName, string commandName)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new CommandBase(commandName);
            }
        }

        [Serializable]
        private class GetAttributeSeparatelyCommand : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string att;
            }

            public Param param = new Param();

            public GetAttributeSeparatelyCommand(string parameterName)
                : base("getAttributeSeparately")
            {
                this.param.att = parameterName;
            }
        }

        [Serializable]
        private class AnchorAddModelCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public AddModelCmd content;
            }

            public AnchorAddModelCommand(string anchorName, ModelProperties properties)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new AddModelCmd(properties);
            }
        }

        [Serializable]
        private class AnchorRemoveModelCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public RemoveModelCmd content;
            }

            public AnchorRemoveModelCommand(string anchorName, string modelName)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new RemoveModelCmd(modelName);
            }
        }

        [Serializable]
        private class AnchorSetWorkSpacesCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetWorkSpacesCmd content;
            }

            public AnchorSetWorkSpacesCommand(string anchorName, WorkSpace.Configuration config)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetWorkSpacesCmd(config);
            }
        }

        [Serializable]
        private class AnchorSetModelPropertyEnabledCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetModelPropertyEnabledCmd content;
            }

            public AnchorSetModelPropertyEnabledCommand(
                string anchorName,
                string name,
                bool enabled)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetModelPropertyEnabledCmd(name, enabled);
            }
        }

        [Serializable]
        private class AnchorSetModelPropertyOccluderCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetModelPropertyOccluderCmd content;
            }

            public AnchorSetModelPropertyOccluderCommand(
                string anchorName,
                string name,
                bool occluder)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetModelPropertyOccluderCmd(name, occluder);
            }
        }

        [Serializable]
        private class AnchorSetModelPropertyIgnoreForZoomingCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetModelPropertyIgnoreForZoomingCmd content;
            }

            public AnchorSetModelPropertyIgnoreForZoomingCommand(
                string anchorName,
                string name,
                bool ignoreForZooming)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content =
                    new SetModelPropertyIgnoreForZoomingCmd(name, ignoreForZooming);
            }
        }

        [Serializable]
        private class AnchorSetModelPropertyURICommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetModelPropertyURICmd content;
            }

            public AnchorSetModelPropertyURICommand(string anchorName, string name, string uri)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetModelPropertyURICmd(name, uri);
            }
        }

        [Serializable]
        private class AnchorSetModelPropertiesCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetMultipleModelPropertiesCmd content;
            }

            public AnchorSetModelPropertiesCommand(
                string anchorName,
                ModelDataDescriptorList models)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetMultipleModelPropertiesCmd(models);
            }
        }

        [Serializable]
        protected class AnchorSetAttributeCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string name;
                public AnchorAttribute[] values;
            }

            public AnchorSetAttributeCommand(
                string attributeName,
                List<AnchorAttribute> anchorValueList)
                : base("setAttributeSeparately")
            {
                this.param.name = attributeName;
                this.param.values = anchorValueList.ToArray();
            }
        }

        [Serializable]
        protected class BinaryAnchorCommandDescription : JsonAndBinaryCommandBase.DescriptionBase
        {
            [Serializable]
            public struct Param
            {
                public string anchorName;
                public CommandBase content;
            }

            public Param param = new Param();

            public BinaryAnchorCommandDescription(string anchorName, string commandName)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new CommandBase(commandName);
            }
        }

        [Serializable]
        protected class AnchorAddModelDataCommand : JsonAndBinaryCommandBase.DescriptionBase
        {
            [Serializable]
            public struct Param
            {
                public string anchorName;
                public AddModelDataCmd content;
            }

            public Param param = new Param();

            public AnchorAddModelDataCommand(string anchorName, AddModelDataCmd command)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = command;
            }
        }

        [Serializable]
        private class AnchorSetGlobalObjectPoseCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetGlobalObjectPoseCmd content;
            }

            public AnchorSetGlobalObjectPoseCommand(string anchorName, InitPose initPose)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetGlobalObjectPoseCmd(initPose);
            }
        }

        [Serializable]
        private class AnchorSetInitPoseCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetInitPoseCmd content;
            }

            public AnchorSetInitPoseCommand(string anchorName, InitPose initPose)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetInitPoseCmd(initPose);
            }
        }

        [Serializable]
        private class AnchorSetRelativeInitPoseCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public SetRelativeInitPoseCmd content;
            }

            public AnchorSetRelativeInitPoseCommand(string anchorName, InitPose initPose)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new SetRelativeInitPoseCmd(initPose);
            }
        }

        /// <summary>
        ///  Result of command returning a bool value
        /// </summary>
        [Serializable]
        private struct BoolValueResult
        {
            public bool value;
        }

        [Serializable]
        private class AnchorSet1DRotationConstraintCommand : CommandBase
        {
            [Serializable]
            public struct Param
            {
                public string anchorName;
                public Set1DRotationConstraintCommand content;
            }

            public Param param;

            public AnchorSet1DRotationConstraintCommand(
                string anchorName,
                Vector3 worldUpVector,
                Vector3 modelUpVector,
                Vector3 modelCenter)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new Set1DRotationConstraintCommand(
                    worldUpVector,
                    modelUpVector,
                    modelCenter);
            }
        }

        [Serializable]
        private class AnchorResetParameterCommand : CommandBase
        {
            public Param param = new Param();

            [Serializable]
            public struct Param
            {
                public string anchorName;
                public ResetParameterCmd content;
            }

            public AnchorResetParameterCommand(string anchorName, string parameterName)
                : base("anchorCommand")
            {
                this.param.anchorName = anchorName;
                this.param.content = new ResetParameterCmd(parameterName);
            }
        }
    }
}
