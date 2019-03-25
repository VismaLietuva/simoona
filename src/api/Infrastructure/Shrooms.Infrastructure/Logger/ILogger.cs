using System;

namespace Shrooms.Infrastructure.Logger
{
    public interface ILogger
    {
        void Debug(string log, Exception e = null);
        void Error(Exception e);
    }
}
