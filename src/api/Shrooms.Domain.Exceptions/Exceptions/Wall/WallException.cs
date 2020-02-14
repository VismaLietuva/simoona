using System;

namespace Shrooms.Domain.Exceptions.Exceptions.Wall
{
    public class WallException : Exception
    {
        public WallException(string message)
            : base(message)
        {
        }
    }
}
