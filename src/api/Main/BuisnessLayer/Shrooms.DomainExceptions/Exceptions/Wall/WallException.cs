using System;

namespace Shrooms.DomainExceptions.Exceptions.Wall
{
    public class WallException : Exception
    {
        public WallException(string message)
            : base(message)
        {
        }
    }
}
