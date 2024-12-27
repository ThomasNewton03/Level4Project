#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Performs an Action and records it in the scene's undo history.
    /// </summary>
    public class DestroyComponentAction : ISetupIssueSolution
    {
        private readonly string message;
        private readonly Queue<Component> componentsToDestroy;

        /// <summary>
        /// Creates a new <see cref="DestroyComponentAction"/>.
        /// </summary>
        /// <param name="component">Reference to the component that should be destroyed.</param>
        public DestroyComponentAction(Component component)
        {
            this.componentsToDestroy = new Queue<Component>();
            this.componentsToDestroy.Enqueue(component);
            this.message = "Destroy " + component.GetType().Name + ".";
        }

        /// <summary>
        /// Creates a new <see cref="DestroyComponentAction"/>.
        /// </summary>
        /// <param name="components">Reference to a collection of components to destroy.</param>
        /// <param name="humanReadableCollectionName"> Returned by GetMessage as: "Destroy $humanReadableCollectionName.". </param>
        public DestroyComponentAction(
            IEnumerable<Component> components,
            string humanReadableCollectionName)
        {
            this.componentsToDestroy = new Queue<Component>(components);
            this.message = "Destroy " + humanReadableCollectionName + ".";
        }

        public void Invoke()
        {
            while (this.componentsToDestroy.Count != 0)
            {
                Undo.DestroyObjectImmediate(this.componentsToDestroy.Dequeue());
            }
        }

        public string GetMessage()
        {
            return this.message;
        }
    }
}
#endif
