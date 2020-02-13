using System.Configuration;
using Hangfire.Annotations;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Shrooms.Host.Contracts.Constants;

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
                    var connectionString = ConfigurationManager.ConnectionStrings[DataLayerConstants.ConnectionStringNameBackgroundJobs].ConnectionString;

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
                if (IsSignalr(request) || IsSuccessfulJobRequest(request) || IsImageCacheRequest(request))
                {
                    return;
                }
            }

            if (item is DependencyTelemetry dependency)
            {
                if (IsHangfireBackgroundJobs(dependency) || IsSuccessfulJobDependency(dependency) || IsImageCacheDependency(dependency))
                {
                    return;
                }
            }

            // Send everything else:
            Next.Process(item);
        }

        private static bool IsSuccessfulJobRequest([NotNull] RequestTelemetry request)
        {
            // Ignore successful job calls to reduce sampling
            if (request.Success == true && request.Name != null && (request.Name.Contains("externalpremiumjobs") || request.Name.Contains("externaljobs")))
            {
                return true;
            }

            return false;
        }

        private static bool IsSuccessfulJobDependency([NotNull] DependencyTelemetry dependency)
        {
            // Ignore successful job calls to reduce sampling
            if (dependency.Success == true && dependency.Context?.Operation?.Name != null && (dependency.Context.Operation.Name.Contains("externalpremiumjobs") || dependency.Context.Operation.Name.Contains("externaljobs")))
            {
                return true;
            }

            return false;
        }

        private static bool IsImageCacheRequest([NotNull] RequestTelemetry request)
        {
            if (request.Name.Contains("imagecache") && request.Success == true)
            {
                return true;
            }

            return false;
        }

        private static bool IsImageCacheDependency([NotNull] DependencyTelemetry dependency)
        {
            if (dependency.Type == "Azure blob" && dependency.Success == true)
            {
                return true;
            }

            return false;
        }

        private static bool IsSignalr([NotNull] RequestTelemetry request)
        {
            if (request.Name.Contains("signalr"))
            {
                return true;
            }

            return false;
        }

        private static bool IsHangfireBackgroundJobs(DependencyTelemetry dependency)
        {
            if (BackgroundJobsDbName != null && dependency.Type == "SQL" && dependency.Success.GetValueOrDefault(false))
            {
                if (dependency.Name.Contains(BackgroundJobsDbName))
                {
                    return true;
                }

                if (dependency.Target.Contains(BackgroundJobsDbName)
                    && (dependency.Name.Equals("sp_getapplock") || dependency.Name.Equals("sp_releaseapplock")))
                {
                    return true;
                }
            }

            return false;
        }
    }
}