using Newtonsoft.Json;
using System.Collections.Generic;

namespace winagent.Settings
{
    class Agent
    {
        [JsonProperty(PropertyName = "autoUpdates")]
        public AutoUpdater UpdateSettings { set; get; }

        [JsonProperty(PropertyName = "inputPlugins")]
        public List<InputPlugin> InputPlugins { set; get; }

        [JsonProperty(PropertyName = "eventsLogs")]
        public List<EventLog> EventLogs { set; get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this); ;
        }
    }
}
