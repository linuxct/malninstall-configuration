using System;
using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models
{
    [Serializable]
    public class PackageDetails
    {
        public PackageCreationStatus Status { get; set; }
        public string FileLocation { get; set; }
        public string ApplicationName { get; set; }
    }
}