using System;
using UnityEngine;
using UnityEngine.UI;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    /// The <see cref="RuntimeParameterToggle"/> implements all the behaviour needed to toggle the
    /// value of a <see cref="RuntimeParameter"/> using a button.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///     The <see cref="RuntimeParameterToggle"/> is placed on a GameObject next to a button
    ///     component. It requires a text component in the GameObject'S children.
    ///  </para>
    ///  <para>
    ///     The prefab "VLShowLineModelButton" contains an example implementation of a button
    ///     that switches the line model in the game view on and off using a
    ///     <see cref="RuntimeParameterToggle"/>.
    ///  </para>
    /// </remarks>
    /// \deprecated Instead of using the Runtime parameter, use the corresponding parameters in the TrackingAnchor.
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "runtime_parameter_toggle.html")]
    [Obsolete("Instead of using the Runtime parameter, use the corresponding parameters in the TrackingAnchor.")]
    [RequireComponent(typeof(Button))]
    public class RuntimeParameterToggle : MonoBehaviour
    {
        /// <summary>
        ///  The target <see cref="RuntimeParameter"/> to be toggled. Only "Bool" type
        ///  <see cref="RuntimeParameter"/>s are supported.
        /// </summary>
        [Tooltip("The target RuntimeParameter to be toggled. Supports only \"Bool\" parameters.")]
        [SerializeField]
        private RuntimeParameter runtimeParameter = null;

        /// <summary>
        ///  The text to be displayed on the button while the target <see cref="RuntimeParameter"/>
        ///  is set to true.
        /// </summary>
        [Tooltip("Text shown on the target button when target RuntimeParameter is set to true.")]
        [SerializeField]
        private string buttonTextIfEnabled = "";

        /// <summary>
        ///  The text to be displayed on the button while the target <see cref="RuntimeParameter"/>
        ///  is set to false.
        /// </summary>
        [Tooltip("Text shown on the target button when target RuntimeParameter is set to false.")]
        [SerializeField]
        private string buttonTextIfDisabled = "";

        private bool previousValue = false;

        private Text linkedButtonText = null;
        private Text LinkedButtonText
        {
            get
            {
                if (!this.linkedButtonText)
                {
                    this.linkedButtonText = this.LinkedButton.GetComponentInChildren<Text>();
                }
                return this.linkedButtonText;
            }
        }

        private Button linkedButton = null;
        private Button LinkedButton
        {
            get
            {
                if (!this.linkedButton)
                {
                    this.linkedButton = this.gameObject.GetComponent<Button>();
                }
                return this.linkedButton;
            }
        }

        private void OnValidate()
        {
            if (!this.LinkedButtonText)
            {
                InstantiateButtonText();
            }
        }

        private void Awake()
        {
            InitializeRuntimeParameter();
            DisableButton();
        }

        private void Start()
        {
            if (TrackingManager.DoesTrackerExistAndIsInitialized())
            {
                EnableButton();
            }
            else
            {
                TrackingManager.OnTrackerInitialized += EnableButton;
            }
        }

        private void OnDisable()
        {
            if (this.runtimeParameter != null)
            {
                this.runtimeParameter.boolValueChangedEvent.RemoveListener(
                    UpdateCurrentActualParameterValue);
            }
        }

        public void ToggleValue()
        {
            SetParameterValue(!this.previousValue);
        }

        private void InitializeRuntimeParameter()
        {
            this.runtimeParameter.changing = true;

            this.runtimeParameter.boolValueChangedEvent.AddListener(
                UpdateCurrentActualParameterValue);
        }
        
        private void SetParameterValue(bool value)
        {
            this.runtimeParameter.SetValue(value);
            this.previousValue = value;
            SetButtonAppearance(value);
        }

        private void InstantiateButtonText()
        {
            var newTextPrefabInstance = (GameObject) Instantiate(
                Resources.Load("VLRuntimeParamToggleText"),
                this.transform);
            newTextPrefabInstance.name = "ButtonText";
            this.linkedButtonText = newTextPrefabInstance.GetComponent<Text>();
        }

        private void SetButtonAppearance(bool runTimeParameterValue)
        {
            this.linkedButtonText.text = runTimeParameterValue
                ? this.buttonTextIfEnabled
                : this.buttonTextIfDisabled;
        }   

        private void UpdateCurrentActualParameterValue(bool broadcastValue)
        {
            SetButtonAppearance(broadcastValue);
            this.previousValue = broadcastValue;
        }

        private void DisableButton()
        {
            this.linkedButton.interactable = false;
        }

        private void EnableButton()
        {
            this.linkedButton.interactable = true;
        }
    }
}
