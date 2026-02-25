using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.Notifications;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.VacationPages;

namespace Shrooms.IoC.Modules
{
    public static class ServicesModule
    {
        public static IServiceCollection AddShroomsServices(this IServiceCollection services)
        {
            services.AddScoped<IPictureService, PictureService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IVacationPageService, VacationPageService>();
            return services;
        }
    }
}