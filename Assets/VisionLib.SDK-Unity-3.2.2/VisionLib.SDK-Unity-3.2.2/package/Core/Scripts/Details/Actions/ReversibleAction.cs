#if UNITY_EDITOR
using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Performs an Action and records it in the scene's undo history.
    /// </summary>
    public class ReversibleAction : ISetupIssueSolution
    {
        private readonly string message;
        private readonly Object changedObject;
        private readonly Action reversibleAction;

        /// <summary>
        /// Creates a new <see cref="ReversibleAction"/>.
        /// </summary>
        /// <param name="action">Action that modifies the 'changedObject'</param>
        /// <param name="changedObject">Reference to the object to modify.</param>
        /// <param name="message">The title of the action.</param>
        public ReversibleAction(Action action, Object changedObject, string message)
        {
            this.reversibleAction = action;
            this.changedObject = changedObject;
            this.message = message;
        }

        public void Invoke()
        {
            Undo.RecordObject(this.changedObject, this.message);
            this.reversibleAction?.Invoke();
            if (this.changedObject)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(this.changedObject);
            }
        }

        public string GetMessage()
        {
            return this.message;
        }
    }
}
#endif
