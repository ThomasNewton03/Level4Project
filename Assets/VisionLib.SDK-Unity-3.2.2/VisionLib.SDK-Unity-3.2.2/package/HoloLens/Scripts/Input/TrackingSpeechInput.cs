using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visometry.Helpers;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core;

#if (UNITY_STANDALONE_WIN || UNITY_WSA_10_0)
using UnityEngine.Windows.Speech;
#endif

namespace Visometry.VisionLib.SDK.HoloLens
{
    /**
     *  @ingroup HoloLens
     */
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "tracking_speech_input.html")]
    [AddComponentMenu("VisionLib/HoloLens/Tracking Speech Input")]
    public class TrackingSpeechInput : MonoBehaviour
    {
        [Tooltip("Configuration to use for starting tracking.")]
        public TrackingConfiguration trackingConfiguration;

        public bool enableInitDataWriting = false;
        [OnlyShowIf("enableInitDataWriting", true)]
        public string initDataPath = "local-storage-dir:/VisionLib/initData.binz";

        public enum VLEvent
        {
            StartTracking, // Simple, AutoInit, Poster, Recorder
            StopTracking, // Simple, AutoInit, Poster, Recorder
            PauseTracking, // Simple, AutoInit, Poster
            ResumeTracking, // Simple, AutoInit, Poster
            ResetTrackingSoft, // Simple, AutoInit
            ResetTrackingHard, // Simple, AutoInit, Poster
            ShowDebugImage, // Simple, AutoInit
            HideDebugImage, // Simple, AutoInit
            ReadInitData, // Simple
            WriteInitData, // Simple
            WideFieldOfView, // Simple, AutoInit, Poster
            NarrowFieldOfView, // Simple, AutoInit, Poster
            ConstrainToPlane, // Simple, AutoInit
            DisableConstraint, // Simple, AutoInit
            PauseCapturing, // Recorder
            ResumeCapturing, // Recorder
        }

        [System.Serializable]
        public class VoiceCommand
        {
            public VLEvent vlEvent;
            public string keyWord;
            public KeyCode keyCode;
        }
#pragma warning disable CS0414
        [SerializeField]
        private VoiceCommand[] voiceCommands = null;
#pragma warning restore CS0414

#if (UNITY_STANDALONE_WIN || UNITY_WSA_10_0)
        private KeywordRecognizer keywordRecognizer;
        private Dictionary<string, VLEvent> keywords = new Dictionary<string, VLEvent>();
        private Dictionary<KeyCode, VLEvent> keyCodes = new Dictionary<KeyCode, VLEvent>();

        private void Awake()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                foreach (VoiceCommand voiceCommand in this.voiceCommands)
                {
                    this.keywords.Add(voiceCommand.keyWord, voiceCommand.vlEvent);
                    this.keyCodes.Add(voiceCommand.keyCode, voiceCommand.vlEvent);
                }

                this.keywordRecognizer = new KeywordRecognizer(this.keywords.Keys.ToArray());
            }
        }

        private void OnEnable()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                this.keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
                this.keywordRecognizer.Start();
            }
        }

        private void OnDisable()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                this.keywordRecognizer.Stop();
                this.keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            }
        }

        private void OnDestroy()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                this.keywordRecognizer.Dispose();
            }
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode code in keyCodes.Keys)
                {
                    if (Input.GetKey(code))
                    {
                        TriggerEvent(this.keyCodes[code]);
                    }
                }
            }
        }

        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            TriggerEvent(this.keywords[args.text]);
        }

        private void TriggerEvent(VLEvent vlEvent)
        {
            try
            {
                switch (vlEvent)
                {
                    case VLEvent.StartTracking:
                    {
                        HoloLensTrackerEvents.StartTracking(this.trackingConfiguration);
                        break;
                    }
                    case VLEvent.StopTracking:
                    {
                        HoloLensTrackerEvents.StopTracking();
                        break;
                    }
                    case VLEvent.PauseTracking:
                    {
                        HoloLensTrackerEvents.PauseTracking();
                        break;
                    }
                    case VLEvent.ResumeTracking:
                    {
                        HoloLensTrackerEvents.ResumeTracking();
                        break;
                    }
                    case VLEvent.ResetTrackingSoft:
                    {
                        HoloLensTrackerEvents.ResetTrackingSoft();
                        break;
                    }
                    case VLEvent.ResetTrackingHard:
                    {
                        HoloLensTrackerEvents.ResetTrackingHard();
                        break;
                    }
                    case VLEvent.ShowDebugImage:
                    {
                        HoloLensTrackerEvents.ShowDebugImage();
                        break;
                    }
                    case VLEvent.HideDebugImage:
                    {
                        HoloLensTrackerEvents.HideDebugImage();
                        break;
                    }
                    case VLEvent.ReadInitData:
                    {
                        HoloLensTrackerEvents.ReadInitData(this.initDataPath);
                        break;
                    }
                    case VLEvent.WriteInitData:
                    {
                        HoloLensTrackerEvents.WriteInitData(this.initDataPath);
                        break;
                    }
                    case VLEvent.WideFieldOfView:
                    {
                        HoloLensTrackerEvents.SetWideFieldOfView();
                        break;
                    }
                    case VLEvent.NarrowFieldOfView:
                    {
                        HoloLensTrackerEvents.SetNarrowFieldOfView();
                        break;
                    }
                    case VLEvent.ConstrainToPlane:
                    {
                        HoloLensTrackerEvents.EnablePlaneConstrainMode();
                        break;
                    }
                    case VLEvent.DisableConstraint:
                    {
                        HoloLensTrackerEvents.DisablePlaneConstrainMode();
                        break;
                    }
                    case VLEvent.PauseCapturing:
                    {
                        HoloLensTrackerEvents.PauseRecording();
                        break;
                    }
                    case VLEvent.ResumeCapturing:
                    {
                        HoloLensTrackerEvents.ResumeCapturing();
                        break;
                    }
                    default:
                        break;
                }
            }
            catch (HoloLensTrackerEvents.ComponentNotFoundException e)
            {
                LogHelper.LogError(
                    "Can not " + vlEvent.ToString() + " because no " + e.Message +
                    " was found in the scene.");
            }
        }
#else
        private void Start()
        {
            LogHelper.LogWarning(
                "The VLWindowsSpeechInput only works for Windows and Windows Store applications",
                this);
        }
#endif
    }
}
