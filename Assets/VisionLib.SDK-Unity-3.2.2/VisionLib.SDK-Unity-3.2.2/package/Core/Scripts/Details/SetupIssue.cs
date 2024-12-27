#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Visometry.VisionLib.SDK.Core.Details
{
    /// <summary>
    /// Common representation for errors in the scene setup. The <see cref="SceneValidator"/> aggregates <see cref="SetupIssue"/>s.
    /// A <see cref="SetupIssue"/> may include a delegate for a method that corrects the underlying setup error.
    /// </summary>
    public class SetupIssue
    {
        public enum IssueType
        {
            Info,
            Warning,
            Error
        }

        public readonly string title;
        public readonly string message;
        public readonly IssueType issueType;
        public readonly GameObject sourceObject;
        public readonly ISetupIssueSolution[] solveErrorFunctions = null;
        /// <summary>
        /// All issues with this identifier will finally be merged into one issue.
        /// Therefore only use the identifier if all issues created with this identifier do the
        /// same. 
        /// </summary>
        public readonly string uniqueIdentifier = null;

        public SetupIssue(
            string title,
            string message,
            IssueType issueType,
            GameObject sourceObject,
            ISetupIssueSolution solveErrorFunction = null,
            string uniqueIdentifier = null)
        {
            this.title = title;
            this.message = message;
            this.issueType = issueType;
            this.sourceObject = sourceObject;
            if (solveErrorFunction != null)
            {
                this.solveErrorFunctions = new ISetupIssueSolution[] {solveErrorFunction};
            }
            this.uniqueIdentifier = uniqueIdentifier;
        }

        public SetupIssue(
            string title,
            string message,
            IssueType issueType,
            GameObject sourceObject,
            ISetupIssueSolution[] solveErrorFunctions,
            string uniqueIdentifier = null)
        {
            this.title = title;
            this.message = message;
            this.issueType = issueType;
            this.sourceObject = sourceObject;
            this.solveErrorFunctions = solveErrorFunctions;
            this.uniqueIdentifier = uniqueIdentifier;
        }

        public static List<SetupIssue> NoIssues()
        {
            return new List<SetupIssue>();
        }
        
        public void Log(IssueType? overrideIssueType = null)
        {
            var log = GetLogDelegate(overrideIssueType ?? this.issueType);
            log("[VisionLib Setup Issue] " + this.message, this.sourceObject);
        }

        private static Action<string, Object> GetLogDelegate(IssueType issueType)
        {
            return issueType switch
            {
                IssueType.Info => LogHelper.LogInfo,
                IssueType.Warning => LogHelper.LogWarning,
                IssueType.Error => LogHelper.LogError,
                _ => throw new ArgumentOutOfRangeException(
                    "Unkoen IssueType: " + issueType.ToString())
            };
        }

        public void Notify(IssueType? overrideIssueType = null)
        {
            var notificationMessage = $"Setup Issue: {this.title}";
            var sendNotification = GetNotificationDelegate(overrideIssueType ?? this.issueType);

            sendNotification(
                notificationMessage,
                notificationMessage + "\n" + this.message,
                this.sourceObject);
        }

        private static Action<string, string, Object> GetNotificationDelegate(IssueType issueType)
        {
            return issueType switch
            {
                IssueType.Info => NotificationHelper.SendInfo,
                IssueType.Warning => NotificationHelper.SendWarning,
                IssueType.Error => NotificationHelper.SendError,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public class EqualityComparer : IEqualityComparer<SetupIssue>
        {
            public bool Equals(SetupIssue x, SetupIssue y)
            {
                return (x == null && y == null) || (x != null && y != null &&
                                                    x.uniqueIdentifier != null &&
                                                    y.uniqueIdentifier != null &&
                                                    x.uniqueIdentifier == y.uniqueIdentifier);
            }

            public int GetHashCode(SetupIssue obj)
            {
                if (obj.uniqueIdentifier != null)
                {
                    return HashCode.Combine(obj.uniqueIdentifier);
                }

                var hashCode = new HashCode();
                hashCode.Add(obj.title);
                hashCode.Add(obj.message);
                hashCode.Add(obj.issueType);
                hashCode.Add(obj.sourceObject);
                if (obj.solveErrorFunctions != null)
                {
                    foreach (var solution in obj.solveErrorFunctions)
                    {
                        hashCode.Add(solution);
                    }
                }
                return hashCode.ToHashCode();
            }
        }
    }
}

#endif
