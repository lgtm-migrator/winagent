using System;
using System.Collections.Generic;
using CommandLine;

namespace winagent
{
    class Options
    {
        // Options

        [Option('s', "service", HelpText = "Run as a service.")]
        public Boolean Service { get; set; }

        [Option('i', "input", Separator = ',', Default = new string[] { "updates" }, HelpText = "Input plugins separated by comma.")]
        public IEnumerable<string> Input { get; set; }

        [Option('o', "output", Separator = ',', Default = new string[] { "console" }, HelpText = "Output plugins separated by comma.")]
        public IEnumerable<string> Output { get; set; }
    }
}
