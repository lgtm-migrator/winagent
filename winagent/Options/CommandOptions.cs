using System.Collections.Generic;
using CommandLine;

namespace Winagent.Options
{
    [Verb("run", HelpText = "Run a specific configuration command.")]
    class CommandOptions
    {
        // Options

        [Value(0)]
        public string ConfigFile { get; set; }

        [Option('i', "input", Default = "updates", HelpText = "Input plugin")]
        public string Input { get; set; }

        [Option('o', "output", Default = "console", HelpText = "Output plugin")]
        public string Output { get; set; }

        [Option("input-options", Separator = ',', HelpText = "Options for the input plugin in the form <opt>:<value>")]
        public IEnumerable<string> InputOptions { get; set; }

        [Option("output-options", Separator = ',', HelpText = "Options for the output plugin separated by coma, in the form <opt>.<value>")]
        public IEnumerable<string> OutputOptions { get; set; }
    }
}
