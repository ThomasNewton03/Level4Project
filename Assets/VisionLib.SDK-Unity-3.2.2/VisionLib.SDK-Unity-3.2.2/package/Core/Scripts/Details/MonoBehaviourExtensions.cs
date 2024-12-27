using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class MonoBehaviourExtensions
    {
        public class MonoBehaviourRemovedDuringTaskException : Exception
        {
            public MonoBehaviourRemovedDuringTaskException()
                : base($"Target MonoBehaviour was removed during task execution.") {}
        }

        public class MonoBehaviourDisabledDuringTaskException : Exception
        {
            public readonly MonoBehaviour monoBehaviour;

            public MonoBehaviourDisabledDuringTaskException(MonoBehaviour behaviour)
                : base($"Target MonoBehaviour was disabled during task execution.")
            {
                this.monoBehaviour = behaviour;
            }
        }

        /// <summary>
        /// Throws an exception if the behaviour was removed or disabled during task execution.
        /// Use this to cancel the execution of subsequent code that depends on the behaviour being
        /// both present and active. If you do not need the behaviour to be active, use
        /// <see cref="ThrowIfNotAlive"/>.
        /// </summary>
        /// <param name="behaviour"></param>
        /// <exception cref="MonoBehaviourRemovedDuringTaskException">If the MonoBehaviour no longer exists</exception>
        /// <exception cref="MonoBehaviourDisabledDuringTaskException">If the MonoBehaviour is disabled</exception>
        public static void ThrowIfNotAliveAndEnabled(this MonoBehaviour behaviour)
        {
            ThrowIfNotAlive(behaviour);
            if (!behaviour.enabled)
            {
                throw new MonoBehaviourDisabledDuringTaskException(behaviour);
            }
        }

        /// <summary>
        /// Returns false if the behaviour was removed or disabled during task execution.
        /// Use this to guard subsequent code that depends on the behaviour being both present and
        /// active.
        /// </summary>
        /// \deprecated StillAliveAndEnabled is obsolete. Use ThrowIfNotAliveAndEnabled instead. The
        ///             thrown exception will automatically be catched by
        ///             TrackingManager.CatchCommandErrors.
        [Obsolete(
            "StillAliveAndEnabled is obsolete. Use ThrowIfNotAliveAndEnabled instead. The thrown exception will automatically be catched by TrackingManager.CatchCommandErrors.")]
        public static bool StillAliveAndEnabled(this MonoBehaviour behaviour)
        {
            return behaviour && behaviour.enabled;
        }

        /// <summary>
        /// Throws an exception if the behaviour was removed during task execution.
        /// Use this to cancel the execution of subsequent code that depends on the behaviour.
        /// </summary>
        /// <param name="behaviour"></param>
        /// <exception cref="MonoBehaviourRemovedDuringTaskException">If the MonoBehaviour no longer exists</exception>
        public static void ThrowIfNotAlive(this MonoBehaviour behaviour)
        {
            if (!behaviour)
            {
                throw new MonoBehaviourRemovedDuringTaskException();
            }
        }

        /// <summary>
        /// Returns false if the behaviour was removed during task execution.
        /// Use this to guard subsequent code that depends on the behaviour.
        /// </summary>
        /// \deprecated StillAlive is obsolete. Use ThrowIfNotAlive instead. The thrown exception
        ///             will automatically be catched by TrackingManager.CatchCommandErrors.
        [Obsolete(
            "StillAlive is obsolete. Use ThrowIfNotAlive instead. The thrown exception will automatically be catched by TrackingManager.CatchCommandErrors.")]
        public static bool StillAlive(this MonoBehaviour behaviour)
        {
            return (bool) behaviour;
        }
    }
}
