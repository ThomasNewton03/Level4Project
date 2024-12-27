using UnityEditor;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// \deprecated The TrackingStateProvider component is obsolete. Use the TrackingEvents of the TrackingAnchor, the PosterTracker or the CameraCalibration instead
    [CustomEditor(typeof(TrackingStateProvider))]
    [System.Obsolete(
        "The TrackingStateProvider component is obsolete. Use the TrackingEvents of the TrackingAnchor, the PosterTracker or the CameraCalibration instead")]
    public class TrackingStateProviderEditor : SceneValidationCheckEditor
    {
    }
}
