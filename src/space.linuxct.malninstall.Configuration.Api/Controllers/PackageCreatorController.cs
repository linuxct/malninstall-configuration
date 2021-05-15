using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Common.Enums;
using space.linuxct.malninstall.Configuration.Common.Extensions;
using space.linuxct.malninstall.Configuration.Common.Models.SafetyNet;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic;
using space.linuxct.malninstall.Configuration.ViewModels.Common;
using space.linuxct.malninstall.Configuration.ViewModels.PackageCreator;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PackageCreatorController : ControllerBase
    {
        private readonly ILogger<PackageCreatorController> _logger;
        private readonly IPackageCreatorLogicService _packageCreatorService;

        public PackageCreatorController(ILogger<PackageCreatorController> logger, IPackageCreatorLogicService packageCreatorService)
        {
            _logger = logger;
            _packageCreatorService = packageCreatorService;
        }

        [HttpPost]
        [EnableCors("FrontEndPolicy")]
        [ProducesResponseType(typeof(GeneratePackageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)] //Error ParametersInvalid
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status412PreconditionFailed)] //Error InvalidSignatureException, Error NotValidPackageName
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status500InternalServerError)] //Error PackageGenerationError
        public async Task<IActionResult> GeneratePackage(GeneratePackageRequest req)
        {
            //Flatten the request into a model easier to work with
            var input = req.Flatten();
            
            //Validate required input parameters are supplied
            if (!_packageCreatorService.ParametersAreValid(input))
            {
                return new JsonResult(new BasicResponse {Message = "Invalid parameters."})
                    {StatusCode = StatusCodes.Status400BadRequest};
            }

            //Verify the token with the SN or Hcaptcha helpers
            var channelValidationResult = await _packageCreatorService.ValidateChannel(input);

            //If it did not succeed, return 412
            if (!channelValidationResult.ChannelVerificationSucceeded)
            {
                return new JsonResult(new BasicResponse
                {
                    Message = string.IsNullOrWhiteSpace(channelValidationResult.ChannelVerificationErrorMessage)
                        ? "Error while processing the preconditions"
                        : channelValidationResult.ChannelVerificationErrorMessage
                }) {StatusCode = StatusCodes.Status412PreconditionFailed};
            }

            //Check the package name is a valid package name (contains only strings and periods, with regex), return 412
            if (!input.PackageName.IsValidPackageName())
            {
                return new JsonResult(new BasicResponse {Message = "Package name supplied is not valid."})
                    {StatusCode = StatusCodes.Status412PreconditionFailed};
            }

            //Generate the package
            var result = await _packageCreatorService.GeneratePackageAsync(input);

            //Process generation result
            return result.ServeStatus == PackageServeStatus.Ready ? 
                new JsonResult(new GeneratePackageResponse().Unflatten(result)) : 
                new JsonResult(new BasicResponse {Message = "The package was not generated, please try again."}) 
                    {StatusCode = StatusCodes.Status500InternalServerError};
        }

        [HttpGet]
        [ProducesResponseType(typeof(NonceResult), StatusCodes.Status200OK)]
        public IActionResult GenerateNonce()
        {
            return new JsonResult(_packageCreatorService.GenerateNonce());
        }
    }
}