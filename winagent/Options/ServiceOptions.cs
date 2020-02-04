using System;
using CommandLine;

namespace Winagent.Options
{
    [Verb("service", HelpText = "Execute the agent as a service.")]
    class ServiceOptions
    {
        // Options

        [Option("config", SetName = "configpath", HelpText = "Path to the configuration file that will be used by the service")]
        public string Config { get; set; }
    }
}
