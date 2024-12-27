using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details.Singleton;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// This component must be placed on the same GameObject as a <see cref="TrackingAnchor"/> component and will
    /// affect only this <see cref="TrackingAnchor"/>.
    ///
    /// If your model is located on a flat surface and will never be titled
    /// relative to the worlds horizon, you can improve the tracking results
    /// in some cases by using the plane constrained mode.
    ///
    /// As long as this is enabled, the model tracker will only try to find
    /// poses that align the models up vector with the worlds up vector.
    ///
    /// Only works with ARCore, ARKit, ARFoundation, HoloLens or other
    /// external SLAM sources. Extendible tracking has to be turned on.
    ///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "plane_constrained_mode.html")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrackingAnchor))]
    [AddComponentMenu("VisionLib/Core/Plane Constrained Mode")]
    public class PlaneConstrainedMode : MonoBehaviour
    {
        [Tooltip("Unit vector defining the up direction with respect to the model in the model coordinate system. For instance, if your model was created in a 3D tool with a Z-up coordinate system — and if the top of your model lies in this direction — you will have to set this to `(0,0,1)`.")]
        public Vector3 modelUpVector = new Vector3(0, 1, 0);
        [Tooltip("The point in the model coordinate system around which the model will be rotated to align the 'Model up Vector' with the specified 'World up Vector'.")]
        public Vector3 modelCenter = new Vector3(0, 0, 0);
        [Tooltip("Unit vector defining the up direction with respect to your scene content in Unity's world coordinate system. If 'up' w.r.t. your content is not the y-direction, replace this setting with your custom unit up vector.")]
        public Vector3 worldUpVector = new Vector3(0, 1, 0);

        private async Task SetConstraintInTrackerAsync()
        {
            var trackingAnchor = this.gameObject.GetComponent<TrackingAnchor>();
            await MultiModelTrackerCommands.Set1DRotationConstraintAsync(
                TrackingManager.Instance.Worker,
                trackingAnchor.GetAnchorName(),
                this.worldUpVector,
                this.modelUpVector,
                this.modelCenter);
            NotificationHelper.SendInfo("Set plane constraint");
        }

        /// <summary>
        /// Limits the rotation of the model to the given plain.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        private void SetConstraintInTracker()
        {
            TrackingManager.CatchCommandErrors(SetConstraintInTrackerAsync(), this);
        }

        private async Task DisableConstraintInTrackerAsync()
        {
            var trackingAnchor = this.gameObject.GetComponent<TrackingAnchor>();
            await MultiModelTrackerCommands.DisableConstraintAsync(
                TrackingManager.Instance.Worker,
                trackingAnchor.GetAnchorName());
            NotificationHelper.SendInfo("Disabled plane constraint");
        }

        /// <summary>
        /// Disables the constraint in the current tracker.
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        private void DisableConstraintInTracker()
        {
            TrackingManager.CatchCommandErrors(DisableConstraintInTrackerAsync(), this);
        }

        private void OnEnable()
        {
            var trackingAnchor = this.gameObject.GetComponent<TrackingAnchor>();
            if (trackingAnchor.AnchorExists)
            {
                SetConstraintInTracker();
            }
            trackingAnchor.OnAnchorAdded += SetConstraintInTracker;
        }

        private void OnDisable()
        {
            var trackingAnchor = this.gameObject.GetComponent<TrackingAnchor>();
            if (trackingAnchor.AnchorExists)
            {
                DisableConstraintInTracker();
            }
            trackingAnchor.OnAnchorAdded -= SetConstraintInTracker;
        }
    }
}
