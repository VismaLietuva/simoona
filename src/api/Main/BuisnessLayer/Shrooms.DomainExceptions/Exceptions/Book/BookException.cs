using System;

namespace Shrooms.DomainExceptions.Exceptions.Book
{
    public class BookException : Exception
    {
        public BookException(string message)
            : base(message)
        {
        }
    }
}
