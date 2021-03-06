using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Common.Extensions;
using space.linuxct.malninstall.Configuration.Common.Models.Persistence;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic;

namespace space.linuxct.malninstall.Configuration.Core.Application.Services.ControllerLogic
{
    public class DownloadLogicService : IDownloadLogicService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<DownloadLogicService> _logger;
        
        public DownloadLogicService(IDistributedCache distributedCache, IHttpContextAccessor contextAccessor, ILogger<DownloadLogicService> logger)
        {
            _distributedCache = distributedCache;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
        public bool RequestIsValid(string guid)
        {
            var downloadContents = _distributedCache.GetObject<DownloadContentsModel>(guid);
            var connectionIdentifierHash = _contextAccessor.HttpContext.GetRemoteIPAddress().ToString().ToSha256();
            return downloadContents != null && System.IO.File.Exists(downloadContents.FilePath) && downloadContents.ConnectionIdentifierHash == connectionIdentifierHash;
        }

        public StoredFileContentDetails GetStoredFileDataForGuid(string guid)
        {
            var result = new StoredFileContentDetails();
            var downloadContents = _distributedCache.GetObject<DownloadContentsModel>(guid);
            result.FilePath = downloadContents.FilePath;
            var downloadLocation = downloadContents.FilePath.Split("/").ToList();
            result.ServeName = downloadLocation.ElementAt(downloadLocation.Count - 1);
            downloadLocation.RemoveAt(downloadLocation.Count - 1);
            var downloadLocationPath = string.Join("/", downloadLocation);
            var serveNamePath = Path.Combine(downloadLocationPath, "ServeName");
            if (File.Exists(serveNamePath))
            {
                result.ServeName = File.ReadAllText(serveNamePath);
            }

            return result;
        }
    }
}