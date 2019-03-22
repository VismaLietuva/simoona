using System;

namespace Shrooms.DomainExceptions.Exceptions.Event
{
    public class EventException : Exception
    {
        public EventException(string message)
            : base(message)
        {
        }
    }
}
