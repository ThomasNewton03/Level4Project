using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Modified version of the original AspectRatioFitter, which also handles
    ///  rectTransforms with rotations around the z-axis.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The original code can be found here:
    ///   * https://bitbucket.org/Unity-Technologies/ui/src/a3f89d5f7d145e4b6fa11cf9f2de768fea2c500f/UnityEngine.UI/UI/Core/Layout/AspectRatioFitter.cs?at=2017.3
    ///   * https://bitbucket.org/Unity-Technologies/ui/src/a3f89d5f7d145e4b6fa11cf9f2de768fea2c500f/UnityEngine.UI/UI/Core/SetPropertyUtility.cs?at=2017.3
    ///  </para>
    ///  <para>
    ///   The original code was released under an MIT/X11 license:
    ///
    /// The MIT License (MIT)
    ///
    /// Copyright (c) 2014-2015, Unity Technologies
    ///
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    ///
    /// The above copyright notice and this permission notice shall be included in
    /// all copies or substantial portions of the Software.
    ///
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    /// THE SOFTWARE.
    ///  </para>
    /// </remarks>
    /// @ingroup Core
    [AddComponentMenu("VisionLib/Core/VL Aspect Ratio Fitter")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    internal class AspectRatioFitter : UIBehaviour, ILayoutSelfController
    {
        internal static class SetPropertyUtility
        {
            public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
            {
                if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                    return false;

                currentValue = newValue;
                return true;
            }
        }

        public enum AspectMode
        {
            None,
            WidthControlsHeight,
            HeightControlsWidth,
            FitInParent,
            EnvelopeParent
        }

        [SerializeField]
        private AspectMode m_AspectMode = AspectMode.None;
        public AspectMode aspectMode
        {
            get
            {
                return m_AspectMode;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_AspectMode, value))
                    SetDirty();
            }
        }

        [SerializeField]
        private float m_AspectRatio = 1;
        public float aspectRatio
        {
            get
            {
                return m_AspectRatio;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_AspectRatio, value))
                    SetDirty();
            }
        }

        [System.NonSerialized]
        private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected AspectRatioFitter() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty(true);
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        // NOTICE: OnRectTransformDimensionsChange doesn't get called, if the
        // rotation was changed. We therefore have to check the rotation manually.
        [System.NonSerialized]
        private Quaternion m_LastLocalRotation = Quaternion.identity;

        private void Update()
        {
            float angle = Quaternion.Angle(m_LastLocalRotation, rectTransform.localRotation);
            if (angle > 0.00001f)
            {
                UpdateRect();
                m_LastLocalRotation = rectTransform.localRotation;
            }
        }

        private void UpdateRect()
        {
            if (!IsActive())
                return;

            m_Tracker.Clear();

            switch (m_AspectMode)
            {
#if UNITY_EDITOR
                case AspectMode.None:
                {
                    if (!Application.isPlaying)
                        m_AspectRatio = Mathf.Clamp(
                            rectTransform.rect.width / rectTransform.rect.height,
                            0.001f,
                            1000f);

                    break;
                }
#endif
                case AspectMode.HeightControlsWidth:
                {
                    m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
                    rectTransform.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal,
                        rectTransform.rect.height * m_AspectRatio);
                    break;
                }
                case AspectMode.WidthControlsHeight:
                {
                    m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
                    rectTransform.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Vertical,
                        rectTransform.rect.width / m_AspectRatio);
                    break;
                }
                case AspectMode.FitInParent: // NOTICE: Case modified to include rotation
                {
                    m_Tracker.Add(
                        this,
                        rectTransform,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDeltaX |
                        DrivenTransformProperties.SizeDeltaY);

                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;

                    Vector2 parentSize = GetParentSize();
                    Rect bounds = new Rect(0, 0, parentSize.x, parentSize.y);
                    if (Mathf.RoundToInt(rectTransform.eulerAngles.z) % 180 == 90)
                    {
                        // Invert the bounds if GameObject is rotated
                        bounds.size = new Vector2(bounds.height, bounds.width);
                    }

                    Vector2 newSize = new Vector2(bounds.height * m_AspectRatio, bounds.height);
                    if (newSize.x > bounds.width)
                    {
                        newSize.x = bounds.width;
                        newSize.y = newSize.x / m_AspectRatio;
                    }
                    rectTransform.sizeDelta = GetSizeDeltaToProduceSize(newSize);

                    break;
                }
                case AspectMode.EnvelopeParent: // NOTICE: Case modified to include rotation
                {
                    m_Tracker.Add(
                        this,
                        rectTransform,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDeltaX |
                        DrivenTransformProperties.SizeDeltaY);

                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;

                    Vector2 parentSize = GetParentSize();
                    Rect bounds = new Rect(0, 0, parentSize.x, parentSize.y);
                    if (Mathf.RoundToInt(rectTransform.eulerAngles.z) % 180 == 90)
                    {
                        // Invert the bounds if GameObject is rotated
                        bounds.size = new Vector2(bounds.height, bounds.width);
                    }

                    Vector2 newSize = new Vector2(bounds.height * m_AspectRatio, bounds.height);
                    if (newSize.x < bounds.width)
                    {
                        newSize.x = bounds.width;
                        newSize.y = newSize.x / m_AspectRatio;
                    }
                    rectTransform.sizeDelta = GetSizeDeltaToProduceSize(newSize);

                    break;
                }
            }
        }

        // NOTICE: Return Vector2 instead of float, because we always compute the
        // complete size.
        private Vector2 GetSizeDeltaToProduceSize(Vector2 size)
        {
            return size - Vector2.Scale(
                GetParentSize(),
                rectTransform.anchorMax - rectTransform.anchorMin);
        }

        private Vector2 GetParentSize()
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            if (!parent)
                return Vector2.zero;
            return parent.rect.size;
        }

        public virtual void SetLayoutHorizontal() {}

        public virtual void SetLayoutVertical() {}

        private IEnumerator DelayUpdate()
        {
            yield return null;
            UpdateRect();
        }

        protected void SetDirty(bool delayUpdate = false)
        {
            if (delayUpdate)
                StartCoroutine(DelayUpdate());
            else
                UpdateRect();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_AspectRatio = Mathf.Clamp(m_AspectRatio, 0.001f, 1000f);
            SetDirty();
        }

#endif
    }
}
