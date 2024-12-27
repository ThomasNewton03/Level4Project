using System;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///     Interface for listeners to <see cref="GameObjectPoseInteraction"/> events
    ///     <see cref="GameObjectPoseInteraction.interactionStarted"/> and
    ///     <see cref="GameObjectPoseInteraction.interactionEndeded"/>.
    /// </summary>
    /// <remarks>
    ///     Subscribe a <see cref="IObjectPoseInteractionEventListener"/> via
    ///     <see cref="GameObjectPoseInteraction.RegisterListener"/>.
    ///     Unsubscribe via <see cref="GameObjectPoseInteraction.DeregisterListener"/>.
    /// </remarks>>
    /// @ingroup Core
    /// \deprecated Use a IObjectPoseInteractionProvider instead and directly register to the corresponding events
    [Obsolete("Use a IObjectPoseInteractionProvider instead and directly register to the corresponding events")]
    public interface IObjectPoseInteractionEventListener
    {
        void OnInteractionStarted();
        void OnTransformChanged();
        void OnInteractionEnded();
    }
}
