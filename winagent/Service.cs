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
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // Get application settings
                Settings.Agent settings = Agent.GetSettings();

                // Create tasks
                Agent.CreateTasks(settings.InputPlugins);

                // Create detached autoupdater if autoupdates are enabled
                if (settings.UpdateSettings.Enabled)
                {
                    // Run the updater after 1 minute
                    // The timer will run every 10 mins
                    Timer updaterTimer = new Timer(new TimerCallback(RunUpdater), null, 60000, settings.UpdateSettings.Schedule.GetTime());
                    // Save reference to avoid GC
                    Agent.timersReference.Add(updaterTimer);
                }
            }
            catch (Exception e)
            {
                // EventID 1 => An error ocurred
                ExceptionHandler.HandleError(String.Format("General error during service execution"), 1, e);
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
                    ExceptionHandler.HandleInformation(String.Format("Application updated: \"{0}\"", "winagent-updater.exe"), 3, null);
                }
                Process.Start(@"winagent-updater.exe");
            }
            catch (Exception e)
            {
                // EventID 2 => Error executing updater
                ExceptionHandler.HandleError(String.Format("An error ocurred executing updater"), 2, e);
            }
        }

    }
}
