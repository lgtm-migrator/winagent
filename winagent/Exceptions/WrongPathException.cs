using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winagent.Exceptions
{
    class WrongPathException : Exception
    {
        public WrongPathException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
