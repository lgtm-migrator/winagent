using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using plugin;
using Winagent.Settings;
using Winagent.Models;

namespace Winagent
{
    static class CLI
    {       
        //TODO: Add Error management
        // Non-service execution
        internal static void ExecuteConfig(string path)
        {
            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Get application settings
            Settings.Agent settings = Agent.GetSettings(path);

            // Create envent handlers
            Agent.SetEventReaders(settings.EventLogs);

            // Create tasks
            Agent.CreateTasks(settings.InputPlugins);
        }
                       
        // Selects the specified plugin and executes it   
        internal static void ExecuteCommand(string i, string o, string[] inputOptions, string[] outputOptions)
        {
            // TODO: This has to be improved, it's a big workaround to convert the optons to JObject
            // Convert options to dictionary
            IDictionary<string, string> inputOptionsDict = inputOptions.Select(part => part.Split(':')).ToDictionary(sp => sp[0], sp => sp[1]);
            IDictionary<string, string> outputOptionsDict = outputOptions.Select(part => part.Split(':')).ToDictionary(sp => sp[0], sp => sp[1]);
            
            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Create Settings
            Models.InputPlugin input = new Models.InputPlugin()
            {
                Name = i,
                Settings = JObject.FromObject(inputOptionsDict),
                Instance = (IInputPlugin)Agent.GetPluginInstance(i)
            };

            Models.OutputPlugin output = new Models.OutputPlugin()
            {
                Name = i,
                Settings = JObject.FromObject(outputOptionsDict),
                Instance = (IOutputPlugin)Agent.GetPluginInstance(o)
            };

            // Load plugins after parse options
            //List<PluginDefinition> pluginList = Agent.LoadPlugins();
            string inputResult = input.Instance.Execute(input.Settings);

            output.Instance.Execute(inputResult, output.Settings);
        }
    }
}