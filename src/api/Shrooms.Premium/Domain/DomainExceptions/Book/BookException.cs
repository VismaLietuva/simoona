using System;

namespace Shrooms.Premium.Domain.DomainExceptions.Book
{
    public class BookException : Exception
    {
        public BookException(string message)
            : base(message)
        {
        }
    }
}
