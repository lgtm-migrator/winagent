using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.ServiceProcess;
using System.Diagnostics;

using plugin;

// TODO: Create constants for all the config stuff

namespace winagent
{
    class Agent
    {
        /// <summary>
        /// List of timers to keep a reference of each task
        /// Avoid the timers to be garbage collected
        /// </summary>
        internal static List<Timer> timersReference;

        /// <summary>
        /// List of plugins in the "plugins" folder
        /// </summary>
        internal static List<PluginDefinition> pluginList;

        /// <summary>
        /// Static constructor to initialize static data when any static member is referenced
        /// </summary>
        static Agent()
        {
            pluginList = LoadPlugins();
            timersReference = new List<Timer>();
        }

        // Parse settings
        internal static Settings.Agent GetSettings(string path = @"config.json")
        {
            try
            {
                // Content of the onfiguration file "config.json"
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings.Agent>(File.ReadAllText(path));
            }
            catch (FileNotFoundException ex)
            {
                // EventID 9 => Config file not found
                using (EventLog eventLog = new EventLog("Application"))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder("Error reading config file: ");
                    message.Append(path);
                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Information, 9, 1);
                }

                return null;
            }
        }

        // Load plugin assemblies
        internal static List<PluginDefinition> LoadPlugins()
        {
            List<PluginDefinition> pluginList = new List<PluginDefinition>();

            foreach (String path in Directory.GetFiles("plugins"))
            {
                Assembly plugin = Assembly.LoadFrom(path);
                pluginList.AddRange(LoadAllClassesImplementingSpecificAttribute<PluginAttribute>(plugin));
            }

            return pluginList;
        }

        // Selects classes with the specified attribute
        private static List<PluginDefinition> LoadAllClassesImplementingSpecificAttribute<T>(Assembly assembly)
        {
            IEnumerable<Type> typesImplementingAttribute = GetTypesWithSpecificAttribute<T>(assembly);

            List<PluginDefinition> attributeList = new List<PluginDefinition>();
            foreach (Type type in typesImplementingAttribute)
            {
                var attribute = type.GetCustomAttribute(typeof(T));
                attributeList.Add(new PluginDefinition(attribute, type));
            }

            return attributeList;
        }

        // Selects the types of the classes with the specified attribute
        internal static IEnumerable<Type> GetTypesWithSpecificAttribute<T>(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        // Execute updater
        internal static void RunUpdater(object state)
        {
            try
            {
                // If there is a new version of the updater in .\tmp\ copy it
                if(File.Exists(@".\tmp\winagent-updater.exe"))
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

        // Executes a task
        internal static void ExecuteTask(object state)
        {
            try
            {
                ((TaskObject)state).Execute();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);

                // EventID 5 => Error executing plugin
                using (EventLog eventLog = new EventLog("Application"))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder("An error ocurred executing a plugin:");
                    message.Append(Environment.NewLine);
                    message.Append(e.ToString());

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error, 5, 1);
                }
            }
        }
    }
}
