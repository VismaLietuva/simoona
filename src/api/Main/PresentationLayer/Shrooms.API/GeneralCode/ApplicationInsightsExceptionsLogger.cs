using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Microsoft.ApplicationInsights;

namespace Shrooms.API.GeneralCode
{
    public class ApplicationInsightsExceptionsLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            WriteError(context);
            base.Log(context);
        }

        public override async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            WriteError(context);
            await base.LogAsync(context, cancellationToken);
        }

        private void WriteError(ExceptionLoggerContext context)
        {
            if (context?.Exception == null)
            {
                return;
            }

            var ai = new TelemetryClient();
            ai.TrackException(context.Exception);
        }
    }
}