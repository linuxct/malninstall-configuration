using System;
using space.linuxct.malninstall.Configuration.Common.Enums;

namespace space.linuxct.malninstall.Configuration.ViewModels.PackageCreator
{
    [Serializable]
    public class GeneratePackageResponse
    {
        public PackageServeStatus GenerationStatusCode { get; set; }
        public string DownloadUrl { get; set; }
    }
}