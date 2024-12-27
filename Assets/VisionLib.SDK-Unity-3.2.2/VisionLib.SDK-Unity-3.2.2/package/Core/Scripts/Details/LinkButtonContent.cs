using System;
using UnityEngine;
using UnityEditor;

namespace Visometry.VisionLib.SDK.Core.Details
{

        [Serializable]
        public struct LinkButtonContent
        {
            [SerializeField]
            public string linkButtonLabel;
            [SerializeField]
            public string linkButtonTooltip;
            [SerializeField]
            public string linkURL;
        }
}