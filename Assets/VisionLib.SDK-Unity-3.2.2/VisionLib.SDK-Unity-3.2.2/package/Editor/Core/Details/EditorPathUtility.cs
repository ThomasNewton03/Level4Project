using Visometry.Helpers;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// VisionLib functions for working with Paths and URIs in the Editor.
    /// </summary>
    public class EditorPathUtility
    {
        private const string streamingAssetsPrefix = "Assets/StreamingAssets/";
        private const string streamingAssetsScheme = "streaming-assets-dir:";

        public static bool StartsWithStreamingAssetsPrefix(string path)
        {
            return path.StartsWith(streamingAssetsPrefix) || path.StartsWith(streamingAssetsScheme);
        }

        public static string ResolveStreamingAssetsScheme(string path)
        {
            string unifiedPath = PathHelper.UnifySlashes(path);
            if (unifiedPath.StartsWith(streamingAssetsScheme))
            {
                string pathWithoutPrefix = unifiedPath.Substring(streamingAssetsScheme.Length);
                pathWithoutPrefix = PathHelper.RemoveStartingSlashes(pathWithoutPrefix);
                return streamingAssetsPrefix + pathWithoutPrefix;
            }
            return unifiedPath;
        }

        public static string ReplaceStreamingAssetsPrefixWithScheme(string path)
        {
            string unifiedPath = PathHelper.UnifySlashes(path);
            if (unifiedPath.StartsWith(streamingAssetsPrefix))
            {
                string pathWithoutPrefix = unifiedPath.Substring(streamingAssetsPrefix.Length);
                pathWithoutPrefix = PathHelper.RemoveStartingSlashes(pathWithoutPrefix);
                return streamingAssetsScheme + pathWithoutPrefix;
            }
            return unifiedPath;
        }
    }
}
