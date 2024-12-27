using UnityEngine;
using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This class contains shared properties and functionality of Advanced and Simple WorkSpaces.
    ///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
    /// </summary>
    /// @ingroup WorkSpace
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "work_space_helper.html")]
    public static class WorkSpaceHelper
    {
        private static WorkSpaceGeometry CreateGeometryObject(string newName, GameObject parent)
        {
            var gameObject = new GameObject(newName);
#if UNITY_EDITOR
            GameObjectUtility.SetParentAndAlign(gameObject, parent);
#else
            gameObject.transform.SetParent(parent.transform, false);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
#endif
            return gameObject.AddComponentUndoable<WorkSpaceGeometry>();
        }

        public static WorkSpaceGeometry CreateSphereGeometryObject(
            string name,
            GameObject parent,
            float sphereRadius)
        {
            var sphereGeometry = CreateGeometryObject(name, parent);
            sphereGeometry.sphere.radius = sphereRadius;
            return sphereGeometry;
        }

        public static WorkSpaceGeometry CreatePointGeometryObject(string name, GameObject parent)
        {
            var pointGeometry = CreateGeometryObject(name, parent);
            pointGeometry.shape = WorkSpaceGeometry.Shape.Point;
            return pointGeometry;
        }
    }
}
