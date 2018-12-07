using plugin;

namespace winagent
{
    public class TaskObject
    {
        private IInputPlugin inputPlugin;
        private IOutputPlugin outputPlugin;
        private string[] options;

        public TaskObject(IInputPlugin input, IOutputPlugin output, string[] opts)
        {
            inputPlugin = input;
            outputPlugin = output;
            options = opts;
        }

        public void Execute()
        {
            outputPlugin.Execute("test", options);
        }

    }
}