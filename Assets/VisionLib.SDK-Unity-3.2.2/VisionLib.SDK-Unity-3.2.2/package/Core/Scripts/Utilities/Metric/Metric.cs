using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Visometry.VisionLib.SDK.Core
{
    public static class Metric
    {
        public enum Unit
        {
            [InspectorName("km")]
            km, // metric kilometer
            [InspectorName("m")]
            m, // metric meter
            [InspectorName("dm")]
            dm, // metric decimeter
            [InspectorName("cm")]
            cm, // metric centimeter
            [InspectorName("mm")]
            mm, // metric millimeter
            [InspectorName("inch")]
            inch, // imperial inch
            [InspectorName("ft")]
            ft, // imperial foot
            [InspectorName("yd")]
            yd, // imperial yard
            [InspectorName("ch")]
            ch, // imperial chain
            [InspectorName("fur")]
            fur, // imperial furlong
            [InspectorName("ml")]
            ml // imperial mile
        };

        public static float GetScale(Unit unit)
        {
            return unit switch
            {
                Unit.km => 1000.0f,
                Unit.m => 1.0f,
                Unit.dm => 0.1f,
                Unit.cm => 0.01f,
                Unit.mm => 0.001f,
                Unit.inch => 0.0254f,
                Unit.ft => 0.3048f,
                Unit.yd => 0.9144f,
                Unit.ch => 20.1168f,
                Unit.fur => 201.168f,
                Unit.ml => 1609.34f,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        public static Unit? Parse(string metricString)
        {
            try
            {
                return Enum.GetValues(typeof(Unit)).Cast<Unit>()
                    .First(parameterType => parameterType.ToString() == metricString);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static float ScaleFactor(Unit from, Unit to)
        {
            return GetScale(to) / GetScale(from);
        }

        public static string GetDimensionString(this Bounds bounds, Unit unit)
        {
            var dimensionEnding = unit.ToString();
            var scale = GetScale(unit);

            var dimensions = bounds.size / scale;
            return
                $"{dimensions.x:0.###} {dimensionEnding} x {dimensions.y:0.###} {dimensionEnding} x {dimensions.z:0.###} {dimensionEnding}";
        }

        internal static IEnumerable<Unit> GetPlausibleMetrics(
            Vector3 modelDimension,
            Unit currentMetric,
            float minSize,
            float maxSize)
        {
            var minDimension = Math.Min(
                modelDimension.x,
                Math.Min(modelDimension.y, modelDimension.z));
            var maxDimension = Math.Max(
                modelDimension.x,
                Math.Max(modelDimension.y, modelDimension.z));

            return from Unit unit in Enum.GetValues(typeof(Unit))
                let scaleFactor = ScaleFactor(currentMetric, unit)
                where minDimension * scaleFactor > minSize && maxDimension * scaleFactor < maxSize
                select unit;
        }
    }
}
