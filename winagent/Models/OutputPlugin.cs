using Newtonsoft.Json.Linq;

using plugin;

namespace Winagent.Models
{
    class OutputPlugin
    {
        public string Name { get; set; }

        public IOutputPlugin Instance { get; set; }

        public JObject Settings { get; set; }
    }
}
