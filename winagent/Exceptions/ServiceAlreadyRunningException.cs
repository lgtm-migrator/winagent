using System;

namespace Winagent.Exceptions
{
    class ServiceAlreadyRunningException : Exception
    {
        public ServiceAlreadyRunningException(string message) : base(message)
        {

        }
    }
}
