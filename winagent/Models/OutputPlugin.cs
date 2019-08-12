using Newtonsoft.Json.Linq;

using plugin;

namespace winagent.Models
{
    class OutputPlugin
    {
        public string Name { get; }

        public IOutputPlugin Plugin { get; }

        public JObject Settings { get; set; }

        public OutputPlugin(string name, IOutputPlugin plugin)
        {
            Name = name;
            Plugin = plugin;
        }
    }
}
