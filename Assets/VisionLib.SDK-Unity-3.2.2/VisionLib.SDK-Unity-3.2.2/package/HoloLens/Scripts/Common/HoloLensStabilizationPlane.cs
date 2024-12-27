using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Positions stabilization plane over HoloLensTracker content.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The stabilization plane is a method for reducing holographic
    ///   turbulence. See
    ///   https://developer.microsoft.com/en-us/windows/mixed-reality/case_study_-_using_the_stabilization_plane_to_reduce_holographic_turbulence
    ///   for further details.
    ///  </para>
    ///  <para>
    ///   This will only work well, if the origin of the content is actually
    ///   the part the user should focus on.
    ///  </para>
    ///  <para>
    ///   For more advanced stabilization plane features, please consider
    ///   disabling this behaviour and using the functionalities of the
    ///   MixedRealityToolkit for Unity
    ///   (https://github.com/Microsoft/MixedRealityToolkit-Unity) instead.
    ///  </para>
    /// </remarks>
    /// @ingroup HoloLens
    /// \deprecated HoloLensStabilizationPlane is obsolete. Instead enable the \"Enable Depth Buffer Sharing\" option.
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "holo_lens_stabilization_plane.html")]
    [AddComponentMenu("VisionLib/HoloLens/HoloLens Stabilization Plane")]
    [System.Obsolete("HoloLensStabilizationPlane is obsolete. Instead enable the \"Enable Depth Buffer Sharing\" option.")]
    public class HoloLensStabilizationPlane : MonoBehaviour
    {
        /// <summary>
        ///  Reference to the used HoloLensTracker.
        /// </summary>
        /// <remarks>
        ///  If this is not defined, then it will get set automatically.
        /// </remarks>
        public HoloLensTracker holoLensTracker;

        [Tooltip(
            "GameObject shown while tracking is lost. If not specified, focus will lie on the content object of the HoloLens Tracker.")]
        public GameObject trackingAid;

        private bool objectTracked = false;

        private void Start()
        {
            // Automatically find the HoloLensTracker, if it wasn't
            // specified explicitly
            if (this.holoLensTracker == null)
            {
                // Try to find it on the same GameObject
                this.holoLensTracker = GetComponent<HoloLensTracker>();
                if (this.holoLensTracker == null)
                {
                    // Try to find it anywhere in the scene
                    this.holoLensTracker = FindObjectOfType<HoloLensTracker>();
                    if (this.holoLensTracker == null)
                    {
                        LogHelper.LogWarning("No GameObject with HoloLensTracker found");
                    }
                }
            }
        }

        private Transform GetContentTransform()
        {
            if (this.holoLensTracker == null)
            {
                return null;
            }

            if (this.holoLensTracker.content == null)
            {
                return null;
            }

            return this.holoLensTracker.content.transform;
        }

        private Transform GetTrackingAidTransform()
        {
            if (this.trackingAid != null)
            {
                return this.trackingAid.transform;
            }
            else
            {
                return GetContentTransform();
            }
        }

        private void LateUpdate()
        {
            Transform targetTransform;

            // set focus on superimposition
            if (this.objectTracked)
            {
                targetTransform = GetContentTransform();
            }
            // set focus on trackingAid
            else
            {
                targetTransform = GetTrackingAidTransform();
            }

            if (targetTransform == null)
            {
                return;
            }

            XRDisplaySubsystem xRDisplaySubsystem = GetDisplaySubsystem();

            xRDisplaySubsystem?.SetFocusPlane(
                targetTransform.position,
                -CameraProvider.MainCamera.transform.forward,
                Vector3.zero);
        }

        private void HandleTrackingStates(TrackingState state)
        {
            if (state.objects.Length == 0)
            {
                this.objectTracked = false;
                return;
            }

            string newState = state.objects[0].state;

            if (newState == "lost")
            {
                this.objectTracked = false;
            }
            else if (newState == "critical" || newState == "tracked")
            {
                this.objectTracked = true;
            }
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackingStates += HandleTrackingStates;
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackingStates -= HandleTrackingStates;
        }

        /// <summary>
        /// The XR SDK display subsystem for the currently loaded XR plug-in.
        /// </summary>
        private static XRDisplaySubsystem GetDisplaySubsystem()
        {
            List<XRDisplaySubsystem> xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            XRDisplaySubsystem displaySubsystem = null;

            SubsystemManager.GetInstances(xrDisplaySubsystems);
            foreach (XRDisplaySubsystem xrDisplaySubsystem in xrDisplaySubsystems)
            {
                if (xrDisplaySubsystem.running)
                {
                    displaySubsystem = xrDisplaySubsystem;
                    break;
                }
            }
            return displaySubsystem;
        }
    }
}
