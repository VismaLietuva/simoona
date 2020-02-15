using System;

namespace Shrooms.Domain.ServiceExceptions
{
    public class ServiceException : Exception
    {
        public ServiceException()
        {
        }

        public ServiceException(string message)
            : base(message)
        {
        }
    }
}
