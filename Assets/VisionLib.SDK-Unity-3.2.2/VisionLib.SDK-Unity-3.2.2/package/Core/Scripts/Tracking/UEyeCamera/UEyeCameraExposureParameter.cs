using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class UEyeCameraExposureParameter : UEyeCameraParameter
    {
        public UnityEvent<float> OnMinChanged = new UnityEvent<float>();
        public UnityEvent<float> OnMaxChanged = new UnityEvent<float>();

        private float maxValue = -1.0f;
        private float minValue = -1.0f;

        public UEyeCameraExposureParameter()
            : base("imageSource.exposure") {}

        public override async Task InitializeValueFromBackend()
        {
            await base.InitializeValueFromBackend();

            var maxValueFromBackend = await WorkerCommands.GetAttributeAsync<float>(
                TrackingManager.Instance.Worker,
                this.attributeName + "Max");
            this.maxValue = FloorToThreeDecimals(maxValueFromBackend);
            this.OnMaxChanged?.Invoke(this.maxValue);

            var minValueFromBackend = await WorkerCommands.GetAttributeAsync<float>(
                TrackingManager.Instance.Worker,
                this.attributeName + "Min");
            this.minValue = CeilToThreeDecimals(minValueFromBackend);
            this.OnMinChanged?.Invoke(this.minValue);
        }

        private static float FloorToThreeDecimals(float value)
        {
            return Mathf.Floor(value * 1000) / 1000;
        }

        private static float CeilToThreeDecimals(float value)
        {
            return Mathf.Ceil(value * 1000) / 1000;
        }
#if UNITY_EDITOR
        public override List<SetupIssue> CheckOnValueChangedForBrokenListeners(
            GameObject sourceGameObject = null)
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new UnityEventBase[] {this.OnValueChanged, this.OnMaxChanged, this.OnMinChanged},
                sourceGameObject);
        }
#endif
    }
}
