using Microsoft.Extensions.DependencyInjection;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.Email.Kudos;
using Shrooms.Premium.Domain.Services.WebHookCallbacks;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.IoC.Modules
{
    public static class WebHookCallbacksPremiumModule
    {
        public static IServiceCollection AddPremiumWebHookCallbacks(this IServiceCollection services)
        {
            services.AddScoped<IKudosPremiumNotificationService, KudosPremiumNotificationService>();
            services.AddScoped<ILoyaltyKudosService, LoyaltyKudosService>();
            services.AddScoped<ILoyaltyKudosCalculator, LoyaltyKudosCalculator>();
            services.AddScoped<IBookRemindService, BookRemindService>();
            services.AddScoped<ILotteryStatusChangeService, LotteryStatusChangeService>();
            services.AddScoped<IEventsWebHookService, EventsWebHookService>();
            services.AddScoped<IWebHookCallbackPremiumServices, WebHookCallbackPremiumServices>();
            return services;
        }
    }
}