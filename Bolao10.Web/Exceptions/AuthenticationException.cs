using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bolao10.Exceptions
{
    public class AuthenticationException : ApplicationException
    {
        public AuthenticationException(string message)
            : base(message)
        {
        }
    }
}