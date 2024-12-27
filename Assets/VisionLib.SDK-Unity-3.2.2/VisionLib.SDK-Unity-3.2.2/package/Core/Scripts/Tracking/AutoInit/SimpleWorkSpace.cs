using UnityEngine;
using Visometry.Helpers;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This class contains shared properties and functionality of Advanced and Simple WorkSpaces.
    ///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
    /// </summary>
    /// @ingroup WorkSpace
    [AddComponentMenu("VisionLib/Core/AutoInit/Simple WorkSpace")]
    [HelpURL(DocumentationLink.simpleWorkSpace)]
    public class SimpleWorkSpace : WorkSpace
    {
        [SerializeField]
        private SimpleSphereGeometry sourceGeometry = new SimpleSphereGeometry();

        public override BaseGeometry GetSourceGeometry()
        {
            return this.sourceGeometry;
        }

        protected override API.WorkSpace.Geometry GetDestinationGeometryDefinition()
        {
            var localBounds = BoundsUtilities.GetMeshBoundsInParentCoordinates(
                this.destinationObject,
                GetRootTransform(),
                true);
            return new API.WorkSpace.BoundingBox(localBounds);
        }

        protected override API.WorkSpace.Geometry GetSourceGeometryDefinition()
        {
            return GetSourceGeometry()?.CreateGeometry(null);
        }

        public override API.WorkSpace.Definition GetWorkSpaceDefinition(bool useCameraRotation)
        {
            return base.GetWorkSpaceDefinitionFromType(
                API.WorkSpace.Definition.Type.Simple,
                useCameraRotation);
        }

        public override Vector3 GetCenter()
        {
            return WorkSpace.GetCenter(this.destinationObject);
        }

        public override float GetSize()
        {
            // radius of simple sphere geometry
            return GetSimpleSphereRadius() * 1.2f;
        }

        public float GetSimpleSphereRadius()
        {
            return base.GetOptimalCameraDistance(this.destinationObject);
        }

        public override int GetVerticesCount()
        {
            return this.sourceGeometry.currentMesh.Length * GetDestinationVertices().Length;
        }

#if UNITY_EDITOR
        protected override void CreateSourceObject()
        {
            this.sourceGeometry = new SimpleSphereGeometry();
        }
#endif
    }
}
