#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class TextureExtensions
    {
        internal static bool HasMainTexture(this Material material)
        {
            return material.HasTexture("_MainTex");
        }
        
        internal static bool IsSerializable(this Material material)
        {
            return !material.HasMainTexture() || material.mainTexture == null || material.mainTexture.isReadable;
        }

#if UNITY_EDITOR
        public static void SetIsReadableOnTextureAsset(
            this Material material,
            bool isReadable,
            GameObject gameObject = null)
        {
            var assetPath = AssetDatabase.GetAssetPath(material.mainTexture);
            AssetHelper.SetIsReadableOnTextureAsset(assetPath, isReadable, gameObject);
        }
#endif
    }
}
