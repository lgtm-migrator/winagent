using Newtonsoft.Json.Linq;
using plugin;

namespace Winagent
{
    public class TaskObject
    {
        private IInputPlugin inputPlugin;
        private IOutputPlugin outputPlugin;
        private JObject inputOptions;
        private JObject outputOptions;
        public object _locker = new object();

        public TaskObject(IInputPlugin input, IOutputPlugin output, JObject inputOptions, JObject outputOptions)
        {
            inputPlugin = input;
            outputPlugin = output;
            this.inputOptions = inputOptions;
            this.outputOptions = outputOptions;
        }

        public void Execute()
        {
            outputPlugin.Execute(inputPlugin.Execute(inputOptions), outputOptions);
        }

    }
}