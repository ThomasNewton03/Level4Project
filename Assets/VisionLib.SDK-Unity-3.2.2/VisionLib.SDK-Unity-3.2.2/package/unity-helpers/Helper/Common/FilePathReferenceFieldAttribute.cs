using System;
using UnityEngine;

namespace Visometry.Helpers
{
    /// <summary>
    /// Add the [FilePathReferenceField("MyLabel", ".myExtension", false)] Attribute
    /// above a public FilePathReference to draw its custom appearance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FilePathReferenceFieldAttribute : PropertyAttribute
    {
        public enum AllowProjectDir
        {
            Yes,
            No
        }

        public enum Mandatory
        {
            Yes,
            No
        }

        public readonly string displayLabel;
        public readonly string fileEnding;
        public readonly bool mandatory;
        public readonly bool allowProjectDir;

        public FilePathReferenceFieldAttribute(
            string displayLabel,
            string fileEnding,
            Mandatory mandatory,
            AllowProjectDir allowProjectDir)
        {
            this.displayLabel = displayLabel;
            this.fileEnding = fileEnding;
            this.mandatory = (mandatory == Mandatory.Yes);
            this.allowProjectDir = (allowProjectDir == AllowProjectDir.Yes);
        }
    }
}
