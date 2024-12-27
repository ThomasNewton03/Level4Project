using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Interface for Interaction Provider
    /// </summary>
    /// @ingroup Core
    public interface IObjectPoseInteractionProvider
    {
        /// <summary>
        ///  Delegate for events giving no feedback
        /// </summary>
        public delegate void VoidDelegate();

        public event VoidDelegate InteractionStarted;
        public event VoidDelegate TargetTransformChanged;
        public event VoidDelegate InteractionEnded;
        
        /// <summary>
        /// Setter for the interaction camera
        /// All interaction should take place relative to this camera.
        /// </summary>
        /// <param name="interactionCamera"></param>
        public void SetInteractionCamera(Camera interactionCamera);

        /// <summary>
        /// Getter for the interaction camera
        /// All interaction should take place relative to this camera.
        /// </summary>
        /// <returns></returns>
        public Camera GetInteractionCamera();

        /// <summary>
        /// Function for enabling and disabling the interaction
        /// </summary>
        /// <param name="newEnabledState"></param>
        public void SetEnabled(bool newEnabledState);
    }
}
