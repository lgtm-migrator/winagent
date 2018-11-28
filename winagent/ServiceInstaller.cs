using System;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace winagent
{
    class ServiceManager
    {
        // Uninstall the service if it exists and install it again
        public static void Install(string[] args)
        {
            try
            {
                using (AssemblyInstaller installer = new AssemblyInstaller(typeof(Agent).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    installer.UseNewContext = true;
                    try
                    {
                        // Check if service exits
                        if (ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals("Winagent")))
                        {
                            installer.Uninstall(state);
                        }

                        installer.Install(state);
                        installer.Commit(state);
                    }
                    catch
                    {
                        try
                        {
                            installer.Rollback(state);
                        }
                        catch(Exception e)
                        {
                            throw e;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }

    [RunInstaller(true)]
    public sealed class WinagentInstallerProcess : ServiceProcessInstaller
    {
        public WinagentInstallerProcess()
        {
            this.Account = ServiceAccount.NetworkService;
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
            this.StartType = System.ServiceProcess.ServiceStartMode.Manual;
        }
    }

}
