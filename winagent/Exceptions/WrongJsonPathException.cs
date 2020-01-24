using System;

namespace Winagent.Exceptions
{
    class WrongJsonPathException : Exception
    {
        public WrongJsonPathException(string message) : base(message)
        {

        }
    }
}
