using System;
using System.IO;
using UnityEngine;

namespace Visometry.Helpers
{
    /// <summary>
    /// Editor-Only VisionLib functions for working with Paths and URIs.
    /// </summary>
    public static class EditorPathHelper
    {
#if UNITY_EDITOR
        /// <summary>
        /// Gets a parent Directory of the "Assets" folder.
        /// Example usage VL Unity SDK: Pass "visionlib.sdk.unity" if you cloning the repo used
        /// the default folder name.
        /// This function will not work as expected if you attempt to call it in the deployed player,
        /// since Application.dataPath has a different meaning in this context.
        /// </summary>
        public static string GetPartialProjectPath(string baseFolderName)
        {
            var assetsPath = Application.dataPath;
            var positionOfBaseFolder = assetsPath.IndexOf(baseFolderName, StringComparison.OrdinalIgnoreCase);
            if (positionOfBaseFolder == -1)
            {
                throw new Exception($"{assetsPath} does not contain {baseFolderName}");
            }
            var repoPath = assetsPath.Substring(0, positionOfBaseFolder + baseFolderName.Length);
            if (Directory.Exists(PathHelper.CombinePaths(repoPath, ".git")))
            {
                return repoPath;
            }
            throw new Exception("No repository found at expected location: \"" + repoPath + "\"");
        }
#endif
    }
}
