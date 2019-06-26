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

        /// <summary>
        /// Parse settings
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when the config file could not be found</exception>
        internal static Settings.Agent GetSettings(string path = @"config.json")
        {
            try
            {
                // Content of the onfiguration file "config.json"
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Settings.Agent>(File.ReadAllText(path));
            }
            catch (FileNotFoundException fnfe)
            {
                // Event 9 => Config file not found
                ExceptionManager.HandleError(String.Format("The specified config file \"{0}\" could not be found", path), 9, fnfe.ToString());

                // TODO: Return null?
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
                ExceptionManager.HandleInformation(String.Format("An error ocurred executing a plugin"), 5, e.ToString());
            }
        }
    }
}
