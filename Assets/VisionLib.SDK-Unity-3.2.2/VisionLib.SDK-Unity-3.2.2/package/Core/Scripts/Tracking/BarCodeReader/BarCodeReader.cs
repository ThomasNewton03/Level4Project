using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "bar_code_reader.html")]
    [AddComponentMenu("VisionLib/Core/Bar Code Reader")]
    public class BarCodeReader : MonoBehaviour, ISceneValidationCheck
    {
        [Serializable]
        public class RegionOfInterest
        {
            public double left = 0.0;
            public double top = 0.0;
            public double width = 1.0;
            public double height = 1.0;
        }

        [Serializable]
        public class OnJustScannedEvent : UnityEvent<string> {}

        /// <summary>
        ///  Event fired once after the code detection state changed to "found".
        /// </summary>
        public OnJustScannedEvent justScannedEvent = new OnJustScannedEvent();

        /// <summary>
        ///  Event fired once after the code detection state changed to "lost".
        /// </summary>
        public UnityEvent justLostEvent = new UnityEvent();

        private string previousValue = null;
        private int frameThreshold = 20;
        private SingletonTaskExecutor getBarCodeResultExecuter;

        public async Task SetRegionOfInterestAsync(RegionOfInterest roi)
        {
            await BarCodeReaderCommands.SetRegionOfInterest(TrackingManager.Instance.Worker, roi);
        }
        
        public void SetRegionOfInterest(RegionOfInterest roi)
        {
            TrackingManager.CatchCommandErrors(SetRegionOfInterestAsync(roi));
        }

        private async Task GetBarCodeResultAsync()
        {
            var result =
                await BarCodeReaderCommands.GetBarCodeResultAsync(TrackingManager.Instance.Worker);
            this.ThrowIfNotAliveAndEnabled();
            string currentValue = null;
            if (result.valid && result.framesSinceRecognition <= this.frameThreshold)
            {
                currentValue = result.value;
            }
            if (this.previousValue != currentValue)
            {
                if (currentValue == null)
                {
                    LogHelper.LogInfo("Bar code lost.");
                    this.justLostEvent.Invoke();
                }
                else
                {
                    LogHelper.LogInfo("Bar code found with value: " + currentValue);
                    this.justScannedEvent.Invoke(currentValue);
                }
            }
            this.previousValue = currentValue;
        }

        private void GetBarCodeResult()
        {
            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                getBarCodeResultExecuter.TryExecute();
            }
        }

        void Start()
        {
            getBarCodeResultExecuter = new SingletonTaskExecutor(GetBarCodeResultAsync, this);
        }

        void Update()
        {
            this.GetBarCodeResult();
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new UnityEventBase[] {this.justLostEvent, this.justScannedEvent},
                this.gameObject);
        }
#endif
    }
}
