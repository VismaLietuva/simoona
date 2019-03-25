using System;

namespace Shrooms.DomainExceptions.Exceptions.Likes
{
    public class LikeException : Exception
    {
        public LikeException(string message)
            : base(message)
        {
        }
    }
}
