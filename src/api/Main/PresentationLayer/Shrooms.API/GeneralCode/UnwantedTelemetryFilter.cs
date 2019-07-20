using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Shrooms.API.GeneralCode
{
    public class UnwantedTelemetryFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        public UnwantedTelemetryFilter(ITelemetryProcessor next)
        {
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            var request = item as RequestTelemetry;

            if (request?.Name != null)
            {
                if (request.Name.Contains("signalr"))
                {
                    return;
                }
            }

            // Send everything else:
            Next.Process(item);
        }
    }
}