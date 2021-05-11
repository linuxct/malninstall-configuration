using System;
using System.Text.Json.Serialization;

namespace space.linuxct.malninstall.Configuration.Common.Models.Hcaptcha
{
    [Serializable]
    public class HcaptchaResponse
    {
        public bool Success { get; set; }
        [JsonPropertyName("challenge_ts")]
        public string ChallengeTimestamp { get; set; }
        public string Hostname { get; set; }
    }

}