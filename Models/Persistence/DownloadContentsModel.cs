using System;

namespace space.linuxct.malninstall.Configuration.Models.Persistence
{
    [Serializable]
    public class DownloadContentsModel
    {
        public string FilePath { get; set; }
        public string ConnectionIdentifierHash { get; set; }
    }
}