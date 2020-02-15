using System;

namespace Shrooms.Domain.Exceptions.Exceptions.Kudos
{
    public class KudosException : Exception
    {
        public KudosException(string message)
            : base(message)
        {
        }
    }
}
