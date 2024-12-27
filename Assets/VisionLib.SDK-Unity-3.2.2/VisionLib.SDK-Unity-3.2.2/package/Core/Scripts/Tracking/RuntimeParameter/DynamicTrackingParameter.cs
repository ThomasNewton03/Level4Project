using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    public interface IParameterHandler
    {
        public bool ActiveInBackend();

        public Task<WorkerCommands.CommandWarnings> SetParameterAsync(
            string parameterName,
            string parameterValue);

        Task<T> GetParameterAsync<T>(string parameterName);
    }

    public interface ITrackingParameter
    {
        public void SetUseValueFromUnity(bool useValueFromUnity);
        public void EmitOnValueChangedWithCurrentValue();
        public Task<WorkerCommands.CommandWarnings> ResetAsync(IParameterHandler parameterHandler);
        public Task<WorkerCommands.CommandWarnings> UpdateInBackendAsync(
            IParameterHandler parameterHandler);
#if UNITY_EDITOR
        public List<SetupIssue> CheckOnValueChangedForBrokenListeners(
            GameObject sourceGameObject = null);
#endif
        public string GetDescriptiveName();
        public string GetNativeName();
        public string GetDescription();
    }

    [Serializable]
    public abstract class DynamicTrackingParameter<T> : ITrackingParameter
    {
        private bool updateInProgress;

        [SerializeField]
        private bool useValueFromUnity;

        [SerializeField]
        protected T value;

        [SerializeField]
        public UnityEvent<T> onValueChanged = new UnityEvent<T>();

        protected DynamicTrackingParameter(T defaultValue)
        {
            this.value = defaultValue;
        }

        public async Task<WorkerCommands.CommandWarnings> SetValueAsync(
            T newValue,
            IParameterHandler parameterHandler)
        {
            if (Equals(this.value, newValue))
            {
                return WorkerCommands.NoWarnings();
            }
            SetValueInternal(newValue);
            if (parameterHandler.ActiveInBackend())
            {
                return await UpdateInBackendAsync(parameterHandler);
            }
            EmitOnValueChangedWithCurrentValue();
            return WorkerCommands.NoWarnings();
        }

        public T GetValue()
        {
            return this.value;
        }

        /// <summary>
        /// This function sets the value without writing it to the backend.
        /// The parameter will be marked as "useValueFromUnity".
        /// </summary>
        /// <param name="newValue"></param>
        internal void SetValueInternal(T newValue)
        {
            this.value = newValue;
            SetUseValueFromUnity(true);
        }

        /// <summary>
        /// This function sets the value without writing it to the backend.
        /// The "useValueFromUnity" will also be copied.
        /// </summary>
        /// <param name="other"></param>
        internal void SetValueInternal(DynamicTrackingParameter<T> other)
        {
            SetValueInternal(other.value);
            SetUseValueFromUnity(other.useValueFromUnity);
        }

        /// <summary>
        /// Returns the json key-value-pair string
        /// </summary>
        /// <returns></returns>
        internal string GetJsonStringIfUsed()
        {
            if (this.useValueFromUnity)
            {
                return GetNativeName() + ": " + GetValue();
            }
            return "";
        }

        public void SetUseValueFromUnity(bool useValueFromUnity)
        {
            this.useValueFromUnity = useValueFromUnity;
        }

        public void EmitOnValueChangedWithCurrentValue()
        {
            EmitOnValueChanged(this.value);
        }

#if UNITY_EDITOR
        public List<SetupIssue> CheckOnValueChangedForBrokenListeners(
            GameObject sourceGameObject = null)
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new UnityEventBase[] {this.onValueChanged},
                sourceGameObject);
        }
#endif

        public async Task<WorkerCommands.CommandWarnings> ResetAsync(IParameterHandler parameterHandler)
        {
            var warnings = await SetValueAsync(GetDefaultValue(), parameterHandler);
            SetUseValueFromUnity(false);
            return warnings;
        }

        protected virtual T GetIndependentValueCopy()
        {
            if (!typeof(T).IsValueType)
            {
                throw new NotSupportedException(
                    "You must override GetIndependentValueCopy for" +
                    " parameters whose values are reference types.");
            }
            return this.value;
        }

        protected void EmitOnValueChanged(T parameterValue)
        {
            this.onValueChanged?.Invoke(parameterValue);
        }

        protected virtual string GetValueString()
        {
            return this.value.ToString();
        }

        public abstract string GetDescriptiveName();

        public abstract string GetNativeName();

        public abstract string GetDescription();
        public abstract T GetDefaultValue();

        // If useValueFromUnity: Push frontend value to backend.
        // If not useValueFromUnity: Pull backend value to the frontend. 
        public async Task<WorkerCommands.CommandWarnings> UpdateInBackendAsync(
            IParameterHandler parameterHandler)
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized() ||
                !parameterHandler.ActiveInBackend())
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
                if (this.useValueFromUnity)
                {
                    var warnings = await parameterHandler.SetParameterAsync(
                        GetNativeName(),
                        GetValueString());
                    EmitOnValueChangedWithCurrentValue();
                    return warnings;
                }
                this.value = await parameterHandler.GetParameterAsync<T>(GetNativeName());
                EmitOnValueChangedWithCurrentValue();
                return WorkerCommands.NoWarnings();
            }
            finally
            {
                this.updateInProgress = false;
            }
        }
    }
}
