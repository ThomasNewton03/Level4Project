using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class EventSetupIssueHelper
    {
#if UNITY_EDITOR
        public static List<SetupIssue> CheckEventsForBrokenReferences(
            IEnumerable<UnityEventBase> events,
            GameObject sourceObject)
        {
            return events
                .Select(eventToTest => CheckEventForBrokenReferences(eventToTest, sourceObject))
                .Where(issue => issue != null).ToList();
        }

        private static SetupIssue CheckEventForBrokenReferences(
            UnityEventBase eventToTest,
            GameObject sourceObject)
        {
            if (HasBrokenListener(eventToTest))
            {
                return new SetupIssue(
                    $"Event in {sourceObject} has broken/null reference",
                    "This might indicate that you have removed a component which was still in use.",
                    SetupIssue.IssueType.Info,
                    sourceObject,
                    new ReversibleAction(
                        () => { RemoveBrokenListeners(eventToTest); },
                        sourceObject,
                        "Remove broken listener."));
            }
            return null;
        }

        private static void RemoveBrokenListeners(UnityEventBase eventToClear)
        {
            for (var i = 0; i < eventToClear.GetPersistentEventCount(); i++)
            {
                if (eventToClear.GetPersistentTarget(i) == null)
                {
                    UnityEventTools.RemovePersistentListener(eventToClear, i);
                }
            }
        }

        private static bool HasBrokenListener(UnityEventBase eventToTest)
        {
            for (var i = 0; i < eventToTest.GetPersistentEventCount(); i++)
            {
                if (eventToTest.GetPersistentTarget(i) == null)
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }
}
