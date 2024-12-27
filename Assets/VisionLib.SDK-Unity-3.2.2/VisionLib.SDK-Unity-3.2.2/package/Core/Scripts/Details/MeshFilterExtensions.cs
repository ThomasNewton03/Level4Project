using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class MeshFilterExtensions
    {
        public class ModelNotFoundException : Exception
        {
            public ModelNotFoundException()
                : base("The GameObject contains no mesh.") {}
        }

        public class SharedMeshNotFoundException : Exception
        {
            public SharedMeshNotFoundException()
                : base("The mesh on the GameObject contains no 'sharedMesh'.") {}
        }

        public class ModelNotReadableException : Exception
        {
            public ModelNotReadableException()
                : base(
                    "The mesh on the GameObject can't be serialized because " +
                    "'Read/Write Enabled' is not activated in the corresponding asset's import " +
                    "settings. Enable this option!") {}
        }

        public static void CheckSerializability(this MeshFilter filter)
        {
            if (!filter)
            {
                throw new ModelNotFoundException();
            }
            var mesh = filter.sharedMesh;
            if (!mesh)
            {
                throw new SharedMeshNotFoundException();
            }
            if (!mesh.isReadable)
            {
                throw new ModelNotReadableException();
            }
        }

#if UNITY_EDITOR
        public static bool DoesUseFileScale(this MeshFilter filter)
        {
            var assetPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
            if (assetPath == "" || assetPath == "Library/unity default resources")
            {
                return true;
            }
            try
            {
                var importerForAsset = AssetHelper.GetImporter<ModelImporter>(assetPath);
                return importerForAsset.useFileScale;
            }
            catch(ArgumentException)
            {
            }
            return true;
        }

        public static void SetUsageOfFileScaleOnSharedMesh(
            this MeshFilter filter,
            bool useFileScale)
        {
            var assetPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
            AssetHelper.SetUsageOfFileScaleOnModelAsset(assetPath, useFileScale, filter.gameObject);
        }

        public static void SetIsReadableOnSharedMesh(this MeshFilter filter, bool isReadable)
        {
            var assetPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
            AssetHelper.SetIsReadableOnModelAsset(assetPath, isReadable, filter.gameObject);
        }
#endif
    }
}
