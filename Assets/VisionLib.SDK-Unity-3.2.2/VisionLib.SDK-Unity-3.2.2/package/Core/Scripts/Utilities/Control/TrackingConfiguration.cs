using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Windows;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.API.Native;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// Use this component to save a reference to the used tracking
    /// configuration (vl-file), license and calibration file
    /// and to start tracking with the options:
    /// auto start, input selection and external SLAM.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "tracking_configuration.html")]
    [AddComponentMenu("VisionLib/Tracking Configuration")]
    public class TrackingConfiguration : MonoBehaviour, ISceneValidationCheck
    {
        /// \deprecated `TrackingConfiguration.path` is obsolete. Use `TrackingConfiguration.configurationFileReference.uri` instead.
        [Obsolete(
            "`TrackingConfiguration.path` is obsolete. " +
            "Use `TrackingConfiguration.configurationFileReference.uri` instead.")]
        [SerializeField]
        private string path = "";

        [Serializable]
        public class FilePathReference
        {
            public string uri;

            public enum FieldType
            {
                Object,
                URI
            }

            [SerializeField]
            private FieldType fieldType;
        }

        [FilePathReferenceField(
            "Tracking Configuration",
            ".vl",
            FilePathReferenceFieldAttribute.Mandatory.Yes,
            FilePathReferenceFieldAttribute.AllowProjectDir.No)]
        [SerializeField]
        public FilePathReference configurationFileReference = null;

        [FilePathReferenceField(
            "License",
            ".xml",
            FilePathReferenceFieldAttribute.Mandatory.No,
            FilePathReferenceFieldAttribute.AllowProjectDir.No)]
        [SerializeField]
        private FilePathReference licenseFileReference = null;

        [FilePathReferenceField(
            "Calibration",
            ".json",
            FilePathReferenceFieldAttribute.Mandatory.No,
            FilePathReferenceFieldAttribute.AllowProjectDir.Yes)]
        [SerializeField]
        private FilePathReference calibrationFileReference = null;

        [Tooltip("Automatically start tracking as soon as a TrackingManager is enabled.")]
        public bool autoStartTracking = false;

        [Tooltip(
            "Enable SLAM e.g. from ARCore/ ARKit. Changes will take effect on the next tracking start.")]
        public bool extendTrackingWithSLAM = false;

        [Tooltip(
            "Enable improved localization of the objects, if they have fixed positions in the scene. Changes will take effect on the next tracking start.")]
        public bool staticScene = false;

        [Tooltip("Disregard setup issues during the build process.")]
        public bool ignoreSetupIssues = false;

        public enum InputSource
        {
            TrackingConfig = 0,
            InputSelection = 1,
            ImageSequence = 2,
            ImageInjection = 3
        }

        [Tooltip(
            "\"TrackingConfig\": Use the input that is defined in the referenced Tracking Configuration File." +
            "\n\n\"InputSelection\": Show available input sources in the UI to select which one is used on tracking start." +
            "\n\n\"ImageSequence\": Use an image sequence from a specified URI as the input." +
            "\n\n\"ImageInjection\": Use your own camera implementation to acquire images and inject them into VisionLib.")]
        [FormerlySerializedAs("useInputSelection")]
        public InputSource inputSource = InputSource.TrackingConfig;

        [Tooltip("The location of the image sequence, e.g. \"project-dir:testsequence/*.jpg\".")]
        public string imageSequenceURI = "";

        [HideInInspector]
        public bool useResolutionSelection = false;
        [Tooltip("If true, the camera selection dialog will also show up on mobile devices.")]
        public bool showOnMobileDevices = false;

        private InputSourceSelection inputSelection;

        private void OnEnable()
        {
#pragma warning disable CS0618 // TrackingConfiguration.path is obsolete
            if (this.path != "")
            {
                SetConfigurationUriFromLegacyPathParameter();
            }
#pragma warning restore CS0618 // TrackingConfiguration.path is obsolete

            if (this.autoStartTracking)
            {
                if (TrackingManager.Instance.HasWorker())
                {
                    StartTracking();
                }
                else
                {
                    TrackingManager.OnWorkerCreated += StartTracking;
                }
            }
        }

        /// <summary>
        /// Start tracking using the tracking configuration, license,
        /// and calibration that are set in this component.
        /// </summary>
        public void StartTracking()
        {
            StartTrackingWithParameters();

            if (this.autoStartTracking)
            {
                TrackingManager.OnWorkerCreated -= StartTracking;
            }
        }

        /// <summary>
        /// Start tracking with arguments that are only applied for this tracking start.
        /// </summary>
        /// \deprecated `StartTracking` with arguments is obsolete. Use `StartTrackingWithParameters` instead.
        [Obsolete("`StartTracking` with arguments is obsolete. Use `StartTrackingWithParameters` instead.")]
        public void StartTracking(
            bool? extendTrackingWithSLAMOverride = null,
            bool? useInputSelectionOverride = null,
            bool? useResolutionSelectionOverride = null,
            bool? showOnMobileDevicesOverride = null,
            bool? staticSceneOverride = null)
        {
            InputSource inputSourceOverride = useInputSelectionOverride.HasValue
                ? (useInputSelectionOverride.Value
                    ? InputSource.InputSelection
                    : InputSource.TrackingConfig)
                : this.inputSource;

            StartTrackingWithParameters(
                extendTrackingWithSLAMOverride,
                inputSourceOverride,
                useResolutionSelectionOverride,
                showOnMobileDevicesOverride,
                staticSceneOverride);
        }

        /// <summary>
        /// Start tracking with arguments that are only applied for this tracking start.
        /// </summary>
        public void StartTrackingWithParameters(
            bool? extendTrackingWithSLAMOverride = null,
            InputSource? inputSourceOverride = null,
            bool? useResolutionSelectionOverride = null,
            bool? showOnMobileDevicesOverride = null,
            bool? staticSceneOverride = null)
        {
            SetLicenseAndCalibrationInTrackingManager();

            var usedInputSource = inputSourceOverride ?? this.inputSource;
            var extendWithSLAM = extendTrackingWithSLAMOverride ?? this.extendTrackingWithSLAM;
            var useStaticScene = staticSceneOverride ?? this.staticScene;

            if (usedInputSource == InputSource.TrackingConfig ||
                QueryHelper.CustomInputSetInQueryString(this.configurationFileReference.uri))
            {
                StartTrackingWithParameters(extendWithSLAM, useStaticScene);
            }
            else if (usedInputSource == InputSource.InputSelection)
            {
                if ((showOnMobileDevicesOverride ?? this.showOnMobileDevices) ||
                    !RunsOnMobileDevice())
                {
                    StartCameraSelection(
                        useResolutionSelectionOverride ?? this.useResolutionSelection);
                }
                else
                {
                    StartTrackingWithParameters(extendWithSLAM, useStaticScene);
                }
            }
            else if (usedInputSource == InputSource.ImageSequence)
            {
                StartTrackingWithParameters(extendWithSLAM, useStaticScene, this.imageSequenceURI);
            }
            else if (usedInputSource == InputSource.ImageInjection)
            {
                StartTrackingWithParameters(extendWithSLAM, useStaticScene, null, null, true);
            }
        }

        /// <summary>
        /// See <see cref="ConfigurationFileWriter.SaveCurrentConfigurationAsync"/>.
        /// </summary>
        public void SaveCurrentTrackerConfiguration(string fileURI)
        {
            TrackingManager.CatchCommandErrors(
                ConfigurationFileWriter.SaveCurrentConfigurationAsyncAndLogExceptions(
                    fileURI,
                    this),
                this);
        }

        private void StartTrackingWithParameters(
            bool extendibleTracking,
            bool staticSceneValue,
            string imageSequenceURI = null,
            InputSourceSelection.InputSource inputDevice = null,
            bool useImageInjection = false)
        {
            if (string.IsNullOrEmpty(this.configurationFileReference.uri))
            {
                var errorMessage = "No tracking configuration set. Can not start tracking.";
                NotificationHelper.SendError(errorMessage, this);

                return;
            }

            var additionalQueryParameters = GetAdditionalQueryParameters(
                this.configurationFileReference.uri,
                extendibleTracking,
                staticSceneValue,
                imageSequenceURI,
                inputDevice,
                useImageInjection);
            var URIwithQueryString = QueryHelper.AppendQueryParametersToURI(
                this.configurationFileReference.uri,
                additionalQueryParameters);

            LogHelper.LogDebug("Start Tracking with uri: " + URIwithQueryString);
            TrackingManager.Instance.StartTracking(URIwithQueryString);
        }

        private static List<string> GetAdditionalQueryParameters(
            string uri,
            bool extendTracking,
            bool staticScene,
            string imageSequenceURI,
            InputSourceSelection.InputSource inputDevice,
            bool useImageInjection)
        {
            var queryParameters = new List<string>();

            if (extendTracking && !QueryHelper.CustomExtendibleTrackingValueSetInQueryString(uri))
            {
                queryParameters.Add(
                    QueryHelper.GenerateBooleanQuery(
                        TrackingParameterNames.extendibleTracking,
                        true));

                if (staticScene && !QueryHelper.CustomStaticSceneValueSetInQueryString(uri))
                {
                    queryParameters.Add(
                        QueryHelper.GenerateBooleanQuery(TrackingParameterNames.staticScene, true));
                }
            }
            if (!extendTracking && staticScene)
            {
                NotificationHelper.SendWarning(
                    "Using \"staticScene\" requires the use of \"Extend Tracking With SLAM\"." +
                    "Since the prerequisites aren't met, the \"staticScene\" parameter will be ignored.");
            }

            if (!string.IsNullOrEmpty(imageSequenceURI) && inputDevice != null)
            {
                NotificationHelper.SendWarning(
                    "Both input device and image sequence URI are set. " +
                    "Using the image sequence as an input.");
            }

            if (!string.IsNullOrEmpty(imageSequenceURI))
            {
                queryParameters.AddRange(
                    QueryHelper.GenerateImageSequenceQueryParameters(
                        new ImageSequenceQueryParameter {uri = imageSequenceURI}));

                if (extendTracking)
                {
                    queryParameters.Add(
                        QueryHelper.GenerateBooleanQuery(
                            TrackingParameterNames.simulateExternalSLAM,
                            true));
                }

                return queryParameters;
            }

            if (inputDevice != null && !QueryHelper.CustomInputSetInQueryString(uri))
            {
                queryParameters.AddRange(
                    QueryHelper.GenerateInputSourceQueryParameters(inputDevice));
            }

            if (useImageInjection)
            {
                queryParameters.AddRange(QueryHelper.GenerateImageInjectionQueryParameters());
                if (extendTracking)
                {
                    queryParameters.Add(
                        QueryHelper.GenerateBooleanQuery(
                            TrackingParameterNames.simulateExternalSLAM,
                            true));
                }
            }

            return queryParameters;
        }

        private void SetLicenseAndCalibrationInTrackingManager()
        {
            if (!String.IsNullOrEmpty(this.licenseFileReference.uri))
            {
                TrackingManager.Instance.licenseFile.path = this.licenseFileReference.uri;
            }

            if (!String.IsNullOrEmpty(this.calibrationFileReference.uri))
            {
                TrackingManager.Instance.calibrationDataBaseURI = this.calibrationFileReference.uri;
            }
        }

        private static bool RunsOnMobileDevice()
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            return true;
#else
            return false;
#endif
        }

        private void SetConfigurationUriFromLegacyPathParameter()
        {
#pragma warning disable CS0618 // TrackingConfiguration.path is obsolete
            this.configurationFileReference.uri = PathHelper.CombinePaths(
                "streaming-assets-dir:VisionLib",
                this.path);
            this.path = "";
#pragma warning restore CS0618 // TrackingConfiguration.path is obsolete
        }

        /// <summary>
        /// If SLAM (e.g. ARKit/ ARCore or internal SLAM) is enabled.
        /// Will restart the tracking if it is already running
        /// to apply the changes.
        /// </summary>
        public void ExtendTrackingWithSLAM(bool useSLAM)
        {
            bool restartTracking = (this.extendTrackingWithSLAM != useSLAM) &&
                                   TrackingManager.DoesTrackerExistAndIsInitialized();

            this.extendTrackingWithSLAM = useSLAM;

            if (restartTracking)
            {
                StartTracking();
            }
        }

        /// <summary>
        /// Enable/Disable enhanced tracking for static scenes.
        /// Will restart the tracking if it is already running
        /// to apply the changes.
        /// </summary>
        public void StaticScene(bool useStaticScene)
        {
            bool restartTracking = (this.staticScene != useStaticScene) &&
                                   TrackingManager.DoesTrackerExistAndIsInitialized();

            this.staticScene = useStaticScene;

            if (restartTracking)
            {
                StartTracking();
            }
        }

        /// <summary>
        /// Set the URI of the tracking configuration file,
        /// which is used for the next tracking start.
        /// </summary>
        /// <remarks>
        /// Example: streaming-assets-dir:VisionLib/MyTracking.vl
        /// </remarks>
        public void SetConfigurationPath(string newURI)
        {
            this.configurationFileReference.uri = newURI;
        }

        /// <summary>
        /// Get the URI of the tracking configuration file,
        /// which is used to start tracking.
        /// </summary>
        public string GetConfigurationPath()
        {
            return this.configurationFileReference.uri;
        }

        /// <summary>
        /// Set the URI of the used license file.
        /// Will be applied on the next tracking start.
        /// </summary>
        /// <remarks>
        /// Example: streaming-assets-dir:VisionLib/license.xml
        /// </remarks>
        public void SetLicensePath(string newURI)
        {
            this.licenseFileReference.uri = newURI;
        }

        /// <summary>
        /// Get the URI of the used license file.
        /// </summary>
        public string GetLicensePath()
        {
            return this.licenseFileReference.uri;
        }

        /// <summary>
        /// Set the URI of the calibration file.
        /// Will be applied on the next tracking start.
        /// </summary>
        /// <remarks>
        /// Example: streaming-assets-dir:VisionLib/calibration.json
        /// </remarks>
        public void SetCalibrationPath(string newURI)
        {
            this.calibrationFileReference.uri = newURI;
        }

        /// <summary>
        /// Get the URI of the used calibration file.
        /// </summary>
        public string GetCalibrationPath()
        {
            return this.calibrationFileReference.uri;
        }

        private async void StartCameraSelection(bool resolutionSelection)
        {
            if (!this.inputSelection)
            {
                this.inputSelection = this.gameObject.AddComponentUndoable<InputSourceSelection>();
            }

            try
            {
                var selectedSource = await this.inputSelection.GetUserInputSelectionAsync(
                    resolutionSelection);
                this.ThrowIfNotAliveAndEnabled();
                StartTrackingWithInputDevice(selectedSource);
            }
            catch (TaskCanceledException) {}
        }

        private void StartTrackingWithInputDevice(InputSourceSelection.InputSource inputDevice)
        {
            StartTrackingWithParameters(
                this.extendTrackingWithSLAM,
                this.staticScene,
                null,
                inputDevice);
        }

        public void CancelCameraSelection()
        {
            if (this.inputSelection != null)
            {
                this.inputSelection.Cancel();
            }
        }

#if UNITY_EDITOR
        private List<SetupIssue> EvaluateImageSequenceURI(string singleImageSequenceURI)
        {
            if (string.IsNullOrEmpty(singleImageSequenceURI))
            {
                return SetupIssue.NoIssues();
            }
            try
            {
                var physicalPath = VLSDK.GetPhysicalPath(singleImageSequenceURI);
                var parentDirectory = System.IO.Directory.GetParent(physicalPath)?.FullName;
                if (!Directory.Exists(parentDirectory))
                {
                    return new List<SetupIssue>
                    {
                        new SetupIssue(
                            "Image Sequence path does not exist",
                            $"The given image sequence path points to a directory ({parentDirectory}), which does not exist.",
                            SetupIssue.IssueType.Error,
                            this.gameObject)
                    };
                }

                if (singleImageSequenceURI.Contains('*') || VLSDK.FileExists(physicalPath))
                {
                    // List with wildcards are not evaluated and existing files are no problem.
                    return SetupIssue.NoIssues();
                }

                Debug.Log($"physicalPath {physicalPath}");
                if (Directory.Exists(physicalPath))
                {
                    // URI is directory
                    return new List<SetupIssue>
                    {
                        new SetupIssue(
                            "URI is directory",
                            $"The given image sequence path points to an existing directory ({physicalPath}) without a wildcard component.",
                            SetupIssue.IssueType.Error,
                            this.gameObject)
                    };
                }
                return new List<SetupIssue>
                {
                    new SetupIssue(
                        "Image Sequence path does not exist",
                        $"The given image sequence path points to a file ({physicalPath}), which does not exist.",
                        SetupIssue.IssueType.Error,
                        this.gameObject)
                };
            }
            catch (Exception)
            {
                return new List<SetupIssue>
                {
                    new SetupIssue(
                        "Image Sequence path invalid",
                        "The image sequence path could not be resolved correctly.",
                        SetupIssue.IssueType.Error,
                        this.gameObject)
                };
            }
        }

        public List<SetupIssue> GetSceneIssues()
        {
            if (this.inputSource == InputSource.ImageSequence)
            {
                if (string.IsNullOrEmpty(this.imageSequenceURI))
                {
                    return new List<SetupIssue>()
                    {
                        new SetupIssue(
                            "No image sequence selected",
                            "To use the imageSequence input source, you have to provide the path to an image sequence.",
                            SetupIssue.IssueType.Error,
                            this.gameObject)
                    };
                }

                return this.imageSequenceURI.Split(';').SelectMany(EvaluateImageSequenceURI)
                    .ToList();
            }
            return SetupIssue.NoIssues();
        }
#endif
    }
}
