using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    public class ImageSequenceParameters : MonoBehaviour, ISceneValidationCheck
    {
        private const string firstIndexString = "imageSequence.firstIndex";
        private const string nextIndexString = "imageSequence.nextIndex";
        private const string lastIndexString = "imageSequence.lastIndex";
        private const string stepSizeString = "imageSequence.stepSize";

        public enum PlayBack
        {
            FastReverse = -5,
            Reverse = -1,
            Pause = 0,
            Normal = 1,
            FastForward = 5
        }

        [SerializeField]
        public UnityEvent<int> onFirstIndexUpdated = new UnityEvent<int>();
        [SerializeField]
        public UnityEvent<int> onCurrentIndexUpdated = new UnityEvent<int>();
        [SerializeField]
        public UnityEvent<int> onLastIndexUpdated = new UnityEvent<int>();

        [SerializeField]
        private int firstIndex = 0;
        [SerializeField]
        private int currentIndex = 0;
        [SerializeField]
        private int lastIndex = 0;
        [SerializeField]
        private int maxIndex = 0;

        private SingletonTaskExecutor receiveCurrentIndex;

        [SerializeField]
        private PlayBack playBackSpeed = PlayBack.Normal;

        public PlayBack PlayBackSpeed
        {
            get
            {
                return this.playBackSpeed;
            }
            set
            {
                this.playBackSpeed = value;
                SetStepSize((int) this.playBackSpeed);
            }
        }

        public int GetFirstIndex()
        {
            return this.firstIndex;
        }

        public int GetCurrentIndex()
        {
            return this.currentIndex;
        }

        public int GetLastIndex()
        {
            return this.lastIndex;
        }

        public void ResetSequenceRange()
        {
            SetFirstIndex(0);
            SetLastIndex(this.maxIndex);
        }

        private void OnEnable()
        {
            this.receiveCurrentIndex = new SingletonTaskExecutor(UpdateCurrentIndex, this);
            TrackingManager.OnTrackerRunning += LoadImageSequenceParameters;
            TrackingManager.OnTrackerStopped += ClearImageSequenceParameters;

            if (TrackingManager.DoesTrackerExistAndIsRunning())
            {
                LoadImageSequenceParameters();
            }
        }

        private void Update()
        {
            this.receiveCurrentIndex.TryExecute();
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackerRunning -= LoadImageSequenceParameters;
            TrackingManager.OnTrackerStopped -= ClearImageSequenceParameters;

            ClearImageSequenceParameters();
        }

        public void SetFirstIndex(int newFirstIndex)
        {
            TrackingManager.CatchCommandErrors(SetFirstIndexAsync(newFirstIndex), this);
        }

        public void SetCurrentIndex(int nextIndex)
        {
            if (nextIndex < this.firstIndex || nextIndex > this.lastIndex)
            {
                return;
            }
            this.currentIndex = nextIndex;
            this.onCurrentIndexUpdated.Invoke(this.currentIndex);
            TrackingManager.CatchCommandErrors(
                SetParameterAsync(ImageSequenceParameters.nextIndexString, nextIndex),
                this);
        }

        public void SetLastIndex(int newLastIndex)
        {
            TrackingManager.CatchCommandErrors(SetLastIndexAsync(newLastIndex), this);
        }

        private void LoadImageSequenceParameters()
        {
            TrackingManager.CatchCommandErrors(
                Task.WhenAll(
                    UpdateFirstIndex(),
                    UpdateLastIndex(),
                    SetStepSizeAsync((int) (this.playBackSpeed))),
                this);
        }

        private void ClearImageSequenceParameters()
        {
            this.firstIndex = 0;
            this.currentIndex = 0;
            this.lastIndex = 0;
            this.maxIndex = 0;
        }

        private async Task<WorkerCommands.CommandWarnings> SetFirstIndexAsync(int newFirstIndex)
        {
            if (newFirstIndex > this.lastIndex || newFirstIndex < 0)
            {
                return WorkerCommands.NoWarnings();
            }
            this.firstIndex = newFirstIndex;
            var updateTasks = new List<Task<WorkerCommands.CommandWarnings>>();

            if (this.currentIndex < this.firstIndex)
            {
                this.currentIndex = this.firstIndex;
                updateTasks.Add(
                    SetParameterAsync(ImageSequenceParameters.nextIndexString, this.currentIndex));
            }
            updateTasks.Add(
                SetParameterAsync(ImageSequenceParameters.firstIndexString, newFirstIndex));
            var warnings = await WorkerCommands.AwaitAll(updateTasks);
            this.onFirstIndexUpdated.Invoke(newFirstIndex);
            return warnings;
        }

        private async Task UpdateFirstIndex()
        {
            this.firstIndex =
                await TrackingManager.Instance.GetParameterAsync<int>(
                    ImageSequenceParameters.firstIndexString);
            this.onFirstIndexUpdated.Invoke(this.firstIndex);
        }

        private async Task UpdateCurrentIndex()
        {
            this.currentIndex =
                await TrackingManager.Instance.GetParameterAsync<int>(
                    ImageSequenceParameters.nextIndexString);
            this.onCurrentIndexUpdated.Invoke(this.currentIndex);
        }

        private async Task<WorkerCommands.CommandWarnings> SetLastIndexAsync(int newLastIndex)
        {
            if (newLastIndex > this.maxIndex || newLastIndex <= this.firstIndex)
            {
                return WorkerCommands.NoWarnings();
            }
            this.lastIndex = newLastIndex;
            var updateTasks = new List<Task<WorkerCommands.CommandWarnings>>();

            if (this.currentIndex > this.lastIndex)
            {
                this.currentIndex = this.lastIndex;
                updateTasks.Add(
                    SetParameterAsync(ImageSequenceParameters.nextIndexString, this.currentIndex));
            }
            updateTasks.Add(
                SetParameterAsync(ImageSequenceParameters.lastIndexString, newLastIndex));
            var warnings = await WorkerCommands.AwaitAll(updateTasks);
            this.onLastIndexUpdated.Invoke(this.lastIndex);
            return warnings;
        }

        private async Task UpdateLastIndex()
        {
            this.lastIndex =
                await TrackingManager.Instance.GetParameterAsync<int>(
                    ImageSequenceParameters.lastIndexString);
            this.maxIndex = this.lastIndex;
            this.onLastIndexUpdated.Invoke(this.lastIndex);
        }

        private static Task<WorkerCommands.CommandWarnings> SetStepSizeAsync(int stepSize)
        {
            return SetParameterAsync(ImageSequenceParameters.stepSizeString, stepSize);
        }

        private void SetStepSize(int stepSize)
        {
            TrackingManager.CatchCommandErrors(
                SetParameterAsync(ImageSequenceParameters.stepSizeString, stepSize),
                this);
        }

        private static Task<WorkerCommands.CommandWarnings> SetParameterAsync(
            string parameterName,
            int parameterValue)
        {
            if (!TrackingManager.DoesTrackerExistAndIsRunning())
            {
                return Task.FromResult(WorkerCommands.NoWarnings());
            }
            return TrackingManager.Instance.SetParameterAsync(
                parameterName,
                parameterValue.ToString());
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[]
                {
                    this.onFirstIndexUpdated, this.onCurrentIndexUpdated,
                    this.onLastIndexUpdated
                },
                this.gameObject);
        }
#endif
    }
}
