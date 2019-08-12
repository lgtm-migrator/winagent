using Newtonsoft.Json.Linq;

using plugin;

namespace winagent.Models
{
    class InputPlugin
    {
        public string Name { get; }

        public IInputPlugin Plugin { get; }

        public JObject Settings { get; set; }

        public InputPlugin(string name, IInputPlugin plugin)
        {
            Name = name;
            Plugin = plugin;
        }
    }
}
