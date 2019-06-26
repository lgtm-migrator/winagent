using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using plugin;

namespace winagent
{
    class Service : ServiceBase
    {
        public Service()
        {
            ServiceName = "Winagent";

            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                // Get application settings
                Settings.Agent config = Agent.GetSettings();

                foreach (Settings.InputPlugin input in config.InputPlugins)
                {
                    PluginDefinition inputPluginMetadata = Agent.pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == input.Name.ToLower()).First();
                    IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;

                    foreach (Settings.OutputPlugin output in input.OutputPlugins)
                    {
                        PluginDefinition outputPluginMetadata = Agent.pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == output.Name.ToLower()).First();
                        IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;

                        // To pass multiple objects to the timer
                        // It is necessary to create a custom object containing the others
                        // TODO: Pass JObject as parameter
                        TaskObject task = new TaskObject(inputPlugin, outputPlugin, input.Settings, output.Settings);

                        Timer timer = new Timer(new TimerCallback(Agent.ExecuteTask), task, 0, output.Schedule.GetTime());

                        // Save reference to avoid GC
                        Agent.timersReference.Add(timer);
                    }
                }

                // Create detached autoupdater if autoupdates are enabled
                if (config.UpdateSettings.Enabled)
                {
                    // Run the updater after 1 minute
                    // The timer will run every 10 mins
                    Timer updaterTimer = new Timer(new TimerCallback(RunUpdater), null, 60000, config.UpdateSettings.Schedule.GetTime());
                    // Save reference to avoid GC
                    Agent.timersReference.Add(updaterTimer);
                }
            }
            // TODO: Check/Create NEW exception if the config file is wrong
            catch (InvalidOperationException ioe)
            {
                // EventID 4 => There are no plugins to execute
                ExceptionManager.HandleError(String.Format("There are no plugins to execute"), 4, ioe.ToString());
            }
            catch (Exception e)
            {
                // EventID 1 => An error ocurred
                ExceptionManager.HandleError(String.Format("General error during service execution"), 1, e.ToString());
            }
        }

        /// <summary>
        /// Execute updater
        /// </summary>
        /// TODO: ↓
        /// <param name="state">Object to run the timer</param>
        /// <exception cref="Exception">General exception when the updater is executed</exception>
        internal static void RunUpdater(object state)
        {
            try
            {
                // If there is a new version of the updater in .\tmp\ copy it
                if (File.Exists(@".\tmp\winagent-updater.exe"))
                {
                    File.Copy(@".\tmp\winagent-updater.exe", @".\winagent-updater.exe", true);
                    File.Delete(@".\tmp\winagent-updater.exe");

                    // EventID 3 => Application updated
                    ExceptionManager.HandleInformation(String.Format("Application updated: \"{0}\"", "winagent-updater.exe"), 3, null);
                }
                Process.Start(@"winagent-updater.exe");
            }
            catch (Exception e)
            {
                // EventID 2 => Error executing updater
                ExceptionManager.HandleError(String.Format("An error ocurred executing updater"), 2, e.ToString());
            }
        }

    }
}
