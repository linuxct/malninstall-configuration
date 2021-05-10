using System;
using space.linuxct.malninstall.Configuration.Enums;

namespace space.linuxct.malninstall.Configuration.Models.PackageGeneration
{
    [Serializable]
    public class GeneratePackageResponse
    {
        public PackageServeStatus GenerationStatusCode { get; set; }
        public string DownloadUrl { get; set; }
    }
}