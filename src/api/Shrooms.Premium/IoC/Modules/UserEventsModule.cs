using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Users;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;

namespace Shrooms.Premium.IoC.Modules
{
    public static class UserEventsModule
    {
        public static IServiceCollection AddPremiumUserEvents(this IServiceCollection services)
        {
            services.AddScoped<IEventRemindService, EventRemindService>();
            services.AddScoped<IUserEventsService, UserEventsService>();
            return services;
        }
    }
}