using System;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

using Winagent.MessageHandling;
using System.Linq;

namespace Winagent
{
    class ServiceManager
    {
        public enum SetupOperation
        {
            Install,
            Uninstall
        }

        public enum ServiceOperation
        {
            Install,
            Uninstall,
            Start,
            Stop,
            Restart,
            Status
        }

        // Get Installer instance
        public static AssemblyInstaller GetInstaller(string[] args = null)
        {
            var assebmlyLocation = Assembly.GetEntryAssembly().Location;

            InstallContext installContext = new InstallContext();
            installContext.Parameters["logfile"] = "winagent.install.log";
            if(args != null)
            {
                installContext.Parameters["assemblypath"] = $"\"{assebmlyLocation}\" {string.Join(" ", args)}";
            }

            AssemblyInstaller installer = new AssemblyInstaller()
            {
                Context = installContext,
                Path = assebmlyLocation
            };

            return installer;
        }

        public static void Setup(SetupOperation operation, string[] args)
        {
            AssemblyInstaller installer = GetInstaller(args);
            try
            {
                IDictionary state = new Hashtable();
                switch (operation)
                {
                    case SetupOperation.Install:
                        try
                        {
                            installer.Install(state);
                            installer.Commit(state);
                        }
                        catch
                        {
                            installer.Rollback(state);
                            MessageHandler.HandleError("Error during the installation, it has been rolled back", 0);
                        }
                        break;

                    case SetupOperation.Uninstall:
                        installer.Uninstall(state);
                        break;
                }
            }
            catch (InstallException ie)
            {
                MessageHandler.HandleError("An error occurred while setting up the system", 0, ie);
            }
            catch (System.Security.SecurityException se)
            {
                MessageHandler.HandleError("Administrator permissions are required to set up the service", 0, se);
            }
            catch (Exception e)
            {
                MessageHandler.HandleError("An unknown error occurred while setting up the service", 0, e);
            }
            finally
            {
                if (installer != null)
                {
                    installer.Dispose();
                }
            }
        }

        public static void ExecuteOperation(ServiceOperation operation, string[] args = null)
        {
            ServiceController controller = new ServiceController("Winagent");
            try
            {
                switch (operation)
                {
                    case ServiceOperation.Install:
                        if (CheckServiceInstalled())
                        {
                            throw new Exceptions.ServiceAlreadyInstalledException("The service is already installed");
                        }
                        else
                        {
                            Setup(SetupOperation.Install, args);
                        }
                        break;

                    case ServiceOperation.Uninstall:
                        if (!CheckServiceInstalled())
                        {
                            throw new Exceptions.ServiceNotInstalledException("The service is not installed");
                        }
                        else
                        {
                            Setup(SetupOperation.Uninstall, args);
                        }
                        break;

                    case ServiceOperation.Start:
                        if (!CheckServiceStopped())
                        {
                            throw new Exceptions.ServiceAlreadyRunningException("The service is already running");
                        }
                        else
                        {
                            controller.Start();
                            controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(25));
                        }
                        break;

                    case ServiceOperation.Stop:
                        if (CheckServiceStopped())
                        {
                            throw new Exceptions.ServiceNotRunningException("The service is already stopped");
                        }
                        else
                        {
                            controller.Stop();
                            controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(25));
                        }
                        break;

                    case ServiceOperation.Restart:
                        if (CheckServiceStopped())
                        {
                            throw new Exceptions.ServiceNotRunningException("The service is not running");
                        }
                        else
                        {
                            controller.Stop();
                            controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(25));

                            controller.Start();
                            controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(25));
                        }
                        break;

                    case ServiceOperation.Status:
                        MessageHandler.HandleInformation(String.Format("{0} is {1}", controller.ServiceName, controller.Status), 0);
                        break;
                }

                bool CheckServiceStopped()
                {
                    if (!CheckServiceInstalled())
                    {
                        throw new Exceptions.ServiceNotInstalledException("The service is not installed");
                    }

                    return controller.Status == ServiceControllerStatus.Stopped;
                }

                bool CheckServiceInstalled()
                {
                    return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "Winagent") != null;
                }
            }
            catch (Exceptions.ServiceNotInstalledException snie)
            {
                MessageHandler.HandleError(snie.Message, 0);
            }
            catch (Exceptions.ServiceAlreadyInstalledException saie)
            {
                MessageHandler.HandleError(saie.Message, 0);
            }
            catch (Exceptions.ServiceNotRunningException snre)
            {
                MessageHandler.HandleError(snre.Message, 0);
            }
            catch (Exceptions.ServiceAlreadyRunningException sare)
            {
                MessageHandler.HandleError(sare.Message, 0);
            }
            catch (Exception e)
            {
                MessageHandler.HandleError("An error occurred while handling the service", 0, e);
            }
            finally
            {
                if (controller!=null)
                {
                    controller.Dispose();
                }
            }
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
            this.StartType = ServiceStartMode.Automatic;
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

}
