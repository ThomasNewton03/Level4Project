using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Visometry.VisionLib.SDK.Core;
using Visometry.VisionLib.SDK.Core.Details;

namespace Visometry.VisionLib.SDK.HoloLens
{
    /// <summary>
    ///  Turns Input.GetKeyDown into a UnityEvent.
    /// </summary>
    /// <remarks>
    ///  This behaviour can be added to the same GameObject multiple times to
    ///  process different key codes.
    /// </remarks>
    /// @ingroup HoloLens
    [HelpURL(DocumentationLink.APIReferenceURI.HoloLens + "keyboard_input.html")]
    [AddComponentMenu("VisionLib/HoloLens/Keyboard Input")]
    public class KeyboardInput : MonoBehaviour, ISceneValidationCheck
    {
        [Serializable]
        public class OnKeyDownEvent : UnityEvent {}

        public KeyCode keyCode;

        /// <summary>
        ///  Event fired whenever Input.GetKeyDown(keyCode) returns true.
        /// </summary>
        [SerializeField]
        public OnKeyDownEvent keyDownEvent = new OnKeyDownEvent();

        private void Update()
        {
            if (Input.GetKeyDown(this.keyCode))
            {
                this.keyDownEvent.Invoke();
            }
        }
        
#if UNITY_EDITOR
        public List<SetupIssue> GetSceneIssues()
        {
            return EventSetupIssueHelper.CheckEventsForBrokenReferences(
                new[] {this.keyDownEvent},
                this.gameObject);
        }
#endif
    }
}
