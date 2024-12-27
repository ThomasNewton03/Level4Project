#if UNITY_EDITOR
using System;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// <see cref="ISetupIssueSolution"/>, which cannot be undone easily, because it changes assets outside the scene.
    /// </summary>
    public class AssetRelatedAction : ISetupIssueSolution
    {
        private readonly Action assetAction;
        private readonly string message;

        /// <summary>
        /// Creates a new <see cref="AssetRelatedAction"/>.
        /// </summary>
        /// <param name="action">Action that modifies an asset to solve a <see cref="SetupIssue"/>.</param>
        /// <param name="message">Short explanation what the fix for the <see cref="SetupIssue"/> does.</param>
        public AssetRelatedAction(Action action, string message)
        {
            this.assetAction = action;
            this.message = message;
        }

        public void Invoke()
        {
            this.assetAction?.Invoke();
        }

        public string GetMessage()
        {
            return this.message;
        }
    }
}

#endif
