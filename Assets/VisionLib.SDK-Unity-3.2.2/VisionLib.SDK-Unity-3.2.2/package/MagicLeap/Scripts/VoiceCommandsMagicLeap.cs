using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.MagicLeap
{
    // @ingroup MagicLeap
    [HelpURL(DocumentationLink.APIReferenceURI.MagicLeap + "voice_commands_magic_leap.html")]
    public class VoiceCommandsMagicLeap : MonoBehaviour, ISceneValidationCheck
    {
        private MLVoiceIntentsConfiguration voiceConfiguration;
        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        [Serializable]
        public class NamedEvent
        {
            public string name;
            public UnityEvent action = new UnityEvent();
        }

        public NamedEvent[] voiceEvents;

        private void Awake()
        {
            permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
        }

        private void Start()
        {
            MLPermissions.RequestPermission(MLPermission.VoiceInput, permissionCallbacks);

        }

        private void OnPermissionDenied(string permission)
        {
            NotificationHelper.SendError("Permission for receiving voice commands denied.", this);
        }

        private void OnPermissionGranted(string permission)
        {
            LogHelper.LogDebug("Permission for receiving voice command granted.", this);
            Initialize();
        }

        private void Initialize()
        {
            if (!MLVoice.VoiceEnabled)
            {
                NotificationHelper.SendError(
                    "Voice commands are disabled on your device. Go to `Settings > Magic Leap Inputs > Voice` to enable them.",
                    this);
                return;
            }

            InitVoiceConfiguration();
            MLResult result = MLVoice.SetupVoiceIntents(this.voiceConfiguration);
            if (!result.IsOk)
            {
                NotificationHelper.SendError("Could not setup VoiceIntent: " + result.ToString(), this);
                return;
            }

            MLVoice.OnVoiceEvent += VoiceEvent;
        }

        private void InitVoiceConfiguration()
        {
            this.voiceConfiguration = ScriptableObject.CreateInstance<MLVoiceIntentsConfiguration>();
            this.voiceConfiguration.VoiceCommandsToAdd = new List<MLVoiceIntentsConfiguration.CustomVoiceIntents>();
            this.voiceConfiguration.AllVoiceIntents = new List<MLVoiceIntentsConfiguration.JSONData>();
            this.voiceConfiguration.SlotsForVoiceCommands = new List<MLVoiceIntentsConfiguration.SlotData>();

            for (int i = 0; i < this.voiceEvents.Length; i++)
            {
                MLVoiceIntentsConfiguration.CustomVoiceIntents newIntent = new MLVoiceIntentsConfiguration.CustomVoiceIntents
                {
                    Value = voiceEvents[i].name,
                    Id = (uint)i
                };
                this.voiceConfiguration.VoiceCommandsToAdd.Add(newIntent);
            }
        }

        void VoiceEvent(in bool wasSuccessful, in MLVoice.IntentEvent voiceEvent)
        {
            if (!wasSuccessful)
            {
                return;
            }

            LogHelper.LogDebug(voiceEvent.EventName, this);
            var id = voiceEvent.EventID;
            if (id >= this.voiceEvents.Length)
            {
                NotificationHelper.SendError("VoiceEvent with unknown id: " + id, this);
                return;
            }

            voiceEvents[id].action.Invoke();
        }

        private void OnDestroy()
        {
            MLVoice.Stop();
            MLVoice.OnVoiceEvent -= VoiceEvent;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                voiceEvents.Select(namedEvent => namedEvent.action),
                this.gameObject);
        }
#endif
    }
}
