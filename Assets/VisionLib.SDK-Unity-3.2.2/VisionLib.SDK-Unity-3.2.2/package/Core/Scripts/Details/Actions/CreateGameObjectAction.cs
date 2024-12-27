#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Creates a new GameObject to solve a problem in the scene.
    /// </summary>
    public class CreateGameObjectSolution : ISetupIssueSolution
    {
        private readonly string message;
        private readonly string gameObjectName;
        private readonly Type[] components;

        /// <summary>
        /// Creates a new <see cref="CreateGameObjectSolution"/>.
        /// </summary>
        /// <param name="message">The title of the action.</param>
        /// <param name="gameObjectName">Name of the new GameObject</param>
        /// <param name="components">Components of the newly created GameObject</param>
        public CreateGameObjectSolution(string message, string gameObjectName, params Type[] components)
        {
            this.components = components;
            this.gameObjectName = gameObjectName;
            this.message = message;
        }
        
        public void Invoke()
        {
            var newGameObject = new GameObject(gameObjectName, this.components);
            Undo.RegisterCreatedObjectUndo(newGameObject, this.message);
        }

        public string GetMessage()
        {
            return this.message;
        }
    }
}
#endif