using System;

namespace Shrooms.DomainExceptions.Exceptions.Kudos
{
    public class KudosException : Exception
    {
        public KudosException(string message)
            : base(message)
        {
        }
    }
}
