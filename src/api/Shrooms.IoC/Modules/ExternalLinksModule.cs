using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.ExternalLinks;

namespace Shrooms.IoC.Modules
{
    public static class ExternalLinksModule
    {
        public static IServiceCollection AddExternalLinks(this IServiceCollection services)
        {
            services.AddScoped<IExternalLinkService, ExternalLinkService>();
            return services;
        }
    }
}
