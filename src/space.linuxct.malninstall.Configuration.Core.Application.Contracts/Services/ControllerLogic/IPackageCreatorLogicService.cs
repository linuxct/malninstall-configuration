using System.Threading.Tasks;
using space.linuxct.malninstall.Configuration.Common.Models.SafetyNet;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;

namespace space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic
{
    public interface IPackageCreatorLogicService
    {
        public bool ParametersAreValid(PackageGenerationInput input);
        public Task<ChannelVerificationResult> ValidateChannel(PackageGenerationInput input);
        public Task<PackageGenerationResult> GeneratePackageAsync(PackageGenerationInput input);
        public NonceResult GenerateNonce();
    }
}