using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Visometry.DesignSystem
{
    /// <summary>
    /// MonoBehaviour that is used with the VLNotification.prefab
    /// and which is managing its UI components.
    /// Its appearance and behaviour is set during instantiation
    /// by its NotificationObject.
    /// </summary>
    [AddComponentMenu("VisionLib/Design System/Notification")]
    public class Notification : MonoBehaviour
    {
        public enum Kind
        {
            Error,
            Warning,
            Success,
            Info
        }

        public enum Type
        {
            Inline
        }

        public enum Position
        {
            Bottom,
            Ceiling
        }

        public bool isDisabled = false;

        [SerializeField]
        private Text title = null;
        [SerializeField]
        private Text caption = null;
        [SerializeField]
        private Text icon = null;

        [SerializeField]
        private Outline outline = null;
        [SerializeField]
        private Image border = null;
        [SerializeField]
        private Image background = null;

        [SerializeField]
        private Button actionButton = null;

        [SerializeField]
        private Button closeButton = null;

        private RectTransform rectTransform;
        private List<GameObject> children;

        private float originalYPosition;

        private const int timeBufferBeforeDestroy = 500;
        private bool isBeingDestroyed = false;
        private float spacing = 15.0f;

        /// <summary>
        ///  Delegate for event texts.
        /// </summary>
        public delegate void NotificationDelegate(Notification notification);

        /// <summary>
        ///  Event which will be emitted as soon as the notification
        ///  reached its time to be destroyed.
        /// </summary>
        public event NotificationDelegate OnWaitingForDestroyFinished;

        /// <summary>
        ///  Event which will be emitted after the notification has been deleted externally.
        /// </summary>
        public event NotificationDelegate OnDeleted;

        private void Awake()
        {
            this.actionButton.gameObject.SetActive(false);

            this.closeButton.onClick.AddListener(SendDestroyMessage);

            this.rectTransform = GetComponent<RectTransform>();
            this.children = new List<GameObject>();

            foreach (Transform child in transform)
            {
                this.children.Add(child.gameObject);
            }
        }

        public void SetPosition(Position position)
        {
            if (position == Position.Ceiling)
            {
                this.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                this.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                this.rectTransform.anchoredPosition = new Vector2(
                    0f,
                    -this.rectTransform.anchoredPosition.y);
                this.rectTransform.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                this.rectTransform.anchoredPosition = new Vector2(
                    0f,
                    this.rectTransform.anchoredPosition.y);
            }
            this.originalYPosition = this.rectTransform.anchoredPosition.y;
        }

        public void ApplyOffset(bool applyInPositiveDirection, int order)
        {
            if (this.rectTransform == null)
            {
                return;
            }

            float offset = order *
                           (LayoutUtility.GetPreferredHeight(this.rectTransform) + this.spacing);

            if (!applyInPositiveDirection)
            {
                offset *= -1;
            }

            offset += this.originalYPosition;
            this.rectTransform.anchoredPosition = new Vector2(
                this.rectTransform.anchoredPosition.x,
                offset);
        }

        public void SetTexts(string titleText, string captionText)
        {
            this.title.text = titleText;
            this.caption.text = captionText;
        }

        public void SetColor(Color color)
        {
            this.border.color = color;
            this.outline.effectColor = color;
            this.icon.color = color;
        }

        public void SetIcon(string icon)
        {
            this.icon.text = icon;
        }

        public void SetAction(System.Action action)
        {
            if (action != null)
            {
                this.actionButton.gameObject.SetActive(true);
                this.actionButton.onClick.AddListener(() => action.Invoke());
            }
        }

        public void SetVisibilityOfChildren(bool show)
        {
            this.background.enabled = false;
            this.isDisabled = !show;
            foreach (GameObject child in this.children)
            {
                child.SetActive(show);
            }
        }

        private void SendDestroyMessage()
        {
            TriggerDestroyAfterSeconds(0f);
        }

        public async void TriggerDestroyAfterSeconds(float seconds)
        {
            await Task.Delay((int) (seconds * 1000.0f));

            if (!this || this.isBeingDestroyed)
            {
                return;
            }
            this.isBeingDestroyed = true;
            SetVisibilityOfChildren(false);
            await Task.Delay(timeBufferBeforeDestroy);
            OnWaitingForDestroyFinished?.Invoke(this);
        }

        private void OnDestroy()
        {
            OnDeleted?.Invoke(this);
        }
    }
}
