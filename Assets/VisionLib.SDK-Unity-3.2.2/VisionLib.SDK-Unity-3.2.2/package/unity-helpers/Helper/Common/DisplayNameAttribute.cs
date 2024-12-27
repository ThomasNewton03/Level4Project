using System;
using UnityEngine;

namespace Visometry.Helpers
{
    /// <summary>
    /// Add the [DisplayName("newName")] Attribute above a public parameter
    /// to make its Inspector label show 'newName' instead of the parameter name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayNameAttribute : PropertyAttribute
    {
        public readonly string displayName;

        public DisplayNameAttribute(string displayName)
        {
            this.displayName = displayName;
        }
    }
}
