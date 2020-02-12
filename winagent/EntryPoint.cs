using System;
using CommandLine;
using System.ServiceProcess;

using Winagent.Options;
using System.Linq;
using System.Collections.Generic;

namespace Winagent
{
    class EntryPoint
    {
        // Entrypoint
        static void Main(string[] args)
        {

            if (Environment.UserInteractive)
            {
                // Parse CommandLine options
                // https://github.com/commandlineparser/commandline
                var options = Parser.Default.ParseArguments<CommandOptions, ManagementOptions>(args);

                // Call the right method
                options.WithParsed<CommandOptions>(opts => Command(opts));
                options.WithParsed<ManagementOptions>(opts => Service(opts));
            }
            else
            {
                using (var service = new Service(args))
                {
                    ServiceBase.Run(service);
                }
            }

        }

        // Command execution with parsed options
        static void Command(CommandOptions options)
        {
            // TODO: Specify config file
            if (options.ConfigFile != null)
            {
                // Execute with config
                CLI.ExecuteConfig(options.ConfigFile);
                Console.Error.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            else
            {
                CLI.ExecuteCommand(options.Input, options.Output, (string[])options.InputOptions, (string[])options.OutputOptions);
            }
        }
        
        // Service management with parsed options
        static void Service(ManagementOptions options)
        {
            if (options.Install)
            {
                var args = new List<string>
                {
                    options.Config
                };

                ServiceManager.ExecuteOperation(
                    ServiceManager.ServiceOperation.Install,
                    optionsToArray(args)
                );
            }
            else if (options.Uninstall)
            {
                ServiceManager.ExecuteOperation(ServiceManager.ServiceOperation.Uninstall);
            }
            else if (options.Start)
            {
                var args = new List<string>
                {
                    options.Config
                };

                ServiceManager.ExecuteOperation(
                    ServiceManager.ServiceOperation.Start,
                    optionsToArray(args)
                );
            }
            else if (options.Stop)
            {
                ServiceManager.ExecuteOperation(ServiceManager.ServiceOperation.Stop);
            }
            else if (options.Restart)
            {
                ServiceManager.ExecuteOperation(ServiceManager.ServiceOperation.Restart);
            }
            else if (options.Status)
            {
                ServiceManager.ExecuteOperation(ServiceManager.ServiceOperation.Status);
            }


            /// <summary>
            /// Splits all the option strings into separated arguments
            /// as the service requires each argument as a separated element
            /// string[1]{--config example.json} => string[2]{--config, example.json}
            /// </summary>
            /// <param name="args">List of options as strings</param>
            /// <returns>Array of strings with separated arguments</returns>
            string[] optionsToArray(List<string> args)
            {
                return args
                    // Get non null elements
                    .Where(s => !string.IsNullOrEmpty(s))
                    // Split each string into string[]
                    .Select(s => s.Split(' '))
                    // Flatten the list of arrays
                    .SelectMany(l => l)
                    // Conver the list into a string array
                    .ToArray();
            }
        }
    }
}
