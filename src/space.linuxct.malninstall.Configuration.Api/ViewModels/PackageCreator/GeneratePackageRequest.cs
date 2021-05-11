using System;
using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.ViewModels.PackageCreator
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