using System.Web.Http.ExceptionHandling;
using NLog;

namespace Shrooms.API.GeneralCode
{
    public class NLogExceptionLogger : ExceptionLogger
    {
        private static readonly Logger NLog = LogManager.GetCurrentClassLogger();

        public override void Log(ExceptionLoggerContext context)
        {
            NLog.Log(LogLevel.Error, context.Exception);
        }
    }
}