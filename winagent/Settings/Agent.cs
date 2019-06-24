using Newtonsoft.Json;
using System.Collections.Generic;

namespace winagent.Settings
{
    class Agent
    {
        [JsonProperty(PropertyName = "auto_updates")]
        public AutoUpdater UpdateSettings { set; get; }

        [JsonProperty(PropertyName = "input_plugins")]
        public List<InputPlugin> InputPlugins { set; get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this); ;
        }
    }
}
