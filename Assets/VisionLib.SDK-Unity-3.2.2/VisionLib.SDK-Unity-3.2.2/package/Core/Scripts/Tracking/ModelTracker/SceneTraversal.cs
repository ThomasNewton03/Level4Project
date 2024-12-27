using UnityEngine;
using System;

namespace Visometry.VisionLib.SDK.Core
{
    /// @ingroup Core
    public static class SceneTraversal
    {
        /// <summary>
        /// Call action on every transform starting with rootNode, ignoring subtrees where predicate(node) returns false.
        /// </summary>
        /// <param name="rootNode">Root of tree to traverse</param>
        /// <param name="action">Action to call</param>
        /// <param name="predicate">Predicate </param>
        public static void Traverse(
            Transform rootNode,
            Action<Transform> action,
            Func<Transform, bool> predicate)
        {
            Traverse(rootNode, action, predicate, true);
        }

        private static void Traverse(
            Transform node,
            Action<Transform> action,
            Func<Transform, bool> predicate,
            bool isRoot)
        {
            if (!isRoot && !predicate(node))
            {
                return;
            }
            action(node);
            foreach (Transform child in node)
            {
                Traverse(child, action, predicate, false);
            }
        }
    }
}
