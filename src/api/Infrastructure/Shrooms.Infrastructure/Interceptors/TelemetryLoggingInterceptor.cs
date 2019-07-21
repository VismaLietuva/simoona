using System;
using System.Linq;
using Castle.DynamicProxy;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Shrooms.Infrastructure.Interceptors
{
    public class TelemetryLoggingInterceptor : IInterceptor
    {
        private readonly TelemetryClient _telemetryClient;

        public TelemetryLoggingInterceptor()
        {
            _telemetryClient = new TelemetryClient();
        }

        public void Intercept(IInvocation invocation)
        {
            string operationName;

            if (invocation.Method.DeclaringType != null)
            {
                if (invocation.Method.DeclaringType == typeof(IDisposable))
                {
                    return;
                }

                operationName = $"{invocation.Method.DeclaringType.Name} {invocation.Method.Name}";
            }
            else
            {
                operationName = $"{invocation.Method.Name}";
            }

            using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>(operationName))
            {
                try
                {
                    var args = string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray());

                    operation.Telemetry.Type = "METHOD";
                    operation.Telemetry.Data = $"Args: {args}";

                    invocation.Proceed();

                    operation.Telemetry.Success = true;
                }
                catch (Exception ex)
                {
                    operation.Telemetry.Success = false;
                    _telemetryClient.TrackException(ex);
                }
            }
        }
    }
}