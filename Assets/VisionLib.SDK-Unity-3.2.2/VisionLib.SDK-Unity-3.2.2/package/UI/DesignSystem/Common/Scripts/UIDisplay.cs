using UnityEngine;

namespace Visometry.DesignSystem
{
    public abstract class UIDisplay : MonoBehaviour
    {
        private Canvas displayCanvas;
        public Canvas DisplayCanvas
        {
            get
            {
                if (this.displayCanvas == null)
                {
                    this.displayCanvas = GetComponentInParent<Canvas>();
                }
                return this.displayCanvas;
            }
        }
    }
}
