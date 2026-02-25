using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Monitors;

namespace Shrooms.IoC.Modules
{
    public static class MonitorsModule
    {
        public static IServiceCollection AddMonitors(this IServiceCollection services)
        {
            services.AddScoped<IMonitorService, MonitorService>();
            return services;
        }
    }
}
