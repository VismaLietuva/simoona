using System;

namespace Shrooms.Domain.Exceptions.Exceptions.KudosBaskets
{
    public class KudosBasketException : Exception
    {
        public KudosBasketException(string message)
            : base(message)
        {
        }
    }
}
