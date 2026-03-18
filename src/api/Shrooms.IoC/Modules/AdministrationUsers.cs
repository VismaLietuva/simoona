using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Email.AdministrationUsers;

namespace Shrooms.IoC.Modules
{
    public static class AdministrationUsers
    {
        public static IServiceCollection AddAdministrationUsers(this IServiceCollection services)
        {
            services.AddHttpClient<IAdministrationUsersService, AdministrationUsersService>();
            services.AddScoped<IAdministrationNotificationService, AdministrationUsersNotificationService>();
            return services;
        }
    }
}
