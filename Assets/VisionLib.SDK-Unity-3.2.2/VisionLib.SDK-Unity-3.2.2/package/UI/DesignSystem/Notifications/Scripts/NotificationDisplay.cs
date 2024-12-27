using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visometry.DesignSystem
{
    /// <summary>
    /// The NotificationDisplay manages a list of Notifications
    /// that should be displayed depending on their kind after they
    /// have been added through the static NotificationManager.
    ///
    /// Add this component to a canvas or as its child to visualize
    /// notifications from the vlUnitySDK.
    /// </summary>
    [AddComponentMenu("VisionLib/Design System/Notification Display")]
    public class NotificationDisplay : UIDisplay
    {
        private List<NotificationObject> errorAndWarningNotificationsQueue =
            new List<NotificationObject>();
        private List<NotificationObject> activeInfoNotificationStack =
            new List<NotificationObject>();
        private const int maxInfoNotificationStackSize = 3;

        private NotificationObject activeErrorOrWarningNotification = null;

        private void OnEnable()
        {
            NotificationManager.OnNotificationAdded += AddNotification;
            NotificationManager.OnNotificationReset += ResetNotifications;
        }

        private void OnDisable()
        {
            NotificationManager.OnNotificationAdded -= AddNotification;
            NotificationManager.OnNotificationReset -= ResetNotifications;
        }

        /// <summary>
        /// Add a notification to the queue of notifications to be displayed.
        /// </summary>
        /// <remarks>
        /// Notifications of kind "Info" will be displayed immediately
        /// (destroying other active "Info" notifications) and disappear automatically,
        /// while those of kind "Error" or "Warning" will be displayed one after
        /// another and only disappear through user interaction.
        /// Notifications with priority 1 will be prioritized
        /// compared to notifications with the default priority 0.
        /// </remarks>
        public void AddNotification(NotificationObject newNotification)
        {
            if (newNotification.kind == Notification.Kind.Warning ||
                newNotification.kind == Notification.Kind.Error)
            {
                var identicalNotification = this.errorAndWarningNotificationsQueue.Find(
                    notification => notification.IsEqualTo(newNotification));
                if (identicalNotification != null)
                {
                    identicalNotification.IncreaseEncounters();
                    return;
                }

                this.errorAndWarningNotificationsQueue.Add(newNotification);

                if (this.activeErrorOrWarningNotification == null)
                {
                    InstantiateNextErrorWarningNotification();
                }
            }
            else if (newNotification.kind == Notification.Kind.Info ||
                     newNotification.kind == Notification.Kind.Success)
            {
                var identicalNotification = this.activeInfoNotificationStack.Find(
                    notification => notification.Equals(newNotification));
                if (identicalNotification != null)
                {
                    identicalNotification.IncreaseEncounters();
                    return;
                }

                this.activeInfoNotificationStack.Insert(
                    this.activeInfoNotificationStack.FindLastIndex(
                        notification => notification.priority == 0) + 1,
                    newNotification);

                InstantiateNextInfoNotification(newNotification);
            }
        }

        /// <summary>
        /// Finds next "Error" or "Warning" notification in the queue
        /// and instantiates it.
        /// </summary>
        /// <remarks>
        /// "Error" notifications will be prioritized
        /// </remarks>
        private void InstantiateNextErrorWarningNotification()
        {
            NotificationObject nextError = FindNextNotificationInList(
                this.errorAndWarningNotificationsQueue,
                Notification.Kind.Error);

            if (nextError != null)
            {
                SetUpNotification(nextError);
                this.activeErrorOrWarningNotification = nextError;
                return;
            }

            NotificationObject nextWarning = FindNextNotificationInList(
                this.errorAndWarningNotificationsQueue,
                Notification.Kind.Warning);

            if (nextWarning != null)
            {
                SetUpNotification(nextWarning);
                this.activeErrorOrWarningNotification = nextWarning;
            }
        }

        /// <summary>
        ///  Instantiates the next "Info" notification.
        /// </summary>
        /// <remarks>
        /// If there is currently an "Info" notification from the same contentCategory
        /// active, it will be destroyed.
        /// If there is currently an "Info" notification from another category active,
        /// it will be moved downwards.
        /// If the maxInfoNotificationStackSize is reached, the oldest "Info"
        /// notification will be destroyed.
        /// </remarks>
        private void InstantiateNextInfoNotification(NotificationObject nextNotificationObject)
        {
            SetUpNotification(nextNotificationObject);

            if (!String.IsNullOrEmpty(nextNotificationObject.contentCategory))
            {
                ReplaceNotificationFromSameCategory(
                    this.activeInfoNotificationStack,
                    nextNotificationObject);
            }

            if (this.activeInfoNotificationStack.Count > maxInfoNotificationStackSize)
            {
                NotificationObject oldestInfoNotification =
                    this.activeInfoNotificationStack.FirstOrDefault(
                        notification => notification.priority == 0);

                // Too many high priority notifications active.
                if (oldestInfoNotification == null)
                {
                    return;
                }

                this.activeInfoNotificationStack.Remove(oldestInfoNotification);
                DestroyNotificationImmediate(oldestInfoNotification);
            }

            if (this.activeInfoNotificationStack.Count > 1)
            {
                MoveNotificationsInStack(this.activeInfoNotificationStack, false);
            }
        }

        private static void ReplaceNotificationFromSameCategory(
            List<NotificationObject> notificationList,
            NotificationObject notificationObject)
        {
            NotificationObject notificationFromSameCategory = null;

            notificationFromSameCategory = notificationList.FirstOrDefault(
                notification => (notification != notificationObject &&
                                 notification.contentCategory ==
                                 notificationObject.contentCategory));

            if (notificationFromSameCategory != null)
            {
                notificationList.Remove(notificationObject);
                notificationList.Insert(
                    notificationList.IndexOf(notificationFromSameCategory),
                    notificationObject);
                notificationList.Remove(notificationFromSameCategory);
                DestroyNotificationImmediate(notificationFromSameCategory);
            }
        }

        /// <summary>
        /// Instantiates a Notification (below a "Notifications" parent
        /// in the hierarchy) according to the given NotificationObject
        /// and registers to its OnWaitingForDestroyFinished event.
        /// </summary>
        private void SetUpNotification(NotificationObject notificationObject)
        {
            notificationObject.Instantiate(this.gameObject);
            notificationObject.notification.OnWaitingForDestroyFinished += DismissNotification;
            notificationObject.notification.OnDeleted += HandleNotificationDeletion;
        }

        private void DismissNotification(Notification notification)
        {
            notification.OnWaitingForDestroyFinished -= DismissNotification;
            NotificationObject notificationObject = GetNotificationObject(notification);

            if (notificationObject != null)
            {
                DestroyNotificationImmediate(notificationObject);
            }
        }

        /// <summary>
        /// Destroys the given notification.
        /// </summary>
        private static void DestroyNotificationImmediate(NotificationObject notificationObject)
        {
            if (notificationObject.notification != null)
            {
                MonoBehaviour.Destroy(notificationObject.notification.gameObject);
            }
        }

        /// <summary>
        /// After a notification was destroyed, remove its references and if its
        /// kind was warning or error, the next available error or
        /// warning notification will be instantiated.
        /// </summary>
        /// <param name="notification"></param>
        private void HandleNotificationDeletion(Notification notification)
        {
            notification.OnDeleted -= HandleNotificationDeletion;
            NotificationObject notificationObject = GetNotificationObject(notification);

            if (notificationObject == null)
            {
                return;
            }
            if (this.activeInfoNotificationStack.Contains(notificationObject))
            {
                this.activeInfoNotificationStack.Remove(notificationObject);

                if (this.activeInfoNotificationStack.Count > 0)
                {
                    MoveNotificationsInStack(this.activeInfoNotificationStack, true);
                }
            }

            if (this.activeErrorOrWarningNotification == notificationObject)
            {
                this.errorAndWarningNotificationsQueue.Remove(notificationObject);
                this.activeErrorOrWarningNotification = null;
            }

            if (this.activeErrorOrWarningNotification == null &&
                this.activeInfoNotificationStack.Count == 0 &&
                this.errorAndWarningNotificationsQueue.Count == 0)
            {
                return;
            }

            if ((notificationObject.kind == Notification.Kind.Warning ||
                 notificationObject.kind == Notification.Kind.Error) &&
                this.errorAndWarningNotificationsQueue.Count > 0)
            {
                InstantiateNextErrorWarningNotification();
            }
        }

        /// <summary>
        /// Destroys all active notifications and clears the queue.
        /// </summary>
        public void ResetNotifications()
        {
            if (this.activeErrorOrWarningNotification != null)
            {
                if (this.activeErrorOrWarningNotification.notification != null)
                {
                    MonoBehaviour.Destroy(
                        this.activeErrorOrWarningNotification.notification.gameObject);
                }
                this.activeErrorOrWarningNotification = null;
            }
            if (this.activeInfoNotificationStack.Count > 0)
            {
                foreach (NotificationObject notificationObject in this.activeInfoNotificationStack)
                {
                    if (notificationObject.notification != null)
                    {
                        MonoBehaviour.Destroy(notificationObject.notification.gameObject);
                    }
                }
                this.activeInfoNotificationStack.Clear();
            }

            this.errorAndWarningNotificationsQueue.Clear();
        }

        private void OnApplicationQuit()
        {
            ResetNotifications();
        }

        private static void MoveNotificationsInStack(
            List<NotificationObject> notificationStack,
            bool moveInPositiveDirection)
        {
            for (int i = 0; i < notificationStack.Count; i++)
            {
                notificationStack[i].notification.ApplyOffset(
                    moveInPositiveDirection,
                    (notificationStack.Count - 1) - i);
            }
        }

        private static NotificationObject FindNextNotificationInList(
            List<NotificationObject> sourceList,
            Notification.Kind kind)
        {
            NotificationObject nextNotification = null;

            List<NotificationObject> notificationsOfKind =
                sourceList.Where(notification => notification.kind == kind).ToList();

            nextNotification =
                notificationsOfKind.FirstOrDefault(notification => notification.priority > 0);

            if (nextNotification == null)
            {
                nextNotification = sourceList.First();
            }

            return nextNotification;
        }

        /// <summary>
        /// Returns the NotificationObject from the list of notifications
        /// according to the given Notification.
        /// </summary>
        private NotificationObject GetNotificationObject(Notification notification)
        {
            Predicate<NotificationObject> matchesNotification = notificationObject =>
                notification == notificationObject.notification;
            var match = this.errorAndWarningNotificationsQueue.Find(matchesNotification);
            if (match != null)
            {
                return match;
            }
            return this.activeInfoNotificationStack.Find(matchesNotification);
        }
    }
}
