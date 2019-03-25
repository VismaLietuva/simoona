using System;
using System.Collections.Generic;

namespace Shrooms.DomainExceptions.Exceptions.UserAdministration
{
    public class UserAdministrationException : Exception
    {
        public UserAdministrationException(string message)
            : base(message)
        {
        }

        public UserAdministrationException(IEnumerable<string> errors)
            : base(errors.ToString())
        {
        }
    }
}
