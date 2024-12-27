using System;
using System.Linq;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public enum TriState
    {
        False,
        True,
        Mixed
    }

    public static class TriStateHelper
    {
        public static TriState IsTrackingObject(
            GameObject gameObject,
            bool includeInactiveChildren = false)
        {
            var trackingObject = gameObject.GetComponent<TrackingObject>();
            return trackingObject
                ? TriState.True
                : HasChildWithTrackingObject(gameObject.transform, includeInactiveChildren);
        }

        private static TriState HasChildWithTrackingObject(
            Transform root,
            bool includeInactiveChildren = false)
        {
            var meshFilter = root.GetComponentsInChildren<MeshFilter>(includeInactiveChildren);
            if (meshFilter.Length == 0)
            {
                return TriState.False;
            }

            var meshFilterWithTrackingObject =
                meshFilter.Count(transform => transform.GetComponent<TrackingObject>());
            var meshFilterWithoutTrackingObject = meshFilter.Length - meshFilterWithTrackingObject;

            return GetTriState(meshFilterWithTrackingObject, meshFilterWithoutTrackingObject);
        }

        public static TriState IsTrackingObjectEnabled(
            GameObject gameObject,
            bool includeInactiveChildren = false)
        {
            var trackingObject = gameObject.GetComponent<TrackingObject>();
            if (trackingObject && trackingObject.enabled)
            {
                return TriState.True;
            }
            
            return GetTriStateIncludingChildren<TrackingObject>(
                gameObject.transform,
                trackingObjectInstance => trackingObjectInstance.enabled,
                includeInactiveChildren);
        }

        public static TriState IsMeshRendererEnabled(
            GameObject gameObject,
            bool includeInactiveChildren = false)
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer && meshRenderer.enabled)
            {
                return TriState.True;
            }
            
            return GetTriStateIncludingChildren<MeshRenderer>(
                gameObject.transform,
                meshRendererInstance => meshRendererInstance.enabled,
                includeInactiveChildren);
        }

        public static TriState IsOccluder(
            GameObject gameObject,
            bool includeInactiveChildren = false)
        {
            var trackingObject = gameObject.GetComponent<TrackingObject>();
            if (trackingObject && trackingObject.occluder)
            {
                return TriState.True;
            }
            
            return GetTriStateIncludingChildren<TrackingObject>(
                gameObject.transform,
                trackingObjectInstance => trackingObjectInstance.occluder,
                includeInactiveChildren);
        }

        private static TriState GetTriStateIncludingChildren<ComponentType>(
            Transform root,
            Func<ComponentType, bool> predicate,
            bool includeInactiveChildren = false)
        {
            var components = root.GetComponentsInChildren<ComponentType>(includeInactiveChildren);
            if (components.Length == 0)
            {
                return TriState.False;
            }

            var numInstancesTotal = components.Length;
            var numInstancesThatFulfillPredicate = components.Count(predicate);

            return GetTriState(
                numInstancesThatFulfillPredicate,
                numInstancesTotal - numInstancesThatFulfillPredicate);
        }

        private static TriState GetTriState(int trueCount, int falseCount)
        {
            if (trueCount > 0)
            {
                return falseCount == 0 ? TriState.True : TriState.Mixed;
            }
            return TriState.False;
        }
    }
}
