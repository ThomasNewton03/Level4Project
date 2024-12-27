namespace Visometry.VisionLib.SDK.Core
{
    /**
     *  @ingroup Core
     */
    public class ProgressIndication
    {
        public float minValue;
        public float maxValue;
        public string category;

        private float value;
        public float Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnValueChanged?.Invoke(this, this.value);
                }
            }
        }

        public delegate void ProgressBarAndTextDelegate(
            ProgressIndication progressBar,
            string text);

        public static event ProgressBarAndTextDelegate OnProgressBarInit;
        public static event ProgressBarAndTextDelegate OnProgressBarFinish;
        public static event ProgressBarAndTextDelegate OnProgressBarAbort;

        public delegate void ProgressBarAndValueDelegate(
            ProgressIndication progressBar,
            float newValue);

        public static event ProgressBarAndValueDelegate OnValueChanged;

        public ProgressIndication(
            string inProgressText,
            float minValue,
            float maxValue,
            string category)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.category = category;

            this.value = minValue;

            OnProgressBarInit?.Invoke(this, inProgressText);
        }

        public void Finish(string message)
        {
            OnProgressBarFinish?.Invoke(this, message);
        }

        public void Abort(string message)
        {
            OnProgressBarAbort?.Invoke(this, message);
        }
    }
}
