using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    [AddComponentMenu("VisionLib/Core/Screen Orientation Observer")]
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "screen_orientation_observer.html")]
    public class ScreenOrientationObserver : MonoBehaviour
    {
        [Serializable]
        public class OrientationOverride
        {
            public enum Orientation
            {
                Portrait,
                PortraitUpsideDown,
                LandscapeLeft,
                LandscapeRight
            }

            public bool active = false;
            public Orientation orientation = Orientation.Portrait;
        }

        private DeviceOrientation currentDeviceOrientation = DeviceOrientation.Unknown;

        public delegate void OrientationChangeAction(ScreenOrientation orientation);
        public static event OrientationChangeAction OnOrientationChange;

        public delegate void SizeChangeAction(int width, int height);
        public static event SizeChangeAction OnSizeChange;

        /// <summary>
        ///  Settings for overwriting the screen orientation.
        /// </summary>
        /// <remarks>
        ///  On systems without a screen orientation sensor, Unity will always
        ///  report a portrait screen orientation. By activating the orientation
        ///  override, it's possible to simulate a different screen orientation.
        ///  This allows the proper playback of iOS and Android image sequences
        ///  captured in landscape mode with an "imageRecorder" configuration.
        /// </remarks>
        [FormerlySerializedAs("overwrite")]
        public OrientationOverride orientationOverride;

        /// The orientation, width and height will be set to invalid values at startup. In the first
        /// `OnEnable` call these variables will be set. This will also trigger
        /// <see cref="OnOrientationChange"> and <see cref="OnSizeChange"> events.
        private ScreenOrientation? orientation;
        private int width = -1;
        private int height = -1;

        private static ScreenOrientationObserver instance = null;
        private static ScreenOrientationObserver Instance
        {
            get
            {
                if (ScreenOrientationObserver.instance == null)
                {
                    ScreenOrientationObserver.instance =
                        FindObjectOfType<ScreenOrientationObserver>();
                }
                return ScreenOrientationObserver.instance;
            }
        }

        [System.Obsolete("FindInstance(GameObject go) is obsolete. Use FindInstance() instead.")]
        /// \deprecated FindInstance(GameObject go) is obsolete. Use FindInstance() instead.
        public static ScreenOrientationObserver FindInstance(GameObject go)
        {
            return FindInstance();
        }

        public static ScreenOrientationObserver FindInstance()
        {
            return ScreenOrientationObserver.Instance;
        }

        [System.Obsolete(
            "GetOrientation(GameObject go) is obsolete. Use GetScreenOrientation() instead.")]
        /// \deprecated GetOrientation(GameObject go) is obsolete. Use GetScreenOrientation() instead.
        public static ScreenOrientation GetOrientation(GameObject go)
        {
            return ScreenOrientationObserver.GetScreenOrientation();
        }

        public static ScreenOrientation GetScreenOrientation()
        {
            if (ScreenOrientationObserver.Instance == null)
            {
#if !UNITY_WSA_10_0
                LogHelper.LogWarning(
                    "No ScreenOrientationObserver component found in scene. Returning default orientation.");
#endif
#if (UNITY_WSA_10_0 || UNITY_ANDROID || UNITY_IOS)
                return ScreenOrientation.LandscapeLeft;
#else
                return ScreenOrientation.Portrait;
#endif
            }

            return ScreenOrientationObserver.Instance.GetOrientation();
        }

        /// <summary>
        ///  Returns the current screen orientation considering the override
        ///  setting.
        /// </summary>
        /// <returns>
        ///  Screen.orientation or <see cref="orientationOverride.orientation"/> depending on
        ///  the <see cref="orientationOverride.active"/> value.
        /// </returns>
        public ScreenOrientation GetOrientation()
        {
            if (!this.orientationOverride.active)
            {
                return Screen.orientation;
            }

            // Use the user-defined screen orientation
            switch (this.orientationOverride.orientation)
            {
                case OrientationOverride.Orientation.Portrait:
                    return ScreenOrientation.Portrait;
                case OrientationOverride.Orientation.PortraitUpsideDown:
                    return ScreenOrientation.PortraitUpsideDown;
                case OrientationOverride.Orientation.LandscapeLeft:
                    return ScreenOrientation.LandscapeLeft;
                case OrientationOverride.Orientation.LandscapeRight:
                    return ScreenOrientation.LandscapeRight;
            }

            // This should never happen
            return ScreenOrientation.AutoRotation;
        }

        private void Awake()
        {
            if (ScreenOrientationObserver.instance != null &&
                ScreenOrientationObserver.instance != this)
            {
                Debug.LogWarning(
                    "There already is another ScreenOrientationObserver(" +
                    ScreenOrientationObserver.instance.gameObject.name +
                    ") in the Scene. Make sure that there is only one active ScreenOrientationObserver.",
                    ScreenOrientationObserver.instance);
                return;
            }
            ScreenOrientationObserver.instance = this;
        }

        private void OnEnable()
        {
            UpdateScreenOrientation();
            UpdateScreenSize();
            UpdateDeviceOrientation();
        }

        private void OnDestroy()
        {
            ScreenOrientationObserver.instance = null;
        }

        private void Update()
        {
            UpdateScreenOrientation();
            UpdateScreenSize();
            UpdateDeviceOrientation();
        }

        private void UpdateScreenOrientation()
        {
            var currentOrientation = GetOrientation();
            if (currentOrientation == this.orientation)
            {
                return;
            }

            // Unity sometimes returns an unknown screen orientation on iOS for some reason.
            // Therefore we do not add a default section here.
            switch (currentOrientation)
            {
                case ScreenOrientation.AutoRotation:
                    // The screen orientation should never be 'AutoRotation'
                    LogHelper.LogWarning("Cannot derive correct screen orientation");
                    return;
                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    this.orientation = currentOrientation;
                    OnOrientationChange?.Invoke(currentOrientation);
                    break;
            }
        }

        private void UpdateScreenSize()
        {
            // Device orientation changed?
            if (Screen.width != this.width || Screen.height != this.height)
            {
                this.width = Screen.width;
                this.height = Screen.height;

                OnSizeChange?.Invoke(this.width, this.height);
            }
        }

        private void UpdateDeviceOrientation()
        {
            if (this.currentDeviceOrientation != Input.deviceOrientation)
            {
                this.currentDeviceOrientation = Input.deviceOrientation;
                SetDeviceOrientation(this.currentDeviceOrientation);
            }
        }

        private async Task SetDeviceOrientationAsync(DeviceOrientation devOrientation)
        {
            // default image orientation mode in vlSDK
            int imageRotationMode = 0;

            switch (devOrientation)
            {
                case DeviceOrientation.LandscapeRight:
                    imageRotationMode = 2;
                    break;
                case DeviceOrientation.LandscapeLeft:
                    imageRotationMode = 0;
                    break;
                case DeviceOrientation.Portrait:
                    imageRotationMode = 3;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    imageRotationMode = 1;
                    break;
            }

            try
            {
                await WorkerCommands.SetDeviceOrientationAsync(
                    TrackingManager.Instance.Worker,
                    imageRotationMode);
            }
            catch (TrackingManager.WorkerNotFoundException) {}
        }

        /// <summary>
        /// Sets the image rotation mode of AutoInitManager in vlSDK
        /// </summary>
        /// <remarks> This function will be performed asynchronously.</remarks>
        /// <param name="devOrientation"></param>
        private void SetDeviceOrientation(DeviceOrientation devOrientation)
        {
            TrackingManager.CatchCommandErrors(SetDeviceOrientationAsync(devOrientation), this);
        }
    }
}
