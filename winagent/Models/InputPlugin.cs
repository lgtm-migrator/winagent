using Newtonsoft.Json.Linq;

using plugin;

namespace Winagent.Models
{
    class InputPlugin
    {
        public string Name { get; set; }

        public IInputPlugin Instance { get; set; }

        public JObject Settings { get; set; }
    }
}
