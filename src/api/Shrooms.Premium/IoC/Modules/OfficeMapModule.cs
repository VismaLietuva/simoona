using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.OfficeMap;

namespace Shrooms.Premium.IoC.Modules
{
    public static class OfficeMapModule
    {
        public static IServiceCollection AddPremiumOfficeMap(this IServiceCollection services)
        {
            services.AddScoped<IOfficeMapService, OfficeMapService>();
            return services;
        }
    }
}