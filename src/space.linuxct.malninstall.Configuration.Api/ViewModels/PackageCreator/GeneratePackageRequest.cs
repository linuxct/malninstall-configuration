using System;
using space.linuxct.malninstall.Configuration.Common.Enums;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;

namespace space.linuxct.malninstall.Configuration.ViewModels.PackageCreator
{
    [Serializable]
    public class GeneratePackageRequest
    {
        public EntryChannelEnum EntryChannel { get; set; }
        public string SafetyNetJwt { get; set; }
        public string HcaptchaClientResponse { get; set; }
        public string PackageName { get; set; }

        public PackageGenerationInput Flatten()
        {
            return new()
            {
                EntryChannel = EntryChannel,
                PackageName = PackageName,
                Token = EntryChannel == EntryChannelEnum.Application ? SafetyNetJwt : HcaptchaClientResponse
            };
        }
    }
}