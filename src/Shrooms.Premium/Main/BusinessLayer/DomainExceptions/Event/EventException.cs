using System;

namespace Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Event
{
    public class EventException : Exception
    {
        public EventException(string message)
            : base(message)
        {
        }
    }
}
