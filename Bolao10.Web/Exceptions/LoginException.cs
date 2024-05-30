using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bolao10.Exceptions
{
    public class LoginException : ApplicationException
    {
        public LoginException(string message)
            : base(message)
        {
        }
    }
}