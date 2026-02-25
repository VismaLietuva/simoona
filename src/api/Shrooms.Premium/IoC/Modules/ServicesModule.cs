using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Notifications;
using Shrooms.Premium.Infrastructure.GoogleBookApiService;

namespace Shrooms.Premium.IoC.Modules
{
    public static class ServicesModule
    {
        public static IServiceCollection AddPremiumServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBookInfoService, GoogleBookService>();
            return services;
        }
    }
}