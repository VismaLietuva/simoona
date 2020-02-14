using System;

namespace Shrooms.Domain.Exceptions.Exceptions.Book
{
    public class BookException : Exception
    {
        public BookException(string message)
            : base(message)
        {
        }
    }
}
