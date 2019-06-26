using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using plugin;

namespace winagent
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
            Settings.Agent config = Agent.GetSettings(path);

            foreach (Settings.InputPlugin input in config.InputPlugins)
            {
                PluginDefinition inputPluginMetadata = Agent.pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == input.Name.ToLower()).First();
                IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;

                foreach (Settings.OutputPlugin output in input.OutputPlugins)
                {
                    PluginDefinition outputPluginMetadata = Agent.pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == output.Name.ToLower()).First();
                    IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;

                    // TODO: Change parameters to JObject 
                    TaskObject task = new TaskObject(inputPlugin, outputPlugin, input.Settings, output.Settings);

                    Timer timer = new Timer(new TimerCallback(Agent.ExecuteTask), task, 0, output.Schedule.GetTime());

                    // Save reference to avoid GC
                    Agent.timersReference.Add(timer);
                }
            }
        }


        
        // Selects the specified plugin and executes it   
        internal static void ExecuteCommand(String[] inputs, String[] outputs, String[] inputOptions, String[] outputOptions)
        {/*
            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Load plugins after parse options
            List<PluginDefinition> pluginList = Agent.LoadPlugins();

            foreach (String input in inputs)
            {
                PluginDefinition inputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == input.ToLower()).First();
                IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;
                string inputResult = inputPlugin.Execute(inputOptions);

                foreach (String output in outputs)
                {
                    PluginDefinition outputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == output.ToLower()).First();

                    IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;
            
                    outputPlugin.Execute(inputResult, options);
                }
            }
        */}
        
    }
}