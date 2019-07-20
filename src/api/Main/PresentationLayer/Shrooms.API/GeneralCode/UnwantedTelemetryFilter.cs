using System.Configuration;
using Hangfire.Annotations;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Shrooms.Constants.DataLayer;

namespace Shrooms.API.GeneralCode
{
    public class UnwantedTelemetryFilter : ITelemetryProcessor
    {
        private static bool _backgroundJobsDbNameSet;
        private static string _backgroundJobsDbName;

        private static string BackgroundJobsDbName
        {
            get
            {
                if (_backgroundJobsDbNameSet)
                {
                    return _backgroundJobsDbName;
                }

                _backgroundJobsDbNameSet = true;

                if (_backgroundJobsDbName != null)
                {
                    return _backgroundJobsDbName;
                }

                if (ConfigurationManager.ConnectionStrings.Count > 0)
                {
                    var connectionString = ConfigurationManager.ConnectionStrings[ConstDataLayer.ConnectionStringNameBackgroundJobs].ConnectionString;

                    if (connectionString != null)
                    {
                        var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                        _backgroundJobsDbName = builder.InitialCatalog;
                    }
                }

                return _backgroundJobsDbName;
            }
        }

        private ITelemetryProcessor Next { get; set; }

        public UnwantedTelemetryFilter(ITelemetryProcessor next)
        {
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry request)
            {
                if (IsSignalr(request))
                {
                    return;
                }
            }

            if (item is DependencyTelemetry dependency)
            {
                if (IsHangfireBackgroundJobs(dependency))
                {
                    return;
                }
            }

            // Send everything else:
            Next.Process(item);
        }

        private static bool IsSignalr([NotNull]RequestTelemetry request)
        {
            if (request.Name?.Contains("signalr") == true)
            {
                return true;
            }

            return false;
        }

        private static bool IsHangfireBackgroundJobs(DependencyTelemetry dependency)
        {
            if (BackgroundJobsDbName != null && dependency.Type == "SQL" && dependency.Name?.Contains(BackgroundJobsDbName) == true && dependency.Success == false)
            {
                return true;
            }

            return false;
        }
    }
}