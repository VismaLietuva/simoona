using System;

namespace Shrooms.Domain.Exceptions.Exceptions.Organization
{
    public class InvalidOrganizationException : Exception
    {
        public InvalidOrganizationException()
            : base("Invalid organization")
        {
        }
    }
}