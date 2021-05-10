using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using space.linuxct.malninstall.Configuration.Extensions;
using space.linuxct.malninstall.Configuration.Helpers.RateLimit;
using space.linuxct.malninstall.Configuration.Models;
using space.linuxct.malninstall.Configuration.Models.Persistence;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DownloadController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly RateLimitHelper _rateLimitHelper;
        public DownloadController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _rateLimitHelper = new RateLimitHelper(HttpContext, _distributedCache);
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(File), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status403Forbidden)] //Error TooManyCallsException
        [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status412PreconditionFailed)] //Error GuidNotFound
        public IActionResult GetTool(string guid)
        {
            //Find number of calls from this IP in Redis, block if needed
            if (_rateLimitHelper.IsClientRateLimited())
            {
                return new JsonResult(new BasicResponse { Message = "Too many calls, try again later." }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            var downloadContents = _distributedCache.GetObject<DownloadContentsModel>(guid);
            var connectionIdentifierHash = HttpContext.GetRemoteIPAddress().ToString().ToSha256();
            if (downloadContents == null || !System.IO.File.Exists(downloadContents.FilePath) || downloadContents.ConnectionIdentifierHash != connectionIdentifierHash)
            {
                return new JsonResult(new BasicResponse { Message = "The requested file was not found." }) { StatusCode = StatusCodes.Status412PreconditionFailed };
            }

            var downloadLocation = downloadContents.FilePath.Split("/").ToList();
            var serveName = downloadLocation.ElementAt(downloadLocation.Count - 1);
            downloadLocation.RemoveAt(downloadLocation.Count - 1);
            var downloadLocationPath = string.Join("/", downloadLocation);
            var serveNamePath = Path.Combine(downloadLocationPath, "ServeName");
            if (System.IO.File.Exists(serveNamePath))
            {
                serveName = System.IO.File.ReadAllText(serveNamePath);
            }
            
            return PhysicalFile(downloadContents.FilePath, "application/vnd.android.package-archive", serveName);
        }
    }
}