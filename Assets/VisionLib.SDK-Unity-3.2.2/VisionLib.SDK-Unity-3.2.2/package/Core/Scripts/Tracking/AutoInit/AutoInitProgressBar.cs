using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visometry.VisionLib.SDK.Core.API;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  The AutoInit Progress Bar displays the current state of the AutoInit learning.
    ///  **THIS IS SUBJECT TO CHANGE** Do not rely on this code in productive environments.
    /// </summary>
    /// @ingroup WorkSpace
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "auto_init_progress_bar.html")]
    [AddComponentMenu("VisionLib/Core/AutoInit/AutoInit Progress Bar")]
    public class AutoInitProgressBar : MonoBehaviour, ISceneValidationCheck
    {
        private ProgressIndication progressBar;
        private const float progressBarMinValue = 0.0f;
        private const float progressBarMaxValue = 1.0f;

        private AutoInitSetupState previousSetupState = AutoInitSetupState.INACTIVE;

        private enum AutoInitSetupState
        {
            INACTIVE,
            PREPARING,
            READY
        }

        private void OnEnable()
        {
            TrackingManager.OnTrackingStates += UpdateAutoInitSetupProgress;
            TrackingManager.OnTrackerStopped += ClearProgressState;
        }

        private void OnDisable()
        {
            TrackingManager.OnTrackingStates -= UpdateAutoInitSetupProgress;
            TrackingManager.OnTrackerStopped -= ClearProgressState;
        }

        /// <summary>
        /// Communicate AutoInit progress via GUI.
        /// </summary>
        /// <param name="state">Current tracking state</param>
        private void UpdateAutoInitSetupProgress(TrackingState state)
        {
            var setupState = AggregateAutoInitSetupStates(state.objects);

            if (setupState == AutoInitSetupState.PREPARING)
            {
                InitializeProgressBarIfNull();
                UpdateProgressBar(state);
            }
            if (setupState == this.previousSetupState)
            {
                return;
            }

            switch (setupState)
            {
                case AutoInitSetupState.READY:
                    InitializeProgressBarIfNull();
                    this.progressBar.Finish(
                        this.previousSetupState == AutoInitSetupState.PREPARING
                            ? "Completed AutoInit Learning"
                            : "AutoInit Ready");
                    this.progressBar = null;
                    break;
                case AutoInitSetupState.INACTIVE:
                    InitializeProgressBarIfNull();
                    this.progressBar.Abort(
                        this.previousSetupState == AutoInitSetupState.PREPARING
                            ? "Canceled AutoInit Learning"
                            : "AutoInit Disabled");
                    this.progressBar = null;
                    break;
            }

            this.previousSetupState = setupState;
        }

        private void ClearProgressState()
        {
            this.previousSetupState = AutoInitSetupState.INACTIVE;
            if (this.progressBar == null)
            {
                return;
            }
            this.progressBar.Abort("AutoInit Preparation Canceled: Tracker Stopped");
            this.progressBar = null;
        }

        private void InitializeProgressBarIfNull()
        {
            this.progressBar ??= new ProgressIndication(
                "Preparing AutoInit",
                AutoInitProgressBar.progressBarMinValue,
                AutoInitProgressBar.progressBarMaxValue,
                "AutoInitSetupProgress");
        }

        private void UpdateProgressBar(TrackingState state)
        {
            var progress = 0.0f;
            var numberOfObjects = 0;
            foreach (var obj in state.objects)
            {
                progress += obj._AutoInitSetupProgress;
                numberOfObjects++;
            }
            this.progressBar.Value = numberOfObjects > 0 ? progress / numberOfObjects : 1.0f;
        }

        private static AutoInitSetupState AggregateAutoInitSetupStates(
            IEnumerable<TrackingState.Anchor> trackingObjects)
        {
            var anyPreparing = false;
            var allInactive = true;

            foreach (var obj in trackingObjects)
            {
                var setupState = obj._AutoInitSetupState;

                anyPreparing |= setupState == AutoInitSetupState.PREPARING.ToString();
                allInactive &= (setupState == AutoInitSetupState.INACTIVE.ToString() ||
                                setupState == null);
            }

            return anyPreparing
                ? AutoInitSetupState.PREPARING
                : (allInactive ? AutoInitSetupState.INACTIVE : AutoInitSetupState.READY);
        }

#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            var issues = new List<SetupIssue>();

            if (!FindObjectsOfType<TrackingAnchor>().SelectMany(anchor => anchor.workSpaces).Any())
            {
                issues.Add(
                    new SetupIssue(
                        "AutoInit progress bar in a scene without any WorkSpaces",
                        "AutoInit is disabled if there aren't any WorkSpaces in " +
                        "the scene. Add WorkSpaces to set up AutoInit or remove the" +
                        " AutoInit progress bar component.",
                        SetupIssue.IssueType.Warning,
                        this.gameObject,
                        new DestroyComponentAction(this)));
            }
            return issues;
        }
#endif
    }
}
