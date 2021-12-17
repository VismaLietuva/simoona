using System;

namespace Shrooms.Premium.Domain.DomainExceptions.Event
{
    public class EventException : Exception
    {
        public EventException(string message)
            : base(message)
        {
        }
    }
}
