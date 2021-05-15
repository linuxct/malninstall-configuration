using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Common.Enums;
using space.linuxct.malninstall.Configuration.Common.Exceptions;
using space.linuxct.malninstall.Configuration.Common.Extensions;
using space.linuxct.malninstall.Configuration.Common.Helpers.Hcaptcha;
using space.linuxct.malninstall.Configuration.Common.Helpers.SafetyNet;
using space.linuxct.malninstall.Configuration.Common.Models.Persistence;
using space.linuxct.malninstall.Configuration.Common.Models.SafetyNet;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.PackageGenerationLogic;

namespace space.linuxct.malninstall.Configuration.Core.Application.Services.ControllerLogic
{
    public class PackageCreatorLogicService : IPackageCreatorLogicService
    {
        private readonly IPackageGenerationService _packageGenerationService;
        private readonly ILogger<PackageCreatorLogicService> _logger;
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly LinkGenerator _linkGenerator;

        public PackageCreatorLogicService(IPackageGenerationService packageGenerationService,
            IDistributedCache distributedCache, IHttpContextAccessor contextAccessor,
            IConfiguration configuration, LinkGenerator generator, ILogger<PackageCreatorLogicService> logger)
        {
            _packageGenerationService = packageGenerationService;
            _distributedCache = distributedCache;
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _linkGenerator = generator;
            _logger = logger;
        }

        public bool ParametersAreValid(PackageGenerationInput input)
        {
            return !string.IsNullOrWhiteSpace(input.PackageName) && !string.IsNullOrWhiteSpace(input.Token);
        }

        public async Task<ChannelVerificationResult> ValidateChannel(PackageGenerationInput input)
        {
            var result = new ChannelVerificationResult();
            var connectionIdentifierHash = _contextAccessor.HttpContext.GetRemoteIPAddress().ToString().ToSha256();

            //Run validation depending on the Entry Channel (Web, Application)
            switch (input.EntryChannel)
            {
                case EntryChannelEnum.Web:
                    try
                    {
                        result.ChannelVerificationSucceeded =
                            await HcaptchaValidationHelper.CheckHcaptchaResponseIsValid(input.Token,
                                _configuration.GetSection("HcaptchaSecret").Value);
                    }
                    catch (InvalidSignatureException ise)
                    {
                        result.ChannelVerificationErrorMessage = ise.Message;
                    }

                    break;
                case EntryChannelEnum.Application:
                    try
                    {
                        result.ChannelVerificationSucceeded =
                            SafetyNetValidationHelper.CheckSafetyNetResponseIsValid(input.Token, 
                                _distributedCache, connectionIdentifierHash);
                    }
                    catch (InvalidSignatureException ise)
                    {
                        result.ChannelVerificationErrorMessage = ise.Message;
                    }

                    break;
            }

            return result;
        }

        public async Task<PackageGenerationResult> GeneratePackageAsync(PackageGenerationInput input)
        {
            var packageDetails = await _packageGenerationService.ProcessForPackageAsync(input.PackageName);

            var result = new PackageGenerationResult();
            switch (packageDetails.Status)
            {
                case PackageCreationStatus.NotCreated:
                    result.ServeStatus = PackageServeStatus.FileDoesNotExist;
                    break;
                case PackageCreationStatus.Success:
                case PackageCreationStatus.AlreadyExists:
                {
                    var downloadKey = $"{Guid.NewGuid():N}";
                    var connectionIdentifierHash =
                        _contextAccessor.HttpContext.GetRemoteIPAddress().ToString().ToSha256();
                    var downloadContents = new DownloadContentsModel
                    {
                        FilePath = packageDetails.FileLocation,
                        ConnectionIdentifierHash = connectionIdentifierHash
                    };

                    //Result is stored for 1 hour, but once accessed it gets deleted
                    await _distributedCache.SetTimedObjectAsync(downloadKey, downloadContents, 1);

                    //Finalize setting up the result object
                    result.ServeStatus = PackageServeStatus.Ready;
                    result.DownloadUrl = _linkGenerator.GetUriByPage(_contextAccessor.HttpContext,
                        "/Download/GetTool", null, new {guid = downloadKey});
                    result.FileName = packageDetails.ApplicationName;
                    break;
                }
            }

            return result;
        }

        public NonceResult GenerateNonce()
        {
            var connectionIdentifierHash = _contextAccessor.HttpContext.GetRemoteIPAddress().ToString().ToSha256();

            //Generate 16 byte random and pass static CF data as seed
            var random = new Random();
            var bArr = new byte[16];
            random.NextBytes(bArr);
            var result = new NonceResult {GeneratedNonce = bArr, ConnectionIdentifierHash = connectionIdentifierHash};

            //Store the resulting 16 byte plus connectionIdentifierHash inside Redis
            var hashedRandom = bArr.ToSha256();
            _distributedCache.SetTimedObject(hashedRandom, result, 1);
            return result;
        }
    }
}