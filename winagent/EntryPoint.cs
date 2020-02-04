using System;
using CommandLine;
using System.ServiceProcess;

using Winagent.Options;
using System.Linq;

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
                ServiceManager.ExecuteOperation(
                    ServiceManager.ServiceOperation.Install,
                    new string[]
                    {
                        options.Config
                    }
                );
            }
            else if (options.Uninstall)
            {
                ServiceManager.ExecuteOperation(ServiceManager.ServiceOperation.Uninstall);
            }
            else if (options.Start)
            {
                ServiceManager.ExecuteOperation(ServiceManager.ServiceOperation.Start);
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
        }
    }
}
