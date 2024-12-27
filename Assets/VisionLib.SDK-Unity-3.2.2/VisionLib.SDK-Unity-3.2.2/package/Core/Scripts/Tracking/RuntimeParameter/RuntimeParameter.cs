using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  The RuntimeParameter can be used to set tracking parameters at
    ///  runtime.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   You can use the VLRuntimeParameters prefab to create
    ///   GameObjects with the available parameters attached to them.
    ///  </para>
    ///  <para>
    ///   The <see cref="SetValue"/> functions and the
    ///   events (<see cref="stringValueChangedEvent"/>,
    ///   <see cref="intValueChangedEvent"/>, <see cref="floatValueChangedEvent"/>,
    ///   <see cref="boolValueChangedEvent"/>) can be used to modify parameters
    ///   using Unity GUI elements.
    ///  </para>
    /// </remarks>
    /// \deprecated \"RuntimeParameter\" is obsolete and will be removed in the future. Use the parameters on the TrackingAnchor instead.
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "runtime_parameter.html")]
    [Obsolete("\"RuntimeParameter\" is obsolete and will be removed in the future. Use the parameters on the TrackingAnchor instead.")]
    [Serializable]
    [AddComponentMenu("VisionLib/Obsolete/Core/Runtime Parameter")]
    public class RuntimeParameter : MonoBehaviour, ISceneValidationCheck
    {
        /// <summary>
        ///  Event with the value of a string parameter.
        /// </summary>
        [Serializable]
        public class OnStringValueChangedEvent : UnityEvent<string> {}

        /// <summary>
        ///  Event with the value of an integer parameter.
        /// </summary>
        [Serializable]
        public class OnIntValueChangedEvent : UnityEvent<int> {}

        /// <summary>
        ///  Event with the value of a floating point parameter.
        /// </summary>
        [Serializable]
        public class OnFloatValueChangedEvent : UnityEvent<float> {}

        /// <summary>
        ///  Event with the value of a boolean parameter.
        /// </summary>
        [Serializable]
        public class OnBoolValueChangedEvent : UnityEvent<bool> {}

        public enum ParameterType
        {
            String = 0,
            Int,
            Float,
            Bool
        };

        /// <summary>
        ///  Name of the tracking parameter.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Right now this value can't be changed at runtime.
        ///  </para>
        ///  <para>
        ///   The available parameter names depend on the tracking method.
        ///  </para>
        /// </remarks>
        public string parameterName;

        /// <summary>
        ///  Type of the parameter.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   Right now this value can't be changed at runtime.
        ///  </para>
        ///  <para>
        ///   Only the corresponding events will get called.
        ///  </para>
        /// </remarks>
        public ParameterType parameterType;

        /// <summary>
        ///  Whether the parameter might be changed by the internal tracking
        ///  pipeline at runtime.
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   If a parameter is changing, we will always check for changed values
        ///   and emit events for it. This check will be skipped for parameters
        ///   which are constant and events will only be emitted after the
        ///   initialization.
        ///  </para>
        /// </remarks>
        public bool changing;

        /// <summary>
        ///  Event, which will be invoked after receiving the value of a string
        ///  parameter.
        /// </summary>
        /// <remarks>
        ///  Used if <see cref="parameterType"/> is set to
        ///  <see cref="ParameterType.String"/>.
        /// </remarks>
        public OnStringValueChangedEvent stringValueChangedEvent = new OnStringValueChangedEvent();

        /// <summary>
        ///  Event, which will be invoked after receiving the value of an integer
        ///  parameter.
        /// </summary>
        /// <remarks>
        ///  Used if <see cref="parameterType"/> is set to
        ///  <see cref="ParameterType.Int"/>.
        /// </remarks>
        public OnIntValueChangedEvent intValueChangedEvent = new OnIntValueChangedEvent();

        /// <summary>
        ///  Event, which will be invoked after receiving the value of a floating
        //   point parameter.
        /// </summary>
        /// <remarks>
        ///  Used if <see cref="parameterType"/> is set to
        ///  <see cref="ParameterType.Float"/>.
        /// </remarks>
        public OnFloatValueChangedEvent floatValueChangedEvent = new OnFloatValueChangedEvent();

        /// <summary>
        ///  Event, which will be invoked after receiving the value of a boolean
        ///  parameter.
        /// </summary>
        /// <remarks>
        ///  Used if <see cref="parameterType"/> is set to
        ///  <see cref="ParameterType.Bool"/>.
        /// </remarks>
        public OnBoolValueChangedEvent boolValueChangedEvent = new OnBoolValueChangedEvent();

        private string internalParameterName;
        private ParameterType internalParameterType;
        private string parameterValue;

        private bool setting = false;
        private bool attributeRequested = false;
        private SingletonTaskExecutor getAttributeExecuter;

        private static bool parameterSettingPaused = false;

        public RuntimeParameter()
        {
            getAttributeExecuter = new SingletonTaskExecutor(GetAttributeAsync, this);
        }

        private static void PauseParameterSetting()
        {
            parameterSettingPaused = true;
        }

        private static void ResumeParameterSetting()
        {
            parameterSettingPaused = false;
        }

        /// <summary>
        ///  Set tracking parameter to given value.
        /// </summary>
        /// <remarks>
        ///  The name of the attribute can be specified with the
        ///  <see cref="parameterName"/> member variable.
        /// </remarks>
        public void SetValue(string value)
        {
            this.SetAttribute(value);
        }

        /// <summary>
        ///  Set tracking parameter to given value.
        /// </summary>
        /// <remarks>
        ///  The name of the attribute can be specified with the
        ///  <see cref="parameterName"/> member variable.
        /// </remarks>
        public void SetValue(int value)
        {
            this.SetAttribute(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///  Set tracking attribute to given value.
        /// </summary>
        /// <remarks>
        ///  The name of the attribute can be specified with the
        ///  <see cref="parameterName"/> member variable.
        /// </remarks>
        public void SetValue(float value)
        {
            this.SetAttribute(value.ToString("R", CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///  Set tracking attribute to given value.
        /// </summary>
        /// <remarks>
        ///  The name of the attribute can be specified with the
        ///  <see cref="parameterName"/> member variable.
        /// </remarks>
        public void SetValue(bool value)
        {
            if (value)
            {
                this.SetAttribute("true");
            }
            else
            {
                this.SetAttribute("false");
            }
        }

        private static bool ToBoolean(string str)
        {
            if (str == "0")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task GetAttributeAsync()
        {
            this.attributeRequested = true;
            var result = await WorkerCommands.GetAttributeAsync(
                TrackingManager.Instance.Worker,
                this.internalParameterName);
            this.ThrowIfNotAliveAndEnabled();
            PauseParameterSetting();
            if (this.parameterValue != result.value && result.value != "AMBIGUOUS" && !this.setting)
            {
                this.parameterValue = result.value;
                switch (this.internalParameterType)
                {
                    case ParameterType.String:
                        this.stringValueChangedEvent.Invoke(result.value);
                        break;
                    case ParameterType.Int:
                        this.intValueChangedEvent.Invoke(
                            Convert.ToInt32(result.value, CultureInfo.InvariantCulture));
                        this.floatValueChangedEvent.Invoke(
                            Convert.ToSingle(result.value, CultureInfo.InvariantCulture));
                        break;
                    case ParameterType.Float:
                        this.floatValueChangedEvent.Invoke(
                            Convert.ToSingle(result.value, CultureInfo.InvariantCulture));
                        break;
                    case ParameterType.Bool:
                        this.boolValueChangedEvent.Invoke(RuntimeParameter.ToBoolean(result.value));
                        break;
                    default:
                        LogHelper.LogWarning("Unknown parameter type");
                        break;
                }
            }
            ResumeParameterSetting();
        }

        private void GetAttribute()
        {
            getAttributeExecuter.TryExecute();
        }

        private async Task<WorkerCommands.CommandWarnings> SetAttributeAsync(string value)
        {
            if (parameterSettingPaused)
            {
                return WorkerCommands.NoWarnings();
            }

            this.parameterValue = value;
            this.setting = true;
            var warnings = await WorkerCommands.SetAttributeAsync(
                TrackingManager.Instance.Worker,
                this.internalParameterName,
                value);
            this.ThrowIfNotAlive();
            this.setting = false;
            return warnings;
        }

        /// <summary>
        /// Sets the attribute of this behaviour to the given value.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="value"></param>
        private void SetAttribute(string value)
        {
            TrackingManager.CatchCommandErrors(SetAttributeAsync(value), this);
        }

        private void OnTrackerInitialized()
        {
            this.setting = false;
            this.attributeRequested = false;
            this.parameterValue = null;
            this.GetAttribute();
        }

        private void Awake()
        {
            if (this.stringValueChangedEvent == null)
            {
                this.stringValueChangedEvent = new OnStringValueChangedEvent();
            }
            if (this.intValueChangedEvent == null)
            {
                this.intValueChangedEvent = new OnIntValueChangedEvent();
            }
            if (this.floatValueChangedEvent == null)
            {
                this.floatValueChangedEvent = new OnFloatValueChangedEvent();
            }
            if (this.boolValueChangedEvent == null)
            {
                this.boolValueChangedEvent = new OnBoolValueChangedEvent();
            }

            this.internalParameterName = this.parameterName;
            this.internalParameterType = this.parameterType;
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackerInitialized += OnTrackerInitialized;

            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                GetAttribute();
            }
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackerInitialized -= OnTrackerInitialized;
        }

        private void Update()
        {
            if (!TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                return;
            }

            // It's possible, that the worker is only available later. Therefore
            // we try to get the value as long as the worker is not ready or if
            // the user always wants the latest value.
            if (!this.attributeRequested || this.changing)
            {
                // Do not get the value, if we haven't received the previous value
                if (!this.setting)
                {
                    this.GetAttribute();
                }
            }
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new UnityEventBase[]
                {
                    this.stringValueChangedEvent, this.intValueChangedEvent,
                    this.floatValueChangedEvent, this.boolValueChangedEvent
                },
                this.gameObject);
        }
#endif
    }
}
