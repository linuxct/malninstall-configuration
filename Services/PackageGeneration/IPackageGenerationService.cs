using System.Threading.Tasks;
using space.linuxct.malninstall.Configuration.Models;

namespace space.linuxct.malninstall.Configuration.Services.PackageGeneration
{
    public interface IPackageGenerationService
    {
        public Task<PackageDetails> ProcessForPackageAsync(string packageName);
    }
}