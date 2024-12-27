#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Utility functions for working with GameObjects and Components.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Destroys the given object either immediately with the option to undo (Editor) or via
        /// garbage collection (Application)
        /// </summary>
        public static void Destroy(this Object var)
        {
            if (var == null)
            {
                return;
            }
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(var);
#else
            Object.Destroy(var);
#endif
        }

        /// <summary>
        /// Destroys all instances of the component on the specified GameObject
        /// </summary>
        public static void DestroyComponent<T>(this GameObject gameObject) where T : Component
        {
            var components = gameObject.GetComponents<T>();
            if (components == null || components.Length == 0)
            {
                return;
            }
            foreach (var component in components)
            {
                component.Destroy();
            }
        }

        /// <summary>
        /// Returns the component of the given type if the GameObject has one attached, adds and
        /// returns it otherwise.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            return gameObject.AddComponentUndoable<T>();
        }

        /// <summary>
        /// Adds the component to the GameObject.
        /// If in editor, this will also register an Undo for adding this component.
        /// </summary>
        public static T AddComponentUndoable<T>(this GameObject gameObject) where T : Component
        {
#if UNITY_EDITOR
            return Undo.AddComponent<T>(gameObject);
#else
            return gameObject.AddComponent<T>();
#endif
        }

        /// <summary>
        /// Extract only the objects that exist. This takes unity object peculiarities into account. 
        /// </summary>
        public static IEnumerable<TObject> WhereAlive<TObject>(
            this IEnumerable<TObject> set) where TObject : UnityEngine.Object
        {
            return set.Where(obj => obj);
        }
    }
}
