using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Badges;

namespace Shrooms.Premium.IoC.Modules
{
    public static class BadgesModule
    {
        public static IServiceCollection AddPremiumBadges(this IServiceCollection services)
        {
            services.AddScoped<IBadgesService, BadgesService>();
            return services;
        }
    }
}