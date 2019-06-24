using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winagent.Settings
{
    class AutoUpdater
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "schedule")]
        public Schedule Schedule { get; set; }
    }
}
