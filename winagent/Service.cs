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
using Winagent.MessageHandling;

namespace Winagent
{
    class Service : ServiceBase
    {
        private const string AgentName = "Winagent";
        private const string Updater = @"winagent-updater.exe";

        public Service()
        {
            ServiceName = AgentName;

            // Set current directory as base directory
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // Get application settings
                Settings.Agent settings = Agent.GetSettings();

                // Create envent handlers
                Agent.SetEventReaders(settings.EventLogs);

                // Create tasks
                Agent.CreateTasks(settings.InputPlugins);

                // Create detached autoupdater if autoupdates are enabled
                if (settings.AutoUpdates.Enabled)
                {
                    // Run the updater after 1 minute
                    // The timer will run every 10 mins
                    Timer updaterTimer = new Timer(new TimerCallback(RunUpdater), null, 60000, settings.AutoUpdates.Schedule.GetTime());
                    // Save reference to avoid GC
                    Agent.timersReference.Add(updaterTimer);
                }
            }
            catch (Exception e)
            {
                // EventID 1 => An error ocurred
                MessageHandler.HandleError(String.Format("General error during service execution."), 1, e);
            }
        }

        /// <summary>
        /// Execute updater
        /// </summary>
        /// <param name="state">State object passed to the timer</param>
        /// <exception cref="UpdaterNotFoundException">Thrown when the executable of the updater does not exist</exception>
        /// <exception cref="Exception">General exception when the updater is executed</exception>
        internal static void RunUpdater(object state)
        {
            try
            {
                var tmpLocation = @".\tmp\" + Updater;

                // If there is a new version of the updater in .\tmp\ copy it
                if (File.Exists(tmpLocation))
                {
                    File.Copy(tmpLocation, Updater, true);
                    File.Delete(tmpLocation);

                    // EventID 3 => Application updated
                    MessageHandler.HandleInformation(String.Format("Application updated: \"{0}\".", Updater), 3);
                }

                if (File.Exists(Updater))
                {
                    Process.Start(Updater);
                }
                else
                {
                    throw new Exceptions.UpdaterNotFoundException("Could not find the updater.");
                }
            }
            catch (Exceptions.UpdaterNotFoundException unfe)
            {
                // EventID 12 => Could not find the updater executable
                MessageHandler.HandleError("An error ocurred while executing the updater.", 12, unfe);
            }
            catch (Exception e)
            {
                // EventID 2 => Error executing updater
                MessageHandler.HandleError("An error ocurred while executing the updater.", 2, e);
            }
        }

    }
}
