﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using CommandLine;

using plugin;

namespace winagent
{
    class Agent
    {
        static JObject config;

        // Entrypoint
        static void Main(string[] args)
        {
            // Parse CommandLine options
            // https://github.com/commandlineparser/commandline
            var options = Parser.Default.ParseArguments<Options>(args);

            // Call to overloaded Main method
            options.WithParsed(opts => Main(opts));
        }

        // Overloaded Main method with parsed pptions
        static void Main(Options options)
        {
            // Load plugins after parse options
            List<PluginDefinition> pluginList = LoadPlugins();
            
            if (options.Service)
            {
                //TODO: Change absolute path
                config = JObject.Parse(File.ReadAllText(@"config.json"));
                ExecuteService(pluginList);
            }
            else
            {
                ExecutePlugin(pluginList, (String[]) options.Input,(String[]) options.Output, new String[] { "table" });
            }

            // Prevents the test console from closing itself
            // Console.ReadKey();
        }

        // Load plugin assemblies
        public static List<PluginDefinition> LoadPlugins()
        {
            List<PluginDefinition> pluginList = new List<PluginDefinition>();

            foreach (String path in Directory.GetFiles("plugins"))
            {
                Assembly plugin = Assembly.LoadFrom(path);
                pluginList.AddRange(LoadAllClassesImplementingSpecificAttribute<PluginAttribute>(plugin));
            }

            return pluginList;
        }

        // Executes the windows service 
        public static void ExecuteService(List<PluginDefinition> pluginList)
        {

            foreach (JProperty input in ((JObject) config["input"]).Properties())
            {
                PluginDefinition inputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == input.Name.ToLower()).First();
                IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;

                foreach (JProperty output in ((JObject)input.Value).Properties())
                {
                    PluginDefinition outputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == output.Name.ToLower()).First();
                    IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;

                    TaskObject task = new TaskObject(inputPlugin, outputPlugin, output.Value["options"].ToObject<string[]>());
                    
                    Timer testimer = new Timer(ExecuteTask, task, 1000, CalculateTime());
                    
                    outputPlugin.Execute(inputPlugin.Execute(), output.Value["options"].ToObject<string[]>());
                }
            }
                            
        }

        // Executes a task
        public static int CalculateTime()
        {
            return 1000;
        }


        // Executes a task
        public static void ExecuteTask(object state)
        {
            
        }


        // Selects the specified plugin and executes it   
        public static void ExecutePlugin(List<PluginDefinition> pluginList, String[] inputs, String[] outputs, String[] options)
        {
            foreach (String input in inputs)
            {
                PluginDefinition inputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == input.ToLower()).First();
                IInputPlugin inputPlugin = Activator.CreateInstance(inputPluginMetadata.ImplementationType) as IInputPlugin;
                string inputResult = inputPlugin.Execute();

                foreach (String output in outputs)
                {
                    PluginDefinition outputPluginMetadata = pluginList.Where(t => ((PluginAttribute)t.Attribute).PluginName.ToLower() == output.ToLower()).First();

                    IOutputPlugin outputPlugin = Activator.CreateInstance(outputPluginMetadata.ImplementationType) as IOutputPlugin;
            
                    outputPlugin.Execute(inputResult, options);
                }
            }

        }


        // Selects the classes with the specified attribute
        public static List<PluginDefinition> LoadAllClassesImplementingSpecificAttribute<T>(Assembly assembly)
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
        private static IEnumerable<Type> GetTypesWithSpecificAttribute<T>(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

    }
}
