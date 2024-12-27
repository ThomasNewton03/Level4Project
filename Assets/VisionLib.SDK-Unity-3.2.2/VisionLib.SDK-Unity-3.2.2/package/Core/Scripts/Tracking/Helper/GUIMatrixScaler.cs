using System;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Scales <c>GUI.matrix</c> to a reference size.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   On hight-DPI devices the UI drawn inside the OnGUI function is
    ///   incredible small. This class can be used to scale the UI to a reference
    ///   size.
    ///  </para>
    /// </remarks>
    /// @ingroup Core
    internal class GUIMatrixScaler
    {
        /// <summary>
        ///  Stores the original GUI matrix.
        /// </summary>
        private Matrix4x4 originalMatrix = Matrix4x4.identity;

        /// <summary>
        ///  Computed GUI Scale matrix.
        /// </summary>
        private Matrix4x4 matrix = Matrix4x4.identity;

        /// <summary>
        ///  Contains the complete size of the reference screen.
        /// </summary>
        /// <remarks>
        ///  Use this to position the content relative to some point.
        /// </remarks>
        private Rect scaledScreenRect = new Rect(0.0f, 0.0f, 640.0f, 480.0f);

        /// <summary>
        ///  Width for which OnGUI is programmed.
        /// </summary>
        private int referenceScreenWidth = 640;

        /// <summary>
        ///  Height for which OnGUI is programmed.
        /// </summary>
        private int referenceScreenHeight = 480;

        /// <summary>
        ///  Constructor of the GUIMatrixScaler class.
        /// </summary>
        /// <remarks>
        ///  NOTICE: The current implementation only works for
        ///  <c>referenceScreenWidth >= referenceScreenHeight</c>.
        /// </remarks>
        /// <param name="referenceScreenWidth">
        ///  Width for which OnGUI is programmed.
        /// </param>
        /// <param name="referenceScreenHeight">
        ///  Height for which OnGUI is programmed.
        /// </param>
        public GUIMatrixScaler(int referenceScreenWidth, int referenceScreenHeight)
        {
            this.referenceScreenWidth = referenceScreenWidth;
            this.referenceScreenHeight = referenceScreenHeight;
        }

        /// <summary>
        ///  Updates the internal scale matrix and the scaled screen rectangle.
        /// </summary>
        public void Update()
        {
            float screenAspectRatio = (float) Screen.width / Screen.height;
            if (Screen.width < Screen.height)
            {
                float scale = (float) Screen.width / this.referenceScreenWidth;
                this.matrix = Matrix4x4.TRS(
                    Vector3.zero,
                    Quaternion.identity,
                    new Vector3(scale, scale, 1));
                this.scaledScreenRect.Set(
                    0.0f,
                    0.0f,
                    this.referenceScreenWidth,
                    this.referenceScreenWidth / screenAspectRatio);
            }
            else
            {
                float scale = (float) Screen.height / this.referenceScreenHeight;
                this.matrix = Matrix4x4.TRS(
                    Vector3.zero,
                    Quaternion.identity,
                    new Vector3(scale, scale, 1));
                this.scaledScreenRect.Set(
                    0.0f,
                    0.0f,
                    this.referenceScreenHeight * screenAspectRatio,
                    this.referenceScreenHeight);
            }
        }

        public Rect GetScaledScreenRect()
        {
            return this.scaledScreenRect;
        }

        public void Set()
        {
            GUI.matrix = this.matrix;
        }

        public void Unset()
        {
            GUI.matrix = this.originalMatrix;
        }
    }
}
