using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Component that can be used to save the current model tracker
    /// parameter configuration into a file.
    /// </summary>
    /// <remarks>
    /// Set the `saveConfigurationURI` parameter to specify where
    /// the parameter configuration is to be written.
    /// It's possible to use VisionLib file schemes (e.g. `local-storage-dir`) here.
    /// On HoloLens, use the `capture-dir` scheme to write the configuration
    /// to the Videos/Captures folder which can be accessed via the file explorer.
    /// </remarks>
    /// @ingroup Core
    /// \deprecated ModelTrackerParameters is obsolete and should not be used. It only exists for
    ///  backwards-compatibility with legacy VisionLib tracking scenes.
    ///  Save init poses directly using ConfigurationFileWriter.SaveCurrentConfiguration or via
    ///  the TrackingConfiguration Component in your scene.
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "model_tracker_parameters.html")]
    [Obsolete(
        "ModelTrackerParameters is obsolete and should not be used. It only exists for" +
        " backwards-compatibility with legacy VisionLib tracking scenes." +
        " Save init poses directly using " +
        " ConfigurationFileWriter.SaveCurrentConfiguration or via " +
        " the TrackingConfiguration Component in your scene.")]
    [AddComponentMenu("VisionLib/Core/Model Tracker Parameters")]
    public class ModelTrackerParameters : MonoBehaviour
    {
        [Tooltip(
            "Specifies where the parameter configuration is to be written." +
            "\nE.g. local-storage-dir:VisionLib/myConfig.vl")]
        public string saveConfigurationURI = "";

        public void SaveCurrentConfiguration()
        {
            TrackingManager.CatchCommandErrors(
                ConfigurationFileWriter.SaveCurrentConfigurationAsyncAndLogExceptions(
                    this.saveConfigurationURI),
                this);
        }
    }
}
