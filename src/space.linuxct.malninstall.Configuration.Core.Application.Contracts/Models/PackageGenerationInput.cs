using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models
{
    public class PackageGenerationInput
    {
        public EntryChannelEnum EntryChannel { get; set; }
        public string PackageName { get; set; }
        public string Token { get; set; }
    }
}