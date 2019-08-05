using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using plugin;
using Winagent.Settings;

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
            
            // Convert options to JObject
            JObject inputOptionsJObj = JObject.FromObject(inputOptionsDict);
            JObject outputOptionsJObj = JObject.FromObject(outputOptionsDict);


            // Create Settings
            Settings.InputPlugin input = new Settings.InputPlugin()
            {
                Name = i,
                Settings = inputOptionsJObj,
                OutputPlugins = new List<Settings.OutputPlugin>
                {
                    new Settings.OutputPlugin
                    {
                        Name = o,
                        Settings = outputOptionsJObj
                    }
                }
            };
            
            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Load plugins after parse options
            List<PluginDefinition> pluginList = Agent.LoadPlugins();

            PluginDefinition inputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == input.Name.ToLower()).First();
            IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;
            string inputResult = inputPlugin.Execute(input.Settings);

            foreach (Settings.OutputPlugin output in input.OutputPlugins)
            {
                PluginDefinition outputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == output.Name.ToLower()).First();

                IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;
            
                outputPlugin.Execute(inputResult, output.Settings);
            }
        }

    }
}