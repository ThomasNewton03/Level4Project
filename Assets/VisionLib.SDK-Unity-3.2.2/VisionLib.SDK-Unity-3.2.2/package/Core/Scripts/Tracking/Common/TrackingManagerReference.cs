using UnityEngine;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Base class for MonoBehaviours, which need access to the
    ///  <see cref="Worker"/> and <see cref="TrackingManager"/> objects.
    /// </summary>
    /// @ingroup Core
    /// \deprecated TrackingManagerReference is obsolete and slated for removal in the
    ///  next major release. Instead of inheriting from TrackingManagerReference,
    ///  use TrackingManager.Instance to access the tracking manager in the scene.
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracking_manager_reference.html")]
    [System.Obsolete(
        "TrackingManagerReference is obsolete and slated for removal in the" +
        " next major release. Instead of inheriting from TrackingManagerReference," +
        " use TrackingManager.Instance to access the tracking manager in the scene.")]
    public abstract class TrackingManagerReference : MonoBehaviour
    {
        /// \deprecated TrackingManagerNotFoundException is obsolete and slated for removal in the
        ///  next major release. Use NullSingletonException instead.
        [System.Obsolete(
            "TrackingManagerNotFoundException is obsolete and slated for removal in the" +
            " next major release. Use NullSingletonException instead.")]
        public class TrackingManagerNotFoundException : NullSingletonException
        {
            /// <summary>
            /// Exception that is thrown when the TrackingManager is tried to be accessed
            /// while it is null. This happens if there is no active GameObject
            /// with a `TrackingManager` component in the scene.
            /// </summary>
            public TrackingManagerNotFoundException()
                : base("Could not find a TrackingManager in the scene") {}
        }

        /// <summary>
        ///  Reference to used TrackingManager.
        /// </summary>
        /// <remarks>
        ///  Is set automatically by searching for an active GameObject
        ///  with a TrackingManager in the scene.
        /// </remarks>
        protected TrackingManager trackingManager
        {
            get => TrackingManager.Instance;
        }

        protected Worker worker
        {
            get => TrackingManager.Instance.Worker;
        }

        protected virtual void ResetReference()
        {
            
        }

        /// <summary>
        ///  Initializes the <see cref="trackingManager"/> and <see cref="worker"/>
        ///  member variables.
        /// </summary>
        /// <returns>
        ///  <c>true</c>, on success; <c>false</c> otherwise.
        /// </returns>
        /// \deprecated InitWorkerReference is obsolete.\nThe reference will be searched automatically when accessing the \"trackingManager\" property.
        [System.Obsolete("InitWorkerReference is obsolete.\nThe reference will be searched automatically when accessing the \"trackingManager\" property.")]
        protected virtual bool InitWorkerReference()
        {
            return this.trackingManager != null && this.worker != null;
        }
    }
}
