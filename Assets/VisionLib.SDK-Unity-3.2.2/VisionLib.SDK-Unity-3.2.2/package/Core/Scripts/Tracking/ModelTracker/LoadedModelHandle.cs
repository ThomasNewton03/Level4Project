using System;
using System.Linq;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// A <see cref="LoadedModelHandle"> loads a model into the <see cref="Worker"> for
    /// tracking and governs the model's state.
    /// While a <see cref="LoadedModelHandle"> is inactive, it will neither reload its model nor
    /// apply any changes to a currently loaded model.
    /// 
    /// A <see cref="LoadedModelHandle"> loads its model by executing the delegate loadModelsTask.
    /// </summary>
    /// @ingroup core
    public class LoadedModelHandle
    {
        private bool busyLoadingModel;
        private bool busySettingParameters;

        private ModelSerialization.LoadedModelDescription loadedModel = null;
        private readonly TrackingObject owner;
        private ModelPropertyCache propertyCache;
        private readonly string anchorName;
        private readonly Func<string, Task<ModelSerialization.LoadedModelDescription>> loadModelsTask;

        public bool ModelStateSynchronizedWithBackend
        {
            get
            {
                return this.loadedModel != null && !this.busySettingParameters;
            }
        }

        public LoadedModelHandle(
            TrackingObject owner,
            string anchorName,
            Func<string, Task<ModelSerialization.LoadedModelDescription>> loadModelsTask)
        {
            this.owner = owner;
            this.anchorName = anchorName;
            this.busyLoadingModel = false;
            this.loadModelsTask = loadModelsTask;
            this.propertyCache = new ModelPropertyCache();

            var trackingAnchor = this.owner.GetTrackingAnchor();
            trackingAnchor.OnAnchorAdded += LoadModel;
            trackingAnchor.OnAnchorRemoved += Invalidate;
            if (trackingAnchor.IsAnchorEnabled)
            {
                LoadModel();
            }
        }

        public void SetAllParameters(ModelTransform transform, bool active, bool useAsOccluder, bool useLines)
        {
            if (!this.propertyCache.UpdateCache(transform, active, useAsOccluder, useLines))
            {
                return;
            }
            if (this.loadedModel != null)
            {
                UpdateModelProperties();
            }
        }

        public void Clear()
        {
            UnloadModel();

            if (!this.owner)
            {
                return;
            }
            var trackingAnchor = this.owner.GetTrackingAnchor();
            if (!trackingAnchor)
            {
                return;
            }
            trackingAnchor.OnAnchorRemoved -= Invalidate;
            trackingAnchor.OnAnchorAdded -= LoadModel;
        }

        private void LoadModel()
        {
            TrackingManager.CatchCommandErrors(LoadModelAsync(), this.owner);
        }

        private async Task<WorkerCommands.CommandWarnings> LoadModelAsync()
        {
            if (this.busyLoadingModel)
            {
                return WorkerCommands.NoWarnings();
            }
            this.busyLoadingModel = true;
            this.owner.GetTrackingAnchor().RegisterLoadingModel(this);
            await UnloadModelAsync();
            this.owner?.GetTrackingAnchor().ThrowIfNotAlive();
            if (!this.owner)
            {
                return WorkerCommands.NoWarnings();
            }
            var warnings = await AddModelDataAsync();
            this.owner?.GetTrackingAnchor().ThrowIfNotAlive();
            if (!this.owner)
            {
                return warnings;
            }
            this.owner.GetTrackingAnchor().FinalizeLoadingModel(this);
            this.busyLoadingModel = false;
            return warnings;
        }

        private async Task<WorkerCommands.CommandWarnings> AddModelDataAsync()
        {
            try
            {
                var commandWarnings = WorkerCommands.NoWarnings();

                this.loadedModel = await this.loadModelsTask(this.anchorName);
                if (this.loadedModel != null)
                {
                    commandWarnings.warnings = this.loadedModel.warnings;
                }

                if (!this.owner)
                {
                    return commandWarnings;
                }
                this.owner.GetTrackingAnchor().ThrowIfNotAlive();

                var modelPropertiesWarnings = await UpdateModelPropertiesAsync();
                commandWarnings.Concat(modelPropertiesWarnings);
                return commandWarnings;
            }
            catch (MeshFilterExtensions.ModelNotReadableException e)
            {
                NotificationHelper.SendWarning(e.Message, e, this.owner);
            }
            return WorkerCommands.NoWarnings();
        }

        private void UpdateModelProperties()
        {
            TrackingManager.CatchCommandErrors(UpdateModelPropertiesAsync(), this.owner);
        }

        private async Task<WorkerCommands.CommandWarnings> UpdateModelPropertiesAsync()
        {
            this.busySettingParameters = true;

            var warnings = await ModelSerialization.UpdateModelPropertiesAsync(
                this.anchorName,
                this.loadedModel.name,
                this.propertyCache.RelativeTransform,
                this.propertyCache.IsEnabled,
                this.propertyCache.IsOccluder,
                this.propertyCache.UseLines);

            this.busySettingParameters = false;
            return warnings;
        }

        private void UnloadModel()
        {
            TrackingManager.CatchCommandErrors(UnloadModelAsync(), this.owner);
        }

        private async Task UnloadModelAsync()
        {
            this.propertyCache = new ModelPropertyCache();
            if (this.loadedModel == null)
            {
                return;
            }
            var modelNameToUnload = this.loadedModel.name;
            this.loadedModel = null;

            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return;
            }
            await MultiModelTrackerCommands.AnchorRemoveModelAsync(
                TrackingManager.Instance.Worker,
                this.anchorName,
                modelNameToUnload);
            NotificationHelper.SendInfo("Model " + modelNameToUnload + " removed");
        }

        private void Invalidate()
        {
            this.propertyCache = new ModelPropertyCache();
            this.loadedModel = null;
        }
    }
}
