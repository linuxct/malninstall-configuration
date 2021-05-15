using System.Threading.Tasks;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;

namespace space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.PackageGenerationLogic
{
    public interface IPackageGenerationService
    {
        public Task<PackageDetails> ProcessForPackageAsync(string packageName);
    }
}