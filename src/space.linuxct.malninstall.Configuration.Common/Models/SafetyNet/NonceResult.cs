using System;

namespace space.linuxct.malninstall.Configuration.Common.Models.SafetyNet
{
    [Serializable]
    public class NonceResult
    {
        public byte[] GeneratedNonce { get; set; }
        public string ConnectionIdentifierHash { get; set; }
    }
}