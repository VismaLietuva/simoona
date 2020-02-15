using System;

namespace Shrooms.Domain.Exceptions.Exceptions.Likes
{
    public class LikeException : Exception
    {
        public LikeException(string message)
            : base(message)
        {
        }
    }
}
