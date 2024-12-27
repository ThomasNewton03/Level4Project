using UnityEngine;
using UnityEngine.UI;

namespace Visometry.DesignSystem
{
    /// <summary>
    /// ProgressBar that consists of a slider.
    /// </summary>
    [AddComponentMenu("VisionLib/Design System/Progress Bar")]
    public class LinearProgressBar : MonoBehaviour
    {
        public Slider slider;
        private const string progressBarName = "VLLinearProgressBar";

        public float Value
        {
            get
            {
                return this.slider.value;
            }

            set
            {
                this.slider.value = value;
            }
        }

        public static LinearProgressBar Instantiate()
        {
            return Instantiate(Resources.Load<LinearProgressBar>(progressBarName).gameObject)
                .GetComponent<LinearProgressBar>();
        }

        public void SetMinMaxValues(float minValue, float maxValue)
        {
            this.slider.minValue = minValue;
            this.slider.maxValue = maxValue;
        }
    }
}
