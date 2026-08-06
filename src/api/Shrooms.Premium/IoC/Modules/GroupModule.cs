using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Groups;

namespace Shrooms.Premium.IoC.Modules
{
    public static class GroupModule
    {
        public static IServiceCollection AddPremiumGroup(this IServiceCollection services)
        {
            services.AddScoped<IGroupsService, GroupsService>();
            services.AddScoped<IGroupTypesService, GroupTypesService>();
            services.AddScoped<IGroupKudosService, GroupKudosService>();
            return services;
        }
    }
}
