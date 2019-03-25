using System;

namespace Shrooms.DomainExceptions.Exceptions.KudosBaskets
{
    public class KudosBasketException : Exception
    {
        public KudosBasketException(string message)
            : base(message)
        {
        }
    }
}
