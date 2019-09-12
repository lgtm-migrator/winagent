using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using plugin;

namespace Winagent.Models
{
    class Task
    {
        public InputPlugin InputPlugin { get; }

        public OutputPlugin OutputPlugin { get; }

        public JObject InputOptions { get; }

        public JObject OutputOptions { get; }

        public object Locker { get; set; } = new object();

        public Task(InputPlugin input, OutputPlugin output)
        {
            InputPlugin = input;
            OutputPlugin = output;
            InputOptions = input.Settings;
            OutputOptions = output.Settings;
        }

        public void Execute()
        {
            OutputPlugin.Instance.Execute(InputPlugin.Instance.Execute(InputOptions), OutputOptions);
        }
    }
}
