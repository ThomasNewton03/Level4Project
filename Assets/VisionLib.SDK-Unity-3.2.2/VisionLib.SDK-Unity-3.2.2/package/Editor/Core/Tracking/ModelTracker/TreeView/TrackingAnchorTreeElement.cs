using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details.TrackingAnchorTreeView
{
    /// <summary>
    /// Data class to hold all data for a single row element in the tree.
    /// </summary>
    [Serializable]
    public class TrackingAnchorTreeElement
    {
        [SerializeField]
        public int elementID;
        [SerializeField]
        public string elementName;
        [NonSerialized]
        public TrackingAnchorTreeElement elementParent;
        [NonSerialized]
        public List<TrackingAnchorTreeElement> elementChildren;

        public TriState isTrackingObject;
        public TriState isTrackingObjectEnabled;
        public TriState isOccluder;
        public TriState isMeshRendererEnabled;

        private TrackingAnchorTreeElement(
            GameObject gameObject,
            TrackingAnchorTreeElement parent)
        {
            this.elementName = gameObject.name;
            var trackingObject = gameObject.GetComponent<TrackingObject>();
            if (trackingObject != null)
            {
                var dimensionString = trackingObject.GetModelDimensionsString();
                this.elementName += string.IsNullOrEmpty(dimensionString)
                    ? ""
                    : $" ({trackingObject.GetModelDimensionsString()})";
            }

            this.elementID = gameObject.GetInstanceID();
            this.elementParent = parent;
            this.elementChildren = new List<TrackingAnchorTreeElement>();
            this.isOccluder = TriStateHelper.IsOccluder(gameObject);
            this.isTrackingObject = TriStateHelper.IsTrackingObject(gameObject);
            this.isTrackingObjectEnabled = TriStateHelper.IsTrackingObjectEnabled(gameObject);
            this.isMeshRendererEnabled = TriStateHelper.IsMeshRendererEnabled(gameObject);
        }

        private TrackingAnchorTreeElement(
            string name,
            int id,
            TrackingAnchorTreeElement parent)
        {
            this.elementName = name;
            this.elementID = id;
            this.elementParent = parent;
            this.elementChildren = new List<TrackingAnchorTreeElement>();
            this.isOccluder = TriState.False;
            this.isTrackingObject = TriState.False;
            this.isTrackingObjectEnabled = TriState.False;
            this.isMeshRendererEnabled = TriState.False;
        }

        public bool HasChildren
        {
            get
            {
                return this.elementChildren != null && this.elementChildren.Count > 0;
            }
        }

        public TrackingAnchorTreeElement Find(int id)
        {
            return this.elementID == id
                ? this
                : this.elementChildren.Select(child => child.Find(id))
                    .FirstOrDefault(foundElement => foundElement != null);
        }

        public static TrackingAnchorTreeElement GenerateTree(Transform treeRootTransform)
        {
            var root = new TrackingAnchorTreeElement("Root", 0, null);
            root.elementChildren.Add(CreateChildRecursive(treeRootTransform.gameObject, root));
            return root;
        }

        private static TrackingAnchorTreeElement CreateChildRecursive(
            GameObject gameObject,
            TrackingAnchorTreeElement parent)
        {
            var newElement = new TrackingAnchorTreeElement(gameObject, parent);
            foreach (Transform childTransform in gameObject.transform)
            {
                newElement.elementChildren.Add(
                    CreateChildRecursive(childTransform.gameObject, newElement));
            }
            return newElement;
        }
    }
}
