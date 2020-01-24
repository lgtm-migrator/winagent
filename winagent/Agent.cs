using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using plugin;
using Winagent.Settings;
using Winagent.MessageHandling;
using Winagent.Models;
using Winagent.Exceptions;
using System.ComponentModel;
using System.Text;

namespace Winagent
{
    class Agent
    {
        private const string ConfigFile = @"config.json";


        /// <summary>
        /// List of timers to keep a reference of each task
        /// Avoid the timers to be garbage collected
        /// </summary>
        internal static List<Timer> timersReference;

        /// <summary>
        /// Static constructor to initialize static data when any static member is referenced
        /// </summary>
        static Agent()
        {
            timersReference = new List<Timer>();
        }

        /// <summary>
        /// Parse settings
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when the config file could not be found</exception>
        /// <exception cref="Newtonsoft.Json.JsonSerializationException">Thrown when the content of the config file is incorrect</exception>
        /// <exception cref="Exception">Thrown when a different error occurs</exception>
        internal static Settings.Agent GetSettings(string path = ConfigFile)
        {
            try
            {
                // Content of the configuration file
                var settings = JObject.Parse(File.ReadAllText(path));

                var references = settings.SelectTokens("$..[?(@ =~ /\\$\\..*/)]").ToList();

                foreach (JToken referencePath in references)
                {
                    // Get values of the referenced variable
                    JToken variableValue = settings.SelectToken(referencePath.ToString());

                    if (variableValue is null)
                    {
                        throw new WrongJsonPathException(String.Format("Unable to resolve the specified JsonPath: {0}.", referencePath.Parent));
                    }

                    // Merge variable values with the specified settings
                    if (variableValue.GetType() == typeof(JValue))
                    {
                        referencePath.Replace(variableValue);
                    }
                    else
                    {
                        // Merge the variable value with the already specified
                        MergeSettings(referencePath.Parent.Parent, variableValue.Children());

                        // Remove the variable reference
                        referencePath.Parent.Remove();
                    }
                }

                // Parsed settings
                return settings.ToObject<Settings.Agent>();
            }
            catch (WrongJsonPathException wjpe)
            {
                MessageHandler.HandleError("The agent could not parse the config file, please check the syntax.", 7, wjpe);

                throw;
            }
            catch (FileNotFoundException fnfe)
            {
                // EventID 6 => Config file not found
                MessageHandler.HandleError(String.Format("The specified path \"{0}\" does not appear to be valid.", path), 6, fnfe);

                throw;
            }
            catch (Newtonsoft.Json.JsonSerializationException jse)
            {
                // EventID 7 => Error in config file
                MessageHandler.HandleError("The agent could not parse the config file, please check the syntax.", 7, jse);

                throw;
            }
            catch (Exception e)
            {
                // EventID 8 => Error while parsing the config file
                MessageHandler.HandleError("An undefined error occurred while parsing the config file.", 8, e);

                throw;
            }


            /// <summary>
            /// Merge the settings inside the variable with the arleady existing ones
            /// The existing ones will be prioritized
            /// </summary>
            /// <param name="settings">Object that contain settings and variable reference(s)</param>
            /// <param name="variable">Content of the referenced variable that will be merged</param>
            void MergeSettings(JContainer settings, JEnumerable<JToken> variable)
            {
                // Iterate through all the values contained in the variable
                foreach (JProperty attribute in variable)
                {
                    // Check if some of the attributes (keys) already exist
                    if (settings[attribute.Name] == null)
                    {
                        // If the attribute does not exists, set the one contained in the referenced variable
                        settings.Add(attribute);
                    }
                }
            }
        }


        /// <summary>
        /// Load plugin and get an instance of it by its name
        /// </summary>
        /// <param name="name">Name of the plugin</param>
        /// <returns>Instance of the plugin</returns>
        internal static object GetPluginInstance(string name)
        {
            try
            {
                // Load plugin assembly
                Assembly assembly = Assembly.LoadFrom(String.Format("plugins/{0}.dll", name));

                // Load the class that is implementing a custom attribute in the assembly
                // "I<plugin>" / "O<plugin>"
                // TODO: Null pointer exception if there is no attribute
                Type typeImplementingAttribute = (from type in assembly.GetTypes()
                                                  where type.IsDefined(typeof(PluginAttribute), false)
                                                  select type).FirstOrDefault();
                // Return 
                return Activator.CreateInstance(typeImplementingAttribute);
            }
            catch (BadImageFormatException)
            {
                // EventID 15 => Error while parsing the config file
                MessageHandler.HandleError("The plugin could not be loaded.", 15);

                throw;
            }
            catch (Exception)
            {
                // Fail on any not-controlled error
                throw;
            }
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
                    var inputPlugin = new Winagent.Models.InputPlugin()
                    {
                        Name = input.Name,
                        Settings = input.Settings,
                        Instance = (IInputPlugin)GetPluginInstance(input.Name)
                    };

                    foreach (Settings.OutputPlugin output in input.OutputPlugins)
                    {
                        var outputPlugin = new Winagent.Models.OutputPlugin()
                        {
                            Name = output.Name,
                            Settings = output.Settings,
                            Instance = (IOutputPlugin)GetPluginInstance(output.Name)
                        };

                        // Create task with the input and output plugins to be run by the Timer
                        Task task = new Task(inputPlugin, outputPlugin);

                        // Set event handler for the messages received from this plugins
                        inputPlugin.Instance.MessageEvent += new EventHandler<MessageEventArgs>((sender, eventArgs) => OnMessageEvent(sender, eventArgs, task));
                        outputPlugin.Instance.MessageEvent += new EventHandler<MessageEventArgs>((sender, eventArgs) => OnMessageEvent(sender, eventArgs, task));

                        // Create Timer to schedule the task
                        Timer timer = new Timer(new TimerCallback(ExecuteTask), task, 0, output.Schedule.GetTime());

                        // Put a reference to the timer in the task, so it can be managed from inside the callback
                        task.Timer = timer;

                        // Save reference to avoid GC
                        timersReference.Add(timer);
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    // EventID 4 => There are no plugins to execute
                    MessageHandler.HandleError(String.Format("The specified plugin does not exist in the \"plugins\" directory."), 4, ioe);

                    throw;
                }
                
            }
        }

        private static void OnMessageEvent(object sender, MessageEventArgs eventArgs, Task task)
        {
            StringBuilder logMessage = new StringBuilder(String.Format("Thread: [{0} → {1}].", task.InputPlugin.Name, task.OutputPlugin.Name));
            logMessage.Append(Environment.NewLine);
            logMessage.Append(String.Format("Source: {0}.", sender.ToString()));
            logMessage.Append(Environment.NewLine);
            logMessage.Append(String.Format("Message: {0}.", eventArgs.Message));

            MessageHandler.HandleMessage(logMessage.ToString(), 0, eventArgs.Type, eventArgs.Exception);
        }


        /// <summary>
        /// Executes a task if it is not already being executed
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="WarningException">Internal plugin exception</exception>
        /// <exception cref="Exception">General error during plugin execution</exception>
        internal static void ExecuteTask(object state)
        {
            var hasLock = false;

            try
            {
                // Try to get the lock
                Monitor.TryEnter(((Task)state).Locker, ref hasLock);

                // If the task lock could not be obtained, then the previous execution has not finished yet
                if (!hasLock)
                {
                    // EventID 13 => Plugin execution overlapping
                    MessageHandler.HandleWarning(String.Format("The execution of [{0} → {1}] was skipped because a previous task is still running.", ((Task)state).InputPlugin.Name, ((Task)state).OutputPlugin.Name), 13);
                    return;
                }

                // Run the task
                ((Task)state).Execute();
            }
            catch (WarningException we)
            {
                // EventID 14 => Internal plugin exception, the Task should be stopped
                MessageHandler.HandleWarning(String.Format("The following task will no longer be executed: [{0} → {1}].", ((Task)state).InputPlugin.Name, ((Task)state).OutputPlugin.Name), 14, we);
                ((Task)state).Timer.Dispose();
            }
            catch (Exception e)
            {
                // EventID 5 => Error executing plugin
                MessageHandler.HandleError(String.Format("An error ocurred while executing a plugin: [{0} → {1}].", ((Task)state).InputPlugin.Name, ((Task)state).OutputPlugin.Name), 5, e);
                throw;
            }
            finally
            {
                if (hasLock)
                {
                    // Release task lock
                    Monitor.Exit(((Task)state).Locker);
                }
            }
        }

        /// <summary>
        /// Sets the event readers for the EventLogs
        /// </summary>
        /// <param name="eventLogs">Settings defined for the event logs</param>
        internal static void SetEventReaders(List<Settings.EventLog> eventLogs)
        {
            foreach(Settings.EventLog eventLog in eventLogs)
            {
                System.Diagnostics.EventLog EventLogInstance = new System.Diagnostics.EventLog(eventLog.Name);
                EventLogInstance.EntryWritten += new EntryWrittenEventHandler((sender, e) => OnEntryWritten(sender, e, eventLog));
                EventLogInstance.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Sends the information through the output plugin when an entry is created
        /// </summary>
        /// <param name="sender">The object sendig the event</param>
        /// <param name="e">Event arguments</param>
        /// <param name="settings">Settings for the output plugin</param>
        private static void OnEntryWritten(object sender, EntryWrittenEventArgs e, Settings.EventLog settings)
        {
            var eventdetail = e.Entry;

            try
            {
                var log = new Models.Log()
                {
                    // Using datetime instead of eventdetail.TimeGenerated.ToUniversalTime() since it seems to be getting wrong dates
                    Date = DateTime.Now.ToUniversalTime(),
                    Description = eventdetail.Message,
                    Id = eventdetail.InstanceId,
                    Severity = eventdetail.EntryType.ToString(),
                    Source = eventdetail.Source,
                    HostName = eventdetail.MachineName,
                    EventLogName = settings.Name
                };

                foreach (Settings.OutputPlugin output in settings.OutputPlugins)
                {
                    // Send it through each plugin
                    try
                    {
                        // TODO: Do not load the assembly in each execution
                        var outputInstance = (IOutputPlugin)GetPluginInstance(output.Name);

                        // TODO: Specify exception for the output plugin
                        outputInstance.Execute(JsonConvert.SerializeObject(log), output.Settings);
                    }
                    catch (Exception ex)
                    {
                        CreateMessage(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                CreateMessage(ex);
            }

            // Create an error message if it comes from a different event
            void CreateMessage(Exception ex)
            {
                // Break the loop of event reports
                if (!eventdetail.Source.Equals("Winagent") || eventdetail.InstanceId != 11)
                {
                    // EventID 11 => An error occurred while hadling an event
                    MessageHandler.HandleError("An error occurred while hadling an Event.", 11, ex);
                }
            }
        }
    }
}
