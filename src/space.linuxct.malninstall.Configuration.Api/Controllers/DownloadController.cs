using System.IO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic;
using space.linuxct.malninstall.Configuration.ViewModels.Common;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadLogicService _downloadService;
        private readonly ILogger<DownloadController> _logger;
        public DownloadController(IDownloadLogicService downloadService, ILogger<DownloadController> logger)
        {
            _downloadService = downloadService;
            _logger = logger;
        }
        
        [HttpGet]
        [EnableCors("FrontEndPolicy")]
        [ProducesResponseType(typeof(File), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status412PreconditionFailed)] //Error GuidNotFound
        public IActionResult GetTool(string guid)
        {

            if (!_downloadService.RequestIsValid(guid))
            {
                return new JsonResult(new BasicResponse { Message = "The requested file was not found." }) { StatusCode = StatusCodes.Status412PreconditionFailed };
            }

            var result = _downloadService.GetStoredFileDataForGuid(guid);
            
            return PhysicalFile(result.FilePath, "application/vnd.android.package-archive", result.ServeName);
        }
    }
}