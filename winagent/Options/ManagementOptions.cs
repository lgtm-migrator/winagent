using System;
using CommandLine;

namespace Winagent.Options
{
    [Verb("service", HelpText = "Execute the agent as a service.")]
    class ManagementOptions
    {
        private string _config;

        // Options

        [Option("install", Group = "Service control", SetName = "install", HelpText = "Install the service")]
        public Boolean Install { get; set; }

        [Option("uninstall", Group = "Service control", SetName = "uninstall", HelpText = "Uninstall the service")]
        public Boolean Uninstall { get; set; }

        [Option("start", Group = "Service control", SetName = "start", HelpText = "Start the service if it's installed and stopped")]
        public Boolean Start { get; set; }

        [Option("stop", Group = "Service control", SetName = "stop", HelpText = "Stop the service if it's installed and started")]
        public Boolean Stop { get; set; }

        [Option("restart", Group = "Service control", SetName = "restart", HelpText = "Restart the service if it's installed and running")]
        public Boolean Restart { get; set; }

        [Option("status", Group = "Service control", SetName = "status", HelpText = "Check service status")]
        public Boolean Status { get; set; }

        [Option("config", HelpText = "Path to the configuration file that will be used by the service, can be used with 'install' and 'start'")]
        public string Config {

            get => _config;

            set => _config = $"--config {value}";

        }
    }
}
