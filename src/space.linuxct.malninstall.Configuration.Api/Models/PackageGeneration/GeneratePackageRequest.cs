using System;
using space.linuxct.malninstall.Configuration.Enums;

namespace space.linuxct.malninstall.Configuration.Models.PackageGeneration
{
    [Serializable]
    public class GeneratePackageRequest
    {
        public EntryChannelEnum EntryChannel { get; set; }
        public string SafetyNetJwt { get; set; }
        public string HcaptchaClientResponse { get; set; }
        public string PackageName { get; set; }
    }
}