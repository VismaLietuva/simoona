using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Presentation.Api.BackgroundWorkers;

namespace Shrooms.Premium.IoC.Modules
{
    public static class BackgroundWorkersModule
    {
        public static IServiceCollection AddPremiumBackgroundWorkers(this IServiceCollection services)
        {
            services.AddTransient<NewEventNotifier>();
            services.AddTransient<SharedEventNotifier>();
            return services;
        }
    }
}