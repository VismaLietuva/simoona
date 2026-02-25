using Hangfire.Annotations;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.GeneralCode
{
    public class UnwantedTelemetryFilter : ITelemetryProcessor
    {
        private readonly string _backgroundJobsDbName;

        private ITelemetryProcessor Next { get; set; }

        public UnwantedTelemetryFilter(ITelemetryProcessor next, IConfiguration configuration)
        {
            Next = next;
            _backgroundJobsDbName = ResolveBackgroundJobsDbName(configuration);
        }

        private static string ResolveBackgroundJobsDbName(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(DataLayerConstants.ConnectionStringNameBackgroundJobs);
            if (connectionString == null)
            {
                return null;
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

        public void Process(ITelemetry item)
        {
            if (item is RequestTelemetry request && (IsSignalr(request) || IsSuccessfulJobRequest(request) || IsImageCacheRequest(request)))
            {
                return;
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

        private bool IsHangfireBackgroundJobs(DependencyTelemetry dependency)
        {
            if (_backgroundJobsDbName == null || dependency.Type != "SQL" || !dependency.Success.GetValueOrDefault(false))
            {
                return false;
            }

            if (dependency.Name.Contains(_backgroundJobsDbName))
            {
                return true;
            }

            if (dependency.Target.Contains(_backgroundJobsDbName)
                && (dependency.Name.Equals("sp_getapplock") || dependency.Name.Equals("sp_releaseapplock")))
            {
                return true;
            }

            return false;
        }
    }
}