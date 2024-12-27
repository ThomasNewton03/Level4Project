using System;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  LicenseInformation stores information about the current license.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class LicenseInformation
    {
        public string type;
        public string name;
        public string customerName;
        public string customerContact;
        public string expirationDate;
        public bool valid;
        public bool watermark;

        public string GetLabel()
        {
            int index = this.name.IndexOf("License");
            return index > 0 ? this.name.Substring(0, index).ToUpper() + " LICENSE" : this.name;
        }
    }
}
