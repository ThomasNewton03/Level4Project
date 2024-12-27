using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class AssetHelper
    {
#if UNITY_EDITOR
        public static void SetIsReadableOnTextureAsset(
            string assetPath,
            bool isReadable,
            GameObject gameObject = null)
        {
            try
            {
                var importerForAsset = GetImporter<TextureImporter>(assetPath);
                importerForAsset.isReadable = isReadable;
                AssetDatabase.ImportAsset(assetPath);
            }
            catch (ArgumentException)
            {
                LogHelper.LogWarning(
                    $"Could not find an texture asset at '{assetPath}'. Read/Write cannot be set to {isReadable} automatically.",
                    gameObject);
            }
        }

        public static void SetIsReadableOnModelAsset(
            string assetPath,
            bool isReadable,
            GameObject gameObject = null)
        {
            try
            {
                var importerForAsset = GetImporter<ModelImporter>(assetPath);
                importerForAsset.isReadable = isReadable;
                AssetDatabase.ImportAsset(assetPath);
            }
            catch (ArgumentException)
            {
                LogHelper.LogWarning(
                    $"Could not find an model asset at '{assetPath}'. Read/Write cannot be set to {isReadable} automatically.",
                    gameObject);
            }
        }
        
        public static void SetUsageOfFileScaleOnModelAsset(
            string assetPath,
            bool useFileScale,
            GameObject gameObject = null)
        {
            try
            {
                var importerForAsset = GetImporter<ModelImporter>(assetPath);
                importerForAsset.useFileScale = useFileScale;
                AssetDatabase.ImportAsset(assetPath);
            }
            catch (ArgumentException)
            {
                LogHelper.LogWarning(
                    $"Could not find an model asset at '{assetPath}'. Usage of file scale cannot be set to {useFileScale} automatically.",
                    gameObject);
            }
        }

        internal static T GetImporter<T>(string assetPath) where T : AssetImporter
        {
            if (AssetImporter.GetAtPath(assetPath) is not T importerForAsset)
            {
                throw new ArgumentException(
                    $"Could not find an asset importer of type {typeof(T).Name} for the given path {assetPath}.");
            }
            return importerForAsset;
        }
#endif
    }
}
