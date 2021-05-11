using System;
using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.Common.Models
{
    [Serializable]
    public class PackageDetails
    {
        public PackageCreationStatus Status { get; set; }
        public string FileLocation { get; set; }
    }
}