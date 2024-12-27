using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using UnityEngine;
using UnityEngine.Events;

namespace Visometry.VisionLib.SDK.Core
{
    [Serializable]
    public class UEyeCameraParameter
    {
        [SerializeField]
        public UnityEvent<float> OnValueChanged = new UnityEvent<float>();

        [SerializeField]
        protected string attributeName;

        private float value = -1.0f;
        private bool updateInProgress = false;
        
        public UEyeCameraParameter(string attributeName)
        {
            this.attributeName = attributeName;
        }

        public async Task<WorkerCommands.CommandWarnings> SetAsync(float newValue)
        {
            if (Math.Abs(newValue - this.value) < float.Epsilon)
            {
                return WorkerCommands.NoWarnings();
            }
            if (this.updateInProgress)
            {
                return WorkerCommands.NoWarnings();
            }
            this.updateInProgress = true;

            try
            {
                var warnings = await SetAttributeAsync(
                    this.attributeName,
                    newValue.ToString(CultureInfo.InvariantCulture));
                this.value = newValue;
                this.OnValueChanged?.Invoke(newValue);
                return warnings;
            }
            finally
            {
                this.updateInProgress = false;
            }
        }

        public virtual async Task InitializeValueFromBackend()
        {
            this.value = await WorkerCommands.GetAttributeAsync<float>(TrackingManager.Instance.Worker,this.attributeName);
            this.OnValueChanged?.Invoke(this.value);
        }

        public static async Task<WorkerCommands.CommandWarnings> SetAttributeAsync(
            string attributeName,
            string value)
        {
            return await WorkerCommands.SetAttributeAsync(
                TrackingManager.Instance.Worker,
                attributeName,
                value);
        }

#if UNITY_EDITOR
        public virtual List<SetupIssue> CheckOnValueChangedForBrokenListeners(
            GameObject sourceGameObject = null)
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new UnityEventBase[] {this.OnValueChanged},
                sourceGameObject);
        }
#endif
    }
}
