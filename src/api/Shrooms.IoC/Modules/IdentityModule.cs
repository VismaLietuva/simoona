using Microsoft.Extensions.DependencyInjection;
using Shrooms.Authentification.BasicAuth;
using Shrooms.Authentification.Membership;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.Email;

namespace Shrooms.IoC.Modules
{
    public static class IdentityModule
    {
        public static IServiceCollection AddIdentityRegistrations(this IServiceCollection services)
        {
            services.AddScoped<ShroomsRoleStore>();
            services.AddScoped<ShroomsRoleManager>();
            services.AddScoped<ShroomsUserStore>();
            services.AddScoped<ShroomsUserManager>();
            services.AddScoped<ShroomsClaimsIdentityFactory>();
            services.AddScoped<IBasicAuthValidator, BasicAuthValidator>();
            return services;
        }
    }
}