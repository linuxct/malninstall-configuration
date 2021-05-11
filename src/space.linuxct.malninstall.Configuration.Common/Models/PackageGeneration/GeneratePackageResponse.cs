using System;
using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.Common.Models.PackageGeneration
{
    [Serializable]
    public class GeneratePackageResponse
    {
        public PackageServeStatus GenerationStatusCode { get; set; }
        public string DownloadUrl { get; set; }
    }
}