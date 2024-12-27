using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Component wrapper around several BaseGeometry classes.
    ///
    ///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
    /// </summary>
    /// @ingroup WorkSpace
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "work_space_geometry.html")]
    [AddComponentMenu("VisionLib/Core/AutoInit/WorkSpace Geometry")]
    public class WorkSpaceGeometry : MonoBehaviour
    {
        /// <summary>
        /// Defines the type of a WorkSpaceGeometry. The corresponding parameters will be used
        /// and set in the corresponding BaseGeometry.
        /// </summary>
        [System.Serializable]
        public enum Shape
        {
            Sphere,
            Plane,
            Line,
            Point
        }

        [HideInInspector]
        public Shape shape = Shape.Sphere;

        [HideInInspector]
        public AdvancedSphereGeometry sphere = new AdvancedSphereGeometry();

        [HideInInspector]
        [SerializeField]
        private PlaneGeometry plane = new PlaneGeometry();

        [HideInInspector]
        [SerializeField]
        private LineGeometry line = new LineGeometry();

        [HideInInspector]
        [SerializeField]
        private PointGeometry point = new PointGeometry();

        public BaseGeometry GetGeometry()
        {
            switch (this.shape)
            {
                case Shape.Sphere:
                    return this.sphere;
                case Shape.Plane:
                    return this.plane;
                case Shape.Line:
                    return this.line;
                case Shape.Point:
                    return this.point;
                default:
                    return null;
            }
        }
    }
}
