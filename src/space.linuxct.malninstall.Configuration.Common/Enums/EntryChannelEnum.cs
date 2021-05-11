using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace space.linuxct.malninstall.Configuration.Common.Enums
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntryChannelEnum
    {
        Web,
        Application
    }
}