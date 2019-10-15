using System;

namespace Winagent.Exceptions
{
    class WrongPathException : Exception
    {
        public WrongPathException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
