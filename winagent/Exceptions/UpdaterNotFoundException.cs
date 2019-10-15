using System;

namespace Winagent.Exceptions
{
    class UpdaterNotFoundException : Exception
    {
        public UpdaterNotFoundException(string message) : base(message)
        {

        }
    }
}
