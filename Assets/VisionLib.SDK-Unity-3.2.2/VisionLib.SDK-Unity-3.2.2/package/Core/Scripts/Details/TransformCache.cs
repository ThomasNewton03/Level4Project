using Visometry.VisionLib.SDK.Core.API;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public class TransformCache
    {
        private ModelTransform cachedTransform;
        private bool valid;

        private bool EqualsCachedTransform(ModelTransform trans)
        {
            return this.valid && this.cachedTransform.r == trans.r &&
                   this.cachedTransform.s == trans.s && this.cachedTransform.t == trans.t;
        }

        /// <summary>
        ///     Update the cached transform to the provided <see cref="ModelTransform"/> if
        ///     it is not already up to date.
        /// </summary>
        /// <return>
        ///     'true' if the new transform differs from the cached transform (i.e. if the
        ///     transform was indeed updated);
        ///     'false' if the new transform and the cached transform are the same.
        /// </return>
        public bool UpdateTransform(ModelTransform newTransform)
        {
            if (EqualsCachedTransform(newTransform))
            {
                return false;
            }
            this.cachedTransform = newTransform;
            this.valid = true;
            return true;
        }

        public bool IsValid()
        {
            return this.valid;
        }

        public void Invalidate()
        {
            this.valid = false;
        }
    }
}
