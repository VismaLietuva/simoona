using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using NLog;
using ILogger = Shrooms.Contracts.Infrastructure.ILogger;

namespace Shrooms.Infrastructure.Logger
{
    public class Logger : ILogger
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly NLog.Logger _logger;

        public Logger()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _telemetryClient = new TelemetryClient();
        }

        public void Debug(string log, Exception e = null)
        {
            var formattedLog = $"Debug: {log}";

            if (e == null)
            {
                _logger.Log(LogLevel.Debug, formattedLog);
                _telemetryClient.TrackTrace(formattedLog, SeverityLevel.Information);
            }
            else
            {
                var ex = new Exception(formattedLog, e);
                _logger.Log(LogLevel.Debug, ex);
                _telemetryClient.TrackException(ex);
            }
        }

        public void Error(Exception ex)
        {
            _logger.Log(LogLevel.Error, ex);
            _telemetryClient.TrackException(ex);
        }
    }
}
