using System;

namespace Shrooms.DomainExceptions.Exceptions.Organization
{
    public class InvalidOrganizationException : Exception
    {
        public InvalidOrganizationException()
            : base("Invalid organization")
        {
        }
    }
}