﻿using System;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;
using System.ComponentModel;

namespace Winagent
{
    class ServiceManager
    {
        // Get Installer instance
        public static AssemblyInstaller GetInstaller()
        {
            AssemblyInstaller installer = new AssemblyInstaller(typeof(Service).Assembly, null)
            {
                UseNewContext = true
            };
            return installer;
        }

        // Install the service if it's not installed
        public static void Install()
        {
            try
            {
                using (AssemblyInstaller installer = GetInstaller())
                {
                    IDictionary state = new Hashtable();
                    try
                    {
                        installer.Install(state);
                        installer.Commit(state);
                    }
                    catch
                    {
                        try
                        {
                            installer.Rollback(state);
                        }
                        catch(Exception)
                        {
                            // Since AssemblyInstaller prints the error message, just exit
                            Console.Error.WriteLine();
                            Console.Write("Exiting...");
                            Console.Error.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);

                Console.Error.WriteLine(ex.Message);
            }
        }

        // Uninstall the service if it's installed
        public static void Uninstall()
        {
            try
            {
                using (AssemblyInstaller installer = GetInstaller())
                {
                    IDictionary state = new Hashtable();
                    try
                    {
                        installer.Uninstall(state);
                    }
                    catch (Exception)
                    {
                        // Since AssemblyInstaller prints the error message, just exit
                        Console.Error.WriteLine();
                        Console.Write("Exiting...");
                        Console.Error.WriteLine();
                    }
                }
            }
            
            
            catch (Exception ex)
            {
                Console.Write(ex);

                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void Start()
        {
            try
            {
                ServiceController controller = new ServiceController("Winagent");

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(25));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Service already started or not installed");
                Console.WriteLine("Exiting...");
            }
            catch (Exception ex)
            {
                Console.Write(ex);

                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void Stop()
        {
            try
            {
                ServiceController controller = new ServiceController("Winagent");

                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(25));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Service already stopped or not installed");
                Console.WriteLine("Exiting...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void Restart()
        {
            try
            {
                ServiceController controller = new ServiceController("Winagent");

                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(25));

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(25));
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Service not running or not installed");
                Console.WriteLine("Exiting...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void Status()
        {
            try
            {
                ServiceController controller = new ServiceController("Winagent");

                Console.WriteLine("{0} is {1}", controller.ServiceName,controller.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Console.Error.WriteLine(ex.Message);
            }
        }
    }

    [RunInstaller(true)]
    public sealed class WinagentInstallerProcess : ServiceProcessInstaller
    {
        public WinagentInstallerProcess()
        {
            this.Account = ServiceAccount.LocalSystem;
        }
    }

    [RunInstaller(true)]
    public sealed class WinagentInstaller : ServiceInstaller
    {
        public WinagentInstaller()
        {
            this.Description = "Windows Agent";
            this.DisplayName = "Winagent";
            this.ServiceName = "Winagent";
            this.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
        }
    }

}
