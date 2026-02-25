using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Support;

namespace Shrooms.IoC.Modules
{
    public static class SupportModule
    {
        public static IServiceCollection AddSupport(this IServiceCollection services)
        {
            services.AddScoped<ISupportService, SupportService>();
            return services;
        }
    }
}