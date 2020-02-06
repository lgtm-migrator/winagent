using System;
using CommandLine;

namespace Winagent.Options
{
    [Verb("service", HelpText = "Execute the agent as a service.")]
    class ManagementOptions
    {
        private string _config;

        // Options

        [Option("install", SetName = "install", HelpText = "Install the service")]
        public Boolean Install { get; set; }

        [Option("uninstall", SetName = "uninstall", HelpText = "Uninstall the service")]
        public Boolean Uninstall { get; set; }

        [Option("start", SetName = "start", HelpText = "Start the service if it's installed and stopped")]
        public Boolean Start { get; set; }

        [Option("stop", SetName = "stop", HelpText = "Stop the service if it's installed and started")]
        public Boolean Stop { get; set; }

        [Option("restart", SetName = "restart", HelpText = "Restart the service if it's installed and running")]
        public Boolean Restart { get; set; }

        [Option("status", SetName = "status", HelpText = "Check service status")]
        public Boolean Status { get; set; }

        [Option("config", SetName = "install", HelpText = "Path to the configuration file that will be used by the service")]
        public string Config {

            get => _config;

            set => _config = $"--config {value}";

        }
    }
}
