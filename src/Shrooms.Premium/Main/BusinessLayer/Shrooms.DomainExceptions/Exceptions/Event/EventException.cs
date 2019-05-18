using System;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DomainExceptions.Exceptions.Event
{
    public class EventException : Exception
    {
        public EventException(string message)
            : base(message)
        {
        }
    }
}
