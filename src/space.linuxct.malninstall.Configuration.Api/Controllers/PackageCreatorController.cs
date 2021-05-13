using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Common.Enums;
using space.linuxct.malninstall.Configuration.Common.Exceptions;
using space.linuxct.malninstall.Configuration.Common.Extensions;
using space.linuxct.malninstall.Configuration.Common.Helpers.Hcaptcha;
using space.linuxct.malninstall.Configuration.Common.Helpers.RateLimit;
using space.linuxct.malninstall.Configuration.Common.Helpers.SafetyNet;
using space.linuxct.malninstall.Configuration.Common.Models.Persistence;
using space.linuxct.malninstall.Configuration.Common.Models.SafetyNet;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services;
using space.linuxct.malninstall.Configuration.ViewModels.Common;
using space.linuxct.malninstall.Configuration.ViewModels.PackageCreator;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PackageCreatorController : ControllerBase
    {
        private readonly ILogger<PackageCreatorController> _logger;
        private readonly IPackageGenerationService _packageGenerationService;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;
        
        public PackageCreatorController(ILogger<PackageCreatorController> logger, IPackageGenerationService packageGenerationService, IDistributedCache distributedCache, IConfiguration configuration)
        {
            _logger = logger;
            _packageGenerationService = packageGenerationService;
            _configuration = configuration;
            _distributedCache = distributedCache;
        }
        
        [HttpPost]
        [EnableCors("FrontEndPolicy")]
        [ProducesResponseType(typeof(GeneratePackageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)] //Error ParametersInvalid
        [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status403Forbidden)] //Error TooManyCalls
        [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status412PreconditionFailed)] //Error InvalidSignatureException, Error NotValidPackageName
        [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status500InternalServerError)] //Error PackageGenerationError
        public async Task<IActionResult> GeneratePackage(GeneratePackageRequest req)
        {
            //Validate required input parameters are supplied
            if (string.IsNullOrWhiteSpace(req.PackageName) ||
                req.EntryChannel == EntryChannelEnum.Web && string.IsNullOrWhiteSpace(req.HcaptchaClientResponse) ||
                req.EntryChannel == EntryChannelEnum.Application && string.IsNullOrWhiteSpace(req.SafetyNetJwt))
            {
                return new JsonResult(new BasicResponse { Message = "Invalid parameters." }) { StatusCode = StatusCodes.Status400BadRequest };
            }
            
            //Get static data from API call via CF
            var connectionIdentifierHash = HttpContext.GetRemoteIPAddress().ToString().ToSha256();
            
            //Find number of calls from this IP in Redis, block if needed
            var rateLimitHelper = new RateLimitHelper(HttpContext, _distributedCache);
            if (rateLimitHelper.IsClientRateLimited())
            {
                return new JsonResult(new BasicResponse { Message = "Too many calls, try again later." }) { StatusCode = StatusCodes.Status403Forbidden };
            }
            
            //Verify the token with the SN or Hcaptcha helpers
            var channelValidationMethodResult = false;
            var channelValidationMethodErrorMessage = string.Empty;
            switch (req.EntryChannel)
            {
                case EntryChannelEnum.Web:
                    try
                    {
                        channelValidationMethodResult = await HcaptchaValidationHelper.CheckHcaptchaResponseIsValid(req.HcaptchaClientResponse, _configuration.GetSection("HcaptchaSecret").Value);
                    }
                    catch (InvalidSignatureException ise)
                    {
                        channelValidationMethodErrorMessage = ise.Message;
                    }
                    break;
                case EntryChannelEnum.Application:
                    try
                    {
                        channelValidationMethodResult = SafetyNetValidationHelper.CheckSafetyNetResponseIsValid(req.SafetyNetJwt, _distributedCache, connectionIdentifierHash);
                    }
                    catch (InvalidSignatureException ise)
                    {
                        channelValidationMethodErrorMessage = ise.Message;
                    }
                    break;
            }

            if (!channelValidationMethodResult)
            {
                return new JsonResult(new BasicResponse 
                    { 
                        Message = string.IsNullOrWhiteSpace(channelValidationMethodErrorMessage) ? 
                            "Error while processing the preconditions" : 
                            channelValidationMethodErrorMessage
                    }) { StatusCode = StatusCodes.Status412PreconditionFailed };
            }

            //Check the package name is a valid package name (contains only strings and periods, with regex), return 412
            if (!req.PackageName.IsValidPackageName())
            {
                return new JsonResult(new BasicResponse { Message = "Package name supplied is not valid." }) { StatusCode = StatusCodes.Status412PreconditionFailed };
            }
            
            //Generate the package
            var result = await _packageGenerationService.ProcessForPackageAsync(req.PackageName);
            if (result.Status == PackageCreationStatus.Success || result.Status == PackageCreationStatus.AlreadyExists)
            {
                var downloadKey = $"{Guid.NewGuid():N}";
                var downloadContents = new DownloadContentsModel
                {
                    FilePath = result.FileLocation,
                    ConnectionIdentifierHash = connectionIdentifierHash
                };
                
                //Result is stored for 1 hour, but once accessed it gets deleted
                await _distributedCache.SetTimedObjectAsync(downloadKey, downloadContents, 1);
                
                return new JsonResult(new GeneratePackageResponse
                {
                    GenerationStatusCode = PackageServeStatus.Ready,
                    DownloadUrl = Url.Action("GetTool", "Download", new {guid = downloadKey}),
                    FileName = downloadContents.FilePath.Split('/').LastOrDefault()
                });
            }
            
            return new JsonResult(new BasicResponse { Message = "The package was not generated, please try again." }) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        [HttpGet]
        [ProducesResponseType(typeof(NonceResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status403Forbidden)] //Error TooManyCallsException
        public IActionResult GenerateNonce()
        {
            //Get static data from API call via CF
            var connectionIdentifierHash = HttpContext.GetRemoteIPAddress().ToString().ToSha256();
            var rateLimitHelper = new RateLimitHelper(HttpContext, _distributedCache);
            if (rateLimitHelper.IsClientRateLimited())
            {
                return new JsonResult(new BasicResponse { Message = "Too many calls, try again later." }) { StatusCode = StatusCodes.Status403Forbidden };
            }

            //Generate 16 byte random and pass static CF data as seed
            var random = new Random();
            var bArr = new byte[16];
            random.NextBytes(bArr);
            var result = new NonceResult { GeneratedNonce = bArr, ConnectionIdentifierHash = connectionIdentifierHash};
            
            //Store the resulting 16 byte plus connectionIdentifierHash inside Redis
            var hashedRandom = bArr.ToSha256();
            _distributedCache.SetTimedObject(hashedRandom, result, 1);
            return new JsonResult(result);
        }
    }
}