using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Models;

namespace space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic
{
    public interface IDownloadLogicService
    {
        public bool RequestIsValid(string guid);
        public StoredFileContentDetails GetFilePathForGuid(string guid);
    }
}