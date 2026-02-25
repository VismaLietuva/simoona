using Microsoft.Extensions.DependencyInjection;
using Shrooms.Domain.Services.WebHookCallbacks;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Domain.Services.WebHookCallbacks.BlacklistUsers;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;

namespace Shrooms.IoC.Modules
{
    public static class WebHookCallbacksModule
    {
        public static IServiceCollection AddWebHookCallbacks(this IServiceCollection services)
        {
            services.AddScoped<IBirthdaysNotificationWebHookService, BirthdaysNotificationWebHookService>();
            services.AddScoped<IUsersAnonymizationWebHookService, UsersAnonymizationWebHookService>();
            services.AddScoped<IBlacklistUserStatusChangeWebHookService, BlacklistUserStatusChangeWebHookService>();
            services.AddScoped<IWebHookCallbackServices, WebHookCallbackServices>();
            return services;
        }
    }
}