using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.BlacklistUsers;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;

namespace Shrooms.IoC.Modules
{
    public static class BlacklistUserModule
    {
        public static IServiceCollection AddBlacklistUsers(this IServiceCollection services)
        {
            services.AddScoped<IBlacklistService, BlacklistService>();
            services.AddScoped<IBlacklistValidator, BlacklistValidator>();
            return services;
        }
    }
}
