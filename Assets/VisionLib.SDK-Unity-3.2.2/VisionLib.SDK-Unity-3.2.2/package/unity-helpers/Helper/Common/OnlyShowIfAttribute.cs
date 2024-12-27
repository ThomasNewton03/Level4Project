using System;
using UnityEngine;

namespace Visometry.Helpers
{
    /// <summary>
    /// Add the [OnlyShowIf("fieldToCheck")] Attribute above a public parameter
    /// to only show the parameter in the Inspector, if the bool value of "fieldToCheck" is
    /// true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OnlyShowIfAttribute : PropertyAttribute
    {
        public readonly string fieldToCheck;
        public readonly bool showOnValue;

        public OnlyShowIfAttribute(string fieldToCheck, bool showOnValue)
        {
            this.fieldToCheck = fieldToCheck;
            this.showOnValue = showOnValue;
        }
    }
}
