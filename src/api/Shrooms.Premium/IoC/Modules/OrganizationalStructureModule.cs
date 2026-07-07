using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.OrganizationalStructure;

namespace Shrooms.Premium.IoC.Modules
{
    public static class OrganizationalStructureModule
    {
        public static IServiceCollection AddPremiumOrganizationalStructure(this IServiceCollection services)
        {
            services.AddScoped<IOrganizationalStructureService, OrganizationalStructureService>();
            return services;
        }
    }
}