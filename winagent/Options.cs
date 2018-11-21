using System;
using System.Collections.Generic;
using CommandLine;

namespace winagent
{
    class Options
    {
        // Options

        [Option('s')]
        public Boolean Service { get; set; }

        [Option('i', Separator = ',')]
        public IEnumerable<string> Input { get; set; }

        [Option('o', Separator = ',')]
        public IEnumerable<string> Ouput { get; set; }
    }
}
