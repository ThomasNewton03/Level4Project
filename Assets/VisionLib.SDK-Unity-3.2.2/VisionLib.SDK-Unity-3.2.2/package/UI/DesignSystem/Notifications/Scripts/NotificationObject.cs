using UnityEngine;

namespace Visometry.DesignSystem
{
    /// <summary>
    /// Manages all parameters that are used as settings
    /// for a Notification.
    /// Can instantiate and initialize a notification
    /// depending on its kind and type and keeps a reference
    /// to the according GameObject.
    /// </summary>
    public class NotificationObject
    {
        public Notification notification;
        public Notification.Kind kind;
        public string contentCategory;
        public int priority;

        private Notification.Type type;
        private string title;
        private string caption;

        private System.Action onAction;
        private GameObject additionalContent;
        private bool dismissAutomatically;
        private int encounters = 1;

        private const float dismissAfterSeconds = 5f;
        private const string notificationPrefabName = "VLInlineNotification";

        private static Color errorColor = new Color32(0xda, 0x1e, 0x28, 0xFF);
        private static Color warningColor = new Color32(0xf1, 0xc2, 0x1b, 0xFF);
        private static Color successColor = new Color32(0x42, 0xbe, 0x65, 0xFF);
        private static Color infoColor = Color.grey;

        private const string errorIcon = "\uead4";
        private const string warningIcon = "\ue91d";
        private const string successIcon = "\ueb64";
        private const string infoIcon = "\ueb2b";

        public NotificationObject(
            string title,
            string caption,
            Notification.Kind kind,
            Notification.Type type,
            System.Action onActionButtonClick = null,
            string contentCategory = "",
            GameObject additionalContent = null,
            bool dismissAutomatically = true,
            int priority = 0)
        {
            this.title = title;
            this.caption = caption;
            this.kind = kind;
            this.type = type;
            this.onAction = onActionButtonClick;
            this.contentCategory = contentCategory;
            this.additionalContent = additionalContent;
            this.dismissAutomatically = dismissAutomatically;
            this.priority = priority;
        }

        public void Instantiate(GameObject parent)
        {
            this.notification = GameObject.Instantiate(
                Resources.Load<Notification>(notificationPrefabName).gameObject,
                parent.transform).GetComponent<Notification>();

            InitializeNotification();
        }

        private void InitializeNotification()
        {
            if (!this.notification.gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateNotificationTexts();
            this.notification.SetAction(this.onAction);

            switch (this.kind)
            {
                case Notification.Kind.Error:
                    this.notification.SetPosition(Notification.Position.Bottom);
                    this.notification.SetColor(errorColor);
                    this.notification.SetIcon(errorIcon);
                    break;
                case Notification.Kind.Warning:
                    this.notification.SetPosition(Notification.Position.Bottom);
                    this.notification.SetColor(warningColor);
                    this.notification.SetIcon(warningIcon);
                    break;
                case Notification.Kind.Success:
                    this.notification.SetPosition(Notification.Position.Ceiling);
                    this.notification.SetColor(successColor);
                    this.notification.SetIcon(successIcon);
                    break;
                case Notification.Kind.Info:
                    this.notification.SetPosition(Notification.Position.Ceiling);
                    this.notification.SetColor(infoColor);
                    this.notification.SetIcon(infoIcon);
                    break;
            }

            TriggerDestroyAfterSeconds();

            switch (this.type)
            {
                case Notification.Type.Inline:
                    // type dependent behaviour
                    break;
                default:
                    break;
            }

            if (this.additionalContent != null)
            {
                this.additionalContent.transform.SetParent(this.notification.transform, false);
            }
        }

        public bool IsEqualTo(NotificationObject other)
        {
            if (other == null)
            {
                return false;
            }

            return this.kind == other.kind && this.type == other.type &&
                   this.title == other.title && this.caption == other.caption &&
                   this.contentCategory == other.contentCategory && this.priority == other.priority;
        }

        public void IncreaseEncounters()
        {
            this.encounters++;
            if (this.notification)
            {
                UpdateNotificationTexts();
                TriggerDestroyAfterSeconds();
            }
        }

        private void UpdateNotificationTexts()
        {
            string postfix = "";
            if (this.encounters > 1)
            {
                postfix = " ( " + this.encounters + " )";
            }

            this.notification.SetTexts(this.title, this.caption + postfix);
        }

        private void TriggerDestroyAfterSeconds()
        {
            if (!this.dismissAutomatically)
            {
                return;
            }

            switch (this.kind)
            {
                case Notification.Kind.Error:
                    break;
                case Notification.Kind.Warning:
                    this.notification.TriggerDestroyAfterSeconds(dismissAfterSeconds);
                    break;
                case Notification.Kind.Success:
                    this.notification.TriggerDestroyAfterSeconds(dismissAfterSeconds);
                    break;
                case Notification.Kind.Info:
                    this.notification.TriggerDestroyAfterSeconds(dismissAfterSeconds);
                    break;
            }
        }
    }
}
