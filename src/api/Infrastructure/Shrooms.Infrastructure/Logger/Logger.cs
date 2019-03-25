using System;
using NLog;

namespace Shrooms.Infrastructure.Logger
{
    public class Logger : ILogger
    {
        public void Debug(string log, Exception e = null)
        {
            var formattedLog = $"Debug: {log}";
            var ex = new Exception(formattedLog, e);
            LogManager.GetCurrentClassLogger().Log(LogLevel.Debug, ex);
        }

        public void Error(Exception e)
        {
            LogManager.GetCurrentClassLogger().Log(LogLevel.Error, e);
        }
    }
}
