using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
#pragma warning disable CS0659
    //Disabled missing GetHashCode override warning since these (essentially) structs
    //have no immutable attributes. This precludes the required attributes-value-independent
    //instance hashing, which means base.GetHashCode has to be used anyway.

    [Serializable]
    public class LineModelParameter<T> where T : struct
    {
        internal const string trackedJsonAttributeName = "tracked";
        internal const string criticalJsonAttributeName = "critical";
        internal const string lostJsonAttributeName = "lost";

        [SerializeField]
        public T tracked;
        [SerializeField]
        public T lost;
        [SerializeField]
        public T critical;

        public LineModelParameter() {}

        public LineModelParameter(T tracked, T critical, T lost)
        {
            this.tracked = tracked;
            this.critical = critical;
            this.lost = lost;
        }

        public LineModelParameter(T value)
            : this(value, value, value) {}

        public virtual string ToJsonString()
        {
            var sharedValueForAllStates = GetSharedValueForAllStates();
            return sharedValueForAllStates.HasValue
                ? JsonHelper.ValueToJsonString(sharedValueForAllStates.Value)
                : $"{{{LineModelParameter<T>.trackedJsonAttributeName.Enquote()}:" +
                  $"{JsonHelper.ValueToJsonString(this.tracked)}," +
                  $"{LineModelParameter<T>.criticalJsonAttributeName.Enquote()}:" +
                  $"{JsonHelper.ValueToJsonString(this.critical)}," +
                  $"{LineModelParameter<T>.lostJsonAttributeName.Enquote()}:" +
                  $"{JsonHelper.ValueToJsonString(this.lost)}}}";
        }

        public virtual void SetForAllStates(T value)
        {
            this.tracked = value;
            this.lost = value;
            this.critical = value;
        }

        public T? GetSharedValueForAllStates()
        {
            if (Equals(this.tracked, this.critical) && Equals(this.critical, this.lost))
            {
                return this.tracked;
            }
            return null;
        }

        public virtual void CopyValuesFrom(LineModelParameter<T> other)
        {
            this.tracked = other.tracked;
            this.lost = other.lost;
            this.critical = other.critical;
        }

        public override bool Equals(object other)
        {
            if (other is not LineModelParameter<T> otherLineModelParameter)
            {
                return false;
            }
            return Equals(this.tracked, otherLineModelParameter.tracked) &&
                   Equals(this.critical, otherLineModelParameter.critical) && Equals(
                       this.lost,
                       otherLineModelParameter.lost);
        }
    }

    [Serializable]
    public class LineModelColor : LineModelParameter<Color>
    {
        public static readonly Color defaultLineModelColorTracked = Color.green;
        public static readonly Color defaultLineModelColorCritical = Color.yellow;
        public static readonly Color defaultLineModelColorLost = Color.red;

        [SerializeField]
        public bool perCorrespondency = false;

        public LineModelColor()
            : base(
                LineModelColor.defaultLineModelColorTracked,
                LineModelColor.defaultLineModelColorCritical,
                LineModelColor.defaultLineModelColorLost) {}

        public LineModelColor(Color value)
            : base(value) {}

        public LineModelColor(Color tracked, Color critical, Color lost)
            : base(tracked, critical, lost) {}

        public LineModelColor(LineModelParameter<Color> parameter)
        {
            base.CopyValuesFrom(parameter);
        }

        public LineModelColor(bool perCorrespondency)
            : this()
        {
            this.perCorrespondency = perCorrespondency;
        }

        public LineModelColor(bool perCorrespondency, Color tracked, Color critical, Color lost)
            : base(tracked, critical, lost)
        {
            this.perCorrespondency = perCorrespondency;
        }

        public override string ToJsonString()
        {
            return this.perCorrespondency ? "\"perCorrespondency\"" : base.ToJsonString();
        }

        public override bool Equals(object other)
        {
            if (other is not LineModelColor otherLineModelParameterColor)
            {
                return false;
            }
            return base.Equals(other) && Equals(
                this.perCorrespondency,
                otherLineModelParameterColor.perCorrespondency);
        }

        public void CopyValuesFrom(LineModelColor other)
        {
            base.CopyValuesFrom(other);
            this.perCorrespondency = other.perCorrespondency;
        }
    }

    [Serializable]
    public class ShowLineModel
    {
        internal const string enabledJsonAttributeName = "enabled";
        internal const string lineWidthJsonAttributeName = "lineWidth";
        internal const string colorJsonAttributeName = "color";

        public enum TrackingState
        {
            Tracked,
            Critical,
            Lost
        }

        public const bool defaultEnabledValue = true;
        public const int defaultLineWidth = 2;

        [SerializeField]
        public LineModelParameter<bool> enabled;
        [SerializeField]
        public LineModelColor color;
        [SerializeField]
        public LineModelParameter<int> lineWidth;

        public ShowLineModel(bool drawLineModels = false)
        {
            this.enabled = new LineModelParameter<bool>(drawLineModels);
            this.color = new LineModelColor();
            this.lineWidth = new LineModelParameter<int>(ShowLineModel.defaultLineWidth);
        }

        public static Color GetDefaultLineModelColorForState(TrackingState state)
        {
            return state switch
            {
                TrackingState.Tracked => LineModelColor.defaultLineModelColorTracked,
                TrackingState.Critical => LineModelColor.defaultLineModelColorCritical,
                TrackingState.Lost => LineModelColor.defaultLineModelColorLost,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public string ToJsonString()
        {
            return
                $"{{{ShowLineModel.enabledJsonAttributeName.Enquote()}:{this.enabled.ToJsonString()}," +
                $"{ShowLineModel.lineWidthJsonAttributeName.Enquote()}:{this.lineWidth.ToJsonString()}," +
                $"{ShowLineModel.colorJsonAttributeName.Enquote()}:{this.color.ToJsonString()}}}";
        }

        public override string ToString()
        {
            return ToJsonString();
        }

        public TriState IsEnabled()
        {
            var isEnabled = this.enabled.GetSharedValueForAllStates();
            return !isEnabled.HasValue ? TriState.Mixed :
                isEnabled.Value ? TriState.True : TriState.False;
        }

        public ShowLineModel Clone()
        {
            var clone = new ShowLineModel();
            clone.enabled.CopyValuesFrom(this.enabled);
            clone.color.CopyValuesFrom(this.color);
            clone.lineWidth.CopyValuesFrom(this.lineWidth);
            return clone;
        }

        public override bool Equals(object other)
        {
            if (other is not ShowLineModel otherShowLineModel)
            {
                return false;
            }
            return Equals(this.enabled, otherShowLineModel.enabled) && Equals(
                this.lineWidth,
                otherShowLineModel.lineWidth) && Equals(this.color, otherShowLineModel.color);
        }
    }
#pragma warning restore CS0659

    [Serializable]
    public class ShowLineModelRuntimeParameter : DynamicTrackingParameter<ShowLineModel>,
        ISerializationCallbackReceiver
    {
        internal const string nativeName = "showLineModel";

        [SerializeField]
        public UnityEvent<bool> onBoolValueChanged = new UnityEvent<bool>();
        private bool previousBoolValue;
        private bool firstStateChange = true;

        public ShowLineModelRuntimeParameter()
            : base(new ShowLineModel())
        {
            this.onValueChanged.AddListener(HandleStateChange);
        }

        public async Task<WorkerCommands.CommandWarnings> SetValueAsync(
            bool drawLineModels,
            IParameterHandler parameterHandler)
        {
            var enabledState = this.value.enabled.GetSharedValueForAllStates();
            if (enabledState.HasValue && Equals(enabledState.Value, drawLineModels))
            {
                return WorkerCommands.NoWarnings();
            }

            this.value.enabled = new LineModelParameter<bool>(drawLineModels);
            SetUseValueFromUnity(true);
            return await UpdateInBackendAsync(parameterHandler);
        }

        public async Task<WorkerCommands.CommandWarnings> SetLineModelEnabled(
            LineModelParameter<bool> newEnabledParameter,
            IParameterHandler parameterHandler)
        {
            if (Equals(this.value.enabled, newEnabledParameter))
            {
                return WorkerCommands.NoWarnings();
            }
            this.value.enabled = newEnabledParameter;
            SetUseValueFromUnity(true);
            return await UpdateInBackendAsync(parameterHandler);
        }

        public async Task<WorkerCommands.CommandWarnings> SetLineModelColor(
            LineModelColor newColor,
            IParameterHandler parameterHandler)
        {
            if (Equals(this.value.color, newColor))
            {
                return WorkerCommands.NoWarnings();
            }
            this.value.color = newColor;
            SetUseValueFromUnity(true);
            return await UpdateInBackendAsync(parameterHandler);
        }

        public async Task<WorkerCommands.CommandWarnings> SetLineModelLineWidth(
            LineModelParameter<int> newLineWidthParameter,
            IParameterHandler parameterHandler)
        {
            if (Equals(this.value.lineWidth, newLineWidthParameter))
            {
                return WorkerCommands.NoWarnings();
            }
            this.value.lineWidth = newLineWidthParameter;
            SetUseValueFromUnity(true);
            return await UpdateInBackendAsync(parameterHandler);
        }

        protected override ShowLineModel GetIndependentValueCopy()
        {
            return this.value.Clone();
        }

        protected override string GetValueString()
        {
            return this.value.ToJsonString();
        }

        public override string GetDescriptiveName()
        {
            return "Show Linemodel";
        }

        public override string GetNativeName()
        {
            return ShowLineModelRuntimeParameter.nativeName;
        }

        public override string GetDescription()
        {
            return
                "This tells VisionLib whether to draw the current line model into the camera image stream.";
        }

        public override ShowLineModel GetDefaultValue()
        {
            return new ShowLineModel();
        }

        private void HandleStateChange(ShowLineModel newShowLineModel)
        {
            var newBoolValue = newShowLineModel.enabled.GetSharedValueForAllStates()
                .GetValueOrDefault(true);
            if (!this.firstStateChange && this.previousBoolValue == newBoolValue)
            {
                return;
            }
            this.previousBoolValue = newBoolValue;
            EmitOnBoolValueChanged(newBoolValue);
            this.firstStateChange = false;
        }

        private void EmitOnBoolValueChanged(bool boolValue)
        {
            this.onBoolValueChanged?.Invoke(boolValue);
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            this.firstStateChange = true;
            this.onValueChanged.AddListener(HandleStateChange);
        }
    }
}
