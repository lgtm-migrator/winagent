using System;

namespace Winagent.Exceptions
{
    class ServiceAlreadyInstalledException : Exception
    {
        public ServiceAlreadyInstalledException(string message) : base(message)
        {

        }
    }
}
