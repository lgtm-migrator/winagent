using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Diagnostics;

using plugin;
using Newtonsoft.Json;

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

        /// <summary>
        /// Parse settings
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when the config file could not be found</exception>
        /// <exception cref="Newtonsoft.Json.JsonSerializationException">Thrown when the content of the config file is incorrect</exception>
        /// <exception cref="Exception">Thrown when a different error occurs</exception>
        internal static Settings.Agent GetSettings(string path = @"config.json")
        {
            try
            {
                // Content of the onfiguration file "config.json"
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings.Agent>(File.ReadAllText(path));
            }
            catch (FileNotFoundException fnfe)
            {
                // EventID 6 => Config file not found
                ExceptionHandler.HandleError(String.Format("The specified path \"{0}\" does not appear to be valid", path), 6, fnfe);

                throw;
            }
            catch (Newtonsoft.Json.JsonSerializationException jse)
            {
                // EventID 7 => Error in config file
                ExceptionHandler.HandleError("The agent could not parse the config file, please check the syntax", 7, jse);

                throw;
            }
            catch (Exception e)
            {
                // EventID 8 => Error while parsing the config file
                ExceptionHandler.HandleError("An undefined error occurred while parsing the config file", 8, e);

                throw;
            }
        }

        /// <summary>
        /// Load plugin assemblies in the "plugins" folder
        /// </summary>
        internal static List<PluginDefinition> LoadPlugins()
        {
            List<PluginDefinition> pluginList = new List<PluginDefinition>();

            try
            {
                foreach (String path in Directory.GetFiles("plugins"))
                {
                    Assembly plugin = Assembly.LoadFrom(path);
                    pluginList.AddRange(LoadAllClassesImplementingSpecificAttribute<PluginAttribute>(plugin));
                }
            }
            catch(DirectoryNotFoundException dnfe)
            {
                // EventID 9 => Invalid plugin files in "plugins" folder
                ExceptionHandler.HandleError(String.Format("Could not find \"plugins\" directory"), 9, dnfe);

                throw;
            }
            catch (BadImageFormatException bie)
            {
                // EventID 10 => Invalid plugin files in "plugins" folder
                ExceptionHandler.HandleError(String.Format("Invalid plugin file found in the \"plugins\" directory"), 10, bie);

                throw;
            }

            return pluginList;
        }

        /// <summary>
        /// Schedule tasks defined in the config file
        /// </summary>
        /// <param name="inputPlugins">Plugins to be executed</param>
        internal static void CreateTasks(List<Settings.InputPlugin> inputPlugins)
        {

            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            foreach (Settings.InputPlugin input in inputPlugins)
            {
                try
                {
                    PluginDefinition inputPluginMetadata = pluginList.Where(p => ((PluginAttribute)p.Attribute).PluginName.ToLower() == input.Name.ToLower()).First();
                    IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;

                    foreach (Settings.OutputPlugin output in input.OutputPlugins)
                    {
                        PluginDefinition outputPluginMetadata = pluginList.Where(p => ((PluginAttribute)p.Attribute).PluginName.ToLower() == output.Name.ToLower()).First();
                        IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;

                        // Create task whitht the input and output plugins to be run by the Timer
                        TaskObject task = new TaskObject(inputPlugin, outputPlugin, input.Settings, output.Settings);

                        // Create Timer to schedule the task
                        Timer timer = new Timer(new TimerCallback(ExecuteTask), task, 0, output.Schedule.GetTime());

                        // Save reference to avoid GC
                        timersReference.Add(timer);
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    // EventID 4 => There are no plugins to execute
                    ExceptionHandler.HandleError(String.Format("The specified plugin does not exist in the \"plugins\" directory"), 4, ioe);

                    throw;
                }
                
            }
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

        /// <summary>
        /// Executes a task
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="Exception">General error during plugin execution</exception>
        internal static void ExecuteTask(object state)
        {
            try
            {
                ((TaskObject)state).Execute();
            }
            catch(Exception e)
            {
                // EventID 5 => Error executing plugin
                ExceptionHandler.HandleError(String.Format("An error ocurred while executing a plugin"), 5, e);
                throw;
            }
        }

        internal static void SetEventReaders(List<Settings.EventLog> eventLogs)
        {
            foreach(Settings.EventLog eventLog in eventLogs)
            {
                EventLog EventLogInstance = new EventLog(eventLog.Name);
                EventLogInstance.EntryWritten += new EntryWrittenEventHandler((sender, e) => OnEntryWritten(sender, e, eventLog));
                EventLogInstance.EnableRaisingEvents = true;
            }
        }

        private static void OnEntryWritten(object sender, EntryWrittenEventArgs e, Settings.EventLog settings)
        {
            try
            {
                var eventdetail = e.Entry;
                var log = new Models.Log()
                {
                    Date = eventdetail.TimeGenerated.ToUniversalTime(),
                    Description = eventdetail.Message,
                    Id = eventdetail.InstanceId,
                    Severity = eventdetail.EntryType.ToString(),
                    Source = eventdetail.Source,
                    HostName = eventdetail.MachineName,
                    EventLogName = settings.Name
                };

                foreach (Settings.OutputPlugin output in settings.OutputPlugins)
                {
                    PluginDefinition outputPluginMetadata = pluginList.Where(p => ((PluginAttribute)p.Attribute).PluginName.ToLower() == output.Name.ToLower()).First();
                    IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;

                    outputPlugin.Execute(JsonConvert.SerializeObject(log), output.Settings);
                }
            }
            catch (Exception ex)
            {
                // EventID 11 => An error occurred while hadling an event
                ExceptionHandler.HandleError(String.Format("An error occurred while hadling an Event"), 11, ex);
            }
        }


    }
}
