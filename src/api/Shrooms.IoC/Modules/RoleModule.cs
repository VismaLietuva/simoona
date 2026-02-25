using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Roles;

namespace Shrooms.IoC.Modules
{
    public static class RoleModule
    {
        public static IServiceCollection AddRoles(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();
            return services;
        }
    }
}
