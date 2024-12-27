using System.Collections.Generic;
using UnityEngine;
using Visometry.DesignSystem;
using Visometry.VisionLib.SDK.Core;

namespace Visometry.VisionLib.SDK.UI
{
    public static class ProgressBarAdapter
    {
        public static readonly Dictionary<ProgressIndication, ProgressBarNotification>
            activeProgressBarNotifications = new();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            ProgressIndication.OnProgressBarInit += OnProgressBarInit;
            ProgressIndication.OnProgressBarFinish += OnProgressBarFinish;
            ProgressIndication.OnProgressBarAbort += OnProgressBarAbort;
            ProgressIndication.OnValueChanged += OnValueChanged;
        }

        private static void OnProgressBarInit(ProgressIndication progressBar, string inProgressText)
        {
            activeProgressBarNotifications.Add(
                progressBar,
                new ProgressBarNotification(
                    inProgressText,
                    progressBar.minValue,
                    progressBar.maxValue,
                    progressBar.category));
        }

        private static void OnProgressBarFinish(ProgressIndication progressBar, string message)
        {
            activeProgressBarNotifications[progressBar].Finish(message);
            activeProgressBarNotifications.Remove(progressBar);
        }

        private static void OnProgressBarAbort(ProgressIndication progressBar, string message)
        {
            activeProgressBarNotifications[progressBar].Abort(message);
            activeProgressBarNotifications.Remove(progressBar);
        }

        private static void OnValueChanged(ProgressIndication progressBar, float newValue)
        {
            activeProgressBarNotifications[progressBar].Value = newValue;
        }
    }
}
