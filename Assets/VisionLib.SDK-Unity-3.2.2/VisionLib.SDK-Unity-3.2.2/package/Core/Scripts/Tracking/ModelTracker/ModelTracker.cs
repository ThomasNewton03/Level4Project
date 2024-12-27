using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  The ModelTracker contains all functions, which are specific
    ///  for the ModelTracker.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "model_tracker.html")]
    [AddComponentMenu("VisionLib/Core/Model Tracker")]
    public class ModelTracker : MonoBehaviour, ISceneValidationCheck
    {
        private static SingletonObjectReference<ModelTracker> instance =
            new SingletonObjectReference<ModelTracker>();
        public static ModelTracker Instance
        {
            get => ModelTracker.instance.Instance;
        }

        /// <summary>
        ///  Sets the modelURI to a new value and thus loads a new model.
        /// </summary>
        /// <param name="modelURI">
        ///  URI of the model, which should be used for tracking.
        /// </param>
        public async Task<WorkerCommands.CommandWarnings> SetModelURIAsync(string modelURI)
        {
            var warnings = await WorkerCommands.SetAttributeAsync(
                TrackingManager.Instance.Worker,
                "modelURI",
                modelURI);
            NotificationHelper.SendInfo("Set model URI: " + modelURI);
            return warnings;
        }

        public void Awake()
        {
            ModelTracker.instance.Instance = this;
        }

        /// <summary>
        ///  Sets the modelURI to a new value and thus loads a new model.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="modelURI">
        ///  URI of the model, which should be used for tracking.
        /// </param>
        public void SetModelURI(string modelURI)
        {
            TrackingManager.CatchCommandErrors(SetModelURIAsync(modelURI), this);
        }

        public async Task ResetTrackingHardAsync()
        {
            await ModelTrackerCommands.ResetHardAsync(TrackingManager.Instance.Worker);
#pragma warning disable CS0618 // OnTrackerResetHard is obsolete
            TrackingManager.InvokeOnTrackerResetHard();
#pragma warning restore CS0618 // OnTrackerResetHard is obsolete
            NotificationHelper.SendInfo("Tracker reset");
        }

        /// <summary>
        ///  Reset the tracking and all keyframes.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void ResetTrackingHard()
        {
            TrackingManager.CatchCommandErrors(ResetTrackingHardAsync(), this);
        }

        public async Task ResetTrackingSoftAsync()
        {
            await ModelTrackerCommands.ResetSoftAsync(TrackingManager.Instance.Worker);
#pragma warning disable CS0618 // OnTrackerResetSoft is obsolete
            TrackingManager.InvokeOnTrackerResetSoft();
#pragma warning restore CS0618 // OnTrackerResetSoft is obsolete
            NotificationHelper.SendInfo("Tracker reset init pose");
        }

        /// <summary>
        ///  Reset the tracking.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        public void ResetTrackingSoft()
        {
            TrackingManager.CatchCommandErrors(ResetTrackingSoftAsync(), this);
        }

        public async Task WriteInitDataAsync(string filePrefix = null)
        {
            await ModelTrackerCommands.WriteInitDataAsync(
                TrackingManager.Instance.Worker,
                filePrefix);
            NotificationHelper.SendInfo("Init data written");
        }

        /// <summary>
        ///  Write the captured initialization data as file to custom location
        ///  with custom name.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <remarks>
        ///  In order to avoid having to use a different file path for each
        ///  platform, the "local-storage-dir" scheme can be used as file prefix.
        ///  This scheme points to different locations depending on the platform:
        ///  * Windows: Current users home directory
        ///  * MacOS: Current users document directory
        ///  * iOS / Android: The current applications document directory
        /// </remarks>
        /// <param name="filePrefix">
        ///  Will be used as filename and path. A time stamp and the file
        ///  extension will be appended automatically. A plausible value could be
        ///  for example "local-storage-dir:MyInitData_".
        /// </param>
        public void WriteInitData(string filePrefix = null)
        {
            TrackingManager.CatchCommandErrors(WriteInitDataAsync(filePrefix), this);
        }

        public async Task ReadInitDataAsync(string uri)
        {
            await ModelTrackerCommands.ReadInitDataAsync(TrackingManager.Instance.Worker, uri);
            NotificationHelper.SendInfo("Init data read.");
        }

        /// <summary>
        ///  Loads the captured initialization data as file from a custom location.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <remarks>
        ///  In order to load init data at best use a static uri. A common way is for each
        ///  platform, is using  "local-storage-dir" scheme which can be used as file prefix.
        ///  This scheme points to different locations depending on the platform:
        ///  * Windows: Current users home directory
        ///  * MacOS: Current users document directory
        ///  * iOS / Android: The current applications document directory
        /// </remarks>
        /// <param name="uri">
        ///  Will be used as filename and path. A time stamp and the file
        ///  extension will be appended automatically. A plausible value could be
        ///  for example "local-storage-dir:MyInitData_".
        ///  </param>
        public void ReadInitData(string uri)
        {
            TrackingManager.CatchCommandErrors(ReadInitDataAsync(uri), this);
        }

        public async Task ResetInitDataAsync()
        {
            await ModelTrackerCommands.ResetInitDataAsync(TrackingManager.Instance.Worker);
            NotificationHelper.SendInfo("Init data reset.");
        }

        /// <summary>
        ///  Reset the offline initialization data.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <remarks>
        ///  In order to reset the initialization data loaded at the beginning this function  can be
        ///  called. The init data learned on the fly, will still be maintained and can be reset by
        ///  issuing a hard reset.
        /// </remarks>
        public void ResetInitData()
        {
            TrackingManager.CatchCommandErrors(ResetInitDataAsync(), this);
        }

        private Task<WorkerCommands.CommandWarnings> AddModelAsync(
            string modelName,
            string modelURI,
            bool enabled,
            bool occluder,
            bool useLines)
        {
            ModelProperties modelProperties = new ModelProperties(
                modelName,
                modelURI,
                enabled,
                occluder,
                useLines);

            return ModelTrackerCommands.AddModelAsync(
                TrackingManager.Instance.Worker,
                modelProperties);
        }

        public void AddModel(
            string modelName,
            string modelURI,
            bool enabled = true,
            bool occluder = false,
            bool useLines = false)
        {
            TrackingManager.CatchCommandErrors(
                AddModelAsync(modelName, modelURI, enabled, occluder, useLines),
                this);
        }

        public async Task<WorkerCommands.CommandWarnings> SetModelPropertyEnabledAsync(
            string name,
            bool state)
        {
            try
            {
                var warnings = await ModelTrackerCommands.SetModelPropertyEnabledAsync(
                    TrackingManager.Instance.Worker,
                    name,
                    state);
                this.ThrowIfNotAliveAndEnabled();
                NotificationHelper.SendInfo("Set model property enabled to " + state);
                return warnings;
            }
            catch (NullSingletonException) {}
            return WorkerCommands.NoWarnings();
        }

        /// <summary>
        /// Enables/Disables a specific model in the current tracker.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="name">Name of the model</param>
        /// <param name="state">Enabled (true/false)</param>
        public void SetModelPropertyEnabled(string name, bool state)
        {
            TrackingManager.CatchCommandErrors(SetModelPropertyEnabledAsync(name, state), this);
        }

        public async Task<WorkerCommands.CommandWarnings> SetModelPropertyOccluderAsync(
            string name,
            bool state)
        {
            var warnings = await ModelTrackerCommands.SetModelPropertyOccluderAsync(
                TrackingManager.Instance.Worker,
                name,
                state);
            this.ThrowIfNotAliveAndEnabled();
            NotificationHelper.SendInfo("Set model property occluder to " + state);
            return warnings;
        }

        /// <summary>
        /// Sets a specific model as occluder in the current tracker.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="name">Name of the model</param>
        /// <param name="state">Occluder (true/false)</param>
        public void SetModelPropertyOccluder(string name, bool state)
        {
            TrackingManager.CatchCommandErrors(SetModelPropertyOccluderAsync(name, state), this);
        }

        public async Task<WorkerCommands.CommandWarnings> SetModelPropertyURIAsync(
            string name,
            string uri)
        {
            var warnings = await ModelTrackerCommands.SetModelPropertyURIAsync(
                TrackingManager.Instance.Worker,
                name,
                uri);
            this.ThrowIfNotAliveAndEnabled();
            NotificationHelper.SendInfo("Set model property uri to " + uri);
            return warnings;
        }

        /// <summary>
        /// Loads a specific model for the current tracker, which is specified by an uri.
        /// This will remove all other models.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="name">Name of the model</param>
        /// <param name="uri">Path to the model file</param>
        public void SetModelPropertyURI(string name, string uri)
        {
            TrackingManager.CatchCommandErrors(SetModelPropertyURIAsync(name, uri), this);
        }

        public Task<ModelPropertiesStructure> GetModelPropertiesAsync()
        {
            return ModelTrackerCommands.GetModelPropertiesAsync(TrackingManager.Instance.Worker);
        }

        public async Task RemoveModelAsync(string name)
        {
            await ModelTrackerCommands.RemoveModelAsync(TrackingManager.Instance.Worker, name);
            NotificationHelper.SendInfo("Model " + name + " removed");
        }

        /// <summary>
        /// Removes a specific model from the current tracker.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="name">Name of the model</param>
        public void RemoveModel(string name)
        {
            TrackingManager.CatchCommandErrors(RemoveModelAsync(name), this);
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            var trackingAnchor = FindObjectOfType<TrackingAnchor>();
            var trackingCamera = FindObjectOfType<TrackingCamera>();
            if (trackingAnchor != null || (trackingCamera != null &&
                                           trackingCamera.coordinateSystemAdjustment ==
                                           TrackingCamera.CoordinateSystemAdjustment
                                               .MultiModelTracking))
            {
                return new List<SetupIssue>()
                {
                    new SetupIssue(
                        "ModelTracker component is used in MultiModelTracking setup",
                        "The ModelTracker component should only be used in legacy SingleModelTracker setups. " +
                        "Use the corresponding functions on the TrackingAnchor instead.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject,
                        new DestroyComponentAction(this))
                };
            }
            return SetupIssue.NoIssues();
        }
#endif
    }
}
