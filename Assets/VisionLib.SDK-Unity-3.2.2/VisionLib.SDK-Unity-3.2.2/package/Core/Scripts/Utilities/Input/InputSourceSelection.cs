using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  This behaviour is used by the TrackingConfiguration
    ///  to enable to select the input source that is used for tracking.
    ///  You can choose from a list of available devices or
    ///  use the input defined in your tracking configuration.
    /// </summary>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "input_source_selection.html")]
    [AddComponentMenu("VisionLib/Core/Input Source Selection")]
    public class InputSourceSelection : MonoBehaviour
    {
        private DeviceInfo.Camera selectedCamera;
        private bool showResolutionSelection = false;

        private TaskCompletionSource<InputSource> userInputSelectionResult;

        private enum SelectionState
        {
            CameraSelection,
            ResolutionSelection,
            None
        }

        private SelectionState selectionState = SelectionState.None;

        /// <summary>
        ///  Rectangle for the camera selection window. The actual values will get
        ///  determined automatically at runtime.
        /// </summary>
        private Rect windowRect;

        /// <summary>
        ///  Used to scale the UI inside the OnGUI function.
        /// </summary>
        private GUIMatrixScaler guiScaler = new GUIMatrixScaler(640, 480);

        /// <summary>
        ///  List of available cameras to select from.
        /// </summary>
        private DeviceInfo.Camera[] availableCameras;

        public class InputSource
        {
            public string deviceID;
            public DeviceInfo.Camera.Format format;

            public InputSource(string deviceID = "", DeviceInfo.Camera.Format format = null)
            {
                this.deviceID = deviceID;
                this.format = format;
            }
        }

        public Task<InputSource> GetUserInputSelectionAsync(bool enableResolutionSelection = false)
        {
            DeviceInfo deviceInfo = TrackingManager.Instance.GetDeviceInfo();
            if (deviceInfo == null)
            {
                LogHelper.LogError("An internal error occurred: Could not get device information.");
                return null;
            }
            SetUpDeviceSelection(deviceInfo, enableResolutionSelection);
            this.userInputSelectionResult?.SetCanceled();
            this.userInputSelectionResult = new TaskCompletionSource<InputSource>();
            return this.userInputSelectionResult.Task;
        }

        public void Cancel()
        {
            this.selectionState = SelectionState.None;

            this.userInputSelectionResult?.SetCanceled();
            this.userInputSelectionResult = null;
        }

        private void SetUpDeviceSelection(DeviceInfo deviceInfo, bool enableResolutionSelection)
        {
            this.availableCameras = deviceInfo.availableCameras;
            this.selectionState = SelectionState.CameraSelection;
            this.showResolutionSelection = enableResolutionSelection;
            this.windowRect = new Rect();
        }

        private void StartResolutionSelection()
        {
            this.selectionState = SelectionState.ResolutionSelection;
            this.windowRect = new Rect();
        }

        private void OnGUI()
        {
            switch (this.selectionState)
            {
                case SelectionState.CameraSelection:
                    DisplayWindow("Select your camera", DoInputSelectionWindow);
                    break;
                case SelectionState.ResolutionSelection:
                    DisplayWindow("Select your resolution", DoResolutionSelectionWindow);
                    break;
                case SelectionState.None:
                    break;
                default:
                    break;
            }
        }

        private void DisplayWindow(string label, GUI.WindowFunction windowFunction)
        {
            this.guiScaler.Update();
            this.guiScaler.Set();

            // (We call GUILayout.Window twice. In order to properly position
            // the window in the center of the screen)
            this.windowRect = GUILayout.Window(0, this.windowRect, windowFunction, label);
            this.windowRect.x =
                (this.guiScaler.GetScaledScreenRect().width - this.windowRect.width) / 2.0f;
            this.windowRect.y =
                (this.guiScaler.GetScaledScreenRect().height - this.windowRect.height) / 2.0f;
            this.windowRect = GUILayout.Window(0, this.windowRect, windowFunction, label);

            this.guiScaler.Unset();
        }

        /// <summary>
        /// Display a window with all available cameras
        /// </summary>
        private void DoInputSelectionWindow(int windowID)
        {
            // the state could have been changed inside the "DisplayWindow" function
            if (this.selectionState != SelectionState.CameraSelection)
            {
                return;
            }

            foreach (DeviceInfo.Camera camera in this.availableCameras)
            {
                if (GUILayout.Button(camera.cameraName))
                {
                    this.selectedCamera = camera;
                    ApplyInputSelection(new InputSource(this.selectedCamera.deviceID));
                    return;
                }
            }

            if (!this.showResolutionSelection)
            {
                if (GUILayout.Button("Tracking Config Input"))
                {
                    this.selectedCamera = null;
                    ApplyInputSelection(null);
                    return;
                }
            }

            if (GUILayout.Button("Cancel"))
            {
                Cancel();
            }
        }

        private void ApplyInputSelection(InputSource inputSource)
        {
            if (this.selectedCamera != null)
            {
                LogHelper.LogInfo("Selected Camera: " + this.selectedCamera + "\n");
            }

            if (this.showResolutionSelection && !String.IsNullOrEmpty(inputSource.deviceID))
            {
                StartResolutionSelection();
            }
            else
            {
                FinalizeInputSelection(inputSource);
            }
        }

        /// <summary>
        ///  Creates selection window for all available resolutions
        ///  of the selected camera.
        ///  Only working on Windows.
        /// </summary>
        private void DoResolutionSelectionWindow(int windowID)
        {
            // the state could have been changed inside the "DisplayWindow" function
            if (this.selectionState != SelectionState.ResolutionSelection)
            {
                return;
            }

            if (this.selectedCamera == null)
            {
                Cancel();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            // Add a button for each possible resolution
            int buttonRow = 1;
            foreach (DeviceInfo.Camera.Format format in this.selectedCamera.availableFormats)
            {
                if (GUILayout.Button(format.ToString()))
                {
                    FinalizeInputSelection(new InputSource(this.selectedCamera.deviceID, format));
                    return;
                }

                // Only show 10 buttons per column
                ++buttonRow;
                if (buttonRow >= 10)
                {
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    buttonRow = 0;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Default button
            if (GUILayout.Button("Default"))
            {
                FinalizeInputSelection(new InputSource(this.selectedCamera.deviceID, null));
                return;
            }

            if (GUILayout.Button("Cancel"))
            {
                Cancel();
            }
        }

        private void FinalizeInputSelection(InputSource inputSource)
        {
            this.selectionState = SelectionState.None;
            this.userInputSelectionResult.SetResult(inputSource);
            this.userInputSelectionResult = null;
        }
    }
}
