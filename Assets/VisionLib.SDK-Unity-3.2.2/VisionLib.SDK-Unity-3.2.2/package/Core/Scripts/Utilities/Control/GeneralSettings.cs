using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "general_settings.html")]
    [AddComponentMenu("VisionLib/Core/General Settings")]
    public class GeneralSettings : MonoBehaviour, ISceneValidationCheck
    {
        public VLSDK.LogLevel logLevel = VLSDK.LogLevel.Warning;
        private VLSDK.LogLevel previousLogLevel;

        private void Awake()
        {
            ApplyLogLevel();
            this.previousLogLevel = this.logLevel;
        }

        public void Update()
        {
            if (this.logLevel != this.previousLogLevel)
            {
                ApplyLogLevel();
                this.previousLogLevel = this.logLevel;
            }
        }

        private void ApplyLogLevel()
        {
            SetLogLevel(this.logLevel);
        }

        private void SetLogLevel(VLSDK.LogLevel newLogLevel)
        {
            this.logLevel = newLogLevel;
#if UNITY_EDITOR
            Undo.RecordObject(
                TrackingManager.Instance,
                "Set Log Level in TrackingManager to " + this.logLevel);
#endif
            TrackingManager.Instance.logLevel = newLogLevel;
            NotificationHelper.logLevel = newLogLevel;
            LogHelper.logLevel = newLogLevel;
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            var issues = new List<SetupIssue>();
            if (TrackingManager.Instance.logLevel != this.logLevel)
            {
                issues.Add(
                    new SetupIssue(
                        "Log Level settings mismatch",
                        "Different log levels set in 'GeneralSettings' and 'TrackingManager'. " +
                        "The setting in the 'TrackingManager' will be overwritten with \"" +
                        this.logLevel + "\".",
                        SetupIssue.IssueType.Info,
                        this.gameObject,
                        new ISetupIssueSolution[]
                        {
                            new ReversibleAction(
                                ApplyLogLevel,
                                TrackingManager.Instance,
                                "Set Log Level in TrackingManager to " + this.logLevel),
                            new ReversibleAction(
                                () => SetLogLevel(TrackingManager.Instance.logLevel),
                                this,
                                "Set Log Level in GeneralSettings to " +
                                TrackingManager.Instance.logLevel)
                        }));
            }
            return issues;
        }
#endif
    }
}
