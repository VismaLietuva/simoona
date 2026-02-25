using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.KudosShop;

namespace Shrooms.Premium.IoC.Modules
{
    public static class KudosShopModule
    {
        public static IServiceCollection AddPremiumKudosShop(this IServiceCollection services)
        {
            services.AddScoped<IKudosShopService, KudosShopService>();
            return services;
        }
    }
}