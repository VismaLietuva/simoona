using System;

namespace Shrooms.Contracts.Infrastructure
{
    public interface ILogger
    {
        void Debug(string log, Exception e = null);

        void Error(Exception e);
    }
}