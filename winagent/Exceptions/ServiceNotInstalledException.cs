using System;

namespace Winagent.Exceptions
{
    class ServiceNotInstalledException : Exception
    {
        public ServiceNotInstalledException(string message) : base(message)
        {

        }
    }
}
