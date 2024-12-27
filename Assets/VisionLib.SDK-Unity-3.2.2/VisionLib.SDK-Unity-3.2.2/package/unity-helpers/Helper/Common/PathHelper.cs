using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Visometry.Helpers
{
    /// <summary>
    /// VisionLib functions for working with Paths and URIs.
    /// </summary>
    public static class PathHelper
    {
        public const string streamingAssetsScheme = "streaming-assets-dir:";
        private const string localStorageDirScheme = "local-storage-dir:";
        private const string projectDirScheme = "project-dir:";
        private const string captureDirScheme = "capture-dir:";

        public static string CombinePaths(string path1, string path2)
        {
            if (path2.Length == 0)
            {
                return path1;
            }

            if (path1.Length == 0 || IsAbsolutePath(path2))
            {
                return path2;
            }

            var directorySeparators = new char[] {'/', '\\'};
            return path1.TrimEnd(directorySeparators) + Path.AltDirectorySeparatorChar +
                   path2.TrimStart(directorySeparators);
        }

        public static string CombinePaths(string path1, string path2, string path3)
        {
            return CombinePaths(path1, CombinePaths(path2, path3));
        }

        public static string CombinePaths(IEnumerable<string> segments)
        {
            return segments.Aggregate("", CombinePaths);
        }

        public static bool IsAbsolutePath(string path)
        {
            return ContainsScheme(path) || IsAbsoluteWindowsPath(path) || IsAbsoluteUnixPath(path);
        }

        private static bool IsAbsoluteWindowsPath(string path)
        {
            return path.Length >= 2 && path[1] == ':' && Char.IsLetter(path[0]);
        }

        private static bool IsAbsoluteUnixPath(string path)
        {
            return path.Length >= 1 && (path[0] == '/' || path[0] == '~');
        }

        public static bool ContainsScheme(string uri)
        {
            return uri.StartsWith(PathHelper.streamingAssetsScheme) ||
                   uri.StartsWith(PathHelper.localStorageDirScheme) ||
                   uri.StartsWith(PathHelper.projectDirScheme) ||
                   uri.StartsWith(PathHelper.captureDirScheme) || uri.StartsWith("data:") ||
                   uri.StartsWith("http://") || uri.StartsWith("https://") ||
                   uri.StartsWith("file://");
        }

        public static string RemoveStartingSlashes(string path)
        {
            return path.TrimStart('/');
        }

        public static string UnifySlashes(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string GetStreamingAssetsPath()
        {
            return UnifySlashes(Application.streamingAssetsPath);
        }

        public static string SubstituteStreamingAssetsPathWithSchema(string path)
        {
            return UnifySlashes(path).Replace(
                GetStreamingAssetsPath(),
                PathHelper.streamingAssetsScheme);
        }

        /// <summary>
        /// Replaces the project-dir schema in the URI with an different path
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="projectDirPath"></param>
        /// <returns></returns>
        public static string SubstituteProjectDirPath(string uri, string projectDirPath)
        {
            var projectDirPathWithTailingSlash = UnifySlashes(projectDirPath);
            if (!projectDirPathWithTailingSlash.EndsWith("/"))
            {
                projectDirPathWithTailingSlash += "/";
            }

            // A uri with project-dir schema might either be of the form `project-dir:/...` or
            // `project-dir:...`. In both cases we have to maintain a `/` between the paths but do
            // not want to insert an additional `/` 
            return UnifySlashes(uri)
                .Replace(PathHelper.projectDirScheme + "/", projectDirPathWithTailingSlash).Replace(
                    PathHelper.projectDirScheme,
                    projectDirPathWithTailingSlash);
        }

        /// <summary>        
        ///  Checks whether a string is an absolute URI. This means
        ///  that the URI has a scheme, an authority (if applicable) and a path.
        /// </summary>
        public static bool IsAbsoluteURI(string potentialURI)
        {
            return Uri.TryCreate(potentialURI, UriKind.Absolute, out _);
        }
    }
}
