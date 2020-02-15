using System.Web.Http.ExceptionHandling;
using NLog;

namespace Shrooms.Presentation.Api.GeneralCode
{
    public class NLogExceptionLogger : ExceptionLogger
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void Log(ExceptionLoggerContext context)
        {
            _logger.Log(LogLevel.Error, context.Exception);
        }
    }
}