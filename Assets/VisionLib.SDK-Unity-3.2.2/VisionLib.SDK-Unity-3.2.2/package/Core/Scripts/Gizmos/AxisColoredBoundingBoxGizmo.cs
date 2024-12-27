#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core;

public static class AxisColoredBoundingBoxGizmo
{
    private class BoundingBoxEdge
    {
        public readonly Vector3 pStart;
        public readonly Vector3 pEnd;
        public readonly AxisOrientation axisOrientation;

        public float Length
        {
            get => Mathf.Abs((this.pEnd - this.pStart).magnitude);
        }

        public Vector3 CenterPoint
        {
            get => this.pStart + (this.pEnd - this.pStart) / 2f;
        }

        public BoundingBoxEdge(Vector3 pStart, Vector3 pEnd, AxisOrientation axisOrientation)
        {
            this.pStart = pStart;
            this.pEnd = pEnd;
            this.axisOrientation = axisOrientation;
        }
    }

    private enum AxisOrientation
    {
        X,
        Y,
        Z
    }

    private static readonly GUIStyle labelToTheRight = new()
    {
        fontSize = 18, normal = {textColor = Color.black, background = Texture2D.grayTexture}
    };

    private static readonly GUIStyle labelToTheLeft =
        new(AxisColoredBoundingBoxGizmo.labelToTheRight) {alignment = TextAnchor.MiddleRight};

    public static void Draw(
        Bounds boundsInLocalSpace,
        Transform localSpace,
        Metric.Unit? metric)
    {
        var originalGizmoColor = Handles.color;
        var boundingBoxEdgesInWorldSpace = GetEdgesInWorldSpace(
            boundsInLocalSpace,
            localSpace.TransformPoint);
        foreach (var edge in boundingBoxEdgesInWorldSpace)
        {
            Handles.color = GetLineColor(edge.axisOrientation);
            Handles.DrawLine(edge.pStart, edge.pEnd);
        }
        Handles.color = originalGizmoColor;

        if (!metric.HasValue)
        {
            return;
        }

        var first = true;
        var scaleFactor = Metric.GetScale(metric.Value);
        foreach (var boundingBoxEdge in GetEdgesToLabel(boundingBoxEdgesInWorldSpace))
        {
            Handles.Label(
                boundingBoxEdge.CenterPoint,
                $"{(boundingBoxEdge.Length / scaleFactor):0.###} {metric.Value}",
                (first
                    ? AxisColoredBoundingBoxGizmo.labelToTheLeft
                    : AxisColoredBoundingBoxGizmo.labelToTheRight));
            first = false;
        }
    }

    private static BoundingBoxEdge[] GetEdgesToLabel(
        IEnumerable<BoundingBoxEdge> boundingBoxEdgesInWorldSpace)
    {
        var view = SceneView.currentDrawingSceneView;
        var camera = view != null ? view.camera : CameraProvider.MainCamera;
        if (camera == null)
        {
            return new BoundingBoxEdge[]{};
        }
        var edgesToLabel = new[]
        {
            boundingBoxEdgesInWorldSpace
                .Where(edge => edge.axisOrientation == AxisOrientation.X)
                .OrderBy(edge => camera.WorldToScreenPoint(edge.CenterPoint).y).First(),
            boundingBoxEdgesInWorldSpace
                .Where(edge => edge.axisOrientation == AxisOrientation.Y).OrderBy(
                    edge => camera.WorldToScreenPoint(edge.CenterPoint).x).Last(),
            boundingBoxEdgesInWorldSpace
                .Where(edge => edge.axisOrientation == AxisOrientation.Z).OrderBy(
                    edge => camera.WorldToScreenPoint(edge.CenterPoint).y).First()
        };
        return edgesToLabel.OrderBy(edge => camera.WorldToScreenPoint(edge.CenterPoint).x)
            .ToArray();
    }

    private static Color GetLineColor(AxisOrientation orientation)
    {
        return orientation switch
        {
            AxisOrientation.X => Handles.xAxisColor,
            AxisOrientation.Y => Handles.yAxisColor,
            AxisOrientation.Z => Handles.zAxisColor,
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }

    private static List<BoundingBoxEdge> GetEdgesInWorldSpace(
        this Bounds bounds,
        Func<Vector3, Vector3> getPointInWorldSpace)
    {
        var edges = new List<BoundingBoxEdge>();

        var min = bounds.min;
        var max = bounds.max;

        var axes = new[] {AxisOrientation.X, AxisOrientation.Y, AxisOrientation.Z};

        foreach (var axis in axes)
        {
            //Add the four bounding box edges that are parallel to this axis
            for (var edgeIndex = 0; edgeIndex < 4; edgeIndex++)
            {
                //The edge index is either 00, 10, 01 or 11. This is used to pick all combinations
                //of min (0) and max (1) values for the coordinates of the two axes other than
                //the main axis.  
                var minOrMaxForOtherAxes = To2BitBinary(edgeIndex);

                Vector3 pStart;
                Vector3 pEnd;
                switch (axis)
                {
                    case AxisOrientation.X:
                    {
                        var y = minOrMaxForOtherAxes[0] ? max.y : min.y;
                        var z = minOrMaxForOtherAxes[1] ? max.z : min.z;
                        pStart = new Vector3(min.x, y, z);
                        pEnd = new Vector3(max.x, y, z);
                        break;
                    }
                    case AxisOrientation.Y:
                    {
                        var x = minOrMaxForOtherAxes[0] ? max.x : min.x;
                        var z = minOrMaxForOtherAxes[1] ? max.z : min.z;
                        pStart = new Vector3(x, min.y, z);
                        pEnd = new Vector3(x, max.y, z);
                        break;
                    }
                    case AxisOrientation.Z:
                    {
                        var x = minOrMaxForOtherAxes[0] ? max.x : min.x;
                        var y = minOrMaxForOtherAxes[1] ? max.y : min.y;
                        pStart = new Vector3(x, y, min.z);
                        pEnd = new Vector3(x, y, max.z);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
                }
                edges.Add(
                    new BoundingBoxEdge(
                        getPointInWorldSpace(pStart),
                        getPointInWorldSpace(pEnd),
                        axis));
            }
        }
        return edges;
    }

    private static bool[] To2BitBinary(int value)
    {
        if (value is > 3 or < 0)
        {
            throw new ArgumentException("Value out of range.");
        }
        return new[] {value > 1, value % 2 == 0};
    }
}
#endif
