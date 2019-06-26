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
            catch (NullReferenceException nre)
            {
                // EventID 6 => Problem with config file
                using (EventLog eventLog = new EventLog("Application"))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder("There is a problem with the config file:");
                    message.Append(Environment.NewLine);
                    message.Append(nre.ToString());

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error, 6, 1);
                }
            }
            catch (InvalidOperationException ioe)
            {
                // EventID 4 => There are no plugins to execute
                using (EventLog eventLog = new EventLog("Application"))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder("There are no plugins to execute:");
                    message.Append(Environment.NewLine);
                    message.Append(ioe.ToString());

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error, 4, 1);
                }
            }
            catch (Exception e)
            {
                // EventID 1 => An error ocurred
                using (EventLog eventLog = new EventLog("Application"))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder("An error ocurred in the winagent service:");
                    message.Append(Environment.NewLine);
                    message.Append(e.ToString());

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error, 1, 1);
                }
            }
        }

        // Execute updater
        public static void RunUpdater(object state)
        {
            try
            {
                // If there is a new version of the updater in .\tmp\ copy it
                if (File.Exists(@".\tmp\winagent-updater.exe"))
                {
                    File.Copy(@".\tmp\winagent-updater.exe", @".\winagent-updater.exe", true);
                    File.Delete(@".\tmp\winagent-updater.exe");

                    // EventID 3 => Application updated
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        System.Text.StringBuilder message = new System.Text.StringBuilder("Application updated");
                        message.Append(Environment.NewLine);
                        message.Append(@"./winagent-updater.exe");
                        eventLog.Source = "Winagent";
                        eventLog.WriteEntry(message.ToString(), EventLogEntryType.Information, 3, 1);
                    }
                }
                Process.Start(@"winagent-updater.exe");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                // EventID 2 => Error executing updater
                using (EventLog eventLog = new EventLog("Application"))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder("An error ocurred executing updater:");
                    message.Append(Environment.NewLine);
                    message.Append(e.ToString());

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error, 2, 1);
                }
            }
        }
    }
}
