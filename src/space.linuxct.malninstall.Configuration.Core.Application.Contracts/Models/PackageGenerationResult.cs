using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models
{
    public class PackageGenerationResult
    {
        public PackageServeStatus ServeStatus { get; set; }
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
    }
}