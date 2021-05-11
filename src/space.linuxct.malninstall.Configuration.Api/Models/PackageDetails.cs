using System;
using space.linuxct.malninstall.Configuration.Enums;

namespace space.linuxct.malninstall.Configuration.Models
{
    [Serializable]
    public class PackageDetails
    {
        public PackageCreationStatus Status { get; set; }
        public string FileLocation { get; set; }
    }
}