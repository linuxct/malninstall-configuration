using System;
using System.Collections.Generic;

namespace space.linuxct.malninstall.Configuration.Common.Models
{
    [Serializable]
    public class WordDictionary
    {
        public List<string> Actions { get; set; }
        public List<string> Connectors { get; set; }
        public List<string> Endings { get; set; }
    }
}