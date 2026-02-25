using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Banners;
using Shrooms.Domain.Services.Events;

namespace Shrooms.IoC.Modules
{
    public static class WidgetModule
    {
        public static IServiceCollection AddWidgets(this IServiceCollection services)
        {
            services.AddScoped<IEventWidgetService, EventWidgetService>();
            services.AddScoped<IBannerWidgetService, BannerWidgetService>();
            return services;
        }
    }
}
