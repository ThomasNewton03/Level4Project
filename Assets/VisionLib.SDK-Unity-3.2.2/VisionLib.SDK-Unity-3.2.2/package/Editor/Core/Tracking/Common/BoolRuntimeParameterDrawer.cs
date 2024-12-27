using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    [CustomPropertyDrawer(typeof(BoolRuntimeParameter), true)]
    public class BoolRuntimeParameterDrawer : DynamicTrackingParameterDrawer<bool>
    {
        protected override void DrawParameterSetterField(
            Rect contentRect,
            DynamicTrackingParameter<bool> parameter,
            SerializedProperty valueProperty)
        {
            if (ToggleSwitch.DrawToggleSwitch(
                    contentRect,
                    valueProperty.boolValue,
                    parameter.GetDescription()))
            {
                valueProperty.boolValue = !valueProperty.boolValue;
            }
        }
    }
}
