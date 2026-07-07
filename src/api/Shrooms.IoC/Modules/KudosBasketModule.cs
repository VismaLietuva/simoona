using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.KudosBaskets;
using Shrooms.Domain.ServiceValidators.Validators.KudosBaskets;

namespace Shrooms.IoC.Modules
{
    public static class KudosBasketModule
    {
        public static IServiceCollection AddKudosBasket(this IServiceCollection services)
        {
            services.AddScoped<IKudosBasketValidator, KudosBasketValidator>();
            services.AddScoped<IKudosBasketService, KudosBasketService>();
            return services;
        }
    }
}
