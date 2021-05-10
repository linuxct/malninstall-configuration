using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace space.linuxct.malninstall.Configuration.Enums
{
    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntryChannelEnum
    {
        Web,
        Application
    }
}