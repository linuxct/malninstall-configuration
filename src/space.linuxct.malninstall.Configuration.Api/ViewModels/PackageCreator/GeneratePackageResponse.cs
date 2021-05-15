using System;
using space.linuxct.malninstall.Configuration.Common.Enums;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;

namespace space.linuxct.malninstall.Configuration.ViewModels.PackageCreator
{
    [Serializable]
    public class GeneratePackageResponse
    {
        public PackageServeStatus GenerationStatusCode { get; set; }
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }

        public GeneratePackageResponse Unflatten(PackageGenerationResult result)
        {
            return new()
            {
                DownloadUrl = result.DownloadUrl,
                FileName = result.FileName,
                GenerationStatusCode = result.ServeStatus
            };
        }
    }
}