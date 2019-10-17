using System;

namespace Winagent.Exceptions
{
    class ServiceNotRunningException : Exception
    {
        public ServiceNotRunningException(string message) : base(message)
        {

        }
    }
}
