using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.Details;

#if (UNITY_STANDALONE_WIN || UNITY_WSA_10_0)
using UnityEngine.Windows.Speech;
#endif

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Turns speech input into a UnityEvent.
    /// </summary>
    /// <remarks>
    ///  This behaviour only works under Windows and Universal Windows Platform
    ///  (so also on HoloLens).
    /// </remarks>
    /// @ingroup HoloLens
    [HelpURL(DocumentationLink.holoLensCommands)]
    [AddComponentMenu("VisionLib/HoloLens/Windows Speech Input")]
    public class WindowsSpeechInput : MonoBehaviour, ISceneValidationCheck
    {
        [System.Serializable]
        public class OnKeywordEvent : UnityEvent {}

        [System.Serializable]
        public class VoiceCommand
        {
            public string keyWord;

            public OnKeywordEvent command;
        }

        public VoiceCommand[] voiceCommands;

#if (UNITY_STANDALONE_WIN || UNITY_WSA_10_0)
        private KeywordRecognizer keywordRecognizer;
        private Dictionary<string, OnKeywordEvent> keywords =
            new Dictionary<string, OnKeywordEvent>();

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            OnKeywordEvent keywordEvent;
            // if the keyword recognized is in our dictionary, call that Action.
            if (this.keywords.TryGetValue(args.text, out keywordEvent))
            {
                keywordEvent.Invoke();
            }
        }

        private void Awake()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                foreach (VoiceCommand voiceCommand in this.voiceCommands)
                {
                    // Create keywords for keyword recognizer
                    this.keywords.Add(voiceCommand.keyWord, voiceCommand.command);
                }

                this.keywordRecognizer = new KeywordRecognizer(this.keywords.Keys.ToArray());
            }
        }

        private void OnEnable()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                this.keywordRecognizer.OnPhraseRecognized +=
                    this.KeywordRecognizer_OnPhraseRecognized;
                this.keywordRecognizer.Start();
            }
        }

        private void OnDisable()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                this.keywordRecognizer.Stop();
                this.keywordRecognizer.OnPhraseRecognized -=
                    this.KeywordRecognizer_OnPhraseRecognized;
            }
        }

        private void OnDestroy()
        {
            if (PhraseRecognitionSystem.isSupported)
            {
                this.keywordRecognizer.Dispose();
            }
        }

#else
        private void Start()
        {
            LogHelper.LogWarning(
                "The WindowsSpeechInput only works for Windows and Windows Store applications",
                this);
        }
#endif

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                voiceCommands.Select(voiceCommands => voiceCommands.command),
                this.gameObject);
        }
#endif
    }
}
