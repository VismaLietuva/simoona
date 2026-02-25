using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.RefreshTokens;

namespace Shrooms.IoC.Modules
{
    public static class RefreshTokenModule
    {
        public static IServiceCollection AddRefreshTokens(this IServiceCollection services)
        {
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            return services;
        }
    }
}
